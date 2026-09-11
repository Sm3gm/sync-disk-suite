# Sync Disk Suite

Three plugins for *Shadows of Doubt*, shipped together as one package.

| Plugin | What it does |
|---|---|
| **Sync Disk Pack** | Adds 12 new sync disks, seven of them with full upgrade chains. |
| **Vanilla Split** | Breaks 17 vanilla sync disks into 37 standalone single-purpose disks. |
| **Disk Availability** | Controls what vendors stock. Three modes: rotating daily stock, the vanilla lists as the game shipped them, or everything everywhere. |

They are separate plugins with separate config files, and **each one can be disabled without touching the others**. If you only want the split disks, disable the other two. Nothing in the package depends on the rest of it.

They ship together because they were designed together. The split mod triples the number of disks in the world, the availability mod is what stops that becoming noise, and the pack is built to sit alongside both without stepping on their effects.

---

## Requirements

- **SOD.Common** by Venomaus, required, install it first
- BepInEx (IL2CPP), which the mod manager handles for you

## Installing

Use [Thunderstore Mod Manager](https://thunderstore.io/c/shadows-of-doubt/) or r2modman and install as normal. Dependencies are pulled in automatically.

To install by hand, drop the three plugin folders into `BepInEx/plugins`.

---

## Read this before you start

**Sync disks are placed in the world when the city is generated.** Loading this package onto a city you have already been playing will get you the new disks in shop stock, but **you will never find them lying around in apartments, offices or lockups in that city.** For the full experience, start a new city.

Existing saves are otherwise safe. Nothing here breaks a city you are already in.

If the suite earns a place in your load order, please leave a like on its [Thunderstore page](https://thunderstore.io/c/shadows-of-doubt/p/Sm3gm/SyncDiskSuite/). It keeps me motivated to make more mods. Donations are welcome too:

[![Support me on Ko-fi](https://raw.githubusercontent.com/sm3gm/sync-disk-suite/main/images/kofi.png)](https://ko-fi.com/sm3gm) [![Buy me a coffee](https://raw.githubusercontent.com/sm3gm/sync-disk-suite/main/images/bmc.png)](https://www.buymeacoffee.com/sm3gm)

---

## Sync Disk Pack

Twelve new disks. Seven carry upgrade chains that you buy with vials at a Sync Clinic. The other five are deliberately single-purpose and have no upgrades.

Following vanilla's own design, **each tier changes what a disk does rather than simply doing more of it.**

### The disks

| Disk | Price | Tiers | What it does |
|---|---|---|---|
| **Leap** | 3500 | 3 | Jump substantially higher. You also land harder, as fall damage rises with every tier. The only disk in the pack that changes how you move through the city rather than how fast. |
| **Deep Pockets** | 3000 | none | One more inventory slot, and the practical route to a full twelve-slot grid. Vanilla's own disks top out at eleven unless you go out of your way to stack a combined disk on top of the split version of itself. |
| **Quickstep** | 2500 | 3 | Run faster. Each tier adds more. No downside. |
| **Sprinter's Curse** | 2500 | 3 | Run considerably faster, permanently take more damage. Every tier buys more speed at a rising price: exhaustion, then bleeding, then heavier landings. |
| **Iron Lung** | 2500 | 3 | Take less damage, at the cost of a heavy gait that slows you down. Tiers reduce tiredness, then speed recovery, then cut hunger and thirst while compensating for the gait. |
| **Second Wind** | 2000 | 3 | Recover faster, tire more slowly, and bruise less easily. Slow and cumulative rather than dramatic. |
| **Lights Out** | 2000 | none | Knocked-out citizens stay down considerably longer. |
| **Plus One** | 2000 | none | Issue guest passes far more freely. |
| **Company Man** | 1500 | none | Everything costs less, everywhere. |
| **Dead Air** | 1250 | 2 | Security systems and breakers stay down longer after you cut them. Tier 1 extends breaker outages, tier 2 extends security resets. |
| **Long Arm** | 1000 | 2 | Reach further to interact with things, and throw harder. |
| **Muckraker** | 500 | none | Photograph illegal operations and get paid for it. Cheap, pays for itself quickly, and the natural first disk of the pack. |

Prices, values and rarities are all config-adjustable, and **none of them are baked into your save**, so you can retune the whole ladder mid-playthrough.

### On pricing

The pack runs from 500 to 3500 credits, where vanilla spans 500 to 1000. That wider ladder is deliberate. There should be real distance between the disk you pick up in week one and the disk you save for.

How much that actually constrains you depends enormously on how you play. A confident player who knows the earning loops will out-pace the ladder without much trouble. This package does not rebalance the game's economy, and does not pretend to.

---

## Vanilla Split

Most vanilla sync disks make you choose. Tenacity offers Clout, Brawn or Reflexes, and once you install it, the other two branches are gone for good on that disk.

This plugin turns each of those branches into its own disk. **Take Brawn without giving up Reflexes.** Buy Stability without giving up Power. The upgrade chains, effects and values all come from the vanilla disk itself, so a split disk behaves exactly as its branch always did. It simply no longer costs you the branches beside it.

Seventeen vanilla disks split into **37 standalone disks**. `Starch-SugarDaddy` is untouched because it only has one branch.

By default the original combined disks stay in the game alongside the splits. **Set `RemoveVanillaParents` to `true`** in the config to take them out of shop stock, which is the intended way to play. Otherwise clinics carry both the split and the combined version of everything.

### The 37 disks

| Vanilla disk | Splits into | Price |
|---|---|---|
| **ElGen-Tenacity** | Clout | 1500 |
| | Brawn | 1250 |
| | Reflexes | 2500 |
| **ElGen-Vigor** | Power | 2500 |
| | Stability | 3000 |
| **ElGen-Physique** | Fortitude | 1750 |
| | Vitality | 1500 |
| **ElGen-Constitution** | Chemistry | 2000 |
| | Cardiovascular | 2000 |
| **ElGen-Beauty** | Allure | 1500 |
| | Charm | 1500 |
| **ElGen-Frame** | Statuesque | 750 |
| | Compact | 750 |
| **BlackMarket-Trespasser** | Lockpicker | 1750 |
| | Resourceful | 1750 |
| | Invisible | 1250 |
| **BlackMarket-Infiltrator** | Rogue | 1750 |
| | Hacker | 1750 |
| **BlackMarket-Interceptor** | Mailing List | 500 |
| | Safecacker | 500 |
| **Kensington-SpartanInsuranceSchemes** | Gold Medical Cover | 1000 |
| | Gold Legal Cover | 1250 |
| | Gold Accident Cover | 750 |
| **Kensington-AmbassadorScheme** | Cash Flow | 500 |
| | Competitor Data Mining | 500 |
| **Kaizen-DovePlus** | Physiological Perception | 1750 |
| | Socioeconomic Perception | 1000 |
| **Candor-ModelCitizen** | Street Cleaner | 500 |
| | Bookworm | 500 |
| **Candor-PublicService** | Food Hygeine Inspector | 500 |
| | Sanitary Hygeine Inspector | 500 |
| **Candor-Cartographer** | Urbex Cartographer | 500 |
| | Crawlspace Engineer | 500 |
| **Candor-Community** | Care in the Community | 750 |
| | Heavy Lifter | 1500 |
| **Starch-BrandAmbassador** | Spread the word! | 500 |
| | Put some life into it! | 500 |

*(The spelling of "Hygeine" and "Safecacker" is the game's, preserved deliberately.)*

### Split disks are priced individually

A split disk does **not** inherit a share of its parent's price. Splitting decouples the branches, so the pricing decouples too. Power and Stability come from the same vanilla disk and are nothing alike, and they are not priced alike.

Broadly, the twelve disks at 500 credits all earn money back, so they are self-funding and self-limiting. The expensive end is where the disks that meaningfully change how you play sit. Stability at 3000, which cures fall damage, is priced deliberately close to Leap at 3500, which causes it.

Every one of the 37 prices is a separate config entry, listed under its parent disk's name.

---

## Disk Availability

Vanilla has a quiet problem. **Every Sync Clinic in the city stocks every disk, permanently.** There are seven of them and their stock lists are identical, so nothing is ever out of reach and there is no reason to shop around.

Add 37 split disks and 12 new ones to that and the shop screen becomes a wall.

This plugin gives each vendor a small rotating selection instead.

### The three modes

Set `Mode` in the config:

| Mode | Behaviour |
|---|---|
| **Rotation** *(default)* | Each vendor stocks a handful of disks, redrawn each in-game day. |
| **Vanilla** | Restores the original stock lists exactly. Use this if you want the split and pack disks but not the scarcity. |
| **Unrestricted** | Every vendor stocks everything, including all the new disks. |

**Rotation is deterministic.** The selection is derived from the in-game day, so reloading a save does not reroll the shop and you cannot save-scum your way to the disk you want. Stock does change when the day ticks over, including while you are standing in the shop.

**Unrestricted is the mode to use if you are hunting for one specific disk** to try it out. A four-disk rotation makes that a genuine chore.

### Which vendors, and how much they carry

| Vendor | Disks in stock | Restricted to |
|---|---|---|
| Sync Clinics | 4 | none |
| Black Market Clinic | 6 | none |
| Black Market Trader | 3 | none |
| Weapons Dealer | 3 | none |
| Newsstands | 2 | Candor News |
| Newspaper Boxes | 2 | Candor News |

Six independently configurable pools. Each has `Enabled`, `DisksInStock`, `ExtraRandomSlots` and `RestrictToManufacturers`, so you can turn any vendor off, widen or narrow it, or tie it to a particular manufacturer. A manufacturer filter that matches nothing falls back to the full pool rather than emptying the shop.

**Pick a mode before you start a city and stay with it.** Switching mid-save is not dangerous, but a disk you were saving for can disappear from a shop's rotation.

---

## Known limitations

**New disks only appear as world loot in a freshly generated city.** Loot is decided when the city is built. On an existing save the new disks will show up in shops but never in containers.

**Combined vanilla disks can still turn up as loot even with `RemoveVanillaParents` enabled.** The setting reliably removes them from shop stock, but not yet from container loot. You will occasionally find a combined parent in an apartment. It works normally if you install it. This is a known limitation rather than a bug report, it is cosmetic, and it is still being investigated.

**Street Cleaner and Bookworm lose their third tier when split.** In vanilla, that tier upgrades *both* branches at once. A split disk has no sibling branch to upgrade, so the tier does nothing. Harmless, and it only affects those two.

**The Black Market Trader and the Weapons Dealer only let you in with a password.** The game shows it only in graffiti tags around town, and a tag is sometimes blank. The trader's rotation works normally once you are in. If the password is the problem, my separate mod [Black Market Passwords](https://thunderstore.io/c/shadows-of-doubt/p/Sm3gm/BlackMarketPasswords/) writes the current passwords into a sticky note.

**The availability rotation includes sync disks added by other mods.** If another mod registers a disk, it joins the pool and will be distributed like any other. There is currently no switch to exclude them.

**The rotation is not weighted by rarity.** A very rare disk is currently as likely to appear in a shop as a common one.

**The vendor duplication fix lives in the Disk Availability plugin.** If you disable that plugin and load more than one city in the same play session, vendor stock lists can accumulate duplicate entries. Restarting the game clears it.

---

## How the disks interact

A few interactions are worth knowing about rather than discovering.

**Iron Lung and Sprinter's Curse pull against each other.** One lowers how fast you tire, the other raises it. Running both, the tiredness effect very nearly cancels out. Deliberately not *exactly*, so that an upgrade always does something visible, but two disks with opposite philosophies fighting to a standstill is the intended joke.

**Iron Lung's gait compensation dilutes when stacked.** Its third tier is tuned to cancel out its own heavy gait exactly. If you are already running Quickstep and Sprinter's Curse, it recovers slightly less than the full amount. The shortfall is a few percent and imperceptible in play.

**Long Arm's reach tier stacks with its own main effect, and with Tenacity's Brawn branch.** Run both and you will be picking things up from across the room.

**Second Wind is slow and cumulative by design.** Recovery rates, energy drain and bruise chance are not things you feel in a moment. You notice it after an hour of play rather than after a fight.

**Dead Air and Long Arm ship with two tiers rather than three.** Both had a third tier that was built, tested and cut. One wrote a value the game never reads, the other a value that another popular mod overwrites. They were removed rather than shipped as upgrades that do nothing.

---

## Compatibility

**PlacementPlus.** No conflict. Long Arm's cut second tier was cut precisely because PlacementPlus owns the carry-distance value. Nothing here competes with it.

**SideStreet.** Its themed case rewards are keyed to a hardcoded list of vanilla disk names. Split and pack disks can never be selected as a themed reward, and with `RemoveVanillaParents` enabled SideStreet degrades gracefully rather than failing.

**Vanilla side-job rewards do include pack disks.** The game's own reward system will hand you one.

**Other sync disk mods** should coexist. Their disks will be swept into the availability rotation alongside everything else.

---

## Configuration

Each plugin writes its own file to `BepInEx/config`:

| Plugin | Config file |
|---|---|
| Sync Disk Pack | `ta.sod.syncdiskpack.cfg` |
| Vanilla Split | `ta.sod.vanillasplit.cfg` |
| Disk Availability | `ta.sod.diskavailability.cfg` |

Every disk in the pack can be individually disabled, repriced and retuned. Every split disk can be individually disabled and repriced. Every vendor pool can be resized, restricted or switched off.

**Edit configs with the game closed.** BepInEx rewrites them on shutdown, so changes made while the game is running are discarded.

Prices, rarities and vendor stock are not stored in your save and can be changed at any point in a playthrough. Disk names and effect descriptions are, so disabling a disk you have already installed on a save is not recommended.

---

## Credits

Built on **SOD.Common** by Venomaus, without which none of this exists.

*Shadows of Doubt* is by **ColePowered Games**.

---

## Also by sm3gm

[Black Market Passwords](https://thunderstore.io/c/shadows-of-doubt/p/Sm3gm/BlackMarketPasswords/): press F8 in a sticky note to write the current passwords for the black market trader and weapons dealer into it.

---

## Support

This mod is free and always will be.

Likes are greatly appreciated and keep me motivated to release more mods, so if you enjoy it, please leave one. You give a like on the mod's [Thunderstore page](https://thunderstore.io/c/shadows-of-doubt/p/Sm3gm/SyncDiskSuite/) while logged in to Thunderstore. If you installed through a mod manager, open that page in your browser to find the button.

And if you love what I do and want to support future projects, or just want to send a friendly thought, you can do so here:

[![Support me on Ko-fi](https://raw.githubusercontent.com/sm3gm/sync-disk-suite/main/images/kofi.png)](https://ko-fi.com/sm3gm) [![Buy me a coffee](https://raw.githubusercontent.com/sm3gm/sync-disk-suite/main/images/bmc.png)](https://www.buymeacoffee.com/sm3gm)

Bug reports and ideas help as well. Post them as [GitHub issues](https://github.com/sm3gm/sync-disk-suite/issues).

Thanks for playing.
