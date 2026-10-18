# What the HoMM2 saves taught that Heroes of Might and Magic III could use

Written on 2026-10-17 after the 82 saves of `TestData/Homm2/Threedom` (see `docs/HOMM2_SAVE_FORMAT.md`), by looking at what the HoMM3 editor knows (`docs/KNOWLEDGE_BASE.md`, `docs/FUTURE_IMPROVEMENTS.md`, `HeroOffsets`) and by comparing the hero chunks of every pair of consecutive saves of the HoMM3 test series (Roe, Sod, Hota/*: about 340 saves) for bytes that change and are not named.
Nothing here is verified for HoMM3 until a probe save says so: the lists are **hypotheses and the saves that would settle them**.

## 1. What carries over

| HoMM2 finding | HoMM3 state | Port |
|---|---|---|
| A hero keeps **bits for the places it visited for a while** (fountain, oasis, faerie ring, graveyard, temple), cleared by the next battle, and the **morale and luck bytes beside them** | `+28`/`+29` (morale, luck) and the bits at `+135`..`+138` are known for watering hole, swan pond, stables, crypt, faerie ring, fountain of fortune, idol of fortune | The same family of bytes should hold **Temple, Oasis, Rally Flag, Mermaids, Fountain of Youth, Buoy, Warrior's Tomb, Shrine of Magic...**: only their bits are missing. Probe set A. Lesson from HoMM2 (T03BONUS): writing the numbers without the bit showed on the troops but not on the hero screen, so the editor must set bit and byte together (it already does for the known ones) |
| A hero keeps **sets of the permanent-bonus places it visited, one bit per place in the order of the map**, in its own record | Not known. `docs/KNOWLEDGE_BASE.md` says the object does not record a visit for Idol/Faerie Ring, but those are the temporary ones. The permanent ones (Arena, Library of Enlightenment, Marletto Tower, Garden of Revelation, School of War/Magic, Star Axis, Tree of Knowledge, Learning Stone, Mercenary Camp, Witch Hut) were never probed | If HoMM3 does the same, the hero page could list each place with its coordinates and forget them one at a time (what HoMM2 now does). Probe set B decides where the memory is: hero record, object, or both |
| The **map keeps who owns a mine** in a table of small records (owner, kind, x, y) and a flag sprite on the tile | `docs/FUTURE_IMPROVEMENTS.md` item 4 wants the mines and their owner editable | HoMM3 has the list of objects with a definition and a position, and the records of the objects after it. The owner of a mine should be one byte in its record, plus the income. Probe set C |
| **Once a week, first visitor of any player** objects (windmill, water mill) change a picture and a flag in the map, not in the hero | `hotseat_13/14` (magic spring: two squares, "visited" until the new week) show a per-square flag and a byte in the player (`+129`) | The player byte `+129` and the per-square flag are likely the HoMM3 counterpart of the weekly state. Probe set C |
| **Once per game** objects (daemon cave, sphinx) change the object itself | Seer's Hut, Pandora's Box, Sphinx, Genie's Lamp... not probed | Probe set C |
| The **byte of a tile that tells what the object is** (HoMM2 byte 9, replaced by a marker under a hero, with the real one kept in the hero) found every kind of place at once | HoMM3 keeps `+11` of the hero = the type of the map object under it, and a list of objects with types | The type list can find every object of a kind (obelisks already work this way); the same code can list forts-like places once their type numbers are known |
| **Level is worked out from the experience**; the game ignores the level byte | `LevelCalculator` | Nothing to port; worth one probe (edit only the level byte in a copy, load in the game) |

## 2. Unknown bytes of the HoMM3 hero chunk that change in the test saves

Offsets are from the start of the hero chunk (the same as `HeroOffsets`). Counts are how many (hero, save pair) changes the whole HoMM3 test set shows. "Known" fields (position, owner, movement, experience, level, army, skills, spells, artifacts, active spells, `+28/+29`, `+135..138`) are left out.

| Offset | Changes | What was seen | First guess |
|---|---|---|---|
| 7, 8, 9 | 43, 97, 44 | change with the day and with moves (`14`→`23`, `38`→`F8`, `07`→`08`) | look like a small counter or a path/animation state: low value for the editor |
| 10 | 4 | `00`↔`04` between the baseline and new-day saves | a flag next to the above |
| 30 | 7 | counts down `08`→`06`→`01`→`00` (Tazar, grail saves), `00`→`03` (Crag Hack with horde artifacts) | **a count of something that is used up** (spell points? artifacts' charges? the movement of a boat?) |
| 43, 44 | 156, 3 | `43` changes in almost every pair (hundreds); `44` a few times | the movement trail / facing counter. `44` `00`→`06` after a battle (Tazar) |
| 51, 52 | 15 | `FF FF`→`00 00` in pairs when a hero is in a town (grail saves, `sod_day_02`) and back to `FF` | **a 16-bit index of a town (0xFFFF none)**. Cheap to confirm with two heroes in different towns |
| 53, 55 | 2, 5 | `00`→`01`; `00`→`04`→`06`→`07` after battles | `55` grows with fights (a **battle counter**?) |
| 77, 78, 79 | 4, 2, 2 | set at a new day for several heroes (`1E`, `57`, `28`) | **per-day value**: random seed? |
| 95 | 2 | `00`↔`04` | a flag |
| 223..235 | few | `01`→`02`→`03`→`00` for Tazar after battles (`225`), `231`, `234`, `235` | the **secondary skill part / specialty counters**; `225` counts like `55` |
| 263 | 2 | `00`→`05` for one hero | unknown |
| 554, 555 | 2 | `FF FF`→`00 00` when a hero gets an artifact into the backpack | **the count of the backpack** (known? not named): check |
| 601..623 | 1 each | `00`→`FF` in bulk for one hero at its first save | the backpack slots, written when a hero is set up: not meaningful |
| 1088 | 8 | `0x40` set on fleeing/defeat, `0xCE`/`0x04` otherwise (`docs/KNOWLEDGE_BASE.md`: not understood) | **a bit set** |
| 1095 | many | moved today (known) | |
| 1102..1144 (HotA) | 30 | Cove saves: `1102 1103 1106 1107 1118 1123 1125 1127 1128 1133 1144` change with the hero's bits (`DC`↔`18`, `05`↔`06`, `01`↔`02`) | **HotA-only struct tail**: possibly the **specialty or the bonus counters of the new HotA mechanics** (boats, the new objects) |

From `docs/KNOWLEDGE_BASE.md`, still unnamed: `+135 0x10` (ring and idol together), `+138 0x01` (first castle visit), the hero `+34` (always 0), the third byte of the visit effects (`+137`), the date marker `7F xx 02 yy` (`yy`), the table of two bytes per object type at date + 12 (the faerie ring and idol bits), the dug tiles' number (154, 536), the 4282-byte block that appears on a new day, the team table, banishing's counter and records, and the tavern's memory of a defeated hero.

## 3. Probe saves that would settle it

One game, one hero (and a second hero where it says), the objects placed next to each other on a test map (the editor is not needed; "save after each step with a name that says the step"). For each save, send the file and the name of the hero. I diff the **hero chunk, the player struct and the table of objects**, as for HoMM2.

### Set A: temporary bonuses until the next battle (hero chunk `+28`, `+29`, `+135..138`)
Visit one object, save; fight a battle, save; end the day, save.

1. Temple (a first visit)
2. Oasis
3. Rally Flag
4. Mermaids
5. Fountain of Youth
6. Buoy
7. Shrine of Magic (any level) and Mystic Garden
8. Warrior's Tomb (the morale part), Shipwreck survivor (a morale part), Sirens
9. Stables **a second week** (to see the stables bit comes back)
10. Two bonus places in one turn that share a bit (Oasis + Temple) to see `+135 0x10` meaning

### Set B: permanent bonuses and who remembers (hero chunk, object records)
For each place: hero A visits, save; hero A visits again, save; hero B visits, save; a new week, save. If a place needs a condition (Tree of Knowledge: gold or gems, Library of Enlightenment: skill level, Star Axis: a visit per hero) say what it was.

11. Arena (+2 attack or defense)
12. Marletto Tower, Garden of Revelation, School of War, School of Magic
13. Star Axis, Learning Stone, Tree of Knowledge
14. Mercenary Camp, Library of Enlightenment, Witch Hut (skill)
15. The same place twice on one map: hero A at the first, hero B at the second, then hero A at the second (to see the **bit per place in map order**, as HoMM2 has)

### Set C: the map (object records)
16. Capture each kind of mine (Sawmill, Alchemist's Lab, Ore Pit, Sulfur Dune, Crystal Cavern, Gem Pond, Gold Mine, Abandoned Mine), one save each; and a mine taken from another player: **the owner byte and the income at the next day**.
17. Weekly objects: Windmill, Water Wheel, Magic Spring (known), Mystical Garden, Wagon, Campfire, Lean To; first visitor, second visitor the same week, new week.
18. Once-per-game: Sphinx (answer right and wrong), Pandora's Box, Seer's Hut quest taken and done, Genie's Lamp, Keymaster's tent (and the matching gate), Border guard.
19. Creature banks: one fight each (Pyramid, Crypt, Dragon Utopia) to see whether the bank keeps its state.
20. Dwellings and a flagged garrison: the owner of a dwelling.

### Set D: the unknown bytes
21. **Two heroes in two different towns**, one in the first town and one in the third, then both outside: confirms `+51/+52` as a town index.
22. **Several battles with one hero**, one save after each: `+53`, `+55`, `+44`, `+225` counters.
23. A hero that **fights, flees, is defeated, is hired again**: `+1088` bits and the tavern memory.
24. A boat: board, sail, land; a boat built; HotA's new boats: bytes `+30` and the HotA tail `+1102..1144`.
25. One hero with the **edit only the level byte** (in the hex editor) and one with only the experience: does the game ignore the level byte as in HoMM2?
26. New day, new week, new month, each with nobody moved: the 4282-byte block and `+77..79`.
27. **Dig** for the Grail at a wrong spot and at the right one, with the same hero: the dug-tile number.

## 4. Suggested order

1. Set D 21 and 22 (cheap, two saves each, unlock `+51/+52`, `+53/+55`).
2. Set A 1 to 4 (a handful of bits of the bonus family we already read and edit).
3. Set B 11 and 15 (decides whether HoMM3 can list the visited places of a hero at all).
4. Set C 16 (the mines, wanted since `FUTURE_IMPROVEMENTS.md` item 4).

When a probe settles an offset, it goes into `HeroOffsets` with a test that names the save, as the others do.

## 5. Checklist: saves for the places a HoMM3 hero visited

Goal: find where a hero remembers the permanent-bonus places it visited (hero chunk, object record, or both), so that the hero page can list each place by its coordinates, as HoMM2 does (`VisitedPlacesCard`). This is Set B above, made exact.

**Series:** `TestData/Sod/VisitedPlaces/` (Shadow of Death; repeat the Arena part in Horn of the Abyss if the bytes differ), files `visited_places_<NN>_<what changed>.GM1`, following the rules of `TestData/README.md` (two digits, the one thing that changed, `00_baseline` first). Add a `README.md` row for each. One map, one human player, **two heroes** (`tazar` and `orrin` are only examples: name the heroes in the file names) and **two copies of the same place** next to each other, so a bit per place in map order can show.

| Save | What to do before saving |
|---|---|
| `visited_places_00_baseline` | The map as it is, both heroes in the same town, nothing visited. Save at the start of a day |
| `visited_places_01_tazar_arena_first` | Tazar visits the first Arena (take attack) |
| `visited_places_02_tazar_arena_again` | Tazar visits the same Arena again (the game refuses) |
| `visited_places_03_orrin_arena_first` | Orrin visits the same Arena |
| `visited_places_04_tazar_arena_second` | Tazar visits the **second** Arena of the map |
| `visited_places_05_new_week` | End the week with nobody moving |
| `visited_places_06_tazar_marletto_tower` | Tazar visits Marletto Tower (a place that gives defense) |
| `visited_places_07_tazar_star_axis` | Tazar visits the Star Axis |
| `visited_places_08_orrin_star_axis` | Orrin visits the same Star Axis |
| `visited_places_09_tazar_learning_stone` | Tazar visits the Learning Stone |
| `visited_places_10_tazar_mercenary_camp` | Tazar visits the Mercenary Camp |
| `visited_places_11_tazar_witch_hut` | Tazar visits the Witch Hut (note the skill it gave) |
| `visited_places_12_tazar_library_of_enlightenment` | Tazar visits the Library (note his skill levels) |

Rules for the saves:
- One step between two saves, and nothing else (no moves, no battles, no spending), so a diff of two files shows only that step.
- Order matters: 01 to 05 decide the Arena first (hero chunk or object), the rest repeat it for one place of each kind.
- Send the file names with the **names of the heroes and the coordinates of the places**; the editor lists each place by those.
- The saves of HoMM2 that settled the same question are `TestData/Homm2/Threedom` (`docs/HOMM2_SAVE_FORMAT.md`): a set of bits in the hero's record, one bit per place in the order of the map.

When a probe settles an offset, it goes into `HeroOffsets` with a test that names the save, and HoMM3 builds `VisitedPlaceRowViewModel`s from the objects of the map (`docs/FUTURE_IMPROVEMENTS.md`, item 9).

