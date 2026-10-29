# Test review

Reviewed on `dimif-test-suite-review` at e5c478d, after slices 01 to 29 of the cleanup (branch based on `feature/homm2-support`).
The first review of this tree (at dc14cef, 4,000 tests, 263 classes) is kept as history in the slice log at the end of this file; this text replaces its summary, tables and findings with measurements of the final suite.
The rules for kinds, folders and names that this review checks against are in the "Tests" section of `CLAUDE.md`.
The review is repeatable with the `test-reviewer` agent (`.github/agents/test-reviewer.agent.md`, `.claude/agents/test-reviewer.md`); "How this review was done" says which numbers come from a script.

## Summary

- **5,411 tests in 423 classes and 7 projects, all passing.** The test host time is about 5 minutes for `Siniskalc.Homm3.Tests` and under 20 seconds for all the others together; the sum of test durations is 335 s.
- **Every class has a kind and sits in the matching folder**: 207 Unit, 59 MultiUnit and 157 Integration classes, none without `[Category(TestKinds.X)]`. Four `[Explicit]` helpers carry the method-level label `Manual` and are never run.
- **Unit and MultiUnit tests are fast and need no savefile.** 1,527 Unit tests take 1.9 s and 433 MultiUnit tests 3.1 s in all. The only ones over 100 ms are WPF windows (intentional) and the first test of a class that creates a Moq proxy.
- **Integration tests carry the run time**: 3,451 tests, 330 s, 99% of it, almost all in the HoMM3 savefile series.
- **No `Assert.Pass`, `Assert.Inconclusive`, `Assert.Ignore`, `[Ignore]` or `Assume` is left**, and no test skips its assertions with `continue`, `return` or a guard (the `continue` left in a test is an allow-list in a layering check and a skip of existing files in a manual writer).
- **Every serializer of both games has tests that assert the literal bytes.** All but two have a Unit class on records made by hand (D9); the savefile round trips are separate Integration classes (`...SerializerSavegameTests`) and say so.
- **Every test is in the project of the class it tests**, and no shared project names a game type (`...LayeringTests` of each project enforce it).
- **Open**: the defects listed below, mostly gaps in coverage of thin or window-bound types.

### After the merge into feature/homm2-support

The cleanup was merged into `feature/homm2-support` (tip `ebf7b75`, 17 commits ahead of the first review: Open folder, Send to..., the dwelling buttons, the mines card, the Settings window, the new army facts, the "Pos:" position text). The class table and the run-time figures below were measured before the merge; the totals now are:

- **5,445 tests in 429 classes** (207 Unit, 59 MultiUnit, 163 Integration), all passing: Core 157, Heroes 180, Heroes.Wpf 379, Wpf 189, Homm2 636, Homm3 3,895, App 9.
- Eleven fixtures came with the branch and were categorized and placed in the merge: `DwellingStockActionsTests`, `SendToViewModelTests` (Unit), `HeroSendToTests`, `HeroNextBattleTests` (HoMM2) and `HeroSendToTests`, `DailyIncomeTests`, `HeroTabLayoutTests`, `OpenFolderTests`, `PlayerTabLayoutTests` (HoMM3) and `SettingsViewModelTests`, `SettingsWindowTests` (Wpf) as Integration, and `HexEditorTests` as MultiUnit.
- Tests of the cleanup that asserted wording the branch changed were updated: positions read "Pos: x, y, Surface", an army slot lists HP, Damage, Stack HP and Stack Damage, the resources are listed in game order with gold last, the toolbar has 6 icons and the menus 10. Two tests of the branch that had to move with the fixtures slice 29 removed went to the ports that kept their behaviour (`PlayerViewModelTests`, `Homm2EditorCastlePageTests`).
- `HeroActiveSpellsTests` of HoMM2 was deleted by the branch (replaced by `HeroNextBattleTests`) and stays deleted.

### Since the first review

| | First review (dc14cef) | Now |
|---|---:|---:|
| Tests | 4,000 | 5,411 |
| Classes | 263 | 423 |
| Classes with a kind | 0 | 423 |
| Sum of test durations | 804 s | 335 s |
| `Siniskalc.Homm3.Tests` | 3,186 tests, 775 s | 3,883 tests, 317 s |
| Tests that are Unit or MultiUnit | not counted | 1,960 (36%) |

Production changes made along the way, all with their own tests (details in the slice log):

- Seams that keep behaviour: `ISettingsStore` and `IRecentFilesStore` (slice 15), `IHistoryEvents` and `IHeroSession` (18b), `ICastleSession` (18f).
- Fixes: a map tile at the largest coordinate (251) was refused by `MapObjectPositions` (24d-iii) and by the finders of the mine and dwelling tables (28d).

## Findings

### Defects (open)

| # | Where | Problem | Fix |
|---|---|---|---|
| D1 | ~~`ObeliskVisitsTests`, HoMM2 `PlayerViewModelTests`, `ShellWindowTests`~~ | Fixed: they now assert the position of the second obelisk (34,58), the three castles of Blue, and the 5 toolbar and 9 menu icons that the window has | none |
| D2 | ~~every-save tests~~ | Fixed: the read-only every-save tests (15 load sites in 11 classes) now use `ReadOnlySaves`, so a save is parsed once per run. `CastleDeserializerTests` pays the one pass (about 24 s); the sum of the HoMM3 test durations fell from 449 s to 317 s. Left: `CastleBuilderReplayTests` (10 s, already shared) and the series tests that load each save per case | none |
| D3 | all projects | Nothing fails when a fixture has no `[Category]` or sits in the wrong folder (a MultiUnit fixture of slice 28m was placed wrongly and found only by this review) | One check test per project over its own assembly: category present, `Integration` and `MultiUnit` fixtures in namespaces that contain those folder names |
| D4 | HoMM3 `SavegameParser.StandingOn` | A hero standing on a place hides it on the map; its three conditions (type at +11, visitable at +15, number at +16) are untested | A parser test with a hero record that stands on a place |
| D5 | HoMM2 `PageViewModel`, `CastleLinks`, `Homm2SaveDocuments`, `Homm2EditSessions`; HoMM3 `Homm3SaveDocuments`, `Homm3EditSessions`, `ByteEditTargetFactory`, `MapHeaderLocation`, `DwellingDeserializer` | No test names them; they are thin and used by every Integration test, so only a wrong wiring would fail | Direct tests where a rule is in them (`PageViewModel`: reset and refresh) |
| D6 | Heroes.Wpf `HeroesSecondarySkillsViewModel`, `NextBattleCardViewModel`, `HeroLinkViewModel`, `OrderOfPlayItemViewModel`; Wpf `HexRowList`, `HexRow`, `HexPalette`, `NavButton`, `RegionNode`, `EditorCommands`, `WpfMessageService`, `WpfFileDialogs`, `AdvancedModeVisibility`; Core `ForwardingLogger` | No test names them (most of the Wpf ones need a window) | Test the ones with logic (`HexRowList`, `EditorCommands`); leave the rest to the rendered-view tests |
| D7 | `HeroAndCastleLookupTests` | Joins two subjects in one class | Split by subject |
| D9 | HoMM3 `SafeHeroSerializer` and `SafeHeroOwnersSerializer` | Their literal-byte tests are Integration (real saves); there is no Unit class on a hero or hero-owner record made by hand | `SafeHeroSerializerTests` and `SafeHeroOwnersSerializerTests` as Unit tests on records of zeros, the savefile ones renamed `...SavegameTests` |
| D8 | HoMM3 loaders | The end bound of the cell scan in `MapPlacesLoader` (`i + 10 <= firstDefinition`) and a few other mutants were left open on purpose (see the slice log) | None needed unless the layout changes |

Questions about production behaviour, not defects of the tests; each is written down where it was found:

- A record of obelisks shorter than its count makes the reader throw (slice 23b).
- `MovedToday` is listed for every game version in `HeroRegionProvider` (slice 24d-ii).
- The text of a visited-place row counts every bit of the hero's mask, so it can read "3 of 2" (slice 28g).
- `Effects` of an artifact with no text is `null` though typed `string` (slice 28h).

### Intentional exceptions

These look like findings and are not; a reviewer should not report them again.

| What | Why it stays |
|---|---|
| `[Explicit]` tests with `[Category(TestKinds.Manual)]` that write edited saves to `TestData/*/out` (HoMM2 `EditedSavesTests`, `EditorSavesTests`, `ThreedomEditorSavesTests`; HoMM3 `GoldLimitSavesTests`) | They exist to be opened in the game; a filter `TestCategory!=Manual` never runs them |
| MultiUnit view tests that open real WPF windows and take 100 to 400 ms (`PageStylesTests`, `DaysLeftCardTests`, `HeroCardsTests`) | A template that did not apply leaves a control empty and raises no binding error; only a rendered window shows it (`CLAUDE.md`) |
| Integration tests over 2 s that walk every save or replay a reducer over a series (15 classes) | They are data contracts over 448 saves; the cost is in loading the saves, which `ReadOnlySaves` now does once per run (D2) |
| Scenario classes `Roe*`, `Sod*`, `Hota*`, the faction `*CastleTests`, `RampartAndBulwarkCastleTests`, `CastleGrailAndCapitolPillTests` | `CLAUDE.md` allows a class named after a scenario or a feature tested through every layer |
| Savegame serializer tests (`...SerializerSavegameTests`) that use the reader to build or compare a model | They are the round-trip contract on a real save; the byte contract is in the Unit class of the same subject |
| HoMM2 fixtures of slice 17e (`HeroDestination`, `MapObjects`, `MinesLists`, `HeroActiveSpells`) kept Integration | The saves are the contract |
| HoMM3 tavern, overview and navigation-badge fixtures kept Integration | `PlayerViewModel` takes the concrete `PlayerEditSession`; `CastleLinksTests` covers their logic on a hand-made document |
| `CastleHeroLinksTests`, `CastleGrailAndCapitolPillTests`, `CastleGarrisonedHeroTests` | They test the player page, the overview and the navigation tree on a real save, which no port covers |
| Equivalent mutants documented in the slice log (for example the version filter of `SpellGrantProvider`, the outer scan bound of `MineBytesLoader`) | No behaviour differs, so no test can tell |
| The first test of a Unit class over 100 ms (`CastleChunkCollectionTests`, 0.25 s) | It creates the Moq proxies; later tests of the class are under 1 ms |
| Moq fakes of `IEditSession<T>` in the view-model tests | A fake session has no history, so "undo" there is "put the model back"; the history is tested with the real session in the Integration classes |

### Fixed during the cleanup

Slice numbers refer to the slice log below.

- Assertion-free and conditional tests (01, 02); in this review also the `Assume` of `CastleEditSessionTests`, three `Assert.Ignore` guards (two in HoMM2 `Homm2EditorNavigationTests`, one in HoMM3 `NavigationBuilderTests`, each now an assertion that the save has what the test needs), a test name that ended in "StillWorks", and the placement of `SphinxViewModelTests`.
- Serializer tests that went through the deserializer now assert literal bytes (10, 11, 17b, 21, 28a to 28c).
- Unit-shaped classes that loaded savefiles became Unit or MultiUnit tests on records made by hand (12, 16 to 19b, 22 to 24); the save-based cases remain as Integration classes with `Savegame` in their names.
- Multi-subject classes split and test names rewritten (20a to 20d).
- Gaps filled: both games' loaders, differs, region providers, view models, Core and Wpf types (22 to 28).
- Duplicates removed after the ports (29).

## Every test class

Generated from the TRX files of the run and the sources; "Reads" is a text search of the class for `TestServices`, saves, `TestData`, the disk and windows.

