using E = SyncDiskPreset.Effect;
using U = SyncDiskPreset.UpgradeEffect;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using UnityEngine;
using SOD.Common;
using SOD.Common.Helpers.SyncDiskObjects;

namespace SoDVanillaSplit;

[BepInPlugin(GUID, "SoD Vanilla Split", "0.5.0")]
[BepInDependency("Venomaus.SOD.Common", BepInDependency.DependencyFlags.HardDependency)]
public class Plugin : BasePlugin
{
    public const string GUID = "ta.sod.vanillasplit";
    internal static BepInEx.Logging.ManualLogSource L;
    internal static ConfigFile Cfg;

    internal static ConfigEntry<bool> RemoveParents;
    internal static ConfigEntry<bool> ClearParentSpawnData;
    internal static ConfigEntry<string> SaleLocation;
    internal static ConfigEntry<bool> VanillaBalance;

    // Registered presets, keyed by DiskName, handed to phase 2.
    internal static readonly Dictionary<string, SyncDiskPreset> Made = new();
    internal static readonly Dictionary<string, SplitDef> Defs = new();

    // Config-resolved prices, keyed by DiskName. Filled in Load(), read in
    // phase 2. Splits are priced for what the branch does, NOT copied from
    // the parent - a set of splits deliberately costs more than the parent.
    internal static readonly Dictionary<string, int> Prices = new();

    // Rebalance.Table rows keyed by disk name, built once in Load(). Used to
    // pick the config default price and, when VanillaBalance is on, the
    // vanilla price directly (see Copy() in SplitPatch).
    internal static readonly Dictionary<string, Rebalance.Row> RebalanceByDisk = new();

    public override void Load()
    {
        L = Log;
        L.LogInfo("=== SoDVanillaSplit 0.5.0 starting ===");
        Cfg = OpenSettings("sm3gm.sod.vanillasplit.cfg");

        RemoveParents = Cfg.Bind("General", "RemoveVanillaParents", false,
            "Remove the 17 split parent presets from Toolbox.allSyncDisks so only the "
          + "split disks are sold. Starch-SugarDaddy is never removed. "
          + "Leave false to run the splits ALONGSIDE vanilla.");

        ClearParentSpawnData = Cfg.Bind("General", "ClearParentSpawnData", true,
            "Also clear occupation and trait spawn data on the removed parents. "
          + "Toolbox removal alone suppresses vendor stock but NOT world loot: "
          + "a combined parent can still be found in the world. Only applies "
          + "when RemoveVanillaParents is true.");

        SaleLocation = Cfg.Bind("General", "SaleLocation", "SyncClinic",
            "Menu preset the split disks are added to. Blank for none.");

        VanillaBalance = Cfg.Bind("General", "VanillaBalance", false,
            "Revert every rebalanced payout, tier and price to the vanilla values shipped in 1.0.3. "
          + "When true, the per-disk price keys below are ignored in favour of the original prices.");

        string loc = SaleLocation.Value == null ? "" : SaleLocation.Value.Trim();

        foreach (var r in Rebalance.Table) RebalanceByDisk[r.Disk] = r;

        int made = 0;
        foreach (var d in Splits.All)
        {
            bool on = Cfg.Bind(d.Parent, d.DiskName, true,
                "Register this split disk.").Value;
            if (!on) { L.LogInfo("skipped: " + d.DiskName); continue; }

            int priceDefault = RebalanceByDisk.TryGetValue(d.DiskName, out var row)
                              ? row.NewPrice : d.Price;

            Prices[d.DiskName] = Cfg.Bind(d.Parent, d.DiskName + " price", priceDefault,
                "Credits. Split disks are priced for what the branch does, not "
              + "what the parent cost, so a set of splits costs more in total "
              + "than the disk they came from. Price is not a save key and can "
              + "be changed on a live save.").Value;

            try
            {
                Register(d, loc);
                made++;
            }
            catch (System.Exception e)
            {
                L.LogError("FAILED to register " + d.DiskName + ": " + e);
            }
        }

        Harmony.CreateAndPatchAll(typeof(SplitPatch));
        Harmony.CreateAndPatchAll(typeof(LootRemovePatch));
        L.LogInfo("=== SoDVanillaSplit phase 1 done, " + made + " of "
                + Splits.All.Length + " registered ===");
    }


    static ConfigFile OpenSettings(string newFileName)
    {
        string dest = Path.Combine(Paths.ConfigPath, newFileName);
        string src = Path.Combine(Paths.ConfigPath, GUID + ".cfg");
        if (File.Exists(src) && !File.Exists(dest))
        {
            File.Copy(src, dest);
            L.LogInfo("Copied settings from " + GUID + ".cfg to " + newFileName);
        }
        return new ConfigFile(dest, true);
    }

    // Phase 1. Vanilla presets do not exist yet, so this registers shells with
    // placeholder text. Real effects, values and descriptions arrive in phase 2.
    static void Register(SplitDef d, string loc)
    {
        var builder = Lib.SyncDisks.Builder(d.DiskName, GUID)
            .AddEffect(d.DiskName, "Pending vanilla data.", out int effId);

        if (loc.Length > 0) builder.AddSaleLocation(loc);

        if (d.Tiers > 0)
        {
            string t1 = d.DiskName + " upgrade one.";
            string t2 = d.DiskName + " upgrade two.";
            string t3 = d.DiskName + " upgrade three.";

            SyncDiskBuilder.Options opts;
            if (d.Tiers == 1) opts = new SyncDiskBuilder.Options(t1);
            else if (d.Tiers == 2) opts = new SyncDiskBuilder.Options(t1, t2);
            else opts = new SyncDiskBuilder.Options(t1, t2, t3);

            builder.AddUpgradeOption(opts, out _);
        }

        var disk = builder.CreateAndRegister();
        var p = disk.Preset;

        // Builder leaves 0.2 in the unused branches even though the effect is none.
        p.mainEffect2 = E.none;
        p.mainEffect2Value = 0f;
        p.mainEffect3 = E.none;
        p.mainEffect3Value = 0f;

        Made[d.DiskName] = p;
        Defs[d.DiskName] = d;

        L.LogInfo("shell registered: " + d.DiskName + " <- " + d.Parent
                + " branch " + d.Branch + " tiers " + d.Tiers + " customId " + effId);
    }

    // 0.4.4. An upgrade name reference is a KEY into the evidence.syncdisks DDS
    // table. SOD.Common registers keys before Toolbox.Start, so any key authored
    // in phase 2 must be registered here or it renders as a generic label.
    // The setter UPDATES existing keys, so never call this on a vanilla key.
    internal static int DdsRegistered = 0;
    internal static int DdsThrew = 0;

    internal static void RegisterUpgradeString(string s)
    {
        try
        {
            Lib.DdsStrings["evidence.syncdisks", s] = s;
            DdsRegistered++;
        }
        catch (System.Exception e)
        {
            DdsThrew++;
            L.LogWarning("[DDSREG] " + e.GetType().Name + " registering \"" + s + "\"");
        }
    }
}

[HarmonyPatch(typeof(Toolbox), nameof(Toolbox.Start))]
public static class SplitPatch
{
    static bool done = false;

    static void Postfix()
    {
        if (done) return;
        done = true;

        var parents = Index();
        Copy(parents);
        Rebalance.Apply(Plugin.Made);
        Remove(parents);
    }

    // Snapshot the vanilla presets by name before anything is removed.
    static Dictionary<string, SyncDiskPreset> Index()
    {
        var map = new Dictionary<string, SyncDiskPreset>();
        var all = Toolbox.Instance.allSyncDisks;
        if (all == null)
        {
            Plugin.L.LogError("=== Toolbox.allSyncDisks is null, phase 2 aborted ===");
            return map;
        }

        foreach (var p in all)
        {
            if (p == null) continue;
            foreach (var want in Splits.Parents)
            {
                if (p.name == want && !map.ContainsKey(want)) map[want] = p;
            }
        }

        Plugin.L.LogInfo("=== indexed " + map.Count + " of "
                       + Splits.Parents.Length + " parent presets ===");
        return map;
    }

