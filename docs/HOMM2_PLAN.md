# HoMM2 support: plan

Status: planning. The savefile format is in [HOMM2_SAVE_FORMAT.md](HOMM2_SAVE_FORMAT.md).
Decisions already taken:

- **The HoMM3 code is split out first** into `Siniskalc.Homm3`, and the exe becomes a thin host (this is the first item of `FUTURE_IMPROVEMENTS.md`).
- **The first HoMM2 version is a read-only viewer, then resources and heroes.** Castle editing comes after that.

## Phase 0: housekeeping (done)

1. The saves were renamed to the rules of `TestData/README.md`, keeping the numbers (never renumbered):
   `ScenarioA/N_NAME.GM1` is now `TestData/Homm2/BrokenAlliance/broken_alliance_NN_<what changed>.GM1` (24 saves), and `ScenarioB/1_EXP_MA.GX1` is `TestData/Homm2/ArraxTheJerk/arrax_the_jerk_01_start.GX1`. The `save-details.md` notes became each folder's `README.md`, with one row per save.
2. **HoMM2 only accepts savegame names of at most 8 characters before the extension.** The repo names are longer, which is fine for the tests, so:
   - each README has a column with **the name the game gave the save** (`10_GUILD.GM1`), so a save can be copied back into the game folder;
   - the saves the tool writes for the game to load (Phase 4) get 8-character names, upper case, like the game's own (`E01GOLD.GM1`), in `TestData/Homm2/out` (not committed);
   - the game also stores its own file name inside the save (the buffer at `0x29E`); a writer keeps those bytes as they are;
   - the editor's backup (`name.bak`, `name.bak2`) and "save as" must respect the limit for HoMM2 files: the module supplies the rule (`IsValidSavegameName`) and the file dialog and the backup use it.
3. **The Homm2 saves are not linked into `Siniskalc.Tests`**: `PlayerHeadTests`, `ObeliskVisitsTests` and `CastleSamples` parse every `*.GM?` under `TestData` as a HoMM3 save and would fail. They are copied by `Siniskalc.Homm2.Tests` (Phase 3), which also gets its own naming test (lower-case snake case, the `.GM1`/`.GX1` extensions, unique numbers) copied from `TestDataNamesTests`, so `.GX1` is not added to the HoMM3 test.
   Until then the names were checked by a script with the same regular expressions.
4. Add a `Homm2` section to `docs/KNOWLEDGE_BASE.md` once the format is stable; `HOMM2_SAVE_FORMAT.md` stays the byte reference.
5. Baseline, measured on 12 October 2026 on the tree before the move: 1,911 tests pass (Core 43, Wpf 27, Siniskalc 1,841), none fail; the Siniskalc suite takes about 2 minutes. Every gate below means "the same numbers" (plus the new tests).

## Phase 1: split HoMM3 into `Siniskalc.Homm3` (mechanical, no behaviour change; done)

Goal: the same application, the same tests, the code in a library.

1. New `Siniskalc.Homm3` (net8.0-windows, WPF library, root namespace `Siniskalc.Homm3`), references Core and Wpf.
2. Move `Data`, `Entities`, `Models`, `Runtime`, `ViewModels`, `Views`, `Resources` and `Constants.cs` (HoMM3 limits) into it. Namespaces follow the folders, so `Siniskalc.Data` becomes `Siniskalc.Homm3.Data` and so on, in code, XAML (`clr-namespace:`), `pack://` URIs of resources, tests and docs.
3. What stays in the exe `Siniskalc` for now: `App.xaml(.cs)`, `AppServices` (the composition root) and the icon. As built, `MainWindow`, `MainViewModel`, `SavegameFileCommands`, `Constants.cs` and the menu icons moved into `Siniskalc.Homm3` too (they use dozens of its view models); Phase 2 makes them generic.
4. Tests: `Tests/Siniskalc.Tests` becomes `Siniskalc.Homm3.Tests` (a test goes with the project of the class it tests); `TestServices` and the `TestData` links follow. `ViewModelLayeringTests` and `TestDataNamesTests` must find their folders.
5. Gate: `dotnet test` of the whole solution gives the same results as the baseline (it did: 43 + 27 + 1,841 pass), and the app starts and opens a Hota save.

