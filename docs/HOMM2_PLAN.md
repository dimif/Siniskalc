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

## Phase 2: a thin host (done)

`MainViewModel` (667 lines), the navigation tree and `MainWindow` knew heroes, castles and players, and a second game needs a shell that does not. As built:

- **`Siniskalc.Core`**: `IGameEditor` (what the shell needs from the editor of one game: load, reload, save, the history with undo, redo, the pending changes and their revert, the bytes to save, the hex dump, `File`, `Name`) and `IGameModule` (`Name`, the file patterns for open and save, `Recognizes(path, head bytes)`, `CreateEditor()`).
- **`Siniskalc.Wpf`**: `ShellViewModel` (the open game's editor, the preferences, the pending changes panel, the open and save filters, `LoadGame(path)` that asks each module to recognise the file and loads it in that game's editor, keeping what is open when the load fails), `SavegameFileCommands` (generic now), `ShellWindow` (menus, toolbar, status bar, pending changes panel, the menu icons) and `HelpLinks`. The editor is shown by a `ContentControl` through the data template of its type.
- **`Siniskalc.Homm3`**: `MainViewModel` became `Homm3EditorViewModel : IGameEditor` (without the preferences and the pending changes panel, which are the shell's); the navigation tree and the page frame became `Homm3EditorView`; `Homm3Module` recognises the gzip magic; the data template of the editor is in `Views/Generic.xaml`.
- **The exe**: `AppServices` lists the modules and makes the shell (`CreateShellViewModel`, `CreateHomm3Editor`), `AppInfo` holds the name and the web addresses, `App` shows a `ShellWindow`; `App.xaml` merges the shared styles and the game's templates.
- **Tests**: the 1,841 HoMM3 tests pass unchanged apart from the renames (`MainViewModel` to `Homm3EditorViewModel`, `CreateMainWindow` to `CreateShellWindow`, the shell's `IsAdvancedMode` and `IsPendingChangesOpen`); `ShellViewModelTests` (17) test the shell with fake games in `Siniskalc.Wpf.Tests`.

Not done on purpose, left for when HoMM2 needs them: a game-neutral navigation tree (the HoMM3 one stays in its editor view); the module's `IsValidSavegameName` rule for the 8-character names (Phase 5, with the save dialog); registering a module's data templates in code instead of `App.xaml`.

## Phase 3: `Siniskalc.Homm2` data layer (done)

New projects `Siniskalc.Homm2` and `Siniskalc.Homm2.Tests`; the exe references the new library. As built:

1. **Locating.** `Homm2SaveLayoutLocator` detects the base game's `.GM1` and the Price of Loyalty's `.GX1` (marker, shift 5, hero stride 236 or 250) and gives a `Homm2SaveLayout` with the date and the starts of the tables of players, heroes and castles. The tables are checked (every hero and every castle holds its own number), so a savegame that does not match is refused (`UnsupportedSavefileException`) and never misread. The same class recognises a file from its first 16 bytes (the shell uses that to pick the game).
2. **Layouts.** `Homm2HeaderLayout`, `Homm2PlayerLayout`, `Homm2HeroLayout`, `Homm2CastleLayout`; the sizes and limits are in `Constants`.
3. **Chunks and collections.** `Homm2ChunkKinds`, `Homm2HeroChunk`, `Homm2PlayerChunk`, `Homm2CastleChunk` (54 heroes, 6 players, 72 castle records, contiguous), their collections, `Homm2BytesLoader`, `Homm2SaveDocumentFactory`, edit sessions and pending changes.
4. **Reading and writing separate** (`Homm2HeroDeserializer` and `SafeHomm2HeroSerializer`; the same for players and castles), sharing `Homm2Bytes`. A byte the editor does not know reads as the first known value (or `null`) and is left alone until the model changes it. Names, the header, the facing and the 14 extra bytes of the Price of Loyalty are never written.
5. **Models**: `Homm2HeroModel`, `Homm2PlayerModel`, `Homm2CastleModel` with value types that compare by content (`Homm2Army`, `Homm2Artifacts`, `Homm2SecondarySkills`, `Homm2SpellBook`, `Homm2Numbers`, `Homm2Resources`), `Homm2GameDate`, `Homm2MapHeader`.
6. **Reducers**: `WithSpellBook` and `WithoutSpellBook`. Not done yet: keeping the experience and the level in step (the game's experience table is not verified: Kalindra is level 14 with 28,037 experience), the maximum movement (derived by the game) and the rule that a town without a castle cannot build.
7. **Entities** (`Homm2Creature` with 66 creatures, `Homm2Spell` with 65, `Homm2Artifact` with 99, `Homm2SecondarySkill` with 14, `Homm2Race`, `Homm2Color`, `Homm2Resource`, the building flags): tables made from the open source engine fheroes2 (read only, GPL-2.0, no code copied) and checked against the saves: the creature codes of the Peasant, the Archer and the Black Dragon, the spells of the Mage Guild, the artifacts that were picked up, the weekly growth of a castle. Names are plain strings for now (`.resx` is left for when the UI is localised).
8. **Tests** (86): every save is located, loaded and written back byte for byte; the values of every README row are checked; edits change only their bytes; unknown values survive; the 14 extra bytes survive; pending changes revert one by one; the names of the saves follow the rules; the shell opens both games.

Also done ahead of Phase 5, because the title of the window had to follow the game: `Homm2Module`, `Homm2EditorViewModel` (an `IGameEditor`) and a first view, `Homm2EditorView`, that shows the map, the date and the players. `AppServices` lists both games, and the window title becomes "Siniskalc - Heroes of Might and Magic II" when a savegame of that game is open.

## Phase 4: the write test with the game (in progress)

`Homm2EditedSaves` (in the tests) builds 13 savegames from the saves of Broken Alliance, one kind of edit each, with names of at most 8 characters (`E00COPY` is an unchanged control, then gold, resources, army, primary skills, secondary skills, artifacts, spell book with every spell, level and movement, castle buildings and creatures, garrison, a castle given to a player, and everything together). `Homm2EditedSavesTests` checks what they hold; the explicit test `WriteTheSavesForTheGame` writes them to `TestData/Homm2/out` (ignored by git) with a README that says what to look at in the game:

```
dotnet test Tests/Siniskalc.Homm2.Tests --filter Name=WriteTheSavesForTheGame
```

Results from the game (2026-10-13): E01-E05, E07, E08, E10 and E11 load and show what was written, so gold, resources, army, primary and secondary skills, the spell book, level, experience, movement, the garrison and a castle's owner are safe to edit. E00 (a byte for byte copy) loads, so nothing in the file is checked; the "Maximus not in the castle" was a wrong expectation (he is outside in save 05). E09: the buildings were as written (the README expectation was unclear about dwellings 1 and 2); the user listed no tavern, to be confirmed. E12 has one town because the owner change is only in E11. **E06 is a real finding:** the artifacts show, but Maximus keeps 2/2/1/1, so the game does not add the bonus of an artifact when it loads a save; either it adds it to the stored skills when the hero picks the artifact up, or the hero screen must be refreshed. **Answered by E13ARTSW** (Maximus gave the Ultimate Sword of Dominion to Sir Gallant in the game): Maximus's attack fell from 15 to 3 and Gallant's rose from 2 to 14, so the savegame keeps the primary skills with the artifact bonuses in them and the game moves them with the artifact. `HeroModel.WithArtifact`, `WithoutArtifact` and `WithArtifact(slot, artifact)` do the same (bonus table `Homm2ArtifactBonuses`; only the +12 of the sword is checked in the game, the others are the values of fheroes2). E06 and E12 are made with them now (the saves in `out/` still have the old values: regenerate them if they are to be tried again).

The user copies them to the save folder of HoMM2, loads each and ticks the README. What each result means:
- the control fails: the game rejects a file that is byte for byte a copy of a save (a name, a date or a checksum is checked): stop and find out what;
- the control loads but an edit fails to load: something in that kind of record is checked by the game (the README says which);
- an edit loads but shows something else: the model or the layout of that field is wrong;
- all load and show what was written: the format is safe to edit, and Phase 6 can go ahead.

## Phase 5: the viewer and its pages (done)

- Done: `Homm2Module` is registered in the host, so detection, the recent files and the window title work for both games; the hex viewer works (`Homm2SaveRegionProvider`).
- Done: the UX follows the HoMM3 editor. `Homm2EditorView` has the search box and the navigation tree (`Homm2NavigationTree`: game, a node per player with its "Heroes" and "Towns" groups, "Neutral" for what nobody owns; nodes show the race and level, the hero that is in a town, a dot for unsaved edits), and a frame with the page of what is picked. Pages (each a `Homm2PageViewModel` with tabs, an "edited" mark and a Reset pill): the overview (map, scenario, players that open their page), the player (Resources with the daily income, Heroes with the tavern, Towns), the hero (General, Skills, Creatures, Spells, Artifacts) and the castle (General, Buildings with dwellings, Creatures, Mage Guild). Heroes and towns link to each other.
- Done: the hex tab of the player, the hero and the castle page (`Homm2RecordRegions` holds the known fields of each record, `IHomm2ByteEditTargetFactory` makes the target; the tab shows in advanced mode as in HoMM3), and the rule for the names of the saves: `IGameModule.ValidateSavegameName` (a default of "any name"), which `Homm2Module` answers with the 8 characters before the extension, and which "Save as" asks before it writes. The backup keeps the extension of the game plus `.bak`, which the game does not list, so it needs no rule.
- Left: the game-neutral navigation tree (the HoMM3 and HoMM2 trees are two copies; the HoMM3 one was left unchanged).

## Phase 6: editing (done for heroes, players and castles; unverified in the game)

Everything below is a reducer on the immutable model, undoable, with the pages built on shared view models of `Siniskalc.Wpf` (`AmountRowViewModel`, `ArmyViewModelBase`, `SecondarySkillsViewModelBase`, `SpellsViewModelBase`, `ArtifactPickerViewModel`, `ToggleRowViewModel`, `ChoiceFieldViewModel`, with the HoMM3 pages built on the same ones):

- **Player**: the seven resources (gold up to 999,999, the others up to 999 until tested in the game), quick add, Reset.
- **Hero**: primary skills, level, experience, spell points, movement (and "Full movement"); the army (5 slots, finder); the secondary skills (8 slots in the order learned, the list of 14); the spell book (the Magic Book artifact) and the 65 spells; the 14 artifact slots with a finder. An artifact moves its skill bonus with it (`HeroModel.WithArtifact`), as the game does (E13ARTSW).
- **Castle**: the owner (the castle and the lists of castles of both players change in one step; a player left without a castle starts the countdown of 7 days, as in the savegames), the buildings (castle, tavern, thieves guild, shipyard, well, statue), the six dwellings with the creatures that wait in them (up to 999), the garrison, and the Mage Guild at level 0 to 2 with its spells.
- Not done: level and experience kept in step (the experience table is unverified), the dwelling upgrades, the Mage Guild levels 3 to 5, the marketplace and the other bits of the high byte of the buildings, the rule that a town needs its castle, the maximum movement (the game works it out), the hex tab of each page, the map (mines, objects).

