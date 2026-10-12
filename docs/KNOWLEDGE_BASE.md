# Knowledge base

Findings about the savefile format that are not obvious from the code, one section per topic.
How each thing is found is in `docs/LOCATING_HOMM3_ENTITIES.md`; the saves that show it are in the READMEs of `TestData`.
Offsets are from the start of the chunk unless said otherwise.
"Sure" means confirmed by diffing saves or against the game; anything else is a hypothesis.
Add new findings to the section of their topic.

## Savegame format

A HoMM3 savegame is supposed to be a GZip file, but common tools and libraries fail on it or complain about a CRC error: 7-Zip extracts but complains, .NET's `GZipStream` and sharpcompress throw on the compression method, and SharpZipLib raises a CRC exception that can stop decompression halfway through.
This project therefore includes the GZip part of SharpZipLib 1.2.0 with the line that raises the CRC exception commented out.

## Savefile container

- Saves (`.GM1`, `.GM2`, `.GM8`, `.CGM`, ...) are gzip files whose CRC does not match, so strict decoders fail (Python `gzip` raises "CRC check failed", .NET `GZipStream` "unsupported compression method").
  The editor uses the SharpZipLib copy in the repository (`GzipSavefileCodec`).
  In Python, skip the 10-byte header and inflate the raw stream: `zlib.decompressobj(-15).decompress(raw[10:])`.
  The stream is complete (it ends with an 8-byte trailer).
  Saves edited by hand or by other tools show the same CRC error and still load.
- The decompressed data starts with the ASCII magic `H3SVG`.
  The first few hundred bytes hold the map's title and description as plain text, then the map file name (for example `Free for All.h3m`), `maps` and the marker `GAME_BEGIN`.
  Hero and town names are plain text too, in fixed-size zero-padded fields.
- When searching for a string, search the decompressed bytes, in ASCII and UTF-16, and check the surrounding bytes: partial hits are common ("Jenova" and "Christian" while looking for "Jenna" and "Christina").
- **Hotseat player names are not in the save.** The game assigns them at runtime.
  The save only says which colours are human.

## Header: map, setup and teams

- **Map name and description** are each a 16-bit length and ASCII text.
  The name starts at byte 95 in HotA saves and byte 58 in SoD saves.
  Some names keep a leading space (`" The empire of the world"`).
  Non-ASCII names would break `MapHeaderLocator`.
- **Map difficulty** is the byte after the description, 0 Pawn to 4 King (3 = Queen checked against the game, 0 to 2 only against samples; `map_difficulty_edited_to_expert`).
  It is separate from the game difficulty of the setup block.
  The byte after it was 0 everywhere and is not read.
- **Map size** is a 32-bit integer five bytes before the name (36, 72, 144 seen).
  The byte before the name is read as the underground flag: 1 in nearly every sample, 0 only in `bios_custom`.
  This is the **least certain** item; it was not checked on a map with an underground.
- **Human flags**: eight 32-bit values in colour order (1 = human), followed by the colour whose turn it is, then a marker: `FF 10` eight bytes later in HotA, `FF 00 00 00` right after in SoD and RoE (+36 from the flags; offsets 1130, 1228, 1262 and 1286 seen, so locate by shape).
  The header's one-byte-per-colour list (`FF` not in game, `0A` computer, other = human) only names the first human of a hotseat game.
  Found by comparing `players_human_red`, `players_human_tan` and `players_human_red_and_tan`; only red and tan were seen human, so the stride for the other colours is assumed.
  Current turn checked on four saves.
- **Teams** (SoD and RoE; not found in HotA saves; not read by the editor): in the header, just before two lists `00 01 02 03 04 05 06 07`, a byte with the number of teams, and when it is not 0, eight bytes with the team of each colour (Red, Blue, Tan, Green, Orange, Purple, Teal, Pink).
  The hotseat map has `03 | 00 00 01 01 01 02 00 02`; Teal, which nobody plays, keeps the default 0.
  With no teams the byte is 0 and there are no team bytes.

## Players

The struct of a player starts `PlayerLayout.HeadLength` (97) bytes before its first resource, and a player chunk is the whole struct (145 bytes in SoD, 149 in HotA).
The head is the same for every colour and in SoD and HotA (`PlayerHeadTests`).
From the start of the chunk:

