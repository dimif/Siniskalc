# Test review

Reviewed on `feature/homm2-support` at dc14cef, which is the base of all current work.
An earlier review on `main` (c352615) covered 1,911 tests in three projects; this one replaces it.
The rules for kinds, folders and names that this review checks against are in the "Tests" section of `CLAUDE.md`.
The review is repeatable with the `test-reviewer` agent (`.github/agents/test-reviewer.agent.md`, `.claude/agents/test-reviewer.md`).

## Incremental fixes after the review

The class table and original timings below remain the historical review snapshot, not a fresh measurement after each slice.
Line references refer to that snapshot and may move as tests change.

### Slice 01: assertion-free diagnostics and unchecked spell sources

- Removed `TmpTurnDump`, an assertion-free diagnostic that wrote to a user-specific path.
- Replaced the obelisk `Assert.Pass` with assertions that an absent editable block has no model and that serializing the untouched document preserves all save bytes.
  All 448 save cases remain; cases with an editable block retain their map-object count and index checks.
  The existing named-save tests still require known obelisk blocks to be found, so this is not a replacement for presence coverage.
- Replaced the spell-source diagnostic output with an assertion that no displayed spell reports `Unknown source`.
- Targeted validation: 481 passed, zero failed or skipped, including all 448 save cases.
- HoMM2 project validation after removing the diagnostic: 389 passed, zero failed; three explicit manual save-writing helpers were not executed.
- Subsequent slices run tests only for affected projects, as directed by the user.
  The full-solution run started for this slice was stopped on that instruction and is not reported as completed validation.

### Slice 02: silent skips and conditional assertions

- `CastleBuilderTests` now compares masks for every eligible transition, including no-building steps and transitions with multiple additions.
  The first strengthened run exposed two additions in `bulwark_34_spirit_guardian__nothing.GM1` for Still Water, which the old single-addition condition skipped.
  Replaying additions in upgrade-code order and removing them in reverse order checks this transition in both directions.
- The two full-castle replays explicitly select castles of verified factions and require at least one such castle in every selected save.
- `RoeBuildingEditsTests` now asserts unchanged masks for castles with no additions and fails on unexpected multiple manual additions.
  Every named castle is checked, replacing the aggregate counter that allowed a case to check no castles.
- `SodSeriesTests` explicitly selects owned heroes with meaningful movement data, requires an eligible sample in every save, and requires both moved and unmoved heroes across the sample.
- `HeroOwnersBytesLoaderTests` requires players in every save and always checks unlisted owned heroes against castle garrisons.
- The nullable Oidana expectations became 13 explicit save/hero/on-map cases, retaining all eight Akka checks and all five Oidana checks with an explicit hero-presence assertion.
- Validation ran only the six affected HoMM3 fixtures: 655 passed, zero failed, skipped or unexecuted, in about 2 minutes 5 seconds.
  The 12 additional discovered cases come from splitting one aggregate hero test into 13 cases, not from adding new savefiles.
  These targeted measurements are not a like-for-like full-suite performance comparison.

| Affected fixture | Passed | Sum of test durations (s) |
|---|---:|---:|
| `CastleBuilderTests` | 4 | 41.22 |
| `CastleBuilderRulesTests` | 13 | 11.91 |
| `RoeBuildingEditsTests` | 62 | 8.12 |
| `SodSeriesTests` | 92 | 25.87 |
| `HeroOwnersBytesLoaderTests` | 454 | 36.51 |
| `HotaSleepGarrisonVisitingTests` | 30 | 2.11 |

### Slice 03a: Core filesystem fixture classification

Core's 20 fixtures are being migrated in three bounded commits (03a runtime, 03b Data, 03c remaining fixtures).
Only 03a is complete at this checkpoint.

- Moved eight fixtures to `Tests/Siniskalc.Core.Tests/Integration/Runtime/` and updated their namespaces to match.
  Each fixture now has `[Category(TestKinds.Integration)]`, using the project-local `TestKinds` constants.
- Fixtures: `SettingsStoreTests`, `RecentFilesStoreTests`, `RawSavefileCodecTests`, `NumberedSavefileBackupTests`, `LogPathsTests`, `FileBackupServiceProviderTests`, `DisabledFileBackupServiceTests` and `ShellUrlLauncherTests`.
- Classification reflects current behaviour: all eight fixtures use real filesystem operations, including setup or a subset of cases.
  `ShellUrlLauncherTests` injects the shell callback but creates real directories for folder-launch tests; `LogPathsTests` includes real write probes.
  These mixed fixtures remain Integration as a whole; separating pure cases or isolating collaborators is later work, not part of this placement-only commit.
- All eight fixture bodies and method names are unchanged.
  Comparing discovered identities before and after the move, normalizing the intentional namespace change, retained all 79 Core cases.
- Validation ran only Core: 79 passed before and after the move; the Integration category filter passed all 42 migrated cases.
  There were zero failures or skipped cases.
  Reported test durations were 164 ms for the full Core run and 121 ms for the category-filtered run; this slice makes no performance improvement claim.
- Twelve Core fixtures remain uncategorized pending 03b and 03c; cross-project category/folder enforcement remains deferred until migration is complete.

## Summary

- **4,000 tests in 263 classes and 7 projects, all passing.**
  The sum of test durations is 804 s; `Siniskalc.Homm3.Tests` alone takes 775 s (about 13 minutes) and every other project together under 30 s.
- **Run time is driven by savefile replays, not by units.**
  The four `CastleBuilderTests` take 132 s, and the every-save sources (`ObeliskVisitsTests`, `PlayerHeadTests`, `HeroOwnersBytesLoaderTests`, `SafeHeroOwnersSerializerTests`, 1,831 cases) another 148 s.
  `TestData` grew to 448 saves, and every test that walks all saves grows with it.
- **At review time no test had a `[Category]`, and no project had `Integration/` or `MultiUnit/` folders.**
  Slice 03a has since categorized eight Core fixtures (42 cases) as Integration and moved them into `Integration/Runtime/`; the rest of the migration remains open.
  By what they read, 92 classes are unit tests, 19 multi-unit, 94 integration, and 58 are named after one production type (unit-shaped) but load a savefile through `TestServices` or the samples.
- **The reported assertion-free and skipped-check findings are addressed in slices 01 and 02.**
  Weak value assertions and the serializer assertion issues below remain open.
- **Four serializer tests check their output through the deserializer**, which `CLAUDE.md` forbids for the production code and which hides symmetric bugs in tests.
- **Gaps**: most HoMM2 deserializers and serializers, several HoMM3 loaders and differs, and many Core, Wpf and Heroes.Wpf types have no test class of their own; the unknown-byte rule is not tested for HoMM3 players and obelisks or any HoMM2 record except heroes.
- **Every test is in the project of the class it tests**, and no shared project names a game type.

### Changes since the `main` review

- Moq is now referenced by five test projects and used in 27 files, mostly in the Heroes.Wpf view-model tests.
- `[Category]` was used nowhere at review time; the Core migration started in slice 03a.
- The suite grew from 1,911 tests in 2 min 22 s to 4,000 tests in about 13 min; the growth is almost all in HoMM3 savefile tests.
- At review time the `Assert.Pass`, the `continue` loops, the `TestContext.Out` line and the serializer-through-deserializer tests reported then were all still there.
  Slice 01 subsequently addressed the `Assert.Pass` and the `TestContext.Out` line.
  Slice 02 addressed the six reported `continue` sites and the two reported conditional checks.

## How the review was done

1. `dotnet test Siniskalc.sln --logger trx` with the artifacts outside the repo; `Siniskalc.Homm3.Tests` again alone with `--blame-hang-timeout 10m`, because its host prints nothing for minutes while one test loads all 448 saves.
2. Per-test durations parsed from the TRX files and summed per class.
3. Every test file read by one reviewer per area (Core, Wpf and the app; Heroes; Heroes.Wpf; Homm2; Homm3 Data A–L; Homm3 Data M–Z; Homm3 other layers), and every finding below checked at its line.
4. Each class classified by script: a class that reads `TestServices`, `Saves`, `CastleSamples`, `SeriesSaves`, `TestData` or the disk is integration (or unit-shaped, when it is named after one production type); a class that opens a WPF window or composes several real collaborators is multi-unit; the rest are unit tests.

Thresholds: a unit or multi-unit test should take under 100 ms, and an integration test under 2 s.

## Every test class

Kind is what the class does, not where it is.
**Unit-shaped** marks a class named after one production type that nevertheless loads a savefile; it should become a unit test with fakes, or move to `Integration/` and be renamed.

| Class | Project / folder | Kind | Tests | Total s | Max s |
|---|---|---|---:|---:|---:|
| `RangeGuardTests` | Core / (root) | Unit | 4 | 0.00 | 0.00 |
| `ByteEnumTests` | Core / Data | Unit | 3 | 0.01 | 0.00 |
| `ChangeSinkTests` | Core / Data | Unit | 2 | 0.00 | 0.00 |
| `ChunkCollectionTests` | Core / Data | Unit | 2 | 0.00 | 0.00 |
| `ChunkPendingChangeTests` | Core / Data | MultiUnit | 3 | 0.00 | 0.00 |
| `RecordChunkCollectionTests` | Core / Data | MultiUnit | 4 | 0.01 | 0.00 |
| `RecordChunkTests` | Core / Data | Unit | 2 | 0.00 | 0.00 |
| `SaveDocumentTests` | Core / Data | MultiUnit | 7 | 0.01 | 0.00 |
| `SavefileBytesPatcherTests` | Core / Data | Unit | 3 | 0.00 | 0.00 |
| `VersionedEnumerationTests` | Core / Entities | Unit | 2 | 0.00 | 0.00 |
| `NumbersTests` | Core / Models | Unit | 2 | 0.00 | 0.00 |
| `DisabledFileBackupServiceTests` | Core / Runtime | Integration | 2 | 0.00 | 0.00 |
| `FileBackupServiceProviderTests` | Core / Runtime | Integration | 3 | 0.02 | 0.02 |
| `LogFactoryTests` | Core / Runtime | Unit | 3 | 0.00 | 0.00 |
| `LogPathsTests` | Core / Runtime | Integration | 8 | 0.00 | 0.00 |
| `NumberedSavefileBackupTests` | Core / Runtime | Integration | 5 | 0.02 | 0.01 |
| `RawSavefileCodecTests` | Core / Runtime | Integration | 3 | 0.03 | 0.02 |
| `RecentFilesStoreTests` | Core / Runtime | Integration | 5 | 0.05 | 0.02 |
| `SettingsStoreTests` | Core / Runtime | Integration | 8 | 0.06 | 0.01 |
| `ShellUrlLauncherTests` | Core / Runtime | Integration | 8 | 0.00 | 0.00 |
| `AboutViewModelTests` | Wpf / ViewModels | Unit | 2 | 0.25 | 0.25 |
| `FinderViewModelTests` | Wpf / ViewModels | Unit | 4 | 0.06 | 0.05 |
| `LoggingSettingsViewModelTests` | Wpf / ViewModels | Integration | 9 | 0.05 | 0.03 |
| `RecentFilesMenuViewModelTests` | Wpf / ViewModels | Integration | 6 | 0.01 | 0.00 |
| `ShellViewModelTests` | Wpf / ViewModels | Integration | 17 | 0.04 | 0.01 |
| `SortableListViewModelTests` | Wpf / ViewModels | Unit | 3 | 0.01 | 0.00 |
| `ErrorWindowTests` | Wpf / Views | Unit | 3 | 0.17 | 0.16 |
| `HexFormatTests` | Wpf / Views | Unit | 12 | 0.00 | 0.00 |
| `LoggingSettingsWindowTests` | Wpf / Views | Integration | 1 | 0.23 | 0.23 |
| `ShellWindowTests` | Wpf / Views | Integration | 1 | 0.24 | 0.24 |
| `HeroesLayeringTests` | Heroes / (root) | Unit | 1 | 0.00 | 0.00 |
| `ArmyReaderTests` | Heroes / Data | Unit | 4 | 0.02 | 0.02 |
| `ArmyWriterTests` | Heroes / Data | Unit | 6 | 0.01 | 0.01 |
| `ChangeSinkExtensionsTests` | Heroes / Data | Unit | 18 | 0.02 | 0.00 |
| `ObeliskChunkCollectionTests` | Heroes / Data | Unit | 4 | 0.16 | 0.14 |
| `ObeliskDifferTests` | Heroes / Data | Unit | 4 | 0.02 | 0.01 |
| `ResourceTests` | Heroes / Entities | Unit | 1 | 0.00 | 0.00 |
| `SkillLevelTests` | Heroes / Entities | Unit | 2 | 0.00 | 0.00 |
| `ArmyTests` | Heroes / Models | Unit | 7 | 0.00 | 0.00 |
| `BanishablePlayerBaseTests` | Heroes / Models | Unit | 10 | 0.00 | 0.00 |
| `ChangeAreasTests` | Heroes / Models | Unit | 1 | 0.01 | 0.01 |
| `ExperienceTableTests` | Heroes / Models | Unit | 10 | 0.00 | 0.00 |
| `GameDateTests` | Heroes / Models | Unit | 2 | 0.00 | 0.00 |
| `GameRuleExceptionTests` | Heroes / Models | Unit | 1 | 0.00 | 0.00 |
| `GameSetupTests` | Heroes / Models | Unit | 2 | 0.00 | 0.00 |
| `HeroLevelTests` | Heroes / Models | Unit | 2 | 0.00 | 0.00 |
| `HeroPositionTests` | Heroes / Models | Unit | 3 | 0.00 | 0.00 |
| `HeroSummaryTests` | Heroes / Models | Unit | 1 | 0.00 | 0.00 |
| `MapPositionTests` | Heroes / Models | Unit | 3 | 0.00 | 0.00 |
| `MovementPointsTests` | Heroes / Models | Unit | 1 | 0.00 | 0.00 |
| `ObeliskVisitsTests` | Heroes / Models | Unit | 8 | 0.00 | 0.00 |
| `OwnerNamesTests` | Heroes / Models | Unit | 3 | 0.00 | 0.00 |
| `PlayerModelExtensionsTests` | Heroes / Models | Unit | 4 | 0.00 | 0.00 |
| `PrimarySkillsTests` | Heroes / Models | Unit | 8 | 0.00 | 0.00 |
| `ResourceSetTests` | Heroes / Models | Unit | 2 | 0.00 | 0.00 |
| `SpellSetTests` | Heroes / Models | Unit | 3 | 0.01 | 0.00 |
| `TavernHeroesTests` | Heroes / Models | Unit | 2 | 0.00 | 0.00 |
| `HeroesSavegameBaseTests` | Heroes / Runtime | MultiUnit | 2 | 0.00 | 0.00 |
| `CreatureChoiceViewModelTests` | Heroes / ViewModels | Unit | 4 | 0.01 | 0.01 |
| `MapLevelTextTests` | Heroes / ViewModels | Unit | 2 | 0.00 | 0.00 |
| `OverviewTextTests` | Heroes / ViewModels | Unit | 2 | 0.00 | 0.00 |
| `PlayerPaletteTests` | Heroes / ViewModels | Unit | 3 | 0.00 | 0.00 |
| `PlayerTypeTextTests` | Heroes / ViewModels | Unit | 2 | 0.01 | 0.00 |
| `PrimarySkillBonusTextTests` | Heroes / ViewModels | Unit | 2 | 0.00 | 0.00 |
| `TurnTextTests` | Heroes / ViewModels | Unit | 2 | 0.00 | 0.00 |
| `PageStylesTests` | Heroes.Wpf / Styles | MultiUnit | 11 | 0.67 | 0.67 |
| `ArmyTotalsTests` | Heroes.Wpf / ViewModels | Unit | 3 | 0.01 | 0.01 |
| `BanishingTextProviderTests` | Heroes.Wpf / ViewModels | Unit | 4 | 0.01 | 0.01 |
| `CastleHeroRowsTests` | Heroes.Wpf / ViewModels | Unit | 5 | 0.36 | 0.35 |
| `CastleLinkViewModelTests` | Heroes.Wpf / ViewModels | Unit | 3 | 0.01 | 0.01 |
| `DaysLeftCardViewModelTests` | Heroes.Wpf / ViewModels | Unit | 6 | 0.02 | 0.01 |
| `HeroesArmyViewModelBaseTests` | Heroes.Wpf / ViewModels | Unit | 7 | 0.05 | 0.02 |
| `LevelAndClassCardViewModelTests` | Heroes.Wpf / ViewModels | Unit | 4 | 0.00 | 0.00 |
| `MapPointCardViewModelTests` | Heroes.Wpf / ViewModels | Unit | 4 | 0.00 | 0.00 |
| `MineRowViewModelBaseTests` | Heroes.Wpf / ViewModels | Unit | 3 | 0.00 | 0.00 |
| `MineRowViewModelTests` | Heroes.Wpf / ViewModels | Unit | 3 | 0.02 | 0.02 |
| `MinesListViewModelTests` | Heroes.Wpf / ViewModels | Unit | 7 | 0.04 | 0.03 |
| `MovementAndSpellPointsCardViewModelTests` | Heroes.Wpf / ViewModels | Unit | 4 | 0.00 | 0.00 |
| `ObeliskVisitsViewModelTests` | Heroes.Wpf / ViewModels | MultiUnit | 5 | 0.04 | 0.02 |
| `OverviewViewModelBaseTests` | Heroes.Wpf / ViewModels | Unit | 7 | 0.07 | 0.05 |
| `OwnedLinksTests` | Heroes.Wpf / ViewModels | Unit | 5 | 0.20 | 0.14 |
| `OwnerNavGroupTests` | Heroes.Wpf / ViewModels | Unit | 7 | 0.12 | 0.04 |
| `PrimarySkillsCardViewModelTests` | Heroes.Wpf / ViewModels | Unit | 5 | 0.02 | 0.01 |
| `RecordsPageViewModelBaseTests` | Heroes.Wpf / ViewModels | MultiUnit | 2 | 0.06 | 0.05 |
| `ResourcesEditorViewModelTests` | Heroes.Wpf / ViewModels | Unit | 6 | 0.07 | 0.03 |
| `SessionPageViewModelBaseTests` | Heroes.Wpf / ViewModels | MultiUnit | 2 | 0.03 | 0.03 |
| `TavernChoicesTests` | Heroes.Wpf / ViewModels | Unit | 4 | 0.03 | 0.02 |
| `TavernPlaceViewModelTests` | Heroes.Wpf / ViewModels | Unit | 5 | 0.02 | 0.01 |
| `VisitedPlacesCardViewModelTests` | Heroes.Wpf / ViewModels | Unit | 2 | 0.01 | 0.01 |
| `WeeklyGrowthTextTests` | Heroes.Wpf / ViewModels | Unit | 2 | 0.00 | 0.00 |
| `DaysLeftCardTests` | Heroes.Wpf / Views | MultiUnit | 3 | 0.43 | 0.20 |
| `HeadedCardTests` | Heroes.Wpf / Views | MultiUnit | 1 | 0.12 | 0.12 |
| `HeroCardsTests` | Heroes.Wpf / Views | MultiUnit | 5 | 0.71 | 0.18 |
| `ItemsCardTests` | Heroes.Wpf / Views | MultiUnit | 2 | 0.23 | 0.12 |
| `MinesCardTests` | Heroes.Wpf / Views | MultiUnit | 4 | 0.91 | 0.30 |
| `ObeliskVisitsCardTests` | Heroes.Wpf / Views | MultiUnit | 3 | 0.24 | 0.23 |
| `OwnerPickerTests` | Heroes.Wpf / Views | MultiUnit | 2 | 0.33 | 0.17 |
| `PageContractTests` | Heroes.Wpf / Views | MultiUnit | 3 | 0.66 | 0.24 |
| `PageHeaderTests` | Heroes.Wpf / Views | MultiUnit | 2 | 0.20 | 0.10 |
| `ResourcesPanelTests` | Heroes.Wpf / Views | MultiUnit | 1 | 0.13 | 0.13 |
| `Homm2LayeringTests` | Homm2 / (root) | Unit | 2 | 0.00 | 0.00 |
| `ShellTests` | Homm2 / (root) | Integration | 6 | 3.95 | 3.86 |
| `BuiltTodayTests` | Homm2 / Data | Integration | 7 | 0.34 | 0.20 |
| `ByteEditTargetFactoryTests` | Homm2 / Data | **Unit-shaped** | 4 | 0.03 | 0.02 |
| `CastleDifferTests` | Homm2 / Data | **Unit-shaped** | 5 | 0.09 | 0.06 |
| `CastleTests` | Homm2 / Data | **Unit-shaped** | 8 | 0.11 | 0.02 |
| `EditedSavesTests` | Homm2 / Data | Integration | 7 | 0.06 | 0.03 |
| `EditingTests` | Homm2 / Data | Integration | 9 | 0.03 | 0.01 |
| `EditorSavesTests` | Homm2 / Data | Integration | 7 | 0.44 | 0.39 |
| `HeaderReaderTests` | Homm2 / Data | **Unit-shaped** | 21 | 0.33 | 0.06 |
| `HeroChunkCollectionTests` | Homm2 / Data | **Unit-shaped** | 2 | 0.00 | 0.00 |
| `HeroDestinationAndFacingTests` | Homm2 / Data | Integration | 14 | 2.86 | 2.76 |
| `HeroDifferTests` | Homm2 / Data | **Unit-shaped** | 4 | 0.13 | 0.12 |
| `HeroTests` | Homm2 / Data | **Unit-shaped** | 13 | 0.03 | 0.00 |
| `MapObjectsTests` | Homm2 / Data | Integration | 12 | 0.49 | 0.23 |
| `ObeliskTests` | Homm2 / Data | Integration | 5 | 0.02 | 0.01 |
| `PlayerControlTests` | Homm2 / Data | Integration | 30 | 0.49 | 0.17 |
| `PlayerDifferTests` | Homm2 / Data | **Unit-shaped** | 4 | 0.24 | 0.12 |
| `PlayerTests` | Homm2 / Data | **Unit-shaped** | 8 | 0.02 | 0.01 |
| `RoundTripTests` | Homm2 / Data | Integration | 4 | 2.11 | 1.05 |
| `SafeCastleSerializerTests` | Homm2 / Data | **Unit-shaped** | 4 | 0.01 | 0.00 |
| `SafeHeroSerializerTests` | Homm2 / Data | **Unit-shaped** | 2 | 0.00 | 0.00 |
| `SaveLayoutLocatorTests` | Homm2 / Data | **Unit-shaped** | 7 | 0.03 | 0.02 |
| `SavegameParserTests` | Homm2 / Data | **Unit-shaped** | 5 | 4.10 | 4.07 |
| `TestDataNamesTests` | Homm2 / Data | Integration | 5 | 0.02 | 0.01 |
| `ThreedomEditorSavesTests` | Homm2 / Data | Integration | 7 | 0.30 | 0.28 |
| `ThreedomTests` | Homm2 / Data | Integration | 33 | 0.07 | 0.01 |
| `CreatureTests` | Homm2 / Entities | Unit | 6 | 0.01 | 0.00 |
| `EntityTablesTests` | Homm2 / Entities | Unit | 21 | 0.01 | 0.00 |
| `SharedEntityTests` | Homm2 / Entities | Unit | 5 | 0.01 | 0.00 |
| `SpellTests` | Homm2 / Entities | Unit | 8 | 0.00 | 0.00 |
| `CastleModelTests` | Homm2 / Models | **Unit-shaped** | 11 | 0.02 | 0.00 |
| `HeroLevelsTests` | Homm2 / Models | Unit | 7 | 0.00 | 0.00 |
| `HeroModelTests` | Homm2 / Models | **Unit-shaped** | 14 | 0.02 | 0.00 |
| `MaxSpellPointsCalculatorTests` | Homm2 / Models | **Unit-shaped** | 1 | 0.00 | 0.00 |
| `SecondarySkillsTests` | Homm2 / Models | Unit | 4 | 0.00 | 0.00 |
| `ArmyViewModelTests` | Homm2 / ViewModels | MultiUnit | 3 | 0.00 | 0.00 |
| `CastleWeeklyGrowthTests` | Homm2 / ViewModels | Integration | 4 | 0.01 | 0.00 |
| `HeroActiveSpellsTests` | Homm2 / ViewModels | Integration | 1 | 0.00 | 0.00 |
| `HeroVisitedPlacesTests` | Homm2 / ViewModels | Integration | 5 | 0.02 | 0.01 |
| `Homm2EditorViewModelTests` | Homm2 / ViewModels | **Unit-shaped** | 40 | 0.38 | 0.05 |
| `MapViewModelTests` | Homm2 / ViewModels | **Unit-shaped** | 6 | 0.01 | 0.00 |
| `MinesListsTests` | Homm2 / ViewModels | **Unit-shaped** | 2 | 0.03 | 0.02 |
| `PlayerViewModelTests` | Homm2 / ViewModels | **Unit-shaped** | 5 | 0.01 | 0.00 |
| `SavegameFileCommandsTests` | Homm2 / ViewModels | **Unit-shaped** | 13 | 0.38 | 0.28 |
| `PageRenderingTests` | Homm2 / Views | Integration | 1 | 2.72 | 2.72 |
| `ConstantsTests` | Homm3 / (root) | Unit | 3 | 0.01 | 0.01 |
| `Homm3LayeringTests` | Homm3 / (root) | Unit | 2 | 0.00 | 0.00 |
| `ActiveSpellsTests` | Homm3 / Data | Integration | 12 | 1.62 | 0.35 |
| `BiographyTests` | Homm3 / Data | Integration | 6 | 0.60 | 0.18 |
| `CastleBuilderRulesTests` | Homm3 / Data | Integration | 13 | 36.92 | 32.70 |
| `CastleBuilderTests` | Homm3 / Data | **Unit-shaped** | 4 | 132.34 | 42.15 |
| `CastleDeserializerTests` | Homm3 / Data | **Unit-shaped** | 10 | 34.04 | 33.44 |
| `CastleDifferTests` | Homm3 / Data | **Unit-shaped** | 13 | 1.43 | 0.16 |
| `CastleEditSessionTests` | Homm3 / Data | **Unit-shaped** | 7 | 0.89 | 0.19 |
| `CastleEditingTests` | Homm3 / Data | Integration | 11 | 1.82 | 0.30 |
| `CastleRegionProviderTests` | Homm3 / Data | **Unit-shaped** | 1 | 0.05 | 0.05 |
| `CollectionAccessTests` | Homm3 / Data | Integration | 5 | 0.78 | 0.27 |
| `CoveCastleTests` | Homm3 / Data | Integration | 16 | 4.10 | 1.06 |
| `DwellingCreaturesTests` | Homm3 / Data | Unit | 15 | 0.00 | 0.00 |
| `DwellingTests` | Homm3 / Data | Integration | 7 | 3.94 | 2.32 |
| `FactoryCastleTests` | Homm3 / Data | Integration | 21 | 3.21 | 0.21 |
| `GameDateLocatorTests` | Homm3 / Data | **Unit-shaped** | 16 | 4.57 | 2.43 |
| `GenderTests` | Homm3 / Data | Integration | 12 | 0.66 | 0.09 |
| `GoldLimitSavesTests` | Homm3 / Data | Integration | 2 | 0.96 | 0.96 |
| `HeroByteEditTargetTests` | Homm3 / Data | **Unit-shaped** | 9 | 2.54 | 0.35 |
| `HeroDeserializerTests` | Homm3 / Data | **Unit-shaped** | 5 | 0.01 | 0.00 |
| `HeroDestinationTests` | Homm3 / Data | Integration | 3 | 0.23 | 0.08 |
| `HeroEditSessionTests` | Homm3 / Data | **Unit-shaped** | 3 | 0.00 | 0.00 |
| `HeroOffsetsTests` | Homm3 / Data | Unit | 7 | 0.00 | 0.00 |
| `HeroOwnerEditsTests` | Homm3 / Data | **Unit-shaped** | 18 | 1.86 | 0.19 |
| `HeroOwnersBytesLoaderTests` | Homm3 / Data | **Unit-shaped** | 454 | 36.46 | 0.30 |
| `HeroOwnersDeserializerTests` | Homm3 / Data | Unit | 1 | 0.00 | 0.00 |
| `HeroRegionProviderTests` | Homm3 / Data | **Unit-shaped** | 9 | 2.51 | 0.32 |
| `Homm3PendingChangesFactoryTests` | Homm3 / Data | Unit | 1 | 0.00 | 0.00 |
| `HotaFourCornersTests` | Homm3 / Data | Integration | 2 | 0.28 | 0.14 |
| `HotaSaveTests` | Homm3 / Data | Integration | 6 | 0.78 | 0.77 |
| `HotaSleepGarrisonVisitingTests` | Homm3 / Data | Integration | 18 | 2.02 | 0.36 |
| `MageGuildLevelTests` | Homm3 / Data | Integration | 12 | 0.94 | 0.09 |
| `MageGuildSpellEditingTests` | Homm3 / Data | Integration | 10 | 1.25 | 0.24 |
| `MageGuildSpellsTests` | Homm3 / Data | Integration | 8 | 0.57 | 0.10 |
| `MapDifficultyTests` | Homm3 / Data | Integration | 6 | 0.70 | 0.27 |
| `MapHeaderTests` | Homm3 / Data | **Unit-shaped** | 8 | 1.04 | 0.29 |
| `MapObjectByteEditTargetTests` | Homm3 / Data | Integration | 4 | 0.31 | 0.08 |
| `MerchantStockTests` | Homm3 / Data | Integration | 5 | 2.79 | 2.48 |
| `MineTests` | Homm3 / Data | Integration | 7 | 4.66 | 1.84 |
| `MovedTodayTests` | Homm3 / Data | Integration | 13 | 1.16 | 0.15 |
| `ObeliskVisitsTests` | Homm3 / Data | Integration | 465 | 39.85 | 1.72 |
| `OtherFactionsCastleTests` | Homm3 / Data | Integration | 49 | 89.91 | 29.22 |
| `PlayerBanishingTests` | Homm3 / Data | Integration | 25 | 5.57 | 1.49 |
| `PlayerBytesLoaderTests` | Homm3 / Data | **Unit-shaped** | 7 | 0.62 | 0.10 |
| `PlayerControlLocatorTests` | Homm3 / Data | **Unit-shaped** | 14 | 1.26 | 0.27 |
| `PlayerDeserializerTests` | Homm3 / Data | **Unit-shaped** | 8 | 2.48 | 0.55 |
| `PlayerDifferTests` | Homm3 / Data | **Unit-shaped** | 1 | 0.08 | 0.08 |
| `PlayerHeadTests` | Homm3 / Data | Integration | 460 | 37.10 | 0.31 |
| `PlayerRegionProviderTests` | Homm3 / Data | **Unit-shaped** | 3 | 0.27 | 0.27 |
| `PlayersInTheGameTests` | Homm3 / Data | **Unit-shaped** | 5 | 0.04 | 0.01 |
| `RampartAndBulwarkCastleTests` | Homm3 / Data | Integration | 39 | 12.29 | 0.37 |
| `RampartCastleTests` | Homm3 / Data | Integration | 31 | 5.02 | 0.22 |
| `RoeBuildingEditsTests` | Homm3 / Data | Integration | 62 | 8.41 | 0.19 |
| `RoeCastleBuildingsTests` | Homm3 / Data | Integration | 146 | 54.83 | 6.18 |
| `RoeCastleTests` | Homm3 / Data | Integration | 45 | 16.58 | 1.36 |
| `RoeHeroTests` | Homm3 / Data | Integration | 16 | 2.61 | 1.01 |
| `RoeMapStateTests` | Homm3 / Data | Integration | 32 | 4.04 | 0.47 |
| `RoeSanyaMovementTests` | Homm3 / Data | Integration | 5 | 1.68 | 0.45 |
| `SafeCastleSerializerTests` | Homm3 / Data | **Unit-shaped** | 14 | 33.12 | 26.28 |
| `SafeHeroOwnersSerializerTests` | Homm3 / Data | **Unit-shaped** | 452 | 35.04 | 0.28 |
| `SafeHeroSerializerTests` | Homm3 / Data | **Unit-shaped** | 22 | 0.01 | 0.00 |
| `SaveRegionProviderTests` | Homm3 / Data | **Unit-shaped** | 15 | 3.01 | 0.69 |
| `SavefileDetectionTests` | Homm3 / Data | Integration | 21 | 1.63 | 0.27 |
| `SavefileRoundTripTests` | Homm3 / Data | Integration | 14 | 1.80 | 0.49 |
| `SodHotseatTests` | Homm3 / Data | Integration | 12 | 2.11 | 0.38 |
| `SodSaveTests` | Homm3 / Data | Integration | 11 | 0.63 | 0.16 |
| `SodSeriesTests` | Homm3 / Data | Integration | 92 | 23.87 | 8.52 |
| `TestDataNamesTests` | Homm3 / Data | Integration | 10 | 0.01 | 0.00 |
| `VisitedBuildingEffectsTests` | Homm3 / Data | **Unit-shaped** | 12 | 1.52 | 0.35 |
| `VisitedPlacesTests` | Homm3 / Data | Integration | 19 | 6.39 | 2.74 |
| `ArtifactTests` | Homm3 / Entities | Unit | 1 | 0.00 | 0.00 |
| `BuildingTests` | Homm3 / Entities | Unit | 9 | 0.01 | 0.00 |
| `CreatureInfoTests` | Homm3 / Entities | Unit | 4 | 0.00 | 0.00 |
| `EntityTablesTests` | Homm3 / Entities | Unit | 5 | 0.01 | 0.00 |
| `SharedEntityTests` | Homm3 / Entities | Unit | 5 | 0.00 | 0.00 |
| `CastleModelTests` | Homm3 / Models | **Unit-shaped** | 2 | 0.17 | 0.09 |
| `ChangeAreaTests` | Homm3 / Models | Unit | 1 | 0.00 | 0.00 |
| `GrantedSpellsResolverTests` | Homm3 / Models | Unit | 1 | 0.00 | 0.00 |
| `HeroEditingTests` | Homm3 / Models | Integration | 15 | 4.62 | 0.82 |
| `HeroModelTests` | Homm3 / Models | **Unit-shaped** | 7 | 0.59 | 0.10 |
| `HeroOwnersModelTests` | Homm3 / Models | Unit | 4 | 0.00 | 0.00 |
| `HeroPrimarySkillsTests` | Homm3 / Models | Integration | 14 | 1.61 | 0.25 |
| `MaxSpellPointsCalculatorTests` | Homm3 / Models | **Unit-shaped** | 2 | 0.38 | 0.23 |
| `PlayerModelTests` | Homm3 / Models | Unit | 10 | 0.00 | 0.00 |
| `SaveBaselineTests` | Homm3 / Models | Integration | 7 | 5.56 | 1.14 |
| `SpellBookAndGrantsTests` | Homm3 / Models | Integration | 11 | 0.36 | 0.10 |
| `GameServiceTests` | Homm3 / Runtime | **Unit-shaped** | 8 | 0.72 | 0.23 |
| `GzipSavefileCodecTests` | Homm3 / Runtime | Integration | 5 | 0.10 | 0.05 |
| `Homm3ModuleTests` | Homm3 / Runtime | **Unit-shaped** | 1 | 0.00 | 0.00 |
| `SavegameParserTests` | Homm3 / Runtime | **Unit-shaped** | 2 | 0.00 | 0.00 |
| `ArtifactFinderTests` | Homm3 / ViewModels | Integration | 8 | 1.66 | 0.61 |
| `BanishingViewModelTests` | Homm3 / ViewModels | Integration | 9 | 0.96 | 0.25 |
| `CastleDetailsViewModelTests` | Homm3 / ViewModels | Integration | 14 | 1.23 | 0.17 |
| `CastleEditingViewModelTests` | Homm3 / ViewModels | Integration | 10 | 7.86 | 6.98 |
| `CastleGarrisonedHeroTests` | Homm3 / ViewModels | Integration | 4 | 0.29 | 0.08 |
| `CastlePillAndTavernLinkTests` | Homm3 / ViewModels | Integration | 5 | 0.36 | 0.07 |
| `CastleViewModelTests` | Homm3 / ViewModels | **Unit-shaped** | 9 | 0.47 | 0.09 |
| `CreatureFinderAndArtifactEffectsTests` | Homm3 / ViewModels | Integration | 11 | 5.85 | 0.58 |
| `EquippedArtifactsViewModelTests` | Homm3 / ViewModels | **Unit-shaped** | 6 | 0.00 | 0.00 |
| `HeroEditingViewModelTests` | Homm3 / ViewModels | Integration | 14 | 7.58 | 0.61 |
| `HeroGeneralTabTests` | Homm3 / ViewModels | Integration | 9 | 4.81 | 0.61 |
| `HeroOwnerPickerTests` | Homm3 / ViewModels | Integration | 2 | 0.18 | 0.10 |
| `MapViewModelTests` | Homm3 / ViewModels | **Unit-shaped** | 2 | 0.16 | 0.09 |
| `NavigationBuilderTests` | Homm3 / ViewModels | **Unit-shaped** | 10 | 1.03 | 0.17 |
| `OrderOfPlayTests` | Homm3 / ViewModels | Integration | 1 | 0.09 | 0.09 |
| `OwnedMapObjectsViewModelTests` | Homm3 / ViewModels | **Unit-shaped** | 8 | 0.64 | 0.10 |
| `PlayerAndOverviewViewModelTests` | Homm3 / ViewModels | Integration | 18 | 5.42 | 0.56 |
| `SavegameFileCommandsTests` | Homm3 / ViewModels | **Unit-shaped** | 9 | 0.68 | 0.15 |
| `ShellSmokeTests` | Homm3 / ViewModels | Integration | 7 | 14.57 | 6.29 |
| `SkillsAndSpellsViewModelTests` | Homm3 / ViewModels | Integration | 16 | 8.68 | 0.61 |
| `SpellsFromArtifactsTests` | Homm3 / ViewModels | Integration | 4 | 2.13 | 0.61 |
| `ViewModelLayeringTests` | Homm3 / ViewModels | Unit | 1 | 0.01 | 0.01 |
| `HexCardsTests` | Homm3 / Views | Integration | 1 | 2.01 | 2.01 |
| `PageRenderingTests` | Homm3 / Views | Integration | 3 | 14.47 | 9.57 |
| `AppServicesTests` | App / (root) | **Unit-shaped** | 4 | 0.79 | 0.40 |
| `LoggingTests` | App / (root) | Integration | 5 | 0.61 | 0.53 |

## Run time

| Project | Tests | Test time | Over threshold |
|---|---:|---:|---|
| `Siniskalc.Core.Tests` | 79 | 0.2 s | none |
| `Siniskalc.Wpf.Tests` | 58 | 1.1 s | none |
| `Siniskalc.Heroes.Tests` | 132 | 0.3 s | none |
| `Siniskalc.Heroes.Wpf.Tests` | 142 | 5.9 s | 26 view tests between 100 and 700 ms (real windows) |
| `Siniskalc.Homm2.Tests` | 393 | 19.9 s | 5 integration tests over 2 s; 3 `[Explicit]` tests not run |
| `Siniskalc.Homm3.Tests` | 3,186 | 775 s | 21 integration tests over 2 s; 155 unit-shaped tests over 100 ms |
| `Siniskalc.Tests` | 9 | 1.4 s | `AppServicesTests` 0.4 s |

Slowest classes:

| Class | Tests | Total s | Slowest s | Why |
|---|---:|---:|---:|---|
| `CastleBuilderTests` | 4 | 132.3 | 42.2 | replays the reducer over every castle of every series save |
| `OtherFactionsCastleTests` | 49 | 89.9 | 29.2 | `EveryBuildingBuiltHasItsPrerequisitesBuilt` walks every save |
| `RoeCastleBuildingsTests` | 146 | 54.8 | 6.2 | one save load per case |
| `ObeliskVisitsTests` | 465 | 39.9 | 1.7 | every-save source |
| `PlayerHeadTests` | 460 | 37.1 | 0.3 | every-save source |
| `CastleBuilderRulesTests` | 13 | 36.9 | 32.7 | `RemovingABuilding…` replays every save |
| `HeroOwnersBytesLoaderTests` | 454 | 36.5 | 0.3 | every-save source on a unit-shaped class |
| `SafeHeroOwnersSerializerTests` | 452 | 35.0 | 0.3 | every-save source on a unit-shaped class |
| `CastleDeserializerTests` | 10 | 34.0 | 33.4 | one test loads all 448 saves |
| `SafeCastleSerializerTests` | 14 | 33.1 | 26.3 | `EveryCastleOfEverySampleSave…` |
| `SodSeriesTests` | 92 | 23.9 | 8.5 | |
| `RoeCastleTests` | 45 | 16.6 | 1.4 | |
| `ShellSmokeTests` | 7 | 14.6 | 6.3 | real shell, every page |
| `PageRenderingTests` (Homm3) | | 14.5 | | |

The HoMM3 hero view-model classes take 0.3–0.6 s per test because each test loads a save in `[SetUp]`: `SkillsAndSpellsViewModelTests` 8.7 s, `HeroEditingViewModelTests` 7.6 s, `CreatureFinderAndArtifactEffectsTests` 5.8 s, `HeroGeneralTabTests` 4.8 s, `HeroEditingTests` 4.6 s.

In HoMM2 the slow ones are `SavegameParserTests` (4.1 s in one test), `ShellTests` (3.9 s), `HeroDestinationAndFacingTests` (2.8 s in one test), `PageRenderingTests` (2.7 s) and `RoundTripTests` (2.1 s).

Unit-shaped classes spend 307 s, integration classes 490 s, and unit and multi-unit classes together 7 s.
Making the unit-shaped classes real unit tests and loading each save once would bring the suite back to about 5 minutes; the `CastleBuilder` reducer tests alone are 2 minutes of it.

Running the suite: `Siniskalc.Homm3.Tests` prints nothing while `CastleDeserializerTests.EveryCastleOfEverySavefileHasANameSoTheMapUsesIt` runs (about 2 minutes); it is slow, not hung.
Wait at least 15 minutes, or use `--blame-hang-timeout` well above 2 minutes.

## Inconclusive and weak tests

**Assertion-free findings addressed in slice 01:**

- `Homm2.Tests/Data/TmpTurnDump.cs`: removed the leftover `[Test] Dump()` and its hard-coded output path.
- `Homm3.Tests/Data/ObeliskVisitsTests.cs`: replaced `Assert.Pass` with absent-model and untouched-save byte-preservation assertions.
- `SkillsAndSpellsViewModelTests.cs`: replaced diagnostic output with a failing assertion for unknown spell sources.

**Skipped checks addressed in slice 02:**

- The six reported `continue` sites in `CastleBuilderTests`, `CastleBuilderRulesTests`, `RoeBuildingEditsTests` and `SodSeriesTests` were replaced by assertions or explicit eligible samples with presence guards.
  In particular, zero-building and multiple-building transitions are checked rather than filtered out.
- The player-count guard in `HeroOwnersBytesLoaderTests` became a player-presence assertion.
- The nullable Oidana guard in `HotaSleepGarrisonVisitingTests` became explicit parameterized expectations.

**Manual helpers, intentional**: `[Explicit]` tests that write edited saves for checking in the game: Homm2 `EditedSavesTests:111`, `EditorSavesTests:169`, `ThreedomEditorSavesTests:102`, Homm3 `GoldLimitSavesTests:52`.
They are fine; mark them `[Category("Manual")]` as well, so a filter never picks them up by accident.

**Weak assertions** (`Is.Not.Null`, `Is.Not.Empty` where a value is known): `ObeliskVisitsTests:178`, `PlayerDeserializerTests:85`, `CollectionAccessTests:42,46,95`, `ChangeAreaTests:17`, `BuildingTests:42`, `EntityTablesTests:52-53`, `ShellWindowTests:53,62`, Homm2 `PlayerViewModelTests:18-31`, Heroes `ChangeAreasTests:14`.

**Serializer tests that check through the deserializer** (should assert the bytes at their offsets):

- Homm3 `SafeHeroSerializerTests:39` and `SafeCastleSerializerTests:22`.
- Heroes `ArmyWriterTests.cs:76-81` (`WhatTheReaderReadsWritesBackAsTheSameBytes` uses `ArmyReader`); a round trip is a fine integration test, but the class should also pin bytes directly.
- Homm2 `EditingTests:101-119` checks the unknown-byte rule through the read model rather than the byte.

## Unit tests that are not isolated

Unit-shaped classes (named after one type, but loading saves or the whole service graph):

- **Homm3 Data**: `CastleDeserializerTests`, `CastleDifferTests`, `CastleEditSessionTests`, `CastleRegionProviderTests`, `GameDateLocatorTests`, `HeroByteEditTargetTests`, `HeroDeserializerTests`, `HeroEditSessionTests`, `HeroRegionProviderTests`, `HeroOwnersBytesLoaderTests`, `PlayerBytesLoaderTests`, `PlayerControlLocatorTests`, `PlayerDeserializerTests`, `PlayerDifferTests`, `PlayerRegionProviderTests`, `SaveRegionProviderTests`, `SafeCastleSerializerTests`, `SafeHeroSerializerTests`, `SafeHeroOwnersSerializerTests`.
- **Homm3 Models**: `CastleBuilderTests` (a reducer, tested only by replaying saves), `CastleModelTests`, `HeroModelTests`, `MaxSpellPointsCalculatorTests`.
- **Homm3 other**: `GameServiceTests`, `Homm3ModuleTests`, `SavegameParserTests`, `CastleViewModelTests`, `EquippedArtifactsViewModelTests` (real `EffectLineProvider` from `TestServices`; a small step to a fake), `MapViewModelTests`, `NavigationBuilderTests`, `OwnedMapObjectsViewModelTests`, `SavegameFileCommandsTests`.
- **Homm2**: `ByteEditTargetFactoryTests`, `CastleDifferTests`, `HeaderReaderTests`, `HeroChunkCollectionTests`, `HeroDifferTests`, `PlayerDifferTests`, `SafeCastleSerializerTests`, `SafeHeroSerializerTests`, `SaveLayoutLocatorTests`, `SavegameParserTests`, `CastleModelTests`, `HeroModelTests`, `MaxSpellPointsCalculatorTests`, `Homm2EditorViewModelTests`, `MapViewModelTests`, `PlayerViewModelTests`, `SavegameFileCommandsTests`.
- **App**: `AppServicesTests` (by nature a composition test; move it to `Integration/`).

Multi-unit without saying so:

- Core: `SaveDocumentTests`, `ChunkPendingChangeTests`, `RecordChunkCollectionTests`.
- Wpf: `ErrorWindowTests`, `ShellWindowTests` (real windows); `LoggingSettingsViewModelTests`, `RecentFilesMenuViewModelTests`, `ShellViewModelTests` use the real `SettingsStore` on disk, so they are integration.
- Heroes: `HeroesSavegameBaseTests` (real document graph).
- Heroes.Wpf: `ObeliskVisitsViewModelTests`, `RecordsPageViewModelBaseTests`, `SessionPageViewModelBaseTests` (real `SaveDocument` and history), and every `Views/*` test and `Styles/PageStylesTests` (real WPF windows).

Integration in a layer folder at review time: the Core file tests and nearly all Homm2 `Data` and `ViewModels` classes.
Slice 03a moved the eight filesystem-using Core runtime fixtures to `Integration/Runtime/`.

## Placement and naming

- **Projects**: every class is in the test project of the code it tests; `Siniskalc.Core.Tests`, `Siniskalc.Wpf.Tests`, `Siniskalc.Heroes.Tests` and `Siniskalc.Heroes.Wpf.Tests` name no HoMM2 or HoMM3 type.
  `HeroesLayeringTests` enforces it for `Siniskalc.Heroes` (it also rules out the planned `Siniskalc.Fheroes`).
- **Folders**: at review time none were in `Integration/` or `MultiUnit/`.
  Slice 03a began migration with the eight Core runtime fixtures; the historical class table above retains their original paths.
- **One class, several subjects** (against "one test class per subject"): Homm3 `SpellBookAndGrantsTests`, `CreatureFinderAndArtifactEffectsTests`, `CastlePillAndTavernLinkTests`, `PlayerAndOverviewViewModelTests`, `RampartAndBulwarkCastleTests`; Homm2 `HeroDestinationAndFacingTests`; and `CollectionAccessTests`.
  Scenario classes (`Roe*`, `Sod*`, `Hota*`, the faction `*CastleTests`) are allowed by `CLAUDE.md`.

### Naming consistency

`CLAUDE.md` sets the format, including the rubric added with the first review:

| Kind | Class | Example |
|---|---|---|
| Unit, multi-unit | `<ProductionType>Tests`, one type | `SafeCastleSerializerTests` |
| Integration, one save or series | `<Version><Scenario>Tests` | `RoeCastleBuildingsTests`, `SodHotseatTests` |
| Integration, one feature over every layer | `<Feature>Tests` in `Integration/` | `GenderTests`, `ObeliskVisitsTests` |

Methods are a present-tense sentence with articles, subject first, then the observable result (`AnUnknownClassReadsAsKnightAndIsKept`).
A member is named only when it is the public API under test; a save, hero or castle only together with what it shows.

What holds: every file has one test class named like the file, namespaces follow folders, no method has an underscore or a `Test`/`Should` prefix, and helpers (`TestServices`, `CastleSamples`, `SeriesSaves`, …) have files of their own.
167 of 263 classes are named after a production type.

Where names drift:

