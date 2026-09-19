# Changelog

## 1.1.0

The suite now rebalances vanilla sync disks as well as splitting them.

Most vanilla earner disks pay so little that there is no reason to buy one. A 500 credit disk that pays 10 crows every two hours takes four in-game days to break even, and several upgrade chains were dead on arrival because the effects behind them do nothing once a disk is split. This release retunes those payouts, gives every earner a full three tier chain, and replaces the dead tiers with effects that work.

This changes disk payouts only. It is not an economy rebalance, and prices of ordinary goods are untouched.

If you prefer the old numbers, set `VanillaBalance = true` in `sm3gm.sod.vanillasplit.cfg` and the entire rebalance pass is skipped, prices included.

### Existing saves

Your save is safe. Nothing is lost, no disk is removed, and no upgrade you paid for disappears.

One piece of cosmetic drift is worth knowing about. The game records purchased upgrade tiers by position rather than by name. Chemistry's effects changed in this release, so a player who already upgraded Chemistry will see the old upgrade text while receiving the new effect. The effect is correct, the description is stale. Removing and reinstalling the disk clears it, but there is no need to do so.

### Earner payouts raised

Safecacker, Mailing List, Crawlspace Engineer, Urbex Cartographer, Cash Flow, Competitor Data Mining, Street Cleaner, Bookworm, Food Hygeine Inspector, Sanitary Hygeine Inspector, Spread the word!, Put some life into it!, Care in the Community and Gold Accident Cover all pay more, at every tier.

Bookworm's tier 2 now pays a series bonus of 400. Street Cleaner's tier 3 grants smell immunity instead of another small payout increase.

Prices moved with the payouts. The two Starch disks drop to 100, because vanilla sells the parent for 5 and charging 500 for half of it never made sense.

Statuesque and Compact are no longer mirror priced, because they are not mirror useful. Statuesque gains a speed bonus at tier 3 and Compact gains extra reach.

### Nine splits that had no upgrades at all now have full chains

Lockpicker, Resourceful, Invisible, Power, Stability, Urbex Cartographer, Crawlspace Engineer, Mailing List and Safecacker shipped with completely empty upgrade lists. Every one of them now has three tiers.

Stability is worth calling out. Its four steps sum to exactly total fall immunity, so the disk only reaches what vanilla handed you for free once you have fully upgraded it.

Power's tier 3 lets you barge any door open in a single attempt.

Invisible's base effect is unchanged. Its three tiers are new.

### Single tier splits extended to three

Rogue, Hacker and Fortitude had one upgrade each. Rogue and Hacker now reach the same total as vanilla's full chain. Fortitude gains health regeneration at tier 3.

### Rewritten chains

Clout's three upgrades are reordered so the chain builds rather than peaking early.

Physiological Perception and Socioeconomic Perception had an upgrade tier that could never function, because the effect behind it cannot be applied to a split disk. That tier is replaced with a dialogue bonus, which also offsets the dialogue penalty both disks inherit from their parent. If you have noticed shopkeepers asking you for passwords after installing these, that is why, and this upgrade is the fix.

### Constitution's immunities, redistributed

Vanilla stacked all three status immunities on Chemistry and gave Cardiovascular three identical inventory slots.

Chemistry now reads as a constitution disk. Incoming damage reduced across two steps, a health increase, and cold immunity at the top.

Cardiovascular keeps its three inventory slots and gains tiredness immunity at tier 3.

Smell immunity moved to Street Cleaner.

### Unchanged

Vitality and Heavy Lifter are untouched. Both grant whole inventory slots with no smaller step available, and nothing unclaimed fits them. This is a known gap rather than an oversight.

The disk pack and the stock rotation plugin are unchanged in this release.

### Component versions

SoDVanillaSplit 0.5.0, SoDDiskPack 0.12.1, SoDDiskAvailability 0.3.1.

## 1.0.3

Packaging and dependency fixes.

## 1.0.0

Initial release. Twelve new sync disks, 37 vanilla branches split into standalone disks, and configurable vendor stock rotation.