- 0: number of heroes of the player. 1: the selected hero (`FF` for none). 2 to 9: the eight places for hero ids, `FF` for an empty one; a garrisoned hero is not in the list.
- 10 and 11: the two heroes of the tavern, by hero id (`PlayerModel.Tavern`).
  The computer's taverns change both heroes at the start of a week and, within a week, only the place of a hero the computer hired (`SodSeriesTests`).
- 19: changed from 03 to A7 when a hero dug; unexplained. 23: number of castles. 24: the selected castle (index in the table of castles, `FF` for none), exclusive with a selected hero.
- 129 (Red): became 1 with the first visit to a magic spring and stayed 1 on the other square and after the week change.
  It is **not** the visit mark, which the game keeps per square, probably with the map objects.
  Open.
- The two bytes of another player's struct that change when resources are traded are bytes 1 and 24, the selected hero and town.

**Who is in play.** A save has a struct for every colour.
A colour is listed when it has resources, heroes or a town, and the setup does not say "not in the game" for one without heroes or towns (`PlayersInTheGame`).
`tan_not_parsing`: Tan is not in the game but has 10000 gold in the table, like a computer player never given a start; the colour that plays is Teal (confirmed in the game).
A human with two heroes and no resources, as in `SleepGarrisonVisiting` saves 1 to 3 and Blue in the hotseat saves, counts through its heroes.
A slot with the marker but no valid colour byte (an unused last slot) is named after the colour that follows the previous one.

**Not checked in the game**: editing the tavern.
A table of `00`, `40` and `FF` bytes 230 to 290 bytes before the date also changes when a hero flees or is defeated, so a tavern hero set by the editor may be treated differently from one the game chose.
The tavern page offers only heroes no player owns, and never the same hero twice.

## Castles

Layout (owner, faction, position, shipyard tile, garrison, hero ids, name, dwelling counts, Mage Guild counts, and the built, current and enabled masks) is the same in HotA and SoD, and every castle writes back to identical bytes.
RoE, AB and Chronicles saves were not seen.

