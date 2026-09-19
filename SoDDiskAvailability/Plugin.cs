using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using UnityEngine;
 
namespace SoDDiskAvailability;
 
[BepInPlugin(GUID, "SoD Disk Availability", VER)]
[BepInDependency("ta.sod.vanillasplit", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("ta.sod.syncdiskpack", BepInDependency.DependencyFlags.SoftDependency)]
public class Plugin : BasePlugin
{
    public const string GUID = "ta.sod.diskavailability";
    public const string VER = "0.3.1";
 
    internal static BepInEx.Logging.ManualLogSource L;
    internal static ConfigFile Cfg;
 
    internal static ConfigEntry<string> Mode;
    internal static ConfigEntry<int> Salt;
    internal static ConfigEntry<bool> LogStock;
        public static ConfigEntry<bool> DedupeMenus;
    internal static ConfigEntry<bool> IncludeOtherMods;
 
    // One block of keys per vendor pool.
    internal static readonly List<Pool> Pools = new();
 
    public override void Load()
    {
        L = Log;
        L.LogInfo("=== SoD Disk Availability " + VER + " starting ===");
        Cfg = OpenSettings("sm3gm.sod.diskavailability.cfg");
 
        Mode = Cfg.Bind("General", "Mode", "Rotation",
            "Unrestricted = every disk stocked everywhere, always. "
          + "Vanilla = restore the stock lists as the game shipped them. "
          + "Rotation = each vendor stocks a small set that changes daily.");
 
        Salt = Cfg.Bind("General", "RotationSalt", 0,
            "Change this number to get a different rotation sequence. "
          + "Rotation is derived from the in-game day plus this salt, so the same "
          + "day always produces the same stock and reloading never rerolls it.");
 
                LogStock = Cfg.Bind("General", "LogStockToConsole", true,
            "Write the chosen stock to the BepInEx log on every rotation.");

        DedupeMenus = Cfg.Bind("General", "FixVendorDuplicates", true,
            "Removes duplicate sync disk entries that build up in vendor stock lists "
          + "when a city is loaded more than once in the same session. Works around a "
          + "known issue in SOD.Common 2.1.4. Leave this on unless it conflicts with another mod.");

        // Default false: LifeAndLiving's Echolocation disk was being drawn into
        // the daily rotation and taking a clinic slot. The rotation is for
        // vanilla plus this suite. Turn this on to put other mods' disks back
        // in the pool.
        IncludeOtherMods = Cfg.Bind("General", "IncludeDisksFromOtherMods", false,
            "When false, Rotation and Unrestricted ignore sync disks registered by "
          + "other mods (for example LifeAndLiving Echolocation). Vanilla disks and "
          + "this suite's pack and split disks stay in the pool. Default false "
          + "because those extra disks were taking rotation slots unintentionally.");

        DedupePatch.Enabled = DedupeMenus.Value;
        DedupePatch.Log = L;
 
        // preset name, default count, default extra random slots
        Add("SyncClinic", "SyncClinics", 4, 0,
            "The ordinary sync clinics. There are usually six or seven in a city and "
          + "they all share one stock list, so they will show the same disks as each other.");
        Add("BlackmarketSyncClinic", "BlackMarketClinic", 6, 0,
            "The black market doctor. Stocks independently of the ordinary clinics.");
        Add("BlackmarketTrader", "BlackMarketTrader", 3, 0,
            "The black market trader. Vanilla sells almost no disks here.");
        Add("WeaponsDealer", "WeaponsDealer", 3, 0,
            "The arms dealer. Vanilla sells no disks here at all, so this gives a "
          + "reason to visit in person.");
        Add("Newsstand", "Newsstands", 2, 0,
            "Street newsstands. Vanilla draws one random CandorNews disk.");
        Add("NewspaperBox", "NewspaperBoxes", 2, 0,
            "Coin-operated newspaper boxes. Vanilla sells no disks here.");
 
        Harmony.CreateAndPatchAll(typeof(StockPatch));
                Harmony.CreateAndPatchAll(typeof(DedupePatch));
        Harmony.CreateAndPatchAll(typeof(LoadAllPatch));
        AddComponent<DayWatcher>();
 
        L.LogInfo("=== SoD Disk Availability ready, mode=" + Mode.Value + " ===");
    }
 

    static ConfigFile OpenSettings(string newFileName)
    {
        string dest = Path.Combine(Paths.ConfigPath, newFileName);
        try
        {
            string src = Path.Combine(Paths.ConfigPath, GUID + ".cfg");
            if (File.Exists(src) && !File.Exists(dest))
            {
                File.Copy(src, dest);
                L.LogInfo("Copied settings from " + GUID + ".cfg to " + newFileName);
            }
        }
        catch (System.Exception)
        {
        }
        return new ConfigFile(dest, true);
    }

    void Add(string preset, string section, int count, int slots, string blurb)
    {
        var p = new Pool
        {
            Preset = preset,
            Enabled = Cfg.Bind(section, "Enabled", true,
                blurb + " Set false to leave this vendor exactly as the game left it."),
            Count = Cfg.Bind(section, "DisksInStock", count,
                "How many disks this vendor offers at once in Rotation mode. 0 for none."),
            Slots = Cfg.Bind(section, "ExtraRandomSlots", slots,
                "Additional disks the game draws at random on top of the list above, "
              + "filtered by the manufacturers below. Vanilla uses 1 for sync clinics."),
            Manufacturers = Cfg.Bind(section, "RestrictToManufacturers", "",
                "Comma separated. Blank means draw from every disk in the game. "
              + "Valid names: ElGen, Kaizen, KensingtonIndigo, StarchKola, CandorNews, BlackMarket.")
        };
        Pools.Add(p);
    }
}
 
internal class Pool
{
    public string Preset;
    public ConfigEntry<bool> Enabled;
    public ConfigEntry<int> Count;
    public ConfigEntry<int> Slots;
    public ConfigEntry<string> Manufacturers;
}
 
// Runs after SoDVanillaSplit's own Toolbox.Start postfix, so the pool it captures
// already reflects any parent removal and any custom disks that were registered.
[HarmonyPatch(typeof(Toolbox), nameof(Toolbox.Start))]
public static class StockPatch
{
    static bool captured = false;
 
    [HarmonyPriority(Priority.Last)]
    static void Postfix()
    {
        if (captured) return;
        captured = true;
 
        try
        {
            Stock.CaptureOriginals();
            Stock.Apply(force: true);
        }
        catch (Exception e)
        {
            Plugin.L.LogError("initial stock pass failed: " + e);
        }
    }
}

[HarmonyPatch(typeof(Toolbox), "LoadAll")]
public static class LoadAllPatch
{
    [HarmonyPriority(Priority.Last)]
    static void Postfix()
    {
        try { Stock.Apply(force: true); }
        catch { }
    }
}

// Polls for the in-game day changing. Cheap: one int compare per second.
public class DayWatcher : MonoBehaviour
{
    public DayWatcher(IntPtr ptr) : base(ptr) { }
 
    float _next;
 
    void Update()
    {
        if (Time.time < _next) return;
        _next = Time.time + 1f;
 
        try { Stock.Apply(force: false); }
        catch { /* a failed poll must never spam or crash */ }
    }
}
 
internal static class Stock
{
    // The full set of disks available to draw from, snapshotted once.
    static readonly List<SyncDiskPreset> MasterPool = new();
 
    // Each pool's stock list exactly as the game had it, for Vanilla mode.
    static readonly Dictionary<string, List<SyncDiskPreset>> Originals = new();
    static readonly Dictionary<string, int> OriginalSlots = new();
 
    static int _lastDay = int.MinValue;
    static string _lastMode = null;
 
    public static void CaptureOriginals()
    {
        MasterPool.Clear();
        Originals.Clear();
        OriginalSlots.Clear();
 
        var all = Toolbox.Instance.allSyncDisks;
        if (all == null)
        {
            Plugin.L.LogError("Toolbox.allSyncDisks null - availability disabled this session");
            return;
        }
 
        int skippedOther = 0;
        var skippedNames = new System.Text.StringBuilder();
        for (int i = 0; i < all.Count; i++)
        {
            var p = all[i];
            if (p == null) continue;
            if (p.disabled) continue;
            if (!Plugin.IncludeOtherMods.Value && IsOtherModDisk(p))
            {
                skippedOther++;
                if (skippedNames.Length > 0) skippedNames.Append(", ");
                skippedNames.Append(Short(p.name));
                continue;
            }
            MasterPool.Add(p);
        }
        if (skippedOther > 0)
            Plugin.L.LogInfo("[STOCK] excluded " + skippedOther
                           + " disk(s) from other mods: " + skippedNames);
 
        var menus = Resources.FindObjectsOfTypeAll<MenuPreset>();
        if (menus != null)
        {
            foreach (var m in menus)
            {
                if (m == null || m.name == null) continue;
                if (Originals.ContainsKey(m.name)) continue;   // presets can appear twice
 
                var snap = new List<SyncDiskPreset>();
                if (m.syncDisks != null)
                {
                    for (int i = 0; i < m.syncDisks.Count; i++)
                    {
                        var p = m.syncDisks[i];
                        if (p != null) snap.Add(p);
                    }
                }
                Originals[m.name] = snap;
                OriginalSlots[m.name] = m.syncDiskSlots;
            }
        }
 
        Plugin.L.LogInfo("=== captured " + MasterPool.Count + " disks in the master pool, "
                       + Originals.Count + " menu presets snapshotted ===");
    }
 
    static int CurrentDay()
    {
        try { return SessionData.Instance.dayInt; }
        catch { return 0; }
    }
 
    static bool _warnedNoPool = false;

    public static void Apply(bool force)
    {
        if (MasterPool.Count == 0)
        {
            if (!_warnedNoPool)
            {
                _warnedNoPool = true;
                Plugin.L.LogInfo("[STOCK] apply skipped, master pool not captured yet");
            }
            return;
        }
 
        string mode = (Plugin.Mode.Value ?? "Rotation").Trim();
        int day = CurrentDay();
 
        if (!force && day == _lastDay && mode == _lastMode) return;
        _lastDay = day;
        _lastMode = mode;
 
        bool unrestricted = mode.Equals("Unrestricted", StringComparison.OrdinalIgnoreCase);
        bool vanilla = mode.Equals("Vanilla", StringComparison.OrdinalIgnoreCase);
 
        foreach (var pool in Plugin.Pools)
        {
            if (!pool.Enabled.Value) continue;
 
            var menus = Resources.FindObjectsOfTypeAll<MenuPreset>();
            if (menus == null) continue;
 
            foreach (var m in menus)
            {
                if (m == null || m.name != pool.Preset) continue;
                if (m.syncDisks == null) continue;
 
                if (vanilla)
                {
                    WriteList(m, Originals.ContainsKey(m.name) ? Originals[m.name] : new List<SyncDiskPreset>());
                    if (OriginalSlots.ContainsKey(m.name)) m.syncDiskSlots = OriginalSlots[m.name];
                    continue;
                }
 
                if (unrestricted)
                {
                    WriteList(m, MasterPool);
                    m.syncDiskSlots = pool.Slots.Value;
                    continue;
                }
 
                // Rotation
                var eligible = Filter(pool.Manufacturers.Value);
                var picked = Pick(eligible, pool.Count.Value, day, pool.Preset);
                WriteList(m, picked);
                m.syncDiskSlots = pool.Slots.Value;
 
                if (Plugin.LogStock.Value)
                {
                    var names = new System.Text.StringBuilder();
                    foreach (var p in picked)
                    {
                        if (names.Length > 0) names.Append(", ");
                        names.Append(Short(p.name));
                    }
                    Plugin.L.LogInfo("[STOCK] day " + day + " " + pool.Preset
                                   + " (" + picked.Count + "): " + names);
                }
            }
        }
    }
 
    // SOD.Common names are {id}_{hash}_{True|False}_{Name}. Take everything
    // after the bool token so a name with extra underscores stays intact.
    // LastIndexOf('_') keeps only the last word if Name contains underscores.
    static string Short(string n)
    {
        if (string.IsNullOrEmpty(n)) return "?";
        const string trueTok = "_True_";
        const string falseTok = "_False_";
        int t = n.IndexOf(trueTok, StringComparison.Ordinal);
        if (t >= 0) return n.Substring(t + trueTok.Length);
        int f = n.IndexOf(falseTok, StringComparison.Ordinal);
        if (f >= 0) return n.Substring(f + falseTok.Length);
        return n;
    }

    // SOD.Common custom disks share one {id}_{hash}_{bool}_{Name} shape, so
    // plugin GUID is not in the preset name. Treat custom names outside the
    // suite allowlist as other-mod disks (LifeAndLiving Echolocation).
    static readonly HashSet<string> SuiteNames = new HashSet<string>(StringComparer.Ordinal)
    {
        "Quickstep", "Low Profile", "Squatter's Rights", "Homebound", "Signpost",
        "Persona Non Grata", "Free Rein", "Deep Pockets", "Sprinter's Curse",
        "Iron Lung", "Leap", "Dead Air", "Plus One", "Muckraker", "Blank Face",
        "Company Man", "Second Wind", "Long Arm", "Lights Out",
        "Clout", "Brawn", "Reflexes", "Lockpicker", "Resourceful", "Invisible",
        "Gold Medical Cover", "Gold Legal Cover", "Gold Accident Cover",
        "Street Cleaner", "Bookworm", "Power", "Stability",
        "Physiological Perception", "Socioeconomic Perception",
        "Food Hygeine Inspector", "Sanitary Hygeine Inspector",
        "Rogue", "Hacker", "Cash Flow", "Competitor Data Mining",
        "Fortitude", "Vitality", "Allure", "Charm", "Chemistry", "Cardiovascular",
        "Spread the word!", "Put some life into it!",
        "Urbex Cartographer", "Crawlspace Engineer",
        "Care in the Community", "Heavy Lifter", "Statuesque", "Compact",
        "Mailing List", "Safecacker"
    };

    static bool IsOtherModDisk(SyncDiskPreset p)
    {
        if (p == null || string.IsNullOrEmpty(p.name)) return false;
        string n = p.name;
        if (n.IndexOf("_True_", StringComparison.Ordinal) < 0
            && n.IndexOf("_False_", StringComparison.Ordinal) < 0)
            return false;
        return !SuiteNames.Contains(Short(n));
    }
 
    static void WriteList(MenuPreset m, List<SyncDiskPreset> items)
    {
        // Wholesale rewrite, which makes this naturally idempotent. That matters:
        // SOD.Common appends custom disks to menu presets once per city load with
        // no containment check, and this pass overwrites the duplicates away.
        m.syncDisks.Clear();
        foreach (var p in items)
        {
            if (p != null) m.syncDisks.Add(p);
        }
    }
 
    static List<SyncDiskPreset> Filter(string csv)
    {
        if (string.IsNullOrEmpty(csv) || csv.Trim().Length == 0) return MasterPool;
 
        var wanted = new List<string>();
        foreach (var s in csv.Split(','))
        {
            var t = s.Trim();
            if (t.Length > 0) wanted.Add(t);
        }
        if (wanted.Count == 0) return MasterPool;
 
        var outp = new List<SyncDiskPreset>();
        foreach (var p in MasterPool)
        {
            string mfr;
            try { mfr = p.manufacturer.ToString(); } catch { continue; }
            foreach (var w in wanted)
            {
                if (string.Equals(mfr, w, StringComparison.OrdinalIgnoreCase)) { outp.Add(p); break; }
            }
        }
        return outp.Count > 0 ? outp : MasterPool;
    }
 
    // Deterministic shuffle. Same day plus same salt plus same vendor always
    // gives the same stock, so a save reload never rerolls the shop.
    //
    // TODO: mix a city seed in here so two different cities differ on day 1.
    // Nothing in the assembly has been confirmed to expose one yet.
    static List<SyncDiskPreset> Pick(List<SyncDiskPreset> from, int count, int day, string vendor)
    {
        var result = new List<SyncDiskPreset>();
        if (count <= 0 || from.Count == 0) return result;
        if (count >= from.Count)
        {
            result.AddRange(from);
            return result;
        }
 
        uint seed = (uint)(day * 2654435761u);
        seed ^= (uint)(Plugin.Salt.Value * 40503);
        foreach (char c in vendor) seed = seed * 31u + c;
        if (seed == 0) seed = 2463534242u;
 
        var idx = new List<int>();
        for (int i = 0; i < from.Count; i++) idx.Add(i);
 
        // Fisher-Yates driven by xorshift32.
        for (int i = idx.Count - 1; i > 0; i--)
        {
            seed ^= seed << 13; seed ^= seed >> 17; seed ^= seed << 5;
            int j = (int)(seed % (uint)(i + 1));
            int tmp = idx[i]; idx[i] = idx[j]; idx[j] = tmp;
        }
 
        for (int i = 0; i < count; i++) result.Add(from[idx[i]]);
        return result;
    }
}
