# Built this turn (HoMM2, base game)

Fifteen saves of the base-game map with the castles Xabran (Blue, Knight, slot 3), Woodhaven (Orange, slot 7) and Corackston (slot 2, neutral at the start), made to find where the game remembers that **a castle has already built today** (one building a day).

Result (`docs/HOMM2_SAVE_FORMAT.md`): a set of 72 bits, one for each castle slot, 9 bytes that start 0x48 bytes after the end of the castle table (`0x5602` here, `0x58FB` in Threedom). Bit *n* is set when castle *n* has built today; the whole set is cleared when a new day starts, not when the turn passes to the next player.

The game only lists save names of at most 8 characters before the extension; the second column is the name the game gave the save.

| Save | Name in the game | What it is |
|---|---|---|
| built_this_turn_00_baseline | `0_NOTBUI.GM1` | Day 1, nothing built in Xabran |
| built_this_turn_01_xabran_well | `1_BUILT.GM1` | Day 1, Xabran builds the Well |
| built_this_turn_02_day_2 | `3_NB.GM1` | Day 2, nothing built yet |
| built_this_turn_03_xabran_dwelling_3 | `4_B.GM1` | Day 2, Xabran builds the third dwelling |
| built_this_turn_04_hotseat_blue_turn_day_1 | `0_NB.GM2` | Hot seat, Blue and Orange human (Orange has only Woodhaven). Day 1, Blue's turn, nothing built |
| built_this_turn_05_hotseat_xabran_well | `1_BB.GM2` | Blue builds the Well in Xabran |
| built_this_turn_06_hotseat_orange_turn_day_1 | `2_ORNB.GM2` | Orange's turn, nothing built yet |
| built_this_turn_07_hotseat_woodhaven_well | `3_ORB.GM2` | Orange builds the Well in Woodhaven |
| built_this_turn_08_hotseat_blue_turn_day_2 | `4_BNB.GM2` | Day 2, Blue's turn |
| built_this_turn_09_hotseat_xabran_dwelling_3 | `5_BB.GM2` | Blue builds the third dwelling in Xabran |
| built_this_turn_10_hotseat_orange_turn_day_2 | `6_ONB.GM2` | Orange's turn |
| built_this_turn_11_hotseat_woodhaven_dwelling_3 | `7_ORB.GM2` | Orange builds the third dwelling in Woodhaven |
| built_this_turn_12_two_castles_day_4 | `5_2CASNB.GM1` | One player (Blue) with two castles, Corackston and Xabran, day 4, nothing built |
| built_this_turn_13_corackston_well__xabran_well | `6_2CASB.GM1` | Both castles build the Well. Xabran's dwelling mask (+0x1A) also gains 0x3, which is not explained |
| built_this_turn_14_two_castles_day_5 | `7_2CASNB.GM1` | Day 5, nothing built yet |