Done with `git mv` (the moves show as renames, so `git log --follow` keeps the history), a build after each step, and the full suite at the end. Not committed yet: the whole move is staged as one change for the user to commit.
Other tools and people edit this tree at the same time, so it is done on the feature branch and the tree should be quiet while it runs.

## Phase 2: a thin host

Today `MainViewModel` (667 lines), `NavigationTree` and `MainWindow` know heroes, castles and players. A second game needs a shell that does not.

Proposed split (to be confirmed after reading `MainWindow.xaml`, which is a spike of about a day):

- In `Siniskalc.Core` (no UI): `IGameModule` with `Name`, the file extensions, `bool Recognizes(byte[] head)` and `IGameEditor Open(path/bytes)`. `ISavefileCodec` already has the gzip implementation; HoMM2 adds an identity codec.
- In `Siniskalc.Wpf`: a `ShellViewModel` that holds what is not game-specific: open, reload, save, save as, recent files, undo, redo, pending changes with revert, backup on save, advanced mode, about. It talks to the open game through `IGameEditor` (the history, the file, the page to show, the navigation nodes, the hex dump).
- In `Siniskalc.Homm3`: what remains of `MainViewModel` becomes `Homm3Editor : IGameEditor`; the navigation tree stays HoMM3-shaped for now (a generic node when HoMM2 needs one, see FUTURE_IMPROVEMENTS).
- In the exe: the window, the list of modules (`Homm3Module`, `Homm2Module`), detection by content (gzip magic = HoMM3; `0x5C000000` at offset 8, or `FFFFFFFF` first and then the same at 12 = HoMM2), and the `DataTemplate`s of each module merged as resource dictionaries so the content area picks the page by the type of the editor.

Risks: `MainWindow` binds directly to HoMM3 properties (`Heroes`, `HeroFilter`, `SelectedHeroViewModel`), so the window may have to split in two (a shell, and the HoMM3 editor view). If it is bigger than expected, stop after Phase 2a (the interface and the shell, with HoMM3 as the only module) before any HoMM2 code, and re-plan.
Gate: HoMM3 tests unchanged and passing, and a new `Siniskalc.Wpf.Tests` set for the shell with a fake game.

## Phase 3: `Siniskalc.Homm2` data layer (no UI)

New projects `Siniskalc.Homm2` and `Siniskalc.Homm2.Tests`; the exe references the new library.

