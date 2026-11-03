# Town ownership

HotA hotseat saves supplied from an in-game capture sequence. Town ids in file order:
0 Red_Tower, 1 Red_Inferno, 2 Blue_Necropoli, 3 Neutral_Bulwar.
Town lists follow acquisition order, not sorted ids. Used by `CastleOwnershipTests`.

| Save | State |
|---|---|
| town_ownership_00_baseline.GM2 | Red owns Tower then Inferno; Blue owns Necropolis; Bulwark is neutral |
| town_ownership_01_blue_captures_red_inferno.GM2 | Blue captures Inferno; its list is 2, 1 |
| town_ownership_02_blue_captures_red_tower.GM2 | Blue captures Tower in the same turn; Red is townless; Blue's list is 2, 1, 0 |
| town_ownership_03_red_townless_day_2.GM2 | Red's second day, before recapturing |
| town_ownership_04_red_recaptures_red_inferno.GM2 | Red recaptures Inferno; Blue's list compacts to 2, 0 |
| town_ownership_05_blue_idle_day_2.GM2 | Blue's second day; visible town order is Necropolis, Tower |
| town_ownership_06_red_idle_day_3.GM2 | Red moves in front of the neutral town without capturing |
| town_ownership_07_red_captures_neutral_bulwar.GM2 | Red captures Bulwark on day 4; its list is 1, 3 |
| town_ownership_08_red_selects_red_inferno.GM2 | Same turn, Red selects Inferno; selected town byte is 1 |
| town_ownership_09_blue_captures_red_inferno__selects_red_tower.GM2 | Blue captures Inferno and selects Tower; list is 2, 0, 1 and selected town byte is 0 |
| town_ownership_10_before_red_recaptures_red_tower.GM2 | Red's fifth day before recapturing Tower |
| town_ownership_11_red_recaptures_red_tower.GM2 | Red captures Tower; Red's list is 3, 0 and Blue's is 2, 1 |

The original `blue_capt1` description called it the first Red town, but the bytes
show that Inferno was captured first. Selections clear when turns pass. The user
also observed that loading a save clears town selection; ownership edits leave
the selection bytes unchanged.
