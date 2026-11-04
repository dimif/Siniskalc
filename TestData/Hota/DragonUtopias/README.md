# Dragon Utopias (HotA)

Controls from the same `Art Merch Dr Utopia` game as `Hota/MerchantStock`.
The companion map is `../MerchantStock/merchant_stock_baseline.h3m`.
Its top row has strengths 4, 3, 2, 1 from left to right; the bottom four
are random. Object anchors are x=5,11,17,23 at y=15 and y=21, surface.
Entrances are one tile west of the anchors.

| Save | Original filename | Observation |
|---|---|---|
| dragon_utopias_00_before_first | `before_ut1.GM2` | Before clearing the strongest, top-left Utopia |
| dragon_utopias_01_first_cleared | `ut01_cleared.GM2` | Marius cleared it; the user confirmed four specified relics and 50,000 gold |
| dragon_utopias_02_first_cleared_again | `ut1_reset_cleared.GM2` | After loading the reset trial and defeating it again; battle and rewards confirmed |
| dragon_utopias_03_tier_3_before | `uto2_bef.GM2` | Creature Banks map, before the 40,000-gold Utopia |
| dragon_utopias_04_tier_3_won | `uto2_win.GM2` | Victory, hero still at entrance (10,15,0) |
| dragon_utopias_05_tier_3_stepoff | `uto2_stepoff.GM2` | Hero one tile south of that entrance |
| dragon_utopias_06_tier_2_before | `uto3_bef.GM2` | Before the 30,000-gold Utopia |
| dragon_utopias_07_tier_2_won | `uto3_win.GM2` | Victory, hero still at entrance (16,15,0) |
| dragon_utopias_08_tier_2_stepoff | `uto3_stepoff.GM2` | Hero one tile south of that entrance |
| dragon_utopias_09_tier_1_before | `uto4_bef.GM2` | Before the 20,000-gold Utopia |
| dragon_utopias_10_tier_1_won | `uto4_win.GM2` | Victory, hero still at entrance (22,15,0) |
| dragon_utopias_11_tier_1_stepoff | `uto4_stepoff.GM2` | Hero one tile south of that entrance |
| dragon_utopias_12_tier_3_won_again | `utopia_2_tier_3_reset_cleared.GM2` | Re-fight after reset; another 40,000 gold and four artifacts |
| dragon_utopias_13_tier_2_won_again | `utopia_3_tier_2_reset_cleared.GM2` | Re-fight after reset; another 30,000 gold and four artifacts |
| dragon_utopias_14_tier_1_won_again | `utopia_4_tier_1_reset_cleared.GM2` | Re-fight after reset; another 20,000 gold and four artifacts |

Saves 03..11 come from the user's `games/Creature Banks` folder. Their
companion `Creature_Banks.h3m` is preserved byte-for-byte as
`dragon_utopias_03_tier_3_before.h3m`. It has eight Utopias followed by the
other creature banks, 28 bank records altogether. Its Utopia anchors match
the earlier map but object indexes differ. The suffixes uto2, uto3 and uto4
mean the second, third and fourth top-row objects, not ascending strength.

Each weaker victory changes only 20 bytes in that bank's 56-byte guard
region across the entire bank table; all rewards and 27 other banks stay
unchanged. Bank indexes 1, 2 and 3 have 8/6/4/3, 8/6/3/2 and 8/5/2/1
Green/Red/Gold/Black Dragons. Red gains exactly 40,000, 30,000 and 20,000 gold.
The bank table starts at `0x7EB9B` and ends at `0x7F611`.
The entrance cells start at `0x6BD7`, `0x6C77` and `0x6D17`.
Their states change `FC00201F` -> `FE00203F`, `FC00401F` -> `FE00403F`,
and `FC00601F` -> `FE00603F`. In the won saves, these states are instead
in Marius's true header `0x4AC51 +16`; stepping off returns them to the grid
without changing any bank record.

Separate reset trials were created beside the user's originals, restoring
only the matching before-save guard bytes and observed entrance state in
each step-off save: `utopia_2_tier_3_reset_trial_unverified.GM2`,
`utopia_3_tier_2_reset_trial_unverified.GM2` and
`utopia_4_tier_1_reset_trial_unverified.GM2`. Each differs by exactly 22
decompressed bytes; originals were hash-checked unchanged. Saves 12..14
confirm successful in-game re-fights. Red has 80,700, 60,700 and 40,700 gold
respectively, another 40,000, 30,000 and 20,000 over each step-off control.
The bank tables and occupied hero headers match the respective first wins:
guards are empty again, rewards retained, and all other banks unchanged.
Tests also count the four awarded artifacts across equipment and backpack.
All four original tiers now have verified resets with repeat rewards.
Changing a tier or choosing different artifacts remains a separate,
unverified edit; no second step-off/reload was supplied for these re-fights.

The strongest reward is Admiral's Hat, Angel Wings, Angelic Alliance and
Armageddon's Blade. Its guards are 8 Green, 7 Red, 6 Gold and 5 Black Dragons.
Clearing empties all seven saved guard slots but retains gold and artifacts.
Restoring guards alone does not reset the bank: the in-game trial still gave
the already-defeated dialog, including after walking away and back.

The successful trial also restored the entrance state from `FE00003F` to
`FC00001F`. While Marius occupies the entrance, this value is at +16 of his
record, not in the map cell. This permitted another battle and another
50,000 gold/four-artifact reward. Both trial saves were separate copies;
the original controls were not overwritten.

Entrance state encodes the bank-table index at bits 13..24. Footprint
object numbering differs: the weakest fixed top-right Utopia is footprint
number 5, but bank index 3. Do not map rewards using footprint order.
The eight bank records begin at `0x7E962` in the first two controls, after
a uint16 count of 8. Each has 28 bytes of creature codes, 28 of counts,
28 of resources (gold at +80), other retained fields, a uint16 artifact
count at +89 and uint32 artifacts at +91. Four artifacts make 107 bytes.
The retained fields at +84 and +88 hold the reward creature type and byte
count for other bank kinds; a Utopia has type -1 and count 0. Other creature
banks can share this variable-length table.

`DragonUtopiaTests` covers the eight identities, retained rewards after both
victories, the exact successful reset, all four guard/gold presets, independent
artifact edits, grid and occupied entrances (including inserted biographies),
ambiguous/truncated/moved tables, mixed-bank saves and unchanged corpus saves.
`PageRenderingTests` checks the actual Map tab, artifact names, guard choices
and Restore command.
