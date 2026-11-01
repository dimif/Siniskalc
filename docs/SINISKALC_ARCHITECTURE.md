# Architecture

## Projects

**`Siniskalc.Core`** (net8.0, no UI) is the editing engine, and it knows no game.
It turns a savefile into a list of byte chunks and keeps them in an immutable document with undo and redo.
An edit goes through an edit session that deserializes a chunk, applies the change and serializes it back, so only the bytes that changed are patched on save and the rest of the file is left as it was.
It also reads and writes the compressed savefile, makes the numbered backups, and stores the user's settings and recent files.
It defines what the application shell needs from a game: `IGameModule<out TEditor>` (how its savefiles are recognised and picked, and how to make its editor), `IGameEditor` (loading, saving, undo and redo, the pending changes, the bytes to save, and the `ISavegame` it has loaded, if any, which says the name, the file and the history; the editor lives as long as the application and holds at most one savegame; it has no change notification, which is a WPF interface: `Siniskalc.Wpf` adds `IObservableGameEditor : IGameEditor, INotifyPropertyChanged`, which the shell works with and `EditorViewModelBase` implements), `ISavegame` and `ISavegameService<TGame>` (what an editor needs of an open savefile and of the service that loads and saves it), and `IGameEntity` (a name and the code the savefile holds for it).

**`Siniskalc.Wpf`** (net8.0-windows) is the reusable user interface, and it knows no game either.
It holds the base view models and commands that every page builds on, the dialogs and message boxes behind interfaces, and the controls any editor of this kind needs: steppers, a search box, the finder, the hex editor and viewer. The army, skill, spell and artifact editors are not here but in `Siniskalc.Heroes.Wpf`: they name no game, but they are the vocabulary of the series.
It also holds the colours and styles every page shares, and the shell of the application: `ShellWindow` (menus, toolbar, the panel of pending changes and the menu icons), `ShellViewModel` and `SavegameFileCommands`.
The shell picks the game of the file the user opens, and shows the game's editor through the data template of the editor's type.
The editors of the games share `EditorViewModelBase<TGame>` (loading, saving, undo, pending changes, the page that is shown), the generic `NavigationTree` and `NavNodeViewModel` (a game adds badges and notes to a node and fills the tree through its `INavigationBuilder<TGame>`) and `NavigationEditorView` (search box, tree and page frame; the game gives it a page factory).
A page with tabs derives from `TabbedPageViewModelBase`: its tab strip is a `ListBox` bound to `SelectedTab`, and the panels of the tabs sit in one grid, each with the style of its number.

**`Siniskalc.Heroes`** (net8.0, no UI) is what the games of the series have in common and that knows no single game.
It holds the values that mean the same in every game (`Resource`, `GameDate`, `TavernHeroes`, `ChangeAreas`), and small interfaces for the things each game has its own set of: `IPlayerColor`, `IFaction`, `ISecondarySkill` and `ISkillLevel` (all `IGameEntity`), `ILink` (a hero or town shown as a name that opens it) and `IOwnerChoice`.
A game implements them with its own types, and is free to repeat itself: factions and secondary skills come from the game's immutable JSON catalog, while HoMM2's `Color` and HoMM3's `PlayerColor` remain fixed `AbstractEnumeration`s. Models, readers and view models use those typed entities, not JSON records; `SkillLevel` remains an enum with a small catalog (`SkillLevels.Of(level)`).
A colour has a `Neutral` member (code 255, which is the byte of a savefile for what no player owns), and each game's catalog defines a neutral faction (normally code 99). Neutral choices are excluded where a picker requires a playable faction. An unknown colour or faction reads as neutral, so owners and creature factions are never `null`. `OwnerNames<TColor>` names the owners and `ByteEnumerations` handles fixed colour bytes; faction readers and safe serializers use the catalog, preserving unrecognised bytes while the model still holds the stand-in.
A level a hero has not learned is `null`, whatever the game stores.
The colours the editor draws a player with are in `PlayerPalette`, by the name of the colour; they are the editor's colours and not what a savefile holds.
`Siniskalc.Heroes` references nothing of a game and no UI (`HeroesLayeringTests`).
The creature, spell and artifact tables remain game-specific: they differ in codes, sets or meaning. `MapPosition`, `HeroPosition` and `MapHeader` are shared values.

