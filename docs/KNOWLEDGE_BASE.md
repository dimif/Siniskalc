# Knowledge base

What is known about the savefile formats of Heroes of Might and Magic III and II that is not obvious from the code.
The document has three parts: what is **common to both games**, what is **specific to HoMM2** and what is **specific to HoMM3**. A topic says what is known, what it means for the editor and what is still to be worked out; the saves that show it are listed under "Supporting savegames" at the end of the topic and described in the READMEs of `TestData` (folder names below are under `TestData`).
How each thing is found is in `docs/LOCATING_ENTITIES.md`.
Offsets are from the start of the chunk unless said otherwise. "Sure" means confirmed by diffing saves or against the game; anything else is a hypothesis.
Add new findings to the topic they belong to, in the part of the game it concerns (or in the common part when it is true in both games).

# Common to both games

Where the games work the same way the rule is stated once here; the layouts (offsets) are in the part of each game.

## Players that lose their last town

* **What**
  * A player that owns no town has a countdown: `FF` while the player owns a town, 7 the day the last town is lost, one less each day, 0 once banished (eliminated). Regaining a town sets `FF` at once.
  * A banished player has no towns and no heroes: the heroes it had get owner `FF` and a shrunk or emptied army (in HoMM2 they keep their position), and another player can hire them again.
* **Editor**
  * Giving a town to another owner applies the same rule in both games (`PlayerModelBase.DaysLeftOnGainingATown` / `DaysLeftOnLosingATown`): the player who loses the last town gets 7 at once, one who gains a town while counting gets `FF` at once, losing one of several or gaining one while not counting leaves `FF`. The days left can be edited; a banished player is shown but cannot be edited.
* **Not yet known**
  * The other bytes banishing changes in HoMM3, so the editor cannot banish a player outright or bring one back.

## Heroes: artifact bonuses, owners and defeat

* **What**
  * The primary skills the savegame holds already include the bonus of the equipped artifacts in both games; the game changes the bytes when an artifact is put on or taken off. A hero never holds less than its bonus.
  * A hero that is defeated gets owner `FF`, loses its place on the map, is removed from the player's list (the later ids move up a place in HoMM3) and goes back to the pool, where another player can hire it. A hire sets the owner colour, appends the id to the player's list and refills the tavern place with a hero the game draws.
  * A player's hero list and the owner bytes of the heroes are two records that must agree; the castle count and castle list of a player follow the owners of the castles the same way.
  * A hero remembers the places that give a lasting bonus itself, one 32-bit number per kind of place and one bit per place in the order of the map (HoMM3: bit n for place n; HoMM2: the first place is bit 1). Clearing a bit lets the hero visit again.
* **Editor**
  * The model keeps the base skill; adding or taking away an artifact moves the skill by its bonus. The owner of a hero or castle is changed together with the players' lists in one step of the history (`PlayerListsReconciler`), and can be reverted in one step. Forgetting places was tried in the game in both games.
* **Not yet known**
  * What happens to a garrisoned hero's owner on a change of owner or on defeat in HoMM3 (the castle's garrison byte), and the hero bytes whose meaning is open (see each game).

### Supporting savegames

* HoMM3: `Hota/Arts`, `Hota/HeroOwners`, `Hota/VisitedPlaces`. HoMM2: Threedom, the Broken Alliance write tests (`E19ARTS`, `E20TAKE`, `T02FORGT`, `T05FORT`, `T10HEROW`). The detail is in the two parts below.

## Obelisks

* **What**
  * The visited flag of an obelisk is a bit mask by colour, one byte per obelisk in the order of the obelisks of the map, not of the visits.
  * HoMM3: a visit also sets the bit of every colour in the visitor's team. HoMM2 has no teams.
* **Editor**
  * An obelisk's visited marks are edited one colour at a time on the player page, in both games (tried in the game).
* **Not yet known**
  * In HoMM3 where the visits of other players are and what the puzzle map needs besides the flags.

### Supporting savegames

* HoMM3: the RoE series, the hotseat map and `five_teams_*`. HoMM2: Threedom (`T04OBELS`).

## Mines

