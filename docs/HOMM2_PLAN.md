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

1. **Locating.** `SaveLayoutLocator` detects the base game's `.GM1` and the Price of Loyalty's `.GX1` (marker, shift 5, hero stride 236 or 250) and gives a `SaveLayout` with the date and the starts of the tables of players, heroes and castles. The tables are checked (every hero and every castle holds its own number), so a savegame that does not match is refused (`UnsupportedSavefileException`) and never misread. The same class recognises a file from its first 16 bytes (the shell uses that to pick the game).
2. **Layouts.** `HeaderLayout`, `PlayerLayout`, `HeroLayout`, `CastleLayout`; the sizes and limits are in `Constants`.
3. **Chunks and collections.** `Homm2ChunkKinds`, `HeroChunk`, `PlayerChunk`, `CastleChunk` (54 heroes, 6 players, 72 castle records, contiguous), their collections, `BytesLoader`, `SaveDocumentFactory`, edit sessions and pending changes.
4. **Reading and writing separate** (`HeroDeserializer` and `SafeHeroSerializer`; the same for players and castles), sharing `Bytes`. A byte the editor does not know reads as the first known value (or `null`) and is left alone until the model changes it. Names, the header, the facing and the 14 extra bytes of the Price of Loyalty are never written.
5. **Models**: `HeroModel`, `PlayerModel`, `CastleModel` with value types that compare by content (`Army`, `Artifacts`, `SecondarySkills`, `SpellBook`, `Numbers`, `Resources`), `GameDate`, `MapHeader`.
6. **Reducers**: `WithSpellBook` and `WithoutSpellBook`. Not done yet: keeping the experience and the level in step (the game's experience table is not verified: Kalindra is level 14 with 28,037 experience), the maximum movement (derived by the game) and the rule that a town without a castle cannot build.
7. **Entities** (`Creature` with 66 creatures, `Spell` with 65, `Artifact` with 99, `SecondarySkill` with 14, `Race`, `Color`, `Resource`, the building flags): tables made from the open source engine fheroes2 (read only, GPL-2.0, no code copied) and checked against the saves: the creature codes of the Peasant, the Archer and the Black Dragon, the spells of the Mage Guild, the artifacts that were picked up, the weekly growth of a castle. Names are plain strings for now (`.resx` is left for when the UI is localised).
8. **Tests** (86): every save is located, loaded and written back byte for byte; the values of every README row are checked; edits change only their bytes; unknown values survive; the 14 extra bytes survive; pending changes revert one by one; the names of the saves follow the rules; the shell opens both games.

Also done ahead of Phase 5, because the title of the window had to follow the game: `Homm2Module`, `Homm2EditorViewModel` (an `IGameEditor`) and a first view, `Homm2EditorView`, that shows the map, the date and the players. `AppServices` lists both games, and the window title becomes "Siniskalc - Heroes of Might and Magic II" when a savegame of that game is open.

## Phase 4: the write test with the game (in progress)

`EditedSaves` (in the tests) builds 13 savegames from the saves of Broken Alliance, one kind of edit each, with names of at most 8 characters (`E00COPY` is an unchanged control, then gold, resources, army, primary skills, secondary skills, artifacts, spell book with every spell, level and movement, castle buildings and creatures, garrison, a castle given to a player, and everything together). `EditedSavesTests` checks what they hold; the explicit test `WriteTheSavesForTheGame` writes them to `TestData/Homm2/out` (ignored by git) with a README that says what to look at in the game:

```
dotnet test Tests/Siniskalc.Homm2.Tests --filter Name=WriteTheSavesForTheGame
```

Results from the game (2026-10-13): E01-E05, E07, E08, E10 and E11 load and show what was written, so gold, resources, army, primary and secondary skills, the spell book, level, experience, movement, the garrison and a castle's owner are safe to edit. E00 (a byte for byte copy) loads, so nothing in the file is checked; the "Maximus not in the castle" was a wrong expectation (he is outside in save 05). E09: the buildings were as written (the README expectation was unclear about dwellings 1 and 2); the user listed no tavern, to be confirmed. E12 has one town because the owner change is only in E11. **E06 is a real finding:** the artifacts show, but Maximus keeps 2/2/1/1, so the game does not add the bonus of an artifact when it loads a save; either it adds it to the stored skills when the hero picks the artifact up, or the hero screen must be refreshed. **Answered by E13ARTSW** (Maximus gave the Ultimate Sword of Dominion to Sir Gallant in the game): Maximus's attack fell from 15 to 3 and Gallant's rose from 2 to 14, so the savegame keeps the primary skills with the artifact bonuses in them and the game moves them with the artifact. `HeroModel.WithArtifact`, `WithoutArtifact` and `WithArtifact(slot, artifact)` do the same (bonus table `ArtifactBonuses`; only the +12 of the sword is checked in the game, the others are the values of fheroes2). E06 and E12 are made with them now (the saves in `out/` still have the old values: regenerate them if they are to be tried again).

The user copies them to the save folder of HoMM2, loads each and ticks the README. What each result means:
- the control fails: the game rejects a file that is byte for byte a copy of a save (a name, a date or a checksum is checked): stop and find out what;
- the control loads but an edit fails to load: something in that kind of record is checked by the game (the README says which);
- an edit loads but shows something else: the model or the layout of that field is wrong;
- all load and show what was written: the format is safe to edit, and Phase 6 can go ahead.

## Phase 5: the viewer and its pages (done)

- Done: `Homm2Module` is registered in the host, so detection, the recent files and the window title work for both games; the hex viewer works (`SaveRegionProvider`).
- Done: the UX follows the HoMM3 editor. `Homm2EditorView` has the search box and the navigation tree (`NavigationTree`: game, a node per player with its "Heroes" and "Towns" groups, "Neutral" for what nobody owns; nodes show the race and level, the hero that is in a town, a dot for unsaved edits), and a frame with the page of what is picked. Pages (each a `PageViewModel` with tabs, an "edited" mark and a Reset pill): the overview (map, scenario, players that open their page), the player (Resources with the daily income, Heroes with the tavern, Towns), the hero (General, Skills, Creatures, Spells, Artifacts) and the castle (General, Buildings with dwellings, Creatures, Mage Guild). Heroes and towns link to each other.
- Done: the hex tab of the player, the hero and the castle page (`RecordRegions` holds the known fields of each record, `IByteEditTargetFactory` makes the target; the tab shows in advanced mode as in HoMM3), and the rule for the names of the saves: `IGameModule.ValidateSavegameName` (a default of "any name"), which `Homm2Module` answers with the 8 characters before the extension, and which "Save as" asks before it writes. The backup keeps the extension of the game plus `.bak`, which the game does not list, so it needs no rule.
- The game-neutral navigation tree was left for later; it is done in Phase 7.

## Phase 6: editing (done for heroes, players and castles; unverified in the game)

Everything below is a reducer on the immutable model, undoable, with the pages built on shared view models of `Siniskalc.Wpf` (`AmountRowViewModel`, `ToggleRowViewModel`, `ChoiceFieldViewModel`) and `Siniskalc.Heroes.Wpf` (`HeroesArmyViewModelBase`, `HeroesSecondarySkillsViewModel`, `SpellsViewModelBase`, `ArtifactPickerViewModel`), with the HoMM3 pages built on the same ones):

- **Player**: the seven resources (gold up to 2,147,483,647, the others up to 65,535, in both games), quick add, Reset.
- **Hero**: primary skills, level, experience, spell points, movement (and "Full movement"); the army (5 slots, finder); the secondary skills (8 slots in the order learned, the list of 14); the spell book (the Magic Book artifact) and the 65 spells; the 14 artifact slots with a finder. An artifact moves its skill bonus with it (`HeroModel.WithArtifact`), as the game does (E13ARTSW).
- **Castle**: the owner (the castle and the lists of castles of both players change in one step; a player left without a castle starts the countdown of 7 days, as in the savegames), the buildings (castle, tavern, thieves guild, shipyard, well, statue), the six dwellings with the creatures that wait in them (up to 999), the garrison, and the Mage Guild at level 0 to 2 with its spells.
- Changed by the write test (see `docs/HOMM2_SAVE_FORMAT.md`, "What the write test with the game showed"): the dwellings have levels (built, upgraded) and an upgrade replaces the bit; the Mage Guild goes to level 5 and a Wizard has a Library; a stack stops at 32,767; the limits of the resources are now 2,147,483,647 gold and 65,535 of the others in both games (`Heroes.Constants.Resources`; both tried in the game, the real limit of the others is higher but unknown).
- Not done: level and experience kept in step (the game showed level 14 for level 20 and 50,000 experience; E30 to E32 are there to find the rule), the map (the flag of a castle, what a player has explored), the marketplace and the other bits of the high byte of the buildings, the rule that a town needs its castle, the maximum movement (the game works it out).


## Phase 7: what the two games share (done)

HoMM2 was built by copying the structure of the HoMM3 editor, so the two had copies of the navigation tree, the editor view, the styles of the pages and some models. Done, in order, with the whole suite green after each step:

1. **HoMM2 types lose the `Homm2` prefix** (`Homm2HeroModel` is `HeroModel` in `Siniskalc.Homm2.Models`); the wrappers of Core types and the entry points the exe names side by side keep it (`CLAUDE.md`).
2. **A shared editor shell** in `Siniskalc.Wpf`: `EditorViewModelBase<TGame>`, the generic `NavigationTree` and `NavNodeViewModel` (badges and notes), `NavigationEditorView`; each game has an `INavigationBuilder<TGame>`. Core gets `ISavegame` and `ISavegameService<TGame>`.
3. **`Siniskalc.Heroes`** for what means the same in every game (`Resource`, `GameDate`, `TavernHeroes`, `ChangeAreas`) and the interfaces of what each game has its own set of (`IPlayerColor`, `IFaction`, `ISecondarySkill`, `ISkillLevel`, `ILink`, `IOwnerChoice`), with a catalog per game for the enums. HoMM2's `Race` became `Faction`. Colours have one palette, by name. Colours, skill levels, creatures, spells, artifacts, positions and map headers are not merged: they differ in codes, sets or shape.
4. **`Siniskalc.Heroes.Wpf`**: the styles and blocks the pages are made of (tab panels, `PageHeader`, `TabStripStyle`, `ResourcesPanel`, `ItemsCard`, `OwnerPicker`, `HeadedCard`) and the interfaces of the pages (`IResettablePage`, `IResourcesPage`, `IPlayerPage`, `ICastlePage`). The hero, player and castle pages of both games are built from them, and the HoMM3 hero page no longer navigates a frame to a page file per tab.
5. **The HoMM3 primary skills** are the saved values with the artifacts' bonus in them, as in HoMM2 (found by comparing the Arts saves; `KNOWLEDGE_BASE.md`). Putting an artifact on or off changes them, and the page edits the base.
6. **The castle pages have the same tabs and header** (Buildings, Garrison, Available creatures, Mage guild, Hex), and the owner of what nobody owns is "Neutral" in both games.

Seen in the game: nothing of this phase. The pages were checked by the tests that open them in a window (`PageRenderingTests`, `PageContractTests`), not by eye.

## Tests to run in the game, to decode what is still a guess

What the shared models of `Siniskalc.Heroes` would hold for HoMM2 but the editor does not read or write yet, and the saves to make for each. Each test is one scenario, saved before and after one change, named by `TestData/README.md`, so the two files can be compared byte by byte (the way the write test and the Arts saves of HoMM3 were).

1. **Game difficulty** (header, `u16` at 0x0C; read as `GameDifficulty` but never checked against the game; `HOMM2_SAVE_FORMAT.md` says "guess"). Start the same scenario five times, once at each difficulty (easy, normal, hard, expert, impossible), and save at once. The five files differ in this field and, if the game changes the starting resources or anything else with it, in those bytes too. Expected: the value goes 0 to 4. Needed for: the overview and `MapHeader` (review item M13).
2. **Who controls each colour** (done with `TestData/Homm2/HumanCpu`: bytes at 0x1BE and 0x2AD, see `HOMM2_SAVE_FORMAT.md`; a save with a colour switched off, and one with two humans and a computer between them in hot seat order, would still tell more) (header, 0x10 on: the colours in play, then the human-allowed and computer-allowed masks; "guess"). Save at the start of a scenario with 3 or more colours, with (a) every colour set to human in a hot seat game, (b) one human and the rest computer, (c) a colour switched off if the scenario allows it. Also, in the same game, the colour whose turn it is, saved after ending a turn. Expected: the masks follow the setup, and the turn is a separate byte. Needed for: a `GameSetup` for HoMM2 (M13), which today exists only for HoMM3.
3. **Obelisks** (HoMM2 has them and they work like HoMM3's: each one visited shows a piece of the puzzle of the Ultimate Artifact; nothing is decoded). Use a map with a known number of obelisks and save: (a) at the start, (b) after one obelisk is visited by player A, (c) after a second, (d) after player B visits one A already visited, (e) after B visits a different one. Compare to find: the number of obelisks on the map, the visited ones per player (a count or a list of tiles or a bitmap), and whether the visits are per player or shared. HoMM2 has no teams, so there is no "team" case to test; confirm that by checking that no scenario in the game offers alliances. Needed for: moving `ObeliskVisits` (HoMM3) and the obelisk view models into `Heroes` so both games use them.
4. **Hero facing** (hero record, byte 0x22 "changes with every move: facing or a sprite phase"). Move one hero in the eight directions, one step each, and save after each step. Expected: eight values. If it is the facing, `FacingDirection` can move into `Heroes`. Needed for: review item E11.
5. **Temporary morale and luck bonuses** (hero record 0x39 to 0x58, "presumably" from the fountain and the like). Save before and after a hero visits each of: a fountain, an oasis, a temple, a shrine, a swan pond-like object, and a graveyard, and again after the next battle and the next day, to see when each clears. Needed for: a `VisitedBuildingEffects`-like model shared with HoMM3 (E11); low priority.
6. **Spell points maximum** (the editor lets a HoMM2 hero have any spell points up to a limit; the game's maximum is probably knowledge x 10). Save a hero with knowledge 5, 10 and 15 at full spell points after resting, with and without Wisdom, to check. Needed for: sharing `ISpellPointsCalculator` (M16).

For each, once the bytes are known: a row in `HOMM2_SAVE_FORMAT.md`, a layout constant, the reader and the writer (separate classes), the model field, and tests that name the save (`TestData/Homm2/...README.md` says what each save holds).

## To find out: the guardians of a mine (2026-10-22)

A hero can put a few creatures on a mine with the spells Set Earth, Air, Fire and Water Guardian (spell numbers 62 to 65): a number of elementals, not a full garrison. The 7 byte record of a mine (owner, kind, x, y) has no room for them, and no save of the editor's test maps has a guarded mine yet, so where the kind and the number are kept is unknown. Saves to make: one mine of the player with each of the four spells cast on it (name the hero's spell power and knowledge), one with the guardians defeated by an enemy, one with a guarded mine that changes owner. HoMM3 mines have a full garrison in their record (see `FUTURE_IMPROVEMENTS.md`, item 4); the editor should model the guard of a mine in both games with the shared army blocks.

## Phase 8: the saves of Threedom (2026-10-17)

Done: the 71 saves are in `TestData/Homm2/Threedom` with a README; the buildings of a town (turrets, marketplace, the second well, moat, the special building and the captain's quarters), the morale and luck of a hero for its next battle and the places it has visited are read, written and edited (`TownBuildings`, `NextBattleBonus`, `VisitedPlaces`, the hero page), with the tests in `ThreedomTests`. The savegames T01BLDS, T02FORGT and T03BONUS in `TestData/Homm2/out` are for the game.
To test in the game: T01BLDS (the buildings), T02FORGT (a hero visits a fort or gazebo again after forgetting it), T03BONUS (morale and luck of the next battle). Then (saves 72 to 82): the places of the map are listed with their coordinates and a hero's visits are edited one at a time (forts, gazebos, huts, camps, stones, Xanadu), the obelisks are on the player page, a hero remembers fountain, oasis, faerie ring, graveyard, temple, arena and stables (`HeroEffects`), and morale and luck are set together with their source. In the game still to try: T04OBELS (all obelisks), T05FORT (one fort forgotten), T06SRC (the sources).
Not done: the flags of the mines and the towns (the second object of a tile), the owner of a mine, the sphinx, the daemon cave and the shrines (objects with state), the bytes +0xE3 to +0xE6 of a hero, +0x51 of a hero, the bit 0x4000 of the buildings.
