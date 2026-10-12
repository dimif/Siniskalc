# Repo rules

These rules apply to everyone (people, Copilot, Claude).
See `docs/SINISKALC_ARCHITECTURE.md` for how the pieces fit together and `docs/FUTURE_IMPROVEMENTS.md` for planned work and handover notes.

## Code layout

- **One top-level type per file, named after the file.** An enum, record, class or interface that other classes use lives in its own file.
  Only a type that is used exclusively inside one other class may be nested or declared in that class's file.
  (`SA1402` is a warning in `.editorconfig`; `SA1649` is not enabled because it crashes on records.)
- Namespaces follow the folders: `Data`, `Entities`, `Models`, `Runtime`, `ViewModels`, `Views`.
  The root namespace is the name of the project: `Siniskalc`, `Siniskalc.Core`, `Siniskalc.Wpf`.
- **Code that knows no game lives in `Siniskalc.Core` (no UI) or `Siniskalc.Wpf` (WPF).** Neither names a Heroes of Might and Magic type, and neither references the application project; a game's values and collaborators are passed in.
  A new project for another game references them.
  Add a type to the application project when it needs a HoMM3 layout, rule or entity.
- Each project has a test project with the same shape (`Siniskalc.Core.Tests`, `Siniskalc.Wpf.Tests`, `Siniskalc.Tests`); a test goes with the project of the class it tests.
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
- **A value the editor does not know never stops a load.** A deserializer reads a byte that is no known entity as the first known one (a hero class that is unknown reads as `Knight`, a facing that is no direction as `North`; for values that can be absent, such as a castle's owner or faction, as `null`) and does not throw.
  The serializer then leaves that byte alone while the model still holds the stand-in, and writes it as soon as the model holds anything else.

## Dependencies

- A class that uses another component (a loader, locator, provider, parser, service, view-model collaborator) gets it through its constructor, preferably as an interface, and never creates it or calls it as a static method.
  `AppServices` is the composition root: it builds each component once and is the only place that does.
  Tests build the same graph with `TestServices`, or give the unit under test fakes.
  What depends on the game version of a savefile (`VersionSpecificEntitiesProvider`, the hero reader and writer and the hero chunk collection) is made per savefile: the provider by `IVersionSpecificEntitiesProviderFactory`, the rest by `SaveDocumentFactory`, which is the one place besides `AppServices` that builds such objects.
- Static classes are for helpers, fixed data and pure functions of immutable models (`RangeGuard`, `Constants`, layouts, extension methods, `CastleBuilder` and `LevelCalculator`, which are reducers and tables).
  Behaviour that has collaborators, state or may be swapped (loaders, locators, providers, serializers, differs) is an injected instance, not a static class.

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
- When extracting a new model, leave existing usages unchanged unless asked to update them.

## Working in this repo

- The WPF exe is often running and locks the build output.
  Build or test with `--artifacts-path <another folder>` (or `-p:OutDir=<another folder>`).
- Several people and tools edit this tree at once.
  Re-read a file before editing it and run the whole suite, not only the tests of the area you touched.
- Do not push unless asked.
- Where the editor skips, keeps or falls back on something it did not expect, leave `// TODO:LOG <level>: what to log` (see `docs/ERROR_MODEL.md`).
  There is no logger yet.

## Tests

- **One test class per subject, named `<Subject>Tests`, in a file of the same name** (`GrantedSpellsResolverTests.cs`).
  The folder follows the layer of the subject (`Data`, `Entities`, `Models`, `Runtime`, `ViewModels`).
  A test class is not split into partial classes over several files; share helpers through a small `internal static` class in its own file (`CastleSamples`, `CastleMasks`).
- **A test method is a PascalCase sentence that states the behaviour being checked**, such as `TheDateOfEachSavegameIsRead`, `RemovingABuildingGivesTheMasksOfNeverHavingBuiltIt` or `ACreatureIsRangedExactlyWhenItHasShots`.
  No `Method_State_Result` underscores, no `Test` or `Should` prefix, and not just the name of a hero or type.
  Name the thing under test the way the code does (`GrantedSpellsResolver`, not an older name).
- Tests that rely on what a savefile contains say which save, and test data lives in `TestData/`.
  **How savefiles are named and organised (`<series>_<NN>_<what changed>.GM1`, folders, numbering, what to update when adding one) is in `TestData/README.md`: read it before adding, renaming or looking for a save.** `TestDataNamesTests` enforces it; each `TestData` folder's own `README.md` says what its saves hold.
- A class that tests one scenario, save or game version across several subjects (`RoeCastleTests`, `HotaSaveTests`, `SodSaveTests`) is allowed to be named after that scenario instead of one subject.
  Its methods are still sentences about the behaviour.
