# Changelog

## 1.0.3

- Fixed: a newly generated city showed every vendor's full stock for the whole of the first day. Stock rotation is now reapplied after a city load, not only when the day changes.
- Fixed: a patch in the vanilla split plugin was never registered and so never ran. Now registered. Testing showed it has nothing to remove, so this is a diagnostic fix rather than a change you will notice.
- New known limitation: installing more than one perception split from Dove Plus stacks the dialogue penalty inherited from the parent, and shopkeepers may start refusing you. Uninstalling one restores access.
- Plugin versions: SoDDiskPack 0.12.1 unchanged, SoDVanillaSplit 0.4.2, SoDDiskAvailability 0.3.1.

## 1.0.2

- README update only. The three plugins are unchanged from 1.0.1.
- New support section, a link to Black Market Passwords, and corrected known limitations: the black market password now has a solution, and the cause of combined disks in loot is still open.

## 1.0.1
- Vendor stock no longer accumulates duplicate entries when more than one city is loaded in the same play session. This works around an issue in SOD.Common 2.1.4.
- Sync Disk Pack and Vanilla Split now declare SOD.Common as a hard dependency, so a missing dependency fails clearly instead of throwing an unclear error partway through loading.
- Corrected package icon.

## 1.0.0
- Initial release.
