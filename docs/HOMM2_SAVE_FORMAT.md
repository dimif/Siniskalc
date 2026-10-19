# HoMM2 savefile format (work in progress)

Reverse-engineered from `TestData/Homm2/BrokenAlliance` (map "Broken Alliance", 72x72; its README lists the saves) and `TestData/Homm2/ArraxTheJerk` (Price of Loyalty). Below, "ScenarioA" is Broken Alliance and "ScenarioB" is Arrax the Jerk (the first folder names), and "save N" is the number NN in the file name (the game's own names are in the README, for example save 10 is `10_GUILD.GM1`).
All integers are little-endian. Offsets are absolute and valid for this map only where noted ("map-dependent").
**Confirmed** = matches a known value or a diff between two saves. **Guess** = plausible, not verified.

## Container

- **Not compressed.** Plain bytes (all five saves are 108,635 bytes; HoMM3 saves are gzipped, HoMM2 ones are not).
- The same size for every save is expected: the layout is fixed for a given map (72x72). Only values change.
- The file starts with the scenario header (it looks like the MP2 map header), then players, castles, heroes, then map-sized arrays.

## Game versions (`.GM1` base game, `.GX1` Price of Loyalty)

ScenarioB (`1_EXP_MA.GX1`, 36x36, 53,820 bytes) is a Price of Loyalty save. Everything below was checked against both versions. Differences:

| | Base game (`.GM1`, ScenarioA) | Price of Loyalty (`.GX1`, ScenarioB) |
|---|---|---|
| start of file | `width, height, 0x5C...` | **`FF FF FF FF`**, then `width, height, 0x5C...` (a 4-byte version marker, so detect the expansion by it) |
| everything after the header text | as documented | **shifted by +5 bytes** (4 from the marker, 1 from an extra header field) |
| hero record | 236 bytes | **250 bytes**: the first 0xD9 bytes are identical, the last 14 are extra zeros before the next record's header |
| castle table | starts 0x36 bytes after the hero table | the same (first castle at `hero table end + 0x36`), 100 bytes per record, 72 slots in both |
| player records | 6 x 0xCF | the same, still 6 (empty seats are all zeros / 0xFF) |
| heroes | 54, original names | 54, some renamed (Bikko replaces Tsabu, King Anthrax replaces Atlas); hero 16 is Blue's start hero in ScenarioB |

Everything is verified by a script that parses both files with the two parameters (shift, hero stride): resources, incomes, heroes, army, skills, artifacts and castles come out sensibly for both (ScenarioB: Blue starts with no resources and the hero in his own castle tile; Green/the AI owns Arrax with King Anthrax at level 17).

The map sizes differ (72x72 versus 36x36) but do not move any table before the map arrays: the arrays that depend on the map size are all after the castle table.

## Header (0x00 - ~0x2A8)

| Offset | Size | Meaning |
|---|---|---|
| 0x00 | u32 | map width (72) - confirmed by value |
| 0x04 | u32 | map height (72) |
| 0x08 | u32 | 0x5C, the MP2 magic |
| 0x0C | u16 | **map difficulty**: 0 easy, 1 normal, 2 hard, 3 expert (saves of maps of each; the map is not the game difficulty, which never changes it) - confirmed. Shown as Pawn, Knight, Rook, Queen like HoMM3 |
| 0x0E,0x0F | u8,u8 | width, height again (0x48) - confirmed: 0x24 small, 0x48 medium, 0x6C large, 0x90 extra large |
| 0x10.. | 3 x 6 x u8 | colours in the game (Blue, Green, Red, Yellow, Orange, Purple), then the colours a human may play, then the colours a computer may play: what the map allows, not who plays - confirmed (`HumanCpu`: Broken Alliance allows Blue, Green and Orange to a human) |
| 0x1B8 | 6 x u8 | the faction of each colour: 0 Knight, 1 Barbarian, 2 Sorceress, 3 Warlock, 4 Wizard, 5 Necromancer - confirmed |
| 0x2B5 (6 bytes before the date) | 6 x u8 | **the mask of the human players, which the game plays from**: a byte for each record of the table of players (not each colour), 1 for a human; in the saves it is always the first N records. Probes: changing it to one human made the game play Green alone (`C09MSK`, `C10ONE`); changing only the controls at 0x1BE and the count at 0x1DA was ignored (`C01` to `C08`). The mask is not enough: a mask of three humans in a file named `.GM2` played only two (`C11THR`), and the same bytes in a file named `.GM3` played all three, Blue, Green and Orange, starting on the turn of record 0 (`C13THR`). The number the game asks for when loading a hot seat game, which is the digit of the extension, caps the humans that play; the name stored in the save (at 0x29E) does not matter (`C12THR`, with the stored extension changed too, behaved the same). So to change who plays, the mask, the order of the records and the extension of the file all have to agree |
| 0x1DA | u8 | **how many humans play** (1, 2, 4, 6 in the saves of `HumanCpu`; it matches the digit of the extension of a hot seat save), only for the scenario summary. The records of the humans come first, in the order they play (Green and Orange human: Green, Orange, Blue, Red, Yellow, Purple; each record says its colour in its byte 1). The hot seat names at 0x1F4 (21 bytes each) follow the same order. Probes that changed only 0x1BE (and the turn) were ignored by the game, which played Green and Orange as before, while its scenario summary showed the changed controls: 0x1BE feeds only that summary |
| 0x1BE | 6 x u8 | **who plays each colour, as the scenario summary shows it**: 0x0A is the computer; a human has his number among the humans, from 0 (the single human of a game of one is 0 whatever his colour; in a game of six humans it equals the colour) - confirmed with one, four and six humans |
| 0x2AD (14 bytes before the date; 5 bytes further on in a Price of Loyalty save, whose header shifts by 4 up to the player names and by 5 after) | u8 | **whose turn it is**: the number among the humans (the 0x1BE byte of that colour), 0 to 5 in the six-human saves; 0 in the one-human saves. Confirmed to be the number and not the colour: with Green and Orange as the only humans it goes 0, 1, 0, 1, 0 (Green is human 0, Orange human 1) |
| 0x42 | char[16] | map name "Broken Alliance" - confirmed |
| 0x76 | char[...] | map description - confirmed |
| 0x1C4 | u8 | **game difficulty**, the byte in front of the map file name: 0 easy, 1 normal, 2 hard, 3 expert, 4 impossible - confirmed (five saves of one map, `MapDifficultySize` 01 to 05). Shown as Pawn, Knight, Rook, Queen, King. 0x1EE.. holds a percentage that follows it (70, 100, 120, 140, 160) but also depends on the map |
| 0x1C9 | char[] | map file name `brokena.mp2` - confirmed |
| 0x1F6 | char[~13] x 6 | default player names "Blue player" ... - confirmed |
| 0x29E | char[] | the savefile's own name (`1_START.GM1`). It is the only thing that differs besides game state; changes each save. |

## Players: 6 records, stride 0xCF (207 bytes), first at about 0x2C0

Offsets below are for Blue; add `n * 0xCF` for colour n (Blue 0, Green 1, Red 2, Yellow 3, Orange 4, Purple 5).

| Offset (Blue) | Size | Meaning |
|---|---|---|
| 0x2BB, 0x2BD, 0x2BF | u8 x 3 (the byte between each is 0; the last is the byte in front of the table of players) | **date**: day of the week (1..7), week (1..4), month (1..; save 22 is day 2, week 1, month 2: after day 7 of week 4 the month increments). Saves 1..5 are day 1, save 6 day 2, saves 7-10 day 3, save 11 day 5, save 12 ("new week") is day 1, week 2 - **confirmed**. Global, not per player |
| 0x2C2 | u8 | number of heroes the player owns (1 -> 2 when Sir Gallant was hired) - confirmed |
| 0x2C3 | u8 | **selected hero ID**, 0xFF when none (the UI selection: it becomes 0xFF when a castle is selected) - likely |
| 0x2C5 | 5 x u8 | the heroes the player owns, as hero IDs, 0xFF for empty (Maximus 7, then Gallant 1 after the hire) - confirmed |
| 0x2CD, 0x2CE | u8, u8 | the two heroes offered in the tavern, as hero IDs (Blue: 0x01 Sir Gallant, 0x33 Roxana). After Gallant was hired slot 1 became 0x10 (Tsabu), a new offer; Roxana stayed - **confirmed** |
| 0x304 | u8 | **days left before the player is eliminated for owning no castle or town**, 0xFF while he owns one. Blue lost his last castle in save 21 (7), then 4 (save 22), 3 (save 23); it went back to 0xFF after the next capture (save 24). Green: 3 in save 21, 0 in save 22 where he was kicked out - **confirmed** |
| 0x305 | u8 | number of castles owned (1 -> 2 when Corackston was captured; 0 when the last was lost) - confirmed |
| 0x306 | u8 | **selected castle index**, 0xFF when none (3 = Xabran when the castle was selected after building) - likely |
| 0x308 | u8 list | **castles owned**, as castle indexes, 0xFF for empty (`03 FF..`, `03 02 FF..`; after the three captures of saves 19-24 `02 03 01`); at least 4 entries are used (Purple holds `08 09 04 0A`) - confirmed |
| 0x350 | u32 | wood - confirmed |
| 0x354 | u32 | mercury - confirmed |
| 0x358 | u32 | ore - confirmed |
| 0x35C | u32 | sulfur - confirmed |
| 0x360 | u32 | crystal - confirmed |
| 0x364 | u32 | gems - confirmed |
| 0x368 | u32 | gold - confirmed. 7500 -> 8700 (two 600 chests) -> 7700 (Archery Range, 1000) -> 6700 on day 2 (+1000 castle income, -2000 Mage Guild) -> 7700 on day 3 -> 5200 after hiring Gallant (2500) |
| 0x36C | 6 x u32 | **daily income per resource**, same order as the stock: wood, mercury, ore, sulfur, crystal, gems. Blue: wood 2 (sawmill, after the turn change), 4 (second sawmill, save 15); mercury 1 and ore 2 (saves 13 on); crystal 1 (save 16). The AI players have the same table (Green's wood income is at 0x43B). It is recalculated at the start of each day, so it lags a capture by one day. A player that was put out (Green, saves 22 to 24) has no mines, but the table holds a single 1 that walks over the resources, one a day (wood in 22, mercury in 23, ore in 24): it is a leftover of the game, not an income - confirmed |
| 0x384 | u32 | **daily gold income**: 0 on day 1, 1000 on day 2 (castle), +250 for the statue (1250), +100 later (1350), 1600 after capturing Corackston and a pickup, 2600 after a gold mine (+1000) in save 16 - recalculated at the start of each day - likely |

Wood and ore checks (day 2): wood 20 + 16 (two stacks) - 5 (Mage Guild) = 31, ore 20 - 5 = 15, both confirmed; day 3 wood +2 (the captured sawmill, 33).

Resource order in the file is wood, mercury, ore, sulfur, crystal, gems, gold (the usual HoMM2 order).
Resource stride between players is 0xCF.
The rest of the player record (0x388..) is unexplored. 0x2F9 went 0 -> 1 in save 13 (possibly a count of captured mines); unknown.

## Castles: 72 slots of 100 bytes (0x64), first at 0x399A in ScenarioA, 12 are used there (4 in ScenarioB)

Layout of one record (offset from record start `S = 0x399A + 100 * i`):

| Offset | Size | Meaning |
|---|---|---|
| +0x00 | u8 | castle index (0..11) |
| +0x01 | u8 | owner colour, 0xFF = nobody (Blue = 0 ... Purple = 5). Capture (save 14): 0xFF -> 0, the race stays (Wizard) - confirmed |
| +0x03 | u8 | race (0 Knight, 1 Barbarian, 2 Sorceress, 3 Warlock, 4 Wizard, 5 Necromancer) - confirmed by values for the player castles |
| +0x04, +0x05 | u8, u8 | tile x, y of the castle - confirmed (Blue's castle is at 28,25 and its hero starts at 28,27) |
| +0x06 | u8, u8 | x, y of the shipyard boat spot (castle x+1, y+2); 0xFF when the castle is inland - guess, matches all coastal castles |
| +0x08 | 5 x u8 | **garrison creature types** (0xFF empty). Neutral Corackston held Halfling/Iron Golem/Boar-like stacks (IDs 38, 40, 39) - confirmed structure |
| +0x0D | 5 x u16 | **garrison counts** (Corackston: 28, 10, 7). When Blue captured the castle (save 14) the garrison was emptied (all 0xFF / 0) - confirmed |
| +0x17 | u8 | **hero ID visiting the castle**, 0xFF when empty. Maximus 7 on day 3, Sir Gallant 1 right after the hire, 0xFF once he left - **confirmed**. HoMM2 has no garrison hero, so this is the only link |
| +0x18 | u16 | **town buildings bit mask**. Low byte: 0x01 Mage Guild (set with any guild level), **0x02 Thieves Guild, 0x04 Tavern (swapped after the write test: E09 set 0x02 and the game showed the Thieves Guild), 0x08 Shipyard**, 0x10 Well, 0x20 "town only" (no castle built; replaced by 0x40 when the castle is built, as AI Corackston did: 0x20 -> 0x50), 0x40 Castle, 0x80 Statue. Blue's Xabran: 0x40 -> 0x41 (guild) -> 0xC1 (statue) -> 0xD1 (well) -> 0xD7 (tavern + thieves guild, save 13); Whiteshield 0xD5 -> 0xDD when the shipyard was built (save 20) - **confirmed** for 0x01-0x10, 0x40, 0x80. The high byte (+0x19) holds more bits (0xBB in Whiteshield, probably marketplace, captain, turrets, upgrades) - not decoded |
| +0x1A | u32 | dwelling and Mage Guild mask. **Dwellings are 0x08, 0x10, 0x20, 0x40, 0x80, 0x100 for dwelling 1..6** (the Warlock's Green Tower, the Green Dragon dwelling, is 0x100: Whiteshield 0x000208B8 -> 0x000209B8 in save 24, and its weekly dwelling 6 count at +0x28 became 1) - confirmed; the upgrade bits are probably 0x200.. 0x4000 for dwelling 2.. 7 (Whiteshield has 0x800 set). 0x08 = dwelling 1 (Thatched Hut), 0x10 = dwelling 2 (Archery Range), **Mage Guild level is one value in 0x10000..: 0x10000 = level 1, 0x20000 = level 2 (the level 1 bit is replaced, not added)** - confirmed. 0x00020018 after save 10 |
| +0x1E | u16 x 6 | creatures available in dwellings 1..6. Blue: 12 peasants -> 12 peasants + 8 archers after building the range - **confirmed** |
| +0x3C | 5 x 4 x u8 | **Mage Guild spell pool**: 4 slots per guild level (level 1 first), each a *zero-based* spell ID (spell ID - 1 in the table below). Xabran: L1 `19 31 05 26`, L2 `22 32 11 02`, L3 `18 34 00 27`, L4 `01 3F 1D 2A`, L5 `08 38 1B 2B`. The first three of L1 are Magic Arrow, View Resources, Cure and the first three of L2 are Disrupting Ray, View Artifacts, Steelskin, exactly what the game showed - **confirmed**. The 4th slot of each level was never shown (probably the Library/extra spell) |
| +0x50 | 5 x u8 | **number of spells shown by the guild per level**: 0 -> 3 when level 1 was built (save 6), +0x51 0 -> 3 for level 2 (save 10) - confirmed (so 3 of the 4 slots are used) |
| +0x55 | u8 | in every castle it grows by 1 per day (2, 3, 4, 6, 9...): it follows the total day count (day 8 = 9: it is 1 + days played) - guess |
| +0x57 | char[~13] | **castle name. It is stored at the END of the record**: the name of the record that begins at `S` is read at `S + 0x57`. Blue's castle (28,25) is "Xabran" - confirmed against the game |

The other building bits are not decoded. Dwelling counts only grow on a new **week** (save 12): 12 peasants + 8 archers became 26 + 18, i.e. the weekly growth (12 and 8) **plus 2 each from the Well** built that week - confirmed.

## Heroes: 54 records, stride 236 (0xEC), first (Lord Kilburn) at 0x7A6

The record index is the hero ID, in the original game order: 0-8 Knight, 9-17 Barbarian, 18-26 Sorceress, 27-35 Warlock, 36-44 Wizard, 45-53 Necromancer.
Maximus is 7, Sir Gallant 1, Roxana 51. Offsets are from the start of the record (the name).

**The record really starts 10 bytes before the name**: the last 10 bytes of each 236-byte slot (0xE2..0xEB) are the header of the *next* hero, and the first hero's header sits at 0x79C. They are listed below as negative offsets, `-0x0A`..`-0x01`. (An earlier version of this document attached them to the wrong hero.)

| Offset | Size | Meaning |
|---|---|---|
| -0x0A | u16 | **current spell points**: 10 for Maximus (knowledge 1) at the start; +1 per day up to the cap; the cap is knowledge x 10 and is not stored. **Casting Magic Arrow (cost 3) took 12 -> 10 (with the +1 of the new day)** and a defeated hero (Natasha) paid 3 for her spell in save 16 (30 -> 27), then regenerated 28, 30 - **confirmed** |
| -0x08 | u8 | hero ID (again) |
| -0x07 | u8 | **owner colour** (0 Blue ... 5 Purple), 0xFF = nobody. Maximus 0; Sir Gallant went 0xFF -> 0 when hired - **confirmed** |
| 0x00 | char[14] | name - confirmed |
| 0x0E | u8 | hero ID - confirmed |
| 0x0F | u32 | tile x (0 while not on the map) - confirmed (Maximus 28 -> 29 after one move) |
| 0x13 | u32 | tile y - confirmed |
| 0x17 | u32, u32 | **destination: the tile the hero is going to, which the game marks on the map** (x, y); 0xFFFFFFFF when none. Set while a move is planned (save 5 held 23,23 when the movement ran out) - **confirmed** by the MarkerFacing saves: the tile was put on each of the eight sides of Colar Blau |
| 0x22 | u8 | **facing**: 0 north, 1 northeast, 2 east, 3 southeast, 4 south, 5 southwest, 6 west, 7 northwest (clockwise); the way of the last step; 0 while inside a castle - **confirmed** by MarkerFacing (east in 83 to 90, west 91, northwest 92, northeast 93); every hero of every test save holds 0 to 7 |
| 0x23 | u8 | tile object under the hero: 0xA3 castle gateway (also Kalindra in Vertigo), 0x66 sawmill, 0x89 fountain - guess |
| 0x25 | u8 | castle index while inside a castle (Maximus 3 in Xabran and 2 in captured Corackston) - confirmed. It was 7 once in save 15 while the hero stood outside, unexplained |
| 0x27 | u32 | maximum movement points. It is **derived from the army**: 1000 with peasants and archers, 1400 (0x578) once the army was black dragons only, and 1200 for Crodo's start army - confirmed. Recalculated by the game, but a writer that changes the army should know it |
| 0x2B | u32 | movement points left (1000 -> 888 -> 688 -> 0) - confirmed |
| 0x2F | u32 | experience (0 for Maximus, 67 for Lord Kilburn; Gallant 50 on hire and 1550 at level 2) - confirmed |
| 0x33 | u8 (u16?) | level - confirmed (Gallant 1 -> 2) |
| 0x35 | 4 x u8 | attack, defense, spell power, knowledge (2,2,1,1) - confirmed; Gallant's level-up raised defense 2 -> 3 |
| 0x59 | u16 | unknown, differs for every hero and never changed here |
| 0x5B | 5 x u8 | creature type per army slot, 0xFF empty. IDs are zero-based in the HoMM2 creature order: 0 Peasant, 1 Archer, 2 Ranger, 3 Pikeman ... 37 (0x25) Black Dragon - confirmed for 0, 1 and 37; the middle of the list follows the fheroes2 order (Knight 0-10, Barbarian 11-19, Sorceress 20-28, Warlock 29-37, Wizard 38-46, Necromancer 47-56) and is not verified. An empty slot is count 0 (earlier saves also hold 0xFFFF) |
| 0x60 | 5 x u16 | creature count per army slot (0xFFFF when empty) - confirmed (37 peasants, 4 archers; after the split 34,4,1,1,1) |
| 0x6A | 14 x u8 | secondary skill level (0 none, 1 basic, 2 advanced, 3 expert) indexed by skill ID: 0 Pathfinding, 1 Archery, 2 Logistics, 3 Scouting, 4 Diplomacy, 5 Navigation, 6 Leadership, 7 Wisdom, 8 Mysticism, 9 Luck, 10 Ballistics, 11 Eagle Eye, 12 Necromancy, 13 Estates. Maximus has [6]=1, [10]=1 - confirmed |
| 0x78 | 14 x u8 | the order in which the skills were learned (1-based), same indexing. Maximus Leadership 1, Ballistics 2 - likely |
| 0x86 | u8 | number of secondary skills (2) - **confirmed** by Gallant's level-up (see below) |
| 0x89 | 66 x u8 | **spells in the spellbook**: one 0/1 byte per spell ID, so spell ID `s` is at `0x89 + s` (ID 0 is unused). Maximus after buying the book and the six guild spells has 6, 18, 26, 35, 50, 51 = Cure, Steelskin, Magic Arrow, Disrupting Ray, View Resources, View Artifacts - **confirmed** (Kalindra's 16 spells and Roxana's Haste look right too) |
| 0xCB | 14 x u8 | **artifact slots**, 0xFF = empty. Slot 1 got 0x44 (68) for Maximus in save 15 (the Ammo Cart, as the game said) and 0x2D (45, Ice Cloak) for Gallant in save 16, which was Natasha's artifact (she held 0x51, 0x2D in save 12) - so the artifact moves with the victory. The names for the artifact IDs follow the fheroes2 order (68 = Ammo Cart confirmed). Filled from the first slot: 0x51 (81) = Magic Book, 0x0D (13) = Medal of Courage. Every spellcasting-class hero starts with 0x51 in slot 0 - **confirmed**. Maximus bought the book in save 10 (slot 0 went 0xFF -> 0x51); Gallant picked up the medal in save 11 (slot 0 went 0xFF -> 0x0D) |

Notes:
- Artifact IDs are the HoMM2 ones used by fheroes2 (Magic Book 81, Medal of Courage 13). Equipment slots are not tied to a body part: it is a plain list of 14.
- Spell IDs (1..65, as in fheroes2): 1 Fireball, 2 Fireblast, 3 Lightning Bolt, 4 Chain Lightning, 5 Teleport, 6 Cure, 7 Mass Cure, 8 Resurrect, 9 Resurrect True, 10 Haste, 11 Mass Haste, 12 Slow, 13 Mass Slow, 14 Blind, 15 Bless, 16 Mass Bless, 17 Stoneskin, 18 Steelskin, 19 Curse, 20 Mass Curse, 21 Holy Word, 22 Holy Shout, 23 Anti-Magic, 24 Dispel, 25 Mass Dispel, 26 Magic Arrow, 27 Berserker, 28 Armageddon, 29 Elemental Storm, 30 Meteor Shower, 31 Paralyze, 32 Hypnotize, 33 Cold Ray, 34 Cold Ring, 35 Disrupting Ray, 36 Death Ripple, 37 Death Wave, 38 Dragon Slayer, 39 Blood Lust, 40 Animate Dead, 41 Mirror Image, 42 Shield, 43 Mass Shield, 44 Summon Earth, 45 Summon Air, 46 Summon Fire, 47 Summon Water, 48 Earthquake, 49 View Mines, 50 View Resources, 51 View Artifacts, 52 View Towns, 53 View Heroes, 54 View All, 55 Identify Hero, 56 Summon Boat, 57 Dimension Door, 58 Town Gate, 59 Town Portal, 60 Visions, 61 Haunt, 62-65 Set Guardians. Only 6, 18, 26, 35, 50, 51 are verified against the game.- Level-ups are written as plain stat changes: Maximus went to level 4 in save 16 (exp 3708) and gained defense, spell power and knowledge ([2,2,1,1] -> [2,4,2,2]) plus Pathfinding to advanced and Wisdom basic. Gallant (save 13, level 3, exp 2550) gained Archery and Estates. The level never limits the experience (exp is stored, level is a separate byte) - the editor must keep them in step.
- Fountain (save 13): bytes 0x3B (0 -> 1) and 0xD9 (0 -> 4) were set and cleared again the next day; presumably a temporary luck/morale bonus. Other small bytes in 0x39..0x58 that changed with play (Gallant 0x3D = 4, Maximus 0x45 = 2) are unexplained - maybe visited-object marks.
- The six bytes just before each name (-0x06..-0x01) hold leftover garbage (`9D FF 00 9D FF`) and are not data.
- Level-up (save 9, Sir Gallant): level 1 -> 2 (0x33), defense 2 -> 3 (0x36), a new secondary skill Pathfinding basic: skill level [0] = 1, learn order [0] = 3, skill count 2 -> 3. This confirms the skill tables above.
- Hire (save 8): owner byte 0xFF -> Blue, position set to the castle (28,25), castle visitor = 1, gold -2500, the player's hero list gets the ID, a new tavern hero replaces him.

## Map-sized arrays (after the castles)

- The arrays that change when the hero moves (fog of war / visibility) are 72 bytes per row, one byte per tile, around 0x6AEA - 0x6E08 in the save where the hero moved.
- 0x5600: a table of 6-byte entries; 0x3E4A+: mostly zeros with scattered objects.
- Not decoded: terrain tiles, objects (the 600 gold chests disappeared in save 4: bytes at 0xD90B, 0xD911, 0xDC77, 0xDC7D and four entries at 0x19D54.. were changed), mines, dates, kingdom state.

## Heroes defeated, castles lost

- A defeated hero (save 16, Natasha): owner byte 0xFF, position cleared, her artifact list emptied of everything but the Magic Book, the stacks she had left stay in the record (a lost stack leaves 0xFF). The winner got her artifact, and the AI player's hero list loses her.
- Castle lost to another player (save 18): castle owner byte Blue -> Yellow, the player's castle count 2 -> 1 and the list `03 02` -> `03 FF`; the garrison holds the attacker's creature (type 0x15, count 9).

## Elimination

The player record of a player who owns no castle shows his countdown in +0x44 (see above). Green (save 22) after the timer ran out: castle count 0, countdown 0, hero count 0, hero list all 0xFF, and the hero Jojosh got owner 0xFF while keeping his old position and a shrunk army (so a written save with an eliminated player needs no special flag other than those). A hero of an eliminated player can be hired again by another one (Crag Hack became Orange's, save 21). In the header, the bytes at 0x2AE and 0x2B0 (0 -> 1 in save 22) look like a counter of eliminated players, guess.

## Tavern rumor text

A text buffer near 0x5CC0 holds the current tavern rumor/tip ("The ultimate artifact may be found ...", later "Purple player earns the most gold."). The game builds it at runtime from the game state and the save only stores the last one. It is not game data, so a writer can leave it alone.

## What the saves did tell us (diffs)

| Save | What was done | Bytes that changed |
|---|---|---|
| 1 -> 2 split | army split | army counts/types only. Many `FF FF FF FF FF FF` runs in the hero and other records became `00` - the first save was written before the game had first initialized them, so ignore those. |
| 2 -> 3 move | 1 square | hero x, y, 0x22, movement left, plus fog bytes and two map bytes (hero marker moved) |
| 3 -> 4 gold | 2 x 600 gold | gold, movement, player 0x2C3, 0x306, map object bytes |
| 4 -> 5 archery | moved to 0 mp, built Archery Range | gold -1000, hero position/target/movement, castle building mask and dwelling counts, fog |
| 5 -> 6 new turn | day 2, +16 wood, sawmill captured, Mage Guild 1 | day counter, wood/ore/gold, income field, castle mask 0x10000, castle flags, AI heroes and AI gold |
| 6 -> 7 garrison | day 3, Maximus entered Xabran | day, selected hero/castle, castle visitor = 7, hero +0x23/+0x25 (in castle), wood +2 (sawmill) |
| 7 -> 8 new hero | hired Sir Gallant | gold -2500, hero count and list, tavern slot, Gallant's owner and position, castle visitor = 1 |
| 9 -> 10 guild 2 + book | Mage Guild 2, Maximus bought the Magic Book | castle mask 0x20000 and spell count, resources, Maximus slot 0 = 0x51 and six spell bytes, castle visitor |
| 10 -> 11 statue + medal | statue built, Gallant picked up Medal of Courage | castle flags 0x80, Gallant slot 0 = 0x0D, date |
| 11 -> 12 new week | week 2, well built | castle flags 0x10, dwelling counts +weekly +2, date, hero movement refilled |
| 12 -> 13 fountain | Maximus visited a fountain, army became 5 black dragons; mines captured | army types/counts, max movement, income table (mercury 1, ore 2), gold income, Gallant's level/skills |
| 13 -> 14 capture | Corackston captured | castle owner 0xFF -> Blue, garrison cleared, visitor = 7, player castle count/list, hero in castle (+0x25 = 2) |
| 14 -> 15 artifact | second artifact (Maximus slot 1 = 0x44), second sawmill | slot byte, wood income 4, gold income 1600, 10 dragons |
| 16 -> 17 fight | Maximus fought and cast Magic Arrow | spell points, exp, stats, Gallant's army (lost an archer stack), gold/resources |
| 17 -> 18 lost castle | Corackston taken by Yellow | castle owner/garrison, Blue's castle list, income |
| 18 -> 19 capture | Blue captured Whiteshield | castle owner Yellow-side -> Blue, garrison cleared, Blue castle list `03 01`, hero in the castle |
| 19 -> 20 shipyard | shipyard built in Whiteshield | only castle +0x18 bit 0x08 and gold |
| 20 -> 21 no castle | Blue lost his last castle | Blue's countdown 7, castle count 0, list FF, income table; AI heroes/castles moved too |
| 21 -> 22 / 22 -> 23 | days pass | countdown 4 / 3; Green eliminated (countdown 0, no heroes) |
| 23 -> 24 captures + Green Tower | Blue recaptured castles 2, 3, 1 and built dwelling 6 | castle owners, castle list, countdown back to 0xFF, castle 1 mask bit 0x100 and count |
| 15 -> 16 level | both heroes levelled, Gallant picked up 0x2D, crystal mine and gold mine | stats, skills, exp, income table crystal 1, gold income 2600 |
| 8 -> 9 level up | Gallant levelled | level, exp, defense, new skill, move away from castle (visitor back to 0xFF) |

## Next steps

1. Missing: mines (which tile bytes mark a mine and its owner), boats, more than two artifacts on one hero. The header bytes 0x1F1..0x1F3 (0, 0x0A, 0x05, 0x09/0x06) and 0x2AE/0x2B0 change with play and are unexplained; they sit in front of the player table and a writer can ignore them.
2. The high byte of the castle building mask (+0x19) and the dwelling upgrade bits in +0x1A (a save with a marketplace, a turret, an upgraded dwelling).
3. Decode the map-sized arrays: mines/objects ownership, visited objects, terrain.
4. The header in front of the player records is not decoded (it holds the variable parts: the seat flags, the name tables and the scenario text); the shift of 5 for the expansion is empirical. Find out what the 1 extra byte is, and whether other maps shift more (more saves from other maps would show it).


## What the write test with the game showed (2026-10-13)

Saves E01 to E13 were made by editing the models, E14 to E26 with the editor itself; each was loaded in the game and what was seen is in `TestData/Homm2/out/README*.md` (not in the repository). The findings:

- **Dwellings.** The six dwellings have the bits 0x08 to 0x100, and an upgrade **replaces** the bit of its dwelling (the second to the sixth dwelling upgrade with 0x200, 0x400, 0x800, 0x1000 and 0x2000, the sixth a second time with 0x4000). Whiteshield has 0x800 and not 0x40: its dwelling 4 is a Minotaur King. Setting 0x40 as well made the game show "Minotaur, 0 waiting" (E17DWEL), so the editor sets one bit only. The upgrades stop where the creatures of the level do (the dragons of a Warlock upgrade twice, the Cyclops not at all).
- **Mage Guild.** The level is a **number** in the bits 16 to 18 of the dwellings (0 to 5: 0x30000 is level 3, 0x50000 is level 5), not a flag per level. The castles of the saves show 3, 3, 2, 2 and 1 spells at the levels they have built; a Wizard castle with the Library shows one more at each (4, 4, 3, 3, 2), and the Library is the bit 0x2000 of the buildings (the only difference between Woodhaven and Vertigo; it is probably the special building of every race). The count of each level is the byte "Mage Guild spell counts"; the fourth spell of a level that is not counted is never shown (E18GUILD).
- **Town buildings.** 0x02 is the Thieves' Guild and 0x04 the Tavern (E09CAST showed a Thieves' Guild for 0x02).
- **Artifacts.** The primary skills hold the bonuses of the artifacts, which the game adds and takes away when an artifact is given or taken (E13ARTSW: the Ultimate Sword is +12 attack). E19, E20: the editor's values load and show.
- **Limits.** A stack is a **signed** 16 bits: 65,535 shows as -1 on the map and 0 on the hero screen and splitting it misbehaves (E24STACK), so the editor stops at 32,767 (not tried). Gold went over a million and resources over a thousand in the next turn without trouble (E23LIMIT), so the limits of the editor (999,999 gold, 9,999 of the rest) are not the game's.
- **Level and experience.** E25STATS wrote level 20 and 50,000 experience and the game showed **level 14**. E08LEVEL (level 10, 14,700 experience) showed 10. E30XP50K, E31LV20 and E32BIGXP write one of the two alone to tell whether the game works the level out from the experience.
- **Owner of a castle.** E14GIVE: the owner and the lists of both players changed and the game agreed (the castle is Orange's on inspection), but the flag on the map stayed Blue's and the castle was missing from Blue's list until the turn changed. The map holds the flag and what each player has explored, which the editor does not know: E15LAST gave Bloodreign to a player who had not explored the area, and the town could only be reached through the list of castles until the area was seen.

- **Level and experience, solved (E30 to E33).** The game works the level out from the experience and ignores the level byte: level byte 1 with 50,000 experience showed level 17 (next level at 54,244), level byte 20 with none showed level 1, level byte 1 with 200,000 showed level 25 (next at 231,549). `HeroLevels` uses the fheroes2 table (54,200 and 232,700 for those two thresholds, so slightly off); the hero page sets the level from the experience and the experience to the start of a level when the level is set. (The level 14 of E25STATS is not explained: that save's file holds level 20 with 50,000.)
- **Stacks.** 32,767 peasants and 9,999 rogues display and split fine; adding one more overflowed to -32,768 (E33STACK), so 32,767 is the limit.
- **Creatures in upgraded dwellings (E27).** Only the plain dwellings showed their 10 creatures: an upgraded dwelling keeps its count in **its own list at +0x2A, five u16 for the dwellings 2 to 6**, and its slot at +0x1E is then 0 (the same in the castles of the save: Hillstone, Dragontooth, Bloodreign, Woodhaven, Vertigo). The model has one count for each dwelling (`CreaturesAvailable`, `Waiting(dwelling)`), as in the game, where the dwelling and its upgrade share it; the reader takes it from the list its level says and the writer moves it when the level changes (and puts the mark the game shows the dragons upgraded twice with). In the Threedom saves the game leaves the old count in the dwelling's own list next to an upgraded dwelling (Hillstone, dwelling 2: 14 beside the live 11); the editor reads the live one and does not touch the old one while the level stays. E34DWUP2 and `SafeCastleSerializerTests` test it.
- **The Black Dragon (E34DWUP2).** Every upgraded dwelling worked except the dragons upgraded a second time (0 waiting): the list of upgraded counts at +0x2A has **six** entries and the last one (+0x34) is for the sixth dwelling upgraded twice. E35BLKDG tests it.
- **The Black Dragon, solved (E36, E37).** The count of the dragons upgraded twice is at +0x34, but the game shows none while the entry before it (+0x32) is 0; the saves the game made have a 3 there (E36: count 9 with the 3 shows 9; E37: 3 and 3 shows 3). The editor writes a 3 there when it has to; what the 3 means is not known. E38DRG5, made by the editor with it, showed 5, could be hired from and restocked at the right rate (confirmed).

## Maps with heroes of their own

In `MapDifficultySize` save 07 (a scenario with heroes such as Famine, War and Pestilence) four heroes hold another number than their index in the byte that is the hero ID elsewhere (`HeroLayout.Id`), probably the portrait the map gave them. The locator therefore accepts a table when most heroes hold their index (and every castle holds its own). How the editor treats those heroes (it finds them by that byte) is not checked yet.

## What the saves of Threedom showed (2026-10-17)

`TestData/Homm2/Threedom`: 71 Price of Loyalty saves (`.GX1`, a medium map, 72 x 72, 131,550 bytes) of one game, a Necromancer (Hillstone) and a Wizard town captured later (Whiteshield), two heroes (Charity, Colar Blau). All 71 load and write back to the same bytes with the existing readers. The list of what was done in each is in the folder's README; below is what the differences between saves told.

**Confirmed against the editor's model**

- **Level and experience.** Seven (hero, save) pairs of levels 3 to 11 match the table of `HeroLevels` (Charity 2,500 = 3, 3,624 = 4, 5,339 = 5, 7,399 = 6, 9,999 = 8; Colar Blau 6,540 = 6, 8,050 = 7, 9,460 = 8, 12,780 = 9, 15,060 = 10, 18,205 = 11).
- **Artifact bonuses.** The skills hold the bonuses: the Dragon Sword of Dominion is +3 attack, the Defender Helm of Protection +1 defense, the Caster's Bracelet of Magic +2 spell power and the Breastplate of Anduran +5 defense, as in the table of `ArtifactBonuses`. The Golden Bow, the Endless Sack of Gold, the Elemental Ring and the Fizbin of Misfortune add no skill.
- **Mage Guild and dwellings.** The level is the number in the bits 16 to 18 (Hillstone 1 to 5 over saves 5 to 14), the dwellings of a Necromancer are the bits 0x08 (excavation), 0x10 (graveyard), 0x20 (pyramid), 0x40 (mansion), 0x80 (mausoleum), 0x100 (laboratory), and an upgrade replaces the bit (saves 40, 43, 53, 57: 0x80 to 0x1000, 0x40 to 0x800, 0x10 to 0x200, 0x20 to 0x400). The Library of the Wizard raises the spell counts of the guild from 3 to 4 as it is built (save 64).
- **Daily income.** The income table of a player (6 numbers from +0xAC of the record, then the gold at +0xC4) is recalculated at the start of a day and counts the mines: 2 ore a day for each ore mine and 2 wood for each sawmill, 1 a day for each crystal, gem, sulfur and mercury mine, 1,000 gold for the gold mine, +250 for a statue (save 71: 3 ore mines gave 6, 2 sawmills 4, 3 crystal mines 3, 2 gem mines 2, 2 alchemist labs 2, 2 sulfur mines 2 - the third was captured that day - and gold 4,500 with the gold mine, the statue and the castles). A mine captured today does not count until tomorrow.
- **Days.** The day, week and month (0x2BB) follow the play: save 16 and save 63 are the first day of a week.
- **A captured town.** A hero that captures a town stands in the square in front of its entrance, which is the tile under the castle's tile (Whiteshield is at 30, 16, Colar Blau at 30, 17 in save 29), and the town has no visiting hero (+0x17 is 0xFF).

**New: the buildings of a town (the 16 bits at +0x18)**

Known before: 0x01 Mage Guild, 0x02 Thieves' Guild, 0x04 Tavern, 0x08 Shipyard, 0x10 Well, 0x20 town only, 0x40 Castle, 0x80 Statue. New, each seen in one save for the Necromancer and for the Wizard:

| Bit | Building | Saves |
|---|---|---|
| 0x0100 | Left turret | 64 |
| 0x0200 | Right turret | 66 |
| 0x0400 | Marketplace | 62 |
| 0x0800 | The second well of the faction (Knight: Farm, Barbarian: Garbage Heap, Sorceress: Crystal Garden, Warlock: Waterfall, Wizard: Orchard, Necromancer: Skull Pile) | 39 (Skull Pile), 62 (Orchard) |
| 0x1000 | Moat | 68, 71 |
| 0x2000 | The special building of the faction (Knight: Fortifications, Barbarian: Coliseum, Sorceress: Rainbow, Warlock: Dungeon, Wizard: Library, Necromancer: Storm) | 47 (Storm), 64 (Library) |
| 0x8000 | Captain's quarters | 71 |

0x4000 was not set by any save. A town that is only a town (0x20) built nothing of this before its castle; Whiteshield built its castle in save 35 (0x20 became 0x40). The editor shows all of these with the names of the faction; none is tried in the game yet (T01BLDS).

**New: what a hero remembers (the record of a hero, offsets from the name)**

- **+0x3A and +0x3B: morale and luck for the next battle only**, two signed bytes. A temple gave +2 morale (save 25), a fountain +1 luck (17), a faerie ring +1 luck (60), a graveyard visited for the second time -1 morale (37), a temple and a fountain together +2 and +1 (34). They are 0 again after the next battle (19, 38). The bytes +0xD9 and +0xDA (Price of Loyalty only) hold flags for the source of each (fountain 0x04, faerie ring 0x10, graveyard 0x20, temple 0x01 in +0xDA); the editor leaves them alone.
- **+0x3D, +0x41, +0x45, +0x49, +0x4D: the places the hero has visited**, a 32 bit set each, a bit for each place of the kind in the order the map has them: gazebos (+0x3D, save 69 sets 0x04), forts (+0x41, saves 26, 36, 50: 0x04, then 0x06), witch doctor's huts (+0x45, saves 6 and 61: 0x02, 0x08), mercenary camps (+0x49, save 65: 0x04) and standing stones (+0x4D, saves 45 and 52: 0x02, 0x08). These are the places that give a hero something once per hero (an experience bonus, +1 defense, +1 knowledge or a skill, +1 attack, +1 spell power). Clearing a set lets the hero visit the places again; the editor has a button for it (T02FORGT). The places that give something once a week to the first visitor of any player (the windmill, the water mill) and the graveyard are not in the record of the hero: they are in the map.
- The Price of Loyalty bytes +0xE3 to +0xE6 turn 0xFF one after the other as a hero carries more artifacts; what they are is not known.

**The map (after the castle table, 108,843 bytes in this map): see "The map part of Threedom" below for what was decoded afterwards. What was known when the first 71 saves were read:**

What the saves showed, for the next person to try:

- Capturing a mine, visiting an obelisk, picking up an artifact or capturing a town changes bytes only in the map part (and the owner's resources), never in the record of a player: the map holds who owns a mine, which obelisks a player has seen, and the flag of a town. Nothing about mines is in the player record besides the daily income table.
- The map part starts with lists and arrays that are not one byte per tile (the first 0x1440 bytes are 0xFF for most positions with a few small numbers, then text: the messages of the map), then a part with one byte per tile of 0x80/0x00 flags (what a tile blocks, rows of 72 bytes), then a table of 7 byte entries (`class, index, flags, 0, 0xFF, link low, link high`; `62 0A 02 00 FF 00 00`) about 0x13C00 to 0x1A800, which is where the flags of the mines and towns live: capturing a mine rewrites a few entries, and the entries are moved around (so a captured mine shifts bytes elsewhere).
- Walking reveals rows of the explored area as runs of 6 to 9 bytes spaced 72 bytes apart in the 0x4000 to 0x4600 part (about 0x1000 bytes of the map part after the castle table).

Decoding these needs a probe that changes one thing: for example a save of a hero standing next to one mine before and after the capture, with the positions of the mine written down.

## The map part of Threedom (2026-10-17, saves 1 to 82)

Offsets are from the end of the table of castles ("M"). Everything below was seen in the 82 saves of Threedom, and the structure that does not depend on the map (the table of tiles, the obelisk bytes, the mine records) was also looked for in the other maps of the test saves.

**The table of tiles.** Somewhere after M the width and the height of the map are written again as two 32 bit numbers, and then the tiles follow: 12 bytes each, row by row from the top left, `width * height` of them (Threedom: at M+0x49B8). A tile is:

| Byte | Meaning |
|---|---|
| 0, 1 | the ground picture |
| 2, 3 | the kind and the picture of the object on the tile (the kind varies with the terrain, so it is not used to find objects) |
| 4, 5 | flags of the tile (what it blocks, and 0x0188 or 0x0100 when a hero stands on it) |
| 6, 7 | a second object, such as the flag of a mine: kind 0x38 and the picture 14 (0x0E) for a mine, 21 for an alchemist's lab, 28 (0x1C) for a sawmill, plus 0 to 5 for the colour of the owner |
| **9** | **what the object a hero enters by is**; this is how the places are found. 0x99 obelisk, 0x8A gazebo, 0xBE fort, 0xC5 witch doctor's hut, 0xC9 mercenary camp, 0xC2 standing stones (two tiles), 0xD6 Xanadu, 0x9F shrine of the first circle, 0xCB of the third, 0xC6 temple, 0x89 fountain, 0xA8 windmill, 0x96 water mill, 0x8C graveyard, 0xA3 the gate of a castle. **On the tile of a hero the byte is 0xAA** and the hero's own record keeps the real one at +0x23 (from the name): that is why a place a hero stands on is not lost |
| 10, 11 | a number that links the tile to an entry of the table of objects after the tiles |

The counts of the bytes at 9 agree with the pictures where both were checked (the map of Threedom: 9 obelisks, 5 gazebos, 5 forts, 6 witch doctor's huts, 4 mercenary camps, 7 standing stones, 1 Xanadu), and the byte at 9 is better than the picture across terrains (the largest test map has 23 gazebos by the byte and only 21 by the picture).

**Obelisks.** At M+0x538 are 48 records of 8 bytes (`id 00 00 00 00 00 FF 00`, the id counting from 0), then 48 bytes of 0xFF (or a few numbers), and at **M+0x6E8 one byte for each of 48 possible obelisks: a bit for every colour that visited it** (bit 0 Blue, 1 Green, 2 Red, 3 Yellow, 4 Orange, 5 Purple; the AI players set theirs as they walk). The obelisks are in the order of the map, row by row, and the obelisks are the tiles with 0x99 at byte 9. Saves 4, 12, 42, 54, 75 and 80 each set the bit of Blue of one obelisk, and the hero stood on it: the obelisk at 28, 29 is the fifth (Charity, save 75) and the one at 1, 62 the seventh (Colar Blau, save 80). The editor lists them on the player page, like Heroes of Might and Magic III.

**What a hero remembers (the record of a hero, offsets from the name)**

- +0x3D, +0x41, +0x45, +0x49, +0x4D: sets of the gazebos, forts, witch doctor's huts, mercenary camps and standing stones the hero visited, **a bit for each place in the order of the map and the first place is bit 1** (bit 0 is not used): the fort at 1, 4 is the first fort of the map (bit 1) and the one at 29, 18 the second (bit 2), and Colar Blau's and Charity's visits to them set exactly those. The standing stones are counted by the left tile of each. +0x55 is the same for Xanadu (save 82: Charity set bit 1 and got +1 in each primary skill). +0x51 was never set.
- +0xD9 (32 bits): the game's memory of what a hero visited that gives a bonus for a while. Bits seen: 0x04 fountain (+1 luck), 0x08 oasis (+1 morale, and the maximum movement of the day went from 1,559 to 2,359), 0x10 faerie ring (+1 luck), 0x20 a graveyard that had been visited (-1 morale), +0x0100 temple (+2 morale); and for good or for a week 0x400000 the arena (a point to a skill, once per hero; Charity's defense went from 5 to 6) and 0x800000 the stables (the maximum movement went up by 400; the user says it ends at the start of the week). The game clears the first group with the bonus after a battle.
- +0x3A and +0x3B: the morale and the luck (signed) that come from those. **`T03BONUS` of the editor set only these two bytes**: the troops showed the bonus (and kept it through later battles) but the hero's own screen did not, so the screen works from the bits of +0xD9 and the bytes are what the troops use. The editor now sets both together (T06SRC).

**The mines (the records of 7 bytes from M+0x05).** `owner, kind, FF, FF, x, y, FF`: the owner is a colour (0 Blue) or FF, the kind is 0 sawmill, 1 alchemist's lab, 2 ore mine, 3 sulfur, 4 crystal, 5 gems, 6 gold; x and y are the position of the mine (the sawmill captured in save 13 is record 43, `00 00 FF FF 2C 38 FF`: 44, 56). Capturing a mine sets the owner here, adds the flag as the second object of a tile (bytes 6 and 7) and the income is recalculated at the start of the next day. Not edited: moving the owner would also have to move the flag.

**Objects with a state of their own (not decoded, not editable yet).**

- **Daemon cave** (save 73), **sphinx** (74) and the windmill and water mill (once a week): nothing changes in the record of the hero besides the reward; in the map part the pictures of the object change (an entry of the table of objects after the tiles goes from `5C 0C` to `5E 00`).
- **The sphinx is in the first part of the map:** a block that starts with `D2 04 00 00` (1234), an id, then a byte that is 1 until the riddle is answered (it went from 1 to 0 at 0xDD1 when Colar Blau answered, save 74), the reward (8,000 gold, an artifact), the number of answers (2) and the answers (13 characters each: **"darkness" and "shadow"**; the sphinx of this map accepts those two), then the text of the riddle (at 0xE59 of the map part, and the messages of the map, signs and rumors after it, all in the savegame).
- **Shrines:** a hero who visits one learns the spell, which is marked in its spell book (saves 78 and 79: Mass Haste, Mass Bless; the spell is part of the object, the second circle shrine is not in the test saves). A shrine is not used up.
- Save 72 was made from `T03BONUS` of the editor, so Colar Blau already had +3 morale and +3 luck there and Charity -3 and -3; they are not from the arena.

## Shrines, sphinxes and arenas (2026-10-17, Threedom saves 72 to 79 and the other test maps)

**The first part of the map is a list of blocks.** Each starts with `D2 04 00 00` (1234), a 16 bit length and the payload: signs (17 bytes, the text "To Agal"), events and messages (the text of a rumor, a bar, a hero's story), and the objects with state. Blocks do not hold a position: the block of an object is found by the order of the objects of its kind on the map (row by row), as the bits of a hero's visited places are.

**The sphinx (a tile with 0xCF at byte 9, 1 on the map of Threedom, at 13, 31)** has one block (335 bytes in Threedom, 377 in Arrax the Jerk), its payload being:

| Offset | Size | Meaning |
|---|---|---|
| 0 | u8 | **1 while the riddle has not been answered, 0 after** (Colar Blau answered in save 74: 1 to 0 at the start of the payload) |
| 1 | 7 x u32 | the reward in resources: wood, mercury, ore, sulfur, crystal, gems, gold (Threedom 8,000 gold; Arrax 15 mercury) |
| 29 | u16 | the artifact reward, 96 in both maps with none given (it is also the code of the Legendary Scepter: read as none) |
| 31 | u8 | how many answers (Threedom 2, Arrax 6) |
| 32 | 8 x 13 bytes | the answers, text padded with zeros (Threedom: "darkness" and "shadow"; Arrax: "1", "one", "1 bushel"...) |
| 136 | text | the riddle, ending with a zero ("I Only Creep. I Never Run...") |

The riddle and the answers are in the savegame, so they can be read and edited; the game compares the answer typed with the list. The gold of the reward arrived with the answer (the gold of save 74 grew by 23,100, which is three days of income and 8,000).

**A shrine is a tile; its spell is in the tile.** The kind of the circle is the byte at 9 (0x9F the first circle, 0xCA the second, 0xCB the third), and the spell is **the 16 bit number at tile bytes 4 and 5 shifted right by 3** (the low 3 bits are the tile's own flags and are kept): Threedom has Shield (42, the first shrine, learned in save 20: `0x0150 >> 3`), Haste (10), Dispel Magic (24) and Cure (6) in the first circle; Steelskin (18), Haunt (61) and 56 in the second; Holy Word (21), Identify Hero (55), Earthquake (48), View Towns (52), Mass Haste (11) and Mass Bless (16) in the third. The shrine of save 78 is the one at 29, 42 (Mass Haste) and of save 79 at 31, 47 (Mass Bless): the tile numbers agree with what Charity learned. All of it checked in the six maps of the test saves (the spells of the first circle are all of level 1, those of the third mostly level 3). A shrine is not used up and leaves no mark in the map; the hero's spell book is the only memory.

**Arena and stables.** Both tiles have 0xFA at byte 9 (7 tiles on the map of Threedom: the ones with `F4` at byte 2 and the ones with `F8`; the stables of save 77 is `F8 04`, the arena of save 72 `F4 46`). A visit changes only the hero's bits (0x400000 for the arena, 0x800000 for the stables of the field at +0xD9) and the skill: the map has no mark of who used an arena, so **the memory of an arena is one bit of the hero, not one for each arena**. Whether the game really gives a hero a bonus from each arena was not tried: it would need a hero that has used one arena visit another (probe saves: Charity, who has used the arena at 44, 28, at the arena at 58, 31; Colar Blau at 58, 31 and then at 19, 33).

**The editor now lists them** (the Map page under the game, with a tab for the mines, the shrines and the sphinxes). Two things about the mines: a mine is also a flag on the map. The record's owner is the one the game counts; the flag is a picture (a second object of a tile, or an addon the tile links to) whose number is the first picture of the kind (14 for the mines, 21 for the alchemist's lab, 28 for the sawmill) plus the colour of the owner. The editor writes both, except for the alchemist's lab (the saves have no flag for it, even for the ones Blue captured). The flag of a mine that is nobody's is the empty object (class 0, picture 0xFF). A hero standing on a shrine hides its spell (the tile holds the marker of the hero): the shrine is listed but not editable until it moves.
