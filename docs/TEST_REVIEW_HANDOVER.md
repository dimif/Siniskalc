# Test-suite improvement: handover

For an agent (or person) continuing the test-suite cleanup of Siniskalc. Read `CLAUDE.md` (repo rules), `docs\TEST_REVIEW.md` (the review and a section per finished slice), `TestData\README.md` (savefile naming) and `docs\LOGGING.md` before changing anything.

## Goal

Make the tests meaningful, honestly classified and cheap to run:

- Each test checks something specific; no `Assert.Pass`, no silent skips, no assertion-free output.
- Every fixture states its kind with `[Category(TestKinds.X)]` and sits in the matching folder:
  - **Unit**: one class, Moq fakes (or injected delegates) for collaborators, no savefiles, no disk. Lives in the layer folder (`Data`, `Models`, `ViewModels`, ...).
  - **MultiUnit**: several real classes, no savefiles. `MultiUnit\<Layer>\`.
  - **Integration**: reads `TestData`, writes to disk, or builds the whole graph (`TestServices`). `Integration\<Layer>\`.
  - **Manual**: an extra method-level label for `[Explicit]` helpers. Never run them (use `TestCategory!=Manual`).
- Only Integration tests and binary serializer/deserializer tests may be slow.
- Serializer tests assert literal bytes, not what the deserializer reads back. Round trips are allowed as an extra, separately named contract.
- Naming follows `CLAUDE.md`: `<Subject>Tests`, methods are PascalCase sentences about behaviour, no underscores.

## Working rules (from the user)

- Work in small slices; one commit per slice; **stop after each commit** and report. Never start the next slice on your own.
- Never push.
- Test only the affected projects per step (`dotnet test Tests\<Project> --artifacts-path $env:TEMP\<folder> --filter ...`). Run only the affected tests. Do not rerun the full solution (about 10 minutes on the HoMM3 project) unless there is a good reason, such as profiling run time; say so when you do, and never claim a measurement that was not like-for-like.
- Do not change production behaviour. Small seams (an interface so a test can mock a collaborator) are acceptable if they keep behaviour; state them in the report.
- Do not share loaded mutable documents between tests. `Tests\Siniskalc.Homm3.Tests\Data\ReadOnlySaves` shares a loaded `Game` only for tests that read; check the code under test does not mutate before using it.
- Commit trailer: `Co-authored-by: Copilot App <223556219+Copilot@users.noreply.github.com>`.
- Branch `dimif-test-suite-review`, based on `feature/homm2-support`. On Windows PowerShell there is no `&&`; each call starts in the repo root.
- Report honestly: say what was and was not run, what was not mutation-checked, and what is left.

## Done (commit per slice)

| Slice | Commit | What |
|---|---|---|
| review | `c3f9b3c` | `docs\TEST_REVIEW.md`, `test-reviewer` agent (`.github\agents`, `.claude\agents`) |
| 01 | `a1dace0` | Replaced unchecked outcomes (obelisk `Assert.Pass`, output-only checks) with assertions |
| 02 | `53237a2` | Replaced silent skips and conditional assertions with checked expectations |
| 03 | `ec69f34`, `e7548f1`, `aec068c` | Core classified and placed (`TestKinds`, `Integration\Runtime`, `MultiUnit\Data`) |
| 04 | `2ced2cf`, `0f91a79` | Wpf classified and placed |
| 05 | `adf84ed` .. `fbe2bfa` | Heroes classified and placed |
| 06 | `99aedf3` .. `1394b08` | Heroes.Wpf classified and placed (rendered-content tests kept) |
| 07 | `bc41230` .. `aedbfde` | HoMM2 classified and placed; manual writers labelled |
| 08 | `2429e84` | HoMM3: 115 fixtures (11 Unit, 3 MultiUnit, rest Integration) |
| 09 | `b695252` | Host tests (`AppServicesTests`, `LoggingTests`) classified; all 263 files checked once by script |
| 10 | `3b61c12` | Literal-byte tests for `SafeHeroSerializer` and `SafeCastleSerializer` |
| 11 | `cac7a10` | `ArmyWriter` and HoMM2 unknown-byte tests with specified bytes |
| 12 | `67fbedc` | `CastleBuilder` rules as Unit tests on in-memory castles (`CastleModels`), message wording as MultiUnit, replays kept as Integration (`CastleBuilderReplayTests`, `CastleBuilderSavefileRulesTests`) |
| 13 | `822e375` | `ReadOnlySaves`: obelisk and player-head every-save contracts load each save once (82 s to 46 s) |
| 14 | `7f26565` | Same for hero-owner contracts and castle replays (replays 36 s to 9 s) |
| 15 | `1fb6894` | `ISettingsStore` / `IRecentFilesStore` seams; `RecentFilesMenuViewModelTests` is Unit, `ShellViewModelTests` uses a mocked store |
| 16 | `b9f2031` | Heroes.Wpf: `MineRowViewModelTests`, `DaysLeftCardViewModelTests`, `PrimarySkillsCardViewModelTests` reclassified Unit and moved to `ViewModels\` |
| 16b | see git log | Heroes.Wpf: `ResourcesEditorViewModelTests`, `MineRowViewModelBaseTests` reclassified Unit; `VisitedPlacesCardViewModelTests`, `HeroesArmyViewModelBaseTests`, `OwnerNavGroupTests` stay MultiUnit on purpose |
| 17 | see git log | HoMM2: `HeroDifferTests`, `PlayerDifferTests`, `CastleDifferTests` are Unit on records made by hand (`DifferSamples`, `PendingChangesFake`); the save-based ones are now `...DifferSavegameTests` (Integration) |
| 17b | see git log | HoMM2: `SafeHeroSerializerTests`, `SafeCastleSerializerTests` (literal bytes via `ByteDiff`) and `BuiltTodayDifferTests` are Unit; the save-based serializer ones are now `...SerializerSavegameTests` (Integration) |
| 17c | see git log | HoMM2: `HeroModelTests` and `CastleModelTests` are Unit on records made by hand, `TurnDifferTests` is new; the save-dependent cases are in `HeroModelSavegameTests` and `CastleModelSavegameTests` (Integration) |
| 17d | see git log | HoMM2: `MineDifferTests`, `ShrineDifferTests`, `SphinxDifferTests` (new, Unit) and `MaxSpellPointsCalculatorTests` (reclassified Unit, now two cases on a skill set made by hand) |

Each slice's details, timings and caveats are in its section of `docs\TEST_REVIEW.md`.

## Remaining plan (in order; dependencies are prerequisites, not permission to batch)

| ID | Scope |
|---|---|
| 17e (optional) | Look at the other HoMM2 fixtures that may be unit-shaped: `CastleWeeklyGrowthTests` (needs saves for the Well/Farm facts, the no-Well case can be in memory), `HeroDestinationAndFacingTests`, `MapObjectsTests`, `MinesListsTests`, `HeroActiveSpellsTests`; decide per fixture whether the save is the contract |
| 18 | Isolate HoMM3 hero/castle view-model tests; keep representative end-to-end editor scenarios |
| 19 | Isolate remaining HoMM3 data/runtime/model tests by real dependencies, not names |
| 20 | Split multi-subject fixtures, fix names that do not describe behaviour (scenario/feature integration fixtures are allowed). Split by project |
| 21-22 | Missing HoMM2 contracts: castle/player read-write boundaries, then map objects and remaining serializers/deserializers (stand-ins, byte preservation, explicit replacement) |
| 23 | HoMM3 player/obelisk unknown-byte contracts and malformed input, without inventing error semantics |
| 24 | HoMM3 loader, differ, region and document/version construction gaps |
| 25-28 | Gaps in Core services and chunk/edit-session infrastructure; Wpf commands, navigation, hex rows; Heroes/Heroes.Wpf edit sessions and cards; both games' runtime and remaining UI. Two subjects per sub-slice |
| 29 | Remove demonstrably duplicated contracts, only after mapping each removed assertion to retained coverage |
| 30 | Refresh the full review and the reviewer agent from the final suite; separate intentional exceptions from defects |

Slice size limits: mechanical slices at most eight fixtures, isolation at most three subjects, gap-filling at most two new subjects. Split larger entries into lettered slices first.

The session plan with the full per-slice acceptance text is `C:\Users\dimit\.copilot\session-state\e0b3336f-09ab-4edd-9cd4-ce96f1b511c8\plan.md`; this document is its portable summary.

## Known weak spots and open items

- `HeroEditingTests.SecondarySkillsEditsSurviveSerialization` (HoMM3) does not serialize.
- Some HoMM2 editor `Assert.Ignore` / conditional checks and one map-object test name with an underscore remain.
- Category/folder consistency was verified once by script (slice 09); no test enforces it. A guard test is worth adding after the migrations end.
- Mutation was not checked for the new byte and rule tests (slices 10-12).
- `GoldLimitSaves` writer is `Manual`/`[Explicit]`; it has never been run.
- `SessionPageViewModelBaseTests` needs a real `SaveDocumentHistory` because `IEditSession.History` returns the sealed class.
- Core MultiUnit fixtures (`SaveDocument`, `RecordChunkCollection`, `ChunkPendingChange`) were not rewritten with mocks (slice 15 only handled the settings stores).
- Other castle-series fixtures (not `CastleBuilderReplayTests`) still load their own saves.
- Several types have no test class (most HoMM2 serializers/deserializers, some HoMM3 loaders/differs). A missing test file is not proof of a gap: confirm first.

## How to run

```powershell
dotnet test Tests\Siniskalc.Heroes.Wpf.Tests --artifacts-path $env:TEMP\siniskalc-x --filter "TestCategory=Unit"
dotnet test Tests\Siniskalc.Homm3.Tests --artifacts-path $env:TEMP\siniskalc-x --filter "FullyQualifiedName~CastleBuilder"
```

The WPF exe is often running and locks the normal output, so always pass `--artifacts-path`. Last known full HoMM3 run: 3198 non-Manual tests passed in about 10.5 minutes (slice 08).