**`Siniskalc.Heroes.Wpf`** (net8.0-windows) is the look of the pages of a Heroes game, and knows no single game either.
`Styles/PageStyles.xaml` has the tab panels (`TabPanel0` to `TabPanel6`, shown while the page's `SelectedTab` is their number), the "edited" mark and Reset pill, the empty note, the templates of a fact row and a link, and the styles of the blocks every page is made of:
`PageHeader` (the marker, the name, what the page adds, the "edited" mark and Reset), `TabStripStyle`, `ResourcesPanel` (a card for each resource, the quick add buttons, a game's own cards in between), `ItemsCard` (a heading and a list, with a note for an empty one), `OwnerPicker` and `HeadedCard` (a card with a heading, a summary and content).
A game's page puts its own content in the blocks; both player pages add the Banishing card (`DaysLeftCard` over `DaysLeftCardViewModel`, the days left of a player who owns no town) to the `ResourcesPanel`.
A game's `Generic.xaml` merges `PageStyles.xaml`. Inside it a `StaticResource` finds only what is defined above it, so the styles of the controls come last.
The blocks bind to what `IResettablePage`, `IResourcesPage`, `IPlayerPage`, `ICastlePage` and `IHeroPage` say a page has (`PageContractTests` puts them on pages that have nothing else).
The page view models share `ResettablePageViewModelBase` (tab, pending changes, Reset, dispose): HoMM3's hero, castle and player pages derive from `SessionPageViewModelBase<TModel>` (the page reads and writes one record through an `IEditSession<TModel>`), both Map pages from `RecordsPageViewModelBase`, and so do HoMM2's (an owner change is a plain edit there; the records that follow it are put in step by the document's reconciler, below).

**`Siniskalc.Homm3`** (net8.0-windows) is the editor for Heroes of Might and Magic III.
It supplies what is specific to the game: where things sit in a savefile, how they are found and converted to models, what the entities are called, the pages that edit them, and the editor that shows them (`Homm3EditorViewModel`, `Homm3EditorView`, `Homm3Module`).

**`Siniskalc.Homm2`** (net8.0-windows) is the editor for Heroes of Might and Magic II, built the same way: `Data` (the layouts of the header, players, heroes and castles, the locator that finds them in the base game's and in the Price of Loyalty's savegames, the chunks and their collections, the deserializers, serializers and differs), `Entities`, `Models`, `Runtime` (`Game`, the parser and `Homm2Module`), and `ViewModels` and `Views` (the overview, the player, hero and castle pages and the navigation builder). Its format is in [HOMM2_SAVE_FORMAT.md](HOMM2_SAVE_FORMAT.md).

**`Siniskalc`** is the exe, the host that starts the window.
It holds `App` and `AppServices`, the composition root that builds all the components once and lists the games (`Homm3Module` and `Homm2Module`; see `HOMM2_PLAN.md`), from the module factory that each game provides (`Homm3ModuleFactory`, `Homm2ModuleFactory`: the wiring of that game, built from what the games share), and `AppInfo` (the name and the web addresses).
A game's data templates are merged in `App.xaml`.

```
Siniskalc.Core         No game, no UI: chunks, history, raw codec, settings, IGameModule / IGameEditor
Siniskalc.Wpf          No game: shell, base view models, controls, EditorViewModelBase, the navigation tree
Siniskalc.Heroes       The series, no UI: shared values and the interfaces of the entities, links and owner
Siniskalc.Heroes.Wpf   The series: the styles and blocks the pages are made of, the interfaces of the pages
Siniskalc.Homm3/
  Data/           Reading and writing savefile bytes: loaders, chunks, (de)serializers, edit sessions
  Entities/       Game vocabulary: artifacts, creatures, spells, skills, factions, buildings, ...
  Models/         Immutable views of what is in a save: heroes, players, castles, game setup
  Runtime/        Loading and holding the open game
  ViewModels/     MVVM view models and the navigation builder
  Views/          XAML pages (one page per kind of edit)
  Resources/      Files of the HoMM3 editor (the styles that only heroes need are in Views/Generic.xaml, with the template of the editor)
Siniskalc.Homm2/  The same folders for HoMM2
Siniskalc/        The exe: App and AppServices (the composition root); each game has its module factory in its own Runtime folder
SharpZipLib.GZip  Trimmed copy of SharpZipLib's GZip code, used by Siniskalc.Homm3 only (see "Savegame format" in the README).
Tests/            Unit tests (NUnit), one project per project (see below).
TestData/         Sample savegames (Roe, Sod, Hota, Homm2) and hero dumps used by the tests.
docs/             Architecture, how things are located in a savefile, knowledge base, user guide, plans.
```

Dependencies point one way: `Siniskalc` references the games and the libraries; `Siniskalc.Homm3` and `Siniskalc.Homm2` each reference `Siniskalc.Heroes.Wpf`, `Siniskalc.Heroes`, `Siniskalc.Wpf` and `Siniskalc.Core`; `Siniskalc.Heroes.Wpf` references `Siniskalc.Heroes` and `Siniskalc.Wpf`; `Siniskalc.Wpf` and `Siniskalc.Heroes` reference `Siniskalc.Core`.
Nothing in Core, WPF or Heroes should refer to the types of a game: a game gives them its values (`ChunkKind`, `ChangeArea`, `ByteRegion`s, `GameVersion` as an availability tier) and its collaborators, and each game wraps the generic types (`Homm3SaveDocuments`, `Homm3EditSessions`, `Homm3PendingChangesFactory` (an `IPendingChangesFactory` of `Siniskalc.Heroes`), `Homm3Enumeration`, and the same for HoMM2).
A game gets a project of its own that references the same libraries; HoMM2 is the second.
A type carries no game prefix (`Siniskalc.Homm2.Models.HeroModel`): the namespace says the game, and the exe uses namespace aliases where it names both.

Each project has a test project with the same shape: `Siniskalc.Core.Tests` (fakes, no savefiles), `Siniskalc.Wpf.Tests`, `Siniskalc.Heroes.Tests`, `Siniskalc.Heroes.Wpf.Tests` (the blocks, rendered), and `Siniskalc.Homm3.Tests` and `Siniskalc.Homm2.Tests` (the real savegames, and the pages of each game opened in a window: `PageRenderingTests`).
A game's tests reference only that game's project, so they build the game from its module factory and never see the other game or the exe (`Homm3LayeringTests`, `Homm2LayeringTests`).
`Siniskalc.Tests` mirrors the exe: it references both games and tests the application as a whole (`AppServices`, the shell with both games), with a few savegames of each game.

## Standard game definitions

The standard entity data belongs to each game, not to Core or the shared Heroes projects.
`GameData\Homm2` and `GameData\Homm3` beside the application contain the shipped JSON for the supported versions.
Files are resolved relative to the application, never the current working directory.
The game's project copies its definitions to build and publish output; the files must be distributed with the executable.

`AppServices` supplies one `IGameDefinitionReader` (`JsonGameDefinitionReader`) to the game module factories.
At startup they read and validate their standard definitions and build typed, immutable catalogs.
Opening a savefile selects its version's already loaded data; it does not read the definitions again.
Every document and its collaborators retain that catalog snapshot throughout loading, editing, pending changes, undo/redo and saving.
There is no mutable global current catalog and neither game can change the other's definitions.

JSON holds creature and ability data, artifacts and their supported effects, factions, castle buildings, supported map-object definitions, spells, hero/faction mappings, secondary skills, spell groupings and war machines where applicable.
References use stable entity IDs rather than localized names or positions in a list.
Save codes remain format-specific; in particular a castle building's bit is interpreted in the context of its faction, not as a globally unique entity code.
Readers, independent safe serializers, reducers, finders and pages use the same typed catalog, not JSON documents.

Definitions supply data and parameters for implemented behaviours, not executable scripts.
New faction content can use existing dwelling, building and effect policies without adding faction-specific branches.
Binary offsets, record sizes, scanning algorithms and encoding stay in the game's Data layer.
The format's fixed capacities still apply: adding data does not enlarge a hero table, spell bitset or building mask, and does not add support to the game executable itself.
Genuinely new binary layouts or behaviour implementations still require C#.

Definitions are read once per application run.
After editing JSON, restart the editor; reopening a savefile is not enough.
There are no custom profiles, user overrides, live reload or in-app entity editor.
Invalid or missing standard definitions prevent startup with `GameDefinitionException`; correct or restore the named file and restart.
This does not change the separate rule that an unknown value inside a savefile loads safely and its original bytes are preserved.
See [GAME_DEFINITIONS.md](GAME_DEFINITIONS.md) for the actual per-game file formats, inheritance, extension examples and contributor checks.

## Adding a game

A new game (HoMM1, fheroes2, VCMI, HoMM4 and later) is a project that references the libraries above and supplies:

1. Its savefile: locator, chunks and collections, deserializers and `Safe...` serializers, differs, edit sessions, an `ISavegameService<Game>` (the shared `SavegameService<Game>`, whose parser is an `ISavegameParser<Game>` of the game) and a `Game : OpenGame`; collections derive from `ModelChunkCollection` (Core), and a list of records of one kind (mines, dwellings, shrines) is a `RecordChunkCollection<TChunk, TModel>` (Core) of `RecordChunk`s (or a chunk derived from it) built from a `RecordKit` (its deserializer, serializer and differ) and edited with `new EditSession<TChunk, TModel>(history, kind, index)`; the module (`IGameModule`) that recognises the file.
2. Its entities and models, with the catalogs that give `IPlayerColor`, `IFaction`, `ISecondarySkill` and `ISkillLevel` to the view models.
3. Its editor view model, an `EditorViewModelBase<Game>` that builds the pages and says which node each belongs to, and a `NavigationBuilder : INavigationBuilder<Game>` that fills the tree.
4. Its pages: view models that implement `IPlayerPage` and `ICastlePage` (and `IResettablePage`, `IOwnerChoice` where they apply), built from the blocks of `Siniskalc.Heroes.Wpf`, with the content only that game has put into them.
5. Its `Generic.xaml`, which merges `PageStyles.xaml` and gives the data template of its editor, and its line in `AppServices`.

What a second savefile format of the same game (fheroes2 for HoMM2, VCMI for HoMM3) would share is the entities, models and pages; splitting the game's project into the rules and the format is left until the first of them is started.

## Layers

```
SaveDocumentHistory (Data)      immutable SaveDocument snapshots: Current, Original, Undo/Redo, Changed event; Apply runs the document's IDocumentReconciler
                                (PlayerListsReconciler of each game keeps the players' lists of heroes, and castles in HoMM2, the HoMM3 table of hero owners and the days left in step with the owners), ApplyAsIs does not (hex edits, Revert all)
  SaveDocument                  chunk collections: Heroes, Castles, Players, Obelisks
    EditSession<TChunk,TModel>  one chunk + its model; Update(edit) deserializes, applies edit, serializes, pushes to history (given its IDeserializer and ISerializer)
      a session class only where it adds a member: HoMM3 HeroEditSession / CastleEditSession (IHeroSession, ICastleSession: what the pages ask of the savefile), PlayerEditSession of both games (IsDirty counts the model only),
      EditSession<ObeliskChunk, ObeliskVisits<TColor>> (shared Core type); HoMM2's heroes and castles use EditSession<TChunk,TModel> itself (Homm2EditSessions.CreateHeroSession / CreateCastleSession)
      the records of the map (mines, dwellings, merchants, shrines, sphinxes) have no session class: EditSession<TChunk,TModel>(history, kind, index) takes the (de)serializer of their RecordChunkCollection
Models / Entities               immutable models and game vocabulary (HeroModel, SpellBook, Artifact, ...)
ViewModels                      hold immutable models, apply edits through reducers, never touch chunks
Views                           XAML
```

Reading and writing a chunk are separate (`IDeserializer<TChunk, TEntity>`, and `ISerializer<TChunk, TModel>` implemented by the `Safe...Serializer` classes), and a value the editor does not know never stops a load or gets overwritten: see `CLAUDE.md`. Constructors, factories and tests take these generics (and `IDiffer<TModel>`) as they are; a game declares its own interface only when it adds members (HoMM3's `ICastleDeserializer`, `IHeroDeserializer`, `IPlayerDeserializer` and `ICastleDiffer`, HoMM2's `ICastleDeserializer` and `IHeroDeserializer` with `DeserializeOwner`, and both games' `IPlayerDiffer`, which is no `IDiffer`).

Obelisk byte conversion is shared in `Siniskalc.Heroes.Data`: `ObeliskDeserializer<TColor>` and `SafeObeliskSerializer<TColor>` take `ObeliskFlagsLayout(FlagsOffset, CountOffset)`. Without a stored count, positions give the count; with one, the byte bounds reading and writing even when positions differ. Writing also stops at the model's flag count and preserves the remaining bytes.

Data flow of an edit: view -> view model property -> `Update(h => h.Reducer(...))` -> `HeroEditSession.Update` -> serializer -> `SaveDocumentHistory.Apply` -> `Changed` -> `HeroViewModel.OnHistoryChanged` -> every child view model `Refresh()`.

## Artifacts, spells and skills

```
Artifact                 EffectInfo.SpellGrants (SpellGrantProvider: named spells, a school, or a level; resolved per game version) + GrantsSpellBook
EquippedArtifacts        GrantsSpellBook; GetTotalEffects() unions. IGrantedSpellsResolver (GrantedSpellsResolver, one per game version) turns them into spell -> names of the artifacts that give it
SpellBook                HasSpellBook, Learned, Granted (from artifacts), OtherAvailable. Available = all three. Immutable.
HeroModel                keeps what depends on the equipped artifacts up to date whenever the artifacts, game version, spell book or skills
                         are set (Reconcile): spell grants, ArtifactEffects, and PrimarySkills (the bonus, and the saved skills move by what the artifacts add or no longer add).
                         WithEquippedArtifacts(...) is the reducer: it also gives the spell book Titan's Thunder brings (and never takes it back)
HeroPrimarySkills        Atk/Def/Pow/Kno = the saved values, which have the artifacts' bonus in them (as in HoMM2); Bonus = what the artifacts add; Base... = saved minus bonus. Equality compares the saved values.
HeroViewModel(session, ..., EffectLineProvider)
  SpellsViewModel(gameVersion, Func<SpellBook>, Action<Func<SpellBook, SpellBook>>)    no HeroModel
  EquippedArtifactsViewModel -> one ArtifactSlotViewModel(label, createFinder, onPicked) per slot
  BackpackViewModel          -> one BackpackSlotViewModel per slot -> ArtifactSlotViewModel(label, createFinder, onPicked)
  ArtifactSlotViewModel asks the factory it was given for its finder when the popup is first shown; the owner decides what the finder offers.
EffectLineProvider       built once in AppServices, passed down by constructor (labels in one place for localization)
```

Facts the savefiles settled (TestData/Hota/Arts): every tome also gives Visions and Magic Arrow (spells of no school); Angelic Alliance gives no spell; Titan's Thunder gives Titan's Lightning Bolt and a spell book.
The file only has one "available" flag per spell, so a spell that is both granted by an artifact and available for another reason is indistinguishable from one that is only granted once it has been saved.

## How view models reach heroes, castles and players

View models never use chunks directly (a test, `ViewModelLayeringTests`, scans `ViewModels/*.cs` for them).
They ask the collections of the current `SaveDocument` (`History.Current`, or `History.Original` for the savefile as loaded):

```
Heroes   (HeroChunkCollection)     IndexOfId(heroId), FindById(heroId), FindByName(name), GetSummaries(), GetSummary(index)     HeroSummary carries Index, HeroId, Name, Owner, ...
Castles  (CastleChunkCollection)   GetModels() (read once per version of the collection), GetModel(i), GetName(i), FindByGarrisonedHero(heroId)
Players  (PlayerChunkCollection)   GetModel(i), GetColor(i), GetColors(), FindByColor(color)
Obelisks (ObeliskChunkCollection<TColor>, in Siniskalc.Heroes, with ObeliskVisits<TColor>, ObeliskChunk and ObeliskDiffer<TColor>)  Any, GetModel() (the visited obelisks, or null when the savefile has none to show); edited with CreateObeliskSession()
SaveDocument.GameVersion
EditSession.IsDirty                differs from the savefile as loaded or saved
EditSession.Revision               an opaque token that changes when this struct is edited, so a view model can tell its own edits from others'
```

A hero's reference id is translated to its page index by `IHeroSummaries.IndexOfId`. HoMM3 resolves `HeroModel.HeroId` in its records; HoMM2 references table indexes. HoMM2's scenario portrait byte (exposed as `HeroModel.PortraitId`, and not part of `IHeroModel`) can differ from the reference id, so its collection supplies `HeroSummary.HeroId` and `Index` from the table index. A castle is identified by its index, a player by its colour.
The hex editor is the one place that works on raw bytes, through the byte edit targets of a session.

Both games derive `Game` from `HeroesSavegameBase<PlayerColor>` and use `SaveMetadata<PlayerColor>` for read-only header facts; their current-player overrides read the editable turn records. Both overviews use `OverviewViewModelBase` for player-card construction and `PlayersInGame.Of` for the same inclusion rule as navigation. Refresh is owned by `EditorViewModelBase` through `IRefreshableViewModel`, on a history change while the overview is shown and before returning to it. Player resource rows are built through `ResourcesEditorViewModel.ForPlayer`, with each game's limits and optional income notes.

## Errors

What is thrown, who catches it and what the user is told: see [ERROR_MODEL.md](ERROR_MODEL.md).

## Logging

A class gets its logger from the static `LogFactory` in `Siniskalc.Core` (`private static readonly ILogger<T> Logger = LogFactory.For<T>();`); the loggers write nowhere until `AppServices` calls `LogFactory.Install`, and
the forwarding logger looks the installed factory up on each use, so a logger created early is never stuck on the silent one.
Only the exe names Serilog: `LoggingSetup` reads `serilog.json` (sinks, template, path, rolling interval, days kept), replaces `{LogFolder}` in the path and the minimum level with what the user set in `settings.json`
(`LogFolder`, `LogLevel`, both optional), and returns the `ILoggerFactory`; `App.OnExit` disposes it to flush the file.
A settings file or a Serilog configuration that cannot be read stops the start with a Reset/Exit dialog (`App.StartServices`).
Where to log, the levels, how to write a message and what never to log are in [LOGGING.md](LOGGING.md).

## Locating

How heroes, castles, players, the date and the map header are found in a savefile: see [LOCATING_HOMM3_ENTITIES.md](LOCATING_HOMM3_ENTITIES.md).

## Navigation tree

`OwnerNavigationBuilderBase` (Heroes.Wpf) fills the tree: the game, then one node per owner made by `OwnerNavGroup`, then what a game adds (the Map node). An owner's node holds sections in the order of the tabs of its page (`OwnerTabs`: General 0, Heroes 1, Towns 2, Mines 3, Dwellings 4, Obelisks 5, Hex 6): `Heroes` and `Towns` list their entries directly, `Mines` and `Dwellings` hold a group for each resource or faction (`NavGroup`, node kind `OwnerSubGroup`) whose children are the mines or dwellings (`NavNodeKind.Mine`, `Dwelling`). A section is only made when it has entries. The groups are not registered as tab nodes, because `NavigationTree` keeps one node for each `(player, tab)`.

What nobody owns is the node `Neutral` (kind `Neutral`, its sections `NeutralTab`), with a page of its own (`NeutralPageViewModelBase` in Heroes.Wpf, `NeutralViewModel` and `NeutralPage` in each game) whose tabs are numbered from the heroes (`NeutralTabs`). Mines and dwellings of one owner are `MinesListViewModel` and, in HoMM3, `OwnedMapObjectsViewModel`, which show the rows whose owner is that owner: the list is filtered again on every history change, so a record given to another owner leaves one page and joins the other. `StructureKey` includes the owners of the mines and dwellings, so the editor rebuilds the tree when one of them changes hands.

The Map page keeps the records no player owns: merchants (HoMM3), shrines and sphinxes (HoMM2).

## Hex views

```
IByteEditTarget (editable)   HeroByteEditTarget / PlayerByteEditTarget / CastleByteEditTarget  ->  HexEditor (hero, player and castle pages)
                             MineByteEditTarget / DwellingByteEditTarget (HoMM3)                 ->  HexEditor (card below the picked mine or dwelling)
  Regions                    HeroRegionProvider / PlayerRegionProvider / CastleRegionProvider     known fields of one chunk, from the layout classes
                             MineRegionProvider / DwellingRegionProvider (HoMM3)                  the fields of a record, from MineLayout / DwellingLayout
SaveHexDump (read only)      SaveRegionProvider  ->  HexViewer in SaveHexWindow                    Advanced > View save hex
```

`SaveRegionProvider` places the regions of every chunk at its position in the savefile (`ByteRegion.Shifted`), and adds the savefile header, the map header, the setup of the players and the date, using the `Locate`/`FindOffset` methods of their locators.
A hero with a biography has its header and biography in front of the chunk, so the fields of the chunk that fall in the header are left out and the header's own regions are used.
The viewer makes its lines when the grid asks for them (`HexRowList`), because a savefile is about a million bytes; the editor builds a `DataTable` and is for single chunks.
Making the viewer editable would need a byte edit target for the bytes that no chunk covers, and undo for them.

The hex tabs and the Advanced menu are only shown in advanced mode (`ShellViewModel.IsAdvancedMode`).
The Hex `NavButton` of each page binds its `Visibility` to it, and a `NavButton` that gets hidden while it is the selected tab selects the first tab.
The Hex cards below a mine or a dwelling (`OwnedMinesCards`, `OwnedDwellingsCard`) do the same through `AdvancedModeVisibility.Apply`, which makes the binding once the card is loaded in a window (a binding to an ancestor made earlier reports an error).

## Settings and backups

`SettingsStore` loads `UserSettings` (`%APPDATA%\Siniskalc\settings.json`) once and is the only thing that writes the file, so the recent files, `BackupOnSave` (default on), `AdvancedMode` (default off) and the two logging choices never overwrite each other.
`EnsureCreated` writes the file the first time, from `Siniskalc.Core/Resources/settings.default.json`.
A setting missing from the file keeps its default.
`AppServices` builds one store and hands it to `RecentFilesStore`, `FileBackupServiceProvider` and `ShellViewModel`; tests give `AppServices` a settings file of their own.

`SavegameService.SaveGame` asks `IFileBackupServiceProvider` for the backup service and calls `Backup(path)` on it before it writes, when the file exists; a backup that fails stops the save and the exception reaches `SavegameFileCommands.Run`, which shows it.
`NumberedSavefileBackupService` copies the file to `name.bak`, or `name.bak<N>` with N one above the highest copy in the folder, never overwrites a copy, has no settings and throws when the file is missing. The provider serves it while `BackupOnSave` is on and `DisabledFileBackupService` (does nothing) while it is off.
