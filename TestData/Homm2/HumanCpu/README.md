# Human and computer players (HoMM2, base game)

Twenty-two saves made to find where the game keeps **who plays each colour** and **whose turn it is**. The first four, and the last eleven, are of Broken Alliance (a map where Blue, Green and Orange can be played by a human), the rest of one map with six colours (`slugfest.mp2`).

Result (`docs/KNOWLEDGE_BASE.md`): a byte for each colour at 0x1BE, Blue first: 0x0A is the computer, anything else a human, and the value is his number among the humans. The byte at 0x2AD is the number of the human whose turn it is (0 for Green and 1 for Orange when they are the two humans). The six bytes at 0x1B8 are the faction of each colour.
Used by `PlayerControlTests`. The game only lists save names of at most 8 characters before the extension; the second column is the name the game gave the save.

| Save | Name in the game | What it is |
|---|---|---|
| human_cpu_01_four_humans | `0_p1_4HUM.GM4` | Four humans (Blue to Yellow), Orange and Purple computer |
| human_cpu_02_blue_human | `1_BLUHUM.GM1` | One human: Blue (Warlock); the others are Knights and computer |
| human_cpu_03_green_human | `2_GREENH.GM1` | Same map, the human is Green |
| human_cpu_04_orange_human | `3_ORANHU.GM1` | Same map, the human is Orange |
| human_cpu_05_six_humans_blue_turn | `1_P1_6HUM_blue_turn.GM6` | Six humans, Blue's turn |
| human_cpu_06_green_turn | `2_GREEN.GM6` | Same, Green's turn |
| human_cpu_07_red_turn | `3_RED.GM6` | Red's turn |
| human_cpu_08_yellow_turn | `4_YELLOW.GM6` | Yellow's turn |
| human_cpu_09_orange_turn | `5_ORANGE.GM6` | Orange's turn |
| human_cpu_10_purple_turn | `6_PURP.GM6` | Purple's turn |
| human_cpu_11_autosave_orange_turn | `AUTOSAVE.GM6` | The autosave, made at the start of Orange's turn |
| human_cpu_12_two_humans_blue_turn_day_1 | `1_2HS_BL_G.GM2` | Hot seat, Blue and Green human (Warlock and Wizard), the rest computer; the game begins, Blue's turn |
| human_cpu_13_two_humans_green_turn_day_1 | `2GREEN.GM2` | Green's turn |
| human_cpu_14_two_humans_blue_turn_day_2 | `3BLUE.GM2` | Blue's turn, day 2 |
| human_cpu_15_two_humans_green_turn_day_2 | `4GREEN.GM2` | Green's turn, day 2 |
| human_cpu_16_two_humans_blue_turn_day_3 | `5BLUE.GM2` | Blue's turn, day 3 |
| human_cpu_17_autosave_green_turn_day_2 | `AUTOSAVE.GM2` | The autosave, made at the start of Green's turn of day 2 |
| human_cpu_18_green_orange_green_turn_day_1 | `1_D1_green.GM2` | Hot seat, Green (Wizard) and Orange (Warlock) human, so the first human is not the first colour and they are not neighbours; Green's turn |
| human_cpu_19_green_orange_orange_turn_day_1 | `2_D1_ORN.GM2` | Orange's turn |
| human_cpu_20_green_orange_green_turn_day_2 | `3_D2_GRE.GM2` | Green's turn, day 2 |
| human_cpu_21_green_orange_orange_turn_day_2 | `4_D2_ORN.GM2` | Orange's turn, day 2 |
| human_cpu_22_green_orange_green_turn_day_3 | `5_D3_GRE.GM2` | Green's turn, day 3 |
