# Knowledge base

Findings about the savefile formats that are not obvious from the code, one section per topic. The sections up to "What is verified and what is assumed" are about Heroes of Might and Magic III; the part headed "Heroes of Might and Magic II" at the end is about HoMM2.
How each thing is found is in `docs/LOCATING_ENTITIES.md`; the saves that show it are in the READMEs of `TestData`.
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
  The name starts at byte 95 in HotA saves and byte 58 in SoD and RoE saves (the 16-bit length is at that byte, the text two bytes later).
  Some names keep a leading space (`" The empire of the world"`).
  Non-ASCII names would break `MapHeaderLocator`.
- **Map difficulty** is the byte after the description, 0 Pawn to 4 King.
  It is separate from the game difficulty of the setup block.
  The byte after it was 0 everywhere and is not read.
- **Map size** is a 32-bit integer five bytes before the name (36, 72, 144 seen).
  The byte before the name is read as the underground flag: 1 in nearly every sample, 0 only in `bios_custom`.
  This is the **least certain** item; it was not checked on a map with an underground.
- **Human flags**: eight 32-bit values in colour order (1 = human), followed by the colour whose turn it is, then a marker: `FF 10` eight bytes later in HotA, `FF 00 00 00` right after in SoD and RoE (+36 from the flags; offsets 1130, 1228, 1262 and 1286 seen, so locate by shape).
  The header's one-byte-per-colour list (`FF` not in game, `0A` computer, other = human) only names the first human of a hotseat game.
  Found by comparing `players_human_red`, `players_human_tan` and `players_human_red_and_tan`; only red and tan were seen human, so the stride for the other colours is assumed.
  The current player is a 32-bit colour index (0 Red to 7 Pink) at the flags +32, and it is always a human.
  Across 21 pairs of consecutive saves only this value changed in the setup window when the turn passed; among them `Hota/Mines` 00-01 and 18-19, `Hota/Banishing` 00-01, 02-03, 03-04, 12-13, 13-14 and 15-16, `Hota/Dwellings` 04-05 and 05-06, `Hota/VisitedPlaces` 00-01, `Sod/Hotseat` five_teams_01-02, hotseat_11-12 and hotseat_12-13, and `Hota/players_human_red_and_tan` to `players_human_tan`. The new-day block does not depend on it.
  The editor writes it (`SafeTurnSerializer`, the Order of play card). Not known: whether the game starts the new player's turn cleanly from an edited save, and whether anything else goes with the turn (`hotseat_12_blue_turn` also changes the selected hero of both players).
- **Teams** (SoD and RoE; not found in HotA saves; not yet read by the editor): in the header, just before two lists `00 01 02 03 04 05 06 07`, a byte with the number of teams, and when it is not 0, eight bytes with the team of each colour (Red, Blue, Tan, Green, Orange, Purple, Teal, Pink).
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
  When the editor gives a castle to another owner (an edit of the castle, followed by `PlayerListsReconciler`) it applies what the saves show: the player who loses his last town gets 7 at once (`banishing_00` to `01`, `04` to `05`), a player who gains a town while counting gets `FF` at once (`01` to `02`), and losing one town of several or gaining one while not counting leaves `FF`; a banished player is not changed (no save shows it). The towns a player owns are counted from the owners of the castles. HoMM2's castle owner change applies the same rule (`PlayerModelBase.DaysLeftOnGainingATown` / `DaysLeftOnLosingATown`).
- 23: number of castles. 24: the selected castle (index in the table of castles, `FF` for none), exclusive with a selected hero.
  The number follows ownership: it went up or down by one with each castle gained or lost in every pair of saves looked at (`banishing_00` to `01` and `01` to `02`, `castles_00` to `01` in `TestData/Hota`, `roe_63` to `64`).
  `PlayerListsReconciler` and `SafePlayerListsSerializer` now write the count together with the list when a castle owner changes. Town selection is deliberately left alone: save 09 of `TownOwnership` has Blue's selected town 0 in the second list position, so it is a global town id, not a list position. The user reports that reloading clears selection; turn changes also clear it in these saves.
- 25 to 96: 72 indices of the player's castles in the table of castles, `FF` in unused places (`PlayerLists.TownIds`). New acquisitions append; losing a town removes it and compacts the remaining list without sorting.
  The twelve hotseat saves of `TestData/Hota/TownOwnership` establish this: global ids 0 Red_Tower, 1 Red_Inferno, 2 Blue_Necropoli, 3 Neutral_Bulwar. Red starts with `[0,1]`, Blue with `[2]`; Blue captures Inferno then Tower, giving `[2,1,0]`. Red retakes Inferno, leaving Blue `[2,0]`, then takes the neutral town, giving Red `[1,3]`. Blue retakes Inferno (`[2,0,1]`), then Red retakes Tower (Red `[3,0]`, Blue `[2,1]`). Counts agree throughout and unused slots are `FF`.
  The editor follows that order for new acquisitions and preserves selection and unrelated bytes. Returning an original town restores its original-relative position, like returning an original hero, so resetting or reverting ownership restores the loaded save byte for byte; it does not simulate a new in-game capture in that case.
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

### The table of hero owners (found in `TestData/Hota/HeroOwners`)

A byte per hero id, in the order of the ids (`HeroOwnersLayout`, `HeroOwnersModel`): the colour of the player who has the hero (0 Red to 7 Pink), `FF` for nobody, and `40` for a hero the game keeps from being drawn for a tavern (the two heroes of every tavern, and the campaign heroes 145 to 155). Hero 3 has `41` in some saves, although it is neither owned nor offered in a tavern; other saves have `FF` there, so this is not universal. Its meaning is unknown. The editor reads it as reserved/unowned and preserves the original byte while the model still says that.
It is 156 bytes in SoD (the `Roe` saves are SoD saves) and 215 in HotA; a second table of as many bytes, all `FF` in every save, follows, then `00 08`, two 16-bit numbers that vary, a zero byte and the 7F marker of the date. So `HeroOwnersBytesLoader` finds it from the date (`date - 11 - 2 * length`) and checks it (every byte `FF` or a colour with or without `40`, the second table all `FF`, the tail `00 08`); it is never searched for. It was found and checked in all 482 HoMM3 saves of `TestData` (`HeroOwnersBytesLoaderTests`): the table says the owner byte of every hero chunk, every hero of a player's list has the player's colour, every tavern hero has `40`, and an owned hero in no list is garrisoned in a castle (Blue's hero 29 in `hero_owners_04`, hired when Blue's list was full).
In the decompressed `hero_owners_00` and `01` it starts at 543453, from `02` on at 543492; its HotA length of 215 is the one for which the second table is all `FF` and is followed by `00 08` (heroes 205 and 210 have bytes in it; 210 is `40`). Every table of `TestData` is written back byte for byte as it was read (`SafeHeroOwnersSerializerTests`).
The table and the lists are two records, and the editor never assumes they agree: a hero the table gives to a player but no list of it has is still taken from the player (the table becomes `FF`, the lists are left), and a savefile with no table has the hero and the lists changed alone (`HeroOwnerEditsTests`).
RoE, AB and Chronicles saves were not seen: no table is read for them and an owner edit there changes the hero and the lists only.

