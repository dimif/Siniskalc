# How things are located in a savefile

A Heroes of Might and Magic savefile (HoMM3 gzip-decompressed) has no table of contents, and the positions of players, castles and heroes move from map to map.
Each thing is therefore found by **what it looks like**: a scan over the bytes that tests fixed-width fields against the values the game really keeps in them.
The checks themselves are in the loader and locator classes (`Siniskalc.Homm3/Data/*BytesLoader.cs`, `*Locator.cs`; `Siniskalc.Homm2/Data/SaveLayoutLocator.cs`); this file is the order, the reasons and the rules.

## HoMM3: the order, and why

`SavegameParser.Parse` runs these in this order, and the order matters:

1. **Version** (`SavegameVersionDetector`) from the header, because every later layout depends on it (RoE, AB, SoD/Chronicles and HotA have different hero shapes and player strides).
2. **Heroes** (`HeroBytesLoader`): a fixed-width struct per game version (`HeroShape`), found by testing its parts in turn (skills used, a real name, skill slots, spell bytes of 0 or 1, equipment, backpack, the tail), then accepted only if the name is real text and the equipment is not all blank. The first hero is searched from byte 30000 on, because before it campaign carry-over heroes can have their own copies; the next one within 5000 bytes of the last.
3. **Castles** (`CastleBytesLoader`): found by the bytes round the name (a short prefix, the 16-bit name length, the name), then validated on the owner, faction, garrison and Mage Guild counts. The chunk starts at the owner byte and ends a fixed distance after the name.
4. **Setup** (`PlayerControlLocator`): who is human, the difficulty and whose turn it is. The header only names the first human of a hotseat game, so the real answer is the eight 32-bit human flags further on, followed by the current colour and a version-specific marker.
5. **Players** (`PlayerBytesLoader`): a table of fixed-size structs, one per colour, found as the longest run of structs a stride apart with a marker after the resources and rising colour bytes. It comes after the heroes and castles because they tell it where the table lies.
6. **Which players are in the game** (`PlayersInGame` in `Siniskalc.Heroes`; HoMM3 still has its own `PlayersInTheGame`): the scan reads bytes only, so a colour that is not played but has gold (the colour Tan of `TestData/Hota/tan_not_parsing`) is dropped here from the setup, unless it has heroes or towns. The parser retains every detected record: hidden records live in `Homm3ChunkKinds.InactivePlayer`, outside navigation, so giving a town to a hidden colour still updates its town count and list.
7. **Obelisks** (`ObeliskBytesLoader`), found relative to the first player's struct. **Date** (`GameDateLocator`), searched for after the last player, where the marker `7F xx 02 yy` in front of it wins. **Map header** (`MapHeaderLocator`), by the shape of the name at the start of the file. Then the loaders of the map: mines, dwellings, merchants, the turn and the table of hero owners.

A *chunk* (`BytesChunk`) is a copy of the bytes of one struct plus where it started (`StartIndex`). Everything after the scan works on a chunk with the offsets of a layout class, never on the savefile directly. The date, setup and header are not chunks: their locators only report values (and positions for the hex viewer).

Things worth knowing that are not obvious from the code:

- A Shadow of Death hero record is the struct plus 12 bytes (1082 + 12 = 1094, the distance between two heroes), and the loader includes them because the flag of a sleeping hero is among them. Horn of the Abyss has twelve bytes there too, laid out differently, and they are left out of its chunk.
- The hero's biography sits in front of the chunk, so `ReadBiography` walks back from the faction byte; the first 26 bytes of such a chunk are biography text.
- RoE and AB assume SoD's player layout, and Chronicles is untested (the modelling is speculative); say so in the remarks of a locator when a version's shape is only assumed.
- Mines and dwellings share the injected `MapObjectTablesLocator`, which finds the complete counted tables, not their editable subsets. Owner/guard count: uint32 in HotA, one byte in SoD (assumed for RoE, AB and Chronicles), followed by 62-byte records; generator count: uint16, followed by records with owner bytes 75 apart. Every owner, position and seven-slot guard must validate. An empty owner/guard table is accepted only with its explicit zero count and a nonempty validated generator table. Contained suffix candidates are discarded, competing tables are rejected, and an overlapping generator table is not exposed.
- Only owner/guard kinds 0..6 become editable mines, including one-record tables and (0,0) tiles. Generator types 17..20 are validated, but only type 17 becomes an editable single-creature dwelling; other types do not hide later type-17 records. Dwelling discovery does not need a mine or a minimum run. Editable dwelling chunks cover only the 74 known bytes through the last guard count; the 75-byte spacing is not permission to include the first byte of the following block (the Arts obelisk marker).
- The shared town Artifact Merchant stock has a separate locator (`TownMerchantStockLocator`), ready for town-page integration but not yet used by `SavegameParser`. It starts 38 bytes after the date (six date bytes + 32 zeros), with seven uint32 slots. Discovery requires one uniquely marked date (`7F flag 02 flag`), never the date reader's shape-only fallback or artifact-value matching. SoD and HotA are enabled; five alternate-prefix HotA saves and unverified RoE/AB/Chronicles formats are refused. Empty or unknown stock entries do not prevent location.
- The layout of the hero comes from [cysun](https://github.com/cysun) and the detection regex from [h3sed](https://github.com/suurjaak/h3sed).

## HoMM2

`SaveLayoutLocator` recognises the base game's `.GM1` and the Price of Loyalty's `.GX1` (marker, header shift of 4 or 5 bytes, hero stride 236 or 250) and gives a `SaveLayout` with the date and the starts of the tables of players, heroes and castles. The tables are checked (every hero and every castle holds its own number), so a savegame that does not match is refused with `UnsupportedSavefileException` and never misread. The same class recognises a file from its first 16 bytes, which the shell uses to pick the game. The offsets are documented on the `Siniskalc.Homm2/Data/*Layout.cs` constants and what the saves showed is in the HoMM2 part of `docs/KNOWLEDGE_BASE.md`.

## Adding or changing a locator

- A locator may only read bytes. The meaning of the fields lives in a `...Layout` / `HeroOffsets` class and the deserializer, so reading and writing a chunk stay independent (see `CLAUDE.md`).
- Make the first test cheap (one byte compare) because the scan tries every position of a megabyte-sized file.
- Reject on the values the game really uses (a colour is below 8, a creature code below 200) rather than on "not zero".
- When the shape of a version is only assumed, say so in the remarks, as the existing locators do.
- When you change one, compare the old and the new on every file under `TestData` for every game version before deleting the old one (this is how the hero regex was replaced), then run the whole suite.
