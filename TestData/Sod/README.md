# Sod savegames

Shadow of Death savegames from the complete edition (156 heroes, Red is the player).

| Save | What it is | Used by |
|---|---|---|
| sod_game_00_baseline | Red plays with Marius (Demoniac, level 1, 20 Imps and 7 Gogs) as the only hero; Red has 20 wood, 10 mercury, 20 ore, 10 sulfur, 10 crystal, 10 gems and 20000 gold | `SodSaveTests`: the version is detected, every hero reads and writes back to the same bytes, Marius, the players and their resources |
| sod_game_01_red_gives_blue_resources | The same game after Red gave resources to Blue | `SodSaveTests`: the resources of both players |
| sod_day_01_m1w1d1, sod_day_02_m1w1d2, sod_day_03_m1w1d3 | The same game on three consecutive days (day 1 to 3 of week 1 of month 1) | `GameDateTests`: the date block on Shadow of Death; `MapDifficultyTests`: the map difficulty (King) |

## The Conflux series (saves 0 to 34)

The Conflux saves are named `conflux_NN_<what changed>` (`conflux_00_baseline` to `conflux_34_grail`); the table below uses the number `NN` and a short name.

The same map as `TestData/Roe`, played with seven colours (all but Teal; the computer plays Blue, Tan, Green, Orange, Purple and Pink).
Red builds its Conflux castle (at 50, 4), one building per save, from the fort to the Grail structure; Tazar, Red's hero with the Grails in his backpack, moves in save 1 and visits the castle in save 34.
Tests: `SodSeriesTests` (and `GameDateTests`, `ObeliskVisitsTests`, `PlayerHeadTests`, which run on every savefile).
See `docs/KNOWLEDGE_BASE.md` ("Castles", dwelling growth and companion bits).

| Save | What changed | What it checks |
|---|---|---|
| 0 baseline | Day 1: all colours have their castles (the Rampart "Marishen" is neutral), Red has seven heroes, Tazar carries eight Grails | The head of every player (heroes, tavern), the five obelisks (none visited), the Grail in the backpack read eight times |
| 1 tazar_wins_boots_tomes | Day 2: Tazar moved and won Spellbinder's Hat, Angel Wings, Boots of Levitation and the four tomes; Pink hired Styg and the Tavern of its castle was built | A hero that moved (movement points 2900 to 2318, position and facing) and no byte that says so; a hero hired by the computer is replaced in the tavern; the computer's garrisoned heroes are asleep |
| 2 conflux_fort | Fort in the Conflux castle | Bit 22 comes with the Conflux fort |
| 3 citadel, 4 castle, 5 taver, 6 blacksm, 7 market, 8 silo | Citadel, castle, tavern, blacksmith, marketplace, Resource Silo | 8 is the first day of week 2: every hero of Red is put to sleep, the tavern of every colour changes |
| 9 to 13 mage1 to mage5 | Mage Guild levels 1 to 5 | The Conflux castle goes to level 5 |
| 14 townh, 15 cityh, 16 capitol | Town hall, city hall, capitol | Bits 27, 28 and 29 come with them; 15 is the first day of week 3 |
| 17 magicuni, 18 artimerch | Magic University, Artifact Merchants | |
| 19 dw1 to 25 dw7 | Magic Lantern, the five altars (Air, Water, Fire, Earth, Thought) and the Pyre, the seven dwellings | The creatures waiting by level; 22 is the first day of week 4 (the weekly growth appears) |
| 26 garden_of_life | Garden of Life | |
| 27 updw1 to 33 updw7 | The seven upgraded dwellings (27 also brings the upgraded Garden of Life by itself) | The count of creatures moves to the upgraded list; 29 is the first day of month 2 |
| 34 conflux_grail | Aurora Borealis, the Grail structure, with Tazar visiting | `CastleLinkViewModel.HasGrail` |
