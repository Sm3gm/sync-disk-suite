using E = SyncDiskPreset.Effect;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using UnityEngine;
using SOD.Common;
using SOD.Common.Helpers.SyncDiskObjects;

namespace SoDDiskPack;

[BepInPlugin(GUID, "SoD Disk Pack", "0.12.1")]
[BepInDependency("Venomaus.SOD.Common", BepInDependency.DependencyFlags.HardDependency)]
public class Plugin : BasePlugin
{
    public const string GUID = "ta.sod.syncdiskpack";
        public const string VER = "0.12.1";
    internal static BepInEx.Logging.ManualLogSource L;
    internal static ConfigFile Cfg;
    internal static ConfigEntry<string> DisableList;
    internal static ConfigEntry<string> RemoveList;

    // Quickstep tier config
    internal static ConfigEntry<float> QsTier1;
    internal static ConfigEntry<float> QsTier2;
    internal static ConfigEntry<float> QsTier3;

    // Leap, absolute values per handoff section 18
    internal static ConfigEntry<float> LeapJump0, LeapJump1, LeapJump2, LeapJump3;
    internal static ConfigEntry<float> LeapFall0, LeapFall1, LeapFall2, LeapFall3;

    // Iron Lung, fractional deltas applied to captured baselines
    internal static ConfigEntry<float> IlTiredness;
    internal static ConfigEntry<float> IlRecovery;
    internal static ConfigEntry<float> IlHunger;
    internal static ConfigEntry<float> IlThirst;
    internal static ConfigEntry<float> IlGaitCure;

    // Sprinter's Curse, fractional deltas
    internal static ConfigEntry<float> ScSpeedPerTier;
    internal static ConfigEntry<float> ScTiredness;
    internal static ConfigEntry<float> ScBleeding;
    internal static ConfigEntry<float> ScFallDamage;

    // Dead Air, security timers. Tier 3 is an ABSOLUTE int, see RecomputeAll.
    internal static ConfigEntry<float> DaBreakerReset;
    internal static ConfigEntry<float> DaSecurityReset;
    internal static ConfigEntry<int> DaTamperGrace;

    // Long Arm, reach and handling
    internal static ConfigEntry<float> LaInteractionRange;
    internal static ConfigEntry<float> LaCarryDistance;
    internal static ConfigEntry<float> LaThrowForce;

    // Second Wind, recovery. Tier 1 SHARES playerRecoveryRate with Iron Lung t2.
    internal static ConfigEntry<float> SwRecovery;
    internal static ConfigEntry<float> SwEnergy;
    internal static ConfigEntry<float> SwBruised;

    // Captured baselines. Null until GameplayControls is first reachable.
    internal static float? BaseRunSpeed = null;
    internal static float? BaseJumpHeight = null;
    internal static float? BaseFallDamage = null;
    internal static float? BaseTiredness = null;
    internal static float? BaseRecovery = null;
    internal static float? BaseHunger = null;
    internal static float? BaseThirst = null;
    internal static float? BaseBleeding = null;
    internal static float? BaseBreakerReset = null;
    internal static float? BaseSecurityReset = null;
    internal static int? BaseTamperGrace = null;
    internal static float? BaseInteractionRange = null;
    internal static float? BaseCarryDistance = null;
    internal static float? BaseThrowForce = null;
    internal static float? BaseEnergyRate = null;
    internal static float? BaseBruised = null;