- **Feature classes outside `Integration/`**: the rubric allows `<Feature>Tests` only there; today they sit in `Data/` (`GenderTests`, `BiographyTests`, `ActiveSpellsTests`, `MovedTodayTests`, `PlayerControlTests`, `PlayerHeadTests`, `ObeliskVisitsTests`, `CastleEditingTests`, …), so moving them fixes the name too.
- **`TmpTurnDump`** was not named `…Tests` and was not a sentence; removed in slice 01.
- **Methods that say nothing about the behaviour**: `HeroDeserializerTests.DeserializingHeroDeserializes` and its siblings, `GrantedSpellsResolverTests.OneVersionsResolversAreEqualAndResolveThatVersionsSpells`, `RawSavefileCodecTests.ReadsWhatWasWrittenAnd…` (no subject), `HeroByteEditTargetTests.ExposesTitleBytesAndRegions`, `PlayerAndOverviewViewModelTests.QuickSetCommandsAndResetPlayerWork`, `SkillsAndSpellsViewModelTests.PickingSkillsAndLevelsInSlots`.
- **Telegraphic style** without articles in the older hero classes (`SingleByteEditCanBeUndoneAndRedone`, `RoundTripKeepsBytesIdentical`); both styles pass the rule, the full-sentence style is the one to keep.

Not findings: `Load`, `MakeTheApplication`, `LoadTheStyles`, `CreateFolder` and similar are `[SetUp]`/`[OneTimeSetUp]`/`[TearDown]` methods, not tests.

## Excessive coverage

- **Castle building progression** is asserted by the `CastleBuilder*` replays (169 s together), the faction `*CastleTests`, `RoeCastleBuildingsTests` and `RoeBuildingEditsTests`.
  Keep the per-faction byte series; make `CastleBuilder` a unit test over castle models built in the test, with one integration replay per series.
- **Every-save sources** (`ObeliskVisitsTests` 465, `PlayerHeadTests` 460, `HeroOwnersBytesLoaderTests` 454, `SafeHeroOwnersSerializerTests` 452, and `CastleDeserializerTests` in one test over 448 saves): useful as data contracts, but each loads every save again.
  Load each save once per run in a shared cache, or check all saves in one test per rule.
- **HoMM2 bulk save suites**: `EditedSavesTests`, `EditorSavesTests` and `ThreedomEditorSavesTests` overlap, and `PlayerControlTests` has about 30 cases over the same saves.
- **Heroes.Wpf view tests** repeat view-model semantics (`HeroCardsTests`, `MinesCardTests`, `ObeliskVisitsCardTests`, `DaysLeftCardTests`); keep them to bindings and layout.
- **Heroes**: `ExperienceTableTests` and `HeroLevelTests` check the same table.
- **HoMM3 view-model classes** load the same save in `[SetUp]` for every test.

## Gaps

Production types without a test class of their own:

| Project | Types |
|---|---|
| Core | `SavegameService`, `ByteBufferHelper`, `ByteRegionMap`, `ChunkByteEditTarget`, `EditSession`, `ModelChunkCollection`, `RecordKit` |
| Wpf | `DelegateCommand`, `NavigationTree`, `NavNodeViewModel`, `SavegameFileCommands`, the field and row view models, the `HexRow` classes |
| Heroes | `ObeliskEditSession`, `LinkViewModel`, `MapLevelChoice`, `MapSizes` |
| Heroes.Wpf | `ActiveSpellsCardViewModel`, `ArtifactPickerViewModel`, `CreatureFinderViewModel`, `HeroChoiceViewModel`, `HeroLinkViewModel`, `NextBattleCardViewModel`, `OrderOfPlayItemViewModel`, `PlayerCardViewModel`, the skill, spell and stack slot view models, many views |
| Homm2 Data | `BytesLoader`, `CastleDeserializer`, `PlayerDeserializer`, the mine, shrine, sphinx, turn and built-today deserializers, every `Safe*Serializer` except castle and hero, `SaveRegionProvider`, `MapPlacesReader`, `MapObjectsReader` |
| Homm2 other | `GameService`, `Homm2Module`, `Homm2ModuleFactory`, `NavigationBuilder`, `OverviewViewModel`, the artifact, spell and skill view models |
| Homm3 Data | `CastleBytesLoader`, `CastleChunkCollection`, the dwelling and mine deserializers, differs and region providers, `HeroBytesLoader`, `HeroDiffer`, `HeroOwnersDiffer`, `ObeliskDeserializer`, `PlayerBytesLoader`, `PlayerDiffer`, `SaveDocumentFactory`, `SavegameVersionDetector` |
| Homm3 other | `SpellGrantProvider`, `GameVersionExtensions`, `MerchantViewModel`, `VisitedPlacesViewModel`, `SpellFinderViewModel`, the slot and row view models |

Rules from `CLAUDE.md` with little direct coverage:

- **Unknown value reads as a stand-in, and the serializer keeps the byte**: covered for HoMM3 castles (owner, creature), heroes (class, facing, creature, scroll, artifact slot) and hero owners, Core `ByteEnum`, Heroes `SkillLevel` and army creature codes, and HoMM2 heroes (through `EditingTests`).
  Missing for HoMM3 players and obelisks, and HoMM2 castles, players and map objects.
- **Malformed input** (truncated chunks, missing markers) is hardly tested; the parser tests only feed random or empty bytes.

## Proposed conventions

Adopted in `CLAUDE.md` with the first review; classification migration started in slice 03a:

1. `[Category("Unit")]`, `[Category("MultiUnit")]` or `[Category("Integration")]` on every fixture, from one `TestKinds` constants class per project; `Integration/<Layer>/` and `MultiUnit/<Layer>/` folders; a check test that fails a fixture without a category or in the wrong folder.
2. Moq fakes for every dependency of a unit test; `TestServices` only in multi-unit and integration tests.
3. Load each save once per run; savefile tests only read.
4. No `Assert.Pass`, no silent `continue`, no conditional assert; a serializer test asserts bytes.

## Backlog (in order)

1. The originally reported assertion-free and skipped-check cleanup is complete in slices 01 and 02.
   The weak value assertions listed above remain to strengthen in the corresponding subject's isolation or coverage slice.
2. Add `TestKinds` and `[Category]` on every fixture with the check test; move classes into `Integration/` and `MultiUnit/`; mark the `[Explicit]` helpers `Manual`.
   Slice 03a completed eight Core runtime fixtures; remaining Core fixtures are scheduled as 03b and 03c.
3. Make the serializer tests assert bytes at offsets without the deserializer (HoMM3 hero and castle, Heroes army, HoMM2 editing).
4. Turn `CastleBuilderTests` and `CastleBuilderRulesTests` into reducer unit tests over models built in the test, with one replay per series (saves about 2–3 minutes).
5. Cache loaded saves for the every-save sources and series, or check all saves in one test per rule (saves several minutes).
6. Isolate the unit-shaped classes with Moq fakes, starting with the HoMM3 hero and castle view models and the HoMM2 differs and serializers.
7. Split the multi-subject classes and rename the methods listed under [Naming consistency](#naming-consistency).
8. Fill the gaps, starting with the HoMM2 serializers and deserializers and the unknown-byte tests for HoMM3 players and obelisks and HoMM2 castles and players.
9. Remove duplicated coverage once the unit tests that replace it are in place.
