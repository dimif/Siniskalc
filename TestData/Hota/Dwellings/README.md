# Hota: the dwellings of a test map

Horn of the Abyss hotseat saves (`.GM2`) of the test map of `Mines` ("Island of HOTA tests 6p"). Red's Tazar flags the creature generators of the Castle faction one by one, from the guardhouse (level 1) to the portal of glory (level 7); Blue's Orrin follows a turn behind and flags to Blue what Tazar took.

Result (`docs/KNOWLEDGE_BASE.md`, "Dwellings"): the dwellings are a table of records of 150 bytes in the map data after the table of the mines; the owner is the byte at +5 (FF none, 0 Red, 1 Blue), the creature the dwelling gives is at +8 (0 pikeman, 2 archer, 4 griffin, 6 swordsman, ... 14 centaur ...), the creatures that can be hired now are the number at +12 and the tile is at +20, +21, +22 (offsets from the end of the mines' table). Read by `DwellingTests`.

| Save | The game's name | What it is |
|---|---|---|
| dwellings_01_tazar_flags_guardhouse | `244` | Tazar flags the guardhouse (castle, level 1). The owner of the first record goes from FF to 0 |
| dwellings_02_tazar_at_archers_tower | `245` | Tazar goes to the archers' tower (castle, level 2); the table does not change yet |
| dwellings_03_tazar_flags_archers_tower | `246` | The owner of the second record (the archer's) goes to 0 |
| dwellings_04_orrin_flags_guardhouse__tazar_flags_griffin_tower | `247` | Orrin flags the guardhouse to Blue (the first owner is 1); Tazar flags the third dwelling (griffins, level 3) |
| dwellings_05_tazar_flags_portal_of_glory | `314` | Tazar has flagged the portal of glory (castle, level 7), the last creature generator of the Castle; Orrin has flagged the first five to Blue. The saves between 247 and 314 are not filed. 31 records in the table: the Castle's seven creatures are 0, 2, 4, 6, 8, 10, 12 and the stock (+15) of the guardhouse is 28 (two weeks of 14) |
| dwellings_06_tazar_flags_centaur_dwelling__orrin_flags_cavalier_dwelling | `315` | Tazar starts on the Rampart dwellings (the centaur stables, creature 14, flagged for Red); Orrin flags the cavalier's dwelling to Blue |
| dwellings_07_orrin_hires_all_castle_dwellings_but_guardhouse | `316` | Orrin flags the angel's dwelling and hires every creature of every Castle dwelling but the guardhouse: **the creatures available to hire of each of those records go to 0**; Tazar flags the second Rampart dwelling |
| dwellings_08_tazar_flags_wood_elf_dwelling | `317` | Tazar flags the Rampart's wood elf dwelling (creature 18) |
| dwellings_09_tazar_flags_pegasus_dwelling_new_week | `321` | A new week (every stock grows by the weekly growth) and Tazar flags the pegasus dwelling |
| dwellings_10_tazar_flags_dendroid_guard_dwelling | `322` | Tazar flags the dendroid guard dwelling |
| dwellings_11_tazar_flags_unicorn_dwelling | `323` | Tazar flags the unicorn dwelling |
| dwellings_12_tazar_flags_green_dragon_dwelling | `324` | Tazar has flagged one of every Rampart dwelling; he moves on to the neutral ones |
| dwellings_13_tazar_visits_rogue_cavern | `325` | Tazar visits a rogue cavern (a neutral dwelling). The table does not change; he stands 3 tiles below its record |
| dwellings_14_tazar_visits_boar_glen | `326` | A boar glen |
| dwellings_15_tazar_visits_hovel | `327` | A hovel |
| dwellings_16_tazar_visits_alehouse_new_week | `331` | A new week, and an alehouse |
| dwellings_17_tazar_visits_tomb_of_curses | `332` | A tomb of curses |
| dwellings_18_tazar_visits_nomad_tent | `333` | A nomad tent |
| dwellings_19_tazar_visits_treetop_tower | `334` | A treetop tower |
| dwellings_20_tazar_visits_wineyard | `335` | A vineyard (the name the game was given is "wineyard") |
| dwellings_21_tazar_visits_troll_bridge | `336` | A troll bridge |
| dwellings_22_tazar_visits_ziggurat | `337` | A ziggurat |
| dwellings_23_tazar_visits_sulfurous_lair_new_week | `341` | A new week, and a sulfurous lair |
| dwellings_24_tazar_visits_magic_forest | `342` | A magic forest |
| dwellings_25_tazar_visits_crystal_cave | `343` | A crystal cave |
| dwellings_26_tazar_visits_frozen_cliffs | `344` | Frozen cliffs |
| dwellings_27_tazar_visits_enchanters_hollow | `345` | An enchanter's hollow |
| dwellings_28_tazar_visits_ancient_altar_and_wins_the_horn_of_the_abyss | `346` | Tazar visits an ancient altar, wins the fight and gets the Horn of the Abyss |
| dwellings_29_marker_on_the_entrance | `347` | Tazar has his destination marker set on the entrance of an object, 1 tile east and 1 north of him: the destination reads (41, 47) while he stands on (40, 48). It showed that the destination is two 32 bit numbers (x, y) at +45 and +49 of the hero and a 16 bit level at +53, not three 16 bit numbers 2 bytes apart |