    public override void Load()
    {
        L = Log;
        L.LogInfo("=== SoDDiskPack " + VER + " starting ===");
        Cfg = OpenSettings("sm3gm.sod.syncdiskpack.cfg");

        DisableList = Cfg.Bind("_Experimental", "DisableVanillaDisks", "",
            "TEST ONLY. Comma-separated vanilla preset names to set disabled=true. "
          + "Leave empty to change nothing.");

        RemoveList = Cfg.Bind("_Experimental", "RemoveVanillaDisks", "",
            "TEST ONLY. Comma-separated vanilla preset names to REMOVE from "
          + "Toolbox.allSyncDisks and from every MenuPreset stock list. "
          + "Suppresses VENDOR stock only; world loot in an existing city is "
          + "unaffected. Throwaway saves only. Leave empty.");

        QsTier1 = Cfg.Bind("Quickstep", "Tier1RunSpeedFraction", 0.05f,
            "Quickstep tier 1. Fraction added to base playerRunSpeed. Cumulative.");
        QsTier2 = Cfg.Bind("Quickstep", "Tier2RunSpeedFraction", 0.05f,
            "Quickstep tier 2. Fraction added to base playerRunSpeed. Cumulative.");
        QsTier3 = Cfg.Bind("Quickstep", "Tier3RunSpeedFraction", 0.10f,
            "Quickstep tier 3. Fraction added to base playerRunSpeed. Cumulative.");

        LeapJump0 = Cfg.Bind("Leap", "JumpHeightInstalled", 6.0f,
            "GameplayControls.jumpHeight while Leap is installed, unupgraded. "
          + "Game baseline is 4.5. Absolute value, not a fraction.");
        LeapJump1 = Cfg.Bind("Leap", "JumpHeightTier1", 7.0f, "jumpHeight at tier 1. Absolute.");
        LeapJump2 = Cfg.Bind("Leap", "JumpHeightTier2", 8.0f, "jumpHeight at tier 2. Absolute.");
        LeapJump3 = Cfg.Bind("Leap", "JumpHeightTier3", 9.0f,
            "jumpHeight at tier 3. Absolute. 9.0 was play-tested as a good ceiling.");

        LeapFall0 = Cfg.Bind("Leap", "FallDamageInstalled", 1.0f,
            "fallDamageMultiplier while Leap is installed. Game baseline is 0.85. "
          + "This is Leap's cost. The cure is the Stability split disk, not a tier.");
        LeapFall1 = Cfg.Bind("Leap", "FallDamageTier1", 1.1f, "fallDamageMultiplier at tier 1.");
        LeapFall2 = Cfg.Bind("Leap", "FallDamageTier2", 1.2f, "fallDamageMultiplier at tier 2.");
        LeapFall3 = Cfg.Bind("Leap", "FallDamageTier3", 1.3f, "fallDamageMultiplier at tier 3.");

        IlTiredness = Cfg.Bind("IronLung", "Tier1TirednessFraction", -0.30f,
            "Iron Lung tier 1. Fraction applied to base playerTirednessRate (0.06). "
          + "Negative tires you more slowly. Deliberately ASYMMETRIC against "
          + "Sprinter's Curse tier 1 (+0.25): if the two were equal and opposite "
          + "a player running both disks would see an upgrade do literally nothing.");
        IlRecovery = Cfg.Bind("IronLung", "Tier2RecoveryFraction", 0.25f,
            "Iron Lung tier 2. Fraction applied to base playerRecoveryRate (0.225). "
          + "Positive heals faster. SUMMED with Second Wind tier 1 if both installed.");
        IlHunger = Cfg.Bind("IronLung", "Tier3HungerFraction", -0.20f,
            "Iron Lung tier 3. Fraction applied to base playerHungerRate (0.125).");
        IlThirst = Cfg.Bind("IronLung", "Tier3ThirstFraction", -0.20f,
            "Iron Lung tier 3. Fraction applied to base playerThirstRate (0.15).");
        IlGaitCure = Cfg.Bind("IronLung", "Tier3GaitCureFraction", 0.25f,
            "Iron Lung tier 3 compensates for the Heavy Gait side effect. The enum "
          + "gives maxSpeedModifier -0.2 (x0.8); this writes playerRunSpeed +25% on "
          + "top. The two compose multiplicatively, so in ISOLATION 0.8 x 1.25 "
          + "returns you to normal exactly. NOTE: run-speed fractions from all "
          + "disks are summed and applied once to the baseline, so the cure is "
          + "DILUTED when Quickstep or Sprinter's Curse are also installed - "
          + "roughly 5% short with both at tier 3. This is expected behaviour.");

        ScSpeedPerTier = Cfg.Bind("SprintersCurse", "SpeedFractionPerTier", 0.08f,
            "Sprinter's Curse. Fraction added to base playerRunSpeed per tier. Cumulative.");
        ScTiredness = Cfg.Bind("SprintersCurse", "Tier1TirednessFraction", 0.25f,
            "Sprinter's Curse tier 1 cost. Fraction applied to base playerTirednessRate. "
          + "Positive tires you faster. See IronLung Tier1TirednessFraction for why "
          + "these two are not equal and opposite.");
        ScBleeding = Cfg.Bind("SprintersCurse", "Tier2BleedingFraction", 0.50f,
            "Sprinter's Curse tier 2 cost. Fraction applied to base "
          + "combatHitChanceOfBleeding (0.1). combatHitChanceOfBrokenLeg was "
          + "rejected for this: at a 0.01 base it is imperceptible even tripled.");
        ScFallDamage = Cfg.Bind("SprintersCurse", "Tier3FallDamageFraction", 0.30f,
            "Sprinter's Curse tier 3 cost. Fraction applied to whatever "
          + "fallDamageMultiplier already is, so it stacks on top of Leap.");

                DaBreakerReset = Cfg.Bind("DeadAir", "Tier1BreakerResetFraction", 0.50f,
            "Dead Air tier 1. Fraction applied to base breakerResetTime (0.5). "
          + "POSITIVE means tripped breakers stay down LONGER, which is the "
          + "stealth benefit. The sign was inverted before 0.10.0 and shortened "
          + "the outage instead.");
        DaSecurityReset = Cfg.Bind("DeadAir", "Tier2SecurityResetFraction", 0.50f,
            "Dead Air tier 2. Fraction applied to base securityResetTime (3). "
          + "POSITIVE means disabled security takes LONGER to come back online. "
          + "Same sign inversion as tier 1, fixed in 0.10.0.");
        DaTamperGrace = Cfg.Bind("DeadAir", "Tier3TamperGraceAbsolute", 5,
            "UNUSED as of 0.10.0. Dead Air ships as a two-tier chain. tamperGrace "
          + "is unobservable by construction: the tampering action completes in "
          + "under 3 seconds, so the grace window never expires at any value. "
          + "The key is retained so existing configs are not disturbed.");

                LaInteractionRange = Cfg.Bind("LongArm", "Tier1InteractionRangeFraction", 0.20f,
            "Long Arm tier 1. Fraction applied to base interactionRange (1.585). "
          + "This is interaction reach, and it stacks with the disk's own "
          + "reachModifier main effect and with Tenacity/Brawn tier 1. "
          + "Confirmed read live and observed in play.");
        LaCarryDistance = Cfg.Bind("LongArm", "Tier2CarryDistanceFraction", 0.40f,
            "UNUSED as of 0.10.0. carryDistance is owned by PlacementPlus while "
          + "that mod is enabled, so writes to it do nothing. The key is retained "
          + "so existing configs are not disturbed.");
        LaThrowForce = Cfg.Bind("LongArm", "Tier3ThrowForceFraction", 0.40f,
            "Long Arm tier 2 as of 0.10.0, despite the key name, which is kept to "
          + "avoid orphaning the value on existing installs. Fraction applied to "
          + "base throwForce (6.4). Raised from 0.25 because 0.25 was too weak to "
          + "feel in play.");

        SwRecovery = Cfg.Bind("SecondWind", "Tier1RecoveryFraction", 0.20f,
            "Second Wind tier 1. Fraction applied to base playerRecoveryRate (0.225). "
          + "SUMMED with Iron Lung tier 2, which writes the same field.");
        SwEnergy = Cfg.Bind("SecondWind", "Tier2EnergyFraction", -0.20f,
            "Second Wind tier 2. Fraction applied to base playerEnergyRate (0.116). "
          + "UNTESTED IN PLAY: this field has never been confirmed to be read live "
          + "rather than consumed once at startup. It will log correctly either way.");
        SwBruised = Cfg.Bind("SecondWind", "Tier3BruisedFraction", -0.40f,
            "Second Wind tier 3. Fraction applied to base combatHitChanceOfBruised "
          + "(0.1). Negative means you bruise less often. A small effect by design; "
          + "no vanilla disk claims this field.");

        int registered = 0, fakeTiers = 0;
        foreach (var d in Disks.All)
        {
            bool enabled = Cfg.Bind(d.Key, "Enabled", d.Enabled, "Register this disk.").Value;
            if (!enabled) { L.LogInfo("skipped: " + d.DiskName); continue; }

            int price = Cfg.Bind(d.Key, "Price", d.Price, "Purchase price.").Value;
            float value = Cfg.Bind(d.Key, "Value", d.Value,
                "Base effect value. Fractions are percentages (0.1 = 10%). "
              + "Ignored on disks whose effect is applied in code.").Value;

            bool hasTiers = d.TierDescs != null && d.TierDescs.Length > 0;
            if (hasTiers && !d.TiersInCode)
            {
                fakeTiers++;
                L.LogWarning("*** " + d.DiskName + " has upgrade options but no code in "
                           + "RecomputeAll. Those upgrades will do NOTHING. ***");
            }

            try { Register(d, price, value); registered++; }
            catch (System.Exception e) { L.LogError("FAILED " + d.DiskName + ": " + e); }
        }

        Harmony.CreateAndPatchAll(typeof(DisablePatch));
        Harmony.CreateAndPatchAll(typeof(TierPatch));

        L.LogInfo("=== SoDDiskPack done: " + registered + " registered, "
                + fakeTiers + " with unimplemented tiers ===");
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

    static void Register(DiskDef d, int price, float value)
    {
        var builder = Lib.SyncDisks.Builder(d.DiskName, GUID)
            .AddEffect(d.EffectName, d.EffectDesc, out int effId);

        if (d.SideEffect != E.none)
            builder.AddSideEffect(d.SideName, d.SideDesc, out _);

        if (d.SaleLocations != null && d.SaleLocations.Length > 0)
            builder.AddSaleLocation(d.SaleLocations);

        bool hasTiers = d.TierDescs != null && d.TierDescs.Length > 0;
        if (hasTiers)
        {
            SyncDiskBuilder.Options opts;
            if (d.TierDescs.Length == 1)
                opts = new SyncDiskBuilder.Options(d.TierDescs[0]);
            else if (d.TierDescs.Length == 2)
                opts = new SyncDiskBuilder.Options(d.TierDescs[0], d.TierDescs[1]);
            else
                opts = new SyncDiskBuilder.Options(d.TierDescs[0], d.TierDescs[1], d.TierDescs[2]);
            builder.AddUpgradeOption(opts, out _);
        }

        var disk = builder.CreateAndRegister();
        var p = disk.Preset;

        bool codeDriven = (d.Effect == E.none);
        if (!codeDriven)
        {
            p.mainEffect1 = d.Effect;
            p.mainEffect1Value = value;
        }

        p.mainEffect2 = E.none;
        p.mainEffect2Value = 0f;
        p.mainEffect3 = E.none;
        p.mainEffect3Value = 0f;

        p.price = price;
        p.rarity = d.Rarity;
        p.manufacturer = d.Manufacturer;
        p.canBeSideJobReward = true;
        p.minimumWealthLevel = 0f;

        if (d.SideEffect != E.none)
        {
            p.sideEffect = d.SideEffect;
            p.sideEffectValue = d.SideValue;
        }

        // ROUTE A: option1UpgradeEffects is deliberately NOT overwritten.
        // SOD.Common stores its own allocated option IDs there and uses them to
        // recognise its disks. Clobbering them broke every pack tier to v0.5.0.

        int slots = 0;
        try { slots = p.option1UpgradeEffects.Count; } catch { }

        L.LogInfo("registered " + d.DiskName + " | effect=" + p.mainEffect1
                + " value=" + p.mainEffect1Value + " mfr=" + p.manufacturer
                + " side=" + p.sideEffect + " sideVal=" + p.sideEffectValue
                + " price=" + p.price + " optionSlots=" + slots
                + " customId=" + effId
                + (codeDriven ? " CODE-DRIVEN" : "")
                + (d.TiersInCode ? " TIERS-IN-CODE" : ""));
    }

    static int LevelOf(string suffix)
    {
        try
        {
            var uc = UpgradesController.Instance;
            if (uc == null || uc.upgrades == null) return -1;
            foreach (var u in uc.upgrades)
            {
                if (u == null || u.preset == null) continue;
                var n = u.preset.name;
                if (n != null && n.EndsWith(suffix)) return u.level;
            }
        }
        catch { }
        return -1;
    }

    // Recompute-from-scratch. Called on any disk change. Never accumulates.
    //
    // COMPOSITION RULE, one per field, no exceptions:
    //   playerRunSpeed     - every disk contributes a FRACTION, summed, applied
    //                        once to the baseline. Quickstep, Sprinter's Curse and
    //                        Iron Lung's gait cure all write here.
    //   playerRecoveryRate - same summed-fraction rule. Iron Lung t2 and
    //                        Second Wind t1 both write here.
    //   playerTirednessRate- same summed-fraction rule, in both directions.
    //   jumpHeight         - Leap only, ABSOLUTE.
    //   tamperGrace        - Dead Air only, ABSOLUTE, and an INT.
    //   fallDamage         - Leap sets an ABSOLUTE target, then Sprinter's Curse
    //                        applies a fraction ON TOP of that result.
    //   everything else    - single owner, fractional against baseline.
    //
    // Mixing absolute and fractional writes on one field is the failure mode
    // where two disks silently cancel. Do not add a second absolute writer.
    internal static void RecomputeAll()
    {
        try
        {
            var gc = GameplayControls.Instance;
            if (gc == null) return;

            if (BaseRunSpeed == null)
            {
                BaseRunSpeed = gc.playerRunSpeed;
                BaseJumpHeight = gc.jumpHeight;
                BaseFallDamage = gc.fallDamageMultiplier;
                BaseTiredness = gc.playerTirednessRate;
                BaseRecovery = gc.playerRecoveryRate;
                BaseHunger = gc.playerHungerRate;
                BaseThirst = gc.playerThirstRate;
                BaseBleeding = gc.combatHitChanceOfBleeding;
                BaseBreakerReset = gc.breakerResetTime;
                BaseSecurityReset = gc.securityResetTime;
                BaseTamperGrace = gc.tamperGrace;
                BaseInteractionRange = gc.interactionRange;
                BaseCarryDistance = gc.carryDistance;
                BaseThrowForce = gc.throwForce;
                BaseEnergyRate = gc.playerEnergyRate;
                BaseBruised = gc.combatHitChanceOfBruised;

                L.LogInfo("[TIER] captured baselines"
                        + " runSpeed=" + BaseRunSpeed
                        + " jump=" + BaseJumpHeight
                        + " fall=" + BaseFallDamage
                        + " tired=" + BaseTiredness
                        + " recovery=" + BaseRecovery
                        + " hunger=" + BaseHunger
                        + " thirst=" + BaseThirst
                        + " bleeding=" + BaseBleeding
                        + " breakerReset=" + BaseBreakerReset
                        + " securityReset=" + BaseSecurityReset
                        + " tamperGrace=" + BaseTamperGrace
                        + " interactionRange=" + BaseInteractionRange
                        + " carryDistance=" + BaseCarryDistance
                        + " throwForce=" + BaseThrowForce
                        + " energy=" + BaseEnergyRate
                        + " bruised=" + BaseBruised);
            }

            int qs = LevelOf("_Quickstep");
            int leap = LevelOf("_Leap");
            int il = LevelOf("_Iron Lung");
            int sc = LevelOf("_Sprinter's Curse");
            int da = LevelOf("_Dead Air");
            int la = LevelOf("_Long Arm");
            int sw = LevelOf("_Second Wind");

            // ---- playerRunSpeed: sum of every fractional contribution ----
            float speedFraction = 0f;
            if (qs >= 1) speedFraction += QsTier1.Value;
            if (qs >= 2) speedFraction += QsTier2.Value;
            if (qs >= 3) speedFraction += QsTier3.Value;

            if (sc >= 1) speedFraction += ScSpeedPerTier.Value;
            if (sc >= 2) speedFraction += ScSpeedPerTier.Value;
            if (sc >= 3) speedFraction += ScSpeedPerTier.Value;

            // Iron Lung tier 3 compensates for its own Heavy Gait side effect.
            if (il >= 3) speedFraction += IlGaitCure.Value;

            gc.playerRunSpeed = BaseRunSpeed.Value * (1f + speedFraction);

            // ---- jumpHeight and fallDamageMultiplier ----
            float jump = BaseJumpHeight.Value;
            float fall = BaseFallDamage.Value;

            if (leap == 0) { jump = LeapJump0.Value; fall = LeapFall0.Value; }
            else if (leap == 1) { jump = LeapJump1.Value; fall = LeapFall1.Value; }
            else if (leap == 2) { jump = LeapJump2.Value; fall = LeapFall2.Value; }
            else if (leap >= 3) { jump = LeapJump3.Value; fall = LeapFall3.Value; }

            // Sprinter's Curse tier 3 stacks on whatever Leap decided.
            if (sc >= 3) fall = fall * (1f + ScFallDamage.Value);

            gc.jumpHeight = jump;
            gc.fallDamageMultiplier = fall;

            // ---- playerTirednessRate: Iron Lung lowers, Sprinter's raises ----
            float tiredFraction = 0f;
            if (il >= 1) tiredFraction += IlTiredness.Value;
            if (sc >= 1) tiredFraction += ScTiredness.Value;
            gc.playerTirednessRate = BaseTiredness.Value * (1f + tiredFraction);

            // ---- playerRecoveryRate: Iron Lung t2 and Second Wind t1 ----
            float recoveryFraction = 0f;
            if (il >= 2) recoveryFraction += IlRecovery.Value;
            if (sw >= 1) recoveryFraction += SwRecovery.Value;
            gc.playerRecoveryRate = BaseRecovery.Value * (1f + recoveryFraction);

            // ---- Iron Lung only ----
            gc.playerHungerRate = BaseHunger.Value * (1f + (il >= 3 ? IlHunger.Value : 0f));
            gc.playerThirstRate = BaseThirst.Value * (1f + (il >= 3 ? IlThirst.Value : 0f));

            // ---- Sprinter's Curse only ----
            gc.combatHitChanceOfBleeding =
                BaseBleeding.Value * (1f + (sc >= 2 ? ScBleeding.Value : 0f));

                        // ---- Dead Air. TWO tiers as of 0.10.0. Positive fractions LENGTHEN. ----
            gc.breakerResetTime =
                BaseBreakerReset.Value * (1f + (da >= 1 ? DaBreakerReset.Value : 0f));
            gc.securityResetTime =
                BaseSecurityReset.Value * (1f + (da >= 2 ? DaSecurityReset.Value : 0f));
            // tamperGrace is CUT: unobservable by construction. Always baseline.
            gc.tamperGrace = BaseTamperGrace.Value;

            // ---- Long Arm. TWO tiers as of 0.10.0. ----
            gc.interactionRange =
                BaseInteractionRange.Value * (1f + (la >= 1 ? LaInteractionRange.Value : 0f));
            // carryDistance is CUT: PlacementPlus owns it. Always baseline.
            gc.carryDistance = BaseCarryDistance.Value;
            gc.throwForce =
                BaseThrowForce.Value * (1f + (la >= 2 ? LaThrowForce.Value : 0f));

            // ---- Second Wind tiers 2 and 3 ----
            gc.playerEnergyRate =
                BaseEnergyRate.Value * (1f + (sw >= 2 ? SwEnergy.Value : 0f));
            gc.combatHitChanceOfBruised =
                BaseBruised.Value * (1f + (sw >= 3 ? SwBruised.Value : 0f));

            L.LogInfo("[TIER] qs=" + qs + " leap=" + leap + " il=" + il + " sc=" + sc
                    + " da=" + da + " la=" + la + " sw=" + sw
                    + " | runSpeed=" + gc.playerRunSpeed
                    + " jump=" + gc.jumpHeight
                    + " fall=" + gc.fallDamageMultiplier
                    + " tired=" + gc.playerTirednessRate
                    + " recovery=" + gc.playerRecoveryRate
                    + " hunger=" + gc.playerHungerRate
                    + " thirst=" + gc.playerThirstRate
                    + " bleeding=" + gc.combatHitChanceOfBleeding
                    + " breakerReset=" + gc.breakerResetTime
                    + " securityReset=" + gc.securityResetTime
                    + " tamperGrace=" + gc.tamperGrace
                    + " interactionRange=" + gc.interactionRange
                    + " carryDistance=" + gc.carryDistance
                    + " throwForce=" + gc.throwForce
                    + " energy=" + gc.playerEnergyRate
                    + " bruised=" + gc.combatHitChanceOfBruised);
        }
        catch (System.Exception e)
        {
            L.LogError("[TIER] RecomputeAll failed: " + e);
        }
    }
}

// Calls RecomputeAll on install, uninstall AND every upgrade. This single
// hook is the whole of Route A's delivery mechanism.
[HarmonyPatch(typeof(UpgradeEffectController), "OnSyncDiskChange")]
public static class TierPatch
{
    [HarmonyPostfix]
    static void Postfix()
    {
        Plugin.RecomputeAll();
    }
}

// _Experimental only. Both lists default empty and change nothing.
// disabled=true does NOT remove a disk from clinic stock; only removal from
// Toolbox.allSyncDisks does. Kept for testing, not for release use.
[HarmonyPatch(typeof(Toolbox), nameof(Toolbox.Start))]
public static class DisablePatch
{
    static bool done;

    static List<string> ParseNames(string raw)
    {
        var names = new List<string>();
        if (string.IsNullOrWhiteSpace(raw)) return names;

        foreach (var part in raw.Split(','))
        {
            var trimmed = part.Trim();
            if (trimmed.Length > 0) names.Add(trimmed);
        }
        return names;
    }

    [HarmonyPostfix]
    static void Postfix()
    {
        if (done) return;
        done = true;
        DoDisable();
        DoRemove();
    }

    static void DoDisable()
    {
        var names = ParseNames(Plugin.DisableList.Value);
        if (names.Count == 0)
        {
            Plugin.L.LogInfo("=== DisableVanillaDisks empty, nothing flagged ===");
            return;
        }

        int matched = 0;
        foreach (var preset in Toolbox.Instance.allSyncDisks)
        {
            foreach (var wanted in names)
            {
                if (preset.name == wanted)
                {
                    preset.disabled = true;
                    matched++;
                    Plugin.L.LogInfo("=== DISABLED vanilla preset: " + preset.name + " ===");
                }
            }
        }

        Plugin.L.LogInfo("=== disable pass complete, " + matched
                       + " of " + names.Count + " names matched ===");
    }

    static void DoRemove()
    {
        var names = ParseNames(Plugin.RemoveList.Value);
        if (names.Count == 0)
        {
            Plugin.L.LogInfo("=== RemoveVanillaDisks empty, nothing removed ===");
            return;
        }

        int removedFromToolbox = 0;
        var toolbox = Toolbox.Instance;

        if (toolbox != null && toolbox.allSyncDisks != null)
        {
            for (int i = toolbox.allSyncDisks.Count - 1; i >= 0; i--)
            {
                var preset = toolbox.allSyncDisks[i];
                if (preset == null) continue;

                foreach (var wanted in names)
                {
                    if (preset.name == wanted)
                    {
                        toolbox.allSyncDisks.RemoveAt(i);
                        removedFromToolbox++;
                        Plugin.L.LogInfo("=== REMOVED from Toolbox.allSyncDisks: "
                                       + wanted + " ===");
                        break;
                    }
                }
            }
        }
        else
        {
            Plugin.L.LogWarning("=== Toolbox.allSyncDisks null, toolbox removal skipped ===");
        }

        int removedFromMenus = 0;
        int menusScanned = 0;
        var menus = Resources.FindObjectsOfTypeAll<MenuPreset>();

        if (menus != null)
        {
            foreach (var menu in menus)
            {
                if (menu == null || menu.syncDisks == null) continue;
                menusScanned++;

                for (int i = menu.syncDisks.Count - 1; i >= 0; i--)
                {
                    var preset = menu.syncDisks[i];
                    if (preset == null) continue;

                    foreach (var wanted in names)
                    {
                        if (preset.name == wanted)
                        {
                            menu.syncDisks.RemoveAt(i);
                            removedFromMenus++;
                            break;
                        }
                    }
                }
            }
        }

        Plugin.L.LogInfo("=== remove pass complete: " + removedFromToolbox
                       + " from Toolbox, " + removedFromMenus
                       + " across " + menusScanned + " menu presets ===");
    }
}