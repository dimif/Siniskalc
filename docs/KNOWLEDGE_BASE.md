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
  Read and written as `PlayerModel.HeroIds` and `SelectedHero` (the list and the count are written together, and only when the places do not hold the model's ids; see "The table of hero owners" below for how an owner edit changes them).
- 10 and 11: the two heroes of the tavern, by hero id (`PlayerModel.Tavern`).
  The computer's taverns change both heroes at the start of a week and, within a week, only the place of a hero the computer hired (`SodSeriesTests`).
- 19: changed from 03 to A7 when a hero dug; unexplained.
- 22: **the days left before the player is banished for owning no town** (`PlayerLayout.DaysLeft`, `PlayerModel.DaysLeft`): `FF` while the player owns a town, 7 the day the last town is lost, one less each day, 0 once banished.
  Regaining a town sets `FF` at once. Found in `TestData/Hota/Banishing` (`PlayerBanishingTests`), in which Blue loses his last town, regains one, loses it again and is banished; same meaning as HoMM2's 0x304.
  **Not checked in the game**: that editing it to a lower or higher value makes the game banish the player earlier or later.
  Banishing also changes bytes the editor does not write (Blue's hero chunk gets owner `FF` and an emptied army, a counter and an 8-byte record `02 00 00 00 00 FF 01 08` appear in the 579000s, header bytes near 1313, 1356 and 12492), so a banished player (0) is shown but cannot be edited.
  When the editor gives a castle to another owner (`CastleEditSession.ChangeOwner`) it applies what the saves show: the player who loses his last town gets 7 at once (`banishing_00` to `01`, `04` to `05`), a player who gains a town while counting gets `FF` at once (`01` to `02`), and losing one town of several or gaining one while not counting leaves `FF`; a banished player is not changed (no save shows it). The towns a player owns are counted from the owners of the castles. HoMM2's castle owner change applies the same rule (`PlayerModelBase.DaysLeftOnGainingATown` / `DaysLeftOnLosingATown`).
- 23: number of castles. 24: the selected castle (index in the table of castles, `FF` for none), exclusive with a selected hero.
  The number follows ownership: it went up or down by one with each castle gained or lost in every pair of saves looked at (`banishing_00` to `01` and `01` to `02`, `castles_00` to `01` in `TestData/Hota`, `roe_63` to `64`).
  **The editor does not write it yet** (nor the list at 25): `SafePlayerSerializer` leaves both as they were when a castle changes owner, as the order the game keeps the list in is not established. So after an owner edit the count is stale; the game may fix it, not checked.
- 25 onwards: the indices of the player's castles in the table of castles, `FF` for none (Blue's castle 8 at 25; Red's third castle went to 27). Not read.
- 129 (Red): became 1 with the first visit to a magic spring and stayed 1 on the other square and after the week change.
  It is **not** the visit mark, which the game keeps per square, probably with the map objects.
  Open.
- The two bytes of another player's struct that change when resources are traded are bytes 1 and 24, the selected hero and town.

**Who is in play.** A save has a struct for every colour.
A colour is listed when it has resources, heroes or a town, and the setup does not say "not in the game" for one without heroes or towns (`PlayersInTheGame`).
`tan_not_parsing`: Tan is not in the game but has 10000 gold in the table, like a computer player never given a start; the colour that plays is Teal (confirmed in the game).
A human with two heroes and no resources, as in `SleepGarrisonVisiting` saves 1 to 3 and Blue in the hotseat saves, counts through its heroes.
A slot with the marker but no valid colour byte (an unused last slot) is named after the colour that follows the previous one.
The byte in front of the marker (`PlayerLayout.FlagOffset`, 44 bytes after the resources) is **1 in most players and 0 in some**: Red's is 1 and Blue's 0 in `banishing_14` to `16`, while every player has 1 in `banishing_00`. What makes it 0 is not known, so the loader does not look at it (it once did, and found only the four players after Blue: Red and Blue had no page).

**Not checked in the game**: editing the tavern.
The table of hero owners (below) marks the heroes the taverns offer with `40`, and the tavern page does not write it, so a tavern hero set by the editor is not marked as one the game chose.

### The table of hero owners (found in `TestData/Hota/HeroOwners`, 2026-10-24)

A byte per hero id, in the order of the ids (`HeroOwnersLayout`, `HeroOwnersModel`): the colour of the player who has the hero (0 Red to 7 Pink), `FF` for nobody, and `40` for a hero the game keeps from being drawn for a tavern (the two heroes of every tavern, and the campaign heroes 145 to 155). Hero 3 has `41` in every HotA save while nobody has it and no tavern offers it: unexplained, read as "kept out of play", left alone.
It is 156 bytes in SoD (the `Roe` saves are SoD saves) and 215 in HotA; a second table of as many bytes, all `FF` in every save, follows, then `00 08`, two 16-bit numbers that vary, a zero byte and the 7F marker of the date. So `HeroOwnersBytesLoader` finds it from the date (`date - 11 - 2 * length`) and checks it (every byte `FF` or a colour with or without `40`, the second table all `FF`, the tail `00 08`); it is never searched for. It was found and checked in all 447 HoMM3 saves of `TestData` (`HeroOwnersBytesLoaderTests`): the table says the owner byte of every hero chunk, every hero of a player's list has the player's colour, every tavern hero has `40`, and an owned hero in no list is garrisoned in a castle (Blue's hero 29 in `hero_owners_04`, hired when Blue's list was full).
In the decompressed `hero_owners_00` and `01` it starts at 543453, from `02` on at 543492; its HotA length of 215 is the one for which the second table is all `FF` and is followed by `00 08` (heroes 205 and 210 have bytes in it; 210 is `40`). Every table of `TestData` is written back byte for byte as it was read (`SafeHeroOwnersSerializerTests`).
The table and the lists are two records, and the editor never assumes they agree: a hero the table gives to a player but no list of it has is still taken from the player (the table becomes `FF`, the lists are left), and a savefile with no table has the hero and the lists changed alone (`HeroOwnerEditsTests`).
RoE, AB and Chronicles saves were not seen: no table is read for them and an owner edit there changes the hero and the lists only.

What the game changes (`hero_owners_00` to `05`, hotseat saves of `Island of HOTA tests 6p`):

- **A defeat** (`00` to `01`: Clancy of Blue and Ufretin of Red; `02` to `03`: four heroes of Red; `04` to `05` and `05` to `06`: seven heroes of Blue at once, Fiona (205) among them, past the 200th byte of the table): the hero's owner byte becomes `FF` and it leaves the map; in the player's record the count drops, the id goes and the later ids move up a place, the freed places become `FF`; if it was the selected hero, the first hero left is selected (Blue: 22 selected, `[0, 22]` became 0 selected, `[0]`); the table's byte becomes `FF`. The tavern bytes do not change. A defeated hero goes back to the pool: Sir Mullich (144), Red's until `03`, was hired by Blue in `05`.
- **A hire** (`02` to `03`, Red hires Vokial, 65, from the first place of its tavern; `04` and `05`, Blue hires seven): the id goes to the first free place after the others (after the compaction of the heroes lost in the same turn), the count goes up, the selected hero does not change; the table's `40` becomes the player's colour; the tavern place is refilled by a hero the game draws (Leena, 160), which gets `40` in the table. Blue's eighth hire in `04` (29) went into the garrison: the table says Blue, the list (full at 8) does not have it.
- **A player knocked out** (Pink in `04`): its tavern bytes become `FF FF` and its two tavern heroes' `40` becomes `FF`.
- Unexplained: in `05` Red's first tavern place changed (160 to 2, with the table following) while Red hired nobody.

What the editor does (`HeroEditSession.ChangeOwner`, `HeroOwnerEdits`), in one step of the history:

- **To nobody** (a defeat): the hero (`HeroModel.WithOwner`: off the map), the old owner's list (`PlayerModel.WithoutHero`) and the table (`FF`), as above. Replayed on `00` to `01` (bytes 0 to 9 of every player and the whole table equal the game's), `02` to `03`, `04` to `05` and `05` to `06` (the whole table and Blue's record but its selected hero, which the game set to Orrin in `06` though no selected hero was lost; the editor leaves a selection alone unless the selected hero goes) (`HeroOwnerEditsTests`).
- **To a player** (a hire of a hero that is in no tavern): the id at the end of the new owner's list, the table says the colour; from another player, the old owner loses it as in a defeat.
- **Refused** (`GameRuleException`, nothing written): a hero a tavern offers (the game refills the tavern place with a hero it draws, which the editor cannot do); a hero the table keeps out of play (`40` with no tavern, the campaign heroes and hero 3); a player whose list has 8 heroes; a hero garrisoned in a castle (the castle holds its id and how a defeat or a change of owner of a garrisoned hero is kept was not seen: hero 29 was defeated between `04` and `05`, with the castle, which was not looked at).
- Reverting the owner change (or the player's "Heroes" change, which is listed with it) puts the hero's owner, the place of the hero in every list, the selected hero and the table back together.

**Open:** whether the game accepts a hero given to a player that it did not hire (a hire from the pool goes through a tavern in every save seen, and nothing shows how the game would treat a hero that skips it; not checked in the game); how a garrisoned hero's owner change and defeat are stored (the castle's garrison byte); what the second table is; the `41` of hero 3; the refill of Red's tavern in `05`.
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
  The first numbers are the growth of the creatures (`Creature.Info.Growth`), which is what a dwelling holds when it is built (`CastleBuilder`), and the castle page shows them as "+N a week" (`CastleModel.BaseWeeklyGrowth`). What the castle, the horde buildings and the Grail add was not worked out from a save, so it is not shown.
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
- **Primary skills** hold what counts: the hero's own skill with the bonus of the equipped artifacts already in it (the game changes the bytes when an artifact is put on or taken off, as in HoMM2). In the Arts saves Edric has 2/2/1/1 bare and 11/11/9/9 with Titan's Thunder (+9/+9/+8/+8), and Christian 5/5/4/7 with Armageddon's Blade (+3/+3/+3/+6); across the 356 test saves no hero holds less than its bonus (`HeroPrimarySkillsTests`). The editor's table of what each artifact adds is checked against the game only for those two artifacts. The skills a hero learned are the saved ones minus the bonus.
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
  `HeroModel.MoraleBonus`, `LuckBonus` and `VisitEffects` are edited on the General page ("Visited buildings", the shared next-battle card) and written by `SafeHeroSerializer` (the two signed bytes, and only the known bit of each building; the other bits of those bytes are left as they are). The watering hole (+1 morale), the swan pond (+2 luck) and the crypt (-1 morale) set their bit and the byte together, as HoMM2 must (`docs/HOMM2_SAVE_FORMAT.md`, T03BONUS); the stables have only a bit. The faerie ring, the fountain of fortune (a luck the game draws) and the idol of fortune are shown but not edited, as the saves do not pin down what they give. **Not tried in the game.**
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

## Mines (Horn of the Abyss, found 2026-10-22)

From the saves of `TestData/Hota/Mines` (Red takes a mine a day, Blue takes it back the next turn).

- A table of **records of 62 bytes**, one for each object of the map that has an owner and a guard (the mines, abandoned ones too), one after the other: x, y, level (a byte each), the owner (FF none, 0 Red, 1 Blue, ...), the kind (0 sawmill, 1 alchemist's lab, 2 ore, 3 sulfur, 4 crystal, 5 gems, 6 gold: the numbers of Heroes II), a byte that is 1 for a mine abandoned at the start, then **the guard: seven creatures (32 bits each, FFFFFFFF none) and seven counts (32 bits each)**. The table has records of other objects too (one with the kind 100 at 66, 61 and at 53, 31), so the loader keeps the records whose kind is one of the seven mines.
- An abandoned mine keeps its record when it is taken: the owner is set, the kind is the resource it turned into and the guard is empty.
- Taking a mine changes only its owner byte (and the guard if it was fought for). The income is not stored.
- **The x, y of a record is not where the mine is entered.** In the first row of the test map a hero that stands on the entrance is 4 tiles to the right and 3 up (5 for the sawmill), in the second row 0 to the right and 3 down. The map-object list does not settle it either: the records are in the order of the mine objects of the list, but the y of the objects of one row is the other row's. The tiles of the map hold, for every object, its type, its place in the list and its number among the objects of the type (the cells of the Hota test map have a stride of 26 bytes in a row); that is where the entrance is likely to come from. Open.

## Places a hero visited (Horn of the Abyss, found 2026-10-22)

From the saves of `TestData/Hota/VisitedPlaces`: Tazar and Orrin visit, a turn apart, two places of each kind.

- **A hero remembers the places of the permanent bonuses itself**, in its record, one 32 bit number for each kind of place. **Bit n is set when the hero visited the place whose number among the places of its kind is n** (hero +16, the number of the object among those of its type, is that number while the hero stands on it; +11 is the type). The two heroes have their own bits: Orrin gets the same bits a turn after Tazar. This is what Heroes II does too.
- The bytes (lowest byte of the number) and the object types: **+79 learning stone (100)**, **+83 Marletto tower (23)**, **+87 garden of revelation (32)**, **+91 mercenary camp (51)**, **+95 star axis (61)**, **+99 tree of knowledge (102)**, **+103 library of enlightenment (41)**, **+107 arena (4)**, **+111 school of magic (47)**, **+115 school of war (107)**; +119 was not tried. In the Horn of the Abyss tail of the record: **+1098 colossus of the magi** and **+1114 gazebo** (both are objects of the type 144), and +1074, +1075 and +1081 changed for the dragon utopia (not yet understood).
- Where each place is: the cells of the map name the type, the place in the object list and the number among the type (as for the obelisks, `ObeliskBytesLoader`), so the number of a place gives its position.
- **Artifact merchant**: the visit changes nothing in the hero. The stock of seven artifacts is a list of seven 32 bit artifact numbers (in the order of the shop: 0x3C, 0x6B, 0x2E, 0x70, 0x1B, 0x37, 0x6D in save 234) in the map data near the start of the file (0x9C2 there; the header before it varies), followed by a second list of seven (0x49, 0x4B, 0x55, 0x62, 0x3D, 0x15, 0x96), which is the stock of the **black market**: save 235 names the seven in that order (charm of mana, mystic orb of mana, hourglass of the evil hour, boots of speed, bowstring of the unicorn's mane, helm of chaos, crown of the five seas). Both lists were the same in the first save of the map, so a visit does not change them. **A purchase empties the slot**: in save 236 Tazar bought the first four of the black market and Orrin the last four of the artifact merchant, and exactly those eight entries are FFFFFFFF (the other six are as they were). The seven are the stock of the one merchant of each kind on this map; whether a map with several merchants has a list for each is not known (this one has one of each). Read and edited by the Merchants tab of the Map page (`MerchantBytesLoader`): the loader looks in the first 0x6000 bytes for fourteen 32 bit numbers that are all known artifacts or FFFFFFFF, behind a 32 bit 2, with at least four artifacts in them. Whether a map with several merchants of a kind has a list for each is not known.
- **A dragon utopia (once per map)** is marked visited for the heroes of the player that won it and not for the others: Orrin (Blue) saw no change in the save after Tazar's win. Where that mark is kept is not found (the saves after it are 99 and 8,749 bytes longer).
- Tree of knowledge: other bytes change too (+231 and +1141 for Tazar, +224 and +1134 for Orrin) and the level (+75) goes up by one: the payment each tree asks (gold, gems...) is not stored in the hero.

### Objects of the map: what the list of positions and the cells say (2026-10-24)

- **In the list of objects (a definition number and x, y, z for each) the position of an entry belongs to the object whose definition is in the next entry.** A hero standing on school of magic 0 in `visited_places_05` is at the position of entry 791 and the definition of entry 792 is the school; the cells of the map that give a place number point at the entry with the position. `MapObjectPositions` therefore gives the position of the object a cell names, and the type of that object is the type of the *next* definition.
- **Definitions have a type and a subtype**: of the 33 bytes after the file name of a definition, the type of the object is at +26 and its subtype at +28. The Horn of the Abyss objects that are not in the older games share the type 144 and tell themselves apart by the subtype: `AVXgazeb` (gazebo) is 6, `AVXmspr0` (mineral spring) is 4, `AVSmgarn` 2, `avwtplce` 3, `AVShrmit` 5, `avmoilpm` 8, `avxtrmtw` 9, `avtprosp` 10, `avstrblz` 11, `AVStmplt` 0. Which of them is the colossus of the magi is not settled: the cells of the AVStmplt objects are the only ones with the type 144 in them and a number of FFFFFFFF, and a hero standing on a colossus reads 144 and the number 0 or 1, but no position of a hero ever agreed with a definition of the type 144.
- **The cell of a tree of knowledge** (`66 00 00 00`, place, number, `20 05 00`) holds more than the number: the byte of the number is 0 or 1 before a visit and 0x60 or 0x61 after it, the three bytes after it are the payment (`20 05 00` is 2,000 gold at first, `2C 05 00` after a visit), and a hero standing on a tree reads 0x20 or 0x21 for its number. The number of the place is the low five bits (`VisitedPlaceKind.NumberMask`). Both trees of the test map are found this way: 0 at 2, 20 and 1 at 2, 23, and the bits Tazar sets are those (`visited_places_23` and `24`).
- **A cell is written for every tile of an object**, and the cells of one object do not always agree on the number (a merchant of `multi_merchants` has 2 on its first tile and 1 on another): the first cell of a place with a valid number is the one to take.

### The merchants (found in `TestData/Hota/multi_merchants`, 2026-10-24)

The 32 bit number in front of the first list of seven artifacts is **the number of merchants**, not a marker: 2 in the first map of the test series, 4 in `multi_merchants`, where two black markets were added with the map editor, and that many lists follow, one for each merchant of the map. The game calls every one of them a black market. Their number is the number among the objects of the type 7 (`07 00 00 00`, place, number), in the order of the object list: (28, 34), (28, 37), (31, 34) and (31, 37) in that map. The first two are the ones Tazar visits as number 0 and 1 in `visited_places_27` to `29`. Which merchant a list belongs to was not confirmed in the game for more than two.

## Dwellings (Horn of the Abyss, found 2026-10-22)

From the saves of `TestData/Hota/Dwellings` (Tazar flags the Castle's creature generators, Orrin flags them to Blue a turn later).

- The dwellings are a table of **records of 150 bytes** in the map data, after the table of the mines (62 byte records) and its other objects; the first starts right after the last mine (the offsets below are counted from there, 3 bytes nearer than the first notes had them). In a record: **the owner at +5** (FF nobody, 0 Red, 1 Blue, ...; it went from FF to 0 when Tazar flagged the guardhouse and to 1 when Orrin flagged it), **the creature the dwelling gives at +8** (0 pikeman, 2 archer, 4 griffin, 6 swordsman: the guardhouse, the archers' tower, the griffin tower and the barracks) and **the creatures that can be hired now as the 32 bit number at +12** (14, 9, 7 and 4 in the first week: the growth of those creatures; 28 pikemen after two weeks; 0 after a hero hired them all, as Orrin did in save 316). **Where it is: x, y and the level at +20, +21 and +22**, which is the tile the hero stood on to flag the Castle and Rampart dwellings (it is **not** always: in the neutral row above, saves 325 to 337, Tazar stood 3 tiles lower than the record while he visited each, and the records do not change when a neutral dwelling is visited, so the entrance of an object is not always the tile of its record, as for the mines). Tazar stood on (31, 56), (27, 56), (25, 56) and (22, 56) when he flagged the guardhouse, the archers' tower, the griffin tower and the barracks, and those are the four records (`1F 38 00`, `1B 38 00`, `19 38 00`, `16 38 00`). The byte at +6 is the object type, 17 (a creature generator), which is also what a hero standing on one reads at +11. The rest of the record holds more creature slots (FF-filled, as the guard of a mine) and a second copy of the type and the tile, and was not decoded. **The stock grows by the weekly growth at the start of each week** (the pikemen of the guardhouse: 28, 42, 56, 70 in the saves 314, 321, 331, 341; every other dwelling the same way), and a hire takes it to 0. The Rampart dwellings follow the Castle's in the table (creatures 14, 16, 18, 20, 22, 24, 26), and the table has 31 records on this map.
- The table has one record for each dwelling of the map, in an order that is not that of the levels.
- The mines' table also ends with other objects (two at 66, 61 and 53, 31 of the kind 100) before the dwellings.
- Not read yet: a Dwellings list on the Map page, with the owner, the creature and the growth, is the natural next step.

## The Places tab of a hero (HoMM3)

The hero page has a Places tab, like HoMM2's, with the card of `VisitedPlacesCard` of the shared blocks (`VisitedPlacesViewModel` builds it): each kind of place with how many the hero visited, None and All, and each place of the map by its position to forget one by one. The places of a kind are found from the cells of the map (`MapPlacesLoader`) and from the heroes that stand on them (a hero hides the cell of the place it stands on). The colossi and the gazebos are listed without their places: they share the object type 144 with the other objects of the Horn of the Abyss and their cells carry no number (see "Objects of the map"). The trees of knowledge are listed with theirs since 2026-10-24. A kind with no places can still be opened, and it shows nothing.

## Hex views

- **Hero with a biography**: the 26-byte hero header (position, gender at 20, the 32-bit biography length at 22) and the biography text sit *in front of* the hero chunk, because the chunk starts 26 bytes before the faction byte, inside the end of the biography.
  The first 26 bytes of such a chunk are biography text, so `SaveRegionProvider` drops the chunk fields below offset 26 and uses `HeroRegionProvider.GetHeaderRegions`.
- **Names are not unique**: `sod_game_00_baseline` has heroes or castles that share a name, so anything that matches regions to chunks goes by position (`StartIndex`), not by name.

## What is verified and what is assumed

- Players, date and difficulty: verified on Horn of the Abyss and Shadow of Death saves.
  The castle layout and building edits are verified on both too (`TestData/Roe`).
  Restoration of Erathia and Armageddon's Blade are assumed to match Shadow of Death; no saves of those were seen.
- Everything was read from a few maps per version, so a shape that is unique there may not be on another map.