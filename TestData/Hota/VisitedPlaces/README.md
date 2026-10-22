# Hota: the places a hero visits (permanent bonuses)

Horn of the Abyss hotseat saves (`.GM2`) of the test map of `Mines` ("Island of HOTA tests 6p"), Red's Tazar and Blue's Orrin, after the mines were taken.
Tazar visits one place a turn, a place of each kind twice (two copies of each on the map), and **Orrin follows a turn behind, visiting the same places**. Each save is the autosave at the start of a turn, so it shows Tazar's new visit and Orrin's visit of the place before. The game names them month, week and day (`137` is month 1, week 3, day 7); the second column gives that name.

Result (`docs/KNOWLEDGE_BASE.md`, "Places a hero visited"): the hero's record keeps, for each kind of place, a 32 bit number with **a bit for each place of the kind, the bit being the number of the place among the places of its kind** (the byte at +16 of the hero holds that number while it stands on the place). The two heroes have their own bits: the places are remembered by hero, not by player.
Used by `VisitedPlacesTests` (not written yet).

| Save | The game's name | What it is |
|---|---|---|
| visited_places_00_baseline | `before_school_of_war.GM2` | Before the first school of war (month 1, week 3, day 7). Tazar and Orrin, nothing visited |
| visited_places_01_tazar_dragon_utopia | `135` | Tazar wins the fight at a dragon utopia (once per map): sword of judgement, statue of legion, lion's shield of courage, ring of the magi and 50,000 gold. The utopia is marked visited for Red and not for Blue (Orrin finds it empty if he goes) |
| visited_places_02_orrin_turn_after_dragon_utopia | `136` | The next turn (Orrin's); the save is 8,749 bytes longer than the one before |
| visited_places_03_tazar_school_of_war_1 | `137` (month 1, week 3, day 7) | Tazar visits a school of war where he stands (+1 attack or defense, once per hero) |
| visited_places_04_tazar_school_of_war_2 | `141` | Tazar visits the second school of war; Orrin has visited the first |
| visited_places_05_tazar_school_of_magic_1 | `142` | Tazar visits a school of magic (+1 spell power or knowledge, once per hero); Orrin visits the second school of war |
| visited_places_06_tazar_school_of_magic_2 | `143` | Tazar visits the second school of magic |
| visited_places_07_tazar_learning_stone_1 | `144` | Tazar visits a learning stone |
| visited_places_08_tazar_learning_stone_2 | `145` | Tazar visits the second learning stone |
| visited_places_09_tazar_garden_of_revelation_1 | `146` | Tazar visits a garden of revelation (+1 knowledge) |
| visited_places_10_tazar_garden_of_revelation_2 | `147` | Tazar visits the second garden of revelation |
| visited_places_11_tazar_star_axis_1 | `211` (month 2, week 1, day 1) | Tazar visits a star axis (+1 spell power) |
| visited_places_12_tazar_star_axis_2 | `212` | Tazar visits the second star axis |
| visited_places_13_tazar_marletto_tower_1 | `213` | Tazar visits a Marletto tower (+1 defense) |
| visited_places_14_tazar_marletto_tower_2 | `214` | Tazar visits the second Marletto tower |
| visited_places_15_tazar_mercenary_camp_1 | `215` | Tazar visits a mercenary camp |
| visited_places_16_tazar_mercenary_camp_2 | `216` | Tazar visits the second mercenary camp |
| visited_places_17_tazar_colossus_1 | `217` | Tazar visits a Colossus of the Magi |
| visited_places_18_tazar_colossus_2 | `221` | Tazar visits the second colossus (the game numbers the autosave 221, not 218) |
| visited_places_19_tazar_arena_1 | `222` | Tazar visits an arena |
| visited_places_20_tazar_arena_2 | `223` | Tazar visits the second arena |
| visited_places_21_tazar_gazebo_1 | `224` | Tazar visits a gazebo (+2,000 experience, once per hero) |
| visited_places_22_tazar_gazebo_2 | `225` | Tazar visits the second gazebo |
| visited_places_23_tazar_tree_of_knowledge_1 | `226` | Tazar visits a tree of knowledge (+1 level, once per hero, for a payment of 2,000 gold) |
| visited_places_24_tazar_tree_of_knowledge_2 | `227` | Tazar visits the second tree of knowledge (each wants a different payment; both of these wanted 2,000 gold) |
| visited_places_25_tazar_library_of_enlightenment_1 | `232` | Tazar visits a library of enlightenment (+2 to every primary skill at his level), where he stands |
| visited_places_26_tazar_library_of_enlightenment_2 | `233` | Tazar visits the second library |
| visited_places_27_tazar_artifact_merchant | `234` | Tazar visits an artifact merchant. In store: bow of elven cherrywood, pendant of total recall, clover of fortune, inexhaustible cart of ore, scales of the greater basilisk, vampire's cowl, everflowing crystal cloak. The hero does not change; the seven are a list of 32 bit artifact numbers near the start of the file (at 0x9C2 in this save) |
| visited_places_28_tazar_black_market | `235` | Tazar visits a black market (the map calls it an artifact merchant too). In store: charm of mana, mystic orb of mana, hourglass of the evil hour, boots of speed, bowstring of the unicorn's mane, helm of chaos, crown of the five seas: the second list of seven artifacts of the file (see save 27) |
| visited_places_29_tazar_black_market_first_4__orrin_artifact_merchant_last_4 | `236` | Tazar buys the first four artifacts of the black market; Orrin, at the artifact merchant of save 27, buys the last four of its seven. In the file the bought slots of each list are FFFFFFFF: the first four of the black market's list and the last four of the artifact merchant's |
| visited_places_30_tazar_refugee_camp | `241` | Tazar visits a refugee camp, which has 2 naga queens for hire. The two do not show as a creature and a count in the records of the mines' table, nor as a pair of 32, 16 or 8 bit numbers in the first 450,000 bytes; where a refugee camp keeps its creatures is not found (the save is 19 bytes longer than `237`, and the saves are not aligned between them) |
