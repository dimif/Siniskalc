# Sod: two-player hotseat (Red and Blue human)

Shadow of Death saves (extension `.GM2`, as the game names hotseat saves) of a map with Red and Blue as human players.
Red's heroes Tazar and Sir Mullich visit obelisks and bonus buildings, one visit per save.
Used by `SodHotseatTests`.

| Save | What it is | What it shows |
|---|---|---|
| hotseat_00_baseline_200_diff | Start, Red's turn | Baseline. The map's obelisk table (7 obelisks) is found, none visited. Red and Blue are read as human from the 32-bit human flags, Teal as not in the game |
| hotseat_01_red_obelisk_1 | Tazar visits the first obelisk | Hero +11 is 57 (obelisk) and +15 is 1; the first obelisk flag byte becomes 0x43, **not 1**: a bit for each colour of Red's team (read per colour now) |
| hotseat_02_red_obelisk_2 | Tazar visits the second obelisk | The second flag byte becomes 0x43 too |
| hotseat_03_tazar_watering_hole | Watering hole | Hero +28 (morale bonus) 0 to 1; effect byte +135 bit 0x40 |
| hotseat_04_tazar_magic_spring | Magic spring | Spell points double; the player gets a byte at +129 set (not the visit mark, see 13 and 14) |
| hotseat_05_tazar_swan_pond | Swan pond | Hero +29 (luck bonus) 0 to 2; +135 bit 0x08 |
| hotseat_06_sir_mullich_stables | Sir Mullich visits the stables | +135 bit 0x02; movement points |
| hotseat_07_sir_mullich_fountain_of_fortune | Fountain of Fortune | +29 is 1; +138 bit 0x08 |
| hotseat_08_sir_mullich_crypt_1 | First crypt visit: a fight, won | Luck bonus and the +138 bit cleared by the battle; army changed |
| hotseat_09_sir_mullich_crypt_2 | Second crypt visit | +28 is 0xFF (morale -1); +136 bit 0x04 |
| hotseat_10_sir_mullich_faerie_ring | Faerie Ring | +29 and +136 bit 0x20 (as in the RoE series) |
| hotseat_11_sir_mullich_magic_well | Magic well | Spell points |
| hotseat_12_blue_turn | Blue's turn (hotseat) | The selected hero of Red becomes 0xFF, Blue's 0; Blue's hero list is unchanged |
| hotseat_13_tazar_magic_spring_other_square | Tazar visits the other square of a magic spring | The game treats the two squares independently (the first is still "visited"). Nothing in the two players tells the squares apart (Red's +129 does not change); the heroes and the map part were not compared |
| hotseat_14_tazar_new_week | A new week | Both squares are "not visited" again. Red's +129 stays 1; the tavern and the income change as every week |

## Five teams (`five_teams_00_game_begin`, `five_teams_01_red_two_obelisks`, `five_teams_02_blue_two_obelisks`)

The same map with the difficulty at 200% and five teams: Red, Blue, Tan, Green and Orange+Purple+Pink alone (the team table is `05 | 00 01 02 03 04 04 00 04`, Teal is 0 by default).

| Save | What it is | What it shows |
|---|---|---|
| five_teams_00_game_begin | The start of the first turn | Obelisk flags all 0 |
| five_teams_01_red_two_obelisks | Red visits two obelisks | The two flags are 0x41: bit 0 Red and bit 6 Teal (Teal is in team 0 with Red). Blue is not in them |
| five_teams_02_blue_two_obelisks | Blue visits the same two | The flags become 0x43: Blue's bit is added. Blue had seen no obelisk before, as another team's visits do not count |

In these saves the game showed Red the tavern heroes Brissa and Dace (heroes 137 and 82 in Red's head) and Blue Malcom and Oris (28 and 110 in Blue's, whom the editor does not list while Blue has no resources).
`SodHotseatTests` checks Red's.