* **What**
  * A mine record holds the owner (a colour, `FF` for nobody), the kind (0 sawmill; HoMM3: 1 alchemist's lab, 2 ore, 3 sulfur, 4 crystal, 5 gems, 6 gold; in HoMM2 the kinds run from 0 sawmill to 6 gold), the guardian (a spell/creature and a count) and the tile. HoMM3 records are 62 bytes, HoMM2 records 7 bytes.
  * Taking a mine changes its owner (and clears the guardian if it was fought for). The income is not stored in HoMM3; in HoMM2 it is a table recalculated at the start of the next day.
* **Editor**
  * The owner of a mine can be changed in both games and the guardian is shown; HoMM3 guard armies are edited with the army editor, HoMM2 guardians are read-only. Mine owner edits were tried in the game in both games.
* **Not yet known**
  * The entrance tile of a mine (HoMM3), and HoMM2's guardian scaling for other spells.

### Supporting savegames

* HoMM3: `Hota/Mines`, `Hota/MineFinding`. HoMM2: Threedom 19 to 21, `Homm2/MineGuardians`, `T07MINES`.

## Resource limits

* **What**
  * Gold is a signed 32-bit number in both games: 2,147,483,647 is the most the game takes. The other six resources 65,535 (likely uint16).
  * Creature stacks are signed 16 bits in HoMM2 (the editor stops at 32,767). Spell points in HoMM2 are limited to 32766 (likely int16).
* **Editor**
  * The limits are in `Heroes.Constants.Resources` (`MaxGold`, `MaxOther`); `MaxOther` is raised only after trying a higher value in the game.

# HoMM2

All integers are little-endian and saves are not compressed. The offsets of every field are documented on the constants of `Siniskalc.Homm2/Data/*Layout.cs` (`HeaderLayout`, `PlayerLayout`, `CastleLayout`, `HeroLayout`, `MapObjectLayout`, `ObeliskLayout`, `SaveLayout`); the topics below record what the layouts cannot say.
"Confirmed" means a diff between saves or a try in the game; "guess" is anything else. Names such as `E06ARTI`, `T11CSTOW` and `C14TURN` are write tests: saves written by the generator tests (`EditorSavesTests`, `ThreedomEditorSavesTests`, `PlayerControlTests`) into `TestData/Homm2/out`, loaded in the game and not kept in the repository. Maps are named after the folders of `TestData/Homm2` (Broken Alliance = `BrokenAlliance`, Arrax the Jerk = `ArraxTheJerk`, Threedom = `Threedom`); "save N" is the number in the file name.

## Game versions and layout

* **What**
  * The base game writes `.GM1`; the Price of Loyalty writes `.GX1`. Both were checked on every table. The expansion differs by:
    * a 4-byte marker `FF FF FF FF` before the width and height, by which the expansion is detected;
    * everything after the header text shifted by +5 bytes (4 from the marker, 1 from an extra header field; the shift is empirical);
    * a hero record of 250 bytes instead of 236: the first 0xD9 bytes are identical and the last 14 are extra (zeros, then `+0xE3..+0xE6` that turn `FF` one after the other as a hero carries more artifacts);
    * the castle table starts 0x36 bytes after the hero table in both, with 72 slots of 100 bytes; players are 6 records of 0xCF in both (empty seats are zeros or `FF`).
  * Map size changes nothing before the map arrays. The file is a fixed layout for a given map, so every save of a map has the same size, and only values change.
  * A byte-for-byte copy of a save loads in the game, so nothing in the file is checked by name, date or checksum.
* **Not yet known**
  * Whether other maps shift more than +5 bytes in the expansion, and what the extra hero bytes are.

### Supporting savegames

* `Homm2/BrokenAlliance` (base game), `Homm2/Threedom` (Price of Loyalty, 82 saves), `Homm2/ArraxTheJerk`: all tables checked on 166 fixtures.

## Header and players

* **What**
  * **Map difficulty and game difficulty are two fields.** The map's (0 to 3) is at 0x0C; the game's (0 to 4, easy to impossible) is the byte in front of the map file name at 0x1C4. A percentage after it (70, 100, 120, 140, 160) also depends on the map.
  * **Who plays each colour has several places and the game reads only some of them.**
    * 0x1BE (6 bytes, 0x0A for the computer, otherwise the human's number among the humans) and 0x1DA (how many humans) feed only the scenario summary: changing only them is ignored and the game plays as before.
    * The mask the game plays from is 6 bytes ending 6 bytes before the date (0x2B5): a byte per record of the table of players, 1 for a human, always the first N records.
    * The mask is not enough: three humans in a file named `.GM2` play only two, and the same bytes in a `.GM3` file play all three. The digit of the extension (the number the game asks for when loading a hot seat game) caps the humans; the name stored in the save (0x29E) does not matter.
    * The table of players is in the order the humans play (hot seat names at 0x1F4 follow it), then the other colours. So to change who plays, the mask, the order of the records and the extension all have to agree.
  * **Whose turn it is** is the number among the humans (the 0x1BE byte of that colour), not the colour: with Green and Orange as the only humans it goes 0, 1, 0, 1. It is 14 bytes before the date (5 bytes further in a Price of Loyalty save).
  * Header bytes 0x2AE and 0x2B0 (0 to 1 when a player was put out) look like a counter of eliminated players (guess); 0x1F1 to 0x1F3 change with play and are unexplained. A text buffer near 0x5CC0 holds the current tavern rumor, built by the game at run time.
  * The date (day of the week 1 to 7, week 1 to 4, month 1 and up) is global, three bytes with a zero between each, in front of the table of players. After day 7 of week 4 the month increments.
  * **Heroes and castles lists of a player** are table indexes: the player's hero list (5 places) holds hero ids and the castle list (8 places, 4 the most seen in use) castle indexes, `FF` for empty.
  * **Daily income** is a table of 6 numbers (wood, mercury, ore, sulfur, crystal, gems) then the gold, recalculated at the start of each day, so it lags a capture by one day. It counts the mines: 2 a day for each ore mine and sawmill, 1 for the others, 1,000 gold for the gold mine, +250 for a statue (3 ore mines gave 6, 2 sawmills 4, 3 crystal mines 3, 2 gem mines 2, 2 alchemist's labs 2, 2 sulfur mines 2, gold 4,500). A put-out player has no mines but the table holds a single 1 that walks over the resources one a day: a leftover, not an income.
  * **Elimination.** A player who owns no castle shows his countdown (7 the day the last castle is lost, then down to 0); `FF` while he owns one, and regaining one sets `FF` at once (the same rule as in HoMM3). At 0 the player has no castle, no heroes, an empty hero list, and the heroes he had get owner `FF` while keeping their position and a shrunk army: no other flag is needed. Another player can hire the hero again.
  * **A defeated hero**: owner `FF`, position cleared, artifacts emptied except the Magic Book (the winner got the best one), and the loser's hero list loses it. A castle lost to another player changes its owner, both players' counts and lists, and the garrison holds the attacker's creature.
* **Editor**
  * The editor writes the turn (`SafeTurnSerializer`, the Order of play card), and the game opens on the new player's turn and goes on in order from there (tried in the game).
  * The editor keeps the hero and castle lists in step with the owner bytes (`PlayerListsReconciler`). Giving a hero to another player and giving a castle were tried in the game.
  * Who plays each colour is shown but not edited, because it needs the mask, the order of the records and the file extension to agree.
  * A writer can ignore the header bytes and the rumor buffer.
* **Not yet known**
  * The meaning of 0x2AE, 0x2B0 and 0x1F1 to 0x1F3.

### Supporting savegames

* `Homm2/MapDifficultySize` (`map_difficulty_size_01_game_easy` to `map_difficulty_size_05_game_impossible`): the game difficulty.
* `Homm2/HumanCpu` and the probe saves `C01` to `C13` (all made from `human_cpu_18_green_orange_green_turn_day_1`): the summary bytes, the mask, the count and the extension (`C12THR.GM3` and `C13THR.GM3` played three humans, Blue, Green and Orange, starting on Green).
* `C14TURN.GM2` and the human/CPU saves: the turn.
* `threedom_71_hillstone_captains_quarters_whiteshield_moat` (income), `broken_alliance_16_sir_gallant_defeats_natasha` (a defeated hero) and the elimination saves `broken_alliance_21_blue_has_no_castle` and `broken_alliance_22_blue_4_days_left`.

## Castles

* **What**
  * **Building bits (16 bits at +0x18).** 0x01 Mage Guild (set with any level), 0x02 Thieves' Guild, 0x04 Tavern, 0x08 Shipyard, 0x10 Well, 0x20 "town only" (no castle; replaced by 0x40 when the castle is built), 0x40 Castle, 0x80 Statue, 0x100 left turret, 0x200 right turret, 0x400 marketplace, 0x800 the second well of the faction (Farm, Garbage Heap, Crystal Garden, Waterfall, Orchard, Skull Pile), 0x1000 moat, 0x2000 the special building of the faction (Fortifications, Coliseum, Rainbow, Dungeon, Library, Storm), 0x8000 captain's quarters. 0x4000 was set by no save.
  * **Dwellings (+0x1A, 32 bits).** Dwellings 1 to 6 are 0x08 to 0x100. An upgrade replaces the bit of its dwelling (dwellings 2 to 6 upgrade with 0x200, 0x400, 0x800, 0x1000, 0x2000, and the sixth a second time with 0x4000); setting both makes the game show "Minotaur, 0 waiting". The upgrades stop where the creatures of the level do (the Cyclops not at all).
  * **Unknown low dwelling bits.** In one pair of saves a castle's dwelling mask changed from `0x08` to `0x0B` when it built a Well; another castle that built a Well kept `0x08`. They are the only castles with the low bits `0x3` set across the 166 fixtures, and a replay of the same Well builds did not reproduce them (only `+0x18` changed from `0x40` to `0x50`, and the built-today set grew). The bits are not established as Well flags.
  * **Mage Guild.** The level is a number in the bits 16 to 18 (0 to 5), not a flag per level. Each level has 4 spell slots (a zero-based spell id) and a byte of how many are shown: 3, or 4 with a Wizard's Library (so the fourth slot of a level that is not counted is never shown). A guild shows 3, 3, 2, 2 and 1 spells at its levels (4, 4, 3 with the Library in a Wizard castle). The spell pool and the counts are in the record.
  * **Creatures waiting.** The counts (six u16 at +0x1E) change only when a week starts, never when a building is built. An upgraded dwelling keeps its count in its own list at +0x2A (six u16; the plain slot at +0x1E is then 0). The dragons upgraded twice use the sixth entry (+0x34), and the game shows none while the entry before it (+0x32) is 0: the saves the game made have a 3 there (what it means is not known). In Threedom the game leaves the old count in the plain list next to an upgraded dwelling (14 beside the live 11).
  * **The Well and the second well**: each week a built dwelling gains its creature's weekly growth, an unbuilt one stays at 0; the Well adds +2 to every built dwelling; the second well adds +8 to the first dwelling only. Seen: a Knight castle 12 + 12 + 2 + 8 = 34 peasants and 8 + 8 + 2 = 18 archers; a Necromancer castle 18 skeletons; confirmed for five factions (the Warlock was in none of the saves).
  * **Built this turn.** A castle can build one thing a day. The game remembers it in a set of 72 bits, one per castle slot, 9 bytes starting 0x48 bytes after the end of the castle table (`0x5602` in the base-game map, `0x58FB` in Threedom). Bit n is set when castle n built today, stays set through the other players' turns, and the whole set is cleared when the day changes. The castle record holds nothing about it.
  * **Owner of a castle**: the owner byte and both players' lists change, and the game also keeps the flags on the map and the colour table (see "Castle flags and the colour table"). A hero that captures a town stands in the square in front of the entrance (the tile under the castle's) and the town has no visiting hero (`+0x17` is `FF`); HoMM2 has no garrison hero, so that field is the only link.
  * The byte `+0x55` grows by 1 a day in every castle (guess: 1 + days played).
* **Editor**
  * The deserializer keeps the complete dwelling mask; the dwelling and Mage Guild reducers preserve unrelated bits, and the serializer writes that mask back. The editor sets one bit per dwelling (the upgrade bit replaces the plain one).
  * The Mage Guild slots offer the spells of their guild's level; a spell of another level that the savegame holds is kept. The levels of 20 spells were corrected to match the guilds and shrines in the saves.
  * The editor does not check what the game needs before a building can be built (a town needs its castle first).
  * The editor writes the 3 of the sixth-dwelling list when it has to (tried in the game: the counts of dwellings, upgraded dwellings and dragons upgraded twice all show). The editor reads the live count of an upgraded dwelling and leaves the old one alone while the level stays.
  * Tried in the game: buildings (including the tavern, thieves' guild and marketplace, turrets, moat and captain's quarters), dwellings and their creatures, the Mage Guild at levels 2, 3 and 5 and the Library, and the castle owner. A castle given to a human player in an area they have not explored stays hidden on the map until the area is uncovered: it can only be reached through the list of castles.
* **Not yet known**
  * The low dwelling bits, the byte `+0x55`, and the meaning of the 3 in the dragon list.
  * The building rules (a turret or moat on a town without a castle, a shipyard inland, one building a day), and whether a dwelling built or upgraded today gets its first creatures at once or next week (`threedom_15_hillstone_mausoleum` and `threedom_16_hillstone_laboratory_new_week` suggest next week).

### Supporting savegames

* `Homm2/Threedom` (`threedom_39_hillstone_skull_pile_whiteshield_foundry` to `threedom_71_hillstone_captains_quarters_whiteshield_moat` for the building bits), `Homm2/SecondWell` (`second_well_01_well__farm`, `second_well_02_next_week`) and `Homm2/BuiltThisTurn` (the 15 saves of the series; `built_this_turn_12_two_castles_day_4` and `built_this_turn_13_corackston_well__xabran_well` for the low bits).
* The write tests `E09CAST`, `E16BLDG`, `E17DWEL`, `E27DWUPG`, `E34DWUP2`, `E35BLKDG`, `E36DRG9`, `E37DRG33`, `E38DRG5` (dwelling counts and the dragon list), `E28GLD5` and `E29LIBRY` (the guild), `T01BLDS` and `T12CANBD` (buildings and built today).

## Heroes

* **What**
  * **The record starts 10 bytes before the name.** The last 10 bytes of each slot are the header of the next hero (spell points, the id again, the owner colour), and the first hero's header sits before the table. The six bytes just before each name are leftover garbage.
  * **The hero id is the table index** (0 to 8 Knight, 9 to 17 Barbarian, and so on). Maps with heroes of their own hold another number in the id byte for four heroes, probably the portrait the map gave them, while the player lists still reference table indexes.
  * **Skills hold the bonuses of the artifacts.** The game adds and takes away the bonus when an artifact is given or taken (the Ultimate Sword is +12 attack; Dragon Sword of Dominion +3 attack, Defender Helm +1 defense, Caster's Bracelet +2 spell power, Breastplate of Anduran +5 defense; the Golden Bow, Endless Sack of Gold, Elemental Ring and Fizbin of Misfortune add no skill). A bonus written to a hero does not show in the game until it is also in the skill.
  * **Level is worked out from the experience; the game ignores the level byte**: level byte 1 with 50,000 experience shows level 17, level byte 20 with none shows level 1, and 200,000 shows level 25. The levels follow the fheroes2 table (agrees with seven (hero, save) pairs of levels 3 to 11, slightly off at the top: 54,200 and 232,700 against 54,244 and 231,549). A file level 20 with 50,000 experience showed level 14, which is not explained.
  * **Maximum movement is derived from the army** (1000 with peasants and archers, 1400 with black dragons, 1200 for one start army); the game recalculates it and it is not stored.
  * **Spell points**: the cap is knowledge x 10 and is not stored; casting spends the cost (Magic Arrow 3) and the hero regains 1 a day. The most the game accepts seems to be 32766: the field is 16 bit, 65535 and 32767 both show as -1, while 12767 loads fine.
  * **Stacks** are signed 16 bits: 65,535 shows as -1 on the map and 0 on the hero screen and splitting misbehaves; 32,767 peasants and 9,999 rogues split fine and one more overflows to -32,768.
  * **Artifacts** are a plain list of 14 slots, not tied to a body part, `FF` empty. A hero who wins a fight gets the loser's best artifact. Every spellcasting-class hero starts with the Magic Book (81) in slot 0; the spell book is a byte per spell id (1 to 65).
  * **Secondary skills** are two tables: a level per skill (0 none to 3 expert, 14 skills) and the order they were learned in (1-based), plus a count. A level-up is a plain stat change (the level never limits experience).
  * **A hire**: the owner byte goes from `FF` to the colour, the position to the castle, the castle visitor to the hero id, gold falls, the player's hero list gets the id and a new tavern hero replaces him.
  * **Destination and facing.** The destination (two u32 at +0x17, `FFFFFFFF` when none) is the tile the game marks on the map; the facing (+0x22, 0 north clockwise to 7 northwest) is the way of the last step and 0 inside a castle. The byte at +0x23 is the tile object under the hero (0xA3 castle gateway, guess). Byte +0x25 can be nonzero while the hero is outside a castle (6, 42 and 3 on a crystal mine, a shrine and a sawmill: the tile quantity shifted right by 3, a relationship that is unverified). On the tile of a hero the map's byte 9 is 0xAA and the hero's +0x23 keeps the real one, which is why a place a hero stands on is not lost.
  * **What a hero remembers** (offsets from the name):
    * **+0x3A and +0x3B: morale and luck for the next battle only** (signed bytes): a temple +2 morale, a fountain +1 luck, a faerie ring +1 luck, a graveyard visited a second time -1 morale. They are 0 again after the next battle. +0xD9 and +0xDA (Price of Loyalty only) hold the memory of what gave them: 0x04 fountain, 0x08 oasis (+1 morale and the maximum movement of the day went from 1,559 to 2,359), 0x10 faerie ring, 0x20 a graveyard that had been visited, 0x0100 temple, and for good or a week 0x400000 the arena (a point to a skill, once per hero) and 0x800000 the stables (+400 maximum movement until the start of the week). The hero's screen works from the bits and the troops from the bytes: setting only the two bytes showed the bonus on the troops but not on the hero's screen. The game clears the first group after a battle.
    * **+0x3D, +0x41, +0x45, +0x49, +0x4D, +0x55: the places visited that give a hero something once**, a 32-bit set each for gazebos, forts, witch doctor's huts, mercenary camps, standing stones and Xanadu, **a bit for each place in the order of the map, and the first place is bit 1** (bit 0 is unused; standing stones are counted by their left tile). Clearing a set lets the hero visit again. +0x51 was never set. The windmill, water mill and graveyard (once a week to the first visitor of any player) are in the map, not the hero.
    * The arena has no mark of its own on the map, so its memory is one bit of the hero, not one per arena.
* **Editor**
  * The model moves the bonus with the artifact, and the editor edits the base skill. The hero page sets the level from the experience (`HeroLevels`) and the experience to the start of a level when the level is set. The maximum movement is not stored; the editor stops stacks at 32,767 and spell points at 32766.
  * The locator accepts a hero table when most heroes hold their index (and every castle holds its own), and links and taverns use the table index; the original id byte is kept untouched. The editor uses the castle record's `VisitingHeroId` to identify a visitor, not byte +0x25, which it leaves untouched.
  * The temple and fountain bits are set together with their morale and luck numbers.
  * Tried in the game: skills with artifact bonuses, secondary skills, the spell book, levels and experience, movement, army sizes, the temple and fountain bonuses, forgetting places (including one forgotten fort), a hero's destination.
* **Not yet known**
  * Loading an edited scenario with its own heroes in the game is unverified.
  * The effect of byte +0x25, the extra tail of the Price of Loyalty hero, and the bonus of each arena.

### Supporting savegames

* The write tests made from `Homm2/BrokenAlliance`: `E04SKILL`, `E05SKL2`, `E06ARTI`, `E07BOOK`, `E08LEVEL` and `E12ALL` (skills, spell book, level and movement edits), `E19ARTS` and `E20TAKE` (artifacts with their skills), `E21SKILL` and `E22BOOK`, `E24STACK`, `E25STATS` and `E30XP50K`, `E31LV20`, `E32BIGXP` (level from the experience), `E33STACK` (the 32,767 stack limit), `E13ARTSW`.
* `map_difficulty_size_07_map_hard_extra_large` (Famine, War, Pestilence: heroes with another id byte), `broken_alliance_16_sir_gallant_defeats_natasha` (defeated hero).
* `Homm2/MarkerFacing` (`marker_facing_83_destination_south` to `marker_facing_90_destination_southwest`: the destination on each of the eight sides of Colar Blau, with the hero facing east; `marker_facing_91_facing_west`, `marker_facing_92_facing_northwest` and `marker_facing_93_facing_northeast`).
* `threedom_19_charity_captures_crystal_mine`, `threedom_20_charity_visits_shrine` and `threedom_21_charity_captures_sawmill`: the byte +0x25. The write tests `T02FORGT` (forget places), `T03BONUS` and `T06SRC` (morale, luck, temple, fountain), `T05FORT` (one fort), `T10HEROW` (a hero given to another player), `T13DESTN` (destination).

## Map part

* **What**
  * Offsets are from the end of the castle table ("M"). Most of this was seen in the 82 saves of Threedom, and the table of tiles, the obelisk bytes and the mine records were also looked for in the other maps.
  * **Capturing a mine, visiting an obelisk, picking up an artifact or capturing a town changes bytes only in the map part (and the owner's resources), never in the player's record.** The player record knows nothing about mines besides the income table.
  * **The first part is a list of blocks** each starting with `D2 04 00 00` (1234), a 16-bit length and a payload: signs, events and messages (rumors, a hero's story), and the objects with state. Blocks hold no position: an object's block is found by the order of the objects of its kind on the map, row by row, as the bits of a hero's visited places are.
  * **The table of tiles.** After M the width and height are written again as two u32, then `width * height` tiles of 12 bytes, row by row (Threedom: M+0x49B8). Bytes 0 and 1 the ground picture; 2 and 3 the kind and picture of the object (the kind varies with the terrain, so it is not used to find objects); 4 and 5 flags (what it blocks; 0x0188 or 0x0100 when a hero stands on it); 6 and 7 a second object such as the flag of a mine; **9 what the object a hero enters by is**, which is how places are found; 10 and 11 a link to an entry of the table of objects after the tiles. Byte 9: 0x99 obelisk, 0x8A gazebo, 0xBE fort, 0xC5 witch doctor's hut, 0xC9 mercenary camp, 0xC2 standing stones (two tiles), 0xD6 Xanadu, 0x9F / 0xCA / 0xCB shrine of the first / second / third circle, 0xC6 temple, 0x89 fountain, 0xA8 windmill, 0x96 water mill, 0x8C graveyard, 0xA3 the gate of a castle, 0xFA arena and stables, 0xCF sphinx. The counts agree with the pictures (Threedom: 9 obelisks, 5 gazebos, 5 forts, 6 huts, 4 camps, 7 stones, 1 Xanadu) and the byte is better across terrains (the largest map has 23 gazebos by the byte and 21 by the picture).
  * **Obelisks.** At M+0x538 are 48 records of 8 bytes, then 48 bytes of `FF`, and at M+0x6E8 one byte per obelisk with a bit for every colour that visited it (bit 0 Blue to 5 Purple; the AI sets theirs as they walk). They are in the order of the map, row by row. HoMM2 has no teams.
  * **Mines.** Records of 7 bytes from M+0x05: `owner, kind, guardian spell, guardian count, x, y, FF`; owner a colour or `FF`, kind 0 sawmill to 6 gold. Unguarded records have `FF, 00` or `FF, FF` in the two guardian bytes. Capturing sets the owner, adds the flag as the second object of a tile (bytes 6 and 7: the first picture of the kind, 14 for mines, 21 for the alchemist's lab, 28 for the sawmill, plus the owner's colour) and recalculates the income the next day. The flag of a mine nobody owns is the empty object (class 0, picture `FF`). The saves have no flag for the alchemist's lab, even when it was captured.
  * **Mine guardians.** A guarded mine has a guardian spell code (Earth 62, Air 63, Fire 64, Water 65; not a creature code) and a count byte. A guardian cast with spell power 5 and knowledge 5 writes count 20; with spell power 10 and knowledge 5 it writes 40: Earth gives 4 guardians per point of spell power in these controls. When a player defeats the guardians and captures the mine, the owner is set, the guardian spell goes to `FF` and the count to zero, with kind and coordinates unchanged.
  * **Castle flags and the colour table.** The two flags of a castle are addons (class `0x38`, as the flags of a mine) of the tiles on the left and the right of the castle's own tile, linked from those tiles by the 16-bit number at byte 10. Their picture is twice the number of the owner's colour (Blue 0, Green 1, Red 2, Yellow 3, Orange 4, Purple 5) plus 0 for the left flag and 1 for the right one, and 12 and 13 for a castle of nobody. It holds for all 268 flags of the castles in ten saves. Conquering a castle changes nothing else on the map but these two addons (and the hero marker on the tile). The minimap draws the castles from another table: 72 bytes right after the castle records, one for each castle slot, with the colour number of the owner and `FF` for nobody (`CastleFlags.OwnerTableStart`); the game changes the byte of a conquered castle. It agrees with the owners of every castle in the Threedom saves the tests read.
  * **Sphinx** (one block, 335 bytes in Threedom, 377 in Arrax): 1 while the riddle is unanswered and 0 after; the reward as seven u32 resources (8,000 gold; 15 mercury in Arrax); the artifact reward as u16 (96 means none and is also the code of the Legendary Scepter, read as none); the number of answers; 8 answers of 13 bytes; then the riddle text. The riddle and answers are in the savegame and can be edited. The gold arrived with the answer.
  * **Shrine:** the spell is in the tile: the 16-bit number at tile bytes 4 and 5 shifted right by 3 (the low 3 bits are the tile's flags and are kept). A shrine is not used up and leaves no mark; the hero's spell book is the only memory. First-circle spells are all level 1, third mostly level 3. A hero standing on a shrine hides its spell (the tile holds the hero marker).
  * **Daemon cave, windmill, water mill:** nothing changes in the hero besides the reward; the picture of the object changes in the table of objects after the tiles (an entry goes from `5C 0C` to `5E 00`).
  * Walking reveals explored rows as runs of 6 to 9 bytes spaced 72 apart; terrain tiles and kingdom state are not decoded.
* **Editor**
  * The editor writes both owner and flag for a mine, except for the alchemist's lab. The mine list shows the guardian (the elemental and the saved count, or "Unguarded") read-only; unknown encodings reaching the deserializer keep their raw spell and count for display instead of being shown as unguarded. `MapObjectsReader` accepts no spell (`FF`) with count zero or `FF`, or a guardian spell 62 to 65 with a nonzero count; guardians are not editable, and changing the owner is not simulated combat: it does not clear the guardians.
  * The editor writes the castle flags and the colour byte together with the owner (`SafeCastleSerializer`); the castle then looks and plays as the new owner's (tried in the game). Writing only the owner byte and the lists, as it first did, left the castle showing its old colour on the map and minimap, in the old owner's list, and open to being conquered back.
  * The editor does not change the shroud. The riddle, answers and rewards of a sphinx are editable within the room the savegame has; the shrine spell is editable unless a hero stands on it.
  * Tried in the game: the obelisks (all nine), mine owners (with their flags and income), shrine spells and the sphinx.
* **Not yet known**
  * The scaling of the other guardian spells, high-count encoding and partial combat losses.
  * The daemon cave, windmill and water mill state; terrain tiles and kingdom state.

### Supporting savegames

* `Homm2/Threedom` (82 saves): the tile table and the blocks; `threedom_19_charity_captures_crystal_mine` to `threedom_21_charity_captures_sawmill` (mines), `threedom_74_colar_blau_visits_sphinx` (the answered sphinx), `threedom_82_charity_visits_xanadu` (the base of the write tests); the write tests `T04OBELS`, `T07MINES`, `T08SHRIN`, `T09SPHNX`, `T11CSTOW` (the castle flags and, in a second version, the colour table).
* `Homm2/MineGuardians` (`mine_guardians_00_baseline` to `mine_guardians_07_orange_captures_mine`; Ariel (hero 24) and the gold mine at (43,23), the record at absolute `0x568A`; `MineGuardianTests`).
* `arrax_the_jerk_01_start` (the sphinx with 377 bytes and 15 mercury); the write test `E15LAST` (a castle in an unexplored area).

## Verification against the game

* **What**
  * Gold, resources, army, primary and secondary skills, the spell book, level, experience, movement, the garrison and a castle's owner were all edited and the result loaded in the game.
  * **Resource limits** (tried in both games): gold is a signed 32-bit and 2,147,483,647 is the most; the other six took 65,535 each without trouble.
  * The creature codes of the Peasant, Archer and Black Dragon, the Mage Guild spells and the artifacts picked up were checked against the saves; the middle of the creature list follows the fheroes2 order and is not verified. Only spells 6, 18, 26, 35, 50 and 51 were verified against the game; the rest of the table is fheroes2's.
* **Editor**
  * The limits are in `Heroes.Constants.Resources` (`MaxGold`, `MaxOther`); `MaxOther` is raised only after trying a higher value in the game. The real limit of the other resources is higher but unknown.
* **Not yet known**
  * The creature codes in the middle of the list and the spell table other than the six verified entries.

### Supporting savegames

* The write tests `E01GOLD`, `E02RESRC`, `E23LIMIT` and `E39GOLDX` (resources).

# HoMM3

Integers are little-endian. Savegames are gzip files (see "Savegame format"); the versions are Restoration of Erathia, Armageddon's Blade, Shadow of Death and Horn of the Abyss, and the saves of `TestData/Roe` are Shadow of Death saves.

## Savegame format

* **What**
  * A savegame (`.GM1`, `.GM2`, `.GM8`, `.CGM`, ...) is a gzip file whose CRC does not match, so strict decoders fail: 7-Zip extracts it but complains, .NET's `GZipStream` and sharpcompress throw on the compression method, and SharpZipLib raises a CRC exception that can stop decompression halfway. The stream is complete (it ends with an 8-byte trailer), and saves edited by hand or by other tools show the same CRC error and still load.
  * The decompressed data starts with the ASCII magic `H3SVG`. The first few hundred bytes hold the map's title and description as plain text, then the map file name (for example `Free for All.h3m`), `maps` and the marker `GAME_BEGIN`. Hero and town names are plain text too, in fixed-size zero-padded fields.
  * Hotseat player names are not in the save: the game assigns them at runtime. The save only says which colours are human.
* **Editor**
  * It includes the GZip part of SharpZipLib 1.2.0 with the line that raises the CRC exception commented out (`GzipSavefileCodec`).
  * When searching for a string, search the decompressed bytes, in ASCII and UTF-16, and check the surrounding bytes: partial hits are common ("Jenova" and "Christian" while looking for "Jenna" and "Christina").

## Header (map, setup and teams)

* **What**
  * **Map name and description** are each a 16-bit length and ASCII text. The name starts at byte 95 in HotA saves and byte 58 in SoD and RoE saves (the 16-bit length is at that byte, the text two bytes later). Some names keep a leading space (`" The empire of the world"`).
  * **Map difficulty** is the byte after the description, 0 Pawn to 4 King. It is separate from the game difficulty of the setup block. The byte after it was 0 everywhere.
  * **Map size** is a 32-bit integer five bytes before the name (36, 72 and 144 seen). The byte before the name is read as the underground flag (1 in nearly every sample, 0 only in one).
  * **Human flags**: eight 32-bit values in colour order (1 = human), followed by the colour whose turn it is, then a marker: `FF 10` eight bytes later in HotA, `FF 00 00 00` right after in SoD and RoE (+36 from the flags; offsets 1130, 1228, 1262 and 1286 seen, so the block is located by its shape). The header's one-byte-per-colour list (`FF` not in game, `0A` computer, other = human) only names the first human of a hotseat game.
  * **Whose turn it is** is a 32-bit colour index (0 Red to 7 Pink) at the flags +32, and it is always a human. When the turn passes it is the only value of the setup window that changes. The new-day block does not depend on it.
  * **Teams** (SoD and RoE; not found in HotA saves): in the header, just before two lists `00 01 02 03 04 05 06 07`, a byte with the number of teams, and when it is not 0, eight bytes with the team of each colour (Red, Blue, Tan, Green, Orange, Purple, Teal, Pink). With no teams the byte is 0 and there are no team bytes. A map with three teams reads `03 | 00 00 01 01 01 02 00 02`; a colour nobody plays keeps the default 0.
* **Editor**
  * The editor writes the turn (`SafeTurnSerializer`, the Order of play card), and the game starts the new player's turn cleanly (tried in the game).
  * The stride of the human flags for colours other than Red and Tan is assumed.
  * Non-ASCII map names would break `MapHeaderLocator`.
  * Teams are not read yet.
* **Not yet known**
  * Whether the underground flag is right on a map with an underground (the least certain item here).
  * Whether anything else goes with the turn: a turn change also changes the selected hero of both players in one save pair.

### Supporting savegames

* `players_human_red`, `players_human_tan`, `players_human_red_and_tan` (Hota): the human flags.
* The 21 consecutive pairs of `Hota/Mines`, `Hota/Banishing`, `Hota/Dwellings`, `Hota/VisitedPlaces`, `Sod/Hotseat` and the `players_human_*` saves: only the turn changed in the setup window.
* `Sod/Hotseat` `five_teams_00_game_begin` to `five_teams_02_blue_two_obelisks` and the hotseat map: the team table.

## Players

* **What**
  * The struct of a player starts `PlayerLayout.HeadLength` (97) bytes before its first resource, and a player chunk is the whole struct (145 bytes in SoD, 149 in HotA). The head is the same for every colour and in SoD and HotA. From the start of the chunk:
    * 0: number of heroes. 1: the selected hero (`FF` for none). 2 to 9: the eight places for hero ids, `FF` for an empty one; a garrisoned hero is not in the list.
    * 10 and 11: the two heroes of the tavern, by hero id. The computer's taverns change both heroes at the start of a week and, within a week, only the place of a hero the computer hired.
    * 22: **the days left before the player is banished for owning no town**: `FF` while the player owns a town, 7 the day the last town is lost, one less each day, 0 once banished. Regaining a town sets `FF` at once. It has the same meaning as HoMM2's 0x304.
    * 23: number of castles. It follows ownership: it goes up or down by one with each castle gained or lost.
    * 24: the selected castle (a global castle id, not a position in the list; `FF` for none), exclusive with a selected hero.
    * 25 to 96: 72 indices of the player's castles in the table of castles, `FF` in unused places. New acquisitions are appended; losing a town removes it and compacts the remaining list without sorting. Example: with towns 0 and 1 Red's and 2 Blue's, Blue capturing 1 then 0 gives `[2,1,0]`; Red retaking 1 leaves Blue `[2,0]`.
    * 129 (Red): becomes 1 with the first visit to a magic spring and stays 1 on the other square and after the week change. It is not the visit mark, which the game keeps per square, probably with the map objects.
    * 19 changed from 03 to A7 when a hero dug.
  * Another player's struct changes only in bytes 1 and 24 (selected hero and town) when resources are traded.
  * **Who is in play.** A save has a struct for every colour. A colour is in the game when it has resources, heroes or a town and the setup does not say "not in the game" for one without heroes or towns. A colour that is not in the game can still have 10000 gold in the table, like a computer player never given a start. A human with two heroes and no resources counts through its heroes. A slot with the marker but no valid colour byte (an unused last slot) is named after the colour that follows the previous one.
  * The byte in front of the marker (`PlayerLayout.FlagOffset`, 44 bytes after the resources) is 1 in most players and 0 in some; what makes it 0 is not known.
  * **Banishing** also changes bytes the editor does not write: the banished player's hero chunk gets owner `FF` and an emptied army, a counter and an 8-byte record (`02 00 00 00 00 FF 01 08`) appear late in the file, and header bytes near 1313, 1356 and 12492 change.
* **Editor**
  * The hero list and the count are written together, and only when the places do not hold the model's ids; an owner edit changes them through `PlayerListsReconciler` (see "Table of hero owners").
  * Giving a castle to another owner follows what the saves show: the player who loses the last town gets 7 at once, a player who gains a town while counting gets `FF` at once, and losing one town of several or gaining one while not counting leaves `FF`. A banished player (0) is shown but cannot be edited. The towns a player owns are counted from the owners of the castles. HoMM2's castle owner change applies the same rule (`PlayerModelBase.DaysLeftOnGainingATown` / `DaysLeftOnLosingATown`).
  * The castle count is written with the list when a castle owner changes. Town selection is deliberately left alone. Returning an original town restores its original-relative position, so resetting or reverting ownership restores the loaded save byte for byte; it does not simulate a new in-game capture.
  * The loader does not look at the byte in front of the marker (it once did, and found only the four players after Blue: Red and Blue had no page).
  * The table of hero owners marks the heroes the taverns offer with `40`, and the tavern page does not write it, so a tavern hero set by the editor is not marked as one the game chose. Editing the tavern was tried in the game and works.
* **Not yet known**
  * What byte 19, byte 129 and the byte in front of the marker mean.
  * What the other bytes banishing changes are, so a banished player cannot be edited and the editor cannot banish or revive a player.
  * Whether the game changes tavern places for another reason (one first place changed from 160 to 2 without a hire).

### Supporting savegames

* `Hota/Banishing`: Blue loses his last town, regains one, loses it again and is banished (`PlayerBanishingTests`); `banishing_00_baseline` to `banishing_01_blue_7_days_left` and `banishing_04_m1w1d2` to `banishing_05_m1w1d3` (7 days at once), `banishing_01_blue_7_days_left` to `banishing_02_m1w1d1` (`FF` at once); `banishing_14_ban_count_start` to `banishing_16_m4w2d3` (the byte in front of the marker 1 for Red and 0 for Blue).
* `Hota/TownOwnership` (twelve hotseat saves): the order of the town list and the selected town (`town_ownership_09_blue_captures_red_inferno__selects_red_tower`: Blue's selected town 0 sits in the second list position).
* `castles_00_baseline` to `castles_01_blacksmith_garrison_tumlac_to_blue` (Hota), `roe_63_tazar_idle_of_fortune` to `roe_64_elfwind_captured` (Roe): the castle count follows ownership.
* `SodSeriesTests` saves: the computer's tavern.
* `tan_not_parsing`: a colour not in the game with 10000 gold; the colour that plays is Teal (confirmed in the game).
* `sleep_garrison_visiting_01_akka_visiting` to `sleep_garrison_visiting_03_akka_asleep` and the hotseat saves: a human counted through its heroes.

## The table of hero owners

* **What**
  * A byte per hero id, in the order of the ids: the colour of the player who has the hero (0 Red to 7 Pink), `FF` for nobody, and `40` for a hero the game keeps from being drawn for a tavern (the two heroes of every tavern, the campaign heroes 145 to 155, and a hero disabled in the map editor).
  * It is 156 bytes in SoD (the `Roe` saves are SoD saves) and 215 in HotA. A second table of as many bytes, all `FF`, follows, then `00 08`, two 16-bit numbers that vary, a zero byte and the 7F marker of the date. So the table is found from the date (`date - 11 - 2 * length`) and checked (every byte `FF` or a colour with or without `40`, the second table all `FF`, the tail `00 08`); it is never searched for. In the HotA table, heroes 205 and 210 have bytes in the second table (210 is `40`).
  * The table says the owner byte of every hero chunk, every hero of a player's list has the player's colour, every tavern hero has `40`, and an owned hero that is in no list is garrisoned in a castle (a hero hired when the list was full).
  * A disabled hero (one disabled in the map editor, or a campaign hero) has the reserved entry `40`. A new game from the same map with the hero enabled has the entry free, and that entry is the only one that differs. A disabled hero can still be hired: put in a tavern by the editor (entry free) she was hired in the game, and after a retreat she was back in the tavern with the game's own reserved entry, on the tile she retreated from.
  * Hero 3 has `41` in some saves, although it is neither owned nor offered in a tavern; other saves have `FF` there.
  * **A defeat**: the hero's owner byte becomes `FF` and it leaves the map; in the player's record the count drops, the id goes, the later ids move up a place and the freed places become `FF`; if it was the selected hero, the first hero left is selected; the table's byte becomes `FF`. The tavern bytes do not change. A defeated hero goes back to the pool and can be hired by another player.
  * **A hire**: the id goes to the first free place after the others (after the compaction of the heroes lost in the same turn), the count goes up, the selected hero does not change; the table's `40` becomes the player's colour; the tavern place is refilled by a hero the game draws, which gets `40` in the table. A hire when the list is full goes into the garrison: the table says the player, the list does not have the hero.
  * **A player knocked out of the game**: its tavern bytes become `FF FF` and its two tavern heroes' `40` becomes `FF`.
* **Editor** (`HeroOwnersLayout`, `HeroOwnersModel`, `HeroModel.WithOwner`, then `PlayerListsReconciler`, in one step of the history)
  * The table is read as reserved/unowned and the original byte is kept while the model still says that. It is only read for HotA and SoD saves; for RoE, AB and Chronicles no table is read and an owner edit changes the hero and the lists only.
  * The table and the lists are two records and the editor never assumes they agree: a hero the table gives to a player but no list has is still taken from the player (the table becomes `FF`, the lists are left); a save with no table has the hero and the lists changed alone.
  * **To nobody** (a defeat): the hero goes off the map, the old owner's list loses it and the table says `FF`, as the game does. The editor leaves a selected hero alone unless the selected hero goes.
  * **To a player** (a hire of a hero that is in no tavern): the id goes to the end of the new owner's list and the table says the colour; from another player, the old owner loses it as in a defeat. Giving a hero to a player and its position: a neutral hero given to a player keeps no tile, and the game crashes on a player's hero without a valid tile, so the hero page warns until a position is set.
  * **Refused** (`GameRuleException`, nothing written): a hero a tavern offers (the game refills the place with a hero it draws, which the editor cannot do); a hero the table keeps out of play (`40` with no tavern, the campaign heroes and hero 3); a player whose list has 8 heroes; a hero garrisoned in a castle.
  * Reverting the owner change, resetting the hero page, or giving the hero back by hand puts the hero's owner, its place in every list, the selected hero and the table back together (a hero that comes back takes its old place). Putting a castle back gives the players who own the towns the savegame gave them their days left back.
  * The editor does not write the reserved entry when it puts a hero in a tavern. The tavern page offers only heroes no player owns, and never the same hero twice.
  * Heroes that are disabled in the map editor or are campaign heroes have a Disabled heroes group under Neutral in the tree and a tab on the Neutral page; they are not given to a player.
  * Changing a hero's owner was tried in the game and works.
* **Not yet known**
  * How the owner change and the defeat of a garrisoned hero are stored (the castle's garrison byte).
  * What the second table is, and the `41` of hero 3.
  * Whether the game ever draws a campaign hero for a tavern by itself (none appeared in about 134 hires from a pool of 188).
  * How "does not come back until the others have been seen" is stored.

### Supporting savegames

* `Hota/HeroOwners` (`hero_owners_00_baseline` to `hero_owners_06_seven_new_blue_heroes_defeated`, hotseat saves of a six-player game; `HeroOwnersBytesLoaderTests`, `SafeHeroOwnersSerializerTests`, `PlayerListsReconcilerTests`): defeats (`00` to `01`, `02` to `03`, `04` to `05`, `05` to `06`: seven heroes of one player at once, past the 200th byte of the table), hires (`02` to `03`, `04` to `05`), a player knocked out (Pink in `04`), a hire into the garrison (hero 29), the table position (543453 in `00` and `01`, 543492 from `02`). Every table in `TestData` is written back byte for byte, and the table loader was checked on all 482 HoMM3 saves.
* `disabled_heroes_00_baseline` a hero disabled in the map editor.
* `Hota/DisabledHeroes`: a disabled hero hired and retreated.

## Castles

* **What**
  * The layout (owner, faction, position, shipyard tile, garrison, hero ids, name, dwelling counts, Mage Guild counts, and the built, current and enabled masks) is the same in HotA and SoD.
  * **Buildings** are one bit each in a bitmask, with an "enabled by the map" mask beside it. A horde building, an upgrade or a unique building that replaces another is cleared in the current mask but stays in the built mask (Miners' Guild replaces the Dwarf Cottage, Cage of Warlords the Gnoll Hut, Capitol the City Hall).
  * **Faction codes**: Inferno 3, Necropolis 4, Dungeon 5.
  * **Bits the game sets by itself**: 19 and 25 (the upgraded growth buildings), 22, 23, 27, 28 and 29. Companion bits: town hall 27, city hall 28, capitol 29 in Rampart, Necropolis and Conflux; fort 27 in Bulwark, Factory and Stronghold; fort 22 in Conflux. Bit 23 is set with the shipyard in a Castle and with the fort in a Fortress, and is already set in Rampart and Necropolis (also in a village-hall-only town); its cause is unknown.
  * **Mage Guild spells** follow the castle chunk (from the end of the building masks): five lists of 32-bit spell codes, 24 bytes apart (level 1 at +0, level 5 at +96), -1 for a level the faction cannot build. They are picked when the map starts and are the same in every save, built guild or not. The guild shows the first count entries of each level (five count bytes in the chunk). In RoE and SoD the entry after the visible ones is the Library's extra spell (Tower only); in HotA the slots after the visible ones are empty (0, which is also Summon Boat) or hold other data, so the count must be read exactly.
  * **Growth buildings**: bits 19 and 25 ("Upg. Griffin Bastion", "Upg. Miners' Guild", ...) are set when a growth building and the upgrade of its dwelling both exist. The growth building works on both dwellings and has no upgrade.
  * **Shipyard**: only coastal castles can build one; it stays disabled in the mask for an inland castle.
  * Stronghold and Fortress stop at Mage Guild level 3 and Castle at level 4. In Fortress, Captain's Quarters is the unique building (bit 17) and Cage of Warlords the horde building (bit 18). In a garrison the strongest creature is slot 1; every castle has seven slots.
  * **Captured castle**: only the owner byte changes and the neutral garrison is gone; the player's castle count grows.
  * **Dwelling growth** (Conflux, SoD): 20, 6, 6, 5, 4, 2, 2 a week, and 60, 18, 18, 5, 4, 2, 2 (110, 30, 30, 15, 12, 6, 6 upgraded) once the buildings have stood for a week. The first numbers are the growth of the creatures, which is what a dwelling holds when it is built. Building the upgraded Magic Lantern gives the upgraded Garden of Life by itself (bits 19 and 37). The Conflux dwellings are the Magic Lantern, the Altars of Air, Water, Fire, Earth and Thought, and the Pyre. At a new week the castles' weekly growth appears. Equipping Head and Arms of Legion changes nothing in a castle; the growth is worked out at the week change.
  * **Garrisoned and visiting hero**: the castle holds both ids (`CastleLayout.VisitingHero`, bytes 65 and 66 in HotA). See "Heroes".
  * The byte after the Mage Guild level is not always zero: neutral SoD and HotA towns have non-zero values there (`73` and `57` seen), preserved through capture; its meaning is unknown.
  * Per faction and game: Rampart's Grail building is bit 26; Conflux's Grail is the Aurora Borealis (bit 26) and the Vault of Ashes is bit 24 with the game-set upgraded bit 25 beside the upgraded Pyre. Bulwark (HotA) has no Mage Guild level 5 (bit 4 is never enabled), dwellings at bits 30 to 36 and upgrades at 37 to 43; its special buildings Fair 17, Hay Bins 18, Sieidi of the Runes 21, Altar of the Runes 22 and Spirits of the Forebears 26 were named by elimination (the bit numbers and the hide/replace behaviour, Hay Bins hides the Mammoth Stalls and Altar of the Runes replaces bit 21, are confirmed); Bulwark's shipyard bit 6 rests on the standard numbering.
  * **Prerequisites** come from VCMI. A chronological series can show that one is too strict, but not that it is right.
* **Editor**
  * Every castle writes back to identical bytes. The editor writes the companion bits that go with a building (`SafeCastleSerializer.CompanionBits`) and leaves the others as the game wrote them; bit 23 is not written. Whether the game cares when it is missing was not tested.
  * It keeps the growth-building bits consistent (`CastleBuilder.IsAutomatic`) but does not list them.
  * The castle page shows the weekly growth as "+N a week" (`CastleModel.BaseWeeklyGrowth`); what the castle, the horde buildings and the Grail add is not shown.
  * A castle's discovery starts after the located map header, to avoid mistaking its title for a town, and checks owner, faction, garrison and Mage Guild counts.
  * The built-building edits of the editor reproduce the saved series step by step (`CastleBuilder`).
* **Not yet known**
  * Whether the map's allowed spells and secondary skills are stored.
  * The cause of bit 23, and whether the game cares when it is missing.
  * What the castle, the horde buildings and the Grail add to the growth. In one Stronghold visit the castle was ahead of the artifacts' bonus; this is unexplained.
  * RoE, AB and Chronicles layouts (not seen), and the Capitol of some factions.

### Supporting savegames

* `Roe` (51 saves of the RoE map with the eight RoE factions built one step at a time; `RoeBuildingEditsTests`) and the Conflux series (`SodSeriesTests`): reproduced by `CastleBuilder`. `other_factions_editor_dung_necro` was built in the map editor, which ignores prerequisites, so it proves bit positions only.
* `sod_game_00_baseline` (four neutral SoD towns with `57` after the Mage Guild level), `Hota/MerchantStock` baseline (two neutral towns with `73`).
* `Hota/castles_*`, `Hota/TownOwnership`: castle capture and ownership.

## Heroes

* **What**
  * A hero record is the chunk plus a tail: in SoD the chunk is 1082 bytes and the record 1094, so 12 bytes follow (`HeroBytesLoader` includes them in the SoD chunk). HotA has twelve bytes there too, laid out differently (`D0 07` in the second and third byte, `FF` in most of the rest), and they are already inside its longer chunk.
  * **Moved today** (HotA): byte 1095 is 1 once the hero has moved and 0 after a new day. SoD does not store it: no byte of the record separates a hero that moved from one that did not, except the movement points. A move changes only the position and its copy, the object under the hero, the facing and the movement points. The game probably compares current with maximum movement points (a guess). Byte 1095 is past the end of a SoD record (1094 bytes), so it would read the next hero. A list of path steps appended at the end of the file carries the hero's object id and survives a new day, so it is the remembered route, not a daily flag.
  * **Primary skills** hold what counts: the hero's own skill with the bonus of the equipped artifacts already in it (the game changes the bytes when an artifact is put on or taken off, as in HoMM2). A hero never holds less than its bonus. Edric has 2/2/1/1 bare and 11/11/9/9 with Titan's Thunder (+9/+9/+8/+8); Christian has 5/5/4/7 with Armageddon's Blade (+3/+3/+3/+6). The skills a hero learned are the saved ones minus the bonus.
  * **Active adventure spells**: bytes 31 to 35 hold the adventure spells cast today: 31 Disguise, 32 Fly, 33 Water Walk, 35 Visions; 255 is not cast, 0 is cast, a new day resets them to 255; 34 is always 0. They are cleared by any visit to a town. Flying and water walking from artifacts are not stored on the hero. Right on HotA and SoD; unverified on RoE and AB.
  * **Available spells** are the learned ones plus those given by equipped artifacts. Tomes also give Visions and Magic Arrow, which belong to no school.
  * **Dig**: the current movement points become 0 and the hero stands on an object of type 124 (a hole). The tile is added to the list of dug tiles (see "Date and map objects"). Whether a dig finds the Grail is not in the save. After a successful dig the first backpack slot holds id 2, the Grail.
  * **Standing on an object**: +11 is the 32-bit type of the map object under the hero (98 town, 57 obelisk, 38 Idol of Fortune, 28 Faerie Ring, 83 Seer's Hut, 124 hole; 0 on bare ground), +15 is 1 when it can be visited, +16 the index of the object among those of its type (the castle index for a town, the obelisk index for an obelisk).
  * **On the map**: +6 is 1 for a hero on the map (a visiting hero too) and 0 for one inside a castle or gone (fled, defeated). Garrisoning changes nothing else but the active spells and spell points.
  * **Asleep** (SoD and HotA): byte +1087 is 1 for a hero put to sleep and goes back to 0 when it moves. In SoD it is in the tail, in HotA inside the chunk. A garrisoned hero keeps the flag. The computer's guards (heroes it garrisons or parks in a castle) have it set.
  * **Bonus effects** (until the next battle, or the next week for some): the byte is read, the bit masked, non-zero means on.
    * +28: morale bonus, +29 luck bonus, signed bytes (watering hole +1, crypt -1; swan pond +2, fountain of fortune +1).
    * +135: `0x02` stables (lasts the week), `0x08` swan pond, `0x40` watering hole. +136: `0x04` crypt, `0x20` Faerie Ring. +138: `0x08` Fountain of Fortune, `0x02` Idol of Fortune.
    * A battle clears luck, the fountain bit, and the Faerie Ring and Idol bits; a new week clears the stables bit. Visiting again after the effect ended grants it again, so the "visited" table beside the date is not what decides.
    * **The stables also add movement points**, to the maximum of the day and to the points left (the bit is only the memory of the visit): +400 in Shadow of Death (2600/2600 became 2418/3000 after walking) and +300 in Horn of the Abyss (3175/2600 became 3390/2900 after walking). Restoration of Erathia, Armor of the Damned and Chronicles use the Shadow of Death value, which is not checked there.
    * Unnamed and left alone: `+135 0x10` (set when the ring and the idol were both visited in one turn, gone after a battle) and `+138 0x01` (set at the first castle visit, survives battles and new days). The effects of other objects (Oasis, Temple, ...) were not looked at.
  * **Destination** (the tile the game marks on the map as the end of the hero's path): two 32-bit numbers, x at `+45` and y at `+49` (`FFFFFFFF` for none), then the map level in 16 bits at `+53`.
  * **Fleeing or defeat**: the owner byte becomes `FF`, +6 becomes 0, and the `0x40` bit of +1088 is set. The bits of +1088 are not understood otherwise (a hero can have `0xCE` all the time), so it must not be read as "not on the map". A hero that fled takes the second place of its player's tavern; one that was hired again and defeated is not in the tavern.
  * **Garrison, visit and swap**: the castle holds the garrisoned and the visiting id (`FF D3`, `D3 FF`, `D3 C7`, ...) and a swap exchanges them. Each hero's +6 is 1 for the one at the gate and 0 for the garrisoned one; a swap flips both and changes nothing else, except that the hero coming out changes its facing (+41). The player's list of heroes holds the hero at the gate only (selected hero too). The sleep flag, the moved flag and the active spells do not change in a swap. So a swap changes four chunks (castle, two heroes, player).
  * **A hero with a biography**: the 26-byte hero header (position, gender at 20, the 32-bit biography length at 22) and the biography text sit in front of the hero chunk, because the chunk starts 26 bytes before the faction byte, inside the end of the biography. Names are not unique (heroes or castles can share a name), so regions are matched to chunks by position, not by name.
* **Editor**
  * Byte 1095 is used for HotA only.
  * The bonus effects are edited on the General page ("Visited buildings", the shared next-battle card) and written by `SafeHeroSerializer`: the two signed bytes and only the known bit of each building (the other bits are left as they are). The watering hole, the swan pond and the crypt set their bit and the byte together; the stables set their bit and add their movement (`HeroModel.WithVisitedBuilding`, `movement` / `versionMovement` of `visitEffects.json`). The faerie ring, the fountain of fortune (a luck the game draws) and the idol of fortune are shown but not edited, as the saves do not pin down what they give. Tried in the game: morale and luck, the watering hole, swan pond, crypt and stables.
  * The editor's table of what each artifact adds to the skills was checked against the game extensively; the stepper edits the base value without artifacts.
  * The Grail in the backpack (`Artifact.CarriedInBackpack`) is never offered by the finders, kept and shown, and can be taken out.
  * The destination is editable on the General tab (tried in the game).
  * `SaveRegionProvider` drops the chunk fields below offset 26 for a hero with a biography and uses `HeroRegionProvider.GetHeaderRegions`.
  * A swap of the garrisoned and the visiting hero is not an edit yet; a hero leaving a castle was never seen.
* **Not yet known**
  * The 4282-byte block that appears on a new day, and a hero that moved and came back to full movement points.
  * The bits of +1088 other than the flee/defeat bit, and where "does not come back until the others have been seen" is stored.
  * The unnamed bytes (see "Unnamed bytes of the hero chunk").

### Supporting savegames

* `Hota/Arts` (`arts_00_before_arts` to `arts_12_all`): primary skills with artifacts, active spells (`ActiveSpellsTests`), moved today (`MovedTodayTests`); `HeroPrimarySkillsTests` over the 356 test saves.
* `SodSeriesTests`, `sanya_01_moved` to `sanya_05_moved_again`, `roe_56_tazar_no_move` and `roe_57_tazar_moved`: SoD does not store moved today (847 heroes checked, 43 with fewer movement points than their maximum).
* `hotseat_05_tazar_swan_pond` to `hotseat_06_sir_mullich_stables` (Sod/Hotseat) and `Hota/Stables` (`stables_00_baseline`, `stables_01_tazar_visits_stables`): the stables movement.
* `Hota/SleepGarrisonVisiting`: sleep flag, garrison, visit and swap.
* `roe_58_haart_dug`, `roe_76_tazar_before_dig` and `roe_77_tazar_grail_found`: the dig and the object under the hero; `roe_63_tazar_idle_of_fortune`, `roe_68_tazar_obelisk` and `roe_70_tazar_post_battle`: the object types under the hero and the effects after a battle.

## Obelisks

* **What**
  * 53 bytes before the start of Red's chunk (Red's struct, in play or not) there is a marker `FF 00 00 NN` (NN the number of obelisks of the map), then NN bytes in the order of the obelisks of the map, not of the visits.
  * **The flag is a bit mask by colour**, bit n for the colour with index n (Red 0, Blue 1, Tan 2, Green 3, Orange 4, Purple 5, Teal 6, Pink 7). A visit sets the bit of every colour in the visitor's team: with Red and Blue in team 1 and Teal (default) there too, Red's visit gives `0x43`. With five teams (team table `05 | 00 01 02 03 04 04 00 04`) Red's two visits give `0x41` (Red and Teal, which shares team 0), and after Blue visits the same two they are `0x43`. What another team visits does not count for a player. With one human and no teams only `1` was ever seen.
  * NN is the number of obelisks of the map: the distinct obelisk numbers found in the map objects (type 57) are NN, or NN - 1 when a hero stands on one and hides it.
  * The marker is not always there: on a map with no obelisks other data sits in its place; then nothing is shown. Nothing seen displaces the flags of a map that has obelisks.
  * **Coordinates** of an obelisk come from the list of map objects: after the table of object definitions and a 16-bit count there is a (definition, x, y, z) entry for each object, and the record of an obelisk holds its place in that list. A hero standing on an obelisk hides its record, so it has no coordinates then.
* **Editor**
  * `ObeliskChunk` / `ObeliskVisits`, edited on the Obelisks tab of the player page; `ObeliskBytesLoader` returns nothing when the marker is missing and the tab is not shown.
  * The editor edits one colour at a time; marking a whole team needs the team table. An edited obelisk mark was checked in the game.
* **Not yet known**
  * Where the visits of other players are, and anything the puzzle map needs besides the flags (its pieces seem to follow the number of visited obelisks).

### Supporting savegames

* The RoE series, the hotseat map and `five_teams_00_game_begin` to `five_teams_02_blue_two_obelisks` (visited flags and teams; flags were only seen to change on these maps); `ObeliskVisitsTests` over every save with the marker.
* `sod_day_02_m1w1d2` and `sod_day_03_m1w1d3` (Sod): maps without obelisks and no marker.

## Date and map objects

* **What**
  * The date (`day`, `week`, `month`, 16 bit each) follows the marker `7F xx 02 yy`. `xx` is 1 until the Grail is dug up and then 0.
  * `yy` is a strong candidate for a cheat-used flag: after entering only the resource cheat it went from 0 to 1, and it was also seen becoming 1 during a castle visit with a garrisoned hero (that comparison did not isolate the cause). The hero-adjacent byte at 300792 stays 0, so it is not a flag of the resource cheat.
  * Other data that looks like a date occurs in some saves (week 4 day 3 read as month 2 week 4 day 5, month 60), so a match with the marker is preferred. Some HotA saves have no marker and use the first match.
  * A table of two bytes per object type seems to start at +12 from the date: the byte at +68 is set once a Faerie Ring (type 28) was visited and +88 once an Idol of Fortune (type 38), at 12 + 2 x type, and they stay set (seen for two types only). The 7 values at +38, +42, ..., +62 and the 20 flags from +107 were constant.
  * **Dug tiles** (sure): a list of 5-byte entries (x, y, level, 16-bit number) in front of a message text ("Uh oh... devil tracks in the grass") that is in every save; entries stay. A dig also adds a map object; `AVLholr0.def` appears only when the Grail was found.
  * **Visited objects** are not recorded on the object: the cell of a visited Idol or Faerie Ring reads as before once the hero left. The cell of an object has its type, its number and the hero standing on it (`FF FF FF FF` for none).
  * **Fog of war** is a long list of 00 and 01 bytes after the map (changes with every step) and a movement trail (15 bytes per step: hero id and tiles) is appended at the end of the file. Neither matters for editing.
* **Editor**
  * `GameDateLocator` prefers a match with the marker; the Conflux saves were wrong before.
* **Not yet known**
  * Whether `yy` persists, changes for other cheat types and affects scoring.
  * The number of a dug tile (154 and 536 seen) and the table of two bytes per object type.

### Supporting savegames

* `Hota/MineFinding/TwoMinesThreeDwellings` (original file names in its README) `two_mines_three_dwellings_05_red_captures_automaton_dwelling` (originally `dwe_3.GM1`).
* The Conflux saves (`conflux_*`, Sod) and the HotA `empire_of_the_world_*` and `new_day_*` saves: date detection. `roe_58_haart_dug` and `roe_77_tazar_grail_found` (dug tiles) and `roe_63_tazar_idle_of_fortune` (Idol of Fortune): the object tables.

## Mines

* **What**
  * A count precedes **62-byte owner/guard records**: a little-endian uint32 in HotA, one byte in SoD (assumed for RoE, AB and Chronicles). A single record is valid, and (0,0) is a valid tile.
  * The record starts at **owner +0** (FF none, 0 Red, 1 Blue, ...), **kind +1** (0 sawmill, 1 alchemist's lab, 2 ore, 3 sulfur, 4 crystal, 5 gems, 6 gold), **abandoned +2**, **seven uint32 guard creature codes at +3** (FFFFFFFF none), **seven uint32 counts at +31**, then **x, y, level at +59, +60, +61**. The table contains kind-100 objects too (on the old test map, 19 mines and two kind-100 records), so the whole counted table is used, not "the last mine".
  * The table order is not the order of acquisition.
  * An abandoned mine keeps its record when it is taken: the owner is set, the kind is the resource it turned into and the guard is empty. Taking a mine changes only its owner byte (and the guard if it was fought for). The income is not stored.
  * The record tile and the sprite/entrance tile differ on some maps (a mine saved at (3,8) has its sprite at (4,8)); the record tile is not automatically the entrance.
* **Editor**
  * Every counted record and the generator table that follows are validated before any mine is exposed. Contained suffix candidates are discarded and competing tables are rejected.
  * Owner and guard edits are written to the local object records; they were tried in the game for owner and guard.
  * The saves of `TestData/Roe` identify as SoD, so they do not establish original RoE support.
* **Not yet known**
  * The entrance tile of a mine; it depends on the map and no blanket adjustment is right.
  * The count width in RoE, AB and Chronicles.

### Supporting savegames

* `Hota/Mines` (Red takes a mine a day, Blue takes it back the next turn), `Hota/MineFinding` (22 small-map saves with zero, one or two mines and zero to three dwellings), the SoD corpus: capture, owner and guard changes. Detection over all 482 saves kept every earlier mine owner and found the formerly missed first mine; `TwoMinesTwoDwellings` shows the table order is not the acquisition order.
* `Hota/MineFinding/TwoMinesThreeDwellings` (original file names in its README) (`two_mines_three_dwellings_00_baseline`, originally `begin.GM1`: count 2 at decompressed offset 240477, records at 240481 and 240543, generator count at 240605).

## Places a hero visited

* **What**
  * **A hero remembers the places of the permanent bonuses itself**, in its record, one 32-bit number for each kind of place. **Bit n is set when the hero visited the place whose number among the places of its kind is n** (hero +16, the number of the object among those of its type, is that number while the hero stands on it; +11 is the type). Each hero has its own bits. HoMM2 works the same way.
  * The bytes (lowest byte of the number) and the object types: **+79 learning stone (100)**, **+83 Marletto tower (23)**, **+87 garden of revelation (32)**, **+91 mercenary camp (51)**, **+95 star axis (61)**, **+99 tree of knowledge (102)**, **+103 library of enlightenment (41)**, **+107 arena (4)**, **+111 school of magic (47)**, **+115 school of war (107)**; +119 was not tried. In the Horn of the Abyss tail of the record: **+1098 colossus of the magi** and **+1114 gazebo** (both are objects of the type 144).
  * Where each place is: the cells of the map name the type, the place in the object list and the number among the type (as for the obelisks), so the number of a place gives its position.
  * Visiting an adventure-map artifact merchant (Black Market) changes nothing in the hero or the stock.
  * A tree of knowledge also changes +231 and +1141 (Tazar's) and the level (+75) goes up by one; the payment each tree asks (gold, gems...) is not stored in the hero.
  * The changes at +1074, +1075 and +1081 seen in an older comparison are combination-artifact reserved-slot counters, not visit marks.
* **Editor**
  * The hero page has a Places tab, like HoMM2's (`VisitedPlacesCard`, `VisitedPlacesViewModel`): each kind of place with how many the hero visited, None and All, and each place of the map by its position to forget one by one. The places of a kind are found from the cells of the map (`MapPlacesLoader`) and from the heroes that stand on them (a hero hides the cell of the place it stands on). The colossi and the gazebos are listed without their places (they share the object type 144 with other Horn of the Abyss objects and their cells carry no number); the trees of knowledge are listed with theirs. A kind with no places can still be opened and shows nothing.
  * Forgetting and remembering places was tried in the game.
* **Not yet known**
  * The mineral spring (type 144, subtype 4: +1 luck and +600 movement points until the end of the day): where the hero keeps that it visited one.
  * The byte +119, and which of the type 144 objects is the colossus of the magi.

### Supporting savegames

* `Hota/VisitedPlaces`: Tazar and Orrin visit, a turn apart, two places of each kind (`visited_places_05_tazar_school_of_magic_1` for a hero standing on a school of magic; `visited_places_23_tazar_tree_of_knowledge_1` and `visited_places_24_tazar_tree_of_knowledge_2` for the trees of knowledge; `visited_places_27_tazar_artifact_merchant` to `visited_places_29_tazar_black_market_first_4__orrin_artifact_merchant_last_4` for the Black Markets).

## Objects of the map (positions and cells)

* **What**
  * **In the list of objects (a definition number and x, y, z for each) the position of an entry belongs to the object whose definition is in the next entry.** A hero standing on school of magic 0 is at the position of entry 791 and the definition of entry 792 is the school; the cells of the map that give a place number point at the entry with the position. So the position of the object a cell names is the position of that entry, and the type of that object is the type of the next definition.
  * **Definitions have a type and a subtype**: of the 33 bytes after the file name of a definition, the type of the object is at +26 and its subtype at +28. The Horn of the Abyss objects that are not in the older games share the type 144 and tell themselves apart by the subtype: `AVXgazeb` (gazebo) is 6, `AVXmspr0` (mineral spring) is 4, `AVSmgarn` 2, `avwtplce` 3, `AVShrmit` 5, `avmoilpm` 8, `avxtrmtw` 9, `avtprosp` 10, `avstrblz` 11, `AVStmplt` 0.
  * **The cell of a tree of knowledge** (`66 00 00 00`, place, number, `20 05 00`) holds more than the number: the byte of the number is 0 or 1 before a visit and 0x60 or 0x61 after it, the three bytes after it are the payment (`20 05 00` is 2,000 gold at first, `2C 05 00` after a visit), and a hero standing on a tree reads 0x20 or 0x21 for its number. The number of the place is the low five bits (`VisitedPlaceKind.NumberMask`). Both trees of the test map are found this way (0 at 2, 20 and 1 at 2, 23).
  * **A cell is written for every tile of an object**, and the cells of one object do not always agree on the number (a merchant has 2 on its first tile and 1 on another): the first cell of a place with a valid number is the one to take.
* **Editor**
  * `MapObjectPositions` gives the position of the object a cell names.
* **Not yet known**
  * Which type 144 object is the colossus of the magi: the cells of the `AVStmplt` objects are the only ones with the type 144 and a number of `FFFFFFFF`, and a hero standing on a colossus reads 144 and the number 0 or 1, but no position of a hero ever agreed with a definition of the type 144.

### Supporting savegames

* `visited_places_05_tazar_school_of_magic_1`, `visited_places_23_tazar_tree_of_knowledge_1`, `visited_places_24_tazar_tree_of_knowledge_2` and `multi_merchants` (Hota).

## Black Markets and shared town Artifact Merchants

* **What**
  * The 32-bit number in front of the first list of seven artifacts is **the number of Black Markets**, not a marker. That many lists of seven uint32 slots follow, one for each Black Market. A Black Market's number is the number among the objects of type 7 (`07 00 00 00`, place, number). Each purchase clears exactly one entry of one list to `FFFFFFFF`. The Black Markets of the baseline test map are at (2,8), (5,8), (8,8), (11,8), (14,8), (17,8), (20,8) and (23,8) (definition 79, `AVXmktb0.def`, type 7), with object indexes 152 to 156 and 158 to 160. Every initial list contains three treasure, three minor and one major artifact; this does not show which replacement classes the game accepts.
  * Some saves have two independent seven-slot lists that look like one town list and one Black Market list: they are two Black Markets.
  * **Town Artifact Merchants** use a different stock, shared by all towns and players, not one of the Black Market lists. The seven uint32 slots start **38 bytes after the saved date** (uint16 day/week/month, then 32 zero bytes). The recognized date prefix is `7F flag 02 flag`, with each flag 0 or 1. Buying an artifact in one town clears it in all of them (Head of Legion disappearing in every town after one purchase). The date and stock relationship holds in 488 saves.
  * Five HotA `empire_of_the_world` / `new_day` saves have another date prefix and no recognized town stock.
* **Editor**
  * `MerchantBytesLoader` searches the first `0x6000` bytes for a count from 1 to 32 and that many lists of seven known wearable artifacts or `FFFFFFFF`, with at least four stocked entries overall. It does not find nearly sold-out tables or accept unknown artifact codes during discovery. The Map page reads the Black Markets by this table and the map-object cells (`MapPlacesLoader`).
  * `TownMerchantStockLocator` uses the injected date locator after the player table, requires the explicit date prefix and room for all 28 stock bytes, and rejects competing marked dates. It never uses an unmarked-date fallback and does not inspect artifact values, so sold-out stock and unknown codes still locate. `SavegameParser` adds one `TownMerchant` record, separate from the Black Markets. Every town with the Artifact Merchant built shows the same stock; history, per-slot reverts and the shared reset apply to it once, and a town reset only resets the town.
  * Genuine RoE, AB and Chronicles are not enabled. The `empire_of_the_world` saves show an unavailable notice on the town tab.
  * Editing merchant stock (town and Black Market) was tried in the game.
* **Not yet known**
  * Refresh rules of the town stock and its general location in other game versions.
  * The structure of the `empire_of_the_world` saves' date prefix needs independent validation before editing.

### Supporting savegames

* `Hota/MerchantStock` (eleven controls: `merchant_stock_00_baseline` (originally `GAME_BEGIN.GM2`) holds count 8 at decompressed `0x87A` and eight lists from `0x87E`; the purchases 01 to 10 each clear one entry; the town stock is at `0x797AD`, `0x797A5` (01 to 08) and `0x7979D` (09 and 10)); `MerchantStockTests`, `MapViewModelTests`.
* `visited_places_27_tazar_artifact_merchant` to `visited_places_29_tazar_black_market_first_4__orrin_artifact_merchant_last_4` and `multi_merchants` (four Black Markets); `merchant_stock_07_red_buys_town_head_of_legion`, `merchant_stock_06_red_buys_bow` and `merchant_stock_08_blue_turn` (the town stock; the sequence Stoic Watchman, Ladybird of Luck, Breastplate of Petrified Wood, Helm of Chaos, Ring of Life, Red Dragon Flame Tongue, Head of Legion).

## Creature banks (Horn of the Abyss)

* **What**
  * The banks are a uint16 count and variable-length records, the same table for every kind. Offsets within each record: seven int32 creature types at +0, seven uint32 counts at +28, seven resources at +56 (gold +80), reward creature type at +84 and its byte count at +88, uint16 artifact count at +89, and that many uint32 artifact IDs at +91. A Utopia has no creature reward and four artifacts, hence 107 bytes; the other five kinds have seven guard slots (five occupied), seven resource amounts, no creature reward (`-1`/0) and no artifacts, hence 91 bytes. A restriction of +88 to 0 or 1 would wrongly reject tables with creature rewards.
  * The order of the banks is the table order, not the order of the footprints on the map.
  * **The map-grid cell** starts with a uint16 type 16 and a uint16 subtype (not a uint32 type 16). The object index is at +4 and the entrance state at +6. Subtypes 0, 1, 3, 4 and 5 identify the kinds below; subtype 2 is not established. The footprint ordinal is not the bank index: the bank index is `(state >> 13) & 0xFFF`. Saved definition IDs can disagree with the actual map definition, so the object-position list is not shifted to compensate.
  * Dragon Utopias have four tiers with Green/Red/Gold/Black Dragon counts of 8/5/2/1 (20,000 gold), 8/6/3/2 (30,000), 8/6/4/3 (40,000) and 8/7/6/5 (50,000). Winning empties all seven guard slots (types `-1`, counts zero) but leaves the gold and the four artifacts intact. A partial defeat does not change the original creature counts, nor the surviving army. A win changes only the fought bank's 56 guard bytes (20 to 25 byte changes); its resources, artifacts and all the other records stay the same.

    | Kind | Subtype | Base / upgraded creature codes | Five equal stack counts, tiers 1 to 4 | Resource rewards, tiers 1 to 4 |
    |---|---|---|---|---|
    | Cyclops Stockpile | 0 | 94 / 95 | 4, 6, 8, 10 | 4, 6, 8, 10 of each of the six non-gold resources; no gold |
    | Dwarven Treasury | 1 | 16 / 17 | 10, 15, 20, 30 | 2/3/5/10 crystal and 2500/4000/5000/7500 gold |
    | Imp Cache | 3 | 42 / 43 | 20, 30, 40, 60 | 2/3/4/6 mercury and 1000/1500/2000/3000 gold |
    | Medusa Stores | 4 | 76 / 77 | 4, 6, 8, 10 | 5/6/8/10 sulfur and 2000/3000/4000/5000 gold |
    | Naga Bank | 5 | 38 / 39 | 2, 3, 4, 6 | 8/12/16/24 gems and 4000/6000/8000/12000 gold |

  * Slot 2 (zero-based) may be upgraded. The retained reward identifies the preset even after the guards are erased, but cannot recover the randomly selected upgrade.
  * **Entrance state**: a type-25 entrance cell holds a uint32 state at +6, bits 13 to 24 the bank index. The observed transition is `FC00001F | index<<13` to `FE00003F | index<<13` (XOR `0x02000020`). The tiers of a Utopia have the states `FC00001F`, `FC00201F`, `FC00401F` and `FC00601F` (ready) and `FE00003F`, `FE00203F`, `FE00403F`, `FE00603F` (cleared). The other kinds' strongest banks are at bank indexes 8 (Cyclops, `FC01001F` to `FE01003F`), 12 (Treasury, `FC01801F` to `FE01803F`), 16 (Imp, `FC02001F` to `FE02003F`), 20 (Medusa, `FC02801F` to `FE02803F`) and 24 (Naga, `FC03001F` to `FE03003F`). Other empties in the corpus have extra low bits (`FE00041F`, `FE08805F`, `FE004C7F`, ...), whose reset meaning is unproven.
  * **With a hero standing on the entrance** the grid type becomes 34 and keeps the subtype at +2 and the object at +4; +6 holds the hero ID. The state is then in the hero's **true header +16**, with the underlying object type 16 at +11 and has-object 1 at +15. The true header does not contain the subtype; surviving footprint cells still identify the kind. Biographies move that header outside the main chunk, so `HeaderIndex` and the header helpers are used, not a fixed hero-chunk offset.
  * **A reset needs both the guards and the entrance state.** Restoring the guard bytes alone gives "already defeated", even after walking away and back. Restoring both allows another battle and awards the gold and all artifacts again.
  * The visit tile of a bank depends on the sprite: the Imp Cache is visited at its anchor (for example (2,27)), the other five kinds one tile left of it. A bank is joined to a hero by the occupied grid cell, the hero ID and the surviving object's subtype and footprints.
* **Editor**
  * The map page and the creature-bank groups show and edit the supported kinds. Reset restores the guards and the entrance state (mask `0200003F` to `0000001F`, keeping the bank index and other bits; a revert restores the original bits), even under a hero standing on it. Recognized entrance states and complete standard guard/gold records are required; unsupported instances are kept without hiding other banks. For a defeated bank the editor shows an explicit prospective choice of upgrade (unupgraded by default); the choice alone is not written into an empty record.
  * Tried in the game: resets of all four Utopia tiers and of the strongest tier of the other five kinds (the second reward is the same as the first: for example +10 of every non-gold resource for a Cyclops Stockpile, +7500 gold and +10 crystal for a Dwarven Treasury, +24 gems and +12000 gold for a Naga Bank), changing the guard preset or tier, picking other rewards, marking an undefeated bank as defeated, and resetting while a hero stands on the entrance.
* **Not yet known**
  * Other players' visits and the next day's behaviour of an edited bank; custom tiers, upgrades and colours beyond the controls; RoE and SoD bank layouts.
  * Reset semantics of the other entrance patterns.

### Supporting savegames

* `Hota/DragonUtopias` (eight Utopias; strengths 4, 3, 2, 1 on the top row; bank order (5,15), (11,15), (17,15), (23,15), (5,21), (11,21), (17,21), (23,21), entrances one tile west; `dragon_utopias_03_tier_3_before` to `dragon_utopias_11_tier_1_stepoff` for the three weaker tiers; the resets `dragon_utopias_12_tier_3_won_again` to `dragon_utopias_14_tier_1_won_again`; `dragon_utopias_02_first_cleared_again`): the win, the entrance state, the reset. `visited_places_27_tazar_artifact_merchant` has a 41-bank table at `0x89E9A` (Utopias at indexes 2 and 3, crypts and banks with creature rewards).
* `Hota/CreatureBanks` (twenty controlled saves and the companion map; `CreatureBankDecodingTests`, `CreatureBankTests`): the 28-record table at `0x7EB9B` to `0x7F611` (Utopias 0 to 7, Cyclops 8 to 11, Treasuries 12 to 15, Imps 16 to 19, Medusas 20 to 23, Nagas 24 to 27), the entrance grid offsets `0xA5B1`, `0xA6A7`, `0xB99B`, `0xBA5D`, `0xBB31`, the occupied hero header at `0x4AC51`, and the re-fights `creature_banks_03_cyclops_stockpile_1_won_again`, `creature_banks_07_dwarven_treasury_1_won_again`, `creature_banks_11_imp_cache_1_won_again` (whose first fought bank had an upgraded Familiar centre), `creature_banks_15_medusa_stores_1_won_again` and `creature_banks_19_naga_bank_1_won_again`.

## Dwellings (Horn of the Abyss)

* **What**
  * A **uint16 count followed by generator records with owner bytes 75 apart** follows the complete owner/guard table of the mines. Offsets: **owner +0**, **object type +1** (17 for a fixed-creature dwelling; generator types 18 to 20 are accepted for location), **creature code byte +3**, **uint32 stock +7**, **x, y, level +15 to +17**; the guard arrays are seven uint32 types at +18 and counts at +46. Creature IDs include Pikeman 0, Archer 2, Griffin 4, Swordsman 6, Centaur 14, Devil 54, Automaton 176 and Jotunn 198. The bytes after the creature-code byte are not a uint32 ID.
  * **The stride is not the editable extent.** The known payload is 74 bytes, ending with the guard's last uint32 count (+70 to +73); the byte at +74 is not part of the dwelling, and the last record may end where the next structure (the obelisk marker) begins.
  * The table has one record for each dwelling of the map, in an order that is not that of the levels. On the old test map it has 62 records, not 31.
  * **Weekly stock** adds the creature's growth at the start of the week (Guardhouse: 28, 42, 56, 70 Pikemen); hiring them all sets it to zero.
  * Capturing a dwelling changes only its owner within its record; a guard is cleared when it is fought (a Devil and a Jotunn three-creature guard were; an unguarded Automaton was not).
  * The record tile is where the hero stood for the Castle and Rampart rows tested, and three tiles away for a neutral row: record coordinates are not automatically entrances.
* **Editor**
  * The Map page reads and edits owner and stock from the shared, validated table boundaries (`MapObjectTablesLocator`). There is no minimum run and no dependency on a detected mine: trailing non-mine owner/guard records are counted, and an explicit empty owner/guard table can precede a single dwelling. Competing tables, invalid guards and overlapping blocks are refused; all counted records must validate.
  * Only type-17 single-creature dwellings are exposed; types 18 to 20 are validated and skipped without hiding later type-17 records.
  * Editing dwelling guards is not implemented.
  * Owner and creature edits were tried in the game.
* **Not yet known**
  * The creature and stock semantics of generator types 18 to 20 (the Portal of Summoning's target, and the refugee-camp stock).
  * Controlled SoD capture variants and genuine RoE, AB and Chronicles formats.

### Supporting savegames

* `Hota/Dwellings` (Tazar flags the Castle's creature generators, Orrin flags them to Blue a turn later; `dwellings_05_tazar_flags_portal_of_glory`, `dwellings_09_tazar_flags_pegasus_dwelling_new_week`, `dwellings_16_tazar_visits_alehouse_new_week` and `dwellings_23_tazar_visits_sulfurous_lair_new_week` for the weekly stock; `dwellings_13_tazar_visits_rogue_cavern` to `dwellings_22_tazar_visits_ziggurat` the neutral row).
* `Hota/Arts` (`arts_00_before_arts`: generator count at 294445, records from 294447 + n*75, the last payload ends at 294821; five dwellings with creature codes 94 (53,33), 36 (45,40), 16 (12,70), 14 (22,58), 36 (47,42)).

## Unnamed bytes of the hero chunk (hypotheses)

* **What**
  * Bytes of the hero chunk that change between consecutive saves and are not named. Offsets are from the start of the hero chunk (as `HeroOffsets`); counts are the (hero, save pair) changes among about 340 saves. **None of this is verified.**

    | Offset | Changes | What was seen | First guess |
    |---|---|---|---|
    | 7, 8, 9 | 43, 97, 44 | change with the day and with moves | a small counter or path/animation state; low value |
    | 10 | 4 | `00`/`04` between a baseline and new-day saves | a flag |
    | 30 | 7 | counts down `08`, `06`, `01`, `00` (Tazar, grail saves); `00` to `03` (Crag Hack with horde artifacts) | a count of something used up (charges, boat movement?) |
    | 43, 44 | 156, 3 | `43` changes in almost every pair; `44` `00` to `06` after a battle | movement trail / facing counter |
    | 51, 52 | 15 | `FF FF` to `00 00` in pairs when a hero is in a town, and back | a 16-bit index of a town (0xFFFF none) |
    | 53, 55 | 2, 5 | `55` grows with fights | a battle counter? |
    | 77, 78, 79 | 4, 2, 2 | set at a new day for several heroes | a per-day value (random seed?) |
    | 95 | 2 | `00`/`04` | a flag |
    | 223..235 | few | `01`, `02`, `03`, `00` for Tazar after battles (`225` counts like `55`) | specialty or secondary-skill counters |
    | 263 | 2 | `00` to `05` for one hero | unknown |
    | 554, 555 | 2 | `FF FF` to `00 00` when an artifact enters the backpack | the count of the backpack |
    | 601..623 | 1 each | `00` to `FF` in bulk at a hero's first save | backpack slots written at set-up; not meaningful |
    | 1088 | 8 | `0x40` set on fleeing or defeat | a bit set (see "Heroes") |
    | 1102..1144 (HotA) | 30 | Cove saves change with the hero's bits | a HotA-only tail: specialty or bonus counters of the new mechanics (boats, new objects) |

  * Still unnamed elsewhere: `+135 0x10`, `+138 0x01`, hero `+34` (always 0), `+137` (the third byte of the visit effects), the `yy` of the date marker, the table of two bytes per object type, the dug tiles' number, the 4282-byte block that appears on a new day, the team table, banishing's counter and records, and the tavern's memory of a defeated hero.
  * What the HoMM2 saves suggest for HoMM3 (not fully tested): the temporary-bonus family should hold bits for Temple, Oasis, Rally Flag, Mermaids, Fountain of Youth, Buoy and Shrine of Magic; the weekly objects (windmill, water wheel, magic spring) change a flag in the map and the player byte `+129`.
* **Not yet known**
  * Everything in the table. The saves that would settle each row: two heroes in two different towns (the town index at 51 and 52); several battles with one hero, a save after each (53, 55, 44, 225); a boat boarded, sailed, landed and built (30 and the HotA tail).

## Coverage by game version

* **What**
  * Players, date and difficulty are verified on Horn of the Abyss and Shadow of Death saves. The castle layout and building edits are verified on both too.
  * Restoration of Erathia and Armageddon's Blade are assumed to match Shadow of Death; no saves of those were seen. The saves in `TestData/Roe` are Shadow of Death saves.
  * Everything was read from a few maps per version, so a shape that is unique there may not be on another map.
