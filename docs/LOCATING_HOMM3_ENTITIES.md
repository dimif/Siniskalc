# How things are located in a savefile

A Heroes 3 savefile (gzip-decompressed) has no table of contents.
The positions of entities (players, castles, heroes) moves from map to map.
Each thing is therefore found by **what it looks like**: a scan over the bytes that tests fixed-width fields against the values the game keeps in them.

`SavegameParser.Parse` does it in this order:

```
SavegameVersionDetector.Detect(bytes)               game version, from the header (major/minor version bytes)
HeroBytesLoader.LoadHeroChunks(bytes, version)      heroes            -> HeroChunk
CastleBytesLoader.LoadCastleChunks(bytes)           castles (towns)   -> CastleChunk
PlayerControlLocator.FindSetup(bytes, version)      who is human, difficulty, whose turn
PlayerBytesLoader.LoadPlayerChunks(bytes, version,  players           -> PlayerChunk
    heroChunks + castleChunks)                      (the others tell where the last player ends)
PlayersInTheGame.Of(players, setup.PlayerControls)  drops the colours the setup says are not played (and have no heroes or towns)
ObeliskBytesLoader.LoadObeliskChunks(bytes, players) visited obelisks   -> ObeliskChunk (in front of the first player's struct)
GameDateLocator.Find(bytes, version, lastPlayerEnd) date              (searched for after the players; a match with the marker 7F xx 02 yy in front of it wins)
MapHeaderLocator.Find(bytes, version)               map name, description, difficulty
```

A *chunk* (`BytesChunk`) is a copy of the bytes of one struct plus where it started (`StartIndex`).
Everything after that works on the chunk with the offsets of a layout class, never on the savefile directly.
The date, the setup and the map header are not chunks: their locators only report values (and positions for the hex viewer, through `Locate`/`FindOffset`).

## Heroes: `HeroBytesLoader`

The hero struct is fixed width for a game version (RoE, AB, SoD/Chronicles, HotA each have a `HeroShape`: number of equipment slots, the unused fifth side slot of AB, the HotA skill layout, the combination-artifact tail).
At each position the scan checks, in order:

| Field | Check |
|---|---|
| skills used | a byte up to 0x1C followed by three zero bytes |
| name | 13 bytes: first byte above 0x20, last byte 0 |
| skill slots | 28 bytes of 0 to 3 then 28 of 0 to 0x1C (HotA: 30 of 0 to 3, then later 29 or 30 of 0 to 0x1C) |
| spells | 140 bytes of 0 or 1 (spell book, available spells) |
| equipment | N slots of 8 bytes: `FF FF FF FF` + 4, or a byte + `00 00 00` + 4 |
| backpack | 64 slots of two 32-bit values, each `FF FF FF FF` or a byte + `00 00 00` |
| tail (SoD and later) | 6 bytes of 0 or 1, then bytes up to 2, 1 and 5 |

A hit is then accepted only if the name is real text (not all `FF`, nothing but zeros after it) and the equipment section is not just blank slots.
The first hero is searched for from byte 30000 on (before it there might be campaign carry-over heroes with their own copies); the next hero within 5000 bytes of the end of the last.
`ReadBiography` then walks back from the faction byte to the optional length-prefixed biography and the hero header in front of it.
Offsets inside the chunk are in `HeroOffsets`.
Layout comes from [cysun](https://github.com/cysun) and the detection regex from [h3sed](https://github.com/suurjaak/h3sed).

A Shadow of Death hero record is the struct and 12 more bytes (1082 + 12 = 1094, the distance between two heroes); the loader includes them in the chunk (`ShadowOfDeathShape`), because the flag of a sleeping hero is the sixth of them.
Horn of the Abyss has twelve bytes there too, laid out differently, and they are left out of its chunk.

## Castles: `CastleBytesLoader`

Castles are found by the bytes round the name: a 4 byte prefix whose third byte (Mage Guild level) is 5 or less and fourth is 0, then the name length (16 bits, up to 48) and the name.
A candidate is a castle when the owner is a colour or `FF`, the faction and the built-this-turn byte are valid, the name is printable ASCII, all seven garrison slots hold a creature type from -1 to 200 with a non-negative count, and the Mage Guild spell counts are 8 or less.
The chunk starts at the owner byte and ends a fixed distance after the name, and the scan continues after it.
What comes before the name is at a fixed offset from the start of the chunk, what comes after at a fixed offset from the end of the name.
Offsets are in CastleLayout; the layout is the same in Horn of the Abyss and Shadow of Death, and Restoration of Erathia-version map created by SoD client.
Once again, Chronicles game versions have not been tested, the modelling is speculative and based of h3sed.

## Players: `PlayerBytesLoader`

The table of players has one fixed-size struct per colour in colour order (`PlayerLayout.Stride`: 145 bytes in SoD, 149 in HotA).
A struct starts 97 bytes (`PlayerLayout.HeadLength`) before its first resource, with the heroes of the player, its tavern and its towns, and the chunk is the whole struct.
The scan looks for a run of structs a stride apart, each with a marker after the resources (`01 00 00` in SoD, `01 10 00 00 00` in HotA) and a colour byte higher than the previous one's. The longest run is the table.
A struct with the marker but no valid colour byte (an unused last slot) is named after the colour that follows the previous one, so every colour of the table is loaded.
Only versions with a known layout are loaded; RoE and AB assume SoD's. The savefile holds a struct for every colour; only the colours in play are returned as chunks: those with resources, heroes or towns (all of them when none is).
The scan reads bytes only: the setup of the game (below) is applied afterwards by `PlayersInTheGame`, which drops a colour the setup says is not played even when it has gold (the colour Tan of `TestData/Hota/tan_not_parsing` has 10000), unless it has heroes or towns.

## The visited obelisks: `ObeliskBytesLoader`

53 bytes in front of Red's struct (found from the first chunk and its colour, so it should also work when Red is not in play) there is a marker `FF 00 00` and the number of obelisks of the map, then one byte for each obelisk, with a bit for every colour that visited it.
The loader returns a chunk when the marker is there and the count is 1 to 48, and nothing otherwise (some saves have other data in that place).
See `ObeliskLayout` and the knowledge base.

## The setup of the players: `PlayerControlLocator`

Not the same as the table of players.
The savefile header holds eight bytes, one per colour: `FF` not in the game, `0A` computer, anything else human.
They are followed by the difficulty byte (0 Pawn to 4 King) and the map's file name ending in `.h3m` or `.h3c`, so the scan looks for exactly that shape.
That header only names the first human of a hotseat game, so who is human is read from eight 32-bit flags further on (searched for within 4000 bytes, ending with the colour whose turn it is and then a marker: `FF 10` 8 bytes later in Horn of the Abyss, `FF 00 00 00` right after it in Shadow of Death and Restoration of Erathia).
Unknown for Chronicles.

## Date and map header

`GameDateLocator`: three 16-bit numbers (day, week, month) followed by a run of 32 zero bytes, the first such run after the last player.
`MapHeaderLocator`: the name and description at the start of the file, each a 16-bit length and then text, then the difficulty byte; found by the shape of the name, 5 bytes after the 32-bit map width.
Their remarks say which saves they were checked on.

## Adding or changing a locator

- A locator may only read bytes.
  The meaning of the fields lives in a `...Layout` / `HeroOffsets` class and the deserializer, so reading and writing a chunk stay independent (see `CLAUDE.md`).
- Make the first test cheap (one byte compare) because the scan tries every position of a megabyte-sized file.
- Reject on the values the game really uses (a colour is below 8, a creature code below 200) rather than on "not zero".
- When the shape of a version is only assumed, say so in the remarks, as the existing locators do.
- When you change one, compare the old and the new on every file under `TestData` for every game version before deleting the old one (this is how the hero regex was replaced), then run the whole suite.
