using E = SyncDiskPreset.Effect;
using U = SyncDiskPreset.UpgradeEffect;
using System.Collections.Generic;

namespace SoDVanillaSplit;

public static class Rebalance
{
    public struct Tier
    {
        public U Effect;
        public float Value;
        public string Desc;

        public Tier(U effect, float value, string desc = null)
        {
            Effect = effect;
            Value = value;
            Desc = desc;
        }
    }

    public struct Row
    {
        public string Disk;
        public bool ChangeMain;
        public E MainEffect;
        public float MainValue;
        public string MainDesc;
        public int NewPrice;
        public int VanillaPrice;
        public Tier?[] Tiers;
        public int[] Reorder;
    }

    public static readonly Row[] Table = new Row[]
    {
        // --- GROUP A ---
        new Row { Disk="Safecacker", NewPrice=750, VanillaPrice=500, ChangeMain=false,
            Tiers = new Tier?[] {
                new Tier(U.moneyForPasscodes, 10f, "Buyers pay a better rate for cracked passcodes"),
                new Tier(U.moneyForPasscodes, 10f, "Your passcode work earns a premium"),
                new Tier(U.moneyForPasscodes, 10f, "Top rate on every passcode you crack"),
            } },

        new Row { Disk="Mailing List", NewPrice=650, VanillaPrice=500, ChangeMain=false,
            Tiers = new Tier?[] {
                new Tier(U.moneyForAddresses, 10f, "Buyers pay a better rate for logged addresses"),
                new Tier(U.moneyForAddresses, 10f, "Your address listings earn a premium"),
                new Tier(U.moneyForAddresses, 10f, "Top rate on every address you log"),
            } },

        new Row { Disk="Crawlspace Engineer", NewPrice=700, VanillaPrice=500, ChangeMain=false,
            Tiers = new Tier?[] {
                new Tier(U.moneyForDucts, 10f, "Buyers pay a better rate for mapped ducts"),
                new Tier(U.moneyForDucts, 10f, "Your duct surveys earn a premium"),
                new Tier(U.moneyForDucts, 10f, "Top rate on every duct you map"),
            } },

        new Row { Disk="Urbex Cartographer", NewPrice=550, VanillaPrice=500, ChangeMain=false,
            Tiers = new Tier?[] {
                new Tier(U.moneyForLocations, 7f, "Buyers pay a better rate for new locations"),
                new Tier(U.moneyForLocations, 7f, "Your location reports earn a premium"),
                new Tier(U.moneyForLocations, 6f, "Top rate on every location you discover"),
            } },

        new Row { Disk="Cash Flow", NewPrice=600, VanillaPrice=500, ChangeMain=false,
            Tiers = new Tier?[] {
                new Tier(U.passiveIncome, 10f, "Your holdings return a little more each hour"),
                new Tier(U.passiveIncome, 10f, "Dividends improve again"),
                new Tier(U.passiveIncome, 10f, "Your portfolio pays out at its best rate"),
            } },

        new Row { Disk="Competitor Data Mining", NewPrice=550, VanillaPrice=500, ChangeMain=false,
            Tiers = new Tier?[] {
                null,
                new Tier(U.installMalware, 25f, "Each installation harvests more data"),
                new Tier(U.installMalware, 25f, "Your malware returns its maximum yield"),
            } },

        new Row { Disk="Street Cleaner", NewPrice=450, VanillaPrice=500, ChangeMain=false,
            Tiers = new Tier?[] {
                new Tier(U.modifyEffect, 3f, "The city pays a better rate for litter cleared"),
                new Tier(U.modifyEffect, 3f, "Sanitation contracts pay a premium"),
                new Tier(U.noSmelly, 1f, "You never carry the smell of the job home"),
            } },

        new Row { Disk="Bookworm", NewPrice=600, VanillaPrice=500, ChangeMain=false,
            Tiers = new Tier?[] {
                new Tier(U.modifyEffect, 10f, "Every book you finish pays a better rate"),
                new Tier(U.readingSeriesBonus, 400f, "Completing a series pays a substantial bonus"),
                new Tier(U.modifyEffect, 10f, "Top rate on every book you finish"),
            } },

        new Row { Disk="Food Hygeine Inspector", NewPrice=500, VanillaPrice=500, ChangeMain=false,
            Tiers = new Tier?[] {
                new Tier(U.kitchenPhotos, 50f, "Kitchen reports earn a better fee"),
                new Tier(U.kitchenPhotos, 50f, "Your inspections command a premium"),
                new Tier(U.kitchenPhotos, 50f, "Top fee on every kitchen you document"),
            } },

        new Row { Disk="Sanitary Hygeine Inspector", NewPrice=500, VanillaPrice=500, ChangeMain=false,
            Tiers = new Tier?[] {
                new Tier(U.bathroomPhotos, 50f, "Bathroom reports earn a better fee"),
                new Tier(U.bathroomPhotos, 50f, "Your inspections command a premium"),
                new Tier(U.bathroomPhotos, 50f, "Top fee on every bathroom you document"),
            } },

        new Row { Disk="Spread the word!", NewPrice=100, VanillaPrice=500, ChangeMain=false,
            Tiers = new Tier?[] {
                new Tier(U.starchAmbassador, 3f, "Starch rewards your advocacy more generously"),
                new Tier(U.starchAmbassador, 3f, "Your ambassador standing improves again"),
                new Tier(U.starchAmbassador, 4f, "Maximum reward for every word you spread"),
            } },

        new Row { Disk="Put some life into it!", NewPrice=100, VanillaPrice=500, ChangeMain=false,
            Tiers = new Tier?[] {
                new Tier(U.starchGive, 5f, "Every can you hand out returns more"),
                new Tier(U.starchGive, 5f, "Starch increases your distribution bonus"),
                new Tier(U.starchGive, 6f, "Maximum return on every can you give away"),
            } },

        new Row { Disk="Care in the Community", NewPrice=800, VanillaPrice=750, ChangeMain=false,
            Tiers = new Tier?[] {
                new Tier(U.sideJobPayModifier, 0.05f, "Side work pays a better rate"),
                new Tier(U.sideJobPayModifier, 0.05f, "Clients offer a further premium"),
                new Tier(U.sideJobPayModifier, 0.05f, "Top rate on every side job you take"),
            } },

        new Row { Disk="Gold Accident Cover", NewPrice=750, VanillaPrice=750, ChangeMain=false,
            Tiers = new Tier?[] {
                new Tier(U.accidentCover, 50f, "Your accident payout increases"),
                new Tier(U.accidentCover, 50f, "Kensington raises your cover again"),
                new Tier(U.accidentCover, 100f, "Full gold-tier accident settlement"),
            } },

        new Row { Disk="Statuesque", NewPrice=600, VanillaPrice=750, ChangeMain=false,
            Tiers = new Tier?[] {
                new Tier(U.playerHeightModifier, 0.05f, "You stand taller still"),
                new Tier(U.playerHeightModifier, 0.05f, "Your frame extends further"),
                new Tier(U.maxSpeedModifier, 0.05f, "Longer stride, greater speed"),
            } },

        new Row { Disk="Compact", NewPrice=900, VanillaPrice=750, ChangeMain=false,
            Tiers = new Tier?[] {
                new Tier(U.playerHeightModifier, -0.05f, "You fold down further"),
                new Tier(U.playerHeightModifier, -0.05f, "Your frame compacts again"),
                new Tier(U.reachModifier, 0.1f, "You learn to stretch for what you cannot reach"),
            } },

        // --- GROUP B (empty lists today, all three indices authored from nothing) ---
        new Row { Disk="Lockpicker", NewPrice=1750, VanillaPrice=1750, ChangeMain=true,
            MainEffect=E.lockpickingSpeedModifier, MainValue=0.15f, MainDesc=null,
            Tiers = new Tier?[] {
                new Tier(U.lockpickingSpeedModifier, 0.1f, "Your picks find the pins faster"),
                new Tier(U.lockpickingSpeedModifier, 0.1f, "Practice shaves more time off every lock"),
                new Tier(U.lockpickingSpeedModifier, 0.15f, "Locks open almost as fast as you reach them"),
            } },

        new Row { Disk="Resourceful", NewPrice=1750, VanillaPrice=1750, ChangeMain=true,
            MainEffect=E.lockpickingEfficiencyModifier, MainValue=0.15f, MainDesc=null,
            Tiers = new Tier?[] {
                new Tier(U.lockpickingEfficiencyModifier, 0.1f, "Your picks survive more punishment"),
                new Tier(U.lockpickingEfficiencyModifier, 0.1f, "You waste fewer picks on every lock"),
                new Tier(U.lockpickingEfficiencyModifier, 0.15f, "A single pick lasts far longer than it should"),
            } },

        new Row { Disk="Invisible", NewPrice=1750, VanillaPrice=1250, ChangeMain=false,
            Tiers = new Tier?[] {
                new Tier(U.securityGraceTimeModifier, 0.25f, "Security takes longer to notice you"),
                new Tier(U.securityBreakerModifier, 0.5f, "Breaker boxes yield to you more readily"),
                new Tier(U.lockpickingSpeedModifier, 0.2f, "Your picks find the pins noticeably faster"),
            } },

        new Row { Disk="Power", NewPrice=2500, VanillaPrice=2500, ChangeMain=true,
            MainEffect=E.doorBargeModifier, MainValue=0.5f, MainDesc="Doors give under your shoulder more often than they should",
            Tiers = new Tier?[] {
                new Tier(U.doorBargeModifier, 0.5f, "Frames splinter more readily"),
                new Tier(U.doorBargeModifier, 0.5f, "Few doors hold against you"),
                new Tier(U.doorBargeModifier, 99999f, "No door in the city holds against you"),
            } },

        new Row { Disk="Stability", NewPrice=3000, VanillaPrice=3000, ChangeMain=true,
            MainEffect=E.fallDamageModifier, MainValue=-0.3f, MainDesc="You land harder than most people and feel less of it",
            Tiers = new Tier?[] {
                new Tier(U.fallDamageModifier, -0.2f, "Longer drops stop hurting"),
                new Tier(U.fallDamageModifier, -0.2f, "You shrug off falls that would break others"),
                new Tier(U.fallDamageModifier, -0.3f, "You land from any height without injury"),
            } },

        // --- GROUP C (branch 1 holds one element today, index 0 overwritten, 1 and 2 appended) ---
        new Row { Disk="Rogue", NewPrice=1750, VanillaPrice=1750, ChangeMain=true,
            MainEffect=E.KOTimeModifier, MainValue=0.5f, MainDesc=null,
            Tiers = new Tier?[] {
                new Tier(U.KOTimeModifier, 0.25f, "Your targets stay down longer"),
                new Tier(U.KOTimeModifier, 0.25f, "They stay down longer still"),
                new Tier(U.KOTimeModifier, 0.5f, "Anyone you drop stays down for a long while"),
            } },

        new Row { Disk="Hacker", NewPrice=1750, VanillaPrice=1750, ChangeMain=true,
            MainEffect=E.securityBreakerModifier, MainValue=0.5f, MainDesc=null,
            Tiers = new Tier?[] {
                new Tier(U.securityBreakerModifier, 0.25f, "Breakers take less work to trip"),
                new Tier(U.securityBreakerModifier, 0.25f, "Security systems fold faster"),
                new Tier(U.securityBreakerModifier, 0.5f, "Breaker boxes barely slow you down"),
            } },

        new Row { Disk="Fortitude", NewPrice=1750, VanillaPrice=1750, ChangeMain=true,
            MainEffect=E.increaseHealth, MainValue=0.1f, MainDesc=null,
            Tiers = new Tier?[] {
                new Tier(U.increaseHealth, 0.05f, "You can take a little more punishment"),
                new Tier(U.increaseHealth, 0.05f, "Your constitution improves again"),
                new Tier(U.increaseRegeneration, 0.1f, "Your body mends itself faster"),
            } },

        // --- GROUP D ---
        new Row { Disk="Clout", NewPrice=1500, VanillaPrice=1500, ChangeMain=false,
            Tiers = null, Reorder = new int[] { 2, 1, 0 } },

        new Row { Disk="Physiological Perception", NewPrice=1750, VanillaPrice=1750, ChangeMain=false,
            Tiers = new Tier?[] {
                null,
                null,
                new Tier(U.dialogChanceModifier, 0.25f, "People open up to you again, whatever you have installed"),
            } },

        new Row { Disk="Socioeconomic Perception", NewPrice=1000, VanillaPrice=1000, ChangeMain=false,
            Tiers = new Tier?[] {
                null,
                null,
                new Tier(U.dialogChanceModifier, 0.25f, "People open up to you again, whatever you have installed"),
            } },

        // --- THE CONSTITUTION PAIR ---
        new Row { Disk="Chemistry", NewPrice=2000, VanillaPrice=2000, ChangeMain=true,
            MainEffect=E.incomingDamageModifier, MainValue=-0.1f, MainDesc="A tailored blood chemistry that blunts what the city does to you",
            Tiers = new Tier?[] {
                new Tier(U.increaseHealth, 0.2f, "Your body carries more damage before it fails"),
                new Tier(U.incomingDamageModifier, -0.1f, "Blows land softer still"),
                new Tier(U.noCold, 1f, "The cold no longer touches you"),
            } },

        new Row { Disk="Cardiovascular", NewPrice=2000, VanillaPrice=2000, ChangeMain=false,
            Tiers = new Tier?[] {
                null,
                null,
                new Tier(U.noTired, 1f, "Your stamina no longer runs out"),
            } },
    };

