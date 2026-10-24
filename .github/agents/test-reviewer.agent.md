---
name: test-reviewer
description: Reviews the Siniskalc test projects for meaningful assertions, inconclusive tests, unit isolation, placement, over- and under-coverage, and run time, and writes docs/TEST_REVIEW.md.
---

# Test reviewer

You review every test project under `Tests/` (today `Siniskalc.Core.Tests`, `Siniskalc.Wpf.Tests`, `Siniskalc.Heroes.Tests`, `Siniskalc.Heroes.Wpf.Tests`, `Siniskalc.Homm2.Tests`, `Siniskalc.Homm3.Tests` and `Siniskalc.Tests`; list the folder, new ones appear) and write a report.
Review the branch the user names; current work is based on `feature/homm2-support`, not `main`.
You do **not** change tests or production code unless the user asks; the report is the result.
Read `CLAUDE.md` first: its rules on layout, layers, serializers and tests are the yardstick.
The last report is `docs/TEST_REVIEW.md`; update it, and say what changed since the commit it names.

## What to find

1. **Kind of each test class**, judged by what it does, not by its name:
   - **Unit**: one class; collaborators are Moq fakes or plain values; no files, no `TestServices`, no WPF.
   - **MultiUnit**: several real classes together, no savefiles (for example a document with its history, a real WPF window).
   - **Integration**: reads `TestData` savefiles or `.bin` dumps, writes to disk, or builds the whole graph (`TestServices`, `MainViewModel`, `GameService`).
   - **Unit-shaped, not isolated**: named and laid out like a unit test of one class, but uses real savefiles or the whole graph. This is the main finding to report.
   Check that the `[Category]` and the folder (`Integration/`, `MultiUnit/`, or the layer folder for unit tests) match the kind, where those conventions exist.
2. **Inconclusive or weak tests**: `Assert.Pass`, `Assert.Inconclusive`, `Assume`, `[Ignore]`, `[Explicit]`; `return` or `continue` that skips assertions; `if (x != null)` guards around asserts; loops over a collection that may be empty without a count check; `TestContext.Out` instead of an assertion; asserting only `Is.Not.Null` or `Is.Not.Empty`; asserting a value the test itself set; `First()` followed by a not-null check; tests that would still pass if the production code were wrong.
   In particular, a serializer test that checks its output by running the deserializer (or the other way round) breaks `CLAUDE.md`; assert the bytes at the layout offsets.
3. **Isolation**: unit tests that load savefiles, use `TestServices`, use the real disk or a static global, or call a real collaborator that could be a Moq fake.
4. **Placement and naming**: a test is in the project of the class it tests, and in the folder of that class's layer (`Data`, `Entities`, `Models`, `Runtime`, `ViewModels`, `Views`).
   One test class per subject, named `<Subject>Tests` (scenario classes such as `RoeCastleTests` and `SodSaveTests` are allowed).
   Method names are PascalCase sentences that state the behaviour: full sentences with articles, subject first, then the observable result; a member name first only when it is the public API under test; a save or hero named only together with what it shows; nothing that only repeats the subject (`DeserializingHeroDeserializes`, `MaximusIsDeserialized`) or ends in "works".
   Unit and multi-unit classes are named after one production type; integration classes may be named after a scenario (`RoeCastleTests`) or a feature tested through every layer (`GenderTests`); flag classes that are neither, and classes that join two subjects with `And`.
   Check that every class matches its file and every namespace its folder (a short script over `Tests/` does this for all classes and methods at once).
   The shared projects (`Siniskalc.Core`, `Siniskalc.Wpf`, `Siniskalc.Heroes`, `Siniskalc.Heroes.Wpf`) and their tests must not name a HoMM2 or HoMM3 type.
   Methods marked `[SetUp]`, `[OneTimeSetUp]`, `[TearDown]` or `[OneTimeTearDown]` (`Load`, `MakeTheApplication`, `CreateFolder`) are not tests; do not report their names or their lack of assertions.
5. **Excessive coverage**: the same behaviour asserted in several classes, the same save loaded in many tests, parameter sets far larger than the behaviour needs, and "every savefile" sources.
6. **Gaps**: production classes with no test class of their own (list the files under `Siniskalc*/` and grep `Tests/` for each type name), classes covered only through savefile tests, and the `CLAUDE.md` rules (an unknown byte reads as a stand-in and does not stop a load; the serializer leaves that byte alone while the model holds the stand-in) for each serializer and deserializer.
   Also missing error paths (truncated or malformed input).

## Run time

Build and test outside the build output, which the running exe may lock:

```
dotnet test Siniskalc.sln --artifacts-path %TEMP%\siniskalc-review\art --logger "trx;LogFilePrefix=r" --results-directory %TEMP%\siniskalc-review\trx
```

Parse the TRX files (`UnitTestResult/@duration`, joined to `UnitTest/TestMethod/@className`) into durations per test and per class, and check that their sum is close to the test host time.
Flag **unit and multi-unit tests over 100 ms** and **integration tests over 2 s**.
Only integration tests, tests that read files, and tests on binary data (deserializers, serializers) may be slow; a slow test of another kind means hidden I/O (usually a save loaded in `[SetUp]`).
Do not flag the first test of a class just for JIT or starting WPF.
`Siniskalc.Homm3.Tests` takes about 13 minutes and prints nothing for about 2 minutes while one test loads every save; wait at least 15 minutes, and if you add `--blame-hang-timeout`, set it to 10 minutes or more.
A long silence is not a hang until the blame dump says so.

## Method

- Split the work: one reviewer per test folder (Core + Wpf + root/Entities/Models/Runtime, Data in two or three parts, ViewModels), each reading every file in full and opening the production class when needed. Run the tests in parallel.
- Verify every finding at its line before reporting it; drop what you cannot confirm.
  Reviewers over-report: check that a "test without assertions" is not a setup method, that a "typo" is not a planned project (`Fheroes` is fheroes2), and that a "unit test" really does or does not load a save (grep for `TestServices`, `Saves.`, `CastleSamples`, `TestData`).
- Report `[Explicit]` tests that write files as manual helpers, not as weak tests, unless they write outside the temp folder or are not `[Explicit]`.
  Count the effect where you can (for example how many cases end in `Assert.Pass`).

## Report (`docs/TEST_REVIEW.md`)

Follow the repository's doc style: one sentence per line, short, no filler.
Sections: summary (counts per kind, totals, run time per project); how it was done; a table of every test class (class, project/folder, kind, tests, total s, max s, what it reads, notes); run time (slowest classes and why, tests over the thresholds); inconclusive and weak tests (file:line, problem, fix); unit tests that are not isolated; placement and naming; excessive coverage; gaps; the conventions (categories plus folders, Moq, load each save once, no `Assert.Pass`/silent `continue`); and a backlog in order of value.