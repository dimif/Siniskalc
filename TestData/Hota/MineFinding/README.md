# Mine finding controls

Small HotA maps supplied to establish mine and dwelling table counts and record boundaries.
Each subfolder is a different map variant, not a continuation of another map.
Used by `MineFindingTests` and `DwellingFindingTests`; see each folder for the source filenames and capture order.
The map editor screenshot and `Two_Mines_three_Dwe.h3m` locate the final variant's objects.
Map anchors can differ: the sulfur mine sprite was reported at (4,8), while its save-record tile is (3,8).

Movement cheats were used in some capture sequences to finish in one turn; their exact activation points are not all known.
Capture comparisons assert the local object records, not that ownership is the only change in the whole save.
The no-mine map is a negative mine control and a positive single-dwelling control. The one-mine/no-dwelling map is the negative dwelling control.
All map variants start with neutral mines; captures give them to Red.

| Folder | Mines | Dwellings |
|---|---:|---:|
| NoMinesOneDwelling | 0 | 1 |
| OneMineNoDwellings | 1 | 0 |
| OneMineOneDwelling | 1 | 1 |
| OneMineTwoDwellings | 1 | 2 |
| TwoMinesTwoDwellings | 2 | 2 |
| TwoMinesThreeDwellings | 2 | 3 |
