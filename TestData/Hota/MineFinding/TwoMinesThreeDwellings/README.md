# Two mines, three dwellings

Sources: `Two Mines Three Dwellings`. File order is sulfur mine (3,8), gold mine (7,8),
Devil dwelling (11,8), Jotunn dwelling (14,8), Automaton dwelling (16,8), all surface record tiles.
Used by `MineFindingTests` for first-record coordinates, captures, round trips, owner/guard editing and neighbouring-byte preservation.
Captures are in one turn without hiring; the Automaton dwelling is unguarded and has stock 5.
Devil and Jotunn dwellings each have stock 1 and a guard of three creatures before capture.
Cheats may account for unrelated hero/resource changes; see the parent README.

| Save | Source | State |
|---|---|---|
| two_mines_three_dwellings_00_baseline.GM1 | begin.GM1 | All neutral |
| two_mines_three_dwellings_01_red_captures_sulfur_mine.GM1 | mine_1.GM1 | Sulfur mine to Red |
| two_mines_three_dwellings_02_red_captures_gold_mine.GM1 | mine_2.GM1 | Gold mine to Red |
| two_mines_three_dwellings_03_red_captures_devil_dwelling.GM1 | dwe_1.GM1 | Devil dwelling to Red; guard removed |
| two_mines_three_dwellings_04_red_captures_jotunn_dwelling.GM1 | dwe_2.GM1 | Jotunn dwelling to Red; guard removed |
| two_mines_three_dwellings_05_red_captures_automaton_dwelling.GM1 | dwe_3.GM1 | Automaton dwelling to Red; stock still 5 |
