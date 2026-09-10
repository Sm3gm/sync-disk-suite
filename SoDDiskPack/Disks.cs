using E = SyncDiskPreset.Effect;
using U = SyncDiskPreset.UpgradeEffect;
using R = SyncDiskPreset.Rarity;
using M = SyncDiskPreset.Manufacturer;

namespace SoDDiskPack;

public class DiskDef
{
    public string Key;
    public string DiskName;
    public string EffectName;
    public string EffectDesc;
    public E Effect;
    public float Value;
    public int Price;
    public R Rarity;
    public M Manufacturer;
    public string[] SaleLocations;
    public string[] TierDescs;

    // NOTE: TierEffects and TierValues are DEAD DATA under Route A. Register()
    // never reads them, because option1UpgradeEffects belongs to SOD.Common and
    // clobbering it broke every tier through v0.5.0. They are kept only so the
    // original vanilla-style intent stays readable. Tier behaviour lives in
    // Plugin.RecomputeAll. A disk with TierDescs but no code in RecomputeAll
    // has upgrade text that does NOTHING.
    public U[] TierEffects;
    public float[] TierValues;

    public E SideEffect;
    public float SideValue;
    public string SideName;
    public string SideDesc;

    // Default registration state. False for disks cut from the release.
    public bool Enabled = true;

    // True when RecomputeAll implements this disk's tiers. Purely for logging,
    // so a disk with fake tiers is visible at startup instead of in a bug report.
    public bool TiersInCode = false;
}