    static void Copy(Dictionary<string, SyncDiskPreset> parents)
    {
        int ok = 0, miss = 0;

        foreach (var kv in Plugin.Made)
        {
            var dst = kv.Value;
            var d = Plugin.Defs[kv.Key];

            SyncDiskPreset src;
            if (!parents.TryGetValue(d.Parent, out src) || src == null)
            {
                Plugin.L.LogWarning("parent NOT FOUND for " + d.DiskName
                                  + " (" + d.Parent + ") - disk left as placeholder");
                miss++;
                continue;
            }

            // Shared preset-level data. Price is the ONE field deliberately not
            // copied from the parent - see SplitDef.Price and the config keys.
            // VanillaBalance ignores the bound config value entirely where a
            // Rebalance.Row exists and uses its VanillaPrice instead.
            if (Plugin.VanillaBalance.Value && Plugin.RebalanceByDisk.TryGetValue(d.DiskName, out var vrow))
            {
                dst.price = vrow.VanillaPrice;
            }
            else
            {
                int cfgPrice;
                dst.price = Plugin.Prices.TryGetValue(d.DiskName, out cfgPrice)
                          ? cfgPrice : src.price;
            }
            dst.rarity = src.rarity;
            dst.manufacturer = src.manufacturer;
            dst.uninstallCost = src.uninstallCost;
            dst.minimumWealthLevel = src.minimumWealthLevel;
            dst.canBeSideJobReward = src.canBeSideJobReward;
            dst.sideEffect = src.sideEffect;
            dst.sideEffectValue = src.sideEffectValue;
            dst.sideEffectDescription = src.sideEffectDescription;

            // Branch-specific data. Written inline per branch so no Il2Cpp list
            // type ever has to be named in a local declaration.
            if (d.Branch == 1)
            {
                dst.mainEffect1 = src.mainEffect1;
                dst.mainEffect1Value = src.mainEffect1Value;
                dst.mainEffect1Description = src.mainEffect1Description;
                dst.mainEffect1Icon = src.mainEffect1Icon;

                var se = src.option1UpgradeEffects;
                var sv = src.option1UpgradeValues;
                var sn = src.option1UpgradeNameReferences;
                var de = dst.option1UpgradeEffects;
                var dv = dst.option1UpgradeValues;
                var dn = dst.option1UpgradeNameReferences;
                for (int i = 0; i < d.Tiers; i++)
                {
                    if (i >= se.Count || i >= de.Count) break;
                    de[i] = Fix(se[i], d, i);
                    dv[i] = sv[i];
                    if (i < sn.Count && i < dn.Count) dn[i] = sn[i];
                }
            }
            else if (d.Branch == 2)
            {
                dst.mainEffect1 = src.mainEffect2;
                dst.mainEffect1Value = src.mainEffect2Value;
                dst.mainEffect1Description = src.mainEffect2Description;
                dst.mainEffect1Icon = src.mainEffect2Icon;

                var se = src.option2UpgradeEffects;
                var sv = src.option2UpgradeValues;
                var sn = src.option2UpgradeNameReferences;
                var de = dst.option1UpgradeEffects;
                var dv = dst.option1UpgradeValues;
                var dn = dst.option1UpgradeNameReferences;
                for (int i = 0; i < d.Tiers; i++)
                {
                    if (i >= se.Count || i >= de.Count) break;
                    de[i] = Fix(se[i], d, i);
                    dv[i] = sv[i];
                    if (i < sn.Count && i < dn.Count) dn[i] = sn[i];
                }
            }
            else
            {
                dst.mainEffect1 = src.mainEffect3;
                dst.mainEffect1Value = src.mainEffect3Value;
                dst.mainEffect1Description = src.mainEffect3Description;
                dst.mainEffect1Icon = src.mainEffect3Icon;

                var se = src.option3UpgradeEffects;
                var sv = src.option3UpgradeValues;
                var sn = src.option3UpgradeNameReferences;
                var de = dst.option1UpgradeEffects;
                var dv = dst.option1UpgradeValues;
                var dn = dst.option1UpgradeNameReferences;
                for (int i = 0; i < d.Tiers; i++)
                {
                    if (i >= se.Count || i >= de.Count) break;
                    de[i] = Fix(se[i], d, i);
                    dv[i] = sv[i];
                    if (i < sn.Count && i < dn.Count) dn[i] = sn[i];
                }
            }

            ok++;
            Plugin.L.LogInfo("copied " + d.DiskName + " | effect=" + dst.mainEffect1
                           + " value=" + dst.mainEffect1Value
                           + " mfr=" + dst.manufacturer + " price=" + dst.price
                           + " side=" + dst.sideEffect + " tiers=" + d.Tiers);
        }

        Plugin.L.LogInfo("=== copy pass complete: " + ok + " copied, "
                       + miss + " missing parents ===");
    }

    // bothConfigurations means "the other branch is also active". Meaningless on a
    // split disk, so it becomes a scaling of the disk's own effect instead.
    static U Fix(U e, SplitDef d, int tier)
    {
        if (e == U.bothConfigurations)
        {
            Plugin.L.LogInfo("override: " + d.DiskName + " tier " + (tier + 1)
                           + " bothConfigurations -> modifyEffect");
            return U.modifyEffect;
        }
        return e;
    }

    // NEW in 0.2.0. Toolbox removal suppresses VENDOR stock only. World loot
    // spawning runs off occupation and trait data on the preset, so a removed
    // parent could still be found in the world (confirmed in game with
    // ElGen-Vigor, two branches, after a logged 17-of-17 removal). Clearing the
    // four spawn fields is the attempt to close that path.
    static void ClearSpawnData(Dictionary<string, SyncDiskPreset> parents)
    {
        if (!Plugin.ClearParentSpawnData.Value)
        {
            Plugin.L.LogInfo("=== ClearParentSpawnData false, spawn data left intact ===");
            return;
        }

        int cleared = 0;

        foreach (var kv in parents)
        {
            var p = kv.Value;
            if (p == null) continue;

            try
            {
                int occBefore = 0;
                int traitBefore = 0;
                try { if (p.occupation != null) occBefore = p.occupation.Count; } catch { }
                try { if (p.traits != null) traitBefore = p.traits.Count; } catch { }

                if (p.occupation != null) p.occupation.Clear();
                p.occupationWeight = 0;
                if (p.traits != null) p.traits.Clear();
                p.traitWeight = 0;

                cleared++;
                Plugin.L.LogInfo("=== SPAWN CLEARED: " + kv.Key
                               + " occ " + occBefore + "->0"
                               + " traits " + traitBefore + "->0 ===");
            }
            catch (System.Exception e)
            {
                Plugin.L.LogError("spawn clear FAILED for " + kv.Key + ": " + e);
            }
        }

        Plugin.L.LogInfo("=== spawn clear complete: " + cleared + " of "
                       + parents.Count + " parents ===");
    }

    static void Remove(Dictionary<string, SyncDiskPreset> parents)
    {
        if (!Plugin.RemoveParents.Value)
        {
            Plugin.L.LogInfo("=== RemoveVanillaParents false, parents left in place ===");
            return;
        }

        // Clear spawn data BEFORE removing from Toolbox, while the presets are
        // still indexed and reachable.
        ClearSpawnData(parents);

        int tbRemoved = 0;
        var all = Toolbox.Instance.allSyncDisks;
        if (all != null)
        {
            for (int i = all.Count - 1; i >= 0; i--)
            {
                var p = all[i];
                if (p == null) continue;
                foreach (var w in Splits.Parents)
                {
                    if (p.name == w)
                    {
                        all.RemoveAt(i);
                        tbRemoved++;
                        Plugin.L.LogInfo("=== REMOVED from Toolbox.allSyncDisks: " + w + " ===");
                        break;
                    }
                }
            }
        }

        int menuRemoved = 0, menusSeen = 0;
        var menus = Resources.FindObjectsOfTypeAll<MenuPreset>();
        if (menus != null)
        {
            foreach (var menu in menus)
            {
                if (menu == null || menu.syncDisks == null) continue;
                menusSeen++;
                for (int i = menu.syncDisks.Count - 1; i >= 0; i--)
                {
                    var p = menu.syncDisks[i];
                    if (p == null) continue;
                    foreach (var w in Splits.Parents)
                    {
                        if (p.name == w) { menu.syncDisks.RemoveAt(i); menuRemoved++; break; }
                    }
                }
            }
        }

        Plugin.L.LogInfo("=== remove pass complete: " + tbRemoved + " from Toolbox, "
                       + menuRemoved + " across " + menusSeen + " menu presets ===");
    }
}

[HarmonyPatch(typeof(InteriorCreator), nameof(InteriorCreator.StartLoading))]
public static class LootRemovePatch
{
    static bool _loggedFirstCall = false;

    static void Prefix()
    {
        if (!_loggedFirstCall)
        {
            _loggedFirstCall = true;
            int total = Toolbox.Instance?.allSyncDisks?.Count ?? 0;
            int parentsPresent = 0;
            var firstCallDisks = Toolbox.Instance?.allSyncDisks;
            if (firstCallDisks != null)
            {
                foreach (var p in firstCallDisks)
                {
                    if (p == null) continue;
                    foreach (var w in Splits.Parents)
                    {
                        if (p.name == w) { parentsPresent++; break; }
                    }
                }
            }
            Plugin.L.LogInfo("[LOOT] first call, allSyncDisks=" + total
                           + ", parents present=" + parentsPresent);
        }

        if (!Plugin.RemoveParents.Value) return;

        var all = Toolbox.Instance?.allSyncDisks;
        if (all == null) return;

        int removed = 0;
        for (int i = all.Count - 1; i >= 0; i--)
        {
            var p = all[i];
            if (p == null) continue;
            foreach (var w in Splits.Parents)
            {
                if (p.name == w) { all.RemoveAt(i); removed++; break; }
            }
        }

        if (removed > 0)
            Plugin.L.LogInfo("=== LOOT PASS: removed " + removed
                           + " parents at chunk generation ===");
    }
}