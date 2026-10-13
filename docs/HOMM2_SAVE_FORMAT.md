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
| 0x0C | u16 | difficulty (1 = normal) - guess |
| 0x0E,0x0F | u8,u8 | width, height again (0x48) |
| 0x10.. | 6 x u8 | colours in play (Blue, Green, Red, Yellow, Orange, Purple), then human-allowed and computer-allowed masks - guess |
| 0x42 | char[16] | map name "Broken Alliance" - confirmed |
| 0x76 | char[...] | map description - confirmed |
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
| 0x17 | u32, u32 | target tile of an unfinished move (x, y); 0xFFFFFFFF when none. Save 5 (movement ran out) holds 23,23 - guess |
| 0x22 | u8 | changes with every move (2,3,4,7,2,...): facing/direction or a sprite phase - guess; 0 while inside a castle |
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

- **Level and experience, solved (E30 to E33).** The game works the level out from the experience and ignores the level byte: level byte 1 with 50,000 experience showed level 17 (next level at 54,244), level byte 20 with none showed level 1, level byte 1 with 200,000 showed level 25 (next at 231,549). `Homm2HeroLevels` uses the fheroes2 table (54,200 and 232,700 for those two thresholds, so slightly off); the hero page sets the level from the experience and the experience to the start of a level when the level is set. (The level 14 of E25STATS is not explained: that save's file holds level 20 with 50,000.)
- **Stacks.** 32,767 peasants and 9,999 rogues display and split fine; adding one more overflowed to -32,768 (E33STACK), so 32,767 is the limit.