public static class Disks
{
    // Descriptions contain NO numbers on purpose: they are save-match keys.
    public static readonly DiskDef[] All = new DiskDef[]
    {
        new DiskDef {
            Key="Quickstep", DiskName="Quickstep",
            EffectName="Fleet Foot", EffectDesc="You move faster on foot.",
            Effect=E.maxSpeedModifier, Value=0.15f,
            Price=2500, Rarity=R.rare, Manufacturer=M.ElGen,
            SaleLocations=new[]{"SyncClinic"},
            TierDescs=new[]{"You move faster still.","Your pace improves further.","You move considerably faster."},
            TiersInCode=true },

        // ---------- SOCIAL CREDIT FAMILY: inert, cut from release ----------
        // These map to flags the Social Credit system owns and sets from SC
        // score. They are never read from a disk. Free Rein was tested inert
        // in game. Kept in the table so the config keys survive and so the
        // designs are not lost, but they do not register by default.
        new DiskDef {
            Key="LowProfile", DiskName="Low Profile", Enabled=false,
            EffectName="Unremarkable", EffectDesc="Citizens are less easily alarmed by you.",
            Effect=E.spookedMultiplier, Value=-0.15f,
            Price=1000, Rarity=R.rare, Manufacturer=M.BlackMarket,
            SaleLocations=new[]{"BlackmarketSyncClinic"},
            TierDescs=new[]{"Citizens are calmer still.","You draw less attention again.","You are markedly harder to alarm."} },

        new DiskDef {
            Key="SquattersRights", DiskName="Squatter's Rights", Enabled=false,
            EffectName="Belong Here", EffectDesc="You are given longer before trespassing is reported.",
            Effect=E.trespassGraceModifier, Value=0.5f,
            Price=1000, Rarity=R.rare, Manufacturer=M.BlackMarket,
            SaleLocations=new[]{"BlackmarketSyncClinic"},
            TierDescs=new[]{"You are given longer still.","Further grace before you are reported.","A considerable delay before you are reported."} },

        new DiskDef {
            Key="Homebound", DiskName="Homebound", Enabled=false,
            EffectName="Homebound", EffectDesc="You may travel directly to your apartment.",
            Effect=E.fastTravelToApartment, Value=1f,
            Price=1500, Rarity=R.veryRare, Manufacturer=M.KensingtonIndigo,
            SaleLocations=new[]{"SyncClinic"} },

        new DiskDef {
            Key="Signpost", DiskName="Signpost", Enabled=false,
            EffectName="Signpost", EffectDesc="You may travel using street signage.",
            Effect=E.fastTravelUsingSignage, Value=1f,
            Price=1500, Rarity=R.veryRare, Manufacturer=M.KensingtonIndigo,
            SaleLocations=new[]{"SyncClinic"} },

        new DiskDef {
            Key="PersonaNonGrata", DiskName="Persona Non Grata", Enabled=false,
            EffectName="Persona Non Grata", EffectDesc="Loitering is no longer held against you.",
            Effect=E.disableLoitering, Value=1f,
            Price=1000, Rarity=R.rare, Manufacturer=M.BlackMarket,
            SaleLocations=new[]{"BlackmarketSyncClinic"} },

        new DiskDef {
            Key="FreeRein", DiskName="Free Rein", Enabled=false,
            EffectName="Free Rein", EffectDesc="Your presence at crime scenes is permitted.",
            Effect=E.allowedAtCrimeScenes, Value=1f,
            Price=1500, Rarity=R.veryRare, Manufacturer=M.KensingtonIndigo,
            SaleLocations=new[]{"SyncClinic"} },

        // ---------- TIERLESS BY DESIGN ----------
        new DiskDef {
            Key="DeepPockets", DiskName="Deep Pockets",
            EffectName="Deep Pockets", EffectDesc="Increase your inventory capacity.",
            Effect=E.increaseInventory, Value=1f,
            Price=3000, Rarity=R.veryRare, Manufacturer=M.BlackMarket,
            SaleLocations=new[]{"BlackmarketSyncClinic"},
            TierDescs=null },

        // ---------- REWORKED: real tiers in RecomputeAll ----------
        // Speed at an escalating physical price. The brittleness is PERMANENT:
        // incomingDamageModifier has no GameplayControls counterpart, so it
        // cannot be cured by compensation the way Iron Lung's is.
        new DiskDef {
            Key="SprintersCurse", DiskName="Sprinter's Curse",
            EffectName="Fast Twitch", EffectDesc="You move considerably faster on foot.",
            Effect=E.maxSpeedModifier, Value=0.2f,
            SideEffect=E.incomingDamageModifier, SideValue=0.25f,
            SideName="Brittle", SideDesc="Your reworked frame takes damage badly. This cannot be removed.",
            Price=2500, Rarity=R.rare, Manufacturer=M.ElGen,
            SaleLocations=new[]{"SyncClinic"},
            TierDescs=new[]{
                "You run faster, and tire sooner for it.",
                "You run faster again. Your skin splits more easily.",
                "You run faster still, and the ground meets you harder."},
            TiersInCode=true },

        // Endurance. Tier 3 compensates for the Heavy Gait side effect: the
        // enum gives maxSpeedModifier -0.2 (x0.8) and RecomputeAll writes
        // playerRunSpeed +25% on top. Enum effects and GameplayControls writes
        // compose multiplicatively, so in ISOLATION 0.8 x 1.25 lands on 1.0.
        // Run-speed fractions are summed across disks, so the compensation is
        // diluted when Quickstep or Sprinter's Curse are also installed.
        new DiskDef {
            Key="IronLung", DiskName="Iron Lung",
            EffectName="Dense Tissue", EffectDesc="You take considerably less damage.",
            Effect=E.incomingDamageModifier, Value=-0.25f,
            SideEffect=E.maxSpeedModifier, SideValue=-0.2f,
            SideName="Heavy Gait", SideDesc="Your added mass slows you down.",
            Price=2500, Rarity=R.rare, Manufacturer=M.ElGen,
            SaleLocations=new[]{"SyncClinic"},
            TierDescs=new[]{
                "You tire far more slowly.",
                "Your body knits itself back together faster.",
                "Your gait is corrected."},
            TiersInCode=true },

        // ---------- LEAP: custom effect, no enum member ----------
        new DiskDef {
            Key="Leap", DiskName="Leap",
            EffectName="Leap", EffectDesc="You jump considerably higher, but land harder.",
            Effect=E.none, Value=0f,
            Price=3500, Rarity=R.rare, Manufacturer=M.ElGen,
            SaleLocations=new[]{"SyncClinic"},
            TierDescs=new[]{"You jump higher still.","Your leap improves again.","You clear considerable heights."},
            TiersInCode=true },

        // ---------- REWORKED IN v0.9.0: tiers now implemented in code ----------
        // Security timers. Tier 3 writes tamperGrace, which is an INT and the
        // only absolute writer in the pack besides Leap. Dead Air is its sole
        // owner; do not add a second writer to that field.
        new DiskDef {
            Key="DeadAir", DiskName="Dead Air",
            EffectName="Slow Response", EffectDesc="Security systems take longer to react to you.",
            Effect=E.securityGraceTimeModifier, Value=0.5f,
            Price=1250, Rarity=R.medium, Manufacturer=M.BlackMarket,
            SaleLocations=new[]{"BlackmarketSyncClinic"},
                        TierDescs=new[]{
                "Tripped breakers stay down longer.",
                "Disabled security is slower to wake."},
            TiersInCode=true },

        new DiskDef {
            Key="PlusOne", DiskName="Plus One",
            EffectName="On The List", EffectDesc="You obtain guest passes more readily.",
            Effect=E.guestPassIssueModifier, Value=0.25f,
            Price=2000, Rarity=R.medium, Manufacturer=M.KensingtonIndigo,
            SaleLocations=new[]{"SyncClinic"},
            TierDescs=null },

        new DiskDef {
            Key="Muckraker", DiskName="Muckraker",
            EffectName="Expose", EffectDesc="Earn money photographing illegal operations.",
            Effect=E.illegalOpsPhotos, Value=40f,
            Price=500, Rarity=R.common, Manufacturer=M.CandorNews,
            SaleLocations=new[]{"SyncClinic"},
            TierDescs=null },

        new DiskDef {
            Key="BlankFace", DiskName="Blank Face", Enabled=false,
            EffectName="Unmemorable", EffectDesc="Citizens are far less easily alarmed by you.",
            Effect=E.spookedMultiplier, Value=-0.3f,
            SideEffect=E.dialogChanceModifier, SideValue=-0.3f,
            SideName="Forgettable", SideDesc="Citizens are less willing to talk to you.",
            Price=2000, Rarity=R.rare, Manufacturer=M.BlackMarket,
            SaleLocations=new[]{"BlackmarketSyncClinic"},
            TierDescs=null },

        new DiskDef {
            Key="CompanyMan", DiskName="Company Man",
            EffectName="Staff Discount", EffectDesc="Everything you buy costs considerably less.",
            Effect=E.priceModifier, Value=0.2f,
            SideEffect=E.dialogChanceModifier, SideValue=-0.2f,
            SideName="Corporate Stink", SideDesc="Citizens trust you less. This cannot be removed.",
            Price=1500, Rarity=R.medium, Manufacturer=M.KensingtonIndigo,
            SaleLocations=new[]{"SyncClinic"},
            TierDescs=null },

        // Recovery. Tier 1 SHARES playerRecoveryRate with Iron Lung tier 2 as a
        // summed fraction. Tier 2 writes playerEnergyRate, which has never been
        // confirmed to be read live rather than consumed once at startup.
        new DiskDef {
            Key="SecondWind", DiskName="Second Wind",
            EffectName="Second Wind", EffectDesc="You recover health more quickly.",
            Effect=E.increaseRegeneration, Value=0.1f,
            Price=2000, Rarity=R.rare, Manufacturer=M.ElGen,
            SaleLocations=new[]{"SyncClinic"},
            TierDescs=new[]{
                "Your wounds close faster still.",
                "Your reserves last longer.",
                "Your flesh bruises less readily."},
            TiersInCode=true },

        // Reach. Tier 1 stacks with this disk's own reachModifier main effect
        // and with Tenacity/Brawn tier 1. All three are scalar, which is
        // permitted; a boolean collision would not be.
        new DiskDef {
            Key="LongArm", DiskName="Long Arm",
            EffectName="Long Arm", EffectDesc="You can reach further.",
            Effect=E.reachModifier, Value=0.1f,
            Price=1000, Rarity=R.common, Manufacturer=M.ElGen,
            SaleLocations=new[]{"SyncClinic"},
                       TierDescs=new[]{
                "You can reach further still.",
                "You throw with considerably more force."},
            TiersInCode=true },

        new DiskDef {
            Key="LightsOut", DiskName="Lights Out",
            EffectName="Lights Out", EffectDesc="Citizens you knock out stay unconscious longer.",
            Effect=E.KOTimeModifier, Value=0.5f,
            Price=2000, Rarity=R.rare, Manufacturer=M.BlackMarket,
            SaleLocations=new[]{"BlackmarketSyncClinic"},
            TierDescs=null },
    };
}