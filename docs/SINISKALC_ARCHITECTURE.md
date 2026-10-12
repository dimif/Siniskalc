# Architecture

## Projects

**`Siniskalc.Core`** (net8.0, no UI) is the editing engine, and it knows no game.
It turns a savefile into a list of byte chunks and keeps them in an immutable document with undo and redo.
An edit goes through an edit session that deserializes a chunk, applies the change and serializes it back, so only the bytes that changed are patched on save and the rest of the file is left as it was.
It also reads and writes the compressed savefile, makes the numbered backups, and stores the user's settings and recent files.

**`Siniskalc.Wpf`** (net8.0-windows) is the reusable user interface, and it knows no game either.
It holds the base view models and commands that every page builds on, the dialogs and message boxes behind interfaces, and the controls any editor of this kind needs: steppers, a search box, the finder, the hex editor and viewer.
It also holds the colours and styles every page shares.

**`Siniskalc`** is the application for Heroes of Might and Magic III (.NET 8, Windows).
It supplies what is specific to the game: where things sit in a savefile, how they are found and converted to models, what the entities are called, and the pages that edit them.
It also builds all the components once, in `AppServices`.

```
Siniskalc/
  Data/           Reading and writing savefile bytes: loaders, chunks, (de)serializers, edit sessions
  Entities/       Game vocabulary: artifacts, creatures, spells, skills, factions, buildings, ...
  Models/         Immutable views of what is in a save: heroes, players, castles, game setup
  Runtime/        Loading and holding the open game
  ViewModels/     MVVM view models and the navigation tree
  Views/          XAML pages (one page per kind of edit)
  Resources/      Icons (the styles that only heroes need are in Views/Generic.xaml)
SharpZipLib.GZip  Trimmed copy of SharpZipLib's GZip code (see "Savegame format" in the README).
Tests/            Unit tests (NUnit), one project per project (see below).
TestData/         Sample savegames (Roe, Sod, Hota) and hero dumps used by the tests.
docs/             Architecture, how things are located in a savefile, knowledge base, user guide, plans.
```

Dependencies point one way: `Siniskalc` references `Siniskalc.Wpf` and `Siniskalc.Core`, and `Siniskalc.Wpf` references `Siniskalc.Core`.
Nothing in Core or WPF should refer to the types of a game: a game gives them its values (`ChunkKind`, `ChangeArea`, `ByteRegion`s, `GameVersion` as an availability tier) and its collaborators, and the HoMM3 side wraps the generic types (`Homm3SaveDocuments`, `Homm3EditSessions`, `Homm3PendingChanges`, `Homm3Enumeration`).
A second game gets a project of its own that references the same two libraries.

Each project has a test project with the same shape: `Siniskalc.Core.Tests` (fakes, no savefiles), `Siniskalc.Wpf.Tests` and `Siniskalc.Tests` (the real savegames).

## Layers

```
SaveDocumentHistory (Data)      immutable SaveDocument snapshots: Current, Original, Undo/Redo, Changed event
  SaveDocument                  chunk collections: Heroes, Castles, Players, Obelisks
    EditSession<TChunk,TModel>  one chunk + its model; Update(edit) deserializes, applies edit, serializes, pushes to history (given its IDeserializer and ISerializer)
      HeroEditSession / CastleEditSession / PlayerEditSession / ObeliskEditSession   (thin: they pick the kind and the collection's (de)serializer)
Models / Entities               immutable models and game vocabulary (HeroModel, SpellBook, Artifact, ...)
ViewModels                      hold immutable models, apply edits through reducers, never touch chunks
Views                           XAML
```

Reading and writing a chunk are separate (`IDeserializer<TChunk, TEntity>`, and `ISerializer<TChunk, TModel>` implemented by the `Safe...Serializer` classes), and a value the editor does not know never stops a load or gets overwritten: see `CLAUDE.md`.

Data flow of an edit: view -> view model property -> `Update(h => h.Reducer(...))` -> `HeroEditSession.Update` -> serializer -> `SaveDocumentHistory.Apply` -> `Changed` -> `HeroViewModel.OnHistoryChanged` -> every child view model `Refresh()`.

## Artifacts, spells and skills

```
Artifact                 EffectInfo.SpellGrants (SpellGrantProvider: named spells, a school, or a level; resolved per game version) + GrantsSpellBook
EquippedArtifacts        GrantsSpellBook; GetTotalEffects() unions. IGrantedSpellsResolver (GrantedSpellsResolver, one per game version) turns them into spell -> names of the artifacts that give it
SpellBook                HasSpellBook, Learned, Granted (from artifacts), OtherAvailable. Available = all three. Immutable.
HeroModel                keeps what depends on the equipped artifacts up to date whenever the artifacts, game version, spell book or skills
                         are set (Reconcile): spell grants, PrimarySkills.Bonus and ArtifactEffects.
                         WithEquippedArtifacts(...) is the reducer: it also gives the spell book Titan's Thunder brings (and never takes it back)
HeroPrimarySkills        Atk/Def/Pow/Kno = saved base values; Bonus = from artifacts; Total... = what counts. Equality compares the base only.
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
Heroes   (HeroChunkCollection)     FindById(heroId), FindByName(name), GetSummaries(), GetSummary(index)     HeroSummary carries Index, HeroId, Name, Owner, ...
Castles  (CastleChunkCollection)   GetModels() (read once per version of the collection), GetModel(i), GetName(i), FindByGarrisonedHero(heroId)
Players  (PlayerChunkCollection)   GetModel(i), GetColor(i), GetColors(), FindByColor(color)
Obelisks (ObeliskChunkCollection)  Any, GetModel() (the visited obelisks, or null when the savefile has none to show); edited with CreateObeliskSession()
SaveDocument.GameVersion
EditSession.IsDirty                differs from the savefile as loaded or saved
EditSession.Revision               an opaque token that changes when this struct is edited, so a view model can tell its own edits from others'
```

A hero is identified by `HeroModel.HeroId` (its place in the savefile's table of heroes; castles hold it as `GarrisonedHeroId` and `VisitingHeroId`), a castle by its index, a player by its colour.
The hex editor is the one place that works on raw bytes, through the byte edit targets of a session.

## Errors

What is thrown, who catches it and what the user is told: see [ERROR_MODEL.md](ERROR_MODEL.md).

## Locating

How heroes, castles, players, the date and the map header are found in a savefile: see [LOCATING_HOMM3_ENTITIES.md](LOCATING_HOMM3_ENTITIES.md).

## Hex views

```
IByteEditTarget (editable)   HeroByteEditTarget / PlayerByteEditTarget / CastleByteEditTarget  ->  HexEditor (hero, player and castle pages)
  Regions                    HeroRegionProvider / PlayerRegionProvider / CastleRegionProvider     known fields of one chunk, from the layout classes
SaveHexDump (read only)      SaveRegionProvider  ->  HexViewer in SaveHexWindow                    Advanced > View save hex
```

`SaveRegionProvider` places the regions of every chunk at its position in the savefile (`ByteRegion.Shifted`), and adds the savefile header, the map header, the setup of the players and the date, using the `Locate`/`FindOffset` methods of their locators.
A hero with a biography has its header and biography in front of the chunk, so the fields of the chunk that fall in the header are left out and the header's own regions are used.
The viewer makes its lines when the grid asks for them (`HexRowList`), because a savefile is about a million bytes; the editor builds a `DataTable` and is for single chunks.
Making the viewer editable would need a byte edit target for the bytes that no chunk covers, and undo for them.

The hex tabs and the Advanced menu are only shown in advanced mode (`MainViewModel.IsAdvancedMode`).
The Hex `NavButton` of each page binds its `Visibility` to it, and a `NavButton` that gets hidden while it is the selected tab selects the first tab.

## Settings and backups

`SettingsStore` loads `UserSettings` (`%APPDATA%\Siniskalc\settings.json`) once and is the only thing that writes the file, so the recent files, `BackupOnSave` (default on) and `AdvancedMode` (default off) never overwrite each other.
A setting missing from the file keeps its default.
`AppServices` builds one store and hands it to `RecentFilesStore`, `NumberedSavefileBackup` and `MainViewModel`; tests give `AppServices` a settings file of their own.

`GameService.SaveGame` calls `ISavefileBackup.Backup(path)` before it writes.
`NumberedSavefileBackup` copies the file to `name.bak`, or `name.bak<N>` with N one above the highest copy in the folder, and never overwrites a copy; it does nothing while `BackupOnSave` is off.