    // Phase 2, runs after the parent copy pass and after Fix() have both
    // finished for all 37 disks. Writes the rebalanced payouts, tiers and
    // prices on top of what Copy() already placed. No-ops entirely when
    // VanillaBalance is on, leaving the straight vanilla copy in place.
    internal static void Apply(Dictionary<string, SyncDiskPreset> made)
    {
        if (Plugin.VanillaBalance.Value) return;

        int rows = 0, mains = 0, tiers = 0, reorders = 0, skipped = 0;

        foreach (var row in Table)
        {
            rows++;

            SyncDiskPreset p;
            if (!made.TryGetValue(row.Disk, out p) || p == null)
            {
                Plugin.L.LogWarning("[REBAL] preset not found for " + row.Disk);
                skipped++;
                continue;
            }

            if (row.Reorder != null)
            {
                ApplyReorder(p, row.Reorder);
                reorders++;
            }
            else if (row.Tiers != null)
            {
                tiers += ApplyTiers(p, row.Tiers);
            }

            if (row.ChangeMain)
            {
                p.mainEffect1 = row.MainEffect;
                p.mainEffect1Value = row.MainValue;
                if (row.MainDesc != null)
                {
                    p.mainEffect1Description = row.MainDesc;
                    Plugin.RegisterUpgradeString(row.MainDesc);
                }
                mains++;
            }
        }

        Plugin.L.LogInfo("[REBAL] rows=" + rows + " mains=" + mains + " tiers=" + tiers
                        + " reorders=" + reorders + " skipped=" + skipped);
        Plugin.L.LogInfo("[DDSREG] registered=" + Plugin.DdsRegistered + " threw=" + Plugin.DdsThrew);
    }

