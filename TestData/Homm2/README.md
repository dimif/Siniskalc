# Homm2 savegames

Heroes of Might and Magic II (`.GM1` base game, `.GX1` Price of Loyalty).
They are not read by the HoMM3 tests: **do not copy them into `Tests/Siniskalc.Tests`** (its tests parse every `*.GM?` under `TestData` as a HoMM3 save).
They belong to `Siniskalc.Homm2.Tests`, whose project file copies them (see "How HoMM2 was added" in `docs/SINISKALC_ARCHITECTURE.md`); `TestDataNamesTests` checks their names.

| Folder | What it is |
|---|---|
| BrokenAlliance | 24 saves of one game on the base-game map "Broken Alliance" |
| ArraxTheJerk | 1 save of the Price of Loyalty map "Arrax the Jerk" |
| MapDifficultySize | 9 Price of Loyalty saves: the 5 game difficulties on one map, then maps of other difficulty and size |
| MarkerFacing | 11 Price of Loyalty saves that continue Threedom: where a hero's destination marker and its facing are kept |
| HumanCpu | 22 base game saves (.GM1, .GM2, .GM4, .GM6) that show who plays each colour, and whose turn it is |
| Threedom | 71 Price of Loyalty saves of one game on a medium map: mines, obelisks, places a hero visits, every building of two towns |
| BuiltThisTurn | 15 base game saves (.GM1, .GM2) that show where a castle remembers it has built today: one player, hot seat, two castles of one player |
| SecondWell | 2 base game saves: the creatures a week adds with the Well and the faction's second well |

Naming follows `TestData/README.md`, and each folder's README gives the original name the game used (HoMM2 only accepts 8 characters before the extension).