1. **Locating.** `Homm2SaveHeaderLocator`: detects `.GM1` versus `.GX1` (marker, shift 5), gives the offsets of the player table, the hero table (stride 236 or 250) and the castle table. Offsets are derived, not hard-coded to one map, and validated: the hero table starts with `Lord Kilburn`, the castle table follows 0x36 bytes after the hero table. When validation fails the loader says the file is unsupported (`UnsupportedSavefileException`), it never guesses. TODO:LOG at every fallback.
2. **Layouts.** `Homm2HeroOffsets`, `Homm2CastleLayout`, `Homm2PlayerLayout` (offsets from HOMM2_SAVE_FORMAT.md), with the version-dependent stride.
3. **Chunks and collections.** `Homm2ChunkKinds` (`Hero`, `Castle`, `Player`) using `ChunkKind`; `HeroChunk` etc. with the byte parts, `ChunkCollection` of each; a `Homm2BytesLoader` per kind; the document factory (the single place besides `AppServices` that builds version-dependent objects).
4. **Reading and writing separate**, as everywhere (`Homm2HeroDeserializer`, `SafeHomm2HeroSerializer`; same for players and castles). A byte the editor does not know reads as the first known value and is left alone while the model still holds the stand-in: unknown creature ID, unknown artifact ID, owner `0xFF` as `null`.
5. **Models** (immutable, reducers): `Homm2HeroModel` (name, ID, owner, position, spell points, movement, experience, level, primary skills, secondary skills with learn order, army of 5, artifacts of 14, spells), `Homm2PlayerModel` (resources, daily incomes, castles, heroes, elimination countdown, tavern heroes), `Homm2CastleModel` (owner, race, position, garrison, building mask, dwellings, available creatures, mage guild pool). `Homm2GameDate`.
6. **Rules in reducers**: a hero with the Magic Book gets its spells recognised (book = artifact 81, spells = flags), a spell granted by the guild; the experience and level are kept in step; max movement is derived from the army and warned about, not written; a town without a castle accepts no other building.
7. **Entities** (`Homm2Creature`, `Homm2Spell`, `Homm2Artifact`, `Homm2Skill`, `Homm2HeroClass`, `Homm2Building`): one checked table each, with the source in a comment. Sources: fheroes2 (GPL-2.0, read only; the repo is MIT so no code or table structure is copied), cross-checked against the HoMM wiki and against the IDs that the saves proved. `.resx` for names, as `CLAUDE.md` asks.
8. **Tests**: every save of the two series parses and gives the values of the README rows (resources, hero stats, army, artifacts, spells, castle buildings, elimination countdown); a round trip (deserialize then serialize) is byte-identical on every save; edits change only the intended bytes.

## Phase 4: the write test with the game (early, before the UI)

`Homm2SaveDocument.ToBytes` is already the patcher of Core. A small tool or test writes edited saves into `TestData/Homm2/out` (not committed) with names of at most 8 characters: more gold, a different army, a spell, a building. The user loads them in HoMM2 and confirms.
This is the real check that nothing in the file (a checksum, a length) rejects an edit; do it before building pages.

## Phase 5: read-only viewer

- `Homm2Module` registered in the host; recent files and detection work for both games.
- Pages: overview (date, players), player (resources, incomes, castles, heroes), hero (everything above, read-only), castle (buildings, dwellings, garrison), and the hex viewer with `ByteRegion`s from the layouts (`Homm2SaveRegionProvider`).
- Navigation tree for HoMM2: players, then their heroes and castles.

## Phase 6: editing (heroes and resources)

Resources; primary skills; experience and level (kept in step); secondary skills (up to 8 skills, learn order); army (5 slots, with the creature finder); artifacts (14 slots, finder); the spell book (flag per spell, and the Magic Book). Each is a reducer on the immutable model, undoable, with `PendingChange` text from an injected provider.
Gate: edit, save, reload in the editor, and load in the game.

## Later

- Castle editing: buildings with their rules, dwellings and available creatures, the garrison, owner; tavern heroes.
- Elimination countdown, date.
- Map arrays: mines and objects (see the open questions of the format document).

## Open questions and risks

- **Header shift of the `.GX1`** (4 + 1 bytes) is verified on one map. The header holds the variable parts, and there may be more shifts for other maps. A save from another expansion map and another base-game map would settle it; until then the locator validates and refuses.
- **Unknown bytes** (spell points maximum, hero bytes 0x39-0x58, castle `+0x19` and the dwelling upgrade bits) are left untouched, never zeroed.
- **Artifact and creature names**: IDs beyond those proved by the saves come from fheroes2/wiki and are not yet checked against the game. The tables get a "verified" marker per row, or a test that lists the unverified ones.
- **Price of Loyalty additions** (extra artifacts, the 14 extra hero bytes, new heroes, new creatures): the 14 extra bytes are copied back unchanged. The new entities are added when a save uses them.
- **Scale**: Phase 1 touches about 250 source files and 100 test files for renames, so it needs a quiet tree (no one else editing) for a short time.
- **The CLAUDE.md rule on static helpers** applies to the new code: a locator, loader or provider is an injected instance, built once in the composition root, and the tests use fakes or `TestServices`.
