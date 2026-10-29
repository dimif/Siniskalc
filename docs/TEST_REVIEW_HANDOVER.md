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
| 18 | see git log | HoMM3: `EquippedArtifactsViewModelTests` reclassified MultiUnit (no `TestServices`, no save); `ArtifactFinderTests` split into MultiUnit (finder and equipment made by hand) and `ArtifactFinderHeroTests` (Integration, the hero page backpack and equip finders) |
| 18b | see git log | **Production seam (behaviour unchanged):** `IHistoryEvents` (Core) implemented by `SaveDocumentHistory`, `IEditSession<T>.Events`, `ResettablePageViewModelBase` listens to `IHistoryEvents`; `IHeroSession` (HoMM3) implemented by `HeroEditSession`, `HeroViewModel` takes it. Tests: `SessionPageViewModelBaseTests` is Unit (fake history), new `HeroViewModelTests` (MultiUnit, 16 cases on `FakeHeroSession`) |
| 19 | see git log | HoMM3: `MaxSpellPointsCalculatorTests`, `CastleModelTests` (moved to `Models\`) and the new `PlayerDifferTests` are Unit on records made by hand; the save-based player differ cases are `PlayerDifferSavegameTests` |
| 19b | see git log | HoMM3: `HeroModelTests` (moved to `Models\`) and the new `CastleDifferTests` are Unit on records made by hand (`HeroModels`, `CastleModels`, `PendingChangesFake`); `Homm3PendingChangesFactoryTests` gained the player, castle and obelisk cases; the save-based castle differ cases are `CastleDifferSavegameTests` |
| 20a | see git log | HoMM2: `Homm2EditorViewModelTests` (719 lines, 40 tests) split into `Homm2EditorNavigationTests`, `...OverviewTests`, `...PlayerPageTests`, `...HeroPageTests`, `...CastlePageTests`, `...HexTabTests` over a shared `Homm2EditorHarness`; `HeroDestinationAndFacingTests` split into `HeroDestinationTests` and `HeroFacingTests`; the underscore in a `MapObjectsTests` name removed |
| 20b | see git log | Names that said nothing about the behaviour, rewritten as sentences (15 methods in 7 fixtures: `HeroDeserializerTests`, `HotaSaveTests`, `CreatureInfoTests`, `GzipSavefileCodecTests`, `RawSavefileCodecTests`, `GrantedSpellsResolverTests`, `HeroByteEditTargetTests`); bodies unchanged |
| 20c | see git log | HoMM3: the six multi-subject fixtures split into 15 (66 tests, unchanged bodies): `SpellBookAndGrantsTests` to `SpellBookTests`, `ArtifactSpellGrantsTests`, `HeroArtifactBonusesTests`; `CastlePillAndTavernLinkTests` to `CastleHeroLinksTests`, `CastleGrailAndCapitolPillTests`; `CreatureFinderAndArtifactEffectsTests` to `CreatureFinderViewModelTests`, `ArtifactEffectsViewModelTests`; `PlayerAndOverviewViewModelTests` to `PlayerNavigationTests`, `PlayerViewModelTests`, `OverviewViewModelTests`; `CollectionAccessTests` to `HeroAndCastleLookupTests`, `SessionRevisionTests`, `PlayerCollectionTests`; `SkillsAndSpellsViewModelTests` to `SecondarySkillsViewModelTests`, `SpellsViewModelTests`; two vague names rewritten |
| 21 | see git log | HoMM2 castle and player boundaries as Unit tests on records made by hand: new `CastleDeserializerTests` (14 cases), `PlayerDeserializerTests` (5), `SafePlayerSerializerTests` (7), 6 more cases in `SafeCastleSerializerTests`; helper `BlankRecords` |
| 22a | see git log | HoMM2 mines and shrines: `MineDeserializerTests`, `SafeMineSerializerTests`, `ShrineDeserializerTests`, `SafeShrineSerializerTests` (Unit, `MapObjectChunk` records made by hand) |
| 22b | see git log | HoMM2 sphinxes and obelisks: `SphinxDeserializerTests`, `SafeSphinxSerializerTests`, `ObeliskDeserializerTests`, `SafeObeliskSerializerTests` (Unit, records made by hand) |
| 22c | see git log | HoMM2 turn and built-today: `TurnDeserializerTests`, `SafeTurnSerializerTests`, `BuiltTodayDeserializerTests`, `SafeBuiltTodaySerializerTests` (Unit, records made by hand) |
| 22d | see git log | HoMM2 `BytesLoaderTests` (11, Unit, a savegame made by hand with a synthetic `SaveLayout`) and `SaveRegionProviderTests` (5, MultiUnit, real collections over fakes) |
| 22e | see git log | HoMM2 `MapPlacesReaderTests` (6) and `MapObjectsReaderTests` (15), Unit, over a savegame made by hand (`MapSavegame`: tile table, mines, shrines, sphinx blocks, heroes on tiles). Slice 22 is complete |
| 23 | see git log | HoMM3 player and obelisk: `PlayerDeserializerTests` (6 cases x 2 layouts), `SafePlayerSerializerTests` (11 x 2), `ObeliskDeserializerTests` (4), `SafeObeliskSerializerTests` (5), Unit, records made by hand (`PlayerRecords`, `ByteDiff`); the save-based player deserializer cases are `PlayerDeserializerSavegameTests` |
| 24a | see git log | HoMM3 `HeroDifferTests` (15) and `HeroOwnersDifferTests` (1), Unit, on `HeroModels.Christian()` with the Moq factory (`PendingChangesFake` now also makes hero changes) |
| 24b | see git log | HoMM3 `MineDifferTests` (4) and `DwellingDifferTests` (4), Unit, records made by hand |
| 24c | see git log | HoMM3 `SavegameVersionDetectorTests` (24 cases, every edge of every range) and `MerchantStockDifferTests` (2), Unit |
| 23b | see git log | HoMM3 `PlayerBytesLoaderTests` (12 cases, both layouts; the save-based ones are `PlayerBytesLoaderSavegameTests`) and `ObeliskBytesLoaderTests` (9), Unit, savegames made by hand |

Each slice's details, timings and caveats are in its section of `docs\TEST_REVIEW.md`.

## Remaining plan (in order; dependencies are prerequisites, not permission to batch)

| ID | Scope |
|---|---|
| 17e | see git log | Decided per fixture. `CastleWeeklyGrowthTests`: the rule is now also stated on a castle made by hand (7 new Unit cases in `CastleModelTests`: growth of a built dwelling, the Well, the growth building on the first only, with and without the Well, an unbuilt dwelling, an upgraded one, no faction, out of range); the save-based facts stay Integration (the saves are the contract for the Well and the Farm). `HeroDestinationTests`, `MapObjectsTests`, `MinesListsTests` and `HeroActiveSpellsTests` stay Integration: the saves are the contract (the markers in MarkerFacing, the objects of Threedom and Arrax the Jerk written back byte for byte) or they need a `Game` the page does not take apart; the hand-made cases of the same readers and writers are the Unit tests of slice 22. Slice 17 is finished. |
| 18c | see git log | `HeroArmyTabTests` (6), `SpellsFromArtifactsViewModelTests` (7, real `GrantedSpellsResolver`) and `GeneralTabArtifactNotesTests` (4) on `FakeHeroSession`, MultiUnit; the three standalone tests of `ArtifactEffectsViewModelTests` moved into the MultiUnit `EquippedArtifactsViewModelTests` and the Integration file removed. |
| 18d | see git log | `SecondarySkillsPageTests` (13), `SpellsPageTests` (11) and `CreatureFinderPageTests` (8), MultiUnit, on `FakeHeroSession` (new `HeroPages.Create` builds the page). The save-based originals stay as Integration. |
| 18e | see git log | `HeroEditingPageTests` (21 cases, MultiUnit) from `HeroEditingViewModelTests` on `FakeHeroSession`. |
| 18f | see git log | Seam `ICastleSession` (implemented by `CastleEditSession`; `CastleViewModel` takes it; `IByteEditTargetFactory.ForCastle` takes the session) and `CastlePageTests` (19, MultiUnit) on `FakeCastleSession`. |
| 18g | see git log | `CastleDetailsPageTests` (22 cases, MultiUnit) from `CastleDetailsViewModelTests`; the garrisoned-hero cases of `CastleGarrisonedHeroTests` moved into `CastlePageTests` (1 new) and `CastleModelTests` (1 new, the rule). |
| 18h | see git log | `CastleLinksTests` (16 cases, MultiUnit): the castle links, hero links, Capitol and Grail pills and the settlement and fort text that the player page and the overview show, on a document made by hand. Not ported, and left as Integration on purpose: the tavern places, the overview players and the navigation badges. `PlayerViewModel` takes the concrete `PlayerEditSession` and builds the obelisk and map-object view models from the history, so a fake needs a wider seam than the hero and castle ones; the logic they show is now covered by `CastleLinksTests`, `CastleHeroRowsTests` and `OwnedLinks` |
| 19c | Remaining HoMM3 candidates: `MageGuildLevelTests` (the save is the contract; a Unit case on an in-memory castle could pin the rule), `CastleRegionProviderTests`, `HeroEditSessionTests`, `CastleEditSessionTests`; then the multi-subject save fixtures belong to slice 20 |
| 20d | see git log | Name scan re-run over all seven test projects (4 words or fewer, 5 words or fewer in the list; none has `_` or a `Test`/`Should` prefix). 19 telegraphic or vague names were rewritten as sentences (Core 4, Homm3 15); the other short names are already sentences about a behaviour and were left. `RampartAndBulwarkCastleTests` stays as a scenario fixture, which the rules allow. One name (`WriteTheGoldLimitSave`) is a helper used three times and was left. |
| 23c | see git log | HoMM3 `CastleBytesLoaderTests` (35) and `HeroBytesLoaderTests` (60), Unit, on savegames made by hand (they had no test of their own; `HotaSaveTests` and `BiographyTests` still read them from saves) |
| 24d-i | see git log | HoMM3 `MineRegionProviderTests`, `DwellingRegionProviderTests`, `PlayerRegionProviderTests` (both layouts) and `CastleRegionProviderTests`, Unit |
| 24d-ii | see git log | HoMM3 `HeroRegionProviderTests` (Unit; the save-based fixture is now `HeroRegionProviderSavegameTests`), `VersionSpecificEntitiesProviderTests` (provider and factory, Unit), `SaveDocumentFactoryTests` (MultiUnit) |
| 24d-iii | see git log | HoMM3 `PlayerSetupLocationTests`, `MapHeaderLocatorTests` and `MapObjectPositionsTests`, Unit, on bytes made by hand |
| 25a | see git log | Core: `BytesChunkTests` (12, Unit), `ChunkByteEditTargetTests` (13, MultiUnit) and `ByteEditTargetExtensionsTests` (3, MultiUnit) |
| 25b | see git log | Core: `ByteRegionTests` (9) and `ByteRegionMapTests` (11), Unit |
| 25c | see git log | Core: `SavegameServiceTests` (13 cases, Unit, parser, codec and backup are fakes) and `AbstractSavegameTests` (6, Unit); `Moq` added to `Siniskalc.Core.Tests` like the other test projects |
| 25d | see git log | Core: `ByteBufferHelperTests` (6), `SaveHexDumpTests` (3), `LogScopeExtensionsTests` (2), Unit. `ForwardingLogger` was already covered through `LogFactoryTests`. Core is finished. |
| 26a | see git log | Wpf: `NavNodeViewModelTests` (14) and `NavigationTreeTests` (19), Unit |
| 26b | see git log | Wpf: `EditorViewModelBaseTests` (30 cases, Unit; new `TestGame`, `TestEditor` and `TestNavigationBuilder` fakes) and `DelegateCommandTests` (6) |
| 26c | see git log | Wpf: `AmountRowViewModelTests` (13 cases) and `TextFieldViewModelTests` (10), Unit |
| 26d | see git log | Wpf: `ToggleRowViewModelTests` (7) and `ChoiceFieldViewModelTests` (7 cases), Unit |
| 26e | see git log | Wpf: `SortColumnViewModelTests` (4) and `RecentMenuItemViewModelTests` (10 cases), Unit (both were also exercised through `SortableListViewModelTests` and `RecentFilesMenuViewModelTests`) |
| 26f | see git log | Wpf: `PageViewModelBaseTests` (2) and `TabbedPageViewModelBaseTests` (3), Unit. Every view model of `Siniskalc.Wpf` is now named by a test. |
| 26g | Open: the views of Wpf. |
| 27a | see git log | Heroes: `ChangeSinkExtensionsTests` (16) and `ChangeTextTests` (3), Unit |
| 27b | see git log | Heroes: `MineKindsTests` (16 cases) and `MapSizesTests` (11), Unit |
| 27c | see git log | Heroes.Wpf: `StackTotalsTests` (10) and `StackSlotViewModelTests` (20), Unit |
| 27d | see git log | Heroes.Wpf: `ArmyViewModelBaseTests` (21, Unit; new `TestArmy` and `TestFinder` fakes) and `SkillSlotViewModelTests` (24 cases) |
| 27e | see git log | Heroes.Wpf: `SecondarySkillsViewModelBaseTests` (35 cases, Unit; new `TestSkills` fake; also covers `SkillRowViewModel`) and `SkillLevelNamesTests` (13 cases) |
| 27f | see git log | Heroes.Wpf: `SpellsViewModelBaseTests` (16, also covers `SpellChoiceViewModel`) and `NeutralPageViewModelBaseTests` (12), Unit |
| 27g | see git log | Heroes.Wpf: `OwnerNavigationBuilderBaseTests` (16, Unit; fake game and builder) and `ArtifactPickerViewModelTests` (9), Unit |
| 27h | see git log | Heroes.Wpf, one mechanical slice of eight small fixtures (Unit): `PlayerCardViewModelTests` (5), `PrimarySkillRowViewModelTests` (9 cases), `ResourceRowViewModelTests` (6), `ObeliskRowViewModelTests` (4), `ResourcePaletteTests` (13 cases), `ResettablePageViewModelBaseTests` (5), `HeroChoiceViewModelTests` (4) and `CastleHeroRowViewModelTests` (2). Left, pure holders of values: `HeroLinkViewModel`, `OrderOfPlayItemViewModel`, `NextBattleCardViewModel` (its refresh only passes on to its rows) |
| 28a | see git log | HoMM3: new Unit `SafeCastleSerializerTests` (26, hand-made record of zeros: heroes, Mage Guild level, counts and spells, creatures waiting moving between dwelling and upgrade, the three masks of buildings and companion bits, neutral castle); the savefile one is now `SafeCastleSerializerSavegameTests` (Integration) |
| 28b | see git log | HoMM3: new Unit `SafeMineSerializerTests` (7) and `SafeDwellingSerializerTests` (8) on records made by hand. |
| 28c | see git log | HoMM3: new Unit `SafeMerchantStockSerializerTests` (8 cases) and `MerchantStockDeserializerTests` (3), on lists of seven slots made by hand; every writer of HoMM3 now has a Unit test on literal bytes |
| 28d | see git log | HoMM3: new Unit `MineBytesLoaderTests` (23 cases) and `DwellingBytesLoaderTests` (21 cases) on bytes made by hand. Production fix (approved): both loaders rejected a tile at `Constants.Map.MaxCoordinate` (251) with `>=`, like `MapObjectPositions` before 24d-iii; they now accept it and 252 is the first refused. Left: `SavegameParser` |
| 28e | see git log | HoMM3: new Unit `MerchantBytesLoaderTests` (13 cases) and MultiUnit `MapPlacesLoaderTests` (18 cases, over the real `MapObjectPositions`), on bytes made by hand |
| 28 | Still open. Wpf views (hex editor 917 lines, hex viewer, hex rows and palette, finder view, stepper, nav button, shell window: most need a window, see what `ShellWindowTests` and `PageRenderingTests` already render); each game's runtime and remaining UI. Two subjects per sub-slice |
| 28f | see git log | HoMM3 view models, Unit/MultiUnit: `DwellingRowViewModelTests` (5), `ArmySlotViewModelTests` (5), `MineRowViewModelTests` (4), `MerchantViewModelTests` (6) |
| 28g | see git log | HoMM3 view models: `VisitedPlacesViewModelTests` (7), `MageGuildRowsTests` (5), `BackpackSlotViewModelTests` (4, MultiUnit), `MapDwellingRowViewModelTests` (6) |
| 28h | see git log | HoMM3 entities and view models: `SpellGrantProviderTests` (4), `ArtifactEffectInfoTests` (21), `ArtifactInfoViewModelBaseTests` (8, MultiUnit), `SpellFinderViewModelTests` (8), `BuildingRowsTests` (11) |
| 28i | see git log | HoMM3 chunk collections: `PlayerChunkCollectionTests` (3), `HeroChunkCollectionTests` (3), `CastleChunkCollectionTests` (2), `WarMachineOffsetsTests` (2), Unit with fake readers |
| 28j | see git log | HoMM3 runtime and data: `SavegameParserTests` (8, Unit with fakes) and `PlayerEditSessionTests` (7, Integration). Open: `StandingOn` of the parser |
| 28k | see git log | HoMM2 entities and small rows, Unit: `ArtifactBonusesTests` (3), `TownBuildingNamesTests` (7), `SpellRowViewModelTests` (3), `GuildLevelViewModelTests` (2), `DwellingRowViewModelTests` (2) |
| 28l | see git log | HoMM2 view models, Unit: `ShrineRowViewModelTests` (9), `SecondarySkillsViewModelTests` (3) |
| 28m | see git log | HoMM2 view models: `SpellFinderViewModelTests` (5, Unit) and `SphinxViewModelTests` (10, MultiUnit) |
| 29 | see git log | Removed six Integration fixtures duplicated by the MultiUnit ports of slices 18c to 18g (`SecondarySkillsViewModelTests`, `SpellsViewModelTests`, `CreatureFinderViewModelTests`, `CastleDetailsViewModelTests`, `HeroGeneralTabTests`, `SpellsFromArtifactsTests`) and 12 of the 14 tests of `HeroEditingViewModelTests`; each mapped to its retained test and checked by 32 mutants |
| 30 | see git log | `docs/TEST_REVIEW.md` rewritten from a fresh run (5,411 tests, 423 classes, all categorized; defects D1 to D9 separated from the intentional exceptions; the class table generated from the TRX files); `test-reviewer` agent updated in both places; fixed on the way: `Assume` and three `Assert.Ignore` guards, a test name ending in "StillWorks", the folder of `SphinxViewModelTests` |

Slice size limits: mechanical slices at most eight fixtures, isolation at most three subjects, gap-filling at most two new subjects. Split larger entries into lettered slices first.

The session plan with the full per-slice acceptance text is `C:\Users\dimit\.copilot\session-state\e0b3336f-09ab-4edd-9cd4-ce96f1b511c8\plan.md`; this document is its portable summary.

## Known weak spots and open items

The current list, with places and fixes, is "Findings" in `docs\TEST_REVIEW.md` (defects D1 to D9, questions about production behaviour and the intentional exceptions). The ones to start from:

- No test enforces that a fixture has a category and sits in the matching folder (D3); a script was the only check.
- Several tests load every save more than once (D2); the HoMM3 project takes about 7.5 minutes.
- `SavegameParser.StandingOn`, HoMM2 `PageViewModel` and the editor-session accessors are untested (D4, D5).
- The Unit tests of the HoMM3 hero and hero-owner serializers on records made by hand are missing (D9).
- Four questions about production behaviour are open (an obelisk record shorter than its count, `MovedToday` for every version, the "3 of 2" text of a visited place, a `null` `Effects`).
- Mutation checks were done per slice and are recorded in the slice log of the review; survivors that were left are listed there.

## How to run

```powershell
dotnet test Tests\Siniskalc.Heroes.Wpf.Tests --artifacts-path $env:TEMP\siniskalc-x --filter "TestCategory=Unit"
dotnet test Tests\Siniskalc.Homm3.Tests --artifacts-path $env:TEMP\siniskalc-x --filter "FullyQualifiedName~CastleBuilder"
```

The WPF exe is often running and locks the normal output, so always pass `--artifacts-path`. Last full run (slice 30): 5,411 tests passed; `Siniskalc.Homm3.Tests` 3,882 tests in 7 minutes 29 seconds. The projects must be run one by one with their own `LogFileName` to get a TRX file for each.