    // Writes the non-null Tier slots at their own index. A slot with Desc ==
    // null writes Effect and Value only, keeping whatever name reference is
    // already at that index. A slot with a non-null Desc writes all three
    // and registers the new name reference for DDS. Lists shorter than the
    // index being written are appended to first so effects, values and name
    // references always stay the same length and order.
    static int ApplyTiers(SyncDiskPreset p, Tier?[] slots)
    {
        var de = p.option1UpgradeEffects;
        var dv = p.option1UpgradeValues;
        var dn = p.option1UpgradeNameReferences;

        if (de == null) { de = new Il2CppSystem.Collections.Generic.List<U>(); p.option1UpgradeEffects = de; }
        if (dv == null) { dv = new Il2CppSystem.Collections.Generic.List<float>(); p.option1UpgradeValues = dv; }
        if (dn == null) { dn = new Il2CppSystem.Collections.Generic.List<string>(); p.option1UpgradeNameReferences = dn; }

        int written = 0;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null) continue;
            var t = slots[i].Value;

            while (de.Count <= i) de.Add(U.none);
            while (dv.Count <= i) dv.Add(0f);
            while (dn.Count <= i) dn.Add("");

            de[i] = t.Effect;
            dv[i] = t.Value;

            if (t.Desc != null)
            {
                dn[i] = t.Desc;
                Plugin.RegisterUpgradeString(t.Desc);
            }

            written++;
        }

        return written;
    }

    // Only Clout uses this. Permutes the three existing lists by the index
    // map so live order noBleeding, punchPowerModifier, fistsThreatModifier
    // becomes fistsThreatModifier, punchPowerModifier, noBleeding. Name
    // references move with their elements - no new strings, nothing to
    // register with DDS.
    static void ApplyReorder(SyncDiskPreset p, int[] order)
    {
        var de = p.option1UpgradeEffects;
        var dv = p.option1UpgradeValues;
        var dn = p.option1UpgradeNameReferences;
        if (de == null || dv == null || dn == null) return;

        var oe = new U[order.Length];
        var ov = new float[order.Length];
        var on = new string[order.Length];

        for (int i = 0; i < order.Length; i++)
        {
            int src = order[i];
            oe[i] = de[src];
            ov[i] = dv[src];
            on[i] = dn[src];
        }

        for (int i = 0; i < order.Length; i++)
        {
            de[i] = oe[i];
            dv[i] = ov[i];
            dn[i] = on[i];
        }
    }
}
