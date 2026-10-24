# Test review

A review of the three test projects: whether the tests are meaningful, which tests are inconclusive, where coverage is excessive or missing, whether each test is in the right project and folder, whether unit tests test one unit in isolation, and how long they run.
It is a report; nothing in the tests was changed.
The backlog at the end lists the fixes in order.
`.github/agents/test-reviewer.agent.md` (Copilot) and `.claude/agents/test-reviewer.md` (Claude) repeat this review.

Reviewed at commit `c352615`, 2026-10-24.

## Summary

- **1,911 tests, all passing.** `Siniskalc.Core.Tests` 43 (0.3 s), `Siniskalc.Wpf.Tests` 27 (0.3 s), `Siniskalc.Tests` 1,841 (2 min 05 s).
  The whole solution takes 2 min 22 s, build included.
- **Of 93 test classes, 21 are unit tests, 4 multi-unit, 50 integration, and 18 look like unit tests but are not isolated.**
  Those 18 are named after one class and test it through a real savefile or the whole `TestServices` graph.
- **The tests are mostly meaningful.**
  The savefile scenario tests are the strongest part of the suite: every claim about a byte is tied to a named save.
  Real problems: one `Assert.Pass` that skips 48 cases, two loops that silently skip cases, and serializer tests that check the serializer by running the deserializer, which `CLAUDE.md` forbids.
- **Run time is spent where it should be (savefile tests), with three exceptions.**
  Hero view-model tests load a full save for every test (about 0.13 s each, 70 tests).
  `CastleBuilder*Tests` replay every sample castle (about 14 s) to test a pure reducer.
  Tests that walk every savefile run hundreds of cases (`ObeliskVisitsTests` 359, `PlayerHeadTests` 352).
- **Coverage is uneven.**
  Castle building progression is asserted in about eight classes.
  The player and obelisk serializers, `PlayerDiffer`, the castle and obelisk loaders, and the castle and player byte edit targets have no direct tests.
  Many view models and providers are only covered through other tests.
- **Nothing marks a test's kind today.** There is no `[Category]` and no folder split, and there is no mocking library; the isolated tests use hand-written fakes.

## How the review was done

- Every test file was read; the production class under test was opened when needed to judge isolation.
- `dotnet test Siniskalc.sln --logger trx --artifacts-path <temp>` was run, and the per-test durations were taken from the TRX files.
  The per-test durations add up to the wall-clock time of the test host (126 s against 125 s), so they are reliable.
- Kinds of test:
  - **Unit**: one class; its collaborators are fakes or plain values; no files.
  - **Multi-unit**: several real classes working together, no savefiles (for example `SaveDocument` with `SaveDocumentHistory`, or a WPF window).
  - **Integration**: reads real savefiles or `TestData`, writes to disk, or builds the whole `TestServices` / `MainViewModel` graph.
  - **Unit-shaped, not isolated**: named and laid out like a unit test of one class, but actually an integration test.
- Time thresholds that flag a test: unit and multi-unit tests over **100 ms**, integration tests over **2 s**.

## Every test class

Times are the sum and maximum over the class's test cases, from one run.
The first test of a class also pays for JIT and loading the save, so a single slow first test is not a finding on its own.

