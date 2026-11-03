# Hota: the mines of a test map

Horn of the Abyss hotseat saves (`.GM2`) of a test map, "Island of HOTA tests 6p", with Red and Blue human. The map has two rows of mines, one of each kind in each row, and two abandoned mines.
Red's hero Tazar takes **one mine per day**; Blue's hero Orrin follows and **takes it from Red the next turn**. Each save is the game's autosave at the start of a turn, so it shows Red's new mine and the mine Blue took before it.
The first set (saves 01 to 09) takes the row of mines that is entered from the north, from the sulfur mine to the alchemist's lab; then the two abandoned mines. The second set (11 to 17) takes the other row in the reverse order, from the alchemist's lab to the sulfur mine; save 18 takes the first sulfur mine back.
Where Tazar and Orrin stand in each save is the entrance of the mine just taken (Tazar) and of the one before (Orrin).

Result (`docs/KNOWLEDGE_BASE.md`, `docs/FUTURE_IMPROVEMENTS.md`, "Mines: the garrison"): a table of records of 62 bytes holds the owner (0xFF none, 0 Red, 1 Blue), the kind (as in Heroes II: 0 sawmill, 1 alchemist's lab, 2 ore, 3 sulfur, 4 crystal, 5 gems, 6 gold) and the guard of every mine. Used by `MineTests`.

| Save | What it is |
|---|---|
| mines_00_game_begin | Start, Red's turn. No mine is owned |
| mines_01_red_sulfur_mine_1 | Tazar took the first sulfur mine (the one entered at 30, 14) |
| mines_02_red_sawmill_1 | Blue took the sulfur mine; Tazar took the sawmill (26, 14) |
| mines_03_red_ore_mine_1 | The sawmill goes to Blue; Tazar takes the ore mine (21, 14) |
| mines_04_red_gold_mine_1 | The ore mine goes to Blue; Tazar takes the gold mine (17, 14) |
| mines_05_red_gem_mine_1 | Gold goes to Blue; Tazar takes the gem mine (13, 14) |
| mines_06_red_crystal_mine_1 | Gems go to Blue; Tazar takes the crystal mine (9, 14) |
| mines_07_red_alchemists_lab_1 | Crystal goes to Blue; Tazar takes the alchemist's lab (5, 14) |
| mines_08_red_abandoned_mine_sulfur | The lab goes to Blue; Tazar takes an abandoned mine, which gives sulfur (1, 14). The save has 13 bytes less before the map, probably because the new week starts here |
| mines_09_red_abandoned_mine_gold | Blue took the sulfur one; Tazar takes the second abandoned mine, which gives gold (1, 17) |
| mines_10_nothing_taken | Same mines as the save before |
| mines_11_red_alchemists_lab_2 | Blue took the gold abandoned mine; Tazar takes the second alchemist's lab (5, 17) |
| mines_12_red_crystal_mine_2 | The second lab goes to Blue; Tazar takes the second crystal mine (9, 17) |
| mines_13_red_gem_mine_2 | Tazar takes the second gem mine (13, 17) |
| mines_14_red_gold_mine_2 | Tazar takes the second gold mine (17, 17) |
| mines_15_red_ore_mine_2 | Tazar takes the second ore mine (21, 17) |
| mines_16_red_sawmill_2 | Tazar takes the second sawmill (26, 17) |
| mines_17_red_sulfur_mine_2 | Tazar takes the second sulfur mine (30, 17) |
| mines_18_red_sulfur_mine_1_again | Tazar takes the first sulfur mine back from Blue |
| mines_19_finished | The state after the mines (`mines_finished.GM2`), before the places of `VisitedPlaces` |
| mines_20_flagged_by_colours_with_no_player | A different game of the map (`flag_mines.GM2`): the players are Tan, Green, Orange and Pink, but the mines are flagged Red (2) and Blue (14), colours the savefile has no player data for. Used by `NavigationBuilderTests` (their nodes and their pages) |
