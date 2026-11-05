# Repo rules

These rules apply to everyone (people, Copilot, Claude).
See `docs/SINISKALC_ARCHITECTURE.md` for how the pieces fit together and `docs/FUTURE_IMPROVEMENTS.md` for planned work and handover notes.

## Scope

- **Every instruction here and in `docs/` applies to both games (HoMM3 and HoMM2) unless it names one game or clearly cannot apply to the other.** If you are in doubt whether a rule, a task or a change applies to both, ask before assuming.

## Code layout

- **One top-level type per file, named after the file.** An enum, record, class or interface that other classes use lives in its own file.
  Only a type that is used exclusively inside one other class may be nested or declared in that class's file.
  (`SA1402` is a warning in `.editorconfig`; `SA1649` is not enabled because it crashes on records.)
- Namespaces follow the folders: `Data`, `Entities`, `Models`, `Runtime`, `ViewModels`, `Views`.
  The root namespace is the name of the project: `Siniskalc` (the exe), `Siniskalc.Homm3`, `Siniskalc.Homm2`, `Siniskalc.Core`, `Siniskalc.Wpf`, `Siniskalc.Heroes`, `Siniskalc.Heroes.Wpf`.
- **Code that knows no game lives in `Siniskalc.Core` (no UI) or `Siniskalc.Wpf` (WPF).** Neither names a Heroes of Might and Magic type, and neither references a game's project; a game's values and collaborators are passed in.
  A new project for another game references them.
- **What the games of the series share, and no single game, lives in `Siniskalc.Heroes` (no UI) or `Siniskalc.Heroes.Wpf` (WPF).** Neither references a game's project (`HeroesLayeringTests` checks it).
  A value goes there only when it means the same in every game (`Resource`, `GameDate`); a thing each game has its own set of (a colour, a faction, a skill) is an interface there (`IPlayerColor`, `IFaction`, ...) that each game implements with its own types (a colour or faction is an `AbstractEnumeration` class of the game; the owner of what no player has is the colour `Neutral` and a castle of no known faction has the faction `Neutral`, never `null`), and a page is made of the blocks of `Siniskalc.Heroes.Wpf` and binds to what `IPlayerPage` / `ICastlePage` say it has.
  Do not merge two games' tables (creatures, spells, artifacts) or enums (colours, skill levels) that differ in codes, sets or meaning.
  Add a type to the game's project (`Siniskalc.Homm3` or `Siniskalc.Homm2`) when it needs a layout, rule or entity of that game. The exe `Siniskalc` is only the host that builds the components and starts the window.
- **A type carries no game prefix**: the namespace says the game (`Siniskalc.Homm2.Models.HeroModel`, not `Homm2HeroModel`).
  The exceptions are the wrappers of generic Core types (`Homm3Enumeration`, `Homm2EditSessions`, `Homm2PendingChangesFactory`, `Homm2SaveDocuments`, `Homm2ChunkKinds`) and the entry points the exe and the shell name side by side (`Homm2Module`/`Homm3Module`, `Homm2EditorViewModel`/`Homm3EditorViewModel`, `Homm2EditorView`/`Homm3EditorView`).
  Code that sees both games (`AppCompositionRoot`, `AppServices`) uses namespace aliases.
- Each project has a test project with the same shape (`Siniskalc.Core.Tests`, `Siniskalc.Wpf.Tests`, `Siniskalc.Heroes.Tests`, `Siniskalc.Heroes.Wpf.Tests`, `Siniskalc.Homm3.Tests`, `Siniskalc.Homm2.Tests`, and `Siniskalc.Tests` for the exe); a test goes with the project of the class it tests.
  **A game's test project references only that game's project** and the test library `Siniskalc.Tests.Homm.Common` (never the other game or the exe `Siniskalc`; the `...LayeringTests` of each game check it). That library has no test and no game: it holds what the test projects of both games would otherwise copy (`TestKinds`, `ByteDiff`, the fake of the pending changes, the WPF test application, the cache of loaded savegames) and builds what it needs from the game's module factory (`Homm3ModuleFactory`, `Homm2ModuleFactory`) and the shared components. A test that needs the whole application (both games, `AppServices`) goes in `Siniskalc.Tests`.
- **A control that is styled by a resource dictionary is tested by what it shows**, not only by its bindings: a template that did not apply leaves the control empty and raises no binding error (`PageRenderingTests`, `PageContractTests`).
- A view model is called `...ViewModel` (also when it is a node of a tree: `NavNodeViewModel`).

## Layers

- `Models` are **immutable**.
  Change them with `with` or a `With...` method (a *reducer*: pure, returns a new model).
  Models do not raise events.
  When a change in one part of a hero must change another (equipped artifacts change the spells a hero can cast and its skills), a reducer on `HeroModel` does it, so the model is never left half updated.
- **View models and views never use byte chunks** (`HeroChunk`, `CastleChunk`, `PlayerChunk`, `BytesChunk`, `byte[]` of a savefile).
  They work with models and ask the collection classes (heroes by hero ID or name, castles, players by colour) for them.
  When a view model needs several models of one kind, add or use a collection class rather than looping over chunks.
  The only exception is the hex editor and its byte edit targets.
- Savefile detection heuristics (how a loader finds a block) and byte layouts (`HeroOffsets`, `CastleLayout`, `PlayerLayout`) stay in the `Data` classes that use them.
  They are layout, not value ranges.
- **Reading and writing a chunk are separate and independent.** A deserializer (chunk to model) never uses a serializer, and a serializer (model to chunk) never uses a deserializer; neither takes the other as a dependency, and they only share layouts (`HeroOffsets`, `CastleLayout`, `PlayerLayout`) and the constants and helpers (`ByteBufferHelper`).
  A serializer writes the model it is given, as it is (`Serialize(chunk, model)`); whoever edits builds the final model.
  When a field's byte can mean more than the model says (an unknown creature, an owner byte that is no colour, a flag that is any non-zero value), the serializer looks at the byte in the chunk and leaves it alone while the model does not disagree with what that byte reads as.
  It checks the byte itself, it does not call the deserializer.
  Serializers implement `ISerializer<TChunk, TModel>` and are named `Safe<Model>Serializer`; deserializers implement `IDeserializer<TChunk, TEntity>` (one `Deserialize(chunk)` method).
- **A value the editor does not know never stops a load.** A deserializer reads a byte that is no known entity as the first known one (a hero class that is unknown reads as `Knight`, a facing that is no direction as `North`; for a colour or a faction that is none or unknown, as its `Neutral` member; `null` only for a value that really can be absent) and does not throw.
  The serializer then leaves that byte alone while the model still holds the stand-in, and writes it as soon as the model holds anything else.

## Dependencies

- A class that uses another component (a loader, locator, provider, parser, service, view-model collaborator) gets it through its constructor, preferably as an interface, and never creates it or calls it as a static method.
  `AppCompositionRoot` is the composition root: it builds each component once and is the only place that does. `AppServices` only holds what it is given; it builds nothing.
  **No dependency is optional for the sake of tests**: a parameter is required, a unit test passes a mock and a test that needs the real thing passes the real thing; never `x ?? new Real()`.
  Tests build the same graph with `TestServices`, or give the unit under test fakes.
  What depends on the game version of a savefile (`VersionSpecificEntitiesProvider`, the hero reader and writer and the hero chunk collection) is made per savefile: the provider by `IVersionSpecificEntitiesProviderFactory`, the rest by `SaveDocumentFactory`, which is the one place besides `AppCompositionRoot` that builds such objects.
- Static classes are for helpers, fixed data and pure functions of immutable models (`RangeGuard`, `Constants`, layouts, extension methods, `CastleBuilder` and `LevelCalculator`, which are reducers and tables).
  Behaviour that has collaborators, state or may be swapped (loaders, locators, providers, serializers, differs) is an injected instance, not a static class.
- **The one exception is logging**: a class gets its logger from the static `LogFactory`, in a `private static readonly ILogger<T> Logger = LogFactory.For<T>();` field (`For(typeof(T))` in a static class), and does not take it through its constructor.
  `AppCompositionRoot` installs the real factory once; until then, and in the tests, loggers write nowhere.

## Values and limits

- Every minimum, maximum, step or size of a value lives in `Constants.cs` (flat constants for the old ones, a nested class per domain for the rest: `Constants.Hero`, `Constants.Map`, `Constants.Army`, ...).
  XAML steppers use `x:Static` on those constants, so the stepper, the "Max" buttons and the guards agree.
- Range checks, clamps and out-of-range exceptions go through `RangeGuard`, using those constants.
  It throws `ArgumentOutOfRangeException`.

## Text

- User-visible wording that is built in code (effect lines, notes) comes from an injected provider such as `EffectLineProvider`, not from static methods, so it can be localized later.
  Prefer `.resx` for names.

## Styles

- Keep page-specific styles inline in the page.
  Only styles used by several pages go to `Views/Generic.xaml`.

## Working in this repo

- The WPF exe is often running and locks the build output.
  Build or test with `--artifacts-path <another folder>` (or `-p:OutDir=<another folder>`).
- Several people and tools edit this tree at once.
  Re-read a file before editing it.
- **Run the tests in proportion to the step.** Between the steps of a piece of work, build and test only the projects that can be affected: the project you changed, its test project, and the test projects of the projects that reference it (`dotnet test Tests/<Project>.Tests --artifacts-path <folder>`, with `--filter` while you iterate).
  Run the whole suite (`dotnet test Siniskalc.sln --artifacts-path <folder>`) **once per batch** of related changes, before the last commit of the batch or before you hand the work over, and say so in your report.
  A full run takes minutes and prints a lot: do not repeat it after every step, and do not paste its whole output.
- **Use the cheapest agent or model that can do a step.** A mechanical step (renames, moving a type, updating usings and docs, adding a test that follows a pattern, applying a decided fix) goes to a cheaper model; keep the stronger one for design, for reading an unknown savefile layout and for anything that decides what is written to a savefile.
  An agent that thinks a step needs a stronger model says so in its report instead of guessing.
- Do not push unless asked.
- Log through `LogFactory.For<T>()`, following `docs/LOGGING.md` (where to log, levels, message templates, what never to log, what is spam). The `logging-reviewer` agent (`.github/agents/logging-reviewer.agent.md`, `.claude/agents/logging-reviewer.md`) checks code against it.
  Where the editor skips, keeps or falls back on something it did not expect, log it there, with the call itself (no `// TODO:LOG` markers; see `docs/ERROR_MODEL.md`).

## Tests

- **One test class per subject, named `<Subject>Tests`, in a file of the same name** (`GrantedSpellsResolverTests.cs`).
  The folder follows the layer of the subject (`Data`, `Entities`, `Models`, `Runtime`, `ViewModels`).
  A test class is not split into partial classes over several files; share helpers through a small `internal static` class in its own file (`CastleSamples`, `CastleMasks`).
- **A test method is a PascalCase sentence that states the behaviour being checked**, such as `TheDateOfEachSavegameIsRead`, `RemovingABuildingGivesTheMasksOfNeverHavingBuiltIt` or `ACreatureIsRangedExactlyWhenItHasShots`.
  No `Method_State_Result` underscores, no `Test` or `Should` prefix, and not just the name of a hero or type.
  Name the thing under test the way the code does (`GrantedSpellsResolver`, not an older name).
  Write full sentences with articles, subject first and the observable result after it (`ANeutralCastleIsWrittenAsTheByteFF`, not `NeutralCastleWrittenAsFF`).
  Start with a member name only when that member is the public API under test (`ToBytesAppliesChunkEdits`).
  A save, hero or castle is named only together with what it shows (`TazarStartsWithEquestriansGlovesAndNoSpellBook`, not `TazarIsDeserialized`), and a name never just repeats the subject (`DeserializingHeroDeserializes`).
- Tests that rely on what a savefile contains say which save, and test data lives in `TestData/`.
  **How savefiles are named and organised (`<series>_<NN>_<what changed>.GM1`, folders, numbering, what to update when adding one) is in `TestData/README.md`: read it before adding, renaming or looking for a save.** `TestDataNamesTests` enforces it; each `TestData` folder's own `README.md` says what its saves hold.
- A class that tests one scenario, save or game version across several subjects (`RoeCastleTests`, `HotaSaveTests`, `SodSaveTests`) is allowed to be named after that scenario instead of one subject.
  An integration test of one savefile feature through every layer (reader, writer, history, view model) may be named after that feature (`GenderTests`, `ObeliskVisitsTests`).
  Its methods are still sentences about the behaviour.
  Unit and multi-unit test classes are always named after one production type.
- **Say what kind of test a class is** (enforced by `TestFixtureConventionsTests` in each test project).
  A unit test checks one class, with Moq fakes for its collaborators and no savefiles; it is `[Category("Unit")]` and lives in the layer folder.
  A test of several real classes without savefiles is `[Category("MultiUnit")]` under `MultiUnit/<Layer>/`.
  A test that reads `TestData`, writes to disk or builds the whole graph (`TestServices`) is `[Category("Integration")]` under `Integration/<Layer>/`.
  Only integration tests and tests on binary data (deserializers, serializers) may be slow.
- A test never ends in `Assert.Pass` or skips its assertions with `continue`; a serializer test asserts the bytes, not what the deserializer reads back.
- The `test-reviewer` agent (`.github/agents/test-reviewer.agent.md`, `.claude/agents/test-reviewer.md`) repeats the review and reports.