| Class | Project / folder | Kind (actual) | Tests | Total s | Max s | Reads | Notes |
|---|---|---|---:|---:|---:|---|---|
| `RangeGuardTests` | Core.Tests / (root) | Unit | 4 | 0.00 | 0.00 |  |  |
| `ByteEnumTests` | Core.Tests / Data | Unit | 1 | 0.01 | 0.01 |  |  |
| `SaveDocumentTests` | Core.Tests / Data | Multi-unit | 7 | 0.01 | 0.01 |  | 2 subjects (`SaveDocument` + `SaveDocumentHistory`) |
| `SavefileBytesPatcherTests` | Core.Tests / Data | Unit | 3 | 0.00 | 0.00 |  |  |
| `GzipSavefileCodecTests` | Core.Tests / Runtime | Integration | 5 | 0.14 | 0.10 | temp files |  |
| `NumberedSavefileBackupTests` | Core.Tests / Runtime | Integration | 6 | 0.04 | 0.02 | temp files |  |
| `RecentFilesStoreTests` | Core.Tests / Runtime | Integration | 5 | 0.05 | 0.02 | temp files |  |
| `SettingsStoreTests` | Core.Tests / Runtime | Integration | 6 | 0.05 | 0.01 | temp files | also tests `RecentFilesStore` |
| `ShellUrlLauncherTests` | Core.Tests / Runtime | Unit | 6 | 0.00 | 0.00 |  |  |
| `ConstantsTests` | Tests / (root) | Unit | 3 | 0.01 | 0.01 |  |  |
| `ActiveSpellsTests` | Tests / Data | Integration | 12 | 0.73 | 0.26 | Hota/Arts |  |
| `BiographyTests` | Tests / Data | Integration | 6 | 0.14 | 0.06 | bios_* |  |
| `CastleBuilderRulesTests` | Tests / Data | **Unit-shaped, not isolated** | 13 | 3.83 | 3.48 | all castle samples | subject is in Models; walks every sample |
| `CastleBuilderTests` | Tests / Data | **Unit-shaped, not isolated** | 4 | 10.38 | 3.23 | Rampart/Bulwark/Cove | subject is in Models; replays >200 castles |
| `CastleDifferTests` | Tests / Data | Integration | 9 | 0.49 | 0.12 | bulwark_13 | tests the differ only through history |
| `CastleEditingTests` | Tests / Data | Integration | 11 | 0.70 | 0.11 | bulwark_24 |  |
| `CastleTests` | Tests / Data | Integration | 10 | 0.21 | 0.05 | castles_00..02 |  |
| `CollectionAccessTests` | Tests / Data | Integration | 5 | 0.30 | 0.12 | 2 saves | multi-subject |
| `CoveCastleTests` | Tests / Data | Integration | 16 | 1.17 | 0.24 | Cove series | contains `MainViewModel` tests |
| `DwellingCreaturesTests` | Tests / Data | Unit | 15 | 0.00 | 0.00 |  |  |
| `FactoryCastleTests` | Tests / Data | Integration | 22 | 0.76 | 0.05 | Factory series |  |
| `GameDateTests` | Tests / Data | Integration | 16 | 1.57 | 0.96 | Hota + Sod series | subject `GameDate` is in Models; uses `OverviewViewModel` |
| `GenderTests` | Tests / Data | Integration | 12 | 0.17 | 0.05 | castles_*, bios_custom |  |
| `HeroByteEditTargetTests` | Tests / Data | **Unit-shaped, not isolated** | 9 | 0.64 | 0.07 | Hota empire save | full `GameService` in `SetUp` |
| `HeroDeserializerTests` | Tests / Data | Integration | 5 | 0.02 | 0.01 | Heroes/*.bin | real entity provider; vague names |
| `HeroEditSessionTests` | Tests / Data | **Unit-shaped, not isolated** | 3 | 0.00 | 0.00 | Heroes/*.bin | real `DocumentFactory`; vague names |
| `HeroOffsetsTests` | Tests / Data | Unit | 7 | 0.00 | 0.00 |  | terse names |
| `HeroRegionProviderTests` | Tests / Data | **Unit-shaped, not isolated** | 7 | 0.54 | 0.09 | Hota empire save | uses the real serializer and deserializer |
| `HotaFourCornersTests` | Tests / Data | Integration | 2 | 0.06 | 0.04 | tan_not_parsing |  |
| `HotaSaveTests` | Tests / Data | Integration | 8 | 0.69 | 0.38 | Hota empire (`OneTimeSetUp`) |  |
| `HotaSleepGarrisonVisitingTests` | Tests / Data | Integration | 18 | 0.49 | 0.14 | SleepGarrisonVisiting series |  |
| `MageGuildSpellEditingTests` | Tests / Data | **Unit-shaped, not isolated** | 10 | 0.37 | 0.10 | bulwark_33 | `CastleBuilder` rule tests through a save |
| `MageGuildSpellsTests` | Tests / Data | Integration | 8 | 0.19 | 0.04 | Roe + Bulwark |  |
| `MapDifficultyTests` | Tests / Data | Integration | 6 | 0.20 | 0.08 | 5 saves |  |
| `MapHeaderTests` | Tests / Data | Integration | 8 | 0.28 | 0.09 | Hota + Sod |  |
| `MovedTodayTests` | Tests / Data | Integration | 13 | 0.34 | 0.10 | Hota/Arts |  |
| `ObeliskVisitsTests` | Tests / Data | Integration | 359 | 8.41 | 0.38 | every save | `Assert.Pass`; view-model tests inside |
| `OtherFactionsCastleTests` | Tests / Data | Integration | 53 | 18.46 | 6.05 | OtherFactions series |  |
| `PlayerAndCastleRegionProviderTests` | Tests / Data | **Unit-shaped, not isolated** | 6 | 0.09 | 0.07 | 2 saves | 3 subjects |
| `PlayerControlTests` | Tests / Data | Integration | 14 | 0.31 | 0.07 | player saves | locator unit tests mixed in |
| `PlayerHeadTests` | Tests / Data | Integration | 352 | 5.93 | 0.09 | every save | view-model tests inside |
| `PlayerTests` | Tests / Data | Integration | 8 | 0.81 | 0.17 | Hota empire 01/02 |  |
| `PlayersInTheGameTests` | Tests / Data | **Unit-shaped, not isolated** | 5 | 0.05 | 0.01 | conflux_00 | a pure filter fed from a save |
| `RampartAndBulwarkCastleTests` | Tests / Data | Integration | 39 | 2.48 | 0.08 | Bulwark series | overlaps `RampartCastleTests` |
| `RampartCastleTests` | Tests / Data | Integration | 31 | 1.03 | 0.04 | Rampart series |  |
| `RoeBuildingEditsTests` | Tests / Data | Integration | 62 | 1.80 | 0.04 | Roe 00..55 | loop skips cases with `continue` |
| `RoeCastleBuildingsTests` | Tests / Data | Integration | 146 | 11.71 | 1.43 | Roe 12..49 |  |
| `RoeCastleTests` | Tests / Data | Integration | 46 | 4.32 | 0.41 | Roe 00..11 |  |
| `RoeHeroTests` | Tests / Data | Integration | 16 | 0.82 | 0.28 | Roe series | `HeroViewModel` tests inside |
| `RoeMapStateTests` | Tests / Data | Integration | 32 | 1.00 | 0.11 | Roe 56..77 | raw-layout research |
| `RoeSanyaMovementTests` | Tests / Data | Integration | 5 | 0.49 | 0.12 | sanya_* |  |
| `SafeCastleSerializerTests` | Tests / Data | **Unit-shaped, not isolated** | 11 | 9.07 | 4.75 | castle samples | checks through the deserializer; session tests inside |
| `SafeHeroSerializerTests` | Tests / Data | **Unit-shaped, not isolated** | 21 | 0.02 | 0.00 | Heroes/*.bin | checks through the deserializer |
| `SaveRegionProviderTests` | Tests / Data | Integration | 15 | 1.46 | 0.63 | 3 saves | a `ByteRegion` unit test inside |
| `SodHotseatTests` | Tests / Data | Integration | 15 | 0.63 | 0.19 | Sod/Hotseat |  |
| `SodSaveTests` | Tests / Data | Integration | 16 | 0.27 | 0.07 | sod_game_00/01 |  |
| `SodSeriesTests` | Tests / Data | Integration | 95 | 5.17 | 1.86 | conflux_00..34 + all Roe/Sod | reloads a save per case |
| `TestDataNamesTests` | Tests / Data | Unit | 10 | 0.01 | 0.00 | TestData tree (names only) |  |
| `VisitedBuildingEffectsTests` | Tests / Data | Integration | 9 | 0.35 | 0.09 | Sod/Hotseat + Roe | `HeroViewModel` test inside |
| `ArtifactTests` | Tests / Entities | Unit | 1 | 0.00 | 0.00 |  |  |
| `BuildingTests` | Tests / Entities | Unit | 9 | 0.03 | 0.02 |  |  |
| `CreatureInfoTests` | Tests / Entities | Unit | 4 | 0.00 | 0.00 |  |  |
| `EntityInvariantTests` | Tests / Entities | Unit | 5 | 0.01 | 0.00 |  | table invariants of 3 entity types |
| `ChangeAreaTests` | Tests / Models | Unit | 1 | 0.00 | 0.00 |  |  |
| `GrantedSpellsResolverTests` | Tests / Models | Unit | 1 | 0.00 | 0.00 |  | 1 test |
| `HeroEditingTests` | Tests / Models | **Unit-shaped, not isolated** | 15 | 1.08 | 0.20 | Hota empire save | reducer tests mixed with save tests |
| `OwnerNamesTests` | Tests / Models | Unit | 3 | 0.00 | 0.00 |  |  |
| `SaveBaselineTests` | Tests / Models | Integration | 7 | 2.67 | 0.62 | Hota empire save + temp | in Models; multi-subject |
| `SpellBookAndGrantsTests` | Tests / Models | **Unit-shaped, not isolated** | 20 | 0.27 | 0.03 | Hota/Arts saves | reducers mixed with save round trips |
| `SpellPointsCalculatorTests` | Tests / Models | Unit | 1 | 0.00 | 0.00 |  | 1 test |
| `GameServiceTests` | Tests / Runtime | Integration | 6 | 0.17 | 0.11 | conflux_00 + temp |  |
| `SavegameParserTests` | Tests / Runtime | **Unit-shaped, not isolated** | 2 | 0.00 | 0.00 |  | uses the static `TestServices.GameService` |
| `ArtifactFinderTests` | Tests / ViewModels | Multi-unit | 8 | 0.40 | 0.14 | Hota empire (2 tests) | unit and save tests mixed |
| `CastleDetailsViewModelTests` | Tests / ViewModels | Integration | 13 | 0.28 | 0.05 | castle saves |  |
| `CastleEditingViewModelTests` | Tests / ViewModels | Integration | 10 | 5.54 | 5.34 | bulwark_24 + WPF | real window |
| `CastleGarrisonedHeroTests` | Tests / ViewModels | Integration | 4 | 0.06 | 0.02 | roe_61 |  |
| `CastlePillAndTavernLinkTests` | Tests / ViewModels | Integration | 5 | 0.12 | 0.03 | several | multi-subject |
| `CastleViewModelTests` | Tests / ViewModels | Integration | 9 | 0.12 | 0.02 | castles_01 |  |
| `CreatureFinderAndArtifactEffectsTests` | Tests / ViewModels | **Unit-shaped, not isolated** | 11 | 1.53 | 0.17 | Hota empire per test | 2 subjects |
| `EquippedArtifactsViewModelTests` | Tests / ViewModels | Unit | 6 | 0.00 | 0.00 |  |  |
| `HeroEditingViewModelTests` | Tests / ViewModels | **Unit-shaped, not isolated** | 14 | 1.91 | 0.16 | Hota empire per test |  |
| `HeroGeneralTabTests` | Tests / ViewModels | **Unit-shaped, not isolated** | 9 | 1.32 | 0.17 | Hota empire per test | not named after a class |
| `PlayerAndOverviewViewModelTests` | Tests / ViewModels | Integration | 17 | 1.50 | 0.14 | Hota empire per test | 2 subjects |
| `SavegameFileCommandsTests` | Tests / ViewModels | Integration | 9 | 0.21 | 0.07 | conflux_00 + temp |  |
| `ShellSmokeTests` | Tests / ViewModels | Integration | 6 | 7.02 | 2.87 | several + WPF | scenario |
| `SkillsAndSpellsViewModelTests` | Tests / ViewModels | **Unit-shaped, not isolated** | 16 | 1.98 | 0.14 | Hota empire per test | 2 subjects |
| `SpellsFromArtifactsTests` | Tests / ViewModels | Multi-unit | 4 | 0.51 | 0.13 | Hota empire per test | a scenario; label it integration |
| `ViewModelLayeringTests` | Tests / ViewModels | Unit | 1 | 0.01 | 0.01 | source files | architecture rule |
| `AboutViewModelTests` | Wpf.Tests / ViewModels | Unit | 2 | 0.01 | 0.01 |  |  |
| `FinderViewModelTests` | Wpf.Tests / ViewModels | Unit | 4 | 0.06 | 0.05 |  |  |
| `RecentFilesMenuViewModelTests` | Wpf.Tests / ViewModels | **Unit-shaped, not isolated** | 6 | 0.03 | 0.02 | temp files (real store) | should use a fake `IRecentFilesStore` |
| `ErrorWindowTests` | Wpf.Tests / Views | Multi-unit | 3 | 0.16 | 0.15 | WPF/STA |  |
| `HexFormatTests` | Wpf.Tests / Views | Unit | 12 | 0.00 | 0.00 |  |  |

## Run time

| Class | Total | Why |
|---|---:|---|
| `OtherFactionsCastleTests` | 18.5 s | Every case loads two saves; `EveryBuildingBuiltHasItsPrerequisitesBuilt` (6.0 s) walks every save and castle. |
| `RoeCastleBuildingsTests` | 11.7 s | 146 cases, each loading a full save. |
| `CastleBuilderTests` | 10.4 s | Replays every Rampart, Bulwark and Cove castle, for a pure reducer. |
| `SafeCastleSerializerTests` | 9.1 s | Two tests serialize every castle of every sample save (4.7 s and 4.1 s). |
| `ObeliskVisitsTests` | 8.4 s | `EverySavefile`: 342 cases, one full load each. |
| `ShellSmokeTests` | 7.0 s | Real WPF windows; acceptable. |
| `PlayerHeadTests` | 5.9 s | `EverySavefile`: 342 cases. |
| `CastleEditingViewModelTests` | 5.5 s | One real-window test (5.3 s). |
| `SodSeriesTests` | 5.2 s | Reloads conflux saves per case; one test scans every Roe/Sod save (1.9 s). |
| `RoeCastleTests` | 4.3 s | Full loads per case. |
| `CastleBuilderRulesTests` | 3.8 s | One test removes more than 1,000 buildings across every sample (3.5 s). |

Flagged by the thresholds:

- **Integration tests over 2 s** (8 tests): `OtherFactionsCastleTests.EveryBuildingBuiltHasItsPrerequisitesBuilt` 6.0 s, `CastleEditingViewModelTests.TheCastlePageLoadsWithoutBindingErrorsAndEditsFlowThroughIt` 5.3 s, `SafeCastleSerializerTests.EveryCastleOfEverySampleSaveSerializesToTheBytesItWasReadFrom` 4.7 s and `.SerializingEveryCastleKeepsTheWholeSavefileIdentical` 4.1 s, `CastleBuilderRulesTests.RemovingABuildingGivesTheMasksOfNeverHavingBuiltIt` 3.5 s, `CastleBuilderTests` (all 4, 2.0–3.2 s), `ShellSmokeTests.HeroPagesLoadWithoutBindingErrors` 2.9 s.
  The two `CastleBuilder*` classes are reducer tests and should not need saves at all; the others are expected and can be reduced by caching the loaded saves.
- **Unit-shaped tests over 100 ms**, all caused by loading the Hota empire save per test (`HeroEditingTests.LoadPrepared()` in `[SetUp]`):
  `HeroEditingViewModelTests` 14 of 14, `SkillsAndSpellsViewModelTests` 16/16, `CreatureFinderAndArtifactEffectsTests` 11/11, `HeroGeneralTabTests` 9/9, `HeroEditingTests` 8/15, `SpellsFromArtifactsTests` 4/4, `ArtifactFinderTests` 3/8 (about 9 s in all).
  `CastleBuilderTests`, `CastleBuilderRulesTests` and `SafeCastleSerializerTests` are flagged above.
- **True unit tests**: none is over 100 ms except the first WPF test of `ErrorWindowTests` (0.15 s, starting the dispatcher) and the first test of `GzipSavefileCodecTests` (0.10 s, JIT); both are acceptable.
- **Binary (de)serializer tests on `.bin` dumps** (`SafeHeroSerializerTests`, `HeroDeserializerTests`) take about 20 ms in all; there is no need for them to be faster.

## Inconclusive and weak tests

| Where | Problem |
|---|---|
| `Data/ObeliskVisitsTests.cs:63-70` | `TheNumberOfObelisksIsTheNumberOfObeliskObjectsOfTheMap` calls `Assert.Pass` when a save has no obelisks: **48 of its 342 cases check nothing** and still report as passed. Filter the source to saves with obelisks, and add one test that a save without obelisks reads as none. |
| `Data/RoeBuildingEditsTests.cs:55-61` | `BuildingWhatTheGameBuiltGivesTheMasksTheGameWrote` skips (`continue`) each castle whose change is not exactly one building; only an aggregate count protects against checking nothing. Make the step list say which castle changed, or assert the count per step. |
| `Data/CastleBuilderRulesTests.cs:27-31`, `Data/CastleBuilderTests.cs:42-47, 72-76, 111-116` | Same `continue` pattern over every sample castle, protected only by `> 1000`, `> 60` and `> 200` totals. |
| `ViewModels/SkillsAndSpellsViewModelTests.cs:46` | Writes "unknown source" rows to `TestContext.Out` instead of asserting; an unknown source never fails the test. |
| `ViewModels/SkillsAndSpellsViewModelTests.cs:236-242` | `Assert.That(row, Is.Not.Null)` after `First()`, which can never fail. |
| `Data/SafeHeroSerializerTests.cs`, `Data/SafeCastleSerializerTests.cs` (most tests) | They check the serializer by deserializing what it wrote. A serializer and deserializer that are wrong in the same way still pass, and this breaks the rule in `CLAUDE.md` that the two are independent. Assert the bytes at the `HeroOffsets`/`CastleLayout` offsets instead. |
| `Data/SodSeriesTests.cs:176-207` | Skips heroes by owner and movement rules and only requires more than 30 to be left; it also uses raw offsets 26, 57 and 61 instead of `HeroOffsets`. |
| `Data/SodSaveTests.cs:179-181, 198-199, 231-232` | `Is.Not.Empty` on the bytes that changed; use the exact set of offsets. |
| `Data/HeroDeserializerTests.cs:28-52` | `Assert.That(() => local, Is.EqualTo(...))` lambdas around values that are already computed; harmless but hide the value. |
| `Data/RoeMapStateTests.cs:87-93, 118-123` | `.Last()` on regex matches and searches for a marker with no precondition, so a missing marker shows up as an unrelated exception. |
| `Data/HotaSleepGarrisonVisitingTests.cs:44-60` | The Oidana checks are skipped in the cases where she is `null`. The intent is clear, but those cases assert less than their names suggest. |
| `Entities/EntityInvariantTests.cs:33-54` | `Is.Not.Null` inside loops over every building; checks that the building exists, not that it is the right one. |
| `Data/CastleTests.cs:23-30`, `Data/HeroByteEditTargetTests.cs:16-30` | Assert only that names exist or that regions are not empty. |
| `Wpf.Tests/Views/ErrorWindowTests.cs:26-31` | Checks `ClipboardText` but not that the copy command uses it. |

No test uses `Assert.Inconclusive`, `Assume`, `[Ignore]` or `[Explicit]`, and no test swallows exceptions.

## Unit tests that are not isolated

These are named and laid out like unit tests of one class but load real saves or the whole graph.

- **Reducers tested through saves.** `CastleBuilderTests`, `CastleBuilderRulesTests` and `MageGuildSpellEditingTests` test `CastleBuilder` (a pure reducer in `Models`) by loading every sample castle through the real deserializer and serializer.
  Keep one replay test per series as integration, and test the rules on castle models built in the test.
- **Hero view models.** `HeroEditingViewModelTests`, `HeroGeneralTabTests`, `SkillsAndSpellsViewModelTests` and `CreatureFinderAndArtifactEffectsTests` build the whole save, history, session and `HeroViewModel` for every test.
  `TestServices.CreateHeroViewModel` already takes a session; give it a `HeroModel` built in the test and a fake (Moq) session.
- **Models.** `HeroEditingTests` and `SpellBookAndGrantsTests` mix pure reducer tests (`WithSkill`, `SpellBook`, grants) with save, serialize and reload tests.
- **Serializers.** `SafeHeroSerializerTests` and `SafeCastleSerializerTests` take models from the deserializer, and `TestServices.VersionEntities` instead of a fake entities provider.
- **Others.** `HeroByteEditTargetTests` and `HeroRegionProviderTests` load the full save in `[SetUp]`.
  `PlayersInTheGameTests` tests a pure filter but feeds it from `conflux_00`.
  `HeroEditSessionTests` uses the real `DocumentFactory` and a `.bin` dump.
  `RecentFilesMenuViewModelTests` (Wpf.Tests) uses the real `RecentFilesStore` and `SettingsStore` on a temp folder.
  `SavegameParserTests` calls the static `TestServices.GameService` and in fact tests `GameService`.
  `PlayerControlTests.UnknownVersionsAndBytesWithoutPlayersGiveNone` and the `ByteRegion` test in `SaveRegionProviderTests` are real unit tests inside integration classes.

## Placement and naming

- **Wrong folder for the subject's layer**: `CastleBuilderTests` and `CastleBuilderRulesTests` (`CastleBuilder` is in `Models`), `GameDateTests` (`GameDate` is in `Models`; the test is about `GameDateLocator` and `OverviewViewModel`), and `SaveBaselineTests` (in `Models`, but tests history, view model and hex target together).
- **View-model tests inside Data classes**: `ObeliskVisitsTests:124-179` (`PlayerViewModel`, `OverviewViewModel`), `PlayerHeadTests:105-123` (`PlayerViewModel` taverns), `CoveCastleTests:202-224` (`MainViewModel`), `RoeHeroTests:165-183` and `VisitedBuildingEffectsTests:76-81` (`HeroViewModel`).
- **One class, several subjects** (against "one test class per subject"): `SaveDocumentTests` (with `SaveDocumentHistory`), `SettingsStoreTests` (also `RecentFilesStore`), `EntityInvariantTests`, `CollectionAccessTests`, `PlayerAndCastleRegionProviderTests`, `CreatureFinderAndArtifactEffectsTests`, `PlayerAndOverviewViewModelTests`, `SkillsAndSpellsViewModelTests`, `CastlePillAndTavernLinkTests`, and the session tests in `SafeCastleSerializerTests:134-164`.
  Scenario classes (`Roe*`, `Sod*`, `Hota*`, `*CastleTests` per faction) are allowed by `CLAUDE.md` and are fine.
- **Names that are not behaviour sentences**: `HeroDeserializerTests` (`DeserializingHeroDeserializes`, `DeserializingHasSpellBookForHeroWithDeserializes`), `HeroEditSessionTests` (`UpdateWritesAndUndoRestores`), `HeroOffsetsTests` (`SkillSlotsDependOnVersion`), `GrantedSpellsResolverTests` (`OneVersions…`), `SkillsAndSpellsViewModelTests.PickingSkillsAndLevelsInSlots`.
  See [Naming consistency](#naming-consistency) for the full list.
- **Projects**: every test is in the project of the class it tests.
  `Siniskalc.Core.Tests` and `Siniskalc.Wpf.Tests` do not name any HoMM3 type.
  `Tests/Siniskalc.Tests/Models/` is listed as an empty `<Folder>` in the csproj although it holds tests; remove that item.

### Naming consistency

`CLAUDE.md` sets the format: a test class is `<Subject>Tests` in a file of the same name, and a scenario, save or game version may be the subject instead of a type.
A method is a PascalCase sentence that states the behaviour, with no underscores, no `Test` or `Should` prefix, not just the name of a hero, and the subject named as the code names it.
Savefile names follow `TestData/README.md` and are checked by `TestDataNamesTests`.

What holds everywhere (checked by script over all 636 methods and 93 classes):

- Every file holds one test class named like the file, and every namespace follows its folder.
- No method has an underscore, a `Test` or `Should` prefix, or a lower-case start.
- The two `partial` test classes are partial only for `[GeneratedRegex]`, in one file.
- Helpers (`TestServices`, `CastleSamples`, `CastleMasks`, `SeriesSaves`, …) are in their own files, as the rules ask.

Where names drift:

- **Class names that are neither a type nor a scenario.**
  43 of 93 classes match a production type.
  The `Roe*`, `Sod*`, `Hota*` and faction `*CastleTests` classes are scenarios, which is allowed.
  A third kind is not covered by the rules: classes named after a **feature** that cuts across reader, writer and view model (`GenderTests`, `BiographyTests`, `ActiveSpellsTests`, `MovedTodayTests`, `MapDifficultyTests`, `PlayerControlTests`, `PlayerHeadTests`, `MageGuildSpellsTests`, `MageGuildSpellEditingTests`, `ObeliskVisitsTests`, `CastleEditingTests`, `HeroEditingTests`, `SaveBaselineTests`, `HeroGeneralTabTests`, `SpellsFromArtifactsTests`, `CastleGarrisonedHeroTests`).
  `CastleTests` and `PlayerTests` look like subjects but there is no `Castle` or `Player` type; they test `CastleModel`/`PlayerModel` read from a save.
  Classes joined with `And` name two subjects (listed above).
- **Method names that say nothing about the behaviour** (tautology, a hero name, or "works"):
  `HeroDeserializerTests.DeserializingHeroDeserializes`, `…FullMaximusDumpDeserializesHero`, `…HasSpellBookForHeroWithDeserializes`, `…WithoutDeserializes`;
  `HotaSaveTests.EveryHeroDeserializes`, `SirMullichIsDeserialized`, `MaximusIsDeserialized`, `ChristianIsDeserialized`;
  `HotaSaveTests` and `SodSaveTests.UnchangedGameSavesIdenticalBytes` (no verb);
  `ArtifactTests.GettingComboArtifactsReturnsCombo` (method-and-result style);
  `CreatureInfoTests.EveryCreatureHasStats`, `ResourceCostIsImported`, `ShotsAreImported`;
  `HeroOffsetsTests.SkillSlotsDependOnVersion`, `FifthSideSlotAndBackpackDependOnVersion` (depend how?);
  `GrantedSpellsResolverTests.OneVersionsResolversAreEqualAndResolveThatVersionsSpells`;
  `HeroByteEditTargetTests.ExposesTitleBytesAndRegions`;
  `PlayerAndOverviewViewModelTests.QuickSetCommandsAndResetPlayerWork`;
  `SkillsAndSpellsViewModelTests.PickingSkillsAndLevelsInSlots`.
- **Two sentence styles.**
  Most of the suite (the savefile, castle, view-model and Core/Wpf tests) writes full sentences with articles: `ANeutralCastleIsWrittenAsTheByteFF`.
  The older hero classes write telegraphic ones without articles: `SingleByteEditCanBeUndoneAndRedone`, `RoundTripKeepsBytesIdentical`, `ArtifactAddedToBackpackHasBlankSpellId`, `EquippingIntoBlankSlotWritesAValidArtifactSlot`, `DisposedTargetStopsListening`, `CreaturesSwapMovesStacksAndIntoEmptySlots`, `SelectingCombinationArtifactMirrorsBlockedSlots`, `FinderListsEveryCreatureUntilNarrowed` (in `HeroByteEditTargetTests`, `HeroEditSessionTests`, `SavefileBytesPatcherTests`, `SafeHeroSerializerTests`, `HeroEditingTests`, `EquippedArtifactsViewModelTests`, `CreatureFinderAndArtifactEffectsTests`).
  Both pass the rule; the full-sentence style is the one to keep.
- **Names that start with the member under test** (`ToBytesAppliesChunkEdits`, `ApplyChangesToKeepsEditsMadeElsewhere`, `ThrowIfNotIndexRejects…`, `WithGrantsReturnsTheSameBook…`, `HistoryOriginalRestoresEverything`).
  The rule allows this ("name the thing under test the way the code does"); it reads well when the member is public API and badly when it is a model field (`HistoryOriginal`).

Proposed format, so the rules decide every case (adopted in `CLAUDE.md`):

| Kind | Class | Example |
|---|---|---|
| Unit, multi-unit | `<ProductionType>Tests`, one type | `SafeCastleSerializerTests` |
| Integration, one save or series | `<Version><Scenario>Tests` | `RoeCastleBuildingsTests`, `SodHotseatTests` |
| Integration, one feature over every layer | `<Feature>Tests` in `Integration/` | `GenderTests`, `ObeliskVisitsTests` |

Methods: a present-tense sentence with articles, subject first, then the observable result (`AnUnknownClassReadsAsKnightAndIsKept`).
Name a member only when it is the public API under test.
Name a save, hero or castle only together with what it shows (`TazarStartsWithEquestriansGlovesAndNoSpellBook`, not `TazarIsDeserialized`).

## Excessive coverage

- **Castle building progression** ("save B adds building X") is asserted in `RoeCastleTests`, `RoeCastleBuildingsTests`, `RoeBuildingEditsTests`, `RampartCastleTests`, `RampartAndBulwarkCastleTests`, `OtherFactionsCastleTests`, `FactoryCastleTests`, `CoveCastleTests`, and again by the `CastleBuilder*` replays (about 400 cases, about 55 s).
  The per-faction series are worth keeping: each pins a faction's bytes.
  Remove the duplicate generic checks: `RampartCastleTests` duplicates `RampartAndBulwarkCastleTests`, and the `CastleBuilder*` replays duplicate `RoeBuildingEditsTests`.
- **Castle serialization round trips**: `SafeCastleSerializerTests` (every sample), `RoeCastleBuildingsTests` (every save from 12), `RoeCastleTests` (every save), `MageGuildSpellsTests` (every castle of a save). Keep one test over all saves.
- **Hero serialization round trips**: `SafeHeroSerializerTests`, `SodSaveTests.EveryHeroDeserializes…`, `SpellBookAndGrantsTests.EveryHeroOfTheArtsSaves…`.
- **Mage guild**: `MageGuildSpellsTests`, `RoeCastleBuildingsTests:182-309`, `MageGuildSpellEditingTests`, `CastleBuilderRulesTests:103-132`, `CastleEditingTests:141-172`, `CoveCastleTests:91-105`.
- **Players**: `PlayerTests`, `PlayerHeadTests`, `PlayersInTheGameTests`, `PlayerControlTests`, `HotaFourCornersTests`, and parts of the Roe/Hota scenarios.
- **Hero sleep, moved-today and movement**: `RoeHeroTests`, `MovedTodayTests`, `HotaSleepGarrisonVisitingTests`, `RoeSanyaMovementTests`, `RoeMapStateTests`.
- **Visited-building effects**: `SodHotseatTests:42-89` (bytes) and `VisitedBuildingEffectsTests:26-59` (model), on the same saves. Merge them into one test that checks both.
- **Every-save sources** (`ObeliskVisitsTests`, `PlayerHeadTests`): 684 cases. They are worth having as data contracts; cache the loaded games, or check all saves in one test per rule.
- **Same prepared hero save**: loaded in each of about 70 tests across 7 view-model and model classes.
- `ShellSmokeTests` repeats view-model state checks that the view-model tests already make; keep it to binding errors and one interaction per page.

## Gaps

**Data classes without a test class of their own:**

| Class | Covered |
|---|---|
| `SafePlayerSerializer`, `SafeObeliskSerializer` | only through save/edit scenarios; no unknown-byte or exact-offset tests |
| `PlayerDeserializer`, `ObeliskDeserializer`, `CastleDeserializer` | only indirectly, from saves |
| `PlayerDiffer`, `ObeliskDiffer` | only through history |
| `CastleBytesLoader`, `ObeliskBytesLoader` | not directly |
| `HeroBytesLoader`, `PlayerBytesLoader` | a few lines in `HotaSaveTests` and `PlayerTests` |
| `CastleByteEditTarget`, `PlayerByteEditTarget` | barely (`PlayerAndCastleRegionProviderTests`) |
| `Homm3EditSessions`, `MapObjectPositions`, `Homm3PendingChanges` | not directly |
| `SavegameVersionDetector`, `SaveDocumentFactory`, `VersionSpecificEntitiesProvider(Factory)` | only through every load |
| `CastleEditSession`, `PlayerEditSession`, `ObeliskEditSession` | through scenarios and `SafeCastleSerializerTests` |

**Rules from `CLAUDE.md` with little direct coverage:**

- *Unknown value reads as a stand-in*: covered for hero class and facing (`SafeHeroSerializerTests`) and castle owner, built flag and creature (`SafeCastleSerializerTests`).
  Not covered for players, obelisks or castle faction, and not for unknown creatures and artifacts in a hero's army or slots.
- *The serializer leaves a byte alone while the model holds the stand-in*: same coverage as above.
- *Malformed input*: truncated chunks, too-short heroes, and markers that are missing or not where expected are hardly tested; `SavegameParserTests` only checks random and empty bytes.

**Thin unit coverage:**

- `SpellPointsCalculator` and `GrantedSpellsResolver` (one test each), `LevelCalculator` (only a table check in `ConstantsTests`), and `HeroDiffer`, `ObeliskVisits`, `TavernHeroes`, `PlayerResources` (only through savefile tests).
- View models with no test class of their own: `NavigationTree`, `NavNodeViewModel`, `ArmyViewModel`, `BackpackViewModel`, `ObeliskVisitsViewModel`, `TavernPlaceViewModel`, `OverviewViewModel`, `PlayerViewModel`, `SpellFinderViewModel`, `CreatureFinderViewModel`, the castle row view models.
  Providers with no tests of their own: `CastleBuildMessageProvider`, `EffectLineProvider`, `PlayerPalette`, `PlayerTypeText`, `SkillLevelNames`.
- `Siniskalc.Wpf`: `DelegateCommand`, `AbstractViewModelBase`/`PageViewModelBase` (property-changed behaviour), `HexRowList`, `HexPalette`, `RegionNode`.
- `Siniskalc.Core`: `ByteRegionMap`, `ByteBufferHelper`, `ChunkCollection`, `ChunkByteEditTarget`, `SaveHexDump`, `EditSession`, `AbstractEnumeration` have no tests of their own (some are covered from `Siniskalc.Tests`).

## Proposed conventions

1. **Mark each test's kind twice, with a category and a folder.**
   - `[Category("Unit")]`, `[Category("MultiUnit")]` or `[Category("Integration")]` on every fixture; use one `const` class (`TestKinds`) per test project so the names cannot drift.
   - Folders: unit tests keep the layer folders (`Data`, `Models`, `ViewModels`, …).
     Multi-unit and integration tests go to `MultiUnit/<Layer>/` and `Integration/<Layer>/` (scenario classes such as `RoeCastleTests` to `Integration/Scenarios/`).
   - `dotnet test --filter TestCategory=Unit` then runs in about a second, and CI can run it first.
   - A test that checks the rule (like `TestDataNamesTests`) can fail any fixture that has no category, or whose category does not match its folder.
2. **Use Moq for dependencies in unit tests.** Add `Moq` to the three test projects.
   Fake interfaces (`ISaveRegionProvider`, `IRecentFilesStore`, `IHeroDeserializer`, `ISavegameParser`, `IGrantedSpellsResolver`, …) instead of reaching into `TestServices`.
   `TestServices` stays for multi-unit and integration tests only.
3. **Load each save once**: cache loaded games in a shared `internal static` helper (as `HotaSaveTests` does with `OneTimeSetUp`); savefile tests only read, so they can share one load.
4. **No `Assert.Pass` and no silent `continue`** in tests; filter the test case source instead.

## Backlog (in order)

1. Remove the `Assert.Pass` in `ObeliskVisitsTests` and the `continue` loops in `RoeBuildingEditsTests` and `CastleBuilder*Tests`; turn the `TestContext.Out` line in `SkillsAndSpellsViewModelTests` into an assertion.
2. Add `TestKinds` with `[Category]` on every fixture, and the check test; move classes into `Integration/` and `MultiUnit/` folders.
3. Add Moq.
   Rewrite `SafeHeroSerializerTests` and `SafeCastleSerializerTests` to assert bytes at offsets, without the deserializer; move the session tests out of `SafeCastleSerializerTests`.
4. Split `CastleBuilderTests` and `CastleBuilderRulesTests` into `Models/CastleBuilderTests` (castle models built in the test) and one integration replay per series; this saves about 14 s.
5. Give the hero view-model tests a `HeroModel` built in the test and a mocked session; keep `SpellsFromArtifactsTests` and two or three save-based tests as integration (saves about 9 s).
6. Cache loaded saves for the every-save and series sources (`ObeliskVisitsTests`, `PlayerHeadTests`, `OtherFactionsCastleTests`, `RoeCastleBuildingsTests`, `SodSeriesTests`).
7. Split the multi-subject classes and move view-model tests out of Data classes; rename the non-sentence test methods listed under [Naming consistency](#naming-consistency) and rewrite the telegraphic hero-class names as full sentences; move `GameDateTests` and `SaveBaselineTests` to where their subjects are.
8. Add tests for the gaps: `SafePlayerSerializer`, `SafeObeliskSerializer`, `PlayerDeserializer`, `ObeliskDeserializer`, `PlayerDiffer`, the loaders, the byte edit targets, and the unknown-byte rules for players, obelisks and castle factions; then the view models and providers without tests.
9. Remove the duplicated coverage listed above once the unit tests that replace it are in place.
