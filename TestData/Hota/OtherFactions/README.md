# OtherFactions savegames

Eight red castles, one per faction, start unbuilt and are built one building per save, per day.
The tests are in `OtherFactionsCastleTests`.
The castles: Corona (Tower), Enkindle (Inferno), Grave Raven (Necropolis), Castigare (Dungeon), Kruber (Stronghold), Deadfall (Fortress), Wazzar (Conflux), Highcastle (Castle).
Three neutral castles (two Factory, one Rampart) never change.

## Order of the saves

The names are `other_factions_NN_<what>`, in the order the saves were made (the order comes from the files' modification times; the old names were codes such as `111`, `124` or `214`).
A name that starts with `all_` or states a condition (`05_castle_and_tavern`, `13_all_mage_guild_3`, `19_dwelling_6_before_stables`) is a save made on the way that adds nothing to the one before it.

    00 baseline, 01 fort, 02 citadel, 03 castle, 04 tavern, 05 castle_and_tavern, 06 blacksmith, 07 marketplace, 08 blacksmith_and_marketplace, 09 resource_silo, 10-12 mage_guild_1-3, 13 all_mage_guild_3, 14-17 dwelling_1-4, 18 dwelling_5, 19 dwelling_6_before_stables (the Castle cannot build its level 6 yet), 20 castle_stables, 21 dwelling_7 (and the Castle's 6 comes later), 22 all_dwellings, 23 first_special, 24-26 special_2-4, 27 before_lighthouse, 28 castle_lighthouse, 29 all_town_halls, 30 additional_1, 31 all_additional, 32 additional_2, 33-37 upg_dwelling_1-5, 38-39 upg_dwelling_6-7, 40 all_upg_dwellings, 41 additional_3, 42 city_hall

What 24 (`special_2`), 30, 32 and 41 (`additional_1` to `additional_3`) add is not pinned down by the tests: they change no building the editor knows.

`dung_necro.GM1` is different: five castles placed with the **map editor**, which ignores prerequisites, so it proves bit positions but not what the game requires.
It has Necropolis castles "Nothing" (cursed temple), "Graves", "Transf" and Dungeon castles "NoRing", "WithRing" (warren, with and without Mushroom Rings).

## What to know

- **Faction codes** in the savegame are Castle 0, Rampart 1, Tower 2, Inferno 3, Necropolis 4, Dungeon 5, Stronghold 6, Fortress 7, Conflux 8, Cove 9, Factory 10, Bulwark 11.
  The editor had Dungeon, Inferno, Necropolis at 3-5 for a long time.
  If a castle's buildings look like another faction's, check this first.
- **Only the Castle built a shipyard**: the other castles on the map are inland.
  That says nothing about which factions may build one.
- **The game sets some bits itself**, not modelled in `Building`: 23 in Necropolis from the start and in Castle, Fortress and Conflux with the fort; 22 in Conflux with the fort; 27 in Stronghold with the fort and with the town hall in Necropolis and Conflux; 28 with the city hall in Necropolis and Conflux.
  The upgraded horde buildings (bits 19 and 25) appear when the horde building and the upgraded dwelling both exist.
- **Prerequisites** in `Building` are VCMI's. A save order only proves a prerequisite is not too strict.
  Known game rules not visible in these saves: the Necropolis Unearthed Graves need the Skeleton Transformer, the Castle Training Grounds need the Stables (which need the Barracks).
- Not yet checked against savegames: the grail buildings (bit 26), the capitol, mage guild level 5 outside the Cove, and the specials of Rampart beyond what the Rampart saves show.
