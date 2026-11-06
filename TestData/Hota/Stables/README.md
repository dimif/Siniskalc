# Stables (HotA)

Two hotseat saves of the map `Island of HOTA tests 6p`, made in the game around Tazar's visit to the Stables.
The game shows Tazar with 3175 movement points and a maximum of 2600 before the visit, and 3390 and 2900 after it: the Stables give +300 to the maximum
until the end of the week and 300 more to the points left today (some points were spent walking to them).
Both saves hold the maximum and the points left as the game wrote them. The bit `0x02` of the hero byte at +135 is set in the second save only.
In Shadow of Death the Stables give +400 (`TestData/Sod/Hotseat/hotseat_05_*` and `hotseat_06_*`: 2600 to 3000).

| Save | Original filename | What it is |
|---|---|---|
| stables_00_baseline | `before_stables.GM2` | Tazar: 3175 points left, maximum 2600 |
| stables_01_tazar_visits_stables | `aft_stab.GM2` | Tazar after the Stables: 3390 points left, maximum 2900 |

Used by `VisitedBuildingEffectsTests`.
