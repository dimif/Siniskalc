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

Core's 20 fixtures were migrated in three bounded commits (03a runtime, 03b Data, 03c remaining fixtures).
All three Core classification slices are complete.

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
- At the 03a checkpoint, twelve Core fixtures remained uncategorized pending 03b and 03c.

### Slice 03b: Core Data fixture classification

- Classified `ByteEnumTests`, `ChangeSinkTests`, `ChunkCollectionTests`, `RecordChunkTests` and `SavefileBytesPatcherTests` as Unit, retaining their `Data/` location.
  Their assertions exercise one subject with enum/chunk/model values, an injected delegate factory or minimal test subclasses.
  `ChunkCollectionTests` checks lookup behaviour; its test subclass's reader/writer helpers do not participate in those checks.
- Classified `ChunkPendingChangeTests`, `RecordChunkCollectionTests` and `SaveDocumentTests` as MultiUnit.
  They compose real document/history, collection, record kit, patcher or edit-session behaviour without filesystem I/O.
  Moved them into `MultiUnit/Data/` with matching namespaces and imports for the existing shared Data helpers.
- All eight fixture bodies and method names are unchanged; all 79 discovered Core identities remain after normalizing the intentional namespace changes.
- Validation ran only Core: 79 passed in the full project run, 12 passed with `TestCategory=Unit`, and 14 passed with `TestCategory=MultiUnit`.
  All runs had zero failures, skipped or unexecuted tests.
  Reported durations were 156 ms, 18 ms and 26 ms respectively; no performance improvement is claimed.
- At the 03b checkpoint Core had 16 categorized fixtures (68 cases): 42 Integration, 12 Unit and 14 MultiUnit cases.
  Four fixtures (11 cases) remained for 03c.

### Slice 03c: complete Core classification

- Classified the four remaining fixtures as Unit: `RangeGuardTests`, `NumbersTests`, `VersionedEnumerationTests` and `LogFactoryTests`.
  The first three exercise pure functions, immutable values or a minimal enumeration subclass.
  `LogFactoryTests` uses an in-memory recording factory/logger and resets the global factory in teardown; no filesystem or application graph is involved.
  Its existing recording doubles are preserved; collaborator/mock refactoring is outside this classification-only slice.
- All four fixture bodies and method names are unchanged.
  All 79 discovered Core identities match the preceding checkpoint exactly.
- All 20 Core fixtures now have exactly one kind, with matching folder and namespace.
  The three category-filtered result sets partition the full set of 79 tests without overlap or omissions.
  Cross-project enforcement remains deferred to slice 09.
- Validation ran only Core: 79 passed in the full project run (172 ms); category runs passed 23 Unit (28 ms), 14 MultiUnit (26 ms) and 42 Integration (120 ms).
  There were zero failures, skipped or unexecuted tests; no performance improvement is claimed.

| Core kind | Fixtures | Cases |
|---|---:|---:|
| Unit | 9 | 23 |
| MultiUnit | 3 | 14 |
| Integration | 8 | 42 |
| Total | 20 | 79 |

### Slice 04a: Wpf view-model classification

- Classified `AboutViewModelTests`, `FinderViewModelTests` and `SortableListViewModelTests` as Unit.
- Moved `LoggingSettingsViewModelTests`, `RecentFilesMenuViewModelTests` and `ShellViewModelTests` to `Integration/ViewModels/`, with matching namespaces and Integration categories.
  Their real settings stores and filesystem setup make them integration fixtures even where individual assertions only inspect state.
- Added project-local `TestKinds`; preserved test bodies and helper ownership.
- Wpf project validation: all 58 tests passed, with no failures or skipped cases.
  Four view fixtures remain for 04b.

### Slice 04b: complete Wpf classification

- Classified `HexFormatTests` as Unit and moved `ErrorWindowTests` to `MultiUnit/Views/`.
- Moved `LoggingSettingsWindowTests` and `ShellWindowTests` to `Integration/Views/`, reflecting their real settings and filesystem use in addition to rendering.
  This corrects the earlier prose that described `ShellWindowTests` as MultiUnit.
- Preserved all test bodies, STA attributes, helpers and rendered-content checks.
- Wpf now has four Unit fixtures (21 cases), one MultiUnit fixture (3 cases) and five Integration fixtures (34 cases).
  Full project and all three category-filtered runs passed; the filtered sets partition all 58 cases without overlap or omissions.
  Fixture namespaces and folders match their categories.
  The full project run reported 630 ms; this is not a performance improvement claim.

### Slice 05a: Heroes data, entities and layering

- Categorized `ArmyReaderTests`, the two obelisk Data fixtures, `ResourceTests`, `SkillLevelTests` and `HeroesLayeringTests` as Unit.
  The obelisk collaborators are mocked; the army reader uses test creature codes and plain chunk/layout inputs.
- Moved `ArmyWriterTests` and `ChangeSinkExtensionsTests` into `MultiUnit/Data/` with matching namespaces and categories.
  Their real reader/writer round trip and real change-sink collaboration respectively make the historical Unit labels inaccurate.
  Retained every assertion, including the legitimate byte-for-byte round trip.
- Added project-local `TestKinds`; all 132 Heroes tests passed with no failures or skipped cases.
  Bodies and method names are unchanged; 27 fixtures remain for the later Heroes batches.

### Slice 05b: Heroes basic models

- Categorized seven basic model fixtures as Unit: army, banishable player base, change areas, experience table, game date, game-rule exception and game setup.
- Moved `HeroLevelTests` to `MultiUnit/Models/`: the reducer calls a real `ExperienceTable` lookup, rather than a mocked service.
  No production API or test body was changed.
- All 132 Heroes cases passed; unchanged bodies/names and category/path/namespace agreement were verified.
  Nineteen fixtures remain unclassified.

### Slice 05c: Heroes positions, ownership and hero values

- Categorized eight model fixtures as Unit: hero position/summary, map position, movement points, obelisk visits, owner names, player-model extensions and primary skills.
  Their records, test colours and test player subclasses are input values rather than real service collaborators.
- All 132 Heroes cases passed.
  Bodies and method names are unchanged and namespace/folder/category agreement was verified; eleven fixtures remain.

### Slice 05d: Heroes sets and savegame base

- Categorized resource sets, spell sets and tavern heroes as Unit.
- Moved `HeroesSavegameBaseTests` to `MultiUnit/Runtime/`: it constructs a real document/history and calls through a real game setup, but does not touch disk or a savegame file.
- All 132 Heroes cases passed; unchanged bodies/names and category/path/namespace agreement were verified.
  Seven ViewModels fixtures remain for the final Heroes batch.

### Slice 05e: complete Heroes classification

- Categorized all seven remaining ViewModels fixtures as Unit.
  `PlayerTypeTextTests` mocks game information; the creature-choice fixture uses test entities, and the other fixtures format immutable values.
- All 35 Heroes fixtures now have exactly one kind: 31 Unit fixtures (104 cases) and four MultiUnit fixtures (28 cases).
  There are no disk/savegame-backed Integration fixtures in this project; those belong to the game projects.
- The full Heroes project passed all 132 cases in 156 ms.
  Unit and MultiUnit filters passed 104 and 28 cases respectively; the Integration filter discovered zero cases.
  These result sets partition the full discovery exactly, without overlaps or omissions.
  No cases failed, skipped or remained unexecuted.
- All test bodies and method names were retained and category/path/namespace agreement was checked.
  The slowest individual case in the full run took 26 ms; no Unit or MultiUnit case exceeded the review's 100 ms threshold.
  These measurements do not establish a performance improvement.
- Slices 04 and 05 changed classification and placement only.
  Later isolation, assertion-strengthening and coverage slices remain open.

### Slice 06a: first Heroes.Wpf view-model fixtures

- Classified five fixtures as Unit: army totals, banishing text, castle links, level/class card and map-point card.
  Their inputs are immutable values, test delegates or mocked interfaces; the castle-link command is carried, not exercised.
- Moved `CastleHeroRowsTests`, `DaysLeftCardViewModelTests` and `HeroesArmyViewModelBaseTests` to `MultiUnit/ViewModels/`, with matching namespaces and categories.
  They exercise returned commands, real child-choice notifications, or army/slot/finder collaboration.
  Kept the shared test entities in their existing namespace.
- Added project-local `TestKinds`; all bodies, method names and assertions are unchanged.
  Styled-control rendering checks are untouched.
- Only Heroes.Wpf was run: all 142 cases passed, with no failures, skipped or unexecuted cases.
  The combined Unit/MultiUnit filter passed exactly the 36 newly categorized cases (18 of each), matching their full-run identities.
  Category/path/namespace agreement was checked.
- The full project reported 2 seconds; the filtered run reported 132 ms.
  The empty castle-row case took 239 ms in the full run and 39 ms in the filtered run; the filtered run's slowest selected case took 59 ms.
  This run-order-sensitive observation is not evidence of a performance improvement or a persistent slow unit.
- Eight of 35 Heroes.Wpf fixtures are categorized; 27 remain for 06b–06e.
  Paused after this bounded commit under the existing checkpoint agreement.

### Slice 06b: Heroes.Wpf mines, ownership and overview

- Classified movement/spell-points and overview view-model fixtures as Unit, with test delegates and mocked game information.
- Moved both mine-row fixtures, mines list, obelisk visits, owned links and owner navigation into `MultiUnit/ViewModels/`.
  Their real choice fields, sorting, synthetic document/history, link builders or navigation tree participate in the assertions.
  Synthetic bytes and mocked codecs do not make the obelisk fixture a real-savegame Integration test.
- Preserved every test body and name; category/path/namespace agreement was checked.
  Only Heroes.Wpf was run: all 142 cases passed, with no failures or skipped cases.
  Nineteen fixtures remain unclassified.

### Slice 06c: remaining Heroes.Wpf view models

- Classified tavern-choice filtering and weekly-growth text as Unit.
- Moved primary-skills card, resources editor, records page, session page, tavern place and visited-places card fixtures to `MultiUnit/ViewModels/`.
  Their assertions exercise child rows, model reducers, choice builders or real document/history behaviour; mocked outer boundaries do not isolate those collaborations.
- All 142 Heroes.Wpf cases passed; bodies/names and category/path/namespace agreement were verified.
  Eleven style/view fixtures remain.

### Slice 06d: Heroes.Wpf styles and rendered controls

- Moved `PageStylesTests` into `MultiUnit/Styles/` and seven control fixtures into `MultiUnit/Views/`: days-left, headed card, hero cards, items card, mines card, obelisk-visits card and owner picker.
  Their minimal WPF application loads compiled resources and hosts controls in memory; it does not compose the editor or load real savegame files.
- Retained every STA/setup attribute and every rendered-text, visibility, selection, geometry and command assertion.
  Test bodies/names and category/path/namespace agreement were checked.
- All 142 Heroes.Wpf cases passed; three view fixtures remain for 06e.

### Slice 06e: complete Heroes.Wpf classification

- Moved the three remaining view fixtures (page contract, page header and resources panel) to `MultiUnit/Views/`.
  Preserved their rendered-content and notification assertions, setup and STA attributes.
- Exact category-filter verification initially found seven omitted cases.
  `System.ComponentModel.CategoryAttribute` had shadowed NUnit's globally imported attribute in owner-picker, page-contract and page-header fixtures.
  Qualified those three attributes as `NUnit.Framework.Category`; the final filters now include every case.
- All 35 Heroes.Wpf fixtures are categorized and placed: nine Unit fixtures (35 cases), 26 MultiUnit fixtures (107 cases), no Integration fixtures.
  Their resources, synthetic binary documents and minimal WPF hosting are in-memory collaborations, not real-savegame or editor-composition integration tests.
- Validation ran only Heroes.Wpf.
  All 142 cases passed, as did the category-filtered runs (35 Unit, 107 MultiUnit, zero Integration discovered).
  The result sets partition discovery exactly without overlap or omissions, failures, skips or unexecuted cases.
  All 35 fixture bodies/names match the pre-slice baseline after normalizing only intentional imports, namespaces and categories; all 142 discovered identities were retained.
- The final full run reported about 2 seconds (2.223 seconds summed case durations).
  Two rendering cases exceeded the 100 ms Unit/MultiUnit review threshold: tab-panel visibility (338 ms) and level-card gender visibility (119 ms).
  The MultiUnit-only run again had slow tab-panel visibility (350 ms).
  The Unit-only run had one 281 ms castle-link case, while all Unit cases were below 100 ms in the full run; this is a run-order-sensitive observation, not a proven persistent unit slowdown.
  Rendering/startup performance remains for the later runtime slice; no category was broadened to hide these timings.
- Slices 06a–06e are complete; no production code or test assertions changed.
  Later isolation, assertion-strengthening and performance work remains open.

### Slice 07a: first HoMM2 data fixtures

- Moved eight real-save fixtures to `Integration/Data/`: built-today, byte-edit targets, castle differ, castles, edited saves, editing, editor saves and header reader.
  Their whole fixtures remain Integration, even when some cases inspect only in-memory values.
- Added project-local `TestKinds` and an additional method-level Manual category to the two Explicit save-writing helpers in this batch.
  Explicit markers are unchanged; neither writer was invoked.
- Adjusted the two `CallerFilePath` parent traversals for the extra folder depth.
  Verified that each still resolves to this worktree's root without running the writer.
  Other bodies and all assertions/names are unchanged; category/path/namespace agreement was checked.
- Only HoMM2 was run: 389 passed in about 17 seconds.
  Three Explicit helpers remain unexecuted (392 discovered); they are not counted as passed.
  Thirty-eight fixtures remain unclassified.

### Slice 07b: HoMM2 heroes, map objects and player control

- Moved eight fixtures to `Integration/Data/`: hero collection, destination/facing, hero differ, heroes, map objects, obelisks, player control and player differ.
  Their real-save reads and `TestServices` composition make them Integration regardless of subject-shaped class names.
- Retained every series case, byte assertion and method name; category/path/namespace agreement was checked.
- HoMM2 validation passed 389 cases; three Explicit helpers remained unexecuted.
  Thirty fixtures remain unclassified.

### Slice 07c: HoMM2 serialization, parsing and layout

- Moved players, round trips, castle/hero serializers, parser, layout locator, test-data naming and Threedom edited-save fixtures to `Integration/Data/`.
  Their real-save inputs or filesystem enumeration make them Integration; the isolated binary-contract work remains later.
- Added Manual to the third Explicit writer and adjusted its source-relative output root for the extra folder depth.
  Verified all three writer roots without executing them; all other test bodies and all assertions/names are unchanged.
- HoMM2 validation passed 389 cases; three Explicit helpers remained unexecuted.
  Category/path/namespace agreement was checked; 22 fixtures remain unclassified.

### Slice 07d: HoMM2 entities, models and Threedom

- Classified creature and spell fixed-data fixtures as Unit.
- Moved entity-table and shared-entity fixtures to `MultiUnit/Entities/`: they span several entity subjects and the shared palette contract.
  Their subject/naming consolidation remains later work.
- Moved hero-level tests to `MultiUnit/Models/`, reflecting calls through the real shared experience table.
- Moved Threedom, castle-model and hero-model fixtures to Integration with matching layer folders.
  Reducer assertions do not make a fixture isolated when its inputs are loaded from real saves.
- All 389 automated HoMM2 cases passed; three Explicit helpers remained unexecuted.
  Bodies/names and category/path/namespace agreement were checked; fourteen fixtures remain.

### Slice 07e: HoMM2 models and editor scenarios

- Classified secondary skills as Unit and moved army view-model tests to `MultiUnit/ViewModels/` for their real army/slot/reducer collaboration.
- Moved the spell-points calculator and weekly-growth tests to `Integration/Models/`.
  The latter was incorrectly under ViewModels: it tests `CastleModel.WeeklyGrowth`.
  Both fixtures take their input from real saves.
- Moved active-spell, visited-place, editor and map-page fixtures to `Integration/ViewModels/`.
  Kept save-based setup and assertions unchanged.
- Existing HoMM2 editor `Assert.Ignore`/conditional checks and the underscore in a map-object test name remain for later assertion/isolation/naming work, not silently fixed in this placement-only slice.
- All 389 automated HoMM2 cases passed; three Explicit helpers remained unexecuted.
  Category/path/namespace agreement was checked; six fixtures remain.

### Slice 07f: complete HoMM2 classification

- Moved the remaining mines, player, save-command, rendered-page and shell scenarios to Integration with matching folders/namespaces.
  The layering fixture also belongs there: it checks actual build-output files, not an isolated in-memory subject.
- All 46 fixtures now have exactly one kind: three Unit, four MultiUnit and 39 Integration.
  Automated cases partition exactly into 18 Unit, 36 MultiUnit and 335 Integration (389 passed).
  Three additional save-writing methods retain both Explicit and Manual; their full-run outcomes were NotExecuted.
  They were never selected for execution.
- Verified all 392 discovered fixture/case identities and outcomes against 07a, ignoring the intentional namespace moves.
  Compared every fixture body/name against the pre-slice baseline, allowing only category/namespace/import changes and the three documented manual output-root corrections.
  Preserved rendering assertions, STA attributes, setup/cleanup, Explicit attributes and test-case sources.
- Only the affected HoMM2 project was run, per the requested validation scope.
  Final project duration was about 8 seconds; filtered runs passed 18/36/335 cases without overlap or omissions.
  The Integration filter explicitly excluded Manual.
  `--list-tests --filter` returned the complete inventory on this runner, so Manual tagging was verified against source and the three unexecuted TRX results instead.
- Final full-run case-duration sum: Unit 0.126 s, MultiUnit 0.008 s, Integration 8.368 s.
  Slowest Integration cases were unedited save serialization (2.136 s), shell bindings (1.509 s) and rendered pages (0.979 s).
  The creature ranged/shots Unit case took 115 ms in the full run but 11 ms in the Unit-only run; this is a run-order/startup observation, not an established persistent unit-performance defect.
  MultiUnit's filtered maximum was 54 ms.
  These measurements do not replace the historical review timings below.
- Classification is complete, not isolation/coverage remediation: mixed save-backed fixtures remain Integration.
  Splitting pure cases, improving weak assertions and reducing replay cost remain later slices.

## Summary

- **4,000 tests in 263 classes and 7 projects, all passing.**
  The sum of test durations is 804 s; `Siniskalc.Homm3.Tests` alone takes 775 s (about 13 minutes) and every other project together under 30 s.
- **Run time is driven by savefile replays, not by units.**
  The four `CastleBuilderTests` take 132 s, and the every-save sources (`ObeliskVisitsTests`, `PlayerHeadTests`, `HeroOwnersBytesLoaderTests`, `SafeHeroOwnersSerializerTests`, 1,831 cases) another 148 s.
  `TestData` grew to 448 saves, and every test that walks all saves grows with it.
- **At review time no test had a `[Category]`, and no project had `Integration/` or `MultiUnit/` folders.**
  Slice 03a has since categorized eight Core fixtures (42 cases) as Integration and moved them into `Integration/Runtime/`; the rest of the migration remains open.
  Slice 03b added five Unit fixtures (12 cases) and three MultiUnit fixtures (14 cases), placing the latter in `MultiUnit/Data/`.
  Slice 03c completed Core: all 20 fixtures and 79 cases are now categorized; migration of the other projects remains open.
  Slices 04a and 04b completed Wpf: all 10 fixtures and 58 cases are categorized and placed.
  Slices 05a–05e completed Heroes: all 35 fixtures and 132 cases are categorized and placed (104 Unit, 28 MultiUnit).
  Slices 06a–06e completed Heroes.Wpf: all 35 fixtures and 142 cases are categorized and placed (35 Unit, 107 MultiUnit).
  Slices 07a–07f completed HoMM2: all 46 fixtures and 392 discovered cases are categorized and placed (18 Unit, 36 MultiUnit, 335 automated Integration and three Explicit/Manual Integration helpers).
  The original automated classification counted 92 unit, 19 multi-unit, 94 integration and 58 unit-shaped classes.
  These are historical estimates, not current category totals; source review has corrected mixed fixtures, real collaborations and filesystem use as detailed above.
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

- Core: `SaveDocumentTests`, `ChunkPendingChangeTests`, `RecordChunkCollectionTests` (categorized and placed in `MultiUnit/Data/` in slice 03b).
- Wpf: `ErrorWindowTests` is MultiUnit; `ShellWindowTests` also uses the real disk and settings, so it is Integration.
  The three disk-backed view-model fixtures are Integration as well.
  All Wpf placement/category changes were completed in 04a and 04b.
- Heroes: `HeroesSavegameBaseTests` (real document graph).
- Heroes.Wpf: `ObeliskVisitsViewModelTests`, `RecordsPageViewModelBaseTests`, `SessionPageViewModelBaseTests` (real `SaveDocument` and history), and every `Views/*` test and `Styles/PageStylesTests` (real WPF windows).

Integration in a layer folder at review time: the Core file tests and nearly all Homm2 `Data` and `ViewModels` classes.
Slice 03a moved the eight filesystem-using Core runtime fixtures to `Integration/Runtime/`.

## Placement and naming

- **Projects**: every class is in the test project of the code it tests; `Siniskalc.Core.Tests`, `Siniskalc.Wpf.Tests`, `Siniskalc.Heroes.Tests` and `Siniskalc.Heroes.Wpf.Tests` name no HoMM2 or HoMM3 type.
  `HeroesLayeringTests` enforces it for `Siniskalc.Heroes` (it also rules out the planned `Siniskalc.Fheroes`).
- **Folders**: at review time none were in `Integration/` or `MultiUnit/`.
  Slices 03a and 03b migrated the eight Core runtime and eight Core Data fixtures; the historical class table above retains their original paths.
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
   Slices 03a through 03c completed all 20 Core fixtures; migration of the other six projects and cross-project enforcement remain open.
   Slices 04a and 04b completed all 10 Wpf fixtures.
3. Make the serializer tests assert bytes at offsets without the deserializer (HoMM3 hero and castle, Heroes army, HoMM2 editing).
4. Turn `CastleBuilderTests` and `CastleBuilderRulesTests` into reducer unit tests over models built in the test, with one replay per series (saves about 2–3 minutes).
5. Cache loaded saves for the every-save sources and series, or check all saves in one test per rule (saves several minutes).
6. Isolate the unit-shaped classes with Moq fakes, starting with the HoMM3 hero and castle view models and the HoMM2 differs and serializers.
7. Split the multi-subject classes and rename the methods listed under [Naming consistency](#naming-consistency).
8. Fill the gaps, starting with the HoMM2 serializers and deserializers and the unknown-byte tests for HoMM3 players and obelisks and HoMM2 castles and players.
9. Remove duplicated coverage once the unit tests that replace it are in place.

## Slice 08: HoMM3 classification

All 115 HoMM3 fixtures classified (Unit 11, MultiUnit 3, Integration the rest) and moved to Integration/MultiUnit folders. Bodies and names unchanged. Run excluding Manual: 3198/3198 passed in 10m28s (save-reading integration tests dominate). GoldLimitSaves writer marked Manual and never run. Known weak: HeroEditingTests.SecondarySkillsEditsSurviveSerialization does not serialize.

## Slice 09: Host tests and cross-project consistency

The two host fixtures (AppServicesTests, LoggingTests) build the whole graph and touch the filesystem, so they are Integration under Tests/Siniskalc.Tests/Integration (9/9 passed, 3 s). LoggingTests' expected log lines now carry the new namespace. A scan of all 263 test files in the seven projects found each has exactly one kind category and a folder that matches it (Integration/ and MultiUnit/ folders for those kinds; Unit in the layer folders): 0 mismatches.

## Slice 10: Independent byte assertions for HoMM3 serializers

SafeHeroSerializerTests gained five tests that assert literal bytes (hero class code, the four primary-skill bytes, little-endian experience and one-byte level, creature type and count slots, secondary-skill count/level/slot bytes) and a changed-offsets helper; SafeCastleSerializerTests gained a literal-byte garrison test. The existing round-trip tests stay as their own labelled contract. 42/42 passed in the two fixtures (37 s).

## Slice 11: ArmyWriter and HoMM2 unknown-byte checks

ArmyWriterTests: the refused-count test now asserts no byte was cut, plus two tests with literal expected bytes (a matching army leaves every byte alone; changing only the count of an unknown creature keeps its number). HoMM2 EditingTests: a new test asserts that editing only the army of a hero holding unknown values changes just the slot's type byte and the two count bytes (little-endian 258) and keeps the unknown artifact and owner bytes. Heroes ArmyWriterTests 8/8, HoMM2 EditingTests 10/10.

## Slice 12 - CastleBuilder rules on in-memory castles

- New Unit fixture `Models\CastleBuilderTests` checks the build/remove rules (upgrade needs base, faction, map, village hall, lighthouse/shipyard, horde, mage guild levels, dwelling creatures) on castles made by `CastleModels`, with no savefile.
- New MultiUnit fixture `MultiUnit\ViewModels\CastleBuildMessageProviderTests` holds the message wording that used to ride along in the savefile tests.
- Integration: `CastleBuilderTests` became `CastleBuilderReplayTests` (replays of real saves); `CastleBuilderRulesTests` became `CastleBuilderSavefileRulesTests` and keeps only the checks that compare against real saves.
- 27 tests in the affected fixtures pass (about 48 s, mostly the save replays). Mutation was not checked.

## Slice 13 - Shared loads in the every-savefile contracts

- New `Data\ReadOnlySaves` loads each savefile once for tests that only read it. `ObeliskVisitsTests` and `PlayerHeadTests` use it for their every-savefile cases (511 saves); tests that edit, undo or serialize a changed document still load their own copy.
- Same 925 tests, none removed. The two fixtures ran in 82 s before and 46 s after (one run each, so a rough figure). Hero-owner contracts are slice 14.

## Slice 14 - Shared loads in hero-owner contracts and castle replays

- `HeroOwnersBytesLoaderTests` and `SafeHeroOwnersSerializerTests` (every-savefile cases) and `CastleBuilderReplayTests` now load saves through `ReadOnlySaves`. The owner serializer does not change the chunk it is given, and the replays only read the loaded castles and serialize through `TestServices`. Tests that edit keep a fresh history.
- Same tests, none removed: the hero-owner pair (906 cases) ran in 34 s from a cold cache; the four replays went from 36 s to 9 s (one run each). Other castle-series fixtures still load their own saves.

## Slice 15 - Isolating the settings-dependent Wpf view models

- Production change (small): `ISettingsStore` and `IRecentFilesStore` in `Siniskalc.Core.Runtime`, implemented by `SettingsStore` and `RecentFilesStore`. `ShellViewModel`, `LoggingSettingsViewModel`, `RecentFilesMenuViewModel`, `SavegameFileCommands` and `FileBackupServiceProvider` take the interfaces; `AppServices` still builds the concrete stores.
- `RecentFilesMenuViewModelTests` is now a Unit fixture (`ViewModels\`) over a Moq store, no temp folder. Its two real-store cases (add and remove through the store) are not repeated there; `RecentFilesStoreTests` in Core covers the store. One case was added for rebuilding on change and unsubscribing on dispose.
- `ShellViewModelTests` uses a mocked `ISettingsStore`; it stays Integration because the shell reads the files it is asked to open. `LoggingSettingsViewModelTests` stays Integration on purpose: the page creates and probes folders and writes the settings file, and that is the contract.
- Not done in Core: the MultiUnit `SaveDocument`/`RecordChunkCollection`/`ChunkPendingChange` fixtures were not rewritten with mocks.
- Core 79, Wpf 58, Heroes 134, Heroes.Wpf 142 and the exe tests (9) pass; the Homm2/Homm3 projects only built.

## Slice 16 - Heroes.Wpf view models that are units

- Reclassified as Unit and moved to `ViewModels\`: `MineRowViewModelTests` (the edit session is a Moq fake), `DaysLeftCardViewModelTests` and `PrimarySkillsCardViewModelTests` (their only collaborators are injected delegates and immutable values). Bodies and names unchanged.
- Left MultiUnit on purpose: `SessionPageViewModelBaseTests` (needs a real `SaveDocumentHistory`, which is sealed and returned by `IEditSession.History`), `RecordsPageViewModelBaseTests`, the rendered controls, and the view models that build real child view models. `ResourcesEditorViewModelTests` and four other candidates are left for a later sub-slice (three-subject limit).
- Heroes tests need no change: their unit-shaped fixtures were already Unit. Heroes.Wpf: 142 pass; the Unit filter now runs 49 cases in 93 ms.
- Added `docs\TEST_REVIEW_HANDOVER.md`: the plan, what is done, working rules and open items for another agent.

## Slice 16b - Remaining Heroes.Wpf view-model candidates

- Reclassified as Unit and moved to `ViewModels\`: `ResourcesEditorViewModelTests` (state and update are injected delegates, limits and resources are immutable values) and `MineRowViewModelBaseTests` (injected delegates plus a passive `ChoiceFieldViewModel`). Bodies and names unchanged.
- Left MultiUnit on purpose: `VisitedPlacesCardViewModelTests` (card over real `VisitedPlaceRowViewModel` and `ToggleRowViewModel` rows), `HeroesArmyViewModelBaseTests` (real `StackSlotViewModel` slots and `CreatureFinderViewModel`), `OwnerNavGroupTests` (real `NavigationTree` and nodes). They check how real view models fit together.
- Heroes.Wpf: 142 pass; the Unit filter runs 58 cases in 105 ms. Only this project was run.

## Slice 17 - HoMM2 differs as units

- New Unit fixtures in `Tests\Siniskalc.Homm2.Tests\Data`: `HeroDifferTests` (9 cases), `PlayerDifferTests` (5) and `CastleDifferTests` (8). They build records by hand (`DifferSamples`) and use a Moq `IPendingChangesFactory` (`PendingChangesFake`) that keeps the revert function of each change by its description, so each test checks the wording of Before/After, the order, which changes bypass the factory (owner) and what the revert function gives back.
- The save-based fixtures keep their cases and are renamed `HeroDifferSavegameTests`, `PlayerDifferSavegameTests` and `CastleDifferSavegameTests` (Integration): they still cover the revert through a real history (owner, lists, countdown). Some wording cases overlap the new Unit ones; removal is for slice 29.
- Mutation check: swapping the known/not-known revert of a spell in `HeroDiffer` made `ALearnedSpellAndASecondarySkillAreListedByName` fail; reverted. Other cases were not mutation-checked.
- Left for 17b: the HoMM2 serializer and model-reducer fixtures and the other differs.
- Homm2.Tests: 412 pass (the whole project); no other project was run, no production code changed.

## Slice 17b - HoMM2 serializers and BuiltTodayDiffer as units

- New Unit fixtures in `Tests\Siniskalc.Homm2.Tests\Data`: `SafeHeroSerializerTests` (11 cases), `SafeCastleSerializerTests` (9) and `BuiltTodayDifferTests` (2). The serializers write into records made by hand (a zero record filled by the sample model, `DifferSamples`) and `ByteDiff` lists exactly which bytes changed, so each case states the offsets and values of one field and that no other byte moves. They do not call a deserializer. Covered: field widths, signed bonuses, artifact and spell marks, secondary skill bytes, unknown owner, faction and facing bytes kept while the model says the stand-in, an oversized coordinate kept, where the creatures of a dwelling move on an upgrade or a take-down, the dragon mark, the count beside the upgraded ones left alone, and out-of-range values refused without touching the record.
- The save-based fixtures are renamed `SafeHeroSerializerSavegameTests` and `SafeCastleSerializerSavegameTests` (Integration) and keep their cases (they read back through the deserializer, which is the round-trip contract). Overlap is for slice 29.
- Mutation check (all caught, production restored): castle creature lists no longer emptied on a move (3 tests fail), hero owner always written (1), built-today castles not ordered (1). Not checked: the other cases.
- Homm2.Tests: 434 pass (the whole project); no other project was run, no production code changed.
- Left for 17c: `HeroModelTests`, `CastleModelTests` and the Mine, Shrine, Sphinx and Turn differs.

## Slice 17c - HoMM2 model reducers and TurnDiffer as units

- `HeroModelTests` (12 cases) and `CastleModelTests` (9) are now Unit fixtures in `Models\`, on the hero and castle of `DifferSamples` (a Warlock or a Wizard is the sample with another faction). The cases that depend on what a savegame holds are kept as Integration in `HeroModelSavegameTests` (faction by hero number, the sword moving between two heroes of a save) and `CastleModelSavegameTests` (Whiteshield's dwelling bits, the guilds and Library of Peacekeeper, Dragontooth, Vertigo and Woodhaven, the shared interface on Xabran, used records, Hillstone's creatures). The in-memory sword case uses a hero made by hand instead of Sir Gallant.
- `SecondarySkillsTests` gained the slot test that was in the hero fixture (`SecondarySkillsCanBeSwappedReplacedAndRemovedBySlot`), since it tests `SecondarySkills`.
- New `TurnDifferTests` (4 cases) on records made by hand.
- Mutation check: the first run left two mutants alive (the Library ignored when a guild is built later; `TurnDiffer` comparing the colour instead of the record). A case was added for each and both are now caught. Also caught: a neutral hero keeping its destination. Not checked: the other cases.
- Homm2.Tests: 442 pass (the whole project); no other project was run, no production code changed.
- Left for 17d: `MineDiffer`, `ShrineDiffer`, `SphinxDiffer` and a look at the other HoMM2 fixtures that may be unit-shaped.

## Slice 17d - HoMM2 map-object differs and the spell points calculator as units

- New Unit fixtures in `Tests\Siniskalc.Homm2.Tests\Data`, one per differ: `MineDifferTests` (2 cases), `ShrineDifferTests` (3) and `SphinxDifferTests` (2). They check the wording of Before/After, the order of the sphinx changes, the chunk kind and index of each change and the name of the record. Their revert functions run through a chunk and a history, so putting back stays covered by the Integration tests (`EditingTests`, `MapObjectsTests`).
- `MaxSpellPointsCalculatorTests` moved to `Models\` as Unit (it only needed a skill set, not a savegame) and gained the case of negative knowledge.
- Mutation check (all caught, production restored from git after one restore was refused by the file system): the Ore reward skipped by the sphinx differ, negative knowledge allowed in the calculator, the mine differ using the shrine chunk kind. Not checked: the shrine revert and the other cases.
- Homm2.Tests: 450 pass (the whole project); no other project was run, no production code changed.
- Slice 17 is done for the differs, serializers of hero and castle, and the hero and castle models. The other HoMM2 fixtures that read saves (`CastleWeeklyGrowthTests`, `HeroDestinationAndFacingTests`, `MapObjectsTests`, `MinesListsTests`, `HeroActiveSpellsTests`) were not rewritten; the handover lists them as an optional 17e. The HoMM2 serializers and deserializers of players and map objects have no tests of their own yet (slices 21-22).

## Slice 18 - HoMM3 view models that need no savegame

- Surveyed `Tests\Siniskalc.Homm3.Tests\Integration\ViewModels`: only the artifact fixtures read no savegame. The hero view models take a concrete `HeroEditSession`, which exists only over a loaded save, and the castle and player ones are driven through the whole editor, so they stay Integration (end-to-end scenarios are wanted by the plan anyway).
- `EquippedArtifactsViewModelTests` moved to `MultiUnit\ViewModels` and builds `EffectLineProvider` itself instead of taking it from `TestServices` (the whole graph).
- `ArtifactFinderTests` is now MultiUnit (5 cases: search words, pick only when allowed, equip from the finder, scroll in an item slot, scrolls only in item slots), on equipment made by hand. The 3 cases that edit the hero page (equip finder to backpack, backpack add and undo, scroll in the backpack) are `ArtifactFinderHeroTests` (Integration).
- Mutation check (caught, production restored with git): `canChoose` ignored by the finder, mirroring of the slots a combination artifact blocks removed (4 tests). Not checked: the other cases.
- Homm3.Tests: the 14 artifact finder and equipment cases and the 95 Unit and MultiUnit cases pass; the full HoMM3 project was not run (the files only moved and one fixture was split).
- Left for 18b: isolating the hero view models needs a small production seam, to be decided with the user (see the handover).

## Slice 18b - A seam for the hero page, and the first hero view-model tests without a savefile

- Production change (small, no behaviour change). Core: `IHistoryEvents` (the `Changed` event), implemented by `SaveDocumentHistory`; `IEditSession<T>.Events` returns it (`EditSession` gives its history). Heroes.Wpf: `ResettablePageViewModelBase` listens to an `IHistoryEvents` and `SessionPageViewModelBase` passes `session.Events` (HoMM2 and the records pages still pass the real history). HoMM3: `IHeroSession : IEditSession<HeroModel>` with `HeroIndex`, `Revision`, `GameVersion`, `SupportsAsleep`, `GarrisonCastleIndex`, `CastleName`, `ChangeOwner` and `CreateByteEditTarget(factory)`; `HeroEditSession` implements it from the document with the same queries `HeroViewModel` used to make, and `HeroViewModel` takes it (and `Session` returns it).
- `SessionPageViewModelBaseTests` (Heroes.Wpf) is Unit now, over a Moq history and session (it was the case slice 16 left), with a new case for the history event and `Dispose`.
- New test helpers in Homm3.Tests: `HeroModels.Christian()` (a hero made by hand) and `FakeHeroSession` (applies edits, raises the history events with a new revision, answers the savefile facts from what a test sets).
- New `HeroViewModelTests` (MultiUnit, 16 cases): header, max and reset of the points, moved flag and active spells, whole-hero reset, clamping, owner pick through the session (rule refusal gives a notice, a fault does not), garrison castle name and opening, sleep box by version, only a revision of the hero refreshes the children, a disposed page stops listening, hex editor target through the factory. The real child view models are used, so it stays MultiUnit. The older save-based fixtures are untouched (their cases overlap; slice 29).
- Mutation check (all caught, production restored): catching every exception for the owner, no revision check, sleep box always on, factory not passed, dispose that does not unsubscribe.
- Run: whole solution builds; Core 79, Wpf 58, Heroes 134, Heroes.Wpf 143, Homm2 450 pass; Homm3 1922 pass (view models, hero fixtures and all Unit/MultiUnit); the exe tests and the rest of the HoMM3 project were not run.

## Slice 19 - HoMM3 model and differ tests by their real dependencies

- Surveyed the HoMM3 data, runtime and model fixtures by what they load. Most read a savefile because the save is the contract (deserializers, serializers, region providers, loaders, the series and scenario tests) and stay Integration. Three only needed models:
- `MaxSpellPointsCalculatorTests` (2 cases) is Unit in `Models\`: the skills come from `HeroModels.Christian()` instead of Valeska in a HotA save.
- `CastleModelTests` (2 cases) is Unit in `Models\` on `CastleModels.Village(Faction.Rampart)`: the shared-interface case and the weekly growth of the Centaur Stables. The save-based reading of a castle stays covered by `CastleDeserializerTests` and the castle series.
- New `PlayerDifferTests` (7 cases, Unit) with a Moq `IPendingChangesFactory` (`PendingChangesFake` of Homm3.Tests): wording of resources, tavern, days left (incl. "not counting") and heroes, the order, and that the heroes change is made without the factory. The save-based case is `PlayerDifferSavegameTests` (Integration) and still checks the revert through a history.
- Mutation check (all caught, production restored): heroes listed before the other changes, selected hero ignored, HotA percentages never used, weekly growth off by one. Not checked: the other cases.
- Run: Homm3 Unit, MultiUnit and the save-based player differ pass (the full HoMM3 project was not run; files moved and tests added only; no production code changed).
- Left for 19b: see the handover.

## Slice 19b - HoMM3 hero model, castle differ and pending changes factory

- `HeroModelTests` (7 cases) moved to `Models\` as Unit on `HeroModels.Christian()` (it took the first hero of a Bulwark save to test reducers).
- New `CastleDifferTests` (9 cases, Unit) on `CastleModels.Village(Faction.Rampart)` with the Moq factory: equal castles, built and removed buildings, the automatic building not listed, allowed and forbidden wording with the revert function, and `RestoreBuilding` (what was built on top comes back, what a removed building needed comes back, a building not in the original is taken down). The Bulwark save fixture is now `CastleDifferSavegameTests` (Integration) and keeps the revert-through-history and several-castle cases.
- `Homm3PendingChangesFactoryTests` gained the player, castle and obelisk cases (3 new, Unit).
- Mutation check (all caught, production restored): castle changes filed as player changes, a neutral hero keeping its destination, `RestoreBuilding` ignoring the original. Not checked: the other cases.
- Run: Homm3 Unit, MultiUnit, `CastleDifferSavegameTests` and `PlayerDifferSavegameTests` pass; the full HoMM3 project was not run; no production code changed.
- Environment note: the .NET SDK was removed by an installer during this slice and came back as 11.0.100-rc.3; the build prints warnings that it cannot download the 8.0.33 reference packs (no network), but compiles and runs with the installed ones.

## Slice 20a - HoMM2 multi-subject fixtures

- `Homm2EditorViewModelTests` (719 lines, 40 tests on eight subjects) is split by page: `Homm2EditorNavigationTests` (15), `Homm2EditorOverviewTests` (2), `Homm2EditorPlayerPageTests` (7), `Homm2EditorHeroPageTests` (10), `Homm2EditorCastlePageTests` (5) and `Homm2EditorHexTabTests` (1). The editor with a Broken Alliance savegame and its settings folder is built by `Homm2EditorHarness` (disposable, also `OpenCastleOf`), and each fixture keeps its own helpers. Test bodies and names are unchanged and the unused `using`s of the copies were dropped.
- `HeroDestinationAndFacingTests` is split into `HeroDestinationTests` and `HeroFacingTests`. The one test that edited the destination and the facing together became three that each name the bytes they may change (the destination edit writes only the eight bytes of the destination, the facing edit only the facing byte, removing the destination writes the none marker).
- `MapObjectsTests.TheSphinxOfArrax_TheJerkHasSixAnswersAndGivesMercury` lost its underscore (`TheSphinxOfArraxTheJerkHasSixAnswersAndGivesMercury`). The HoMM2 `Assert.Ignore` and conditional checks the handover mentioned were not found by a search of the test project; none were changed.
- Method names were scanned in all test projects for underscores, `Test` and `Should` prefixes and very short names; the vague ones are listed in the handover as 20b. Fixture names were checked: every class ends in `Tests`, is named like its file and is alone in it.
- Mutation check (caught, production restored): the facing byte never written, the destination Y never written. Not checked: the moved page tests.
- Homm2.Tests: 452 pass (the whole project; 450 plus the two tests gained by the split); no other project was run; no production code changed.

## Slice 20b - Test names that did not state the behaviour

- Renamed 15 methods in 7 fixtures (bodies untouched), following `CLAUDE.md`: a save, hero or castle is named with what it shows, and a name never repeats the subject.
  - `HeroDeserializerTests`: `DeserializingHeroDeserializes` is `TheSkillsClassSpellBookAndArmyOfAPreHotaHeroAreReadFromItsRecord`; its three siblings now say what they read (`TheWholeModelOfMaximusIsReadFromADumpOfHisHotaRecord`, `AHeroWhoHoldsASpellBookIsReadAsHavingOne`, `AHeroWithoutASpellBookIsReadAsHavingNone`).
  - `HotaSaveTests`: `EveryHeroDeserializes` is `EveryHeroRecordOfTheSaveIsReadWithoutAnError`; the three `...IsDeserialized` are `<Hero>IsReadWithTheOwnerClassLevelSkillsAndSpellsOfTheSave`.
  - `CreatureInfoTests`: `EveryCreatureHasHealthSpeedAndGoldAndANonInvertedDamageRange`, `TheGemsAndMercuryACreatureCostsAreImported`, `ARangedCreatureHasItsShotsAndAMeleeCreatureHasNone`.
  - `GzipSavefileCodecTests` and `RawSavefileCodecTests`: both now start with the subject (`ASavefileIsReadBackAsItWasWritten...`).
  - `GrantedSpellsResolverTests`: `ResolversOfTheSameGameVersionAreEqualAndTheTomeOfFireMagicGrantsFireball`.
  - `HeroByteEditTargetTests`: `TheTargetOfAHeroHasTheNameOfTheHeroItsBytesAndItsRegions`.
- Not renamed here: `PlayerAndOverviewViewModelTests.QuickSetCommandsAndResetPlayerWork` and `SkillsAndSpellsViewModelTests.PickingSkillsAndLevelsInSlots`, which belong to fixtures that slice 20c splits (the limit for a mechanical slice is eight fixtures). The telegraphic names without articles (`SingleByteEditCanBeUndoneAndRedone`, ...) pass the rule and were left.
- Run: the renamed fixtures only (Core 3, Homm3 30) pass; no production code changed.

## Slice 20c - HoMM3 multi-subject fixtures

- Six Integration fixtures that named two or more subjects were split by subject, 66 tests in all (the count by attribute is 66 before and 66 after, and all 66 pass). Test bodies are unchanged; each new fixture keeps only the fields, `SetUp`/`TearDown` and helpers its tests use, and the unused `using`s were dropped.
  - `SpellBookAndGrantsTests` (11): `SpellBookTests` (3), `ArtifactSpellGrantsTests` (6), `HeroArtifactBonusesTests` (2).
  - `CastlePillAndTavernLinkTests` (5): `CastleHeroLinksTests` (3), `CastleGrailAndCapitolPillTests` (2).
  - `CreatureFinderAndArtifactEffectsTests` (11): `CreatureFinderViewModelTests` (7), `ArtifactEffectsViewModelTests` (4).
  - `PlayerAndOverviewViewModelTests` (19): `PlayerNavigationTests` (9), `PlayerViewModelTests` (4), `OverviewViewModelTests` (5).
  - `CollectionAccessTests` (5): `HeroAndCastleLookupTests` (3), `SessionRevisionTests` (1), `PlayerCollectionTests` (1). Note this fixture was in HoMM3 `Data`, not Core as the first review had it.
  - `SkillsAndSpellsViewModelTests` (16): `SecondarySkillsViewModelTests` (6), `SpellsViewModelTests` (10).
- The two names left from 20b were rewritten: `TheQuickSetButtonsAddToTheResourcesAndResetPutsThePlayerBack` and `ASkillPickedInASlotKeepsTheLevelOfTheSlotAndASkillTheHeroHasIsNotAddedTwice`.
- `RampartAndBulwarkCastleTests` stays: it is a scenario fixture, which `CLAUDE.md` allows.
- Other multi-subject candidates were not looked for beyond the review's list; the name scan found nothing more.
- Run: the 15 new fixtures (66 tests) pass; the rest of the HoMM3 project was not run (files were split by script and the test count is unchanged); no production code changed.

## Slice 21 - HoMM2 castle and player read/write boundaries

- `BlankRecords` builds a castle or a player record as the game writes a fresh one (every id, creature and spell the empty mark, the rest zero); a test sets only the bytes it is about. `ByteDiff` (slice 17b) states exactly which bytes a serializer moved.
- `CastleDeserializerTests` (14 cases): tile, boat and visiting hero (and their empty marks), owner bytes (a colour, the neutral byte and bytes that are no colour read as `Neutral`), faction bytes (an unknown one reads as `Neutral`), the name up to the first zero, the creatures read from the list of the level of each dwelling (own, upgraded, upgraded twice, with a stale count left in the own list), the Mage Guild counts and spell numbers (an empty mark, the byte past the last spell and a byte that is no spell read as none), and a garrison creature code that is no creature read as Peasant with its count.
- `PlayerDeserializerTests` (5 cases): the colour is the name of the record, resources and income and the separate gold income, lists that skip the empty mark, and selections, days left and tavern as ids or none.
- `SafePlayerSerializerTests` (7 cases): a model read from a record leaves every byte alone, an amount is written in four little-endian bytes, income places, a stored amount past the limit is kept while the model says it and refused when written elsewhere, a list and its count are written together, a list that reads as the model says is left with its count even with gaps, and ids taken out of a selection leave the empty mark.
- `SafeCastleSerializerTests` gained 6 cases: owner byte that is no colour kept for `Neutral` and replaced for a colour, visiting hero id and empty mark, garrison stack bytes, a creature code that is no creature kept for the Peasant stand-in and replaced by another creature, the buildings and dwellings as little-endian numbers, and the name and the number of the castle never written.
- Mutation check (caught, production restored): the list count not written, the owner stand-in lost, the gold income read from the table, the visiting hero always written. One mutant survived on purpose of the check: writing the selected castle as its plain id is the same as the code that keeps the existing byte for every id from 0 to 254, so it is equivalent, not a gap.
- Homm2.Tests: the whole project passes; no other project was run; no production code changed.
- Not covered here (slice 22): map objects and the remaining deserializers and serializers.

## Slice 22a - HoMM2 mines and shrines

- Four new Unit fixtures over `MapObjectChunk` records made by hand (the chunk carries the tile, the place in the table, a tag and the parts that are the flags drawn on the tiles): `MineDeserializerTests` (4 cases), `SafeMineSerializerTests` (7), `ShrineDeserializerTests` (4) and `SafeShrineSerializerTests` (5).
- Mines: the kind, tile, index and owner are read; an owner byte that is the neutral byte or no colour reads as `Neutral`. The writer returns the very same chunk when the owner reads the same (so a mine whose flag the game never drew stays so, also for an owner byte that is no colour while the model says `Neutral`), writes the colour byte, draws the flag (class `0x38`, picture 14, 21 or 28 plus the colour by kind) on every tile that does not already show the new owner, clears the flags for `Neutral`, and never writes the kind, tile or index.
- Shrines: the circle is the two low bits of the tag and the spell is the number above the low three bits of the word; a hidden shrine reads with spell -1. The writer changes only the spell number, keeps the three low bits, leaves hidden shrines, models with no spell and unchanged spells alone, and refuses a number that does not fit a byte without touching the record. A spell number the game does not have reads as the number it is (`Spell` is `null`), which is what the code does and what the test states.
- Mutation check (all caught, production restored): the low bits of the shrine word dropped, the hidden check removed, the flags never redrawn, the unchanged-owner shortcut removed, the shrine circle mask widened (found a gap in the first version of the test; fixed), and the mine owner byte masked. Not checked: the other cases.
- Homm2.Tests: the whole project passes (504); no other project was run; no production code changed.
- Left for 22b and 22c: see the handover.

## Slice 22b - HoMM2 sphinxes and obelisks

- Four new Unit fixtures on records made by hand: `SphinxDeserializerTests` (9 cases, its `Sphinx(set)` builds a sphinx with room for a riddle of 99 characters), `SafeSphinxSerializerTests` (8), `ObeliskDeserializerTests` (4) and `SafeObeliskSerializerTests` (6).
- Sphinx, reading: the riddle is not answered for any non-zero first byte (0 means answered), the seven rewards are four-byte numbers and the artifact two bytes, the answers are as many texts as the count byte says, each up to its first zero, the riddle is read up to its first zero and the room it has is the bytes after it minus one, and a reward too big for the model reads as the number the cast gives (-16 for `0xFFFFFFF0`) without an error.
- Sphinx, writing: the model read from a record leaves every byte alone, any non-zero active byte is kept while the model says active (and set to 0 or 1 when it says otherwise), a reward is written in four little-endian bytes, a reward that reads as the stored one is kept while a negative one is refused, the artifact in two bytes, new answers and a new riddle clear the old text before they are written, and an empty or too many answers, an answer of 13 characters, an empty riddle and a riddle longer than the room are refused and leave the record unchanged.
- Obelisks: one byte for each tile found, with a bit for each colour; bits of colours the game does not have are read as they are; only as many bytes as there are tiles are read or written (a record with more bytes keeps the others); fewer visits than tiles leave the rest of the bytes alone.
- Mutation check (all caught, production restored): any non-zero active byte read as only 1, the answer area and the riddle area not cleared, the active byte always rewritten, the answer length limit off by one, the obelisk reader reading all bytes, and the obelisk writer writing past the tiles (the first version of the test had exactly as many bytes as tiles and missed it; fixed).
- Homm2.Tests: the whole project passes (531); no other project was run; no production code changed.
- Left for 22c: see the handover.

## Slice 22c - HoMM2 turn and built-today records

- Four new Unit fixtures on records made by hand: `TurnDeserializerTests` (4 cases), `SafeTurnSerializerTests` (4), `BuiltTodayDeserializerTests` (3) and `SafeBuiltTodaySerializerTests` (4).
- Turn: the record is the first byte; its colour comes from the packed order of the colours that sits in the tag of the chunk (`PlayerRecordOrder`), so the same record can be another colour in another savegame; a record past the six players has no colour and is kept as the number it is. The writer returns the same chunk when the record is unchanged (whatever colour the model holds, also for a record past six), writes only the byte of the turn otherwise, and refuses a record that is not a player without touching the chunk. A tag that packs a colour past the sixth (7) would make the reader index past the list; the code that makes the tag (`PlayerRecordOrder.ColorsOf`) never gives one, so no test states what would happen.
- Built-today: nine bytes, one bit for each of the 72 castles of the table, the first in the lowest bit of the first byte. The reader lists the set bits in table order; the writer sets and clears only the bit of each castle, and returns the same chunk when nothing changes.
- Mutation check (all caught, production restored): the colour read without the packed order, the record limit changed, the unchanged shortcut removed, the range guard removed, the bit order reversed, bits never cleared, and a new chunk always made.
- Homm2.Tests: the whole project passes (see the count in the report); no other project was run; no production code changed.
- Left for 22d: `BytesLoader`, `SaveRegionProvider`, `MapPlacesReader`, `MapObjectsReader`.

## Slice 22d - HoMM2 `BytesLoader` and `SaveRegionProvider`

- `BytesLoaderTests` (11 cases, Unit) cut the chunks out of a savegame made by hand: a `SaveLayout` record with the tables one after the other, and a zeroed byte array of about 26 KB. Covered: 54 hero chunks named by the text of the record (up to the first zero) and of the size of a record; 72 castle chunks named by the text at the end of the record; a chunk holds a copy of its bytes; player chunks come in the order of the colours whatever order the records are in (the record says which colour it is) and, when the records do not name six different colours, in the order of the colours; no table of tiles, no obelisk tile, or a savegame cut before the flags of the obelisks (also when the tiles are found) give no obelisk chunk; the obelisk chunk is the 48 bytes of flags after the castles with the tiles of the obelisks, and only the first 48 obelisks of a map are taken.
- `SaveRegionProviderTests` (5 cases, MultiUnit): with no layout there are no regions; the header has the map name, the description and the date with its three parts; each player, hero and named castle has a region of its whole record (an unnamed castle has none); the regions are in the order of the bytes; the fields of a record are shifted to where the record is. The collections are the real ones over fake readers, hence MultiUnit.
- Mutation check (all caught, production restored): a hero chunk one byte short, players in the order of the records, no limit of 48 obelisks, no length check before the flags (survived the first version, whose truncated savegame was cut before the tiles; and the second, whose obelisk tile was overwritten by an all-zero hero record; fixed), a castle name read from the wrong place, unnamed castles listed, regions not ordered, fields not shifted, and regions listed with no layout.
- Homm2.Tests: the whole project passes (see the report); no other project was run; no production code changed.
- Left for 22e: `MapPlacesReader` and `MapObjectsReader`.

## Slice 22e - HoMM2 `MapPlacesReader` and `MapObjectsReader`

- `MapSavegame` (a test helper) builds a savegame with the tables of players, heroes and castles one after the other, the area of the mines and of the sphinx blocks, and the table of tiles of a square map with its addons; it can put an object on a tile, a hero on a tile (the tile then holds the hero marker and the hero keeps the object under it), a mine record and a sphinx block.
- `MapPlacesReaderTests` (6 cases): no table of tiles gives no places; each kind is listed at the tiles with its object, in the order of the tiles row by row; the standing stones are two tiles wide and the place is the left one (also at the start of a row, although the last tile of the row above holds the same object); a hero standing on a place does not hide it; a hero outside the map is ignored.
- `MapObjectsReaderTests` (15 cases): the current turn (the human byte with the order of the colours in the tag), the nine bytes of the castles that built something, and no mines or shrines without a table of tiles; a mine is a seven-byte record with its markers, a tile on the map, a kind up to the gold mine and an owner up to Purple or the neutral byte, named by kind and tile, and a record that fails any check is left out while the others are numbered in order (each of the three markers is checked); the flag part of a mine is on its tile when the slot is empty or holds a flag, in the addon the tile links to otherwise, none when both hold other objects, none for an alchemist's lab, and for a sawmill on the tile up and to the right (none at the edge); a shrine is found at each tile with the object of a circle and holds the two bytes of its quantity, and one with a hero on it is hidden and keeps its circle; a sphinx is a block with the block marker, a length, a flag of 0 or 1, one to eight printable answers and a riddle, blocks that are not are left out, the blocks are numbered from one, a sphinx has its tile only when the map has as many sphinx tiles as blocks, and sphinxes are still found when there is no table of tiles.
- No error semantics were invented: truncated blocks and missing markers are covered only as far as the code defines them (a block that is cut or has no marker is simply not a sphinx; no table of tiles is no places).
- Mutation check (all caught, production restored; three gaps found in the first versions and fixed): the standing stones not paired, the edge rule of the stones, a changed object byte, heroes not keeping their object, the hero bound dropped, the mine owner and kind limits and each marker byte, the sawmill flag place, the addon never used, the hidden mark, the sphinx flag and answer checks, tiles used when the counts differ, and the turn tag.
- Homm2.Tests: the whole project passes (583); no other project was run; no production code changed.

## Slice 23 - HoMM3 player and obelisk contracts

- `PlayerRecords.Blank` builds a record of a player as the game writes a fresh one, in either layout (Shadow of Death 145 bytes, Horn of the Abyss 149): no hero, no selection, no tavern hero, no days left. `ByteDiff` is a copy of the HoMM2 helper (a game's tests do not reference another game's project). The save-based `PlayerDeserializerTests` became `PlayerDeserializerSavegameTests` to leave the name to the new Unit fixture.
- `PlayerDeserializerTests` (7 tests, run for both layouts): a colour byte from 1 to 8 is the colour before it, and a byte that is no colour (0, 9, 0x80, 0xFF) reads as the colour the loader gave the chunk; the resources are signed four-byte numbers in the order of the resource set; hero ids skip the empty mark, the selected hero and the tavern heroes are none for the empty mark, and hero 0 is a hero in every list; the days left are the byte up to the most a player can have and any other byte reads as none.
- `SafePlayerSerializerTests` (11 tests, both layouts): a model read from a record leaves every byte alone (also with a days-left byte that is no count, a list with gaps and a count that does not match); an amount is written in four bytes and one that does not fit is refused; the list of heroes and its count are written together and only when the list does not read as the model says; more than eight heroes, an id of 255 (the empty mark) in the list, as the selected hero or as a tavern hero are refused; the selected hero and tavern heroes are written one by one; a days-left byte that is no count is kept while the model has none and replaced when it has a day, a day taken away gets the empty mark, and a day beyond the most is refused; the number of towns, the town list and the colour byte are never written whatever the model says.
- `ObeliskDeserializerTests` (4) and `SafeObeliskSerializerTests` (5): one byte for each obelisk with a bit for each colour starting with Red; only as many bytes as the count byte says are read or written (the count itself is never written, bytes past it are not touched, fewer visits leave the rest alone); the tiles pass through.
- Not tested, on purpose: a record of obelisks shorter than its count byte (the reader reads past the chunk and throws today; nobody has decided what it should do), and the loaders (23b).
- Mutation check (all caught, production restored; one gap found in the first version, hero id 0 read as none by the tavern, fixed): colour byte 0 accepted, days limit off by one, the empty mark of the hero ids ignored, the list compared with gaps, the count not written, the days-left stand-in lost, the days and tavern and resource guards removed, the obelisk count ignored by the reader and by the writer. One mutant survives and is equivalent: writing the selected hero byte always writes the byte it reads as.
- Homm3.Tests: the Unit, MultiUnit and the renamed save-based fixture pass (192); the full HoMM3 project was not run; no production code changed.

## Slice 24a - HoMM3 `HeroDiffer` and `HeroOwnersDiffer`

- `HeroDifferTests` (15, Unit) on the hero of `HeroModels.Christian()` and the Moq factory (`PendingChangesFake` now also makes the changes of a hero and keeps their revert functions by description): equal heroes; the owner first and made without the factory (it is put back through the session, with the lists of the players and the table of hero owners); class and gender (an unknown gender is worded "unknown"); moved today and asleep as yes or no; a spell cast today, put back without touching the other active spells; morale and luck signed and each visited building listed by name and put back alone; the places visited as the number of visits of each kind; the secondary skills as one list of slots with levels and, when a ninth skill has a level, "not shown"; stacks by slot; the war machines as one change whatever their order; worn artifacts and scrolls by slot (a scroll is worded with its spell) and the knowledge an artifact adds listed by the shared wording; backpack slots numbered from one; the spell book flag and the learned spells in one change; and the primary skills and the level by the shared wording of the Heroes project.
- `HeroOwnersDifferTests` (1): the table of hero owners lists no change of its own (the owner of a hero is listed with the hero).
- Mutation check (all caught, production restored; three gaps found in the first version and fixed: the revert of a visited building cleared the other buildings, the "not shown" skills and the order of the war machines): owner order, class and gender revert and wording, asleep not listed, active spell and visit effect reverts, unsigned morale, places as a mask, scroll spell not named, backpack slot numbering, spell book flag and learned spells.
- Homm3.Tests: the 200 Unit and MultiUnit cases pass; the full HoMM3 project was not run; no production code changed.