| Class | Folder | Kind | Tests | Total s | Max s | Reads |
|---|---|---|---:|---:|---:|---|
| `AppServicesTests` | Siniskalc.Tests/Integration | Integration | 4 | 0.28 | 0.18 | disk, saves |
| `LoggingTests` | Siniskalc.Tests/Integration | Integration | 5 | 0.16 | 0.1 | disk |
| `ByteBufferHelperTests` | Siniskalc.Core.Tests/Data | Unit | 6 | 0.01 | 0.0 |  |
| `ByteEnumTests` | Siniskalc.Core.Tests/Data | Unit | 3 | 0.0 | 0.0 |  |
| `ByteRegionMapTests` | Siniskalc.Core.Tests/Data | Unit | 11 | 0.01 | 0.0 |  |
| `ByteRegionTests` | Siniskalc.Core.Tests/Data | Unit | 9 | 0.0 | 0.0 |  |
| `BytesChunkTests` | Siniskalc.Core.Tests/Data | Unit | 12 | 0.01 | 0.0 |  |
| `ChangeSinkTests` | Siniskalc.Core.Tests/Data | Unit | 2 | 0.0 | 0.0 |  |
| `ChunkCollectionTests` | Siniskalc.Core.Tests/Data | Unit | 2 | 0.0 | 0.0 |  |
| `RecordChunkTests` | Siniskalc.Core.Tests/Data | Unit | 2 | 0.0 | 0.0 |  |
| `SaveHexDumpTests` | Siniskalc.Core.Tests/Data | Unit | 3 | 0.05 | 0.05 |  |
| `SavefileBytesPatcherTests` | Siniskalc.Core.Tests/Data | Unit | 3 | 0.0 | 0.0 |  |
| `VersionedEnumerationTests` | Siniskalc.Core.Tests/Entities | Unit | 2 | 0.01 | 0.0 |  |
| `DisabledFileBackupServiceTests` | Siniskalc.Core.Tests/Integration/Runtime | Integration | 2 | 0.0 | 0.0 | disk |
| `FileBackupServiceProviderTests` | Siniskalc.Core.Tests/Integration/Runtime | Integration | 3 | 0.02 | 0.02 | disk |
| `LogPathsTests` | Siniskalc.Core.Tests/Integration/Runtime | Integration | 8 | 0.0 | 0.0 | disk |
| `NumberedSavefileBackupTests` | Siniskalc.Core.Tests/Integration/Runtime | Integration | 5 | 0.01 | 0.01 | disk |
| `RawSavefileCodecTests` | Siniskalc.Core.Tests/Integration/Runtime | Integration | 3 | 0.02 | 0.01 | disk |
| `RecentFilesStoreTests` | Siniskalc.Core.Tests/Integration/Runtime | Integration | 5 | 0.02 | 0.01 | disk |
| `SettingsStoreTests` | Siniskalc.Core.Tests/Integration/Runtime | Integration | 8 | 0.03 | 0.01 | disk |
| `ShellUrlLauncherTests` | Siniskalc.Core.Tests/Integration/Runtime | Integration | 8 | 0.0 | 0.0 | WPF window, disk |
| `NumbersTests` | Siniskalc.Core.Tests/Models | Unit | 2 | 0.0 | 0.0 |  |
| `ByteEditTargetExtensionsTests` | Siniskalc.Core.Tests/MultiUnit/Data | MultiUnit | 3 | 0.0 | 0.0 |  |
| `ChunkByteEditTargetTests` | Siniskalc.Core.Tests/MultiUnit/Data | MultiUnit | 13 | 0.0 | 0.0 |  |
| `ChunkPendingChangeTests` | Siniskalc.Core.Tests/MultiUnit/Data | MultiUnit | 3 | 0.0 | 0.0 |  |
| `RecordChunkCollectionTests` | Siniskalc.Core.Tests/MultiUnit/Data | MultiUnit | 4 | 0.0 | 0.0 |  |
| `SaveDocumentTests` | Siniskalc.Core.Tests/MultiUnit/Data | MultiUnit | 7 | 0.0 | 0.0 |  |
| `RangeGuardTests` | Siniskalc.Core.Tests | Unit | 4 | 0.0 | 0.0 |  |
| `AbstractSavegameTests` | Siniskalc.Core.Tests/Runtime | Unit | 6 | 0.0 | 0.0 |  |
| `LogFactoryTests` | Siniskalc.Core.Tests/Runtime | Unit | 3 | 0.0 | 0.0 |  |
| `LogScopeExtensionsTests` | Siniskalc.Core.Tests/Runtime | Unit | 2 | 0.01 | 0.01 |  |
| `SavegameServiceTests` | Siniskalc.Core.Tests/Runtime | Unit | 13 | 0.03 | 0.01 | disk |
| `ArmyReaderTests` | Siniskalc.Heroes.Tests/Data | Unit | 4 | 0.01 | 0.01 |  |
| `ChangeSinkExtensionsTests` | Siniskalc.Heroes.Tests/Data | Unit | 16 | 0.02 | 0.0 |  |
| `ObeliskChunkCollectionTests` | Siniskalc.Heroes.Tests/Data | Unit | 4 | 0.04 | 0.03 |  |
| `ObeliskDifferTests` | Siniskalc.Heroes.Tests/Data | Unit | 4 | 0.02 | 0.01 |  |
| `MineKindsTests` | Siniskalc.Heroes.Tests/Entities | Unit | 16 | 0.0 | 0.0 |  |
| `ResourceTests` | Siniskalc.Heroes.Tests/Entities | Unit | 1 | 0.0 | 0.0 |  |
| `SkillLevelTests` | Siniskalc.Heroes.Tests/Entities | Unit | 2 | 0.0 | 0.0 |  |
| `HeroesLayeringTests` | Siniskalc.Heroes.Tests | Unit | 1 | 0.01 | 0.01 | WPF window |
| `ArmyTests` | Siniskalc.Heroes.Tests/Models | Unit | 7 | 0.0 | 0.0 |  |
| `BanishablePlayerBaseTests` | Siniskalc.Heroes.Tests/Models | Unit | 10 | 0.0 | 0.0 |  |
| `ChangeAreasTests` | Siniskalc.Heroes.Tests/Models | Unit | 1 | 0.0 | 0.0 |  |
| `ChangeTextTests` | Siniskalc.Heroes.Tests/Models | Unit | 3 | 0.0 | 0.0 |  |
| `ExperienceTableTests` | Siniskalc.Heroes.Tests/Models | Unit | 10 | 0.0 | 0.0 |  |
| `GameDateTests` | Siniskalc.Heroes.Tests/Models | Unit | 2 | 0.0 | 0.0 |  |
| `GameRuleExceptionTests` | Siniskalc.Heroes.Tests/Models | Unit | 1 | 0.0 | 0.0 |  |
| `GameSetupTests` | Siniskalc.Heroes.Tests/Models | Unit | 2 | 0.0 | 0.0 |  |
| `HeroPositionTests` | Siniskalc.Heroes.Tests/Models | Unit | 3 | 0.0 | 0.0 |  |
| `HeroSummaryTests` | Siniskalc.Heroes.Tests/Models | Unit | 1 | 0.0 | 0.0 |  |
| `MapPositionTests` | Siniskalc.Heroes.Tests/Models | Unit | 3 | 0.0 | 0.0 |  |
| `MapSizesTests` | Siniskalc.Heroes.Tests/Models | Unit | 11 | 0.0 | 0.0 |  |
| `MovementPointsTests` | Siniskalc.Heroes.Tests/Models | Unit | 1 | 0.0 | 0.0 |  |
| `ObeliskVisitsTests` | Siniskalc.Heroes.Tests/Models | Unit | 8 | 0.0 | 0.0 |  |
| `OwnerNamesTests` | Siniskalc.Heroes.Tests/Models | Unit | 3 | 0.0 | 0.0 |  |
| `PlayerModelExtensionsTests` | Siniskalc.Heroes.Tests/Models | Unit | 4 | 0.0 | 0.0 |  |
| `PrimarySkillsTests` | Siniskalc.Heroes.Tests/Models | Unit | 8 | 0.0 | 0.0 |  |
| `ResourceSetTests` | Siniskalc.Heroes.Tests/Models | Unit | 2 | 0.0 | 0.0 |  |
| `SpellSetTests` | Siniskalc.Heroes.Tests/Models | Unit | 3 | 0.01 | 0.0 |  |
| `TavernHeroesTests` | Siniskalc.Heroes.Tests/Models | Unit | 2 | 0.0 | 0.0 |  |
| `ArmyWriterTests` | Siniskalc.Heroes.Tests/MultiUnit/Data | MultiUnit | 8 | 0.0 | 0.0 |  |
| `ChangeSinkExtensionsTests` | Siniskalc.Heroes.Tests/MultiUnit/Data | MultiUnit | 18 | 0.01 | 0.0 |  |
| `HeroLevelTests` | Siniskalc.Heroes.Tests/MultiUnit/Models | MultiUnit | 2 | 0.0 | 0.0 |  |
| `HeroesSavegameBaseTests` | Siniskalc.Heroes.Tests/MultiUnit/Runtime | MultiUnit | 2 | 0.0 | 0.0 |  |
| `CreatureChoiceViewModelTests` | Siniskalc.Heroes.Tests/ViewModels | Unit | 4 | 0.01 | 0.01 |  |
| `MapLevelTextTests` | Siniskalc.Heroes.Tests/ViewModels | Unit | 2 | 0.0 | 0.0 |  |
| `OverviewTextTests` | Siniskalc.Heroes.Tests/ViewModels | Unit | 2 | 0.0 | 0.0 |  |
| `PlayerPaletteTests` | Siniskalc.Heroes.Tests/ViewModels | Unit | 3 | 0.0 | 0.0 |  |
| `PlayerTypeTextTests` | Siniskalc.Heroes.Tests/ViewModels | Unit | 2 | 0.0 | 0.0 |  |
| `PrimarySkillBonusTextTests` | Siniskalc.Heroes.Tests/ViewModels | Unit | 2 | 0.0 | 0.0 |  |
| `TurnTextTests` | Siniskalc.Heroes.Tests/ViewModels | Unit | 2 | 0.0 | 0.0 |  |
| `PageStylesTests` | Siniskalc.Heroes.Wpf.Tests/MultiUnit/Styles | MultiUnit | 11 | 0.4 | 0.4 | WPF window |
| `CastleHeroRowsTests` | Siniskalc.Heroes.Wpf.Tests/MultiUnit/ViewModels | MultiUnit | 5 | 0.07 | 0.06 |  |
| `HeroesArmyViewModelBaseTests` | Siniskalc.Heroes.Wpf.Tests/MultiUnit/ViewModels | MultiUnit | 7 | 0.02 | 0.01 |  |
| `MinesListViewModelTests` | Siniskalc.Heroes.Wpf.Tests/MultiUnit/ViewModels | MultiUnit | 7 | 0.02 | 0.01 |  |
| `ObeliskVisitsViewModelTests` | Siniskalc.Heroes.Wpf.Tests/MultiUnit/ViewModels | MultiUnit | 5 | 0.01 | 0.01 |  |
| `OwnedLinksTests` | Siniskalc.Heroes.Wpf.Tests/MultiUnit/ViewModels | MultiUnit | 5 | 0.03 | 0.02 |  |
| `OwnerNavGroupTests` | Siniskalc.Heroes.Wpf.Tests/MultiUnit/ViewModels | MultiUnit | 7 | 0.01 | 0.0 |  |
| `RecordsPageViewModelBaseTests` | Siniskalc.Heroes.Wpf.Tests/MultiUnit/ViewModels | MultiUnit | 2 | 0.01 | 0.01 |  |
| `TavernPlaceViewModelTests` | Siniskalc.Heroes.Wpf.Tests/MultiUnit/ViewModels | MultiUnit | 5 | 0.01 | 0.01 |  |
| `VisitedPlacesCardViewModelTests` | Siniskalc.Heroes.Wpf.Tests/MultiUnit/ViewModels | MultiUnit | 2 | 0.0 | 0.0 |  |
| `DaysLeftCardTests` | Siniskalc.Heroes.Wpf.Tests/MultiUnit/Views | MultiUnit | 3 | 0.27 | 0.11 | WPF window |
| `HeadedCardTests` | Siniskalc.Heroes.Wpf.Tests/MultiUnit/Views | MultiUnit | 1 | 0.08 | 0.08 | WPF window |
| `HeroCardsTests` | Siniskalc.Heroes.Wpf.Tests/MultiUnit/Views | MultiUnit | 5 | 0.46 | 0.11 | WPF window |
| `ItemsCardTests` | Siniskalc.Heroes.Wpf.Tests/MultiUnit/Views | MultiUnit | 2 | 0.15 | 0.07 | WPF window |
| `MinesCardTests` | Siniskalc.Heroes.Wpf.Tests/MultiUnit/Views | MultiUnit | 4 | 0.35 | 0.09 | WPF window |
| `ObeliskVisitsCardTests` | Siniskalc.Heroes.Wpf.Tests/MultiUnit/Views | MultiUnit | 3 | 0.09 | 0.08 | WPF window |
| `OwnerPickerTests` | Siniskalc.Heroes.Wpf.Tests/MultiUnit/Views | MultiUnit | 2 | 0.15 | 0.08 | WPF window |
| `PageContractTests` | Siniskalc.Heroes.Wpf.Tests/MultiUnit/Views | MultiUnit | 3 | 0.24 | 0.09 | WPF window |
| `PageHeaderTests` | Siniskalc.Heroes.Wpf.Tests/MultiUnit/Views | MultiUnit | 2 | 0.16 | 0.08 | WPF window |
| `ResourcesPanelTests` | Siniskalc.Heroes.Wpf.Tests/MultiUnit/Views | MultiUnit | 1 | 0.09 | 0.09 | WPF window |
| `ArmyTotalsTests` | Siniskalc.Heroes.Wpf.Tests/ViewModels | Unit | 3 | 0.0 | 0.0 |  |
| `ArmyViewModelBaseTests` | Siniskalc.Heroes.Wpf.Tests/ViewModels | Unit | 23 | 0.02 | 0.01 |  |
| `ArtifactPickerViewModelTests` | Siniskalc.Heroes.Wpf.Tests/ViewModels | Unit | 9 | 0.0 | 0.0 |  |
| `BanishingTextProviderTests` | Siniskalc.Heroes.Wpf.Tests/ViewModels | Unit | 4 | 0.0 | 0.0 |  |
| `CastleHeroRowViewModelTests` | Siniskalc.Heroes.Wpf.Tests/ViewModels | Unit | 2 | 0.0 | 0.0 |  |
| `CastleLinkViewModelTests` | Siniskalc.Heroes.Wpf.Tests/ViewModels | Unit | 3 | 0.0 | 0.0 |  |
| `DaysLeftCardViewModelTests` | Siniskalc.Heroes.Wpf.Tests/ViewModels | Unit | 6 | 0.0 | 0.0 |  |
| `HeroChoiceViewModelTests` | Siniskalc.Heroes.Wpf.Tests/ViewModels | Unit | 4 | 0.0 | 0.0 |  |
| `LevelAndClassCardViewModelTests` | Siniskalc.Heroes.Wpf.Tests/ViewModels | Unit | 4 | 0.0 | 0.0 |  |
| `MapPointCardViewModelTests` | Siniskalc.Heroes.Wpf.Tests/ViewModels | Unit | 4 | 0.0 | 0.0 |  |
| `MineRowViewModelBaseTests` | Siniskalc.Heroes.Wpf.Tests/ViewModels | Unit | 3 | 0.0 | 0.0 |  |
| `MineRowViewModelTests` | Siniskalc.Heroes.Wpf.Tests/ViewModels | Unit | 3 | 0.0 | 0.0 |  |
| `MovementAndSpellPointsCardViewModelTests` | Siniskalc.Heroes.Wpf.Tests/ViewModels | Unit | 4 | 0.0 | 0.0 |  |
| `NeutralPageViewModelBaseTests` | Siniskalc.Heroes.Wpf.Tests/ViewModels | Unit | 12 | 0.0 | 0.0 |  |
| `ObeliskRowViewModelTests` | Siniskalc.Heroes.Wpf.Tests/ViewModels | Unit | 4 | 0.0 | 0.0 |  |
| `OverviewViewModelBaseTests` | Siniskalc.Heroes.Wpf.Tests/ViewModels | Unit | 7 | 0.01 | 0.01 |  |
| `OwnerNavigationBuilderBaseTests` | Siniskalc.Heroes.Wpf.Tests/ViewModels | Unit | 16 | 0.01 | 0.0 |  |
| `PlayerCardViewModelTests` | Siniskalc.Heroes.Wpf.Tests/ViewModels | Unit | 5 | 0.0 | 0.0 |  |
| `PrimarySkillRowViewModelTests` | Siniskalc.Heroes.Wpf.Tests/ViewModels | Unit | 9 | 0.0 | 0.0 |  |
| `PrimarySkillsCardViewModelTests` | Siniskalc.Heroes.Wpf.Tests/ViewModels | Unit | 5 | 0.0 | 0.0 |  |
| `ResettablePageViewModelBaseTests` | Siniskalc.Heroes.Wpf.Tests/ViewModels | Unit | 5 | 0.0 | 0.0 |  |
| `ResourcePaletteTests` | Siniskalc.Heroes.Wpf.Tests/ViewModels | Unit | 10 | 0.0 | 0.0 |  |
| `ResourceRowViewModelTests` | Siniskalc.Heroes.Wpf.Tests/ViewModels | Unit | 6 | 0.0 | 0.0 |  |
| `ResourcesEditorViewModelTests` | Siniskalc.Heroes.Wpf.Tests/ViewModels | Unit | 6 | 0.0 | 0.0 |  |
| `SecondarySkillsViewModelBaseTests` | Siniskalc.Heroes.Wpf.Tests/ViewModels | Unit | 34 | 0.01 | 0.0 |  |
| `SessionPageViewModelBaseTests` | Siniskalc.Heroes.Wpf.Tests/ViewModels | Unit | 3 | 0.01 | 0.0 |  |
| `SkillLevelNamesTests` | Siniskalc.Heroes.Wpf.Tests/ViewModels | Unit | 14 | 0.0 | 0.0 |  |
| `SkillSlotViewModelTests` | Siniskalc.Heroes.Wpf.Tests/ViewModels | Unit | 21 | 0.0 | 0.0 |  |
| `SpellsViewModelBaseTests` | Siniskalc.Heroes.Wpf.Tests/ViewModels | Unit | 15 | 0.0 | 0.0 |  |
| `StackSlotViewModelTests` | Siniskalc.Heroes.Wpf.Tests/ViewModels | Unit | 20 | 0.02 | 0.01 |  |
| `StackTotalsTests` | Siniskalc.Heroes.Wpf.Tests/ViewModels | Unit | 10 | 0.0 | 0.0 |  |
| `TavernChoicesTests` | Siniskalc.Heroes.Wpf.Tests/ViewModels | Unit | 4 | 0.0 | 0.0 |  |
| `WeeklyGrowthTextTests` | Siniskalc.Heroes.Wpf.Tests/ViewModels | Unit | 2 | 0.0 | 0.0 |  |
| `BuiltTodayDeserializerTests` | Siniskalc.Homm2.Tests/Data | Unit | 3 | 0.01 | 0.01 |  |
| `BuiltTodayDifferTests` | Siniskalc.Homm2.Tests/Data | Unit | 2 | 0.01 | 0.01 |  |
| `BytesLoaderTests` | Siniskalc.Homm2.Tests/Data | Unit | 11 | 0.01 | 0.01 |  |
| `CastleDeserializerTests` | Siniskalc.Homm2.Tests/Data | Unit | 14 | 0.01 | 0.01 |  |
| `CastleDifferTests` | Siniskalc.Homm2.Tests/Data | Unit | 8 | 0.05 | 0.05 |  |
| `HeroDifferTests` | Siniskalc.Homm2.Tests/Data | Unit | 9 | 0.03 | 0.01 |  |
| `MapObjectsReaderTests` | Siniskalc.Homm2.Tests/Data | Unit | 15 | 0.01 | 0.0 |  |
| `MapPlacesReaderTests` | Siniskalc.Homm2.Tests/Data | Unit | 6 | 0.0 | 0.0 |  |
| `MineDeserializerTests` | Siniskalc.Homm2.Tests/Data | Unit | 4 | 0.0 | 0.0 |  |
| `MineDifferTests` | Siniskalc.Homm2.Tests/Data | Unit | 2 | 0.0 | 0.0 |  |
| `ObeliskDeserializerTests` | Siniskalc.Homm2.Tests/Data | Unit | 4 | 0.0 | 0.0 |  |
| `PlayerDeserializerTests` | Siniskalc.Homm2.Tests/Data | Unit | 5 | 0.01 | 0.01 |  |
| `PlayerDifferTests` | Siniskalc.Homm2.Tests/Data | Unit | 5 | 0.01 | 0.0 |  |
| `SafeBuiltTodaySerializerTests` | Siniskalc.Homm2.Tests/Data | Unit | 4 | 0.0 | 0.0 |  |
| `SafeCastleSerializerTests` | Siniskalc.Homm2.Tests/Data | Unit | 15 | 0.01 | 0.0 |  |
| `SafeHeroSerializerTests` | Siniskalc.Homm2.Tests/Data | Unit | 11 | 0.01 | 0.0 |  |
| `SafeMineSerializerTests` | Siniskalc.Homm2.Tests/Data | Unit | 7 | 0.0 | 0.0 |  |
| `SafeObeliskSerializerTests` | Siniskalc.Homm2.Tests/Data | Unit | 6 | 0.0 | 0.0 |  |
| `SafePlayerSerializerTests` | Siniskalc.Homm2.Tests/Data | Unit | 7 | 0.0 | 0.0 |  |
| `SafeShrineSerializerTests` | Siniskalc.Homm2.Tests/Data | Unit | 5 | 0.0 | 0.0 |  |
| `SafeSphinxSerializerTests` | Siniskalc.Homm2.Tests/Data | Unit | 9 | 0.0 | 0.0 |  |
| `SafeTurnSerializerTests` | Siniskalc.Homm2.Tests/Data | Unit | 4 | 0.0 | 0.0 |  |
| `ShrineDeserializerTests` | Siniskalc.Homm2.Tests/Data | Unit | 4 | 0.0 | 0.0 |  |
| `ShrineDifferTests` | Siniskalc.Homm2.Tests/Data | Unit | 3 | 0.0 | 0.0 |  |
| `SphinxDeserializerTests` | Siniskalc.Homm2.Tests/Data | Unit | 8 | 0.0 | 0.0 |  |
| `SphinxDifferTests` | Siniskalc.Homm2.Tests/Data | Unit | 2 | 0.0 | 0.0 |  |
| `TurnDeserializerTests` | Siniskalc.Homm2.Tests/Data | Unit | 4 | 0.0 | 0.0 |  |
| `TurnDifferTests` | Siniskalc.Homm2.Tests/Data | Unit | 4 | 0.0 | 0.0 |  |
| `ArtifactBonusesTests` | Siniskalc.Homm2.Tests/Entities | Unit | 3 | 0.0 | 0.0 |  |
| `CreatureTests` | Siniskalc.Homm2.Tests/Entities | Unit | 6 | 0.01 | 0.0 |  |
| `SpellTests` | Siniskalc.Homm2.Tests/Entities | Unit | 8 | 0.0 | 0.0 |  |
| `TownBuildingNamesTests` | Siniskalc.Homm2.Tests/Entities | Unit | 7 | 0.0 | 0.0 |  |
| `BuiltTodayTests` | Siniskalc.Homm2.Tests/Integration/Data | Integration | 7 | 0.03 | 0.01 | TestData, TestServices, saves |
| `ByteEditTargetFactoryTests` | Siniskalc.Homm2.Tests/Integration/Data | Integration | 4 | 0.01 | 0.0 | TestServices |
| `CastleDifferSavegameTests` | Siniskalc.Homm2.Tests/Integration/Data | Integration | 5 | 0.05 | 0.05 | TestServices |
| `CastleTests` | Siniskalc.Homm2.Tests/Integration/Data | Integration | 8 | 0.01 | 0.0 | TestData, TestServices |
| `EditedSavesTests` | Siniskalc.Homm2.Tests/Integration/Data | Integration | 7 | 0.05 | 0.02 | TestData, TestServices, disk, saves |
| `EditingTests` | Siniskalc.Homm2.Tests/Integration/Data | Integration | 10 | 0.01 | 0.0 | TestServices, saves |
| `EditorSavesTests` | Siniskalc.Homm2.Tests/Integration/Data | Integration | 7 | 0.29 | 0.25 | TestData, TestServices, disk, saves |
| `HeaderReaderTests` | Siniskalc.Homm2.Tests/Integration/Data | Integration | 21 | 0.01 | 0.0 | TestData, TestServices, saves |
| `HeroChunkCollectionTests` | Siniskalc.Homm2.Tests/Integration/Data | Integration | 2 | 0.02 | 0.02 | TestServices |
| `HeroDestinationTests` | Siniskalc.Homm2.Tests/Integration/Data | Integration | 11 | 0.02 | 0.0 | TestData, TestServices |
| `HeroDifferSavegameTests` | Siniskalc.Homm2.Tests/Integration/Data | Integration | 4 | 0.05 | 0.04 | TestServices |
| `HeroFacingTests` | Siniskalc.Homm2.Tests/Integration/Data | Integration | 5 | 0.12 | 0.11 | TestData, TestServices, saves |
| `HeroTests` | Siniskalc.Homm2.Tests/Integration/Data | Integration | 13 | 0.01 | 0.0 | TestData, TestServices |
| `MapObjectsTests` | Siniskalc.Homm2.Tests/Integration/Data | Integration | 12 | 0.14 | 0.08 | TestServices, saves |
| `ObeliskTests` | Siniskalc.Homm2.Tests/Integration/Data | Integration | 5 | 0.01 | 0.0 | TestData, TestServices |
| `PlayerControlTests` | Siniskalc.Homm2.Tests/Integration/Data | Integration | 30 | 0.22 | 0.08 | TestData, TestServices, disk, saves |
| `PlayerDifferSavegameTests` | Siniskalc.Homm2.Tests/Integration/Data | Integration | 4 | 0.08 | 0.04 | TestServices |
| `PlayerTests` | Siniskalc.Homm2.Tests/Integration/Data | Integration | 8 | 0.01 | 0.0 | TestData, TestServices |
| `RoundTripTests` | Siniskalc.Homm2.Tests/Integration/Data | Integration | 4 | 1.56 | 0.77 | TestServices, saves |
| `SafeCastleSerializerSavegameTests` | Siniskalc.Homm2.Tests/Integration/Data | Integration | 4 | 0.01 | 0.0 | TestServices, saves |
| `SafeHeroSerializerSavegameTests` | Siniskalc.Homm2.Tests/Integration/Data | Integration | 2 | 0.0 | 0.0 | TestServices |
| `SaveLayoutLocatorTests` | Siniskalc.Homm2.Tests/Integration/Data | Integration | 7 | 0.02 | 0.01 | saves |
| `SavegameParserTests` | Siniskalc.Homm2.Tests/Integration/Data | Integration | 5 | 2.55 | 2.52 | TestServices, disk, saves |
| `TestDataNamesTests` | Siniskalc.Homm2.Tests/Integration/Data | Integration | 5 | 0.01 | 0.0 | TestData, disk |
| `ThreedomEditorSavesTests` | Siniskalc.Homm2.Tests/Integration/Data | Integration | 7 | 0.15 | 0.14 | TestData, TestServices, disk, saves |
| `ThreedomTests` | Siniskalc.Homm2.Tests/Integration/Data | Integration | 33 | 0.06 | 0.02 | TestData, TestServices |
| `Homm2LayeringTests` | Siniskalc.Homm2.Tests/Integration | Integration | 2 | 0.0 | 0.0 | disk |
| `CastleModelSavegameTests` | Siniskalc.Homm2.Tests/Integration/Models | Integration | 5 | 0.0 | 0.0 | TestServices |
| `CastleWeeklyGrowthTests` | Siniskalc.Homm2.Tests/Integration/Models | Integration | 4 | 0.0 | 0.0 | TestData, TestServices |
| `HeroModelSavegameTests` | Siniskalc.Homm2.Tests/Integration/Models | Integration | 2 | 0.0 | 0.0 | TestServices |
| `ShellTests` | Siniskalc.Homm2.Tests/Integration | Integration | 6 | 6.74 | 6.62 | WPF window, disk, saves |
| `HeroActiveSpellsTests` | Siniskalc.Homm2.Tests/Integration/ViewModels | Integration | 1 | 0.0 | 0.0 | TestServices |
| `HeroVisitedPlacesTests` | Siniskalc.Homm2.Tests/Integration/ViewModels | Integration | 5 | 0.01 | 0.0 | TestServices |
| `Homm2EditorCastlePageTests` | Siniskalc.Homm2.Tests/Integration/ViewModels | Integration | 5 | 0.04 | 0.02 |  |
| `Homm2EditorHeroPageTests` | Siniskalc.Homm2.Tests/Integration/ViewModels | Integration | 10 | 0.06 | 0.01 | TestServices |
| `Homm2EditorHexTabTests` | Siniskalc.Homm2.Tests/Integration/ViewModels | Integration | 1 | 0.01 | 0.01 |  |
| `Homm2EditorNavigationTests` | Siniskalc.Homm2.Tests/Integration/ViewModels | Integration | 15 | 0.08 | 0.04 | TestServices |
| `Homm2EditorOverviewTests` | Siniskalc.Homm2.Tests/Integration/ViewModels | Integration | 2 | 0.0 | 0.0 |  |
| `Homm2EditorPlayerPageTests` | Siniskalc.Homm2.Tests/Integration/ViewModels | Integration | 7 | 0.04 | 0.01 | TestServices |
| `MapViewModelTests` | Siniskalc.Homm2.Tests/Integration/ViewModels | Integration | 6 | 0.01 | 0.0 | TestServices |
| `MinesListsTests` | Siniskalc.Homm2.Tests/Integration/ViewModels | Integration | 2 | 0.0 | 0.0 | TestServices |
| `PlayerViewModelTests` | Siniskalc.Homm2.Tests/Integration/ViewModels | Integration | 5 | 0.01 | 0.0 | TestServices |
| `SavegameFileCommandsTests` | Siniskalc.Homm2.Tests/Integration/ViewModels | Integration | 13 | 0.06 | 0.03 | disk, saves |
| `PageRenderingTests` | Siniskalc.Homm2.Tests/Integration/Views | Integration | 1 | 0.92 | 0.92 | WPF window, disk, saves |
| `CastleModelTests` | Siniskalc.Homm2.Tests/Models | Unit | 17 | 0.0 | 0.0 |  |
| `HeroModelTests` | Siniskalc.Homm2.Tests/Models | Unit | 12 | 0.01 | 0.0 |  |
| `MaxSpellPointsCalculatorTests` | Siniskalc.Homm2.Tests/Models | Unit | 2 | 0.0 | 0.0 |  |
| `SecondarySkillsTests` | Siniskalc.Homm2.Tests/Models | Unit | 5 | 0.0 | 0.0 |  |
| `SaveRegionProviderTests` | Siniskalc.Homm2.Tests/MultiUnit/Data | MultiUnit | 5 | 0.02 | 0.01 |  |
| `EntityTablesTests` | Siniskalc.Homm2.Tests/MultiUnit/Entities | MultiUnit | 21 | 0.01 | 0.0 |  |
| `SharedEntityTests` | Siniskalc.Homm2.Tests/MultiUnit/Entities | MultiUnit | 5 | 0.01 | 0.0 |  |
| `HeroLevelsTests` | Siniskalc.Homm2.Tests/MultiUnit/Models | MultiUnit | 7 | 0.0 | 0.0 |  |
| `ArmyViewModelTests` | Siniskalc.Homm2.Tests/MultiUnit/ViewModels | MultiUnit | 3 | 0.0 | 0.0 |  |
| `SphinxViewModelTests` | Siniskalc.Homm2.Tests/MultiUnit/ViewModels | MultiUnit | 10 | 0.01 | 0.0 |  |
| `DwellingRowViewModelTests` | Siniskalc.Homm2.Tests/ViewModels | Unit | 2 | 0.0 | 0.0 |  |
| `GuildLevelViewModelTests` | Siniskalc.Homm2.Tests/ViewModels | Unit | 2 | 0.0 | 0.0 |  |
| `SecondarySkillsViewModelTests` | Siniskalc.Homm2.Tests/ViewModels | Unit | 3 | 0.0 | 0.0 |  |
| `ShrineRowViewModelTests` | Siniskalc.Homm2.Tests/ViewModels | Unit | 8 | 0.01 | 0.0 |  |
| `SpellFinderViewModelTests` | Siniskalc.Homm2.Tests/ViewModels | Unit | 5 | 0.0 | 0.0 |  |
| `SpellRowViewModelTests` | Siniskalc.Homm2.Tests/ViewModels | Unit | 3 | 0.0 | 0.0 |  |
| `CastleBytesLoaderTests` | Siniskalc.Homm3.Tests/Data | Unit | 35 | 0.01 | 0.0 |  |
| `CastleChunkCollectionTests` | Siniskalc.Homm3.Tests/Data | Unit | 2 | 0.05 | 0.04 |  |
| `CastleDifferTests` | Siniskalc.Homm3.Tests/Data | Unit | 9 | 0.04 | 0.02 |  |
| `CastleRegionProviderTests` | Siniskalc.Homm3.Tests/Data | Unit | 8 | 0.01 | 0.0 |  |
| `DwellingBytesLoaderTests` | Siniskalc.Homm3.Tests/Data | Unit | 21 | 0.01 | 0.01 |  |
| `DwellingDifferTests` | Siniskalc.Homm3.Tests/Data | Unit | 4 | 0.0 | 0.0 |  |
| `DwellingRegionProviderTests` | Siniskalc.Homm3.Tests/Data | Unit | 4 | 0.0 | 0.0 |  |
| `HeroBytesLoaderTests` | Siniskalc.Homm3.Tests/Data | Unit | 60 | 0.03 | 0.01 |  |
| `HeroChunkCollectionTests` | Siniskalc.Homm3.Tests/Data | Unit | 3 | 0.02 | 0.01 |  |
| `HeroDifferTests` | Siniskalc.Homm3.Tests/Data | Unit | 15 | 0.05 | 0.03 |  |
| `HeroOffsetsTests` | Siniskalc.Homm3.Tests/Data | Unit | 7 | 0.0 | 0.0 |  |
| `HeroOwnersDeserializerTests` | Siniskalc.Homm3.Tests/Data | Unit | 1 | 0.0 | 0.0 |  |
| `HeroOwnersDifferTests` | Siniskalc.Homm3.Tests/Data | Unit | 1 | 0.0 | 0.0 |  |
| `HeroRegionProviderTests` | Siniskalc.Homm3.Tests/Data | Unit | 16 | 0.02 | 0.01 |  |
| `Homm3PendingChangesFactoryTests` | Siniskalc.Homm3.Tests/Data | Unit | 4 | 0.0 | 0.0 |  |
| `MapHeaderLocatorTests` | Siniskalc.Homm3.Tests/Data | Unit | 30 | 0.0 | 0.0 |  |
| `MapObjectPositionsTests` | Siniskalc.Homm3.Tests/Data | Unit | 16 | 0.0 | 0.0 |  |
| `MerchantBytesLoaderTests` | Siniskalc.Homm3.Tests/Data | Unit | 10 | 0.01 | 0.0 |  |
| `MerchantStockDeserializerTests` | Siniskalc.Homm3.Tests/Data | Unit | 3 | 0.0 | 0.0 |  |
| `MerchantStockDifferTests` | Siniskalc.Homm3.Tests/Data | Unit | 2 | 0.0 | 0.0 |  |
| `MineBytesLoaderTests` | Siniskalc.Homm3.Tests/Data | Unit | 27 | 0.0 | 0.0 |  |
| `MineDifferTests` | Siniskalc.Homm3.Tests/Data | Unit | 4 | 0.0 | 0.0 |  |
| `MineRegionProviderTests` | Siniskalc.Homm3.Tests/Data | Unit | 5 | 0.0 | 0.0 |  |
| `ObeliskBytesLoaderTests` | Siniskalc.Homm3.Tests/Data | Unit | 11 | 0.0 | 0.0 |  |
| `ObeliskDeserializerTests` | Siniskalc.Homm3.Tests/Data | Unit | 4 | 0.0 | 0.0 |  |
| `PlayerBytesLoaderTests` | Siniskalc.Homm3.Tests/Data | Unit | 12 | 0.01 | 0.0 |  |
| `PlayerChunkCollectionTests` | Siniskalc.Homm3.Tests/Data | Unit | 3 | 0.01 | 0.0 |  |
| `PlayerDeserializerTests` | Siniskalc.Homm3.Tests/Data | Unit | 12 | 0.0 | 0.0 |  |
| `PlayerDifferTests` | Siniskalc.Homm3.Tests/Data | Unit | 7 | 0.01 | 0.0 |  |
| `PlayerRegionProviderTests` | Siniskalc.Homm3.Tests/Data | Unit | 11 | 0.0 | 0.0 |  |
| `PlayerSetupLocationTests` | Siniskalc.Homm3.Tests/Data | Unit | 5 | 0.0 | 0.0 |  |
| `SafeCastleSerializerTests` | Siniskalc.Homm3.Tests/Data | Unit | 26 | 0.02 | 0.0 |  |
| `SafeDwellingSerializerTests` | Siniskalc.Homm3.Tests/Data | Unit | 8 | 0.0 | 0.0 |  |
| `SafeMerchantStockSerializerTests` | Siniskalc.Homm3.Tests/Data | Unit | 8 | 0.0 | 0.0 |  |
| `SafeMineSerializerTests` | Siniskalc.Homm3.Tests/Data | Unit | 7 | 0.0 | 0.0 |  |
| `SafeObeliskSerializerTests` | Siniskalc.Homm3.Tests/Data | Unit | 5 | 0.0 | 0.0 |  |
| `SafePlayerSerializerTests` | Siniskalc.Homm3.Tests/Data | Unit | 22 | 0.01 | 0.0 |  |
| `SavegameVersionDetectorTests` | Siniskalc.Homm3.Tests/Data | Unit | 24 | 0.0 | 0.0 |  |
| `VersionSpecificEntitiesProviderTests` | Siniskalc.Homm3.Tests/Data | Unit | 6 | 0.0 | 0.0 |  |
| `WarMachineOffsetsTests` | Siniskalc.Homm3.Tests/Data | Unit | 2 | 0.0 | 0.0 |  |
| `ArtifactEffectInfoTests` | Siniskalc.Homm3.Tests/Entities | Unit | 19 | 0.0 | 0.0 |  |
| `ArtifactTests` | Siniskalc.Homm3.Tests/Entities | Unit | 1 | 0.0 | 0.0 |  |
| `BuildingTests` | Siniskalc.Homm3.Tests/Entities | Unit | 9 | 0.02 | 0.01 |  |
| `CreatureInfoTests` | Siniskalc.Homm3.Tests/Entities | Unit | 4 | 0.0 | 0.0 |  |
| `SpellGrantProviderTests` | Siniskalc.Homm3.Tests/Entities | Unit | 4 | 0.0 | 0.0 |  |
| `ActiveSpellsTests` | Siniskalc.Homm3.Tests/Integration/Data | Integration | 12 | 1.5 | 0.26 | TestData, TestServices |
| `BiographyTests` | Siniskalc.Homm3.Tests/Integration/Data | Integration | 6 | 0.57 | 0.18 | TestData, TestServices |
| `CastleBuilderReplayTests` | Siniskalc.Homm3.Tests/Integration/Data | Integration | 4 | 10.2 | 10.12 | TestServices, saves |
| `CastleBuilderSavefileRulesTests` | Siniskalc.Homm3.Tests/Integration/Data | Integration | 7 | 0.33 | 0.32 | TestServices, saves |
| `CastleDeserializerTests` | Siniskalc.Homm3.Tests/Integration/Data | Integration | 10 | 24.79 | 24.22 | TestData, TestServices, disk, saves |
| `CastleDifferSavegameTests` | Siniskalc.Homm3.Tests/Integration/Data | Integration | 13 | 1.26 | 0.14 | saves |
| `CastleEditSessionTests` | Siniskalc.Homm3.Tests/Integration/Data | Integration | 7 | 0.78 | 0.16 | TestData, TestServices, disk |
| `CastleEditingTests` | Siniskalc.Homm3.Tests/Integration/Data | Integration | 11 | 1.64 | 0.28 | TestServices, saves |
| `CastleRegionProviderTests` | Siniskalc.Homm3.Tests/Integration/Data | Integration | 1 | 0.04 | 0.04 | TestData, TestServices |
| `CoveCastleTests` | Siniskalc.Homm3.Tests/Integration/Data | Integration | 16 | 3.89 | 0.95 | TestData, TestServices |
| `DwellingTests` | Siniskalc.Homm3.Tests/Integration/Data | Integration | 7 | 1.57 | 0.51 | TestData, TestServices, disk, saves |
| `FactoryCastleTests` | Siniskalc.Homm3.Tests/Integration/Data | Integration | 21 | 3.02 | 0.16 | TestData, TestServices |
| `GameDateLocatorTests` | Siniskalc.Homm3.Tests/Integration/Data | Integration | 16 | 4.43 | 2.38 | TestData, TestServices, saves |
| `GenderTests` | Siniskalc.Homm3.Tests/Integration/Data | Integration | 12 | 0.64 | 0.09 | TestData, TestServices |
| `GoldLimitSavesTests` | Siniskalc.Homm3.Tests/Integration/Data | Integration | 2 | 0.99 | 0.99 | TestData, TestServices, disk |
| `HeroAndCastleLookupTests` | Siniskalc.Homm3.Tests/Integration/Data | Integration | 3 | 0.24 | 0.08 | TestData, TestServices |
| `HeroByteEditTargetTests` | Siniskalc.Homm3.Tests/Integration/Data | Integration | 9 | 2.4 | 0.31 | TestData, TestServices |
| `HeroDeserializerTests` | Siniskalc.Homm3.Tests/Integration/Data | Integration | 5 | 0.01 | 0.0 | TestData, TestServices, disk |
| `HeroDestinationTests` | Siniskalc.Homm3.Tests/Integration/Data | Integration | 3 | 0.28 | 0.1 | TestData, TestServices |
| `HeroEditSessionTests` | Siniskalc.Homm3.Tests/Integration/Data | Integration | 3 | 0.0 | 0.0 | TestData, TestServices, disk |
| `HeroOwnerEditsTests` | Siniskalc.Homm3.Tests/Integration/Data | Integration | 18 | 2.47 | 0.23 | TestData, TestServices |
| `HeroOwnersBytesLoaderTests` | Siniskalc.Homm3.Tests/Integration/Data | Integration | 454 | 0.87 | 0.08 | TestData, TestServices, disk, saves |
| `HeroRegionProviderSavegameTests` | Siniskalc.Homm3.Tests/Integration/Data | Integration | 9 | 2.6 | 0.35 | TestData, TestServices |
| `HotaFourCornersTests` | Siniskalc.Homm3.Tests/Integration/Data | Integration | 2 | 0.27 | 0.13 | TestData, TestServices |
| `HotaSaveTests` | Siniskalc.Homm3.Tests/Integration/Data | Integration | 6 | 0.69 | 0.68 | TestData, TestServices, disk |
| `HotaSleepGarrisonVisitingTests` | Siniskalc.Homm3.Tests/Integration/Data | Integration | 30 | 2.07 | 0.33 | TestData, TestServices, saves |
| `MageGuildLevelTests` | Siniskalc.Homm3.Tests/Integration/Data | Integration | 12 | 0.86 | 0.08 | TestData, TestServices |
| `MageGuildSpellEditingTests` | Siniskalc.Homm3.Tests/Integration/Data | Integration | 10 | 1.26 | 0.24 | TestServices, saves |
| `MageGuildSpellsTests` | Siniskalc.Homm3.Tests/Integration/Data | Integration | 8 | 0.62 | 0.09 | TestData, TestServices |
| `MapDifficultyTests` | Siniskalc.Homm3.Tests/Integration/Data | Integration | 6 | 0.74 | 0.31 | TestData, TestServices |
| `MapHeaderTests` | Siniskalc.Homm3.Tests/Integration/Data | Integration | 8 | 1.01 | 0.28 | TestData, TestServices |
| `MapObjectByteEditTargetTests` | Siniskalc.Homm3.Tests/Integration/Data | Integration | 4 | 0.33 | 0.09 | TestData, TestServices |
| `MerchantStockTests` | Siniskalc.Homm3.Tests/Integration/Data | Integration | 5 | 0.31 | 0.08 | TestData, TestServices, disk, saves |
| `MineTests` | Siniskalc.Homm3.Tests/Integration/Data | Integration | 7 | 2.54 | 1.64 | TestData, TestServices, disk, saves |
| `MovedTodayTests` | Siniskalc.Homm3.Tests/Integration/Data | Integration | 13 | 1.14 | 0.18 | TestData, TestServices |
| `ObeliskVisitsTests` | Siniskalc.Homm3.Tests/Integration/Data | Integration | 465 | 12.34 | 1.9 | TestData, TestServices, disk, saves |
| `OtherFactionsCastleTests` | Siniskalc.Homm3.Tests/Integration/Data | Integration | 49 | 0.06 | 0.05 | TestData, TestServices, disk, saves |
| `PlayerBanishingTests` | Siniskalc.Homm3.Tests/Integration/Data | Integration | 25 | 2.69 | 0.38 | TestData, TestServices, disk, saves |
| `PlayerBytesLoaderSavegameTests` | Siniskalc.Homm3.Tests/Integration/Data | Integration | 7 | 0.52 | 0.08 | TestData, TestServices, disk |
| `PlayerCollectionTests` | Siniskalc.Homm3.Tests/Integration/Data | Integration | 1 | 0.26 | 0.26 | TestData, TestServices |
| `PlayerControlLocatorTests` | Siniskalc.Homm3.Tests/Integration/Data | Integration | 14 | 1.22 | 0.26 | TestData, TestServices |
| `PlayerDeserializerSavegameTests` | Siniskalc.Homm3.Tests/Integration/Data | Integration | 8 | 2.35 | 0.51 | TestData, TestServices |
| `PlayerDifferSavegameTests` | Siniskalc.Homm3.Tests/Integration/Data | Integration | 1 | 0.08 | 0.08 | TestData, TestServices, disk |
| `PlayerEditSessionTests` | Siniskalc.Homm3.Tests/Integration/Data | Integration | 7 | 0.56 | 0.11 | TestData, TestServices |
| `PlayerHeadTests` | Siniskalc.Homm3.Tests/Integration/Data | Integration | 460 | 0.93 | 0.24 | TestData, TestServices, disk, saves |
| `PlayerRegionProviderTests` | Siniskalc.Homm3.Tests/Integration/Data | Integration | 3 | 0.26 | 0.25 | TestData, TestServices |
| `PlayersInTheGameTests` | Siniskalc.Homm3.Tests/Integration/Data | Integration | 5 | 0.03 | 0.01 | TestData, TestServices |
| `RampartAndBulwarkCastleTests` | Siniskalc.Homm3.Tests/Integration/Data | Integration | 39 | 11.72 | 0.43 | TestData, TestServices |
| `RampartCastleTests` | Siniskalc.Homm3.Tests/Integration/Data | Integration | 31 | 5.0 | 0.21 | TestData, TestServices |
| `RoeBuildingEditsTests` | Siniskalc.Homm3.Tests/Integration/Data | Integration | 62 | 8.67 | 0.2 | TestData, TestServices, saves |
| `RoeCastleBuildingsTests` | Siniskalc.Homm3.Tests/Integration/Data | Integration | 146 | 47.48 | 1.33 | TestData, TestServices, disk, saves |
| `RoeCastleTests` | Siniskalc.Homm3.Tests/Integration/Data | Integration | 45 | 15.83 | 1.26 | TestData, TestServices |
| `RoeHeroTests` | Siniskalc.Homm3.Tests/Integration/Data | Integration | 16 | 2.8 | 1.07 | TestData, TestServices |
| `RoeMapStateTests` | Siniskalc.Homm3.Tests/Integration/Data | Integration | 32 | 4.01 | 0.55 | TestData, TestServices |
| `RoeSanyaMovementTests` | Siniskalc.Homm3.Tests/Integration/Data | Integration | 5 | 1.82 | 0.48 | TestData, TestServices |
| `SafeCastleSerializerSavegameTests` | Siniskalc.Homm3.Tests/Integration/Data | Integration | 15 | 3.77 | 2.6 | TestServices, saves |
| `SafeHeroOwnersSerializerTests` | Siniskalc.Homm3.Tests/Integration/Data | Integration | 452 | 0.02 | 0.0 | TestData, disk, saves |
| `SafeHeroSerializerTests` | Siniskalc.Homm3.Tests/Integration/Data | Integration | 27 | 0.01 | 0.0 | TestData, TestServices, disk |
| `SaveRegionProviderTests` | Siniskalc.Homm3.Tests/Integration/Data | Integration | 15 | 2.95 | 0.73 | TestData, TestServices |
| `SavefileDetectionTests` | Siniskalc.Homm3.Tests/Integration/Data | Integration | 21 | 1.78 | 0.36 | TestData, TestServices, disk |
| `SavefileRoundTripTests` | Siniskalc.Homm3.Tests/Integration/Data | Integration | 14 | 1.95 | 0.53 | TestData, TestServices |
| `SessionRevisionTests` | Siniskalc.Homm3.Tests/Integration/Data | Integration | 1 | 0.25 | 0.25 | TestData, TestServices |
| `SodHotseatTests` | Siniskalc.Homm3.Tests/Integration/Data | Integration | 12 | 1.97 | 0.33 | TestData, TestServices |
| `SodSaveTests` | Siniskalc.Homm3.Tests/Integration/Data | Integration | 11 | 0.6 | 0.13 | TestData, TestServices, disk |
| `SodSeriesTests` | Siniskalc.Homm3.Tests/Integration/Data | Integration | 92 | 14.74 | 2.33 | TestData, TestServices, disk, saves |
| `TestDataNamesTests` | Siniskalc.Homm3.Tests/Integration/Data | Integration | 10 | 0.01 | 0.0 | TestData, disk, saves |
| `VisitedBuildingEffectsTests` | Siniskalc.Homm3.Tests/Integration/Data | Integration | 12 | 1.44 | 0.34 | TestData, TestServices |
| `VisitedPlacesTests` | Siniskalc.Homm3.Tests/Integration/Data | Integration | 19 | 3.7 | 0.26 | TestData, TestServices, disk, saves |
| `Homm3LayeringTests` | Siniskalc.Homm3.Tests/Integration | Integration | 2 | 0.0 | 0.0 | disk |
| `ArtifactSpellGrantsTests` | Siniskalc.Homm3.Tests/Integration/Models | Integration | 6 | 0.22 | 0.12 | TestData, TestServices |
| `HeroArtifactBonusesTests` | Siniskalc.Homm3.Tests/Integration/Models | Integration | 2 | 0.19 | 0.1 | TestData, TestServices |
| `HeroEditingTests` | Siniskalc.Homm3.Tests/Integration/Models | Integration | 15 | 4.95 | 0.87 | TestData, TestServices |
| `HeroPrimarySkillsTests` | Siniskalc.Homm3.Tests/Integration/Models | Integration | 14 | 1.56 | 0.26 | TestData, TestServices |
| `SaveBaselineTests` | Siniskalc.Homm3.Tests/Integration/Models | Integration | 7 | 5.8 | 1.23 | TestServices, disk |
| `SpellBookTests` | Siniskalc.Homm3.Tests/Integration/Models | Integration | 3 | 0.0 | 0.0 |  |
| `GameServiceTests` | Siniskalc.Homm3.Tests/Integration/Runtime | Integration | 8 | 0.71 | 0.17 | TestData, TestServices, disk |
| `GzipSavefileCodecTests` | Siniskalc.Homm3.Tests/Integration/Runtime | Integration | 5 | 0.03 | 0.01 | disk |
| `SavegameParserTests` | Siniskalc.Homm3.Tests/Integration/Runtime | Integration | 2 | 0.0 | 0.0 | TestServices |
| `ArtifactFinderHeroTests` | Siniskalc.Homm3.Tests/Integration/ViewModels | Integration | 3 | 1.69 | 0.57 | TestServices |
| `BanishingViewModelTests` | Siniskalc.Homm3.Tests/Integration/ViewModels | Integration | 9 | 0.91 | 0.26 | TestData, TestServices, disk |
| `CastleEditingViewModelTests` | Siniskalc.Homm3.Tests/Integration/ViewModels | Integration | 10 | 12.38 | 11.55 | TestServices, WPF window, saves |
| `CastleGarrisonedHeroTests` | Siniskalc.Homm3.Tests/Integration/ViewModels | Integration | 4 | 0.3 | 0.08 | TestData, TestServices, WPF window |
| `CastleGrailAndCapitolPillTests` | Siniskalc.Homm3.Tests/Integration/ViewModels | Integration | 2 | 0.14 | 0.07 | TestData, TestServices |
| `CastleHeroLinksTests` | Siniskalc.Homm3.Tests/Integration/ViewModels | Integration | 3 | 0.21 | 0.07 | TestData, TestServices |
| `CastleViewModelTests` | Siniskalc.Homm3.Tests/Integration/ViewModels | Integration | 9 | 0.45 | 0.08 | TestData, TestServices |
| `HeroEditingViewModelTests` | Siniskalc.Homm3.Tests/Integration/ViewModels | Integration | 2 | 1.03 | 0.53 | TestServices |
| `HeroOwnerPickerTests` | Siniskalc.Homm3.Tests/Integration/ViewModels | Integration | 2 | 0.16 | 0.08 | TestData, TestServices |
| `MapViewModelTests` | Siniskalc.Homm3.Tests/Integration/ViewModels | Integration | 2 | 0.19 | 0.1 | TestData, TestServices |
| `NavigationBuilderTests` | Siniskalc.Homm3.Tests/Integration/ViewModels | Integration | 10 | 1.03 | 0.17 | TestData, TestServices |
| `OrderOfPlayTests` | Siniskalc.Homm3.Tests/Integration/ViewModels | Integration | 1 | 0.07 | 0.07 | TestData, TestServices |
| `OverviewViewModelTests` | Siniskalc.Homm3.Tests/Integration/ViewModels | Integration | 5 | 1.6 | 0.51 | TestData, TestServices |
| `OwnedMapObjectsViewModelTests` | Siniskalc.Homm3.Tests/Integration/ViewModels | Integration | 8 | 0.62 | 0.08 | TestData, TestServices |
| `PlayerNavigationTests` | Siniskalc.Homm3.Tests/Integration/ViewModels | Integration | 9 | 2.56 | 0.33 | TestData, TestServices |
| `PlayerViewModelTests` | Siniskalc.Homm3.Tests/Integration/ViewModels | Integration | 4 | 1.06 | 0.27 | TestData, TestServices |
| `SavegameFileCommandsTests` | Siniskalc.Homm3.Tests/Integration/ViewModels | Integration | 9 | 0.64 | 0.21 | TestData, TestServices, disk |
| `ShellSmokeTests` | Siniskalc.Homm3.Tests/Integration/ViewModels | Integration | 7 | 22.82 | 7.04 | TestData, TestServices, WPF window, disk |
| `ViewModelLayeringTests` | Siniskalc.Homm3.Tests/Integration/ViewModels | Integration | 1 | 0.01 | 0.01 | disk |
| `HexCardsTests` | Siniskalc.Homm3.Tests/Integration/Views | Integration | 1 | 3.39 | 3.39 | TestData, TestServices, WPF window |
| `PageRenderingTests` | Siniskalc.Homm3.Tests/Integration/Views | Integration | 3 | 22.32 | 14.71 | TestData, TestServices, WPF window |
| `CastleBuilderTests` | Siniskalc.Homm3.Tests/Models | Unit | 11 | 0.0 | 0.0 |  |
| `CastleModelTests` | Siniskalc.Homm3.Tests/Models | Unit | 3 | 0.0 | 0.0 |  |
| `ChangeAreaTests` | Siniskalc.Homm3.Tests/Models | Unit | 1 | 0.0 | 0.0 |  |
| `GrantedSpellsResolverTests` | Siniskalc.Homm3.Tests/Models | Unit | 1 | 0.0 | 0.0 |  |
| `HeroModelTests` | Siniskalc.Homm3.Tests/Models | Unit | 7 | 0.0 | 0.0 |  |
| `HeroOwnersModelTests` | Siniskalc.Homm3.Tests/Models | Unit | 4 | 0.0 | 0.0 |  |
| `MaxSpellPointsCalculatorTests` | Siniskalc.Homm3.Tests/Models | Unit | 2 | 0.0 | 0.0 |  |
| `PlayerModelTests` | Siniskalc.Homm3.Tests/Models | Unit | 10 | 0.0 | 0.0 | TestData |
| `ConstantsTests` | Siniskalc.Homm3.Tests/MultiUnit | MultiUnit | 3 | 0.0 | 0.0 |  |
| `DwellingCreaturesTests` | Siniskalc.Homm3.Tests/MultiUnit/Data | MultiUnit | 15 | 0.0 | 0.0 |  |
| `MapPlacesLoaderTests` | Siniskalc.Homm3.Tests/MultiUnit/Data | MultiUnit | 18 | 0.0 | 0.0 |  |
| `SaveDocumentFactoryTests` | Siniskalc.Homm3.Tests/MultiUnit/Data | MultiUnit | 8 | 0.02 | 0.01 |  |
| `EntityTablesTests` | Siniskalc.Homm3.Tests/MultiUnit/Entities | MultiUnit | 5 | 0.01 | 0.0 |  |
| `SharedEntityTests` | Siniskalc.Homm3.Tests/MultiUnit/Entities | MultiUnit | 5 | 0.0 | 0.0 |  |
| `ArtifactFinderTests` | Siniskalc.Homm3.Tests/MultiUnit/ViewModels | MultiUnit | 5 | 0.01 | 0.0 |  |
| `ArtifactInfoViewModelBaseTests` | Siniskalc.Homm3.Tests/MultiUnit/ViewModels | MultiUnit | 9 | 0.0 | 0.0 | TestServices |
| `BackpackSlotViewModelTests` | Siniskalc.Homm3.Tests/MultiUnit/ViewModels | MultiUnit | 4 | 0.0 | 0.0 | TestServices |
| `CastleBuildMessageProviderTests` | Siniskalc.Homm3.Tests/MultiUnit/ViewModels | MultiUnit | 5 | 0.0 | 0.0 |  |
| `CastleDetailsPageTests` | Siniskalc.Homm3.Tests/MultiUnit/ViewModels | MultiUnit | 22 | 0.02 | 0.0 |  |
| `CastleLinksTests` | Siniskalc.Homm3.Tests/MultiUnit/ViewModels | MultiUnit | 16 | 0.01 | 0.0 |  |
| `CastlePageTests` | Siniskalc.Homm3.Tests/MultiUnit/ViewModels | MultiUnit | 20 | 0.02 | 0.0 |  |
| `CreatureFinderPageTests` | Siniskalc.Homm3.Tests/MultiUnit/ViewModels | MultiUnit | 8 | 0.03 | 0.01 |  |
| `EquippedArtifactsViewModelTests` | Siniskalc.Homm3.Tests/MultiUnit/ViewModels | MultiUnit | 9 | 0.01 | 0.01 |  |
| `GeneralTabArtifactNotesTests` | Siniskalc.Homm3.Tests/MultiUnit/ViewModels | MultiUnit | 4 | 0.01 | 0.0 |  |
| `HeroArmyTabTests` | Siniskalc.Homm3.Tests/MultiUnit/ViewModels | MultiUnit | 6 | 0.01 | 0.0 |  |
| `HeroEditingPageTests` | Siniskalc.Homm3.Tests/MultiUnit/ViewModels | MultiUnit | 21 | 0.08 | 0.02 |  |
| `HeroViewModelTests` | Siniskalc.Homm3.Tests/MultiUnit/ViewModels | MultiUnit | 16 | 0.04 | 0.01 |  |
| `MerchantViewModelTests` | Siniskalc.Homm3.Tests/MultiUnit/ViewModels | MultiUnit | 6 | 0.02 | 0.01 | TestServices |
| `SecondarySkillsPageTests` | Siniskalc.Homm3.Tests/MultiUnit/ViewModels | MultiUnit | 13 | 0.04 | 0.01 |  |
| `SpellsFromArtifactsViewModelTests` | Siniskalc.Homm3.Tests/MultiUnit/ViewModels | MultiUnit | 7 | 0.02 | 0.01 |  |
| `SpellsPageTests` | Siniskalc.Homm3.Tests/MultiUnit/ViewModels | MultiUnit | 12 | 0.04 | 0.02 |  |
| `Homm3ModuleTests` | Siniskalc.Homm3.Tests/Runtime | Unit | 1 | 0.0 | 0.0 | TestServices |
| `SavegameParserTests` | Siniskalc.Homm3.Tests/Runtime | Unit | 8 | 0.67 | 0.09 | saves |
| `ArmySlotViewModelTests` | Siniskalc.Homm3.Tests/ViewModels | Unit | 5 | 0.0 | 0.0 |  |
| `BuildingRowsTests` | Siniskalc.Homm3.Tests/ViewModels | Unit | 11 | 0.0 | 0.0 |  |
| `DwellingRowViewModelTests` | Siniskalc.Homm3.Tests/ViewModels | Unit | 5 | 0.0 | 0.0 |  |
| `MageGuildRowsTests` | Siniskalc.Homm3.Tests/ViewModels | Unit | 5 | 0.0 | 0.0 |  |
| `MapDwellingRowViewModelTests` | Siniskalc.Homm3.Tests/ViewModels | Unit | 6 | 0.0 | 0.0 |  |
| `MineRowViewModelTests` | Siniskalc.Homm3.Tests/ViewModels | Unit | 4 | 0.0 | 0.0 |  |
| `SpellFinderViewModelTests` | Siniskalc.Homm3.Tests/ViewModels | Unit | 8 | 0.0 | 0.0 |  |
| `VisitedPlacesViewModelTests` | Siniskalc.Homm3.Tests/ViewModels | Unit | 7 | 0.0 | 0.0 |  |
| `LoggingSettingsViewModelTests` | Siniskalc.Wpf.Tests/Integration/ViewModels | Integration | 9 | 0.09 | 0.04 | disk |
| `ShellViewModelTests` | Siniskalc.Wpf.Tests/Integration/ViewModels | Integration | 17 | 0.06 | 0.02 | disk |
| `LoggingSettingsWindowTests` | Siniskalc.Wpf.Tests/Integration/Views | Integration | 1 | 0.36 | 0.36 | WPF window, disk |
| `ShellWindowTests` | Siniskalc.Wpf.Tests/Integration/Views | Integration | 1 | 0.16 | 0.16 | WPF window, disk |
| `ErrorWindowTests` | Siniskalc.Wpf.Tests/MultiUnit/Views | MultiUnit | 3 | 0.01 | 0.0 | WPF window |
| `AboutViewModelTests` | Siniskalc.Wpf.Tests/ViewModels | Unit | 2 | 0.0 | 0.0 |  |
| `AmountRowViewModelTests` | Siniskalc.Wpf.Tests/ViewModels | Unit | 13 | 0.0 | 0.0 |  |
| `ChoiceFieldViewModelTests` | Siniskalc.Wpf.Tests/ViewModels | Unit | 7 | 0.0 | 0.0 |  |
| `DelegateCommandTests` | Siniskalc.Wpf.Tests/ViewModels | Unit | 6 | 0.01 | 0.0 | WPF window |
| `EditorViewModelBaseTests` | Siniskalc.Wpf.Tests/ViewModels | Unit | 30 | 0.04 | 0.01 |  |
| `FinderViewModelTests` | Siniskalc.Wpf.Tests/ViewModels | Unit | 4 | 0.03 | 0.03 | WPF window |
| `NavNodeViewModelTests` | Siniskalc.Wpf.Tests/ViewModels | Unit | 15 | 0.0 | 0.0 |  |
| `NavigationTreeTests` | Siniskalc.Wpf.Tests/ViewModels | Unit | 18 | 0.01 | 0.0 |  |
| `PageViewModelBaseTests` | Siniskalc.Wpf.Tests/ViewModels | Unit | 2 | 0.0 | 0.0 |  |
| `RecentFilesMenuViewModelTests` | Siniskalc.Wpf.Tests/ViewModels | Unit | 6 | 0.01 | 0.0 |  |
| `RecentMenuItemViewModelTests` | Siniskalc.Wpf.Tests/ViewModels | Unit | 10 | 0.0 | 0.0 | WPF window |
| `SortColumnViewModelTests` | Siniskalc.Wpf.Tests/ViewModels | Unit | 4 | 0.0 | 0.0 |  |
| `SortableListViewModelTests` | Siniskalc.Wpf.Tests/ViewModels | Unit | 3 | 0.0 | 0.0 |  |
| `TabbedPageViewModelBaseTests` | Siniskalc.Wpf.Tests/ViewModels | Unit | 3 | 0.0 | 0.0 |  |
| `TextFieldViewModelTests` | Siniskalc.Wpf.Tests/ViewModels | Unit | 10 | 0.01 | 0.0 |  |
| `ToggleRowViewModelTests` | Siniskalc.Wpf.Tests/ViewModels | Unit | 7 | 0.0 | 0.0 |  |
| `HexFormatTests` | Siniskalc.Wpf.Tests/Views | Unit | 12 | 0.0 | 0.0 |  |

## Run time

Thresholds: a Unit or MultiUnit test should take under 100 ms and an Integration test under 2 s.

| Project | Classes | Tests | Test time (s) | Unit (classes / tests / s) | MultiUnit | Integration |
|---|---:|---:|---:|---|---|---|
| `Siniskalc.Tests` | 2 | 9 | 0.4 | 0 / 0 / 0.0 | 0 / 0 / 0.0 | 2 / 9 / 0.4 |
| `Siniskalc.Core.Tests` | 30 | 157 | 0.2 | 17 / 85 / 0.1 | 5 / 30 / 0.0 | 8 / 42 / 0.1 |
| `Siniskalc.Heroes.Tests` | 39 | 180 | 0.1 | 35 / 150 / 0.1 | 4 / 30 / 0.0 | 0 / 0 / 0.0 |
| `Siniskalc.Heroes.Wpf.Tests` | 53 | 362 | 2.7 | 33 / 280 / 0.1 | 20 / 82 / 2.6 | 0 / 0 / 0.0 |
| `Siniskalc.Homm2.Tests` | 92 | 637 | 13.7 | 42 / 264 / 0.2 | 6 / 51 / 0.1 | 44 / 322 / 13.5 |
| `Siniskalc.Homm3.Tests` | 185 | 3883 | 316.6 | 63 / 596 / 1.0 | 23 / 237 / 0.4 | 99 / 3050 / 315.2 |
| `Siniskalc.Wpf.Tests` | 22 | 183 | 0.8 | 17 / 152 / 0.1 | 1 / 3 / 0.0 | 4 / 28 / 0.7 |

- **No hidden I/O in a Unit or MultiUnit test**: none of them loads a save.
- **Where the time goes**: the 25 slowest classes take 81% of the HoMM3 time and all of them are savefile series or every-save contracts (D2).
- **Variation**: `Siniskalc.Homm3.Tests` took 48 minutes 45 seconds in one full run, 14 minutes 45 seconds in another and 5 to 7.5 minutes in the others, with the same tests; single tests were three times slower in the slow runs. The cause was not found (a smaller test run showed that keeping the loaded saves in memory is not it). Repeat a slow measurement before trusting it.
- **Waiting**: `Siniskalc.Homm3.Tests` can print nothing for a minute while one test loads every save; it is slow, not hung. Use `--blame-hang-timeout 20m` or more.

## Gaps

Production types, 25 lines or longer, that no test names by name (checked by script: no `<Type>Tests` class and the type is mentioned in no test file). Types that tests exercise only through a page or a session are not listed; D4 to D6 are the actionable ones.

| Project | Types |
|---|---|
| Core | `ForwardingLogger`, `ByteEnumerations` (used by every serializer) |
| Wpf | `HexRowList`, `NavButton`, `HexPalette`, `WpfMessageService`, `WpfFileDialogs`, `EditorCommands`, `HexRow`, `RegionNode`, `AdvancedModeVisibility` |
| Heroes | none |
| Heroes.Wpf | `HeroesSecondarySkillsViewModel` (through the games' pages), `NextBattleCardViewModel`, `HeroLinkViewModel`, `OrderOfPlayItemViewModel` |
| Homm2 | `RecordRegions` (through `SaveRegionProviderTests`), `HeroEditSession`, `CastleEditSession`, `PlayerEditSession` (through the three `...DifferSavegameTests`), `ArtifactsViewModel`, `SpellsViewModel` (through the hero and castle page tests), `PageViewModel`, `Homm2Module`, `Homm2SaveDocuments`, `MapPlaces`, `CastleLinks`, `Homm2EditSessions` |
| Homm3 | `Homm3SaveDocuments`, `BuildingType`, `Homm3EditSessions`, `ByteEditTargetFactory`, `ArtifactSlotViewModel` (through the merchant and backpack tests), `MapHeaderLocation`, `DwellingDeserializer` |
| App | `LoggingSetup` |

Rules from `CLAUDE.md` and their coverage:

- **An unknown value reads as a stand-in and the serializer keeps the byte**: tested for HoMM3 castles, heroes, hero owners, mines, dwellings and merchants, and for the Core and Heroes types behind them. HoMM3 players and obelisks and HoMM2 castles, players and map objects have it only in part.
- **Malformed input**: the finders of both games are tested with short, cut and wrong bytes; the savefile parsers are still tested only with empty and random bytes.
- **Reading and writing a chunk are independent**: enforced by the layering tests and by the Unit serializer tests, which never call a deserializer.

## Conventions the suite now follows

1. `[Category(TestKinds.Unit|MultiUnit|Integration)]` on every fixture, one `TestKinds` class per project; `Integration/<Layer>/` and `MultiUnit/<Layer>/` folders, Unit tests in the layer folder (D3 asks for a check).
2. Moq fakes for every dependency of a Unit test; `TestServices` only in MultiUnit and Integration tests; a seam is added to production code only when it keeps behaviour and the user agrees.
3. Savefile tests only read; a save is loaded once per run where many tests need it (`ReadOnlySaves`).
4. No `Assert.Pass`, no silent `continue`, no conditional assert, no `Assume`; a serializer test asserts bytes.
5. A test checks the notification, not only the value, when the getter reads the model anyway (a `Refresh` test that watches `PropertyChanged`); this found several weak tests in slices 27 and 28.
6. A claim that a test covers a behaviour is checked by changing the production code and watching the test fail (mutation checks, kept in the slice log with their survivors).
7. A test class per subject, named `<Subject>Tests`; methods are sentences that state the behaviour.

## Backlog (in order of value)

1. D3: a check test per project for category and folder.
2. The series tests that load each save per case (`RoeCastleTests`, `RoeCastleBuildingsTests`, `OtherFactionsCastleTests`) could use `ReadOnlySaves` too; D2 is fixed for the every-save loops.
3. D4 and D5: the parser's `StandingOn`, `PageViewModel` and the editor-session accessors.
4. D1: strengthen the four weak assertions.
5. D6, D7: the remaining untested view models and the split of `HeroAndCastleLookupTests`.
6. Decide the four questions about production behaviour above.

## How this review was done

1. `dotnet test` of each project with `--logger "trx;LogFileName=<project>.trx"` and the artifacts outside the repo (the solution run overwrites one file for all projects, so run them one by one); `Siniskalc.Homm3.Tests` with `--blame-hang-timeout 20m`.
2. A script parsed the TRX files (`UnitTestResult/@duration` joined to `UnitTest/TestMethod/@className`) into the per-class table; the kind comes from the `[Category]` of the class and the folder from its source file.
3. A script checked every fixture for a category, the folder against the kind, the markers `[Explicit]`, `Assert.Pass`, `Assert.Inconclusive`, `Assert.Ignore`, `Assert.Warn`, `[Ignore]` and `Assume.`, and for `continue;` or `return;` after an `if`, method names (underscores, `Test`/`Should` prefixes, names ending in "Works" or shorter than 14 characters) and class names containing `And`.
4. A script listed the production types of 25 lines or more that no test class and no test file names.
5. The weak assertions were found by searching for `Is.Not.Null)` and `Is.Not.Empty)` (59 hits). The hits in the files that the first review named were read, and 4 remain weak; the other hits were not read one by one.
6. Findings of the first review were checked again at their lines; the ones that no longer exist are not repeated.

# Slice log

What follows is the record of each slice, newest last; the numbers in it were true when it was written.

## Incremental fixes after the first review (slices 01 to 07)

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

## Slice 24b - HoMM3 `MineDiffer` and `DwellingDiffer`

- `MineDifferTests` (4, Unit): equal mines; a new owner as one change named by the mine with its level ("... Underground (1)") and filed under the mines; a stack of the guard as a change of its slot under the garrison; the owner before the guard. `DwellingDifferTests` (4, Unit): equal dwellings; a new owner named by the dwelling and filed under the dwellings; the creatures to hire as one change; a dwelling of a creature the game does not have is named by its code ("Creature 200 dwelling ...").
- These differs make their changes through `ChunkPendingChange` directly, so no fake factory is needed.
- Mutation check (all caught, production restored): the guard not listed, the wrong chunk kind, the guard before the owner, the stock not listed, the wording of the stock change.
- Homm3.Tests: the new fixtures pass (8); no other tests were run in this step; no production code changed.

## Slice 24c - HoMM3 version detection and the merchant differ

- `SavegameVersionDetectorTests` (24 cases, Unit) on headers made by hand: the signature (`H3SVG`, the campaign signature `H3SVC`, and the Heroes Chronicles signature, which is enough by itself), and the major and minor numbers of the format. Stated at both edges of each range: 16 to 41 with minor 0 is Restoration of Erathia, 42.1 is Armageddon's Blade, 42 and 43 with a minor up to 4 are Shadow of Death (42.0 included), 44 and up with a minor of 5 or more is Horn of the Abyss; the pairs between the ranges (15.0, 16.1, 41.1, 43.5, 44.4, 0.0) are an unknown game, as is a signature of no Heroes game and bytes too short to hold the numbers (and the shortest length that holds them is detected).
- `MerchantStockDifferTests` (2, Unit): equal stocks, and a changed slot listed by its place in the shop (from one) with "(sold)" for an empty one.
- Mutation check (all caught, production restored): each edge of each range moved by one, the campaign signature refused, the length check moved, the Chronicles signature ignored, the wording of a sold slot and its numbering.
- Homm3.Tests: the new fixtures pass (26); no production code changed.

## Slice 23b - HoMM3 `PlayerBytesLoader` and `ObeliskBytesLoader`

- `PlayerBytesLoaderTests` (12 cases, Unit; the save-based fixture is now `PlayerBytesLoaderSavegameTests`) on savegames of zeros with a table of players made by hand, for both layouts: a version with no known layout has no table; a table of eight gives a chunk for each player in play with its colour, place and the size of a record; only the players with resources, heroes or towns are kept, and all of them when none is; a record with no colour byte takes the colour after the previous one; a run of fewer than two records is no table; the longest run is the table; a record with a negative resource is no player and ends its run (the longest of the remaining runs wins); another chunk that starts right after the table leaves the last player whole, and one that starts inside the last player takes its colour byte away and the player is left out.
- `ObeliskBytesLoaderTests` (9 cases, Unit): no players, no record; the record is the marker FF 00 00, a count and one byte of flags for each obelisk at a fixed distance before the record of Red (found one record for each colour earlier when the first player is not Red); a count of 0, of more than 48 or a byte pattern that is not the marker is no record; 48 is a record; a table too near the start of the savegame has none; without the table of the objects of the map the obelisks have no tiles.
- Two branches cannot be reached with the layouts and constants of today, so they have no test: the player table cutting its last record short where another chunk starts (the colour byte is the last byte of a record in both layouts, so any overlap takes it away), and the obelisk record running into the record of Red (4 + 48 bytes always fit the 53 before Red). Both stay in the code as guards.
- Mutation check (all caught, production restored): the smallest run, the longest run, the in-play filter and each of its three rules, negative resources, the colour after the previous, the other chunks, a version without a layout, the offset of Red from the first colour, the limits of the count, the marker bytes and the start of the savegame.
- Homm3.Tests: the Unit, MultiUnit and the renamed save-based fixture pass (the count is in the report); the full HoMM3 project was not run; no production code changed.
- Not done (23c): `HeroBytesLoader` and `CastleBytesLoader`, and what a record of obelisks shorter than its count does.

## Slice 24d-i - HoMM3 region providers of mine, dwelling, player and castle

- `MineRegionProviderTests` (5), `DwellingRegionProviderTests` (4), `PlayerRegionProviderTests` (7 cases over both layouts) and `CastleRegionProviderTests` (8), all Unit, on records of zeros: each states the name, start and length of every region the hex editor is given, in the order of the bytes. The parts of a slot list (guard, garrison, dwellings, spell counts, resources) are stated, no byte belongs to two fields, and a record or chunk cut short leaves out the fields that would run past its end. Castle: the fields after the name are counted from its end, so a longer name moves them; a castle with no name has no name region.
- Mutation check (all caught, production restored): a moved owner offset, the cut-short rule off by one, a moved days-left offset, spell counts counted from the start of the name instead of its end, and empty regions kept.
- Homm3.Tests: the `RegionProviderTests` fixtures pass (56, including the existing ones); no production code changed.
- Not done (rest of 24d): `HeroRegionProvider`, `SaveDocumentFactory`, `VersionSpecificEntitiesProvider(Factory)`, `PlayerSetupLocation`, `MapHeaderLocator`, `MapObjectPositions`.

## Slice 24d-ii - HoMM3 hero regions, version entities and the document factory

- `HeroRegionProviderTests` (Unit, no savegame; the save-based fixture is now `HeroRegionProviderSavegameTests`): the fixed fields of a Shadow of Death hero at their bytes, the skill and spell tables (one byte per code up to the highest of the version, skills named at their code), the equipment slots and the 64-slot backpack, the fifth side slot and the backpack offset per version, the cannon that shares the ballista's bytes and exists only in Horn of the Abyss, the asleep byte (Shadow of Death and Horn of the Abyss only), the nine reserved slot counters, the active spell and visit effect bytes, the order of the regions, the cut-short rule at both edges (a region ending exactly at the end is kept; the asleep byte and the reserved counters have their own edge), and the header regions.
- `VersionSpecificEntitiesProviderTests` (Unit): the version and resolver are kept; only Horn of the Abyss has the cannon and more than the 28 skills of Shadow of Death; the lists hold only what is available in the version; the factory makes one provider (and calls the resolver factory once) for each version and gives each its own resolver.
- `SaveDocumentFactoryTests` (MultiUnit, fake serializers, real collections): an empty collection of every kind, the chunks given reach the collection of their kind, the game version is the one of the offsets, the hero reader uses the skills of that version (a Horn of the Abyss skill is read), and the differ of the players is given the hero name by id, or "Hero N" when no hero has that id.
- Mutation check (all caught, production restored): the cannon rule, the fifth slot rule, the cut-short rule (and the asleep and reserved edges), skill and spell byte counts, the name length, the order of regions, the biography length of the header, the order of skill parts, the cache of the factory, the filters of skills and war machines, the hero name fallback, the entities of the wrong version, and the mine collection left empty.
- Not testable: the filter of spells by version is a no-op with today's data (every spell exists in every version), so removing it changes nothing. `MovedToday` is listed for every version although its comment says Shadow of Death has no such byte; not changed or tested.
- Homm3.Tests: the new fixtures and the renamed one pass (39); no production code changed.
- Not done (rest of 24d): `PlayerSetupLocation`, `MapHeaderLocator`, `MapObjectPositions`.

## Slice 24d-iii - HoMM3 setup location, map header and map object positions

- `PlayerSetupLocationTests` (5): the difficulty byte after the eight colour bytes, the current player after the eight 32-bit human flags, when the flags count as found, and the marker length (2 for the default distance of Horn of the Abyss, 4 for the classic one).
- `MapHeaderLocatorTests` (30 cases): on headers made by hand (width, underground byte, name, description, difficulty): the five difficulties, a difficulty above King refused, the width five bytes before the name and the underground byte before it, widths outside 1 to 252 read as no size, a name at the very start (no room for the size), the limits of the name (1 to 60), text rules (printable, line breaks allowed in the description), a description running past the end, the 200-byte search window at both edges, versions with no game setup (unknown, Chronicles) and the four that have one, and Locate's offsets. The padding after the header is 0xFF in the test data: zeros after a header can be read as a second header (the description taken for a name), which is how the heuristic works and is not stated as a rule.
- `MapObjectPositionsTests` (16 cases): the positions in list order, five definitions or more needed, a count that bounds the list, the search end at both edges, a list ending at the first entry with z above 1 or x or y of 252 or more, the underground level, a definition name of 20 bytes accepted and 21 refused, a control character refused, and capital letters in later definition names.
- Mutation check (all caught, production restored): 29 changes to constants, comparisons and rules of the three classes. Survivors, all equivalent or deliberate: the width "greater than 0" (a width of 0 reads as 0 anyway), the minimum definition name length (a name under 4 bytes cannot end in .def), and the x or y bound below.
- Fixed in a follow-up: `MapObjectPositions` ended the list at x or y of 251 (`>= MaxCoordinate`, where `MaxCoordinate` is 251, the last tile of a 252 map), so an object on the last row or column cut the list short. It is now `>`: 251 is a tile and 252 ends the list. The tests state both.
- Homm3.Tests: the new fixtures pass (51); no production code changed.

## Slice 23c - HoMM3 `CastleBytesLoader` and `HeroBytesLoader`

- `CastleBytesLoaderTests` (35 cases, Unit) on savegames of zeros with castle records made by hand: the chunk (name, place, length to the end of the spell lists, or shorter when the savegame stops there; none when it stops before the masks), the order of several castles, and every check of the shape at both edges: owner (a colour or none), built this turn (0 or 1), faction (up to the highest code, 11), mage guild level (up to 5) and the zero byte after it, name length (1 to 48) and printable characters, creature types (none or up to 200) and counts (not negative), and guild spell counts (up to 8). A castle-like pattern inside the spell lists of another is not found.
- `HeroBytesLoaderTests` (60 cases, Unit): structs made by hand for the five shapes (Restoration of Erathia 1064 bytes, Armageddon's Blade 1072, Chronicles 1082, Shadow of Death 1094, Horn of the Abyss 1148, or 1147 in the older saves): the chunk, none for an unknown version, for 30000 bytes or less, or for a struct cut short; carry-over heroes before byte 30000 skipped (30000 is found, 29999 is not); later heroes searched within 5000 bytes of the end of the last (counting the hero's own length); the checks of the shape at their edges (skills used, name, skill levels and slots, spell flags, equipment and backpack slots, the combination tail, the Horn of the Abyss skill slots after the gap); and the biography: its text before the faction byte with the length four bytes before it, the header in front, a length that disagrees, and a text that is no text in the encoding.
- Mutation check (all caught, production restored): about 40 changes. Survivors, all equivalent: the length check `<= limit` (the search bound already includes the tail), the start of the savegame `<=` 30000 (a hero cannot fit), and the two guards for a biography near the start of the file (a hero is always past byte 30000).
- Not tested: the biography guards just named, and what a record of obelisks shorter than its count does (still open, see 23b).
- Homm3.Tests: the new fixtures pass (95); no production code changed.

## Slice 18c - three more hero view-model fixtures off the savefile

- `HeroArmyTabTests` (6, MultiUnit) from the army part of `HeroGeneralTabTests`: the AI and fight value of a stack, the army summary (health, cost, AI, fight) and that it follows an edit, the columns of the creature finder, and the special and resources of a creature choice.
- `SpellsFromArtifactsViewModelTests` (7, MultiUnit) from `SpellsFromArtifactsTests`, with the real `GrantedSpellsResolver` (`HeroModels.Christian` takes an optional resolver; with none, artifacts grant nothing as before): a tome gives its school and, from any tome, Magic Arrow and Visions, names its source, gives no spell of another school, and keeps a spell the hero learned; Spellbinder's Hat gives every level five spell and putting the hero back takes them away; a spell from another source survives swapping artifacts and says "Unknown source"; Sea Captain's Hat gives the boat spells.
- `GeneralTabArtifactNotesTests` (4, MultiUnit) from `ArtifactEffectsViewModelTests`: the "+6 from artifacts" note and the total, the note gone when the hero is put back, the empty movement note and the maximum text, and the XP note. The other three tests of that file never used a savefile: they moved into the MultiUnit `EquippedArtifactsViewModelTests` and the Integration file was removed (every test is in one place or the other).
- The original `HeroGeneralTabTests` and `SpellsFromArtifactsTests` stay as Integration (the same behaviour on a real savegame). Their general-tab cases are already covered by `HeroViewModelTests`; slice 29 can decide what is a duplicate.
- Mutation check (caught, production restored): the resolver returning nothing, its source names, the AI and fight values of a stack, the artifact note, and the army summary. Not caught: the scroll spell added to the granted spells (no test puts a scroll in a slot) and the spell-book-granted flag in `SpellBook.WithGrants` (equivalent for a hero that has a book).
- Homm3.Tests: Unit, MultiUnit and the Integration view-model fixtures pass (643); no production code changed.
- Not done (18d): `SecondarySkillsViewModelTests`, `SpellsViewModelTests`, `CreatureFinderViewModelTests`, `HeroEditingViewModelTests`, and the castle view models.

## Slice 18d - secondary skills, spells and creature finder pages off the savefile

- `SecondarySkillsPageTests` (13, MultiUnit) from `SecondarySkillsViewModelTests`, with a hero that has Archery (basic), Wisdom (expert) and Luck (advanced): the slots in order and free slots empty (offering basic), moving, clearing, picking in the first free slot, replacing keeping the slot's level, changing a level, a skill the hero has not added twice, only the first free slot fillable, the skill rows with their placement, extra skills beyond eight slots ("Not shown"), raising and removing an extra, hiding and showing, and putting the hero back. The one long test was split in four.
- `SpellsPageTests` (11, MultiUnit) from `SpellsViewModelTests`, with the real `GrantedSpellsResolver`: the artifact named as the source, ticking a selected spell changing all selected and an unselected one changing only itself, the list with its learned and available flags, learning and putting the hero back, `IsAvailable` read-only, the name and level filters, Learn all and Forget all limited to what is shown (separate cases, now on the spell book instead of the pending changes), and the commands only offered with a spell book.
- `CreatureFinderPageTests` (8, MultiUnit) from `CreatureFinderViewModelTests`: the finder lists, its word filters, the stats of a row, the totals of a stack and none for an empty slot, adding to the first empty slot and putting the hero back, and adding refused when all slots are used.
- Undo in the originals is "put the hero back" here (`session.Update(_ => session.Original)`): a fake session has no history.
- Mutation check (caught, production restored): Learn all learning nothing or forgetting, its condition, the move up and down limits, the level of a free slot, the free-slot condition and summary of the finder, and the amount added. Two survived the first version and got a case each (Learn all without a spell book, the level of a free slot).
- Homm3.Tests: Unit, MultiUnit and the Integration view-model fixtures pass; no production code changed. The three originals stay (slice 29).
- Not done (18e): `HeroEditingViewModelTests` and the castle view models.

## Slice 18e - hero editing page off the savefile

- `HeroEditingPageTests` (21 cases, MultiUnit) from `HeroEditingViewModelTests`, on `FakeHeroSession` with ten pikemen and five archers: the creature slots and their allowed moves, moving, clearing and putting the hero back, picking a creature (one to start, then an amount) and picking none, level and experience in step, a skill, spell points and a war machine edited on the page landing on the hero, ballista and cannon exclusive, unticking a war machine, select all, restore movement, resetting spell points, the Intelligence bonus per game (Shadow of Death 25, 50, 100 percent; Horn of the Abyss 20, 35, 50), rotating and wrapping, removing the destination marker, and the level choices with and without an underground.
- Not ported: the pending-changes list (a fake session has no history; `HeroDifferTests` and the history tests cover it) and making a hero neutral (the session does it; `HeroViewModelTests` covers the page asking the session). The save-based original stays.
- Mutation check (caught, production restored): ballista and cannon sharing a slot, unticking a war machine (survived the first version, now a case), restore movement and its condition, rotation direction and wrap, and the Intelligence percentages for both games.
- Homm3.Tests: the MultiUnit and Integration view-model fixtures pass; no production code changed.
- Not done (18f): the castle view models, which need an `ICastleSession` seam like the hero one (`IHeroSession`).

## Slice 18f - the castle seam and the castle page off the savefile

- Production seam, behaviour unchanged: `ICastleSession : IEditSession<CastleModel>` (name, game version, heroes, finding a hero by id, changing the owner, making the hex editor target). `CastleEditSession` implements it from the document, `CastleViewModel` takes it instead of the sealed `SaveDocumentHistory` and a castle index, `IByteEditTargetFactory.ForCastle` and `CastleByteEditTarget` take the session (as the hero ones do), and `Homm3EditorViewModel` makes the session. The whole solution builds; the castle, hex editor and layering tests that read savegames pass as before (661, run after the change; about 5 minutes, most of it the castle window test).
- `CastlePageTests` (19, MultiUnit) on `FakeCastleSession` and `CastleModels.Village`: the header, a castle of no faction having no buildings, ticking a building and putting the castle back, a building needed by another refused with its reason, a refused edit giving a notice that the next good edit clears, the Mage Guild level, the dwelling count (edited from the row, clamped, not editable when not built), the owners and giving the castle to a player, the garrison edited through the shared army editor, built this turn as a free flag, reset, a change made behind the page, a disposed page, the garrisoned hero (named, its army shown, the editor not editable), the heroes listed and opened by their place, an unknown hero, and the hex editor target.
- A fake session has no history: undo is "put the castle back" (`session.Update(_ => session.Original)`). The pending-changes list, the navigation tree and the overview stay in the save-based fixtures (Integration).
- Mutation check (caught, production restored): the owner call, the hero rows and the hero model of the garrison, the castle index, the notice, the status text, the page title, and the read-only garrison. Two survived the first version and got an assertion each (the garrison editor not being editable, the title).
- A slip worth recording: a mutation helper that restores with `git checkout` also throws away uncommitted work. The seam was lost for a moment and restored from a copy; from then on the work was committed before mutating.
- Homm3.Tests: Unit, MultiUnit and the hex editor fixtures pass (567).
- Not done (18g): the save-based castle fixtures that only need a page (`CastleDetailsViewModelTests`, `CastleGarrisonedHeroTests`, `CastleHeroLinksTests`, `CastleGrailAndCapitolPillTests`).

## Slice 18g - castle details and the garrisoned hero off the savefile

- `CastleDetailsPageTests` (22 cases, MultiUnit) from `CastleDetailsViewModelTests`, on Cove towns made by hand: buildings the map disables are marked with the note and counted, none when the map allows everything, a building an upgrade replaced is marked, the dwellings (upgrade, built, not built, not allowed by the map) with the creatures waiting and the total, the base weekly growth, the Mage Guild per built level and with none, and the shipyard or the inland text. The upgraded growth buildings are not listed, for every faction but the Factory (a case for each): the Factory's "Upg. Gantry" is a real special building, so it is listed (the savegame test only covered the eight towns of Restoration of Erathia).
- Garrisoned hero: the page cases went into `CastlePageTests` (the stored slots left alone while a hero is garrisoned, with the editor not editable) and the rule into `CastleModelTests` (the garrison cannot be set while a hero is garrisoned, and can once none is).
- Not ported (18h): `CastleHeroLinksTests` and `CastleGrailAndCapitolPillTests` test the player page, the overview and the navigation tree; they need a fake player session (and a navigation fixture), not a castle one.
- The originals stay as Integration (slice 29 decides what is duplicate).
- Mutation check (caught, production restored): the count of buildings not allowed, the replaced mark, the "not allowed" text for a dwelling and for a building, the upgraded state, the total, the no-guild text, the inland text, the automatic buildings, the weekly growth text, and the garrison rule.
- Homm3.Tests: the castle fixtures and the models pass; no production code changed.

## Slice 18h - castle links, hero links and pills off the savefile

- `CastleLinksTests` (16 cases, MultiUnit) from `CastleHeroLinksTests` and `CastleGrailAndCapitolPillTests`, on a document made by hand (real hero records and collections through `SaveDocumentFactory`, castle records read by a fake reader): only the castles of the owner, by name, opening by their place among all castles; a record with no name not listed; the garrisoned and visiting heroes as links opening by their place among the heroes, and "Unknown hero N"; the Grail structure named and no mark on others; only the Capitol is the capital; the settlement (village, town, city, capitol) and fort (fort, citadel, castle) text; and the Mage Guild and dwellings in the status.
- Not ported: the tavern places that open a hero, the overview's players and castles, and the badges of the navigation tree. They sit behind `PlayerViewModel`, which takes the concrete `PlayerEditSession` and builds the obelisk and map-object view models from the history; a fake would need a seam wider than the hero and castle ones. The logic they show (the links, heroes, pills) is what `CastleLinksTests` now states, so the save-based fixtures stay Integration and no new seam was added.
- Mutation check (caught, production restored): the Capitol, the Grail text, each hall and fort name, the owner filter, unused records, the order, the castle index, the unknown hero name, the hero index, the visiting hero, and the Mage Guild text.
- Homm3.Tests: the new fixture passes; no production code changed.

## Slice 17e - the rest of the HoMM2 fixtures, decided one by one

- `CastleWeeklyGrowthTests`: `CastleModelTests` (HoMM2, Unit) gained 7 cases on `DifferSamples.Castle()` (Xabran, a Knight castle): a built dwelling gains the growth of its creature (12, 8, 5), the Well adds 2 to every built dwelling and the growth building 8 to the first only, the growth building without the Well, an unbuilt dwelling gains nothing even with both, an upgraded dwelling uses the growth of its creature, a castle of no faction gains nothing, and a dwelling number outside 1 to 6 is refused. The save-based fixture stays (the saves of SecondWell and BrokenAlliance are where the Well and the Farm were seen).
- `HeroDestinationTests`, `MapObjectsTests`, `MinesListsTests`, `HeroActiveSpellsTests` stay Integration. The first two are contracts on real saves (the marker next to Colar Blau in eight saves; every mine, shrine and sphinx of every save written back as the same bytes; the objects of Threedom and Arrax the Jerk). The hand-made versions of those readers and writers are the Unit tests of slice 22. `MinesLists` and the active spells card take a loaded `Game`; they would need a seam as the hero and castle pages have, for little more than they check now.
- Mutation check (caught, production restored): the Well, the growth building (its first-dwelling rule and its amount) and the unbuilt dwelling. Equivalent with today's tables, so not testable: the creature of an upgraded level (no faction has a different growth between a creature and its upgrade), the faction check (a castle of no faction has no creatures anyway) and the range check (the level lookup refuses the same numbers first).
- Slice 17 is finished.
- Homm2.Tests: the model fixture passes (17); no production code changed.

## Slice 20d - final name scan

- The scan (`names.py`: number of words in each test method name) over Core, Heroes, Heroes.Wpf, Homm2, Homm3, Wpf and the exe tests found 74 + 125 + 131 + 483 + 1130 + 48 + 9 test methods. No name has an underscore or a `Test` or `Should` prefix. The fewest words are in Core and Homm3; Homm2, Heroes and Wpf have no name of five words or fewer.
- 19 names were rewritten so they say what is checked, for instance `PatchRejectsOverlappingChunks` is now `ThePatcherRefusesChunksThatOverlap`, `SavingLeavesNothingPending` is `AfterSavingNoChangeIsPending`, `RoundTripKeepsBytesIdentical` is `ReadingAndWritingAHeroUnchangedKeepsTheBytesIdentical`, and `AnotherCastleIsNotAffected` (it checks that the changes of two castles are listed each with its own castle) is `TheChangesOfTwoCastlesAreListedEachWithItsOwnCastle`. The other short names (such as `TheMapNameIsRead`) are sentences about one behaviour and stay.
- Not renamed: `WriteTheGoldLimitSave` (a helper used three times, not a test).
- Core.Tests (79) pass, and so do the 117 Homm3 tests of the renamed classes; names only, no code changed.

## Slice 25a - the bytes of a chunk and the target of the hex editor (Core)

- A survey of `Siniskalc.Core` found these types with no test that names them: `BytesChunk`, `ChunkByteEditTarget`, `ByteRegionMap`, `SavegameService`, `ByteRegion`, `ByteEditTargetExtensions`, `SaveHexDump`, `ByteBufferHelper`, `AbstractSavegame`, `ForwardingLogger` (the rest are interfaces, enums, exceptions and event arguments).
- `BytesChunkTests` (12, Unit) through a raw chunk: name, start, end and length; the bytes are copied in and out (a change to the source or to the array given never reaches the chunk); numbers read little endian, signed and unsigned; reading past the end refused; `CopyTo` puts the bytes at the start of the chunk in the savefile; two chunks have the same bytes whatever their names and places; `WithBytes` gives a new chunk of the same name and place and refuses a shorter or a longer array, saying both lengths; a raw chunk is patched as itself.
- `ChunkByteEditTargetTests` (13, MultiUnit) over a history of two chunks: the title (the chunk's name, or the one given, asked again each time), the regions, the current and the original bytes (copies), an edit reporting whether it changed anything and leaving no step to undo when it did not, the change event (local for the target's own edit, not local for an undo, none for an edit of another chunk, one when saving moves the baseline), and a disposed target no longer listening (disposing twice is fine).
- `ByteEditTargetExtensionsTests` (3, MultiUnit): `WriteBytes` (at an offset, stopping at the end of the chunk, nothing for a negative start or no values) and `FindDifferences` (up to the shorter array).
- Mutation check (caught, production restored): the copies in and out, the end, `CopyTo`, `GetBytesAt`, the equality of bytes, the length check (a longer array survived the first version and got a case), the title, the original bytes, the result of an update, the local flag, the other-chunk filter, the baseline check, dispose, and the three limits of the helpers. Equivalent: reading a signed 16-bit number as a cast of the unsigned one.
- Core.Tests: all pass; no production code changed.
- Not done (25b): `ByteRegionMap` and `ByteRegion`, `SavegameService`, `ByteBufferHelper`, `SaveHexDump`, `AbstractSavegame`, `ForwardingLogger`.

## Slice 25b - regions of a record and the map of its bytes (Core)

- `ByteRegionTests` (9, Unit): where a region ends, its full name with and without a group, which bytes it holds (start in, end out), its leaves (itself, or those of its parts in depth), and shifting (start moved, length and kind kept, the group prefix made the group or put in front of the one it has, the parts moved with the same prefix, the original region unchanged).
- `ByteRegionMapTests` (11, Unit): every byte of a region maps to it; offsets outside the map have no region and shade 0; a region past the length is cut, and one that starts before the first byte maps only what is inside; the parts of a region are mapped and not the region holding them; neighbouring regions alternate shade by the order of their bytes, not of the list; two regions sharing a byte (the ballista and cannon) map to the first and `FindAll` lists both; the shade stays that of the region that owns the byte; an empty map.
- Mutation check (all caught, production restored): the end, the full name, both edges of `Contains`, the leaves, the shift of the start, of the group prefix and of the parts, the order of the leaves, the shade, the clip at the length and at zero, the first owner, `FindAll`, and the bounds of `FindLeaf` and `GetShade`.
- Core.Tests: the new fixtures pass (20); no production code changed.
- Not done (25c): `SavegameService`, `ByteBufferHelper`, `SaveHexDump`, `AbstractSavegame`, `ForwardingLogger`.

## Slice 25c - loading and saving a game (Core)

- `SavegameServiceTests` (13 cases, Unit) with a fake parser, codec and backup service that note what was done and in which order: loading a file reads it with the codec and gives the parser the bytes and the path; no path (null, empty, blanks) is refused before anything is read; loading bytes gives the parser no file and the name asked; reloading reads the file the game came from again and a game with no file (or a blank one) cannot be reloaded; saving to a new file writes the bytes to save and makes no backup; saving over a file backs it up first and then writes; afterwards the game is the file saved, has nothing pending and its baseline moved; if the backup fails nothing is written and the game is not marked saved; if the write fails the game keeps its file and is not marked saved; and if the edits cannot be applied the file is neither backed up nor touched.
- `AbstractSavegameTests` (6, Unit) on a game made of three bytes in a savefile of seven (`TestSavegame`): the name is the one given, else the file name, else "Savefile"; a game from bytes has no file until it is saved; the bytes to save are the original ones, or have the edits of the chunks and nothing else changed; the original bytes are kept and each call gives an array of its own.
- `Moq` 4.20.72 was added to `Siniskalc.Core.Tests` (the other test projects have it). `TestSavegame` is public because Moq cannot make a fake of a generic interface over an internal type here.
- Mutation check (all caught, production restored): the blank path check, the path and the name given to the parser, the reload check, the backup condition and the backup itself, the write, `SavedTo`, `MarkSaved`, the order (bytes before the file is touched), the name fallbacks, and the bytes to save.
- Core.Tests: the new fixtures pass (19); no production code changed.
- Not done (25d): `ByteBufferHelper`, `SaveHexDump`, `ForwardingLogger`.

## Slice 25d - buffer helpers, the dump of a savefile and the log scope (Core)

- `ByteBufferHelperTests` (6, Unit): numbers read and written least significant byte first at an offset (16, 32 and 64 bits, signed and unsigned), nothing else in the buffer changes, what is written is read back, and going past the end of the buffer is refused.
- `SaveHexDumpTests` (3, Unit, a fake region provider): the dump holds the title, the bytes now and the bytes the savefile had; the regions are what the provider gives for the current document and its bytes; after saving the two byte arrays are equal.
- `LogScopeExtensionsTests` (2, Unit, a fake logger): the subject scope is a dictionary with "Subject" and "kind name", and the scope the logger gives is the one returned.
- `ForwardingLogger` needed no new test: `LogFactoryTests` already covers it (nothing written until a factory is installed, a logger made earlier follows a later install, the arity left out of the category of a generic type).
- Mutation check (all caught, production restored): the byte order of each read and write, the offset, the current and original bytes of the dump, the document given to the provider, the title, and the property and text of the scope.
- Core.Tests: the new fixtures pass (11); no production code changed.
- Core is finished: every class of `Siniskalc.Core` that does something is named by a test.

## Slice 26a - the navigation tree (Wpf)

- A survey of `Siniskalc.Wpf` found these view models with no test that names them: `EditorViewModelBase`, `NavigationTree`, `NavNodeViewModel`, `AmountRowViewModel`, `TextFieldViewModel`, `ToggleRowViewModel`, `ChoiceFieldViewModel`, `DelegateCommand`, `SortColumnViewModel`, `RecentMenuItemViewModel`, and the bases of pages; plus the code-behind of the views (`HexEditor` 917 lines, `HexViewer`, `FinderView`, `Stepper`, `NavButton`, `HexRowList`, `HexPalette`, `ShellWindow`...), most of which need a window.
- `NavNodeViewModelTests` (14, Unit): what a node shows and its start state; hero and castle by kind; selecting tells the tree once and deselecting tells nothing; a property raises its notification only when it changes; the dot of a node with changes; the notes joined after the title; the badges (only a different list raises a notification); and the filter: empty shows all, a match by title ignoring case and spaces, a node stays visible and expanded when a child matches, a node with no match is hidden and shut, a matching node shows its children, and clearing goes back to the default expansion.
- `NavigationTreeTests` (19, Unit): the overview is the first node; heroes, castles, the map, mines and dwellings found by the index of their record; a player is its own first tab, other tabs by number, and a missing tab falls back to the player; the neutral node is the one with index -1 and its tabs fall back to it; a kind the tree does not index is not registered; clearing forgets every node; the filter applied to every node, reapplied only when there is one, and a null filter is none; revealing a node expands its ancestors, selects it and tells the tree (a top node expands nothing else, nothing does nothing); and the marks of changes: the node of the record and the nodes above it, not a change of another kind with the same index, taken off when the changes are gone, none for a kind with no chunk kind.
- Mutation check (caught, production restored): 31 changes. Two survived the first version (a null filter then reapplied, and the kind of a change) and got an assertion each. Equivalent: the parent flag passed to children (a node only passes it when it has children), and the player's own entry for tab 0 (the fallback gives the same node).
- Wpf.Tests: the new fixtures pass (33); no production code changed.
- Not done (26b): `EditorViewModelBase`, the field view models, `DelegateCommand`, `SortColumnViewModel`, `RecentMenuItemViewModel`, the page bases.

## Slice 26b - the editor every game builds on, and the command (Wpf)

- `EditorViewModelBaseTests` (30 cases, Unit) over a fake service, region provider and navigation builder, a game of two one-byte records whose edits show as pending changes (`TestGame`) and an editor that notes how its hooks were used (`TestEditor`): with no game nothing is loaded and nothing can be saved, reloaded or dumped, and undo, redo and revert do nothing; loading a file or bytes (with the name of the source) holds the game, prepares it, closes the old one and opens the new one with what was prepared, tells the properties, builds the navigation, shows the overview and starts with no pending change; a second game closes the first and the editor stops listening to its history; an edit becomes a pending change, marks its node, refreshes the marks and tells the hook; undo and redo; the list of changes is refreshed without telling about the ones that did not change; reverting a change runs its revert and reverting all puts the original document back (both edits, not only the last) and only when something is pending; a new structure key rebuilds the navigation and shows the page again, and the same key leaves it alone; the filter is kept, applied again after a rebuild and null is none; showing a page tells only when it is another page, asks the hook first and reveals its node; the node the user picked stays the selected one even when the page belongs to another node; saving to the file of the game (a blank name is no file) or to a path; reloading; the bytes to save; and the title of the hex dump (file name and version, "Savefile" for a blank name).
- `DelegateCommandTests` (6, Unit): execute runs the delegate with the parameter (also when the condition says no: stopping the button is the button's job), the condition is asked each time with the parameter, no condition means yes, and the changed event can be subscribed.
- Mutation check (caught, production restored): about 30 changes. Survivors fixed in the first version: a blank file name taken for a file, the page of the picked node (the fake editor now has a node that shows another node's page, like the Mines tab of a player), and reverting all done as an undo (the test now makes two edits). Not testable: `PendingChanges.Clear()` when a game is loaded (the refresh right after empties the list anyway) and `RaiseCanExecuteChanged` (it only asks the window's `CommandManager`, which needs a dispatcher).
- Wpf.Tests: the Unit and MultiUnit fixtures pass; no production code changed.
- Not done (26c): the field view models, `SortColumnViewModel`, `RecentMenuItemViewModel`, `TabbedPageViewModelBase`, `PageViewModelBase`.

## Slice 26c - the amount row and the text field (Wpf)

- `AmountRowViewModelTests` (13 cases, Unit): what the row shows; the badge is the first two letters of the name (the whole name when it is shorter); a value inside the limits is written as it is, one beyond is written as the nearest limit and the limits themselves are allowed; the row is changed when the value is not the one in the savefile; the delta text is empty when nothing changed and otherwise "+2,500 from save" or "-600 from save"; the note; refresh tells every binding; the value is read from the source each time.
- `TextFieldViewModelTests` (10, Unit): what the field shows; a new text is written, the same text is not written again, no text is written as an empty string (and nothing when the field is already empty); the maximum and the counter are empty without a limit or with a limit of zero, and "5 of 12" with one, following the text; the limit is asked again each time; refresh.
- Mutation check (all 16 caught, production restored): the badge and its short-name rule, the clamp and its lower limit, the changed flag, the delta (zero, sign and wording), the note, refresh (both), the same-text check, the null text, the counter and its zero limit, and the limit.
- Wpf.Tests: the new fixtures pass (23); no production code changed.
- Not done (26d): `ToggleRowViewModel`, `ChoiceFieldViewModel`; then `SortColumnViewModel`, `RecentMenuItemViewModel` and the two page bases.

## Slice 26d - the check box row and the drop-down field (Wpf)

- `ToggleRowViewModelTests` (7, Unit): what the row shows; a check that changes the value is written, one that leaves it as it is writes nothing, and unchecking is written; the row may be toggled unless its condition says no (and with no condition it may); the note; refresh tells every binding.
- `ChoiceFieldViewModelTests` (7 cases, Unit): what the field shows and the list of choices it offers; picking another choice writes it, the same choice writes nothing, and a cleared box (null or empty) writes nothing so the stored choice is kept; enabled unless the condition says no; refresh.
- Mutation check (all 14 caught, production restored): the same-value check of each, the write, the condition and its default of each, the note, the tooltip, the choices, the empty-box rule, and refresh.
- Wpf.Tests: the new fixtures pass (14); no production code changed.
- Not done (26e): `SortColumnViewModel`, `RecentMenuItemViewModel`, `TabbedPageViewModelBase`, `PageViewModelBase`.

## Slice 26e - the sort heading and the recent files entry (Wpf)

- `SortColumnViewModelTests` (4, Unit): a new heading shows its name and holds its command; showing a direction puts the arrow after the name (up or down) and showing none takes it away; showing a direction tells that the label changed.
- `RecentMenuItemViewModelTests` (10 cases, Unit): an entry shows its kind, header, tooltip and command (none when not given); only the separator is a separator; every entry can be chosen except the one that says the list is empty.
- Both types were already exercised through `SortableListViewModelTests` and `RecentFilesMenuViewModelTests`; they now have a fixture that names them.
- Mutation check (all 12 caught, production restored): the label with and without an arrow, the arrow, the stored direction, the notification, the command, the separator rule, the enabled rule (two ways), and the tooltip, command and header.
- Wpf.Tests: the new fixtures pass (14); no production code changed.
- Not done (26f): `TabbedPageViewModelBase`, `PageViewModelBase`.

## Slice 26f - the page bases (Wpf)

- `PageViewModelBaseTests` (2, Unit): a page keeps its title and starts with no pending changes; setting the pending-changes flag tells once and the same value tells nothing.
- `TabbedPageViewModelBaseTests` (3, Unit): a tabbed page starts on the first tab and keeps its title; changing the tab tells once and keeps the new tab (the first version did not check the stored tab and a mutant that never stored it survived; fixed); a tabbed page is a page and has pending changes too.
- Mutation check (all 8 caught, production restored): the title of each, the same-value check, the stored value and the notification of each.
- Every view model of `Siniskalc.Wpf` is now named by a test. What is left in the project is the code-behind of the views.
- Wpf.Tests: the new fixtures pass (5); no production code changed.

## Slice 27a - how every game words a change (Heroes)

- A survey of `Siniskalc.Heroes` and `Siniskalc.Heroes.Wpf` found the types with no test that names them. Heroes: `ChangeSinkExtensions` (289 lines, used by every differ of both games), `ChangeText`, `MineKinds`, `MapSizes`, `LinkViewModel`, `ObeliskEditSession` and interfaces. Heroes.Wpf: the bases `SecondarySkillsViewModelBase`, `ArmyViewModelBase`, `SpellsViewModelBase`, `OwnerNavigationBuilderBase`, `NeutralPageViewModelBase`, and the cards and rows (`SkillSlotViewModel`, `StackTotals`, `SpellChoiceViewModel`, `ArtifactPickerViewModel`, `PlayerCardViewModel`, `PrimarySkillRowViewModel`, `SkillRowViewModel`, `NextBattleCardViewModel`...). Most of the Heroes.Wpf ones are exercised through the pages of both games (slices 18 and 20).
- `ChangeSinkExtensionsTests` (16, Unit) on a made-up record, the test colour and test creatures: for the owner, the army, the primary skills, the hero level, the position, the destination, the movement points, the spell points, the resources, the tavern and the days left, each states the area, the description, the text before and after, that equal values give no change, and that the revert puts back only that value. Particulars: the army lists one change for each slot that differs (counted from one, "empty" for a free slot, thousands separators), armies of different sizes are compared up to the shorter one (either way round); each primary skill is its own change; the position has the facing, and the level only for a map with levels; a destination of none says "none"; movement is "current / maximum"; each resource that changed is its own change; a tavern place with no hero says "(none)"; days left says "not counting" for none.
- `ChangeTextTests` (3, Unit): yes and no, a number or "none", and the words for empty and none.
- Mutation check (all 33 caught, production restored): the first version let one through (the bound of the army comparison, tried only with a shorter army before), and a case with a longer army before fixed it.
- Heroes.Tests: the new fixtures pass; no production code changed.
- Not done (27b): `MineKinds`, `MapSizes`, `LinkViewModel`, `ObeliskEditSession`; then the Heroes.Wpf bases and cards.

## Slice 27b - mine kinds and map sizes (Heroes)

- `MineKindsTests` (16 cases, Unit): the name of each of the seven kinds, the resource each gives, a code that is no kind read as a gold mine (name and resource), and the name at a place (coordinates, with the level only for a map with levels).
- `MapSizesTests` (11 cases, Unit): the name of each standard size from 36 to 252, a size that is not standard written as the number twice ("40 x 40"), and no size (zero or negative) as no text.
- Mutation check (all 24 caught on the first run, production restored): each name, each resource, the default of both, the place text and its level rule, each size, and the two rules for other sizes.
- Heroes.Tests: the new fixtures pass (27); no production code changed.
- Left, optional: `LinkViewModel` (three properties) and `ObeliskEditSession` (a subclass that only fixes the type); both are used by the pages of both games and covered by their tests.
- Not done (27c): the Heroes.Wpf bases and cards.

## Slice 27c - a stack and a slot of an army (Heroes.Wpf)

- `StackTotalsTests` (10, Unit): the hit points, cost, AI value and fight value of a stack are those of one creature times the amount; a damage that does not vary is one number and one that varies is a range; thousands separators; the price beyond gold lists each resource in the order of the game (wood, ore, mercury, sulfur, crystal, gems) and only those the creature costs, with "and" in front and none for a creature that costs only gold; the full list of facts (hit points, damage, cost, the other cost as a note, the two values with their tooltips) and the short list of hit points and damage.
- `StackSlotViewModelTests` (20, Unit): a new slot is empty and shown from one; picking a creature or changing the amount tells the army with the other half of the stack, and the same value tells nothing; initializing shows what the page holds, tells nobody and tells every property; the move buttons only where the slot can move that way, with the slot and direction; clear only with a creature; the facts are asked for the amount and only for a slot with a creature, and follow the amount; the upgrade: none, one (changes the stack into it, keeping the amount), several (the one named, an unknown one refused), and the tooltips; the picker opens and closes, is made only when first asked for, and picking from it closes it and sets the creature.
- Mutation check (all 50 caught, production restored): the first run let two through (the resource lines, whose mutants did not compile, were redone; and a picker set to the state it is in already, which got a case).
- Heroes.Wpf.Tests: the new fixtures pass (30); no production code changed.
- Not done (27d): `ArmyViewModelBase` (needs a fake finder and slots), then the skill and spell bases, the cards and rows.

## Slice 27d - the army editor every game builds on, and a skill slot (Heroes.Wpf)

- `ArmyViewModelBaseTests` (21, Unit) over `TestArmy` (four slots over a list of stacks made by the test that notes what the base class asks of the game) and `TestFinder`: the slots and the creatures to pick from with an empty entry first; the title (tells only when it changes); editable unless read-only; refreshing shows the stacks in their slots (a stack with no name is an empty slot), the first slot cannot move up and the last cannot move down, writes nothing back, lets the game read each slot, tells the summary, the free slot and the editable flag, and refreshes the finder; a slot edited while the page is being refreshed is not written back, and a move or clear asked then is ignored; editing a slot writes the stack back; moving swaps with the neighbour in that direction and does nothing off the ends or outside the army; clearing; the buttons of a slot; whether a slot is free; and the picker finder of each slot.
- `SkillSlotViewModelTests` (24 cases, Unit): an empty slot at Basic shown from one; showing holds the skill and level without telling anybody, an empty level falls back to Basic, and tells every binding; picking another skill or level tells the page with the slot, an empty or unchanged one tells nothing, and a free slot has no level to change; the pips of each level and none for a free slot; the move buttons only where the slot can move that way; clear and hide only for a filled slot.
- Mutation check (all 44 caught, production restored): the first version let five through, all about the guard that keeps a refresh from writing back to the game (a game that edits a slot while the page refreshes is now simulated), a stack with no name, and the refresh of the finder; each got a case.
- Heroes.Wpf.Tests: the new fixtures pass (51); no production code changed.
- Not done (27e): `SkillLevelNames`, then `SecondarySkillsViewModelBase`, `SpellsViewModelBase`, `OwnerNavigationBuilderBase`, `NeutralPageViewModelBase` and the cards and rows.

## Slice 27e - the page of secondary skills every game builds on, and the level names (Heroes.Wpf)

- `SecondarySkillsViewModelBaseTests` (35 cases, Unit) over `TestSkills` (skills in contiguous slots, extra skills the game applies but does not show, and a note of what the base class asks of the game): the slots and a row for every skill; the two columns of slots and of skills (the left one longer when odd); the level lists of a slot (without none) and of a row (with none); the summary of used slots and the extra-skills sentence (one or several); refreshing shows the skills in their slots, empty ones at Basic, which slots can move, and each row's level and slot, writes nothing and tells every binding; picking a skill replaces it in a filled slot, adds it in the first free one, and is refused for a later slot or a skill the game does not have; picking a level for a slot or a row; a level of none is not one of a slot; moving and clearing within the used slots only; the buttons of a slot; hiding where the game supports it (by slot or by skill name, not for an empty slot or a skill in no slot) and showing an extra skill only while a slot is free; and, while the page is refreshed, no choice, move, clear, hide or show reaches the game. The skill rows are tested through the page: show and hide availability, the level that is empty or already there, the placement text and the extra flag.
- `SkillLevelNamesTests` (13 cases, Unit): the four names, the lists with and without none, each level of the game named (a code that is no level is none), each name mapped back to a level and number (anything else is none), and the names are case sensitive.
- Mutation check (all 62 caught, production restored): the first version let 11 through (the guards that keep a refresh from writing to the game for hide and show, the lower bound of a move, hiding an empty slot, a level of none for a slot, and the rows' show, hide, level, extra and placement rules); each got a case. Without the refresh guard the page recurses until the test host stops, which counts as caught. Not testable: the `Contains` guard of a row's level and the upper bound of a slot's level (a row or slot can only be made for a skill or a used slot).
- Heroes.Wpf.Tests: the new fixtures pass (48); no production code changed.
- Not done (27f): `SpellsViewModelBase`, `OwnerNavigationBuilderBase`, `NeutralPageViewModelBase`, and the smaller cards and rows (`SpellChoiceViewModel`, `ArtifactPickerViewModel`, `PlayerCardViewModel`, `PrimarySkillRowViewModel`, `NextBattleCardViewModel`...).

## Slice 27f - the spell page and the page of things that belong to nobody (Heroes.Wpf)

- `SpellsViewModelBaseTests` (16, Unit) over a page of three spells whose game part is a fake that notes what it was asked, and a `TestFinder` that holds the rows: the title and rows; the spell book read from the game, written only when it changes and never while the page is refreshed; refreshing shows what the hero learned, writes nothing, tells every binding and the finder; a tick while refreshing is not passed on; ticking a spell that is not selected learns only it; ticking a selected spell does the same to every other selected spell that is listed; a tick that leaves the state as it was tells the game nothing; unticking forgets; Learn all and Forget all act on every listed spell and are offered only with a spell book. `SpellChoiceViewModel` is covered through it (a selection tells only when it changes).
- `NeutralPageViewModelBaseTests` (12, Unit) over a page whose game part gives made-up links and a real, empty `SaveDocumentHistory`: the name (Neutral unless given), colour, the reset that can never run, nothing listed before the first refresh; refreshing loads the heroes and the castles and lets the game refresh its objects, and tells every binding; one hero or castle is enough to have them; the one-line summary ("3 heroes · 2 towns · 5 mines", singular for one, parts left out when there are none); the phrase for a count; the page follows the history and a disposed page does not; and it keeps the tab shown.
- Mutation check (all 40 caught, production restored): the first version let four through (the default name, one hero being enough, and the same-state checks of a spell's learned and selected flags); each got a case. My first reading of the run counted them as caught by a crash; the total of tests that ran showed they had not been, and the full count is now checked.
- Heroes.Wpf.Tests: the new fixtures pass (27); no production code changed.
- Not done (27g): `OwnerNavigationBuilderBase` and the smaller cards and rows (`ArtifactPickerViewModel`, `PlayerCardViewModel`, `PrimarySkillRowViewModel`, `NextBattleCardViewModel`, `ObeliskRowViewModel`, `ResourceRowViewModel`, `HeroLinkViewModel`...).

## Slice 27g - the navigation tree every game builds on, and the artifact picker (Heroes.Wpf)

- `OwnerNavigationBuilderBaseTests` (16, Unit) over a fake game (a savegame of one byte) and a builder whose game part is lists made by the test: the chunk kinds are those the game gave; the overview is called Game, shows the file name only (empty for no name), starts open and has the colour of no player; a group for each owner of the players of the savefile, the heroes and the castles in use, once each, in the order of the colours with neutral last (a player with no hero or castle still has one; a castle owner not in use has none); the game's own nodes come after the groups; the key that tells when to build again has the owners of castles, heroes, mines and dwellings in that order and changes when any of them changes; the heroes of an owner by name with their place and what they are; the mines of an owner grouped by the resource they give in the order of the resources, ordered by level, then X, then Y, named by place with the level only for a map with levels.
- `ArtifactPickerViewModelTests` (9, Unit): a new picker says "(empty)"; updating from the model shows the artifact and tells the bindings once; the button opens and closes the list and tells only when it changes; the list is made when first asked for; picking closes it and tells the page; clear only with an artifact, telling the page "none" and closing the list; picking does not change what is shown until the model says so.
- Mutation check (all 49 caught, production restored): the first version let three through (a player of the savefile with no hero or castle, the order of mines by X before Y, and an overview name that is already empty, which is equivalent: `GetFileName` of an empty name is empty); the first two got a case each.
- Heroes.Wpf.Tests: the new fixtures pass (25); no production code changed.
- Left, optional (27h): the small cards and rows of Heroes.Wpf, all exercised through the pages of both games.

## Slice 27h - the small cards and rows of Heroes.Wpf

- Eight fixtures that follow the same pattern (a mechanical slice, so more than two subjects), all Unit:
  - `PlayerCardViewModelTests` (5): the name, colour and control text; no resources give no text; the gold alone with thousands separators and the other resources in the order of the game in one line; the banishing text; heroes and castles only when given.
  - `PrimarySkillRowViewModelTests` (9 cases): the label, range, base and total; a base outside the range is shown as the limit; a new base is written, one beyond the range as the limit, and the same value not at all; the note of artifacts ("+6 from artifacts", "-2 from artifacts", none for no bonus); refresh.
  - `ResourceRowViewModelTests` (6): gold has the gold limit and step and the other resources theirs; the name and colour of the resource; only gold is gold; the value and the change against the original set; setting a value changes only that resource within the limits; the note for the resource.
  - `ObeliskRowViewModelTests` (4): the label, number and place (any text is a place, empty is none); the check box writes only a change; refresh.
  - `ResourcePaletteTests` (13 cases): the colour of each of the seven resources, matching ignoring the case, the colour of what is unknown, and each resource of the game having its own.
  - `ResettablePageViewModelBaseTests` (5): the title and the Reset button off at the start; Reset offered only with pending changes and running the game's reset; every change of the history tells the page; a disposed page no longer follows it; the tab is kept.
  - `HeroChoiceViewModelTests` (4): a hero keeps its id as key, nobody has key -1, a hero with id 0 is not nobody, and the name is the text.
  - `CastleHeroRowViewModelTests` (2): the role, name and command, the role as the detail and in lower case as text.
- Mutation check (all 53 caught, production restored): the first run let two through (the banishing text tried only with a long text, and a place tried only with a long text); each got a case with a short one. A fixture that shared state through its fields (NUnit uses one instance for all of a fixture's tests) failed once until its state was set up in `[SetUp]`.
- Heroes.Wpf.Tests: the new fixtures pass (45); no production code changed.
- Left (pure holders): `HeroLinkViewModel`, `OrderOfPlayItemViewModel`, `NextBattleCardViewModel`.

## Slice 28a - the castle writer of HoMM3 on a record made by hand

- `SafeCastleSerializerTests` (26, Unit) writes castles into a record of zeros and asserts the bytes: the two hero bytes (FF for none, 254 the highest, -1 and 255 refused), the Mage Guild level, the five spell counts (fewer counts leave the other levels alone, 256 refused), the spells written into the slots that hold a known spell and a level whose list does not match left alone, the creatures waiting as little-endian shorts (moved between the slot of the dwelling and of its upgrade, the slot not holding the count kept, out of range refused), the three masks (built, visible, enabled), unknown bits kept, the companion bits of the town hall, city hall, capitol and fort, and a castle of no faction leaving the masks alone.
- The save-based tests of the same writer were renamed `SafeCastleSerializerSavegameTests` (Integration).
- Mutation check (14 mutants, 13 caught): the survivor (the faction test in front of the upgrade lookup of the creatures) is equivalent, because no building is found for the neutral faction anyway. Production restored.
- Homm3.Tests: the fixture passes with the savegame one (41); no production code changed.

## Slice 28b - the mine and dwelling writers of HoMM3 on records made by hand

- `SafeMineSerializerTests` (7, Unit): the owner as the code of its colour and Neutral as FF; an owner byte that is no colour kept while the mine stays Neutral and replaced by a colour; the guard as little-endian type and count integers in their own tables; an emptied slot back to the empty mark; only the owner and the guard bytes change; the kind and the tile never written; the source record untouched and the size kept.
- `SafeDwellingSerializerTests` (8, Unit): the same for the owner; the stock as a little-endian integer; 0 and the most allowed, one beyond and a negative stock refused; a stock above the limit that the savefile holds kept while the model still says it; only the owner and the stock change; the creature and tile never written.
- Mutation check (12 mutants, all caught, production restored): the owner test always or never true, the guard write removed, a shifted owner code, the stock test as `>=` or always true, the range guard removed or with a wrong bound on either side, a shifted stock, the owner write removed. The first version of one mutant did not compile and was redone.
- Homm3.Tests: the two fixtures pass (15); no production code changed.
- Left: `SafeMerchantStockSerializer`.

## Slice 28c - the merchant stock writer and reader of HoMM3 on lists made by hand

- `SafeMerchantStockSerializerTests` (8 cases, Unit): an artifact as its code in a little-endian integer in its slot; an emptied slot back to FFFFFFFF; one changed slot changes only its four bytes; a code that is no artifact, or above a byte, kept while the model still holds what it reads as and replaced (or emptied) when the model changes; a list that is not seven long refused; the source untouched.
- `MerchantStockDeserializerTests` (3, Unit): index and name of the record; each slot from its four bytes with FFFFFFFF as empty; a code that is no artifact, above a byte (also when its low byte is an artifact) or FFFFFFFE reads as the first artifact.
- Mutation check (11 mutants, production restored): the first run let two through. The check that a code fits a byte needed a code whose low byte is an artifact other than the first (now 0x17E); the other was a mutant that did not compile and was redone. One is equivalent and documented: a lower bound of 0 on the length of the list, because a list of six fails on the seventh slot with the same exception.
- Homm3.Tests: the two fixtures pass (11); no production code changed.

## Slice 28d - the finders of the mine and dwelling tables of HoMM3 on bytes made by hand

- `MineBytesLoaderTests` (23 cases, Unit): a run of three records is found with offsets, indexes and names; a run of two, short or empty bytes give nothing; a record that is no mine (kind above 6 or tile 0, 0) stays in the run but is not cut out and does not take an index; two tables are both read and the indexes go on; a table ending at the last byte is found; each check (tile, level, owner, abandoned, used slot without count, empty slot with count, creature code, count) ends the run on one side of its limit and lets the record through on the other.
- `DwellingBytesLoaderTests` (21 cases, Unit): the run must start right after the last of the mines (not after the first, not one byte later) and have three records; no mines give nothing; the names (a creature that is unknown is named by its number); each check ends the run and a bad record after three good ones leaves three; a chunk of another kind that overlaps a record stops the run there while one that ends where the first record starts, or starts where the last ends, does not.
- Mutation check (31 mutants, production restored): the first run let seven through. Five got tests (a table ending at the last byte, the last of several mines, a chunk ending at the first record, and the two indexes whose mutant had not changed the file). One is equivalent: the outer bound of the scan of the mines, because a table needs three records and the inner bound decides. Four were left open: the tile at `MaxCoordinate` (251), rejected by `>=` in both loaders, the same off-by-one as in `MapObjectPositions` (24d-iii). It was decided that 251 is valid; see below.
- Homm3.Tests: the two fixtures pass (44); no production code changed.

### Slice 28d, production fix - a mine or dwelling on the last tile

- `MineBytesLoader` and `DwellingBytesLoader` compared the tile with `Constants.Map.MaxCoordinate` using `>=` / `<`, so a record on tile 251 (the last of the largest map, 252 tiles wide) was not a record and a table with one on that tile was not found, or was cut short. They now use `>` / `<=`, as `MapObjectPositions` does.
- The tests have the last tile as a valid case on each axis alone and on both, and 252 as invalid. All four reverse mutants are caught. Homm3.Tests: the mine, dwelling and savefile fixtures that name them pass (911); the whole project was not run.

## Slice 28e - the finders of the merchant lists and of the places of the map of HoMM3 on bytes made by hand

- `MerchantBytesLoaderTests` (13 cases, Unit): the count and the lists behind it with offsets, indexes and the names (one list is the black market, several are numbered from 1); 32 lists are the most; at least four artifacts in stock in all the lists; a code that is no artifact or does not fit a byte ends the search; lists that do not fit the bytes; the table is only searched up to `SearchLength`; the first table that looks right wins.
- `MapPlacesLoaderTests` (18 cases, MultiUnit, uses the real `MapObjectPositions`): no definitions or no objects give `MapPlaces.None`; a merchant cell and a cell of a kind give the number and the tile of their place in the list, in the order of the numbers; a place is taken once, by its first cell, and a number once; place 0, a place outside the list, a number of 32 or more, anything in the type bytes or after the number (for kinds without flags and for merchants) are no places; kinds with flags mask the number; a place beyond the 256th of the list (the place is 16 bits); a `.def` at the very start of the bytes is not the table.
- Mutation check (35 mutants, production restored): the first run let six through. Four got tests (the third and fourth byte of the type, a place above 255, merchants with bytes after the number, a `.def` at the start); two are equivalent: a count of 0 as a valid count of merchants (no stock can be found in no list) and the check that definitions exist in `MapPlacesLoader` (the finder of the objects returns none without them). One is left: the end bound of the scan of the cells (`i + 10 <= firstDefinition`), because the only cell that reaches it would share its last bytes with the length of the first definition.
- Homm3.Tests: the two fixtures pass (31); no production code changed.
- Left in the data layer of HoMM3: `SavegameParser`.

## Slice 28f - four small HoMM3 view models

- `DwellingRowViewModelTests` (5, Unit): what the row shows; editable only when built and given an action to write; the count clamped to the limits and written; no action only changes the row.
- `ArmySlotViewModelTests` (5, Unit): no totals without the info; hit points, damage range, cost, AI and fight values are those of the info times the amount; a change of amount or of info updates them and tells the bindings (including the facts of the base slot); no info clears them.
- `MineRowViewModelTests` of HoMM3 (4, Unit): owner picked from the colours of the game and written with the reducer; the position shows the level; "Unguarded" or the count of slots of the guard; `UpdateGuard` writes the guard.
- `MerchantViewModelTests` (6, MultiUnit, real slot and finder): name, seven slots labelled from 1, location only when given, what each slot shows, a clear and a pick in the finder written to the session, refresh.
- Mutation check (25 mutants, production restored): the first run let three through (the facts the base slot tells, the levels of the position of a mine, an artifact picked by name); each got an assertion and the three mutants were run again and caught.
- Homm3.Tests: the four fixtures pass (20); no production code changed.

## Slice 28g - visited places, Mage Guild rows, backpack slot and map dwelling row of HoMM3

- `VisitedPlacesViewModelTests` (7, Unit): a row for each kind of the game and the kinds of Horn of the Abyss only there; a box for each place of the map labelled with its tile; a box ticked by the bit of the place number; ticking and unticking sets and clears only that bit; Forget clears one kind and leaves the others; Remember adds the places of the map and keeps other bits; refresh tells the rows and the boxes.
- `MageGuildRowsTests` (5, Unit): the spell slot (spell, choices, a pick reported only when it is another spell and not nothing, refresh) and the level row (built or not, the text of its spells, an update refreshes its slots).
- `BackpackSlotViewModelTests` (4, MultiUnit, real picker and finder): number and label, setting the artifact shows it and reports it once, a pick in the finder and a clear.
- `MapDwellingRowViewModelTests` (6, Unit): what the row shows (an unknown creature by its code), the owner picked and written, the stock typed as text and clamped, text that is no number or the same stock written to nothing, refresh.
- Mutation check (34 mutants, production restored): the first run let two through (the page refresh not telling the rows and the owner field not being refreshed, because the getters read the model anyway); the tests now watch the notifications, and five mutants whose pattern did not apply were redone. All caught.
- Observation, not pinned: the text of a kind of place counts all the bits of the mask of the hero, so with bits of places that are not on this map it reads "3 of 2". That may be intended (the hero visited them); it is not tested.
- Homm3.Tests: the four fixtures pass (22); no production code changed.

## Slice 28h - spell grants, artifact effects, artifact info, spell finder and building rows of HoMM3

- `SpellGrantProviderTests` (4, Unit): named spells and their description; a school gives its spells and those of all schools; school and level never give the Titan's Lightning Bolt but naming it does; a level gives exactly its spells.
- `ArtifactEffectInfoTests` (21 cases, Unit): no grants give no spells; spells granted by several grants are listed once; two effects are equal only when every field, the spell book flag and the grants are equal (one case for each of the fourteen numeric fields); equal effects have the same hash code.
- `ArtifactInfoViewModelBaseTests` (8 cases, MultiUnit, real provider of lines): nothing shown at first; an artifact shows skills, class and cost and lines; no cost, no class line; the commands show by name and clear; an empty or unknown name clears; every binding is told.
- `SpellFinderViewModelTests` (8 cases, Unit): the search matches name, school, level and type and every word must match; the summary counts learned and available and the number shown; the columns; a tick reaches the caller; the school names.
- `BuildingRowsTests` (11, Unit) for `BuildingRowViewModel` and `BuildingGroupViewModel`: the count built, toggling, the tooltip (reason, build, remove, what is missing), the forbidden note, the warning for a building built without its requirements, the dwelling creatures and the count waiting, replaced, the allow button, children.
- Mutation check (46 mutants, production restored): the first run let two through (the summary test had as many learned as available spells, which hides a swap) and four patterns did not apply and were redone. Two are equivalent and not tested: the check that a spell exists in the game version in `SpellGrantProvider` (every spell of HoMM3 is in Restoration of Erathia), and the effect lines in `HasSelectedInfo` (every artifact with effect lines also has a cost or a text).
- Observation: `Effects` is `null` for an artifact with no text (a default struct), though typed `string`; bindings take it, so it is left.
- Homm3.Tests: the five fixtures pass (51); no production code changed.

## Slice 28i - the chunk collections and the war machine offsets of HoMM3

- `PlayerChunkCollectionTests` (3, Unit, fake deserializer): found by colour, colours listed, a replaced collection keeps the other chunks, the readers and the way heroes are named for the differ.
- `HeroChunkCollectionTests` (3, Unit): found by id, summaries (owner, class, level), owners, a replaced collection keeps the offsets and readers, the asleep mark only in the versions that have it.
- `CastleChunkCollectionTests` (2, Unit): the castle a hero is garrisoned in (not the one it visits), owners, replacing one castle.
- `WarMachineOffsetsTests` (2, Unit): each machine has its place and every machine of the game has one. (The branch that refuses a machine of no place cannot be reached with the machines of the game.)
- Mutation check (19 mutants, production restored): one let through (the way heroes are named in a replaced player collection), which got a test that goes through `GetPendingChanges`; one pattern was invalid and not rerun (`h5`, the owner in the hero summary, which `GetOwners` and the summary test already cover through `Equals` of the summary).
- Homm3.Tests: the four fixtures pass (10); no production code changed.

## Slice 28j - the parser of the savegame and the session of a player (HoMM3)

- `SavegameParserTests` (8, Unit, fakes for the 14 finders and a real document from the factory): a version that is unknown is refused before any finder is asked; the game carries the file, name, bytes, version and the document of the factory; the finders get the bytes and the version; the players are looked for around the heroes and castles; the obelisks are given the players; the dwellings are looked for after the mines and around the heroes, players, obelisks and mines; the date and the hero owners are looked for from the end of the last player (not the first) or from 0; the document is made from what the finders returned and the offsets of the version.
- `PlayerEditSessionTests` (7, Integration, the baseline save of `TestData/Hota/HeroOwners`): the session knows its index; restoring a player that was not touched changes nothing; restoring the player that lost a hero gives the hero its owner, its place in the list, the selected hero and the table of hero owners back; one step of the history that can be undone; a hero given to another player is taken back to its first owner and both lists are restored; a selected hero put to none and a list edited without touching the owners are brought back.
- Mutation check (28 mutants, production restored): the first run let nine through. Four got tests (the end of the *last* player, the obelisks among the chunks the dwellings avoid and given the players, the selected hero). Left open or equivalent: `StandingOn` of the parser (a hero standing on a place hides it on the map: its three conditions are not tested because it needs a hero record with the type, flag and number of a place), the restore of the selected hero as the last step of `RestoreHeroes` (the list restore already puts it back), and two no-op guards (`restored != player` and the check for an unchanged document), which write the same bytes.
- Homm3.Tests: the two fixtures pass (17); no production code changed.

## Slice 28k - artifact bonuses, town building names and the small rows of HoMM2

- `ArtifactBonusesTests` (3, Unit): every artifact that adds points adds those the game gives it (37 entries, including the negative one of the Broach of Shielding), every other adds none, and the four ultimate artifacts add 12.
- `TownBuildingNamesTests` (7 cases, Unit): the second well and the special building of each of the six towns, and the generic names for the neutral town.
- `SpellRowViewModelTests` (3), `GuildLevelViewModelTests` (2) and `DwellingRowViewModelTests` (2), all Unit: the spell row (level, type, magic, cost; no magic gives no text; a tick reaches the caller), the level of a guild (heading, refresh of every field) and the dwelling row (built and growth read when asked, refresh of its fields and the bindings).
- Mutation check (23 mutants, production restored): all caught; two patterns that removed a table entry or a line did not compile and were not rerun (the full table is tested entry by entry).
- Homm2.Tests: the five fixtures pass (17); no production code changed.

## Slice 28l - the shrine row and the secondary skills page of HoMM2

- `ShrineRowViewModelTests` (9 cases, Unit): the circle, position and spell of the shrine; the picked spell written by its name; a shrine with a hero on it cannot be edited and says so; a spell the editor does not know is named by its number; refresh.
- `SecondarySkillsViewModelTests` (3, Unit): the skills of the version listed, the eight slots, a skill shown in its slot with its level, and the note.
- Mutation check (14 mutants, production restored): all caught but one, which is equivalent: the filter on the version of the game for the secondary skills, because every secondary skill of HoMM2 is in the base game.
- Homm2.Tests: the two fixtures pass (11); no production code changed.
- Left in HoMM2 (not tested by name): `PageViewModel` (the base of the pages, over a whole game), `CastleLinks`, `MapPlaces`, `Homm2Module`, `Homm2SaveDocuments`, `Homm2EditSessions` and the sessions of the hero, the castle and the player, `RecordRegions`, `SphinxViewModel`, `ArtifactsViewModel`, `SpellsViewModel` and `SpellFinderViewModel`.

## Slice 28m - the spell finder and the sphinx page of HoMM2

- `SpellFinderViewModelTests` of HoMM2 (5, Unit): all rows shown and several selectable; the search matches name, level, type and magic and every word must match; the summary counts what is learned and how many are shown; the columns.
- `SphinxViewModelTests` (10, MultiUnit, real picker, finder and fields over a fake session): name and index; solved is the opposite of active in both directions; one reward row for each resource, gold first, each read from and written to the place of its resource in the savegame (which is not the order of the page: wood first, gold last); the artifact picked by name and cleared to none; an answer trimmed in its place, an empty one dropping out, the last one kept, twelve characters of room; the riddle with the room of the original; refresh of the toggle, rewards, answers, riddle and artifact.
- Mutation check (27 mutants, production restored): the first run let five through (the refresh of the toggle, the answers, the riddle and the rewards, which read the model anyway, and the length of an answer); the tests now watch the notifications and the room of an answer, and the five were run again and caught.
- Homm2.Tests: the two fixtures pass (15); no production code changed.
- HoMM2 not tested by a class of its own, and why: the sessions of the hero, the castle and the player (`ChangeOwner`, `RestoreOwner`, `RestoreHeroes`, `RestoreCastles`) are exercised by `HeroDifferSavegameTests`, `CastleDifferSavegameTests` and `PlayerDifferSavegameTests`; `RecordRegions` by `SaveRegionProviderTests`; `ArtifactsViewModel` and `SpellsViewModel` by `Homm2EditorHeroPageTests` and `Homm2EditorCastlePageTests`; `Homm2Module` by `TestApplicationServices`. Left without any direct test: `PageViewModel` (needs a whole game), `CastleLinks`, `Homm2SaveDocuments` and `Homm2EditSessions` (thin accessors).

## Slice 29 - duplicated Integration fixtures removed

Six Integration fixtures whose behaviour was ported to MultiUnit tests in slices 18c to 18g were removed, and a seventh was cut to the two tests that were not ported. Each removed test is mapped to the test that keeps its behaviour, and the claim was checked by mutation.

**Mapping (removed test → retained test, all MultiUnit unless said):**

| Removed | Kept in |
|---|---|
| `SecondarySkillsViewModelTests` (6) | `SecondarySkillsPageTests`: slots in order, moving, clearing, picking and replacing, level change, a skill not added twice, only the first free slot, the rows with placement, extra skills beyond eight, raising and removing an extra, hiding and showing, put back (13) |
| `SpellsViewModelTests` (10) | `SpellsPageTests`: same names for the source of a granted spell, ticking selected and unselected, the list with its flags, learning and put back, read-only available, name and level filters, Learn all; plus Forget all and the spell book rules (12) |
| `CreatureFinderViewModelTests` (7) | `CreatureFinderPageTests`: same names for the list, the word filters, the town names, the stats, the totals of a stack, adding to the first empty slot, adding refused when all are used; plus an empty slot has no totals (8) |
| `CastleDetailsViewModelTests` (7) | `CastleDetailsPageTests`: same names for the buildings the map disables, the replaced building, the dwellings, base growth, the Mage Guild, the boats and inland (split in two), the upgraded growth buildings not listed (12) |
| `HeroGeneralTabTests` (9) | `HeroViewModelTests` (Max, Reset, the moved flag and active spells, clamped values, the header) and `HeroArmyTabTests` (a stack's values, the army summary, the creature finder, a creature choice) |
| `SpellsFromArtifactsTests` (4) | `SpellsFromArtifactsViewModelTests`: tome, Spellbinder's Hat, spells from other sources surviving a swap, Sea Captain's Hat (7) |
| `HeroEditingViewModelTests`, 12 of 14 | `HeroEditingPageTests`: slots, moving, clearing, picking, level and experience, ballista and cannon, select all, restore movement, spell points, rotating, removing the marker, the level choices (21 cases) |

**Kept on purpose:** the two tests of `HeroEditingViewModelTests` that were never ported, `EditsFromThePageLandInThePendingChanges` (a fake session has no history, so only a real savegame shows the pending changes) and `MakingAHeroNeutralTakesItOffTheMap` (the session does the work).

**Check:** 32 mutants (the operators `==`, `!=`, `&&`, `||`, `>=`, `<=` swapped, one at a time, in `SecondarySkillsViewModel`, `SpellsViewModel`, `HeroViewModel`, `CastleViewModel` and `ArmyViewModel`) were run against the seven originals together with every Unit and MultiUnit test. 26 were killed, 4 survived, 2 did not build. All 26 killed mutants were also killed by tests that stay; **none was killed only by an original**. The sample is small, and the names above are the evidence for the rest, so a behaviour that an original asserted and a port does not would not necessarily show.

**Not removed:** `CastleHeroLinksTests`, `CastleGrailAndCapitolPillTests` and `CastleGarrisonedHeroTests` (the player page, the overview and the navigation tree are tested on a real savegame there; `CastleLinksTests` has only the logic), and every other Integration fixture.

Test methods removed: 55 (6 + 10 + 7 + 7 + 9 + 4 + 12). Homm3.Tests: Unit, MultiUnit and the Integration view-model fixtures pass (933); the whole project was not run for this slice; no production code changed.
