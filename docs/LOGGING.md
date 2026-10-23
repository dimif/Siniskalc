# Logging

How and what the editor logs. These rules apply to people and agents alike, to both games, and to every project that is not the exe.
Failures that the user must see are in `ERROR_MODEL.md`; this file is about what goes to the log file.

## At a glance

| Question | Answer |
|---|---|
| What do I inject? | `ILogger<T>` (`Microsoft.Extensions.Logging`), through the constructor, kept in a field called `logger`. Never `Log.Logger` or any static. |
| Who builds the loggers? | `AppServices` (the composition root) via `LoggingSetup` in the exe. Core, Heroes, Homm2, Homm3 and the Wpf projects know no Serilog. |
| Where is the file? | `<LocalApplicationData>/Siniskalc/logs/siniskalc-<date>.log` (`LogPaths`), unless `LogFolder` is set in `settings.json`. |
| How do I write a message? | A constant template with named placeholders, never `$"..."`: `logger.LogWarning("Skipped the backup of {File}", name)`. `CA2254` enforces it. |
| Which level? | Warning = surprising but handled. Error = the user saw an operation fail. See the table below. |
| Where not to log? | Models and reducers, views, property setters, anything per byte or per item of a list. |

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
- In views, XAML code-behind, converters, property setters or bindings.
- Per byte, per hero in a list, per frame, per property change. A loop logs one summary line after it (`Read {Count} heroes`).

Log **once, where the failure is handled** (the catch that turns it into a message for the user, or the code that chooses the
fallback). Do not log at every layer the exception passes through, and do not log and rethrow.

A deserializer that reads a byte it does not know as a stand-in logs it at **Trace**, on every read, with no de-duplication: a
model is read again whenever a page needs it, so the same line repeats. That is acceptable because Trace is off by default; the
noise is handled with the level and filters, not in the code. Such a line says what the byte was and what it read as, and puts
the record it is about in a *subject* (next section).

## Levels

| Level | Use | Example |
|---|---|---|
| Trace | Per-item decode detail. Off by default. | each offset a locator tried; a byte a deserializer read as a stand-in |
| Debug | Flow detail for a developer. | block found at offset, length, the heuristic that chose the version |
| Information | Lifecycle and user commands that worked. | app started (version, OS, runtime), savegame opened or saved (game, version), backup made |
| Warning | The editor met something unexpected and carried on. | backup skipped, a temporary file that could not be removed |
| Error | An operation failed and the user was told. | file unreadable, corrupt or unsupported savegame, save failed |
| Critical | An unhandled exception, or the app cannot start or continue. | `OnUnhandledException`, a settings or logging failure at start-up |

The minimum level is Information (Debug while a debugger is attached) and can be changed with `LogLevel` in `settings.json`.

## How to write a message

- **Templates, not strings.** The template is a constant; values go in as arguments so the file keeps them as named properties.
  Do not use `$"..."`, `string.Format` or `+`.
- **Text.** Starts with a capital, no trailing period, no line breaks. Past tense for something that happened
  (`Opened savegame {File}`), present tense for a state (`No dwelling found for {Colour}`).
- **Property names** are PascalCase and come from this vocabulary, so a search finds them across the code:
  `File`, `Game`, `Version`, `Chunk`, `Offset`, `Length`, `Count`, `HeroId`, `Colour`, `Slot`, `Page`, `Code`, `StandIn`.
  Add to the list here before inventing a new one.
- **Hex.** Offsets and raw byte values are formatted as hex, as the hex editor shows them: `{Offset:X}`, `{Code:X2}`.
- **Exceptions** go in the exception argument, never as `ex.Message` inside the template:
  `logger.LogError(ex, "Failed to save {File}", name)`.
- **Layout** (timestamp, level, category) belongs to the sink in `LoggingSetup`. Code does not format any of it.
  Serilog writes the levels as `VRB` (Trace), `DBG`, `INF`, `WRN`, `ERR` and `FTL` (Critical).
- **Subject.** A line about one record does not repeat its name in the message: the code that reads the record opens
  `using var subject = logger.BeginSubject("Hero", chunk.Name);` (`LogScopeExtensions`, `Kind Name`: `Hero`, `Castle`, `Mine`) and every line
  logged inside it, including those of helpers that were passed the logger (`ArmyReader.Read`, `Bytes.ReadColor`), shows
  `Category (Hero Sandro): message`. It is the property `Subject`, so a filter can select it. A helper that is also used by the
  serializers takes `ILogger? logger = null` and logs only when it gets one, so writing a model does not log what reading it would.

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

## Cross-platform

Logging code must stay portable: build paths with `Path.Combine` and `Environment.GetFolderPath`, use no Windows-only sink (no
EventLog), and name no WPF type outside the exe. The only WPF line is flushing the logger in `App.OnExit`.

## Working with the markers

- A `// TODO:LOG <level>: ...` marker is replaced by the real call, at the level it names, and the marker is deleted.
- Once a spot has a logger, new "unexpected, carried on" code gets the call directly. Do not add new markers in a class that
  already receives an `ILogger<T>`.
- A test that exists to prove "it logs" (a fallback, a failed read) uses a fake `ILogger`; every other test uses `NullLogger<T>`
  (or `NullLoggerFactory.Instance` for `AppServices`).
