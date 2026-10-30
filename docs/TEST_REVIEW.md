# Test review

Reviewed on `dimif-test-suite-review` at e5c478d, after slices 01 to 29 of the cleanup (branch based on `feature/homm2-support`).
The first review of this tree (at dc14cef, 4,000 tests, 263 classes) is kept as history in the git history of this file; this text replaces its summary, tables and findings with measurements of the final suite.
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

### Run time after the merge

After the merge the HoMM3 tests were made cheaper in three steps, with the same 3,895 tests passing each time: the series classes `RoeCastleTests`, `RoeCastleBuildingsTests`, `SodSeriesTests`, `RampartAndBulwarkCastleTests` and `CoveCastleTests` read their saves through `SavedGames` (sum of test durations 317 s to 226 s), and `CastleDeserializerTests` loads the 448 saves eight at a time with `SavedGames.LoadAll` (26.5 s to 5 s). `Siniskalc.Homm3.Tests` now takes about 3 m 18 s with 198 s of test durations (5 m 16 s and 317 s before); the class table below was measured earlier. Then every savefile of `TestData` was made to parse once per project: `SaveCache<TGame>` in the new library `Siniskalc.Tests.Homm.Common` (with `TestKinds`, `ByteDiff`, `PendingChangesFake` and `WpfTestApplication`, which the two game projects used to copy) parses a file on first use and hands out a fresh game around the same immutable document on every call, so a test can edit, undo and save its copy. `SavedGames` is the HoMM3 wrapper; 87 call sites that parsed a file themselves use it, and the editors the tests build load through `CachingGameService`, which `Homm3ModuleFactory` accepts as an optional `decorateGameService` (the application passes nothing). `Siniskalc.Homm3.Tests` now takes about 1 m 50 s with 3,908 tests (3 m 18 s before). HoMM2 was not changed: its 636 tests take about 10 s. What is left is mostly WPF windows (`PageRenderingTests`, `ShellSmokeTests`, `CastleEditingViewModelTests`, the layout tests: about 70 s) and `CastleBuilderReplayTests` (10 s of reducer replays).

### Since the first review

| | First review (dc14cef) | Now |
|---|---:|---:|
| Tests | 4,000 | 5,411 |
| Classes | 263 | 423 |
| Classes with a kind | 0 | 423 |
| Sum of test durations | 804 s | 335 s |
| `Siniskalc.Homm3.Tests` | 3,186 tests, 775 s | 3,883 tests, 317 s |
| Tests that are Unit or MultiUnit | not counted | 1,960 (36%) |

Production changes made along the way, all with their own tests (details in the slice log in the git history):

- Seams that keep behaviour: `ISettingsStore` and `IRecentFilesStore` (slice 15), `IHistoryEvents` and `IHeroSession` (18b), `ICastleSession` (18f).
- Fixes: a map tile at the largest coordinate (251) was refused by `MapObjectPositions` (24d-iii) and by the finders of the mine and dwelling tables (28d).

## Findings

### Defects (open)