What the game changes (`hero_owners_00` to `05`, hotseat saves of `Island of HOTA tests 6p`):

- **A defeat** (`00` to `01`: Clancy of Blue and Ufretin of Red; `02` to `03`: four heroes of Red; `04` to `05` and `05` to `06`: seven heroes of Blue at once, Fiona (205) among them, past the 200th byte of the table): the hero's owner byte becomes `FF` and it leaves the map; in the player's record the count drops, the id goes and the later ids move up a place, the freed places become `FF`; if it was the selected hero, the first hero left is selected (Blue: 22 selected, `[0, 22]` became 0 selected, `[0]`); the table's byte becomes `FF`. The tavern bytes do not change. A defeated hero goes back to the pool: Sir Mullich (144), Red's until `03`, was hired by Blue in `05`.
- **A hire** (`02` to `03`, Red hires Vokial, 65, from the first place of its tavern; `04` and `05`, Blue hires seven): the id goes to the first free place after the others (after the compaction of the heroes lost in the same turn), the count goes up, the selected hero does not change; the table's `40` becomes the player's colour; the tavern place is refilled by a hero the game draws (Leena, 160), which gets `40` in the table. Blue's eighth hire in `04` (29) went into the garrison: the table says Blue, the list (full at 8) does not have it.
- **A player knocked out** (Pink in `04`): its tavern bytes become `FF FF` and its two tavern heroes' `40` becomes `FF`.
- Unexplained: in `05` Red's first tavern place changed (160 to 2, with the table following) while Red hired nobody.

What the editor does after an edit of the owner of a hero (`HeroModel.WithOwner`, then `PlayerListsReconciler`), in one step of the history:

- **To nobody** (a defeat): the hero (`HeroModel.WithOwner`: off the map), the old owner's list (`PlayerLists.WithHero`) and the table (`FF`), as above. Replayed on `00` to `01` (bytes 0 to 9 of every player and the whole table equal the game's), `02` to `03`, `04` to `05` and `05` to `06` (the whole table and Blue's record but its selected hero, which the game set to Orrin in `06` though no selected hero was lost; the editor leaves a selection alone unless the selected hero goes) (`HeroOwnerEditsTests`).
- **To a player** (a hire of a hero that is in no tavern): the id at the end of the new owner's list, the table says the colour; from another player, the old owner loses it as in a defeat.
- **Refused** (`GameRuleException`, nothing written): a hero a tavern offers (the game refills the tavern place with a hero it draws, which the editor cannot do); a hero the table keeps out of play (`40` with no tavern, the campaign heroes and hero 3); a player whose list has 8 heroes; a hero garrisoned in a castle (the castle holds its id and how a defeat or a change of owner of a garrisoned hero is kept was not seen: hero 29 was defeated between `04` and `05`, with the castle, which was not looked at).
- Reverting the owner change, resetting the hero page, or giving the hero back to its owner by hand puts the hero's owner, the place of the hero in every list, the selected hero and the table back together (a hero that comes back takes its old place; the player has no "Heroes" change of its own). Putting a castle back gives the players who own the towns the savegame gave them their days left back.

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
  `HeroModel.MoraleBonus`, `LuckBonus` and `VisitEffects` are edited on the General page ("Visited buildings", the shared next-battle card) and written by `SafeHeroSerializer` (the two signed bytes, and only the known bit of each building; the other bits of those bytes are left as they are). The watering hole (+1 morale), the swan pond (+2 luck) and the crypt (-1 morale) set their bit and the byte together, as HoMM2 must (`docs/KNOWLEDGE_BASE.md`, T03BONUS); the stables have only a bit. The faerie ring, the fountain of fortune (a luck the game draws) and the idol of fortune are shown but not edited, as the saves do not pin down what they give. **Not tried in the game.**
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

The date (`day`, `week`, `month`, 16 bit each) follows the marker `7F xx 02 yy`: `xx` is 1 until the Grail is dug up and then 0. `yy` is a strong candidate for a cheat-used flag: in the supplied HotA `Two Mines Three Dwellings/dwe_3.GM1` to `aft_cheats.GM1` control, the user entered only the resource cheat, with no other action. Outside the embedded filename and seven resource values, the only changed byte is `yy` at decompressed offset 492494, from 0 to 1. Earlier it was observed becoming 1 during a castle visit with a garrisoned hero, but that comparison did not isolate the cause. Persistence, other cheat types and scoring effects have not been checked.

Both resource-cheat control saves decompress to 521457 bytes, with 23 changed bytes in total. The hero-adjacent byte at 300792 remains 0, ruling it out as a flag set by this resource cheat.

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

## Mines (Horn of the Abyss and Shadow of Death)

From `TestData/Hota/Mines` (Red takes a mine a day, Blue takes it back the next turn), the six small-map variants in `TestData/Hota/MineFinding`, and the existing SoD corpus. The files under `TestData/Roe` also identify as SoD, so they do not establish original RoE support.

- The 22 small-map saves cover zero/one/two mines with zero/one/two/three dwellings and separate captures. Movement cheats were used in some sequences to complete captures in one turn; their exact activation points are not all known. Owner/guard comparisons concern the local object records, not single-change whole-save pairs. Table order is not acquisition order: in `TwoMinesTwoDwellings`, the sulfur mine is captured first but the gold mine is stored first. Source-to-fixture names and capture order are in the fixture READMEs.
- Comparing old and counted detection across all 482 HotA/SoD/Roe-folder saves retained every formerly detected mine owner. Existing positive maps gained their omitted first mine; the small-map controls yielded exactly zero, one or two mines. Regression tests also cover unchanged round trips and isolated owner/guard edits.
- In the supplied `Two Mines Three Dwellings/begin.GM1`, count 2 is at decompressed offset 240477; owner-first mine records start at 240481 and 240543, followed by the generator count at 240605. The companion map is `Two_Mines_three_Dwe.h3m`; sprite anchors must not be equated with save-record coordinates without a parsed map-object table.
- A count precedes **62-byte owner/guard records**: little-endian uint32 in HotA, one byte in SoD (assumed, not established, for RoE, AB and Chronicles). Every counted record and the following generator table are validated before any mine is exposed. Contained suffix candidates are discarded; competing tables are rejected. A single record is sufficient, and (0,0) is not excluded.
- The record starts at **owner +0** (FF none, 0 Red, 1 Blue, ...), **kind +1** (0 sawmill, 1 alchemist's lab, 2 ore, 3 sulfur, 4 crystal, 5 gems, 6 gold), **abandoned +2**, **seven uint32 guard creature codes at +3** (FFFFFFFF none), **seven uint32 counts at +31**, then **x, y, level at +59, +60, +61**. The former layout started three bytes too early: the table count masqueraded as the first tile, and each preceding object's trailing tile was assigned to the next object.
- The old test map's count is 21: **19 mines and two kind-100 records** at (53,31) and (51,33), not (66,61) and (53,31). Besides the fourteen ordinary and two abandoned mines in the capture sequence, neutral mines are at (14,62), (2,69) and (66,61). The corrected alignment retains the formerly dropped first mine.
- An abandoned mine keeps its record when it is taken: the owner is set, the kind is the resource it turned into and the guard is empty.
- Taking a mine changes only its owner byte (and the guard if it was fought for). The income is not stored.
- **Record tile versus sprite/entrance remains map-dependent.** The corrected old test-map capture rows match the entrances in its README; the previously reported row swaps were caused by misalignment. In `TwoMinesThreeDwellings`, the sulfur mine's save tile is (3,8), while the user reports its sprite at (4,8); the gold mine is (7,8). Do not apply a blanket entrance adjustment.

## Places a hero visited (Horn of the Abyss)

From the saves of `TestData/Hota/VisitedPlaces`: Tazar and Orrin visit, a turn apart, two places of each kind.

- **A hero remembers the places of the permanent bonuses itself**, in its record, one 32 bit number for each kind of place. **Bit n is set when the hero visited the place whose number among the places of its kind is n** (hero +16, the number of the object among those of its type, is that number while the hero stands on it; +11 is the type). The two heroes have their own bits: Orrin gets the same bits a turn after Tazar. This is what Heroes II does too.
- The bytes (lowest byte of the number) and the object types: **+79 learning stone (100)**, **+83 Marletto tower (23)**, **+87 garden of revelation (32)**, **+91 mercenary camp (51)**, **+95 star axis (61)**, **+99 tree of knowledge (102)**, **+103 library of enlightenment (41)**, **+107 arena (4)**, **+111 school of magic (47)**, **+115 school of war (107)**; +119 was not tried. In the Horn of the Abyss tail of the record: **+1098 colossus of the magi** and **+1114 gazebo** (both are objects of the type 144), and +1074, +1075 and +1081 changed for the dragon utopia (not yet understood).
- Where each place is: the cells of the map name the type, the place in the object list and the number among the type (as for the obelisks, `ObeliskBytesLoader`), so the number of a place gives its position.
- **Adventure-map merchants are Black Markets**, including the object called an artifact merchant in the older fixture names. Visiting changes nothing in the hero or stock; purchasing writes `FFFFFFFF` to the chosen uint32 slot. `visited_places_27` to `29` contain two independent seven-slot lists, not one town list and one Black Market list. The counted format and the distinct shared town stock are described below.
- **A dragon utopia (once per map)** is marked visited for the heroes of the player that won it and not for the others: Orrin (Blue) saw no change in the save after Tazar's win. Where that mark is kept is not found (the saves after it are 99 and 8,749 bytes longer).
- Tree of knowledge: other bytes change too (+231 and +1141 for Tazar, +224 and +1134 for Orrin) and the level (+75) goes up by one: the payment each tree asks (gold, gems...) is not stored in the hero.

### Objects of the map: what the list of positions and the cells say

- **In the list of objects (a definition number and x, y, z for each) the position of an entry belongs to the object whose definition is in the next entry.** A hero standing on school of magic 0 in `visited_places_05` is at the position of entry 791 and the definition of entry 792 is the school; the cells of the map that give a place number point at the entry with the position. `MapObjectPositions` therefore gives the position of the object a cell names, and the type of that object is the type of the *next* definition.
- **Definitions have a type and a subtype**: of the 33 bytes after the file name of a definition, the type of the object is at +26 and its subtype at +28. The Horn of the Abyss objects that are not in the older games share the type 144 and tell themselves apart by the subtype: `AVXgazeb` (gazebo) is 6, `AVXmspr0` (mineral spring) is 4, `AVSmgarn` 2, `avwtplce` 3, `AVShrmit` 5, `avmoilpm` 8, `avxtrmtw` 9, `avtprosp` 10, `avstrblz` 11, `AVStmplt` 0. Which of them is the colossus of the magi is not settled: the cells of the AVStmplt objects are the only ones with the type 144 in them and a number of FFFFFFFF, and a hero standing on a colossus reads 144 and the number 0 or 1, but no position of a hero ever agreed with a definition of the type 144.
- **The cell of a tree of knowledge** (`66 00 00 00`, place, number, `20 05 00`) holds more than the number: the byte of the number is 0 or 1 before a visit and 0x60 or 0x61 after it, the three bytes after it are the payment (`20 05 00` is 2,000 gold at first, `2C 05 00` after a visit), and a hero standing on a tree reads 0x20 or 0x21 for its number. The number of the place is the low five bits (`VisitedPlaceKind.NumberMask`). Both trees of the test map are found this way: 0 at 2, 20 and 1 at 2, 23, and the bits Tazar sets are those (`visited_places_23` and `24`).
- **A cell is written for every tile of an object**, and the cells of one object do not always agree on the number (a merchant of `multi_merchants` has 2 on its first tile and 1 on another): the first cell of a place with a valid number is the one to take.

### Black Markets and shared town Artifact Merchants

The 32 bit number in front of the first list of seven artifacts is **the number of Black Markets**, not a marker: 2 in `VisitedPlaces`, 4 in `multi_merchants`, and 8 in `MerchantStock`. That many lists follow, one for each Black Market. Their number is the number among the objects of type 7 (`07 00 00 00`, place, number): (28, 34), (28, 37), (31, 34) and (31, 37) in `multi_merchants`. The first two are the ones Tazar visits as number 0 and 1 in `visited_places_27` to `29`. `MerchantBytesLoader` searches the first `0x6000` bytes for a count from 1 to 32 and that many seven-uint32 lists of known wearable artifacts or `FFFFFFFF`, with at least four stocked entries overall. It does not discover nearly sold-out tables or accept unknown artifact codes during discovery.

The eleven HotA `Art Merch Dr Utopia` controls are preserved in `TestData/Hota/MerchantStock`, with their source names and actions in its README. `merchant_stock_00_baseline.GM2` holds count 8 at decompressed `0x87A`, followed by eight seven-artifact lists at `0x87E`. Each purchase clears exactly one entry within that table to `FFFFFFFF`: save 01 list 1 slot 6 (Equestrian's Gloves), 02 list 2 slot 7 (Angel Feather Arrows), 03 list 3 slot 5 (Bowstring of the Unicorn's Mane), 04 list 4 slot 7 (Orb of Tempestous Fire), 05 list 5 slot 3 (Pendant of Total Recall), 06 list 6 slot 2 (Bow of Elven Cherrywood), 09 list 7 slot 5 (Boots of Speed, code 98), and 10 list 8 slot 6 (Armor of Wonder, code 31). Red makes the first six purchases, Blue the last two. All 56 initial artifacts, purchase deltas and unchanged lists are covered by `MerchantStockTests`.

The companion `merchant_stock_baseline.h3m` has eight objects referencing definition 79 (`AVXmktb0.def`, type 7), with anchors **(2,8,0), (5,8,0), (8,8,0), (11,8,0), (14,8,0), (17,8,0), (20,8,0), (23,8,0)**. Baseline saved type-7 cells give numbers 0..7 and object indexes 152, 153, 154, 155, 156, 158, 159, 160, resolving to exactly those anchors. The existing `MapPlacesLoader` and Map page already associate them correctly; `MapViewModelTests` checks locations and names across all eleven controls. Every initial list contains three treasure, three minor and one major artifact. This observed distribution does not establish which replacement classes the game accepts, so the picker is not tightened on that evidence alone.

Town Artifact Merchants use a different stock list, not one of those eight Black Market lists. The user reports the same seven items in three Red towns and Blue's town, with Head of Legion disappearing in all of them after one purchase. The corresponding sequence occurs once in each decompressed control: at `0x797AD` in `GAME_BEGIN`, and `0x797A5` in `shop06`, `bought_town` and `[hotseat] 111`. It is Stoic Watchman, Ladybird of Luck, Breastplate of Petrified Wood, Helm of Chaos, Ring of Life, Red Dragon Flame Tongue, Head of Legion; the final entry becomes `FFFFFFFF` in `bought_town` and stays empty in Blue's save, while all eight Black Market lists remain unchanged. This establishes shared town stock across the observed towns and players for this game, not its general location or refresh rules. The current early-file `MerchantBytesLoader` finds the Black Market table, not this town stock.

**Structural town-stock locator:** the seven uint32 slots start **38 bytes after the saved date** (uint16 day/week/month, then 32 zero bytes). The eleven controls give stock offsets `0x797AD` (00), `0x797A5` (01..08), `0x7979D` (09..10); the recognized date prefix is `7F flag 02 flag`, with each flag 0 or 1. This date/stock relationship matches 488 saves across the current corpus; the files under `Roe` are actually SoD-format saves, not evidence for the original RoE format. All seven entries at those locations are known wearable artifacts or `FFFFFFFF`, but this is corroboration, not a production discovery requirement.

`TownMerchantStockLocator` uses the injected date locator after the player table, requires that explicit prefix and room for all 28 stock bytes, and rejects competing marked dates. Unlike the display date reader, it **never uses an unmarked-date fallback**. It does not inspect artifact values, so sold-out stock and unknown codes still locate. `SavegameParser` adds one `TownMerchant` record, separate from the Black Markets, using the existing merchant reader/writer. Every town with its Artifact Merchant building built exposes that same stock; history, per-slot reverts and the explicit shared reset target it once, while a town reset only resets the town. Genuine RoE, AB and Chronicles are not enabled. Five HotA `empire_of_the_world` / `new_day` saves have another date prefix and are deliberately rejected; their town tabs show an unavailable notice. That alternate structure needs independent validation before editing. Editor-written town stock still needs an in-game check.

The merchant baseline contains seven towns: four Red (including the misleadingly named `Neu_Tow`), two Neutral (`Neu_Dung`, `Neu_Fac`) and one Blue. The two neutral records contain `73` immediately after the Mage Guild level; four neutral SoD towns in `sod_game_00_baseline` contain `57` there. This byte's meaning is unknown and it is preserved, not required to be zero. Castle discovery starts after the located map header to avoid mistaking its title for a town, and retains the owner/faction/garrison/spell-count checks.

## Dwellings (Horn of the Abyss)

From the saves of `TestData/Hota/Dwellings` (Tazar flags the Castle's creature generators, Orrin flags them to Blue a turn later).

- A **uint16 count followed by generator records with owner bytes 75 apart** follows the complete owner/guard table. Offsets: **owner +0**, **object type +1** (17 for a fixed-creature dwelling; generator types 18..20 are accepted for location), **creature code byte +3**, **uint32 stock +7**, **x, y, level +15..17**. Creature IDs include Pikeman 0, Archer 2, Griffin 4, Swordsman 6, Centaur 14, Devil 54, Automaton 176 and Jotunn 198. The bytes after the creature-code byte are not treated as a uint32 ID by the reader. The former 150-byte stride combined two objects and skipped every second dwelling; the old test map has **62**, not 31, records. Earlier owner/creature/stock/position offsets +5/+8/+12/+20 were relative to a start five bytes too early.
- **Stride is not editable extent.** The known payload is **74 bytes**, ending with the guard's last uint32 count (+70..73). The byte at +74 is not included in the editable chunk or required after the last payload. In `arts_00_before_arts.GM1`, the generator count is at 294445, five owner bytes start at 294447 + n*75, and the final payload ends at 294821: the first byte of the obelisk marker. Including 75 bytes in the last dwelling caused an overlap and hid the Arts dwellings. Their creature codes/tiles are 94 (53,33), 36 (45,40), 16 (12,70), 14 (22,58), 36 (47,42), all surface. The same boundary relationship holds across the Arts series.
- **Weekly stock** adds the creature's growth at the start of the week (Guardhouse: 28, 42, 56, 70 Pikemen in saves 314, 321, 331, 341); hiring them all sets it to zero. The controlled Automaton hiring pair in `Two Mines Three Dwellings` independently changes stock 5 to 0 at absolute 240764, while owner and guard bytes remain unchanged.
- That hiring pair is `before_hiring.GM1` to `aft_hiring.GM1`, with resource/movement cheats applied before the first save. Both decompress to 521457 bytes; only the stock byte changes within inspected region 240450..240849. There are 25 changed bytes globally, including the filename and changes consistent with payment and adding five Automatons to the hero; those other fields were not independently decoded.
- In the three-dwelling control, owner-first starts are 240607, 240682 and 240757, following count 3 at 240605. Record tiles are Devil (11,8), Jotunn (14,8), Automaton (16,8). Devil and Jotunn captures clear their three-creature guards; the unguarded Automaton capture changes only its owner within its record. Guard arrays are seven uint32 types at +18 and counts at +46, validated by the locator; editing dwelling guards remains unimplemented.
- For the Castle/Rampart capture rows, record tiles match where the hero stood: (31,56), (27,56), (25,56), (22,56). For the neutral row above (saves 325..337), the hero stood three tiles lower. Record coordinates are not automatically entrances.
- The table has one record for each dwelling of the map, in an order that is not that of the levels.
- The owner/guard table includes kind-100 objects at (53,31) and (51,33) in the old test map. A locator must use the complete counted table, not assume the last mine is its final record.
- The Map page reads and edits owner/stock from the shared, independently validated table boundaries (`MapObjectTablesLocator`). There is no minimum run or dependency on a detected mine: trailing non-mine owner/guard records are counted, and an explicit empty owner/guard table can precede a single dwelling. In the no-mine control, count 1 is at 240146 and the Devil dwelling starts at 240148, tile (10,10). Competing tables, invalid guards and overlapping blocks are refused; all counted records must validate rather than exposing a valid-looking prefix.
- Only type-17 single-creature dwellings are exposed. Types 18..20 are validated and skipped without hiding later type-17 records; their complete creature/stock semantics have not been established for editing. No-mine and small-count capture controls are supplied for HotA; the existing SoD corpus is checked for round trips, but controlled SoD capture variants and genuine RoE/AB/Chronicles format confirmation remain outstanding.

## The Places tab of a hero (HoMM3)

The hero page has a Places tab, like HoMM2's, with the card of `VisitedPlacesCard` of the shared blocks (`VisitedPlacesViewModel` builds it): each kind of place with how many the hero visited, None and All, and each place of the map by its position to forget one by one. The places of a kind are found from the cells of the map (`MapPlacesLoader`) and from the heroes that stand on them (a hero hides the cell of the place it stands on). The colossi and the gazebos are listed without their places: they share the object type 144 with the other objects of the Horn of the Abyss and their cells carry no number (see "Objects of the map"). The trees of knowledge are listed with theirs. A kind with no places can still be opened, and it shows nothing.

## Hex views

- **Hero with a biography**: the 26-byte hero header (position, gender at 20, the 32-bit biography length at 22) and the biography text sit *in front of* the hero chunk, because the chunk starts 26 bytes before the faction byte, inside the end of the biography.
  The first 26 bytes of such a chunk are biography text, so `SaveRegionProvider` drops the chunk fields below offset 26 and uses `HeroRegionProvider.GetHeaderRegions`.
- **Names are not unique**: `sod_game_00_baseline` has heroes or castles that share a name, so anything that matches regions to chunks goes by position (`StartIndex`), not by name.

## Unnamed bytes of the hero chunk (hypotheses)

Found by comparing the hero chunks of every pair of consecutive saves of the HoMM3 test series (Roe, Sod, Hota; about 340 saves) for bytes that change and are not named.
Offsets are from the start of the hero chunk (as `HeroOffsets`); counts are the (hero, save pair) changes. **None of this is verified**: each row names the save that would settle it (`FUTURE_IMPROVEMENTS.md`, "Probes").

| Offset | Changes | What was seen | First guess |
|---|---|---|---|
| 7, 8, 9 | 43, 97, 44 | change with the day and with moves | a small counter or path/animation state; low value |
| 10 | 4 | `00`/`04` between a baseline and new-day saves | a flag |
| 30 | 7 | counts down `08`, `06`, `01`, `00` (Tazar, grail saves); `00` to `03` (Crag Hack with horde artifacts) | a count of something used up (charges, boat movement?) |
| 43, 44 | 156, 3 | `43` changes in almost every pair; `44` `00` to `06` after a battle | movement trail / facing counter |
| 51, 52 | 15 | `FF FF` to `00 00` in pairs when a hero is in a town, and back | a 16-bit index of a town (0xFFFF none); cheap to confirm with two heroes in different towns |
| 53, 55 | 2, 5 | `55` grows with fights | a battle counter? |
| 77, 78, 79 | 4, 2, 2 | set at a new day for several heroes | a per-day value (random seed?) |
| 95 | 2 | `00`/`04` | a flag |
| 223..235 | few | `01`, `02`, `03`, `00` for Tazar after battles (`225` counts like `55`) | specialty or secondary-skill counters |
| 263 | 2 | `00` to `05` for one hero | unknown |
| 554, 555 | 2 | `FF FF` to `00 00` when an artifact enters the backpack | the count of the backpack (check whether it is already named) |
| 601..623 | 1 each | `00` to `FF` in bulk at a hero's first save | backpack slots written at set-up; not meaningful |
| 1088 | 8 | `0x40` set on fleeing or defeat | a bit set (see above, not understood) |
| 1102..1144 (HotA) | 30 | Cove saves change with the hero's bits | a HotA-only tail: specialty or bonus counters of the new mechanics (boats, new objects) |

Still unnamed from elsewhere in this file: `+135 0x10` (ring and idol together), `+138 0x01` (first castle visit), hero `+34` (always 0), the third byte of the visit effects (`+137`), the `yy` of the date marker `7F xx 02 yy`, the table of two bytes per object type at date + 12, the dug tiles' number (154, 536), the 4282-byte block that appears on a new day, the team table, banishing's counter and records, and the tavern's memory of a defeated hero.

What the HoMM2 saves suggest for HoMM3 (not fully tested): the temporary-bonus family should hold bits for Temple, Oasis, Rally Flag, Mermaids, Fountain of Youth, Buoy and Shrine of Magic; the permanent-bonus places may be remembered in the hero chunk, one bit per place in map order; the owner of a mine should be one byte in its record; the weekly objects (windmill, water wheel, magic spring) change a flag in the map and the player byte `+129`; the byte `+11` of the hero (the type of the object under it) can list every object of a kind once the type numbers are known.

## What is verified and what is assumed

- Players, date and difficulty: verified on Horn of the Abyss and Shadow of Death saves.
  The castle layout and building edits are verified on both too (`TestData/Roe`).
  Restoration of Erathia and Armageddon's Blade are assumed to match Shadow of Death; no saves of those were seen.
- Everything was read from a few maps per version, so a shape that is unique there may not be on another map.
# Heroes of Might and Magic II

Reverse-engineered from the saves in `TestData/Homm2` (their READMEs list what was done in each).
The offsets of every field, with what it means, are documented on the constants of `Siniskalc.Homm2/Data/*Layout.cs` (`HeaderLayout`, `PlayerLayout`, `CastleLayout`, `HeroLayout`, `MapObjectLayout`, `ObeliskLayout`, `SaveLayout`); this part records what the layouts cannot say: how it was found, what was tried in the game, and what is not understood.
All integers are little-endian. Saves are not compressed. "Confirmed" means a diff between saves or a try in the game; "guess" is anything else. Maps are named after the folders (Broken Alliance = `BrokenAlliance`, Arrax the Jerk = `ArraxTheJerk`, Threedom = `Threedom`); "save N" is the number in the file name.

## HoMM2 game versions and layout

The base game writes `.GM1`; the Price of Loyalty writes `.GX1`. Both were checked on every table. The expansion differs by:

- the file starts with a 4-byte marker `FF FF FF FF` before the width and height, so the expansion is detected by it;
- everything after the header text is shifted by +5 bytes (4 from the marker, 1 from an extra header field; the shift is empirical, and whether other maps shift more is not known);
- a hero record is 250 bytes instead of 236: the first 0xD9 bytes are identical and the last 14 are extra (zeros, then `+0xE3..+0xE6` that turn `FF` one after the other as a hero carries more artifacts; what they are is not known);
- the castle table starts 0x36 bytes after the hero table in both, with 72 slots of 100 bytes; players are 6 records of 0xCF in both (empty seats are zeros or `FF`).

Map size changes nothing before the map arrays. The file is a fixed layout for a given map, so every save of a map has the same size, and only values change.

## HoMM2 header and players

- **Map difficulty and game difficulty are two fields.** The map's (0..3) is at 0x0C; the game's (0..4, easy to impossible) is the byte in front of the map file name at 0x1C4 (five saves of one map, `MapDifficultySize` 01 to 05). A percentage after it (70, 100, 120, 140, 160) also depends on the map.
- **Who plays each colour has several places and the game reads only some of them** (`TestData/Homm2/HumanCpu` and probe saves `C01` to `C13`):
  - 0x1BE (6 bytes, 0x0A for the computer, otherwise the human's number among the humans) and 0x1DA (how many humans) feed only the scenario summary: changing only them was ignored and the game played as before.
  - The mask the game plays from is 6 bytes ending 6 bytes before the date (0x2B5): a byte per record of the table of players, 1 for a human, always the first N records. Changing it to one human made the game play Green alone.
  - The mask is not enough: a mask of three humans in a file named `.GM2` played only two, and the same bytes in a `.GM3` file played all three. The digit of the extension (the number the game asks for when loading a hot seat game) caps the humans; the name stored in the save (0x29E) does not matter. To change who plays, the mask, the order of the records (the humans first, in the order they play; hot seat names at 0x1F4 follow it) and the extension all have to agree. Editing the seats is deliberately not built.
- **Whose turn it is** is the number among the humans (the 0x1BE byte of that colour), not the colour: with Green and Orange as the only humans it goes 0, 1, 0, 1. It is 14 bytes before the date (5 bytes further in a Price of Loyalty save). Editing it is built and not tried in the game.
- Header bytes 0x2AE and 0x2B0 (0 to 1 when a player was put out) look like a counter of eliminated players (guess); 0x1F1..0x1F3 change with play and are unexplained. A writer can ignore them.
- The date (day of the week 1..7, week 1..4, month 1..) is global, three bytes with a zero between each, in front of the table of players. After day 7 of week 4 the month increments.
- **Heroes and castles lists of a player** are table indexes: the player's hero list (5 places) holds hero ids and the castle list (8 places, 4 the most seen in use) castle indexes, `FF` for empty. The editor keeps them in step with the owner bytes of the heroes and castles (`PlayerListsReconciler`).
- **Daily income** is a table of 6 numbers (wood, mercury, ore, sulfur, crystal, gems) then the gold, recalculated at the start of each day, so it lags a capture by one day. It counts the mines: 2 a day for each ore mine and sawmill, 1 for the others, 1,000 gold for the gold mine, +250 for a statue (save 71: 3 ore mines gave 6, 2 sawmills 4, 3 crystal mines 3, 2 gem mines 2, 2 alchemist labs 2, 2 sulfur mines 2 (the third was captured that day), gold 4,500). A put-out player has no mines but the table holds a single 1 that walks over the resources one a day: a leftover, not an income.
- **Elimination.** A player who owns no castle shows his countdown (7 the day the last castle is lost, then down to 0); `FF` while he owns one, and regaining one sets `FF` at once. At 0 the player has no castle, no heroes, an empty hero list, and the heroes he had get owner `FF` while keeping their position and a shrunk army: no other flag is needed. Another player can hire the hero again.
- **Defeated hero (save 16):** owner `FF`, position cleared, artifacts emptied except the Magic Book (the winner got the best one), and the loser's hero list loses her. A castle lost to another player changes its owner, both players' counts and lists, and the garrison holds the attacker's creature.
- A text buffer near 0x5CC0 holds the current tavern rumor, built by the game at run time; a writer leaves it alone.

## HoMM2 castles

- **Building bits (16 bits at +0x18).** 0x01 Mage Guild (set with any level), 0x02 Thieves' Guild and 0x04 Tavern (these two were swapped until the write test: setting 0x02 showed a Thieves' Guild), 0x08 Shipyard, 0x10 Well, 0x20 "town only" (no castle; replaced by 0x40 when the castle is built), 0x40 Castle, 0x80 Statue, 0x100 left turret, 0x200 right turret, 0x400 marketplace, 0x800 the second well of the faction (Farm, Garbage Heap, Crystal Garden, Waterfall, Orchard, Skull Pile), 0x1000 moat, 0x2000 the special building of the faction (Fortifications, Coliseum, Rainbow, Dungeon, Library, Storm), 0x8000 captain's quarters. 0x4000 was set by no save. Each was seen in one or two saves (Threedom 39 to 71); none is tried in the game yet (T01BLDS). A town built nothing of the new bits before its castle.
- **Dwellings (+0x1A, 32 bits).** Dwellings 1 to 6 are 0x08 to 0x100. An upgrade **replaces** the bit of its dwelling (dwellings 2 to 6 upgrade with 0x200, 0x400, 0x800, 0x1000, 0x2000, and the sixth a second time with 0x4000); setting both made the game show "Minotaur, 0 waiting" (E17DWEL), so the editor sets one bit. The upgrades stop where the creatures of the level do (the Cyclops not at all).
- **Unknown low dwelling bits.** In `BuiltThisTurn` saves 12 to 13, Xabran's mask changes from `0x08` to `0x0B`, alongside building its Well; Corackston also builds a Well but keeps its mask at `0x08`. Xabran retains `0x0B` in save 14. These are the only castles with low bits `0x3` set across the 166 HoMM2 fixtures checked. On manually loading the before/after saves (`5_2CASNB.GM1` and `6_2CASB.GM1`), the user could see only the Well being added, with no other apparent changes. A controlled replay from save 12, supplied as `WELL1.GM1` (Corackston's Well only) and `WELL2.GM1` (then Xabran's Well), did not reproduce the low bits: both dwelling masks stayed `0x08`. Each step changed only `+0x18` from `0x40` to `0x50` in the relevant castle record; the built-today set became `0x04`, then `0x0C`. Local copies of these replay saves are in `artifacts\homm2-well-check`, not committed fixtures. The bits remain unexplained, not established Well flags or a demonstrated editor defect. The deserializer retains the complete mask, dwelling and Mage Guild reducers preserve unrelated bits, and the serializer writes that mask back.
- **Mage Guild.** The level is a **number** in the bits 16 to 18 (0 to 5), not a flag per level. Each level has 4 spell slots (a zero-based spell id) and a byte of how many are shown: 3, or 4 with a Wizard's Library (so the fourth slot of a level that is not counted is never shown). The spell pool and the counts are in the record.
- **Creatures waiting.** Counts (six u16 at +0x1E) change only when a week starts, never when a building is built. An **upgraded** dwelling keeps its count in its own list at +0x2A (six u16; the plain slot at +0x1E is then 0, E27). The dragons upgraded twice use the sixth entry (+0x34), and the game shows none while the entry before it (+0x32) is 0: the saves the game made have a 3 there. The editor writes the 3 when it has to (E36 to E38, confirmed); what it means is not known. In Threedom the game leaves the old count in the plain list next to an upgraded dwelling (Hillstone, dwelling 2: 14 beside the live 11); the editor reads the live one and does not touch the old one while the level stays.
- **The Well and the second well** (`SecondWell`, Threedom, BrokenAlliance): each week a **built** dwelling gains its creature's weekly growth, an unbuilt one stays at 0; the Well adds +2 to every built dwelling; the second well adds +8 to the first dwelling only. Seen: Xabran (Knight) 12 + 12 + 2 + 8 = 34 peasants and 8 + 8 + 2 = 18 archers; Hillstone (Necromancer) 18 skeletons; confirmed for five factions, the Warlock was in none of the saves.
- **Built this turn.** A castle can build one thing a day. The game remembers it in **a set of 72 bits, one per castle slot, 9 bytes starting 0x48 bytes after the end of the castle table** (`0x5602` in the base-game map, `0x58FB` in Threedom). Bit n is set when castle n built today, stays set through the other players' turns, and the whole set is cleared when the day changes. The castle record holds nothing about it. Confirmed in Threedom and the 15 saves of BuiltThisTurn.
- **Owner of a castle** (E14GIVE, E15LAST): the owner and both players' lists changed and the game agreed, but the flag on the map stayed the old owner's and the castle was missing from the old owner's list until the turn changed. The map holds the flag and what each player has explored, which the editor does not know: giving a castle to a player who had not explored its area left the town reachable only through the list of castles. A hero that captures a town stands in the square in front of the entrance (the tile under the castle's) and the town has no visiting hero (`+0x17` is `FF`); HoMM2 has no garrison hero, so that field is the only link.

## HoMM2 heroes

- **The record starts 10 bytes before the name.** The last 10 bytes of each slot are the header of the next hero (spell points, the id again, the owner colour), and the first hero's header sits before the table. The six bytes just before each name are leftover garbage.
- **The hero id is the table index** (0 to 8 Knight, 9 to 17 Barbarian, and so on). Maps with heroes of their own (`MapDifficultySize` save 07: Famine, War, Pestilence) hold another number in the id byte for four heroes, probably the portrait the map gave them, while the player lists still reference table indexes. The locator therefore accepts a table when most heroes hold their index (and every castle holds its own), and links and taverns use the table index; the original byte is kept untouched. Loading an edited scenario in the game is unverified.
- **Skills hold the bonuses of the artifacts.** The game adds and takes away the bonus when an artifact is given or taken (E13ARTSW: the Ultimate Sword is +12 attack; Threedom: Dragon Sword of Dominion +3 attack, Defender Helm +1 defense, Caster's Bracelet +2 spell power, Breastplate of Anduran +5 defense; the Golden Bow, Endless Sack of Gold, Elemental Ring and Fizbin of Misfortune add no skill). A bonus written to a hero does not show in the game until it is also in the skill, so the model moves the bonus with the artifact. The editor edits the base.
- **Level is worked out from the experience; the game ignores the level byte** (E30 to E33): level byte 1 with 50,000 experience showed level 17, level byte 20 with none showed level 1, and 200,000 showed 25. `HeroLevels` uses the fheroes2 table, which agrees with seven (hero, save) pairs of levels 3 to 11 in Threedom and is slightly off at the top (54,200 and 232,700 against 54,244 and 231,549). The level of E25STATS (file level 20 with 50,000, game level 14) is not explained. The hero page sets the level from the experience and the experience to the start of a level when the level is set.
- **Maximum movement is derived from the army** (1000 with peasants and archers, 1400 with black dragons, 1200 for Crodo's start army); the game recalculates it, and the editor does not store it.
- **Max spell points the game accepts seems to be 32766.** The field is a 16-bit value, but 65535 and 32767 both show as -1 in game, while 12767 loads fine; the editor caps at 32766.
- **Spell points cap** is knowledge x 10 and is not stored; casting spends the cost (Magic Arrow 3) and the hero regains 1 a day.
- **Artifacts** are a plain list of 14 slots, not tied to a body part, `FF` empty. A hero who wins a fight gets the loser's best artifact. Every spellcasting-class hero starts with the Magic Book (81) in slot 0; the spell book is a byte per spell id (1..65).
- **Secondary skills** are two tables: a level per skill (0 none to 3 expert, 14 skills) and the order they were learned in (1-based), plus a count. A level-up is a plain stat change (the level never limits experience).
- **Hire:** the owner byte goes from `FF` to the colour, the position to the castle, the castle visitor to the hero id, gold falls, the player's hero list gets the id and a new tavern hero replaces him.
- **Destination and facing.** The destination (two u32 at +0x17, `FFFFFFFF` when none) is the tile the game marks on the map; the facing (+0x22, 0 north clockwise to 7 northwest) is the way of the last step and 0 inside a castle (`MarkerFacing`: the marker on each of the eight sides of Colar Blau, east in 83 to 90, west 91, northwest 92, northeast 93). The byte at +0x23 is the tile object under the hero (0xA3 castle gateway, guess). Byte +0x25 is not simply a castle index: it can be nonzero while the hero is outside a castle. In Threedom saves 19 (crystal mine), 20 (shrine) and 21 (sawmill), it is 6, 42 and 3 respectively; these values match the tile quantity shifted right by 3 in those examples, but the relationship and meaning are unverified. The editor uses the castle record's `VisitingHeroId` to identify a visitor, not this byte, and leaves +0x25 untouched. On the tile of a hero the map's byte 9 is 0xAA and the hero's +0x23 keeps the real one, which is why a place a hero stands on is not lost.
- **What a hero remembers** (offsets from the name):
  - **+0x3A and +0x3B: morale and luck for the next battle only** (signed bytes): a temple +2 morale, a fountain +1 luck, a faerie ring +1 luck, a graveyard visited a second time -1 morale. They are 0 again after the next battle. +0xD9 and +0xDA (Price of Loyalty only) hold the memory of what gave them: 0x04 fountain, 0x08 oasis (+1 morale and the maximum movement of the day went from 1,559 to 2,359), 0x10 faerie ring, 0x20 a graveyard that had been visited, 0x0100 temple, and for good or a week 0x400000 the arena (a point to a skill, once per hero) and 0x800000 the stables (+400 maximum movement until the start of the week). **`T03BONUS` set only the two bytes: the troops showed the bonus but the hero's screen did not, so the screen works from the bits and the troops from the bytes. The editor sets both together (T06SRC).** The game clears the first group after a battle.
  - **+0x3D, +0x41, +0x45, +0x49, +0x4D, +0x55: the places visited that give a hero something once**, a 32-bit set each for gazebos, forts, witch doctor's huts, mercenary camps, standing stones and Xanadu, **a bit for each place in the order of the map, and the first place is bit 1** (bit 0 is unused; standing stones are counted by their left tile). Clearing a set lets the hero visit again (T02FORGT). +0x51 was never set. The windmill, water mill and graveyard (once a week to the first visitor of any player) are in the map, not the hero.
  - The arena has no mark of its own on the map, so its memory is one bit of the hero, not one per arena; whether the game gives a bonus from each arena was not tried.

## HoMM2 map part

Offsets are from the end of the castle table ("M"). Most of this was seen in the 82 saves of Threedom, and the table of tiles, the obelisk bytes and the mine records were also looked for in the other maps.

- **Capturing a mine, visiting an obelisk, picking up an artifact or capturing a town changes bytes only in the map part (and the owner's resources), never in the player's record.** The player record knows nothing about mines besides the income table.
- **The first part is a list of blocks** each starting with `D2 04 00 00` (1234), a 16-bit length and a payload: signs, events and messages (rumors, a hero's story), and the objects with state. Blocks hold no position: an object's block is found by the order of the objects of its kind on the map, row by row, as the bits of a hero's visited places are.
- **The table of tiles.** After M the width and height are written again as two u32, then `width * height` tiles of 12 bytes, row by row (Threedom: M+0x49B8). Bytes 0 and 1 the ground picture; 2 and 3 the kind and picture of the object (the kind varies with the terrain, so it is not used to find objects); 4 and 5 flags (what it blocks; 0x0188 or 0x0100 when a hero stands on it); 6 and 7 a second object such as the flag of a mine; **9 what the object a hero enters by is**, which is how places are found; 10 and 11 a link to an entry of the table of objects after the tiles. Byte 9: 0x99 obelisk, 0x8A gazebo, 0xBE fort, 0xC5 witch doctor's hut, 0xC9 mercenary camp, 0xC2 standing stones (two tiles), 0xD6 Xanadu, 0x9F / 0xCA / 0xCB shrine of the first / second / third circle, 0xC6 temple, 0x89 fountain, 0xA8 windmill, 0x96 water mill, 0x8C graveyard, 0xA3 the gate of a castle, 0xFA arena and stables, 0xCF sphinx. The counts agree with the pictures (Threedom: 9 obelisks, 5 gazebos, 5 forts, 6 huts, 4 camps, 7 stones, 1 Xanadu) and the byte is better across terrains (the largest map has 23 gazebos by the byte and 21 by the picture).
- **Obelisks.** At M+0x538 are 48 records of 8 bytes, then 48 bytes of `FF`, and at M+0x6E8 one byte per obelisk with a bit for every colour that visited it (bit 0 Blue to 5 Purple; the AI sets theirs as they walk). They are in the order of the map, row by row. HoMM2 has no teams.
- **Mines.** Records of 7 bytes from M+0x05: `owner, kind, guardian spell, guardian count, x, y, FF`; owner a colour or `FF`, kind 0 sawmill to 6 gold. Unguarded records have also been seen with `FF, FF` in the two guardian bytes, rather than `FF, 00`. Capturing sets the owner, adds the flag as the second object of a tile (bytes 6 and 7: the first picture of the kind, 14 for mines, 21 for the alchemist's lab, 28 for the sawmill, plus the owner's colour) and recalculates the income the next day. The flag of a mine nobody owns is the empty object (class 0, picture `FF`). The editor writes both owner and flag, except for the alchemist's lab (the saves have no flag for it, even when Blue captured it).
- **Mine guardians.** Controlled base-game saves `GUARD0.GM1`, `EARTH.GM1`, `AIR.GM1`, `FIRE.GM1` and `WATER.GM1` use Ariel (hero 24) and the gold mine at (43,23). The record at absolute `0x568A` changes from `00 06 FF 00 2B 17 FF` to `00 06 SS 14 2B 17 FF`, where `SS` is the guardian spell code: Earth 62, Air 63, Fire 64, Water 65, not a creature code. All four casts with spell power 5 and knowledge 5 write count 20. `EARTH10.GM1` changes only spell power to 10 (knowledge stays 5), the count to 40 and the embedded filename compared with `EARTH.GM1`: Earth therefore gives 4 guardians per spell-power point in these controls. In `BEFORECAPT.GM1` to `TAKEN.GM1`, the user reports Orange defeating the guardians and capturing this mine: its record changes from `00 06 3E 14 2B 17 FF` to `04 06 FF 00 2B 17 FF`. This establishes owner 0 (Blue) to 4 (Orange), guardian spell cleared to `FF`, count cleared to zero, with kind and coordinates unchanged. Other parts of these two saves also differ, so this is a local record comparison, not a single-action whole-save control. The other spells' scaling, high-count encoding and partial combat losses remain unverified.
- **Guardian discovery and preservation.** The eight controls are retained in `TestData/Homm2/MineGuardians`; its README maps their original names. `MapObjectsReader` accepts no spell (`FF`) with count zero or `FF`, or guardian spell 62..65 with a nonzero count byte. Other record checks remain in place. The former requirement for `FF, FF` hid both guarded mines and the unguarded baseline. `MineGuardianTests` verifies discovery, owner, unchanged serialization and that changing an owner preserves all guardian bytes. The mine list displays the elemental and saved count in a read-only Guard column, or "Unguarded" when absent. Unknown encodings reaching the deserializer retain their raw spell/count for display rather than pretending to be unguarded. Guardians are not editable, and changing owner in the editor is not simulated combat: it does not clear the guardians.
- **Sphinx** (one block, 335 bytes in Threedom, 377 in Arrax): 1 while the riddle is unanswered and 0 after (answered in save 74); the reward as seven u32 resources (8,000 gold; 15 mercury in Arrax); the artifact reward as u16 (96 means none and is also the code of the Legendary Scepter, read as none); the number of answers; 8 answers of 13 bytes ("darkness" and "shadow"); then the riddle text. The riddle and answers are in the savegame and can be edited. The gold arrived with the answer.
- **Shrine:** the spell is in the tile: **the 16-bit number at tile bytes 4 and 5 shifted right by 3** (the low 3 bits are the tile's flags and are kept). A shrine is not used up and leaves no mark; the hero's spell book is the only memory. First-circle spells are all level 1, third mostly level 3. A hero standing on a shrine hides its spell (the tile holds the hero marker), so it is listed but not editable until it moves.
- **Daemon cave, windmill, water mill:** nothing changes in the hero besides the reward; the picture of the object changes in the table of objects after the tiles (an entry goes from `5C 0C` to `5E 00`). Not decoded.
- Walking reveals explored rows as runs of 6 to 9 bytes spaced 72 apart; terrain tiles and kingdom state are not decoded.

## What the write test with the game showed

Saves E01 to E39 were made by editing the models or with the editor, loaded in the game and ticked (the README is in `TestData/Homm2/out`, not in the repository). The findings that are not in the sections above:

- A control that is a byte-for-byte copy loads, so nothing in the file is checked by name, date or checksum. Gold, resources, army, primary and secondary skills, the spell book, level, experience, movement, the garrison and a castle's owner are safe to edit.
- **Stacks** are a signed 16 bits: 65,535 shows as -1 on the map and 0 on the hero screen and splitting misbehaves; 32,767 peasants and 9,999 rogues split fine and one more overflows to -32,768 (E24, E33), so the editor stops at 32,767.
- **Resource limits** (tried in both games, E39GOLDX and `gold_limit`): gold is a signed 32 bits and 2,147,483,647 is the most; the other six took 65,535 each without trouble, the games' real limit is higher but unknown, and the editor stops at 65,535. Both limits are in `Heroes.Constants.Resources` (`MaxGold`, `MaxOther`); raise `MaxOther` only after trying a higher value in the game.
- The creature codes of the Peasant, Archer and Black Dragon, the Mage Guild spells and the artifacts picked up were checked against the saves; the middle of the creature list follows the fheroes2 order and is not verified. Only spells 6, 18, 26, 35, 50 and 51 were verified against the game; the rest of the table is fheroes2's.
- Not decoded in the castle: the use of the byte `+0x55` (it grows by 1 a day in every castle, guess: 1 + days played).
