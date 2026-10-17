# Map difficulty, game difficulty and map size (HoMM2, Price of Loyalty)

Nine saves made at the start of a game, to find where the savegame keeps the difficulty of the game, the difficulty of the map and the size of the map (`docs/HOMM2_SAVE_FORMAT.md`).
All are Price of Loyalty saves (`.GX1`).

- 01 to 05 are the same map (Arrax the Jerk, small, a map of normal difficulty) started at each of the five difficulties the player can choose, in ascending order.
- 06 to 09 are different maps, all started at normal difficulty: the map's own difficulty and size are what differ.

| Save | Name in the game | What it is | Used by |
|---|---|---|---|
| map_difficulty_size_01_game_easy | `1_EASY.GX1` | Game difficulty easy, small map of normal difficulty | `HeaderReaderTests` |
| map_difficulty_size_02_game_normal | `2_NORM.GX1` | Game difficulty normal | `HeaderReaderTests` |
| map_difficulty_size_03_game_hard | `3_HARD.GX1` | Game difficulty hard | `HeaderReaderTests` |
| map_difficulty_size_04_game_expert | `4_EXPERT.GX1` | Game difficulty expert | `HeaderReaderTests` |
| map_difficulty_size_05_game_impossible | `5_IMPOSS.GX1` | Game difficulty impossible | `HeaderReaderTests` |
| map_difficulty_size_06_map_easy_small | `6_MAPEAS.GX1` | An easy map, small (36 x 36) | `HeaderReaderTests` |
| map_difficulty_size_07_map_hard_extra_large | `7_MAPHAR.GX1` | A hard map, extra large (144 x 144) | `HeaderReaderTests` |
| map_difficulty_size_08_map_expert_large | `8_MAPEXP.GX1` | An expert map, large (108 x 108) | `HeaderReaderTests` |
| map_difficulty_size_09_map_hard_medium | `9_MAPMED.GX1` | A hard map, medium (72 x 72) | `HeaderReaderTests` |

The game only lists savegame names with at most 8 characters before the extension; the second column is the name the game gave the save.