| # | Where | Problem | Fix |
|---|---|---|---|
| D1 | ~~`ObeliskVisitsTests`, HoMM2 `PlayerViewModelTests`, `ShellWindowTests`~~ | Fixed: they now assert the position of the second obelisk (34,58), the three castles of Blue, and the 5 toolbar and 9 menu icons that the window has | none |
| D2 | ~~every-save tests~~ | Fixed: the read-only every-save tests (15 load sites in 11 classes) now use `SavedGames`, so a save is parsed once per run. `CastleDeserializerTests` pays the one pass (about 24 s); the sum of the HoMM3 test durations fell from 449 s to 317 s. The Roe and OtherFactions series classes share the cache too (the two Roe classes went from 67 s to 6 s alone and to about 0 s in a full run; the HoMM3 test durations from 317 s to 245 s, the project from 5 m 16 s to 4 m 5 s). Left: `CastleBuilderReplayTests` (10 s, already shared) and three smaller series classes | none |
| D3 | ~~all projects~~ | Fixed: `TestFixtureConventionsTests` in each test project checks every fixture of its assembly (one `[Category]` of Unit, MultiUnit, Integration or Manual; Integration and MultiUnit fixtures in a namespace with that folder, Unit ones in neither). The rules are `TestFixtureConventions` in `Siniskalc.Tests.Homm.Common`, linked as source into the projects that cannot reference the library. It caught two new fixtures of this batch whose `[Category]` bound to `System.ComponentModel` | none |
| D4 | HoMM3 `SavegameParser.StandingOn` | A hero standing on a place hides it on the map; its three conditions (type at +11, visitable at +15, number at +16) are untested | A parser test with a hero record that stands on a place |
| D5 | HoMM2 `CastleLinks`, `Homm2SaveDocuments`, `Homm2EditSessions`; HoMM3 `Homm3SaveDocuments`, `Homm3EditSessions`, `ByteEditTargetFactory`, `MapHeaderLocation` | Open. `DwellingDeserializer` (7 Unit tests) and HoMM2 `PageViewModel` (10 Unit tests, on a fake `IPendingChangeHistory` that `SaveDocumentHistory` implements; the pages keep their own `Game`) are done. The rest are thin and used by every Integration test, so only a wrong wiring would fail them | Direct tests where a rule is in them |
| D6 | Wpf `HexRow`, `HexPalette`, `NavButton`, `RegionNode`, `EditorCommands`, `WpfMessageService`, `WpfFileDialogs`, `AdvancedModeVisibility` | Open. Done: `HeroesSecondarySkillsViewModel`, `NextBattleCardViewModel` (rows through `IAmountRow` and `IToggleRow`), `HeroLinkViewModel`, `OrderOfPlayItemViewModel`, and `HexRowList` (lookup through `IByteRegionLookup`, which `ByteRegionMap` implements). `ForwardingLogger` is left out on purpose. The rest are windows, brushes or constants and are left to the rendered-view tests | Test the ones that gain logic |
| D7 | `HeroAndCastleLookupTests` | Joins two subjects in one class | Split by subject |
| D9 | HoMM3 `SafeHeroSerializer` and `SafeHeroOwnersSerializer` | Parked. Their literal-byte tests are Integration (real saves); there is no Unit class on a hero or hero-owner record made by hand | `SafeHeroSerializerTests` and `SafeHeroOwnersSerializerTests` as Unit tests on records of zeros, the savefile ones renamed `...SavegameTests` |
| D8 | HoMM3 loaders | Parked. The end bound of the cell scan in `MapPlacesLoader` (`i + 10 <= firstDefinition`) and a few other mutants were left open on purpose (see the slice log in the git history) | None needed unless the layout changes |

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
| Integration tests over 2 s that walk every save or replay a reducer over a series (15 classes) | They are data contracts over 448 saves; the cost is in loading the saves, which `SavedGames` now does once per run (D2) |
| Scenario classes `Roe*`, `Sod*`, `Hota*`, the faction `*CastleTests`, `RampartAndBulwarkCastleTests`, `CastleGrailAndCapitolPillTests` | `CLAUDE.md` allows a class named after a scenario or a feature tested through every layer |
| Savegame serializer tests (`...SerializerSavegameTests`) that use the reader to build or compare a model | They are the round-trip contract on a real save; the byte contract is in the Unit class of the same subject |
| HoMM2 fixtures of slice 17e (`HeroDestination`, `MapObjects`, `MinesLists`, `HeroActiveSpells`) kept Integration | The saves are the contract |
| HoMM3 tavern, overview and navigation-badge fixtures kept Integration | `PlayerViewModel` takes the concrete `PlayerEditSession`; `CastleLinksTests` covers their logic on a hand-made document |
| `CastleHeroLinksTests`, `CastleGrailAndCapitolPillTests`, `CastleGarrisonedHeroTests` | They test the player page, the overview and the navigation tree on a real save, which no port covers |
| Equivalent mutants documented in the slice log (git history) (for example the version filter of `SpellGrantProvider`, the outer scan bound of `MineBytesLoader`) | No behaviour differs, so no test can tell |
| The first test of a Unit class over 100 ms (`CastleChunkCollectionTests`, 0.25 s) | It creates the Moq proxies; later tests of the class are under 1 ms |
| Moq fakes of `IEditSession<T>` in the view-model tests | A fake session has no history, so "undo" there is "put the model back"; the history is tested with the real session in the Integration classes |

### Fixed during the cleanup

Slice numbers refer to the slice log in the git history of this file.

- Assertion-free and conditional tests (01, 02); in this review also the `Assume` of `CastleEditSessionTests`, three `Assert.Ignore` guards (two in HoMM2 `Homm2EditorNavigationTests`, one in HoMM3 `NavigationBuilderTests`, each now an assertion that the save has what the test needs), a test name that ended in "StillWorks", and the placement of `SphinxViewModelTests`.
- Serializer tests that went through the deserializer now assert literal bytes (10, 11, 17b, 21, 28a to 28c).
- Unit-shaped classes that loaded savefiles became Unit or MultiUnit tests on records made by hand (12, 16 to 19b, 22 to 24); the save-based cases remain as Integration classes with `Savegame` in their names.
- Multi-subject classes split and test names rewritten (20a to 20d).
- Gaps filled: both games' loaders, differs, region providers, view models, Core and Wpf types (22 to 28).
- Duplicates removed after the ports (29).

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
3. Savefile tests only read; a save is loaded once per run where many tests need it (`SavedGames`).
4. No `Assert.Pass`, no silent `continue`, no conditional assert, no `Assume`; a serializer test asserts bytes.
5. A test checks the notification, not only the value, when the getter reads the model anyway (a `Refresh` test that watches `PropertyChanged`); this found several weak tests in slices 27 and 28.
6. A claim that a test covers a behaviour is checked by changing the production code and watching the test fail (mutation checks, kept in the slice log with their survivors).
7. A test class per subject, named `<Subject>Tests`; methods are sentences that state the behaviour.

## Backlog (in order of value)

2. The time that is left in the HoMM3 project is WPF windows (about 70 s) and the reducer replays of `CastleBuilderReplayTests` (10 s); other classes that load a save in `[SetUp]` could use `SavedGames` where they only read.
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

## Working notes

- Work in small steps, one commit per step; never push; do not change production behaviour (a seam that keeps behaviour is acceptable, and is stated in the report).
- Test only the affected projects per step, and run the whole solution once per batch (see `CLAUDE.md`). Say what was and was not run and what was not mutation-checked, and never claim a measurement that was not like-for-like (machine timings vary a lot: compare sums of test durations).
- A savefile is read through `SavedGames` (HoMM3, over `SaveCache` of `Siniskalc.Tests.Homm.Common`): it parses a file once and gives every call a game of its own, so a test may edit it. A test whose subject is loading or saving uses `TestServices.GameService`, which reads the file each time.
- The WPF exe is often running and locks the normal output, so pass `--artifacts-path`. Run the projects one by one with their own `--logger "trx;LogFileName=<project>.trx"` to get a TRX per project.
- Last full run: 3,908 HoMM3 tests in about 1 m 50 s; all projects pass.
- The per-class table and the slice log of the cleanup (slices 01 to 30) were removed from this file; they are in its git history (the version at the merge `c93ce4f`). The slice numbers quoted in the findings refer to that log.
