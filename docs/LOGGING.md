# Logging

How and what the editor logs. These rules apply to people and agents alike, to both games, and to every project that is not the exe.
Failures that the user must see are in `ERROR_MODEL.md`; this file is about what goes to the log file.

## At a glance

| Question | Answer |
|---|---|
| Where do I get a logger? | `private static readonly ILogger<T> Logger = LogFactory.For<T>();` (`ILogger Logger = LogFactory.For(typeof(T))` in a static or generic class). Not through the constructor, and never `Serilog.Log`. Only the exe (`LoggingSetup`) names Serilog or `ILoggerFactory`. |
| Who installs the real logger? | `AppServices` (the composition root) calls `LogFactory.Install` once with the factory `LoggingSetup` builds in the exe; until then, and in the tests, loggers write nowhere. Core, Heroes, Homm2, Homm3 and the Wpf projects know no Serilog. |
| Where is the file? | `<LocalApplicationData>/Siniskalc/logs/siniskalc-<date>.log` (`LogPaths`), unless `LogFolder` is set in `settings.json`. Settings... (Logging) changes the folder and the level, and Help > Show logs opens the folder. The file is shared, so two copies of the editor can log to it. |
| How do I write a message? | A constant template with named placeholders, never `$"..."`: `Logger.LogWarning("Skipped the backup of {File}", name)`. `CA2254` enforces it. |
| Which level? | Warning = surprising but handled. Error = the user saw an operation fail. See the table below. |
| Where not to log? | Models and reducers, views, getters, anything per byte or per item of a list, anything that runs on every page change. See "Where to log" and "Spam". |
| Which property names? | The vocabulary under "How to write a message". Add to it before inventing a name. |
| Checked by | The `logging-reviewer` agent (`.claude/agents/logging-reviewer.md`), against the checklist at the end of this file. |

## The configuration files

Two JSON files, both shipped inside the application and copied to the folder of the settings (`%APPDATA%\Siniskalc` on Windows; `UserSettings.PathFor`)
the first time the editor starts, so the user can read and edit them. The committed copies are the ones to change for a new default.

| File | Shipped as | What it holds |
|---|---|---|
| `settings.json` | `Siniskalc.Core/Resources/settings.default.json` | The preferences of the user: recent files, `BackupOnSave`, `AdvancedMode`, and the two logging choices `LogFolder` and `LogLevel` (both `null` = "use `serilog.json`"). A test (`DefaultSettingsTests`) keeps it equal to the defaults of `UserSettings`. |
| `serilog.json` | `Siniskalc/serilog.json` | The Serilog configuration in the standard `Serilog` section (Serilog.Settings.Configuration): `MinimumLevel`, `Enrich`, and `WriteTo` with the Debug, Console and File sinks, each with the `ExpressionTemplate` formatter (the template with the `Subject`), and for File the `path`, `rollingInterval` (Day), `retainedFileCountLimit` (7 days) and `shared`. |

How they meet (`LoggingSetup`):

- `{LogFolder}` in the `path` of `serilog.json` is replaced by the log folder: `LogFolder` of `settings.json`, else `<LocalApplicationData>/Siniskalc/logs`
  (`LogPaths.Resolve`). Write the path with `/`, so the file holds on Linux and macOS too.
- `LogLevel` of `settings.json`, when it is set, replaces `MinimumLevel.Default`. Everything else (sinks, template, rolling interval, days kept) comes
  only from `serilog.json`; `MinimumLevel.Override` entries of the file still apply.
- A `serilog.json` that cannot be parsed, or names a sink, formatter or argument Serilog cannot build, stops the start with a message that names the
  file (`LoggingException`); the start-up dialog offers to put it aside as `serilog.json.bad`, and the shipped one is written again.

## Where to log

Log at **boundaries and decisions**:

- File work: open, read, save, backup, export, load of bytes.
- Savegame detection and parsing: which game and version was found, which blocks, what was rejected and why.
- Settings and logging start-up.
- Unhandled exceptions (`App.OnUnhandledException`).
- Every place that fell back on something unexpected: an unknown value read as a stand-in, a byte kept because the model cannot
  represent it, a heuristic that did not match. These are the former `// TODO:LOG` markers.
- Coarse user commands (open, save, save as, reload, export, close) when they succeed, at Information.

Do **not** log:

- In `Models` or reducers. They are pure and immutable.
- In views, XAML code-behind, converters, getters or bindings.
- Per byte, per hero in a list, per frame, per property change. A loop logs one summary line after it (`Read {Count} heroes`).
- In a handler that runs on every page change or refresh (`Refresh`, `PropertyChanged`), unless it is Trace and says something a decoder did.

One kind of line is allowed although it runs often: **one Debug line per user command or edit** (apply, undo, redo, a setting the user changed:
`SaveDocumentHistory`, `EditSession`, `SettingsStore`), and one Trace line per history change. It is bounded by what the user does, not by the size of the savegame.

## Spam

A normal session (open, edit a few things, save) writes a few Information lines per user command and no Warning.
Debug and Trace volume is bounded by "per record, not per byte".

- Anything above Trace inside a loop over items is wrong, however short the loop (8 players, 7 Mage Guild levels): count in the loop and log a summary after it.
- A line that repeats the same text with the same values on every call is spam. Log the first and a count, or move it to Trace.
- A retry loop logs the first failure and the final outcome, not every attempt.
- A `catch` that is expected control flow (a probe that fails on purpose) logs nothing, or Trace.
- If a line fires on every normal load, it is not a Warning.
- Arguments must be cheap and already computed: no LINQ, formatting or `ToString()` only to log. The code uses no `IsEnabled` guards and no `LoggerMessage`
  generators; the volume above Trace is small and Trace is off by default, so do not add either without a measurement.

A view that shows a failure (a message box in code-behind) hands it to a view model that logs it; the view itself does not log.
The catches of start-up in `App.xaml.cs` run before the logger is installed and cannot log.

Log **once, where the failure is handled** (the catch that turns it into a message for the user, or the code that chooses the
fallback). Do not log at every layer the exception passes through, and do not log and rethrow.

A deserializer that reads a byte it does not know as a stand-in logs it at **Trace**, on every read, with no de-duplication: a
model is read again whenever a page needs it, so the same line repeats. That is acceptable because Trace is off by default; the
noise is handled with the level and filters, not in the code. Such a line says what the byte was and what it read as, and puts
the record it is about in a *subject* (next section).

## Levels

Choose the level in this order:

1. The operation failed and the user was told: **Error**. The app cannot start or continue: **Critical**.
2. The editor met something it did not expect and carried on with a fallback: **Warning**.
   A file the editor refuses (an unsupported or corrupt savegame, a `SavefileException`) is logged once, as a Warning, by the code that shows it to the user (`SavegameFileCommands.ReportError`); the parser, the codec and the locator that throw it log nothing above Debug.
3. The user's input was rejected on purpose (a refused edit value): **Information** or Debug, never Warning.
4. A lifecycle step or user command that worked: **Information**.
5. Flow and decisions a developer wants to see: **Debug**.
6. Fine-grained detail of a decoder, per record or per field: **Trace**. A lifecycle-style Trace line is a nit, not a defect.

| Level | Use | Example |
|---|---|---|
| Trace | Per-item decode detail. Off by default. | each offset a locator tried; a byte a deserializer read as a stand-in |
| Debug | Flow detail for a developer. | block found at offset, length, the heuristic that chose the version |
| Information | Lifecycle and user commands that worked. | app started (version, OS, runtime), savegame opened or saved (game, version), backup made |
| Warning | The editor met something unexpected and carried on. | backup skipped, a temporary file that could not be removed |
| Error | An operation failed and the user was told. | file unreadable, corrupt or unsupported savegame, save failed |
| Critical | An unhandled exception, or the app cannot start or continue. | `OnUnhandledException`, a settings or logging failure at start-up |

The minimum level is Information as shipped (Debug while a debugger is attached). It is `MinimumLevel.Default` in `serilog.json`; `LogLevel` in
`settings.json` (Settings... > Logging) replaces it, and left empty it leaves the level to `serilog.json`.

## How to write a message

- **Templates, not strings.** The template is a constant; values go in as arguments so the file keeps them as named properties.
  Do not use `$"..."`, `string.Format` or `+`.
- **Text.** Starts with a capital, no trailing period, no line breaks. Past tense for something that happened
  (`Opened savegame {File}`), present tense for a state (`No dwelling found for {Colour}`).
- **Property names** are PascalCase and come from this vocabulary, so a search finds them across the code:
  `File`, `Game`, `Version`, `Chunk`, `Offset`, `Length`, `Count`, `HeroId`, `Colour`, `Slot`, `Page`, `Code`, `StandIn`,
  `Kind` (a chunk kind), `Index` (the place of a record among its kind), `Name`, `Local` (whether a change came from the target's own update),
  `Path` (the settings file or the log folder, where the path is the problem), `Folder`, `Url`, `Backup`, `BackupOnSave`, `AdvancedMode`,
  `Reason` (the user-facing reason of a refusal; the one place a message text may be an argument), `Title`, `What` (the name of an action),
  `Value` (a raw value that is no known entity), `Difficulty`, `Date`, `DateOffset` (hex), `Level`, `Skill`, `Size` (a map size), `Player`, `Building`, `MaskOffset`, `FlagsOffset`, `DescriptionOffset`, `ChunkLength`, `Overlaps`,
  `App`, `Os`, `Runtime`,
  `Control`, `Binding`, `Change`, `Changed`, `Undo`, `Redo`, `Source`, `ResourceName`, `HeaderLength`,
  and the plural counts `Players`, `Heroes`, `Castles`, `Mines`, `Dwellings`, `Chunks`, `Slots`.
  `Count` is a count of records, `Length` is a count of bytes, `Index` is a position or a number of a copy.
  `Count` and `Length` print in decimal; only offsets and raw codes print in hex.
  A name means one thing in a template: two offsets get two names.
  Add to the list here before inventing a new one.
- **Hex.** Offsets and raw byte values are formatted as hex, as the hex editor shows them: `{Offset:X}`, `{Code:X2}`.
- **Exceptions** go in the exception argument, never as `ex.Message` inside the template:
  `Logger.LogError(ex, "Failed to save {File}", name)`.
  `LogError(ex, ...)` means the user saw the failure; `LogWarning(ex, ...)` means it was swallowed.
  The only `ex.Message` allowed as an argument is the user-facing reason of a refused edit (`{Reason}`).
- **Stable templates.** The same event has the same text every time, so a search or a group-by finds all of them. Put what varies in a placeholder.
  Pass ids, enums and numbers, not strings built from them. One fact per placeholder.
  The one per-edit Debug line of `EditSession` starts with `{What}` and so varies by command; that is accepted.
- **Objects.** Never log a model, a chunk or a collection (no `@` destructuring); log `{Count}` and at most a few ids.
- **Layout** (timestamp, level, category) belongs to the template in `serilog.json`. Code does not format any of it.
  Serilog writes the levels as `VRB` (Trace), `DBG`, `INF`, `WRN`, `ERR` and `FTL` (Critical).
- **Subject.** A line about one record does not repeat its name in the message: the code that reads the record opens
  `using var subject = Logger.BeginSubject("Hero", chunk.Name);` (`LogScopeExtensions`, `Kind Name`: `Hero`, `Castle`, `Mine`) and every line
  logged inside it, by any class and any static helper (`ArmyReader.Read`, `Bytes.ReadColor`), shows
  `Category (Hero Sandro): message`. It is the property `Subject`, so a filter can select it. The serializers read bytes with the same
  helpers to compare, so they log the same Trace fallbacks; filter by level when that is too much.

The file is plain text, one event per line, followed by the exception when there is one:

```
2026-10-23 14:02:11.532 +02:00 [VRB] Siniskalc.Homm3.Data.HeroDeserializer (Hero Sandro): Class byte FE read as Knight and kept until the class is changed
```

## What to include, and what never to

Include enough to reproduce the problem without the savegame:

- game and game version, chunk kind, offset and length;
- the raw value that was read and the stand-in or fallback used;
- the id of the hero, castle or player involved, and the command or page the user was on.

Never include:

- **Savegame contents or byte dumps.** An offset and a length are enough.
- **Full paths of savegames** at Information or above. Name the file (`{File}` is the file name). The full path is allowed only
  when the path is the problem (the settings file, the log folder) or at Debug. Logs get attached to bug reports and paths hold
  the user name.
- Settings values other than the one being reported, and anything secret.

## Checklist for reviewers

The `logging-reviewer` agent cites these numbers.

- **L1** The logger comes from `LogFactory.For`, in a `private static readonly` field; no constructor logger, no `Serilog.Log`, no `Console`/`Debug` writes (the exe aside).
- **L2** The template is a constant; no `$""`, `string.Format` or `+`.
- **L3** The exception is the exception argument; no `ex.Message` in the arguments except `{Reason}`.
- **L4** Property names are PascalCase and in the vocabulary; offsets and raw codes are hex; `Count` counts records, `Length` bytes.
- **L5** Text starts with a capital, no trailing period, no line breaks.
- **L6** The level follows "Levels": Error only when the user was told, Warning only for an unexpected fallback.
- **L7** No logging in models, reducers, views, converters, getters or per-page handlers.
- **L8** Nothing above Trace inside a loop over items; no repeated identical lines; no expensive arguments.
- **L9** A failure is logged once, where it is handled; no log-and-rethrow; a catch that chooses a fallback logs it.
- **L10** No savegame contents, byte dumps, full savegame paths at Information or above, settings values or secrets.
- **L11** The template is stable and the values are properties; no model, chunk or collection as an argument.
- **L12** `BeginSubject` is disposed (`using var`) and wraps one record, not a loop over a collection.
- **L13** No `TODO:LOG` marker remains; tests do not assert call sites, and a test that installs a real factory calls `LogFactory.Reset()`.

## Cross-platform

Logging code must stay portable: build paths with `Path.Combine` and `Environment.GetFolderPath`, use no Windows-only sink (no
EventLog), and name no WPF type outside the exe. The only WPF line is flushing the logger in `App.OnExit`.

## Working with the markers

- A `// TODO:LOG <level>: ...` marker is replaced by the real call, at the level it names, and the marker is deleted.
- New "unexpected, carried on" code gets the call directly. Do not add new `TODO:LOG` markers: every class can have a logger.
- Logging is not unit-tested: the code has no conditional logging, only calls at different levels. The tests that touch it check the
  infrastructure (`LogFactoryTests`, `LoggingTests`: the log file, the subject, the start-up errors). Tests that build `AppServices` pass
  `NullLoggerFactory.Instance`, and a test that installs a real factory calls `LogFactory.Reset()` afterwards.
