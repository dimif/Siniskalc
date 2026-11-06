# Hota: creature-bank controls

Controls supplied from the user's `games\Creature Banks` directory (`.GM2`, except the Cyclops re-fight, Imp Cache and Naga Bank controls, which are `.GM1`; the Naga re-fight is `.GM2`).
The originals are unchanged; these are byte-identical copies with repository names.
`CreatureBankDecodingTests` pins the map anchors, bank records, victory changes, hero's hidden entrance,
resource reward and exact bytes used for a separate reset trial subsequently verified in game.

| Save | Original name | What changed |
|---|---|---|
| creature_banks_00_baseline.GM2 | GAME_BEGIN.GM2 | Baseline; the first Cyclops Stockpile has five stacks of 10 Cyclopes, no upgraded stack, and 10 of each non-gold resource as reward |
| creature_banks_01_cyclops_stockpile_1_won.GM2 | stockp1_win.GM2 | The first stockpile was won; the hero is still on its entrance. User confirmed 10 wood, mercury, ore, sulfur, crystal and gems; the six player resource fields each increased by 10 |
| creature_banks_02_cyclops_stockpile_1_stepped_off.GM2 | stockp1_stepoff.GM2 | The hero stepped off; the cleared stockpile's entrance is visible in the grid again |
| creature_banks_03_cyclops_stockpile_1_won_again.GM1 | cyclops_stockpile_1_reset_aft_win.GM1 | After loading the generated reset copy, user won the first stockpile again and confirmed receiving the reward again. Resources rise from 10 to 20 each (gold remains 700); empty guards, retained rewards and hidden cleared entrance match the first victory |
| creature_banks_04_dwarven_treasury_1_before_victory.GM2 | dwa_trea_base.GM2 | New Treasury comparison start, not a continuation of the Cyclops re-fight. First Treasury: five stacks of 30 Dwarves, no upgraded center; 10 crystal + 7500 gold reward |
| creature_banks_05_dwarven_treasury_1_won.GM2 | dwa_trea_1_win.GM2 | First Treasury won, hero on entrance (10,24,0). User confirmed 10 crystal + 7500 gold; resources rise from 0 crystal/700 gold to 10 crystal/8200 gold |
| creature_banks_06_dwarven_treasury_1_stepped_off.GM2 | dwa_trea_1_stepoff.GM2 | Hero stepped one tile south to (10,25,0), revealing cleared Treasury state FE01803F; no further reward |
| creature_banks_07_dwarven_treasury_1_won_again.GM2 | dwarven_treasury_1_reset_recleared.GM2 | User re-cleared the reset Treasury; resources are now 20 crystal/15700 gold, proving another 10 crystal/7500 gold. Empty guards, retained rewards, grid subtype and hidden cleared state match the first victory |
| creature_banks_08_imp_cache_1_before_victory.GM1 | imp_cache1_base.GM1 | New Imp Cache comparison start: five stacks of 60, center upgraded to Familiars; reward 6 mercury/3000 gold |
| creature_banks_09_imp_cache_1_won.GM1 | imp_cache1_win.GM1 | Strongest Imp Cache won, hero on entrance (2,27,0). User confirmed 6 mercury/3000 gold; resources rise from 0 mercury/700 gold to 6 mercury/3700 gold |
| creature_banks_10_imp_cache_1_stepped_off.GM1 | imp_cache1_stepoff.GM1 | Hero stepped south to (2,28,0), revealing cleared Imp Cache state FE02003F; no further reward |
| creature_banks_11_imp_cache_1_won_again.GM1 | imp_cache_1_reset_trial_recleared.GM1 | User re-cleared the reset cache; 12 mercury/6700 gold proves another 6 mercury/3000 gold. Whole bank table and hero header match the first victory; grid subtype 3 remains intact |
| creature_banks_12_medusa_stores_1_before_victory.GM2 | med_store_base.GM2 | New Medusa Stores comparison start: five stacks of 10 unupgraded Medusas, reward 10 sulfur/5000 gold |
| creature_banks_13_medusa_stores_1_won.GM2 | med_store1_win.GM2 | Strongest store won, hero on entrance (9,27,0); resources rise from 0 sulfur/700 gold to 10 sulfur/5700 gold, matching the user's reported reward |
| creature_banks_14_medusa_stores_1_stepped_off.GM2 | med_store1_stepoff.GM2 | Hero stepped south to (9,28,0), revealing cleared state FE02803F; no further reward |
| creature_banks_15_medusa_stores_1_won_again.GM2 | medusa_stores_1_reset_trial_win.GM2 | User re-cleared the reset store; 20 sulfur/10700 gold proves another 10 sulfur/5000 gold. Whole bank table and hero header match the first victory; grid subtype 4 remains intact |
| creature_banks_16_naga_bank_1_before_victory.GM1 | nagab_base.GM1 | New Naga Bank comparison start: five stacks of 6 unupgraded Nagas, reward 24 gems/12000 gold |
| creature_banks_17_naga_bank_1_won.GM1 | nagab_1_win.GM1 | Strongest bank won, hero on entrance (17,27,0); resources rise from 0 gems/700 gold to 24 gems/12700 gold, matching the user's reported reward |
| creature_banks_18_naga_bank_1_stepped_off.GM1 | nagab_1_stepoff.GM1 | Hero stepped south to (17,28,0), revealing cleared state FE03003F; no further reward |
| creature_banks_19_naga_bank_1_won_again.GM2 | naga_bank_1_reset_trial_win.GM2 | Reset trial re-fought from (17,27,0): same cleared table/entrance/header as the first win; resources rise again by 24 gems/12000 gold to 48/24700. User confirmed it works |
| creature_banks_00_baseline.h3m | Maps\Tests\Creature_Banks.h3m | Companion map: template 84 is type 16/subtype 0, `AVXbnk10.def`; four stockpile anchors (2,24,0), (4,24,0), (6,24,0), (8,24,0), fixed tiers 3,2,1,0, upgraded-stack selection FF (random) |

Coordinates above are zero-based **map anchors**, not the bottom-right square of the drawn image.
The first stockpile's entrance is (1,24,0), independently confirmed by the winning hero header.
Saved object indices 171..174 have the map anchors above, but their saved definition IDs cannot be trusted:
the first names a gold pile. Do not shift the object-position list to compensate.

The first three saves have a 28-record mixed table at decompressed `0x7EB9B`, ending at `0x7F611`.
Cyclops records are indexes 8..11: five equal stacks of 10,8,6,4; only index 9 has a Cyclops King center.
Only index 8's guards change on victory; the reward bytes and other 27 records do not.
The first entrance is at `0xA5B1`: ready `FC01001F`, cleared `FE01003F`.
In the victory save it is hidden by hero type 34; the cleared state is in the hero's true header at
`0x4AC51 + 16`. See `docs/KNOWLEDGE_BASE.md`, "Creature banks", for exact findings and limitations.

The generated reset trial is deliberately **not a fixture**; its filename retains `_unverified` to identify the original trial.
It is a separate copy of the stepped-off save with the first stockpile's original 56 guard bytes and
four entrance-state bytes restored from the baseline. It preserves the earned resources and every other byte.
The user confirmed the re-fight and second full reward; save 03 independently verifies the second resource increase and re-cleared state.
This proves this exact Cyclops instance's guard-plus-state restoration, not other bank kinds or generalized masks.
There is no subsequent stepped-off/reloaded control after the second victory.

## Dwarven Treasury comparison

Saves 04..06 use the same byte-identical companion map. Treasury template 85 (`AVXbnk20.def`) has type 16/subtype 1.
Anchors are (11,24,0), (14,24,0), (17,24,0), (20,24,0); saved object indices 175..178.
Their bank indexes 12..15 at the same mixed-table offset have five equal stacks of 30/20/15/10,
respectively 10/5/3/2 crystal and 7500/5000/4000/2500 gold. Only index 13 has an upgraded center.
The supplied screenshot confirms fixed 150 Dwarves/10 crystal/7500 gold with random upgraded-stack selection.

First Treasury entrance `0xA6A7` changes `FC01801F -> FE01803F`; first record index 12 is at `0x7F061`.
Only its guard portion changes in the complete table on victory (25 individual bytes); rewards and the other 27 records stay identical.
While occupied, **uint16 type 34 replaces 16 but the grid subtype at +2 remains 1**, object index 175 stays at +4,
and +6 contains hero ID 50. The hero true header `0x4AC51` holds only underlying uint32 type 16 at +11,
has-object 1 at +15 and cleared state at +16; its +13/+14 do not contain the Treasury subtype.
Both surviving footprint cells (`0xA68D`, `0xA6C1`) retain type/subtype/object/ordinal throughout.

`dwarven_treasury_1_reset_trial_unverified.GM2` is a separate copy beside the user's originals, not a fixture.
It restores only the exact baseline 56 guard bytes and four entrance-state bytes (27 actual changed bytes),
leaving subtype 1 and earned 10 crystal/8200 gold intact.
The user confirmed the re-fight worked; save 07 independently verifies the full second reward and the re-cleared state.
The exact strongest Treasury instance's reset is **verified in game**, not a general mask or verification of other tiers/colours.
The trial retains its original `_unverified` filename; there is no subsequent step-off/reload control after the second victory.

## Medusa Stores comparison

Saves 12..14 are a separate comparison start using the same byte-identical companion map.
Template 87 (`AVXbnk50.def`) is type 16/subtype 4; anchors (10,27,0), (12,27,0), (14,27,0), (16,27,0)
match saved objects 183..186. The first saved definition ID instead names an Imp Cache.
Map objects at `0x14A80 + 21*i` select fixed tiers 3,2,1,0 with random upgraded-stack selection FF.
Bank indexes 20..23 have five stacks of 10/8/6/4, rewards 10/8/6/5 sulfur and 5000/4000/3000/2000 gold.
Only the weakest has a Medusa Queen center; the strongest fought store has 50 unupgraded Medusas.

The complete 28-record table at `0x7EB9B` ends at `0x7F611`.
Victory changes only index 20's guards at `0x7F339` (25 actual bytes); rewards and the other 27 records are unchanged.
Victory and step-off tables match. Entrance `0xBA5D` changes `FC02801F -> FE02803F`.
Occupied grid type 34 retains subtype 4/object 183 and holds hero ID 50 at +6.
True hero header `0x4AC51` holds position (9,27,0), underlying type 16, has-object 1 and cleared state at +16.
Footprint `0xBA77` is unchanged. Resources at `0x3B5C4` rise by exactly 10 sulfur/5000 gold.

`medusa_stores_1_reset_trial_unverified.GM2` is a separate copy beside the originals, not a fixture.
It restores only the original 56 guard bytes and four entrance-state bytes at `0xBA63` (27 actual changed bytes),
preserving subtype, earned resources and every other byte.
The user confirmed the re-fight worked; save 15 independently verifies another 10 sulfur/5000 gold
(now 20 sulfur/10700 gold), guards empty again, retained rewards and the same occupied subtype and hidden cleared state.
The exact strongest Medusa Stores instance's reset is **verified in game**, not a general mask or verification of other tiers/colours.
The trial retains its original `_unverified` filename; there is no subsequent step-off/reload control after the second victory.

## Imp Cache comparison

Saves 08..10 are a separate comparison start using the same byte-identical companion map.
Template 86 (`AVXbnk40.def`) is type 16/subtype 3; anchors (2,27,0), (4,27,0), (6,27,0), (8,27,0)
match saved objects 179..182. The first saved definition ID instead names a Treasury; do not shift the position list.
Map objects at `0x14A2C + 21*i` select fixed tiers 3,2,1,0 with random upgraded-stack selection FF.
Bank indexes 16..19 in the same 28-record table have five stacks of 60/40/30/20,
rewards 6/4/3/2 mercury and 3000/2000/1500/1000 gold. The first two have a Familiar center.

Victory empties only index 16's guards at `0x7F1CD` (25 actual byte changes in the complete table).
Rewards and all other bank records remain intact; victory and step-off tables match.
Entrance `0xB99B` changes `FC02001F -> FE02003F`. While occupied, uint16 type becomes 34,
subtype 3 and object 179 survive, and +6 holds hero ID 50.
True header `0x4AC51` holds position (2,27,0), underlying type 16, has-object 1 and cleared state at +16.
The surviving footprint at `0xB981` is unchanged. Resources at `0x3B5C4` rise by exactly 6 mercury/3000 gold.

`imp_cache_1_reset_trial_unverified.GM1` is a separate copy beside the originals, not a fixture.
It restores only the exact original 56 guard bytes (including the Familiar center) and four entrance-state bytes at `0xB9A1`.
Exactly 27 decompressed bytes change; subtype, earned resources and every other byte remain unchanged.
The user confirmed the re-fight worked; save 11 independently verifies another 6 mercury/3000 gold
(now 12 mercury/6700 gold), empty guards again, retained rewards and the same occupied subtype and hidden cleared state.
The exact strongest Imp Cache instance's reset is **verified in game**, not a general mask or verification of other tiers/colours.
The trial retains its original `_unverified` filename; there is no subsequent step-off/reload control after the second victory.

## Naga Bank comparison

Saves 16..18 are a separate comparison start using the same byte-identical companion map.
Template 88 (`AVXbnk60.def`) is type 16/subtype 5; anchors (18,27,0), (20,27,0), (22,27,0), (24,27,0)
match saved objects 187..190. The first saved definition ID instead names a Medusa Stores.
Map objects at `0x14AD4 + 21*i` select fixed tiers 3,2,1,0 with random upgraded-stack selection FF.
Bank indexes 24..27 have five stacks of 6/4/3/2, rewards 24/16/12/8 gems and 12000/8000/6000/4000 gold;
indexes 25 and 27 have a Naga Queen center. This is the first active observation of the 3-per-stack preset.

Victory changes only index 24's guards at `0x7F4A5` (25 actual bytes); rewards and the other 27 records are unchanged.
Victory and step-off tables match. Entrance `0xBB31` changes `FC03001F -> FE03003F`.
Occupied grid type 34 retains subtype 5/object 187 and holds hero ID 50 at +6.
True hero header `0x4AC51` holds position (17,27,0), underlying type 16, has-object 1 and cleared state at +16.
Footprint `0xBB4B` is unchanged. Resources at `0x3B5C4` rise by exactly 24 gems/12000 gold.

`naga_bank_1_reset_trial_unverified.GM1` is a separate copy beside the originals, not a fixture.
It restores only the original 56 guard bytes and four entrance-state bytes at `0xBB37` (27 actual changed bytes),
preserving subtype, earned resources and every other byte. The user re-fought it and confirmed the full repeated reward (fixture 19); only this exact strongest instance is verified.
