using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using BepInEx.Logging;

[HarmonyPatch(typeof(Toolbox), "LoadAll")]
[HarmonyPriority(Priority.Last)]
public static class DedupePatch
{
    public static bool Enabled = true;
        public static ManualLogSource Log;
    static void Postfix()
    {
        if (!Enabled) return;

        int removedTotal = 0;
        int menusTouched = 0;

        var menus = Resources.FindObjectsOfTypeAll<MenuPreset>();
        if (menus == null) return;

        for (int mi = 0; mi < menus.Length; mi++)
        {
            var m = menus[mi];
            if (m == null) continue;
            if (m.syncDisks == null) continue;
            if (m.syncDisks.Count == 0) continue;

            var seen = new HashSet<string>();
            var keep = new List<SyncDiskPreset>();
            int removedHere = 0;

            for (int i = 0; i < m.syncDisks.Count; i++)
            {
                var p = m.syncDisks[i];
                if (p == null) continue;

                string key = p.name;
                if (key == null) key = "null_" + i;

                if (seen.Add(key)) keep.Add(p);
                else removedHere++;
            }

            if (removedHere == 0) continue;

            m.syncDisks.Clear();
            for (int i = 0; i < keep.Count; i++) m.syncDisks.Add(keep[i]);

            removedTotal += removedHere;
            menusTouched++;
        }

        if (removedTotal > 0 && Log != null)
            Log.LogInfo("[DEDUPE] removed " + removedTotal + " duplicate entries across " + menusTouched + " menu presets");
    }
}