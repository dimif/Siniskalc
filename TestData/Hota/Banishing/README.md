# Hota: banishing

A hotseat game in which Blue loses his last town, regains one, loses it again, and is banished after the 7 days.
The other players are not touched.
Used by `PlayerBanishingTests`, `BanishingViewModelTests` and `PlayerBytesLoaderTests`.
Findings are in `docs/KNOWLEDGE_BASE.md` ("Players", byte 22).

The numbers follow the order the saves were made in.
The ones the game wrote at the start of a day (`.GM2` autosaves) are named after the day of the game; the others were saved by hand at the moment the name says.
Save 13 is a `.GM1`: it was saved by hand.

| Save | What it is | Blue's days left (byte 22) |
|---|---|---|
| banishing_00_baseline | The start (named `GAME_BEGIN` by the game): Blue owns one town | FF |
| banishing_01_blue_7_days_left | Blue has just lost his last town (was `1_blue_7_days_left`) | 7 |
| banishing_02_m1w1d1 | Day 1 autosave (was `[hotseat] 111`); Blue owns a town again | FF |
| banishing_03_blue_with_town | Blue with a town (was `2_blue_with_town`) | FF |
| banishing_04_m1w1d2 | Day 2 autosave (was `[hotseat] 112`) | FF |
| banishing_05_m1w1d3 | Day 3 autosave (was `[hotseat] 113`): Blue lost the town again | 7 |
| banishing_06_blue_6_days_left | Blue after one day without a town (was `3_blue_6_days_left`) | 6 |
| banishing_07_m1w1d4 | Day 4 autosave (was `[hotseat] 114`), same as the save before | 6 |
| banishing_08_m1w1d5 | Day 5 autosave (was `[hotseat] 115`) | 5 |
| banishing_09_m1w1d6 | Day 6 autosave (was `[hotseat] 116`) | 4 |
| banishing_10_m1w1d7 | Day 7 autosave (was `[hotseat] 117`) | 3 |
| banishing_11_m1w2d1 | Week 2, day 1 autosave (was `[hotseat] 121`) | 2 |
| banishing_12_m1w2d2 | Week 2, day 2 autosave (was `[hotseat] 122`) | 1 |
| banishing_13_blue_banished | Blue is out of the game (was `4_blue_banished`); his hero is gone | 0 |

The days in the names of the autosaves are the ones the game wrote in the file (checked by `PlayerBanishingTests`).
The old names, in the table, are the ones the saves had when they were added.

## The second game (saves 14 to 16)

Another map (`Island of HOTA tests 6p`, six players), month 4. Blue has lost his last town and counts down; the flag byte after Blue's resources (offset 141 of the struct) is 0 here, which once kept the loader from finding Red and Blue (`PlayerBytesLoaderTests`).

| Save | What it is | Blue's days left (byte 22) |
|---|---|---|
| banishing_14_ban_count_start | Month 4, week 1, day 3 (was `ban_count_start`): Blue has just lost his last town | 7 |
| banishing_15_m4w1d4 | Month 4, week 1, day 4 autosave (was `[hotseat] 414`) | 6 |
| banishing_16_m4w2d3 | Month 4, week 2, day 3 autosave (was `[hotseat] 423`): Blue is banished | 0 |

