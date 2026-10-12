# Hota savegames

Horn of the Abyss savegames.
The castle series each have a test class with the same name; `OtherFactions` has its own README with the order of its saves.
Expected values come from the in-game screens.
See `docs/KNOWLEDGE_BASE.md` for what was found in them.

## Single saves (this folder)

| Save | What it is | Used by |
|---|---|---|
| empire_of_the_world_01_m5w4d6 (+ `_expected_info.md`) | A game of "The empire of the world" on month 5, week 4, day 6, with the values of the hero screens written down | `HotaSaveTests` (heroes), `PlayerTests`, `GameDateTests`, `MapDifficultyTests` (Rook) |
| empire_of_the_world_02_m5w4d6_purple_gifts_red | The same day after Purple sent Red 1 wood, 3 ore, 5 mercury, 7 sulfur, 11 crystal, 13 gems and 17 gold | `PlayerTests`: the resources of both players |
| new_day_01_m5w4d7, new_day_02_m6w1d1, new_day_03_m6w1d2 | The same game on the following days | `GameDateTests`: day, week and month, including the rollovers |
| players_human_red, players_human_tan, players_human_red_and_tan (.GM2) | One map (red is Rampart, tan is Tower) with red, tan or both human | `PlayerControlTests` |
| map_difficulty_edited_to_expert | The map difficulty changed in the map editor from King to Queen | `MapDifficultyTests` |
| tan_not_parsing | A map with a town in each corner, played by Red, Blue, Green and Teal; the colour Tan is not in the game but has 10000 gold in the table of players. Port Evendore (Cove) and the hero Spint (Navigator) of the bottom left corner belong to Teal in the file | `HotaFourCornersTests`: the players listed, who owns what |
| cannot_edit_town_spells | A castle whose Mage Guild spells raised "Can only change SelectedItems collection in multiple selection modes" when one was picked in the drop-down | `ShellSmokeTests` (picking a spell through its combo box) |
| bios_standard, bios_custom | No hero has a biography; then Alkin (Purple) and Ivor (Green) have a long and a short one | `BiographyTests` |
| castles_00_baseline, castles_01_blacksmith_garrison_tumlac_to_blue, castles_02_cloud_temple | Two castles of Tan (Haret and Tumlac): Haret gets a blacksmith and a garrison and Tumlac goes to Blue; then Tumlac builds its cloud temple | `CastleTests`, `GenderTests` (Josephine and Theodorus set to male) |

## Series

| Folder | What it is | Used by |
|---|---|---|
| Arts | Heroes put on artifacts one by one (arts_00_before_arts to arts_12_all; the numbers below are the NN of the name). Edric wears the four tomes and Titan's Thunder, Valeska the Sea Captain's Hat, Ingham the tome of water magic; 1 to 4 move heroes and cross a day (moved today); 6 to 12 cast Water Walk, Disguise, Visions, Water Walk and Fly and all four on one day | `SpellBookAndGrantsTests` (what artifacts grant), `MovedTodayTests` (byte 1095), `ActiveSpellsTests` (bytes 31 to 35) |
| Bulwark | Two castles of Red, the Rampart "Still Water" and the Bulwark "Jotunngard", start unbuilt and get one building per save: first the buildings that are not dwellings, then dwellings, upgrades, support buildings and the grail | `RampartAndBulwarkCastleTests`, `CastleEditingTests` (bulwark_24_nothing__fair), the Bulwark bits and names |
| Cove | `cove_inland_*`: the Cove castle Lakenshire of Red with a baseline (tavern, nymph waterfall, shack, fort and village hall) and one thing added per save (pub, nest, upgrades, roost, thieves guild, blacksmith, frigate, marketplace, grotto, maelstrom, nix fort, tower of the seas; all_cheat has every building). `cove_coastal_*`: a coastal Cove with a shipyard, shack, pub, a visiting hero and a garrisoned hero. neutral_cove_and_factory_castles: neutral castles of both factions | `CoveCastleTests` |
| Factory | Two neutral Factory castles; "Fac Castle G" gets one building per save (named after the building, starting with the gantry); "Fac Castle S" never changes | `FactoryCastleTests` |
| OtherFactions | Eight castles of Red, one of each of Tower, Inferno, Necropolis, Dungeon, Stronghold, Fortress, Conflux and Castle, built one building per save, plus `dung_necro` made in the map editor | `OtherFactionsCastleTests`; README.md in the folder |
| SleepGarrisonVisiting | Akka and Oidana in Red's castle: visiting, garrisoned, asleep, moved away and swapped | `HotaSleepGarrisonVisitingTests`; README.md in the folder |
| Rampart | Two neutral Rampart castles: "Gladeroot" never changes and "Fortune Keep" gets one building per save (the first is the town hall) | `RampartCastleTests` |