- **Buildings** are one bit each in a bitmask, with an "enabled by the map" mask beside it.
  Bit numbers were taken from the bit that changed between consecutive saves.
  A horde building, an upgrade or a unique building that replaces another is cleared in the *current* mask but stays in the *built* mask (Miners' Guild replaces the Dwarf Cottage, Cage of Warlords the Gnoll Hut, Capitol the City Hall).
- **Faction codes**: Inferno 3, Necropolis 4, Dungeon 5.
- **Bits the game sets by itself**: 19 and 25 (the upgraded growth buildings, below), 22, 23, 27, 28 and 29.
  The editor writes the companion bits that go with a building (`SafeCastleSerializer.CompanionBits`) and leaves the others as the game wrote them:
  - Town hall 27, city hall 28, capitol 29: Rampart, Necropolis, Conflux.
    Fort 27: Bulwark, Factory, Stronghold.
    Fort 22: Conflux.
  - **Bit 23 is not written**: it is set with the shipyard in a Castle and with the fort in a Fortress, and is already set in Rampart and Necropolis (also in a village-hall-only town).
    The cause is unknown, and whether the game cares when it is missing was not tested.
- **Mage Guild spells** follow the castle chunk (from the end of the building masks): five lists of 32-bit spell codes, 24 bytes apart (level 1 at +0, level 5 at +96), -1 for a level the faction cannot build.
  They are picked when the map starts and are the same in every save, built guild or not.
  The guild shows the first *count* entries of each level (five count bytes in the chunk).
  In RoE and SoD the entry after the visible ones is the Library's extra spell (Tower only); in HotA the slots after the visible ones are empty (0, which is also Summon Boat) or hold other data, so read exactly the count.
  Whether the map's allowed spells and secondary skills are stored is not known.
- **Growth buildings**: bits 19 and 25 ("Upg.
  Griffin Bastion", "Upg.
  Miners' Guild", ...) are set when a growth building and the upgrade of its dwelling both exist.
  The growth building works on both dwellings and has no upgrade, so the editor keeps the bits consistent (`CastleBuilder.IsAutomatic`) but does not list them.
- **Shipyard**: only coastal castles can build one; it stays disabled in the mask for an inland castle.
- Stronghold and Fortress stop at Mage Guild level 3 and Castle at level 4.
  In Fortress, Captain's Quarters is the unique building (bit 17) and Cage of Warlords the horde building (bit 18).
  In a garrison the strongest creature is slot 1; every castle has seven slots.
- **Captured castle**: only the owner byte changes and the neutral garrison is gone; the player's castle count grows.
- **Prerequisites** come from VCMI.
  A chronological series can show that one is too strict, but not that it is right.
  `dung_necro` was built in the map editor, which ignores prerequisites, so it proves bit positions only.
- **Dwelling growth** (Conflux, SoD): 20, 6, 6, 5, 4, 2, 2 a week and 60, 18, 18, 5, 4, 2, 2 (110, 30, 30, 15, 12, 6, 6 upgraded) once the buildings have stood for a week.
  Building the upgraded Magic Lantern gives the upgraded Garden of Life by itself (bits 19 and 37).
  The Conflux dwellings are the Magic Lantern, the Altars of Air, Water, Fire, Earth and Thought, and the Pyre.
- **Garrisoned and visiting hero**: the castle holds both ids (`CastleLayout.VisitingHero`, bytes 65 and 66 in HotA).
  See "Heroes".

Per faction and game:

- **Rampart**: Grail building bit 26.
  **Conflux**: Grail is Aurora Borealis (bit 26); the Vault of Ashes is bit 24 with the game-set upgraded bit 25 beside the upgraded Pyre.
- **Bulwark (HotA)**: no Mage Guild level 5 (bit 4 is never enabled).
  Dwellings are bits 30 to 36, upgrades 37 to 43.
  The names of the special buildings (Fair 17, Hay Bins 18, Sieidi of the Runes 21, Altar of the Runes 22, Spirits of the Forebears 26) were **inferred by elimination**, so check them on the town screen; the bit numbers and the hide/replace behaviour (Hay Bins hides the Mammoth Stalls, Altar of the Runes replaces bit 21) are confirmed.
  Bulwark's shipyard bit 6 rests on the standard numbering because no save built it.
- **SoD**: 51 saves of the RoE map with the eight RoE factions built one step at a time (`TestData/Roe`) are reproduced step by step by `CastleBuilder` (`RoeBuildingEditsTests`), and the Conflux series the same (`SodSeriesTests`).
  Not seen: Capitol in some factions, and RoE, AB and Chronicles saves.

## Heroes

A hero record is the chunk plus a tail: in SoD the chunk is 1082 bytes and the record 1094, so 12 bytes follow; `HeroBytesLoader` includes them in the SoD chunk.
HotA has twelve bytes there too, laid out differently (`D0 07` in the second and third byte, `FF` in most of the rest), and they are already inside its longer chunk.

- **Moved today** (HotA): byte 1095 is 1 once the hero has moved and 0 after a new day (`MovedTodayTests`).
  **SoD does not store it**: checked on 847 heroes (43 with fewer movement points than their maximum), no byte of the record separates them except the movement points (`SodSeriesTests`; `sanya_01` and `sanya_05` show a move changing only y of the position and its copy, the object under the hero, the facing and the movement points).
  The game probably compares current with maximum movement points (a guess, not seen in the game's code).
  Byte 1095 is also past the end of a SoD record (1094 bytes), so it would read the next hero.
  A diff of the whole decompressed file for two pairs (`sanya_04` to `sanya_05`, `roe_56` to `roe_57`; the SoD gzip CRC is wrong, inflate it raw) found no stray flag elsewhere: only the header (file name), the two tile records at the old and new position, the hero, and a list of path steps appended at the end of the file (it carries the hero's object id and survives a new day, so it is the remembered route, not a daily flag).
  Only `sanya` also flips per-object bytes (stride 144, near the end) and one counter byte; `roe` does not, so neither is a general flag.
  Not covered: the 4282-byte block that appears on a new day, and a hero that moved and came back to full movement points.
- **Active adventure spells**: bytes 31 to 35 hold the adventure spells cast today: 31 Disguise, 32 Fly, 33 Water Walk, 35 Visions; 255 is not cast, 0 is cast, a new day resets them to 255; 34 is always 0.
  Right on HotA and SoD (`ActiveSpellsTests`); unverified on RoE and AB.
  They are cleared by any visit to a town.
  Flying and water walking from artifacts are not stored on the hero.
- **Available spells** are the learned ones plus those given by equipped artifacts.
  Tomes also give Visions and Magic Arrow, which the editor treats as belonging to no school.
- **Dig**: the current movement points become 0 and the hero stands on an object of type 124 (a hole).
  The tile is added to the list of dug tiles (see "Date and map objects").
  Whether a dig finds the Grail is not in the save.
  After a successful dig the first backpack slot holds id 2, the Grail (`Artifact.CarriedInBackpack`: never offered by the finders, kept and shown, and can be taken out).
- **Standing on an object**: +11 is the 32-bit type of the map object under the hero (98 town, 57 obelisk, 38 Idol of Fortune, 28 Faerie Ring, 83 Seer's Hut, 124 hole; 0 on bare ground), +15 is 1 when it can be visited, +16 the index of the object among those of its type (the castle index for a town, the obelisk index for an obelisk).
- **On the map**: +6 is 1 for a hero on the map (a visiting hero too) and 0 for one inside a castle or gone (fled, defeated).
  Garrisoning changes nothing else but the active spells and spell points.
- **Asleep** (`HeroModel.IsAsleep`, SoD and HotA): byte +1087 is 1 for a hero put to sleep and goes back to 0 when it moves.
  In SoD it is in the tail, in HotA inside the chunk; every owned HotA hero of every save has 0 or 1 there.
  A **garrisoned hero keeps the flag**.
  The computer's guards (heroes it garrisons or parks in a castle) have it set.
  Other versions were not seen.
- **Bonus effects** (until the next battle, or the next week for some).
  Read: take the byte, mask the bit, non-zero means on.
  `HeroModel.MoraleBonus`, `LuckBonus` (read only) and `VisitEffects` (editable) are shown on the General page.
  - +28: morale bonus, +29 luck bonus, signed bytes (watering hole +1, crypt -1; swan pond +2, fountain of fortune +1).
  - +135: `0x02` stables (lasts the week), `0x08` swan pond, `0x40` watering hole. +136: `0x04` crypt, `0x20` Faerie Ring. +138: `0x08` Fountain of Fortune, `0x02` Idol of Fortune.
  - A battle clears luck, the fountain bit, and the Faerie Ring and Idol bits; a new day clears the stables bit.
    Visiting again after the effect ended grants it again, so the "visited" table beside the date is not what decides.
  - Unnamed and left alone: `+135 0x10` (set when the ring and the idol were both visited in one turn, gone after a battle) and `+138 0x01` (set at the first castle visit, survives battles and new days).
    Effects of other objects (Oasis, Temple, ...) were not looked at.
- **Fleeing or defeat** (partly understood): the owner byte becomes `FF`, +6 becomes 0, and the `0x40` bit of +1088 is set.
  The bits of +1088 are **not understood** otherwise (a hero can have `0xCE` all the time), so do not read it as "not on the map".
  A hero that fled takes the second place of its player's tavern; one that was hired again and defeated is not in the tavern.
  How "does not come back until the others have been seen" is stored was not found.
- **Garrison, visit and swap** (HotA `SleepGarrisonVisiting`, SoD): the castle holds the garrisoned and the visiting id (`FF D3`, `D3 FF`, `D3 C7`, ...) and a swap exchanges them.
  Each hero's +6 is 1 for the one at the gate and 0 for the garrisoned one; a swap flips both and changes nothing else, except that the hero coming out changes its facing (+41).
  The player's list of heroes holds the hero at the gate only (selected hero too).
  Sleep flag, moved flag and active spells do not change in a swap.
  So a swap changes four chunks (castle, two heroes, player), which no edit session does yet; a hero leaving a castle was never seen.
- **Legion artifacts**: equipping Head and Arms of Legion changes nothing in a castle; the growth is worked out at the week change.
  In the Stronghold visited by Crag Hack the castle was ahead by 3 creatures on level 5 and 1 on level 6, where the artifacts say +2 and +1.
  Unexplained.

## Obelisks

`ObeliskChunk` / `ObeliskVisits`, edited on the **Obelisks tab of the player page**. 53 bytes before the start of Red's chunk (Red's struct, in play or not) there is a marker `FF 00 00 NN` (NN the number of obelisks of the map), then NN bytes in the order of the obelisks of the map, not of the visits.

- **The flag is a bit mask by colour**, bit n for the colour with index n (Red 0, Blue 1, Tan 2, Green 3, Orange 4, Purple 5, Teal 6, Pink 7).
  A visit sets the bit of every colour in the visitor's team: with Red and Blue in team 1 and Teal (default) there too, Red's visit gives `0x43`.
  With five teams (`five_teams_*`, team table `05 | 00 01 02 03 04 04 00 04`) Red's two visits give `0x41` (Red and Teal, which shares team 0), and after Blue visits the same two they are `0x43`.
  What another team visits does not count for a player.
  The editor edits one colour at a time; marking a whole team needs the team table.
  In the single-human RoE series only `1` was ever seen.
- NN is the number of obelisks of the map: for every save with the marker and NN above 0, the distinct obelisk numbers found in the map objects (type 57) are NN, or NN - 1 when a hero stands on one and hides it (four maps; `ObeliskVisitsTests`).
  Visited flags were only seen change on the RoE map and the hotseat map.
- The marker is **not always there**: in `sod_day_02_m1w1d2` and `sod_day_03_m1w1d3` (no obelisks on the map) other data sits in its place.
  `ObeliskBytesLoader` then returns nothing and the tab is not shown.
  Nothing seen displaces the flags of a map that has obelisks.
- **Coordinates** of an obelisk come from the list of map objects: after the table of object definitions and a 16-bit count there is a (definition, x, y, z) entry for each object, and the record of an obelisk holds its place in that list.
  A hero standing on an obelisk hides its record, so it has no coordinates in that save.
- Not found: where the visits of other players are, and anything the puzzle map needs besides the flags (its pieces seem to follow the number of visited obelisks).
  An edited obelisk mark was not checked in the game.

## Date and map objects

The date (`day`, `week`, `month`, 16 bit each) follows the marker `7F xx 02 yy`: `xx` is 1 until the Grail is dug up and then 0; `yy` became 1 when a hero visited a castle with a garrisoned hero and stays 1 (unknown).

- Other data that looks like a date occurs in some saves (week 4 day 3 read as month 2 week 4 day 5, month 60), so `GameDateLocator` prefers a match with the marker.
  That changed where the date is found in 38 saves (the Conflux ones were wrong before); the HotA "empire of the world" saves have no marker and use the first match.
- A table of two bytes per object type seems to start at +12 from the date: the byte at +68 is set once a Faerie Ring (type 28) was visited and +88 once an Idol of Fortune (type 38), at 12 + 2 x type, and they stay set.
  Seen for two types only: a hypothesis.
  The 7 values at +38, +42, ..., +62 and the 20 flags from +107 were constant.
- **Dug tiles** (sure): a list of 5-byte entries (x, y, level, 16-bit number) in front of a message text ("Uh oh... devil tracks in the grass") that is in every save; entries stay.
  The number (154 and 536 seen) is not understood.
  A dig also adds a map object; `AVLholr0.def` appears only when the Grail was found.
- **Visited objects** are not recorded on the object: the cell of a visited Idol or Faerie Ring reads as before once the hero left.
  The cell of an object has its type, its number and the hero standing on it (`FF FF FF FF` for none).
- **Fog of war** is a long list of 00 and 01 bytes after the map (changes with every step) and a **movement trail** (15 bytes per step: hero id and tiles) is appended at the end of the file.
  Neither matters for editing.
- At a new week the castles' weekly growth appears.

## Hex views

- **Hero with a biography**: the 26-byte hero header (position, gender at 20, the 32-bit biography length at 22) and the biography text sit *in front of* the hero chunk, because the chunk starts 26 bytes before the faction byte, inside the end of the biography.
  The first 26 bytes of such a chunk are biography text, so `SaveRegionProvider` drops the chunk fields below offset 26 and uses `HeroRegionProvider.GetHeaderRegions`.
- **Names are not unique**: `sod_game_00_baseline` has heroes or castles that share a name, so anything that matches regions to chunks goes by position (`StartIndex`), not by name.

## What is verified and what is assumed

- Players, date and difficulty: verified on Horn of the Abyss and Shadow of Death saves.
  The castle layout and building edits are verified on both too (`TestData/Roe`).
  Restoration of Erathia and Armageddon's Blade are assumed to match Shadow of Death; no saves of those were seen.
- Everything was read from a few maps per version, so a shape that is unique there may not be on another map.