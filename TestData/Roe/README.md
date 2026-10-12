# Roe savegames

Shadow of Death (header 42.2) savegames of the Restoration of Erathia map.
Red owns eight castles, one of each RoE faction, named `1_Castle`, `2_Rampart`, `3_Tower`, `4_Inferno`, `5_necropolis`, `6_Dung`, `7_Strong` and `8_Fort`, plus a neutral Rampart castle, `Elfwind`, that never changes until save 64.
Only `1_Castle` and `5_necropolis` are on the coast.
The heroes of Red are Lord Haart, Ufretin, Torosar, Nymus, Damacon, Crag Hack and Tazar (Tazar has a huge movement point maximum from save 61: a cheat).

Names are `roe_NN_<what changed>` (and `sanya_NN_...`, see below); the tables use the number `NN` and a short name.
Where two saves were made at the same step the second has a letter (`roe_15b_dwelling_3`, `roe_48b_rampart_capitol`, `roe_68b_tazar_faerie`).
The rules are in `TestData/README.md`.

The saves are a story: each one changes one thing, in the order of the numbers.
Where the story branches it says so below.
Save 28 does not exist, and `roe_68b_tazar_faerie` was made after `roe_68_tazar_obelisk`.

Tests: `RoeCastleTests` (0-11), `RoeCastleBuildingsTests` (12-49), `RoeHeroTests` (0, 32-42, 49), `RoeBuildingEditsTests` (every building step, rebuilt in the editor and compared with the game), `RoeMapStateTests` (56-77: the raw bytes), `PlayerHeadTests` (heroes and tavern of Red: 55, 64-67, 76), `ObeliskVisitsTests` (obelisks: 0, 67-77) and the sleep flag, the Grail in the backpack and the heroes of a castle in `RoeHeroTests`/`RoeMapStateTests` (59, 61, 77).
What was found is in `docs/KNOWLEDGE_BASE.md` ("Castles, Shadow of Death" and "Shadow of Death: map state").

## Castle buildings (all eight castles, one thing per save)

| Save | What changed | What it checks |
|---|---|---|
| 0 baseline | Every castle has only the village hall | The castle layout (owner, faction, name, masks) and the loader find all nine castles on a Shadow of Death save; Tazar starts with Equestrian's Gloves and no spell book |
| 1 resources | Resources spent, nothing built | No building changes |
| 2 shipyard | Shipyard in the two coastal castles | Only the coast can build one |
| 3 fort, 4 cita, 5 cast | Fort, citadel, castle | An upgrade replaces the building it upgrades in the *current* mask but not in the *built* mask |
| 6 tav, 7 blacksm, 8 market | Tavern, blacksmith, marketplace | |
| 9 townhall, 10 mageguild1, 11 cityhall | Town hall, Mage Guild 1, city hall | The town hall replaces the village hall, the city hall the town hall; the Mage Guild level is stored apart (the byte in front of the name) |
| 12 unique_1 | First special building of each faction (Brotherhood, Mystic Pond, Library, Order of Fire, Mana Vortex, Hall of Valhalla, Glyphs of Fear, Necromancy Amplifier) | Brotherhood replaces the tavern; the Library adds a spell to the Mage Guild |
| 13 unique_2 | Second special building (Lighthouse, Fountain of Fortune, Wall of Knowledge, Brimstone, Portal of Summoning, Escape Tunnel, Captain's Quarters, Cover of Darkness) | The Fortress unique building is Captain's Quarters (the notes of the series had it the other way round); Fountain of Fortune replaces Mystic Pond |
| 14 dwell_1, 15 dwell_2, 15 dwell_3, 16 dwell_4, 17 dwell_5 | Dwellings of levels 1 to 5 (the Castle builds level 4 before 3, so 15 dwell_3 is its Barracks) | Creatures waiting per level are in the savefile |
| 18 unique_3 | Third special building (Stables, Miners' Guild, Lookout Tower, Castle Gate, Artifact Merchants, Freelancers' Guild, Blood Obelisk, Skeleton Transformer) | Miners' Guild replaces the Dwarf Cottage in the current mask |
| 19 dwell_6, 21 dwell_7 | Dwellings of levels 6 and 7 | |
| 20 mageguild2, 25 mageguild3 | Mage Guild 2 and 3 | Spell counts per spell level |
| 22 silo | Resource Silo | The weekly growth of every dwelling was added |
| 23 horde1 | Horde buildings | A horde building replaces its dwelling in the current mask |
| 24 unique_4 | Fourth special building (Treasury, Artifact Merchants, Cages, Battle Scholar Academy, Ballista Yard; nothing for Castle, Fortress and Necropolis) | |
| 26 grail_1, 27 grail_2, 29 all_grails | Grail buildings (Colossus, Soul Prison, Guardian of Earth, Warlords' Monument, Carnivorous Plant, Deity of Fire, Skyship, Spirit Guardian) | Tazar visits castles in these saves |
| 30 upg_dw1, 31 upg_dw2, 35 upg_dw3, 39 upg_dw4, 40 upg_dw5, 44 upg_dw6, 45 upg_dw7 | Upgraded dwellings of level 1 to 7 (the upgraded horde buildings come with them) | The creature count moves to the *upgraded* list |
| 43 mageguild4, 46 mageguild5 | Mage Guild 4 and 5 | Stronghold and Fortress stop at 3, Castle at 4 |
| 47 garrisons | The creatures of the dwellings were recruited into the castle garrisons (Tower: three stacks only) | Garrison slot 1 holds the strongest creature |
| 48 castle_capitol, 48 rampart_capitol | The capitol of the Castle, and (a branch from 47) of the Rampart | The capitol replaces the city hall; Rampart sets bit 29 with it |
| 50 necro, 51 tower, 52 inferno, 53 dung, 54 stronghold, 55 fortress capitol | One capitol each (branches from 47; 49 follows 48 rampart) | Necropolis sets bit 29 with it as the Rampart does; `RoeBuildingEditsTests` |

## Heroes

| Save | What changed | What it checks |
|---|---|---|
| 32 tazar_boots_of_lev | Tazar equips Boots of Levitation | Feet slot |
| 33 tazar_gets_spellbook | Tazar gets a spell book, with nine learned spells | Spell book flag, learned spells |
| 34 tazar_all_spell_tomes | The four tomes (Misc1-4) | Spells granted by artifacts; Magic Arrow from all four |
| 36 crag_scroll_tp_ms, 37 crag_scroll_sp_mirth | Crag Hack picks up spell scrolls (Town Portal, Meteor Shower; then Mirth) | The spell of each worn scroll is read (bytes 4-7 of the equipment slot, as in the backpack) and shown as available from "Spell Scroll" |
| 38 damacon_wings | Damacon picks up Angel Wings | Cape slot |
| 41 tazar_adv_spells | Tazar casts Disguise, Fly and Visions | Active spells (offset 31) are right on Shadow of Death |
| 42 tazar_garrisoned | Tazar is garrisoned in the Dungeon | Garrisoned hero of a castle |
| 49 tazar_fly | Tazar casts Fly (20 spell points) | |

## The map and the game state (the editor does not model these yet; they are read by the tests from the bytes)

| Save | What changed | What it shows |
|---|---|---|
| 56 tazar_no_move | Nobody has moved this turn | The baseline for 57 |
| 57 tazar_moved | Tazar moved one step | **No "moved today" flag exists**: only position, facing, what the hero stands on and the movement points change |
| 58 haart_dug | Lord Haart dug (no Grail there) | The movement points become 0; the tile is added to the list of dug tiles; the hero stands on a hole (type 124) |
| 59 nymus_damacon_sleep | Nymus and Damacon put to sleep | The byte 5 after the hero chunk becomes 1 |
| 60 crag_hack_visiting_with_horde_arts | A new day; Crag Hack equips Head and Arms of Legion and visits the Stronghold | Nothing is stored in the castle; the growth appears in save 76 |
| 61 inferno_garrison_and_visiting | Nymus garrisoned in the Inferno, Tazar visiting | Garrisoned and visiting hero of a castle; +6 of the hero is 0 inside; the 0x40 bit after the chunk |
| 62 tazar_combat | Tazar fought a few battles (experience 1071 to 4230, level 2 to 4, Defense +2, Leadership 2 and a new skill) | Only the experience, level and skills of the hero changed; **no "fought today" flag** was found in the hero, the castles or the player (a later battle only clears the morale and luck effects, see 64, 70 and 74) |
| 63 tazar_idle_of_fortune | Tazar visits the Idol of Fortune (morale until the next battle) | Hero effect byte +138 bit 0x02; the object type 38 under the hero |
| 64 elfwind_captured | Tazar captures Elfwind | Owner byte and the neutral garrison; the castle count of the player grows |
| 65 damacon_before_surrender | Damacon next to the enemy | Waking: the sleep byte is 0 again |
| 66 damacon_surrender | Damacon fled | Owner becomes none; he takes the second place in the tavern |
| 67 damacon_defeated | Damacon hired again and defeated (a new day) | The tavern place is Solmyr's now; the 0x40 bit after his chunk |
| 68 tazar_obelisk | Tazar visits an obelisk | The obelisk flag (5 obelisks in the map) and the index of the object under the hero |
| 68 tazar_faerie | Tazar visits the Faerie Ring (luck until the next battle) | Hero effect byte +136 bit 0x20 |
| 70 tazar_post_battle | Tazar fought another battle | The Faerie Ring effect is gone |
| 71 tazar_faerie_idol | Tazar visits the ring and the idol again | The effects return (so the "visited" table is not what grants them) |
| 72 to 75 obelisk2 to all_obelisks | One more obelisk each | The visit order is 1, 2, 5, 4, 3 and the flag array is by obelisk index |
| 76 tazar_before_dig | New week; Tazar on the tile of the Grail | The castles' weekly growth, the Legion artifacts of save 60, the Grail flag still set |
| 77 tazar_grail_found | Tazar dug up the Grail | The second dug tile, the object definition of the hole, the Grail in the first backpack slot (not modelled), the Grail flag beside the date cleared |

## Sanya series (`sanya_00` to `sanya_05`)

A second series on the same map, kept apart from the numbered castle series above (its names start with `sanya_` so they are not mistaken for saves 0 to 5 of it).
Red's hero Sanya (the game calls her "Sanya") is moved, put to sleep and woken, picks up a quest, and the day changes.
Used by `RoeSanyaMovementTests`.

| Save | What it is | What it checks |
|---|---|---|
| sanya_00_baseline | Start, Sanya at 15,12 with full movement (1560) | Baseline |
| sanya_01_moved | Sanya moved one tile south (1510 points) | Movement points, position and facing change; the object under her and its flags clear |
| sanya_02_asleep | Sanya put to sleep | Only the asleep byte (+1087) changes (and byte 8, not understood) |
| sanya_03_picks_quest | Sanya moved away (1260 points) and visited a quest object | Moving clears the asleep flag; object type 83 (Seer's Hut) under her, visitable byte 1. The map block of the file grows by 404 bytes (the quest text and state): not decoded |
| sanya_04_new_day_not_moved | A new day, Sanya has not moved | Movement points back to 1560; nothing else of the hero changes; the file grows by 4282 bytes, not decoded |
| sanya_05_moved_again | Sanya moved again (1419 points) | Only movement, position, facing and the object under her change: **still no "moved today" byte in Shadow of Death** (saves 4 and 5 show it) |
