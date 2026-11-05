# Error model

What can go wrong, what the code throws, who catches it and what the user is told.
What is written to the log file, and when, is in `LOGGING.md`. Every place that was marked `// TODO:LOG` now logs, and no marker remains.

## Kinds of failure

| Kind | Type | Thrown by | Caught by | The user sees |
|---|---|---|---|---|
| Standard game definitions are missing, malformed or inconsistent | `GameDefinitionException` | the JSON reader or a game's catalog loader | `App.StartServices` | the definition file and cause, with an instruction to correct or restore it and restart |
| The file is not a compressed savegame, or is damaged | `CorruptSavefileException` (`SavefileException`) | `GzipSavefileCodec.Read` (HoMM3; HoMM2 files are not compressed) | `SavegameFileCommands.Run` | its message, as it is |
| The file is a savegame of nothing this editor knows (another game, a map, an unknown version) | `UnsupportedSavefileException` (`SavefileException`) | each game's `SavegameParser.Parse` | `SavegameFileCommands.Run` | its message, as it is |
| The file cannot be opened, read or written (missing, locked, disk full) | `IOException` and kin | the codecs (`GzipSavefileCodec`, `RawSavefileCodec`), `NumberedSavefileBackupService` | `SavegameFileCommands.Run` | "Failed to ...: message" |
| An edit the rules of the game do not allow | `GameRuleException` (an `InvalidOperationException`) | `CastleBuilder`, the artifact slots of a hero, `PlayerListsReconciler` | `SessionPageViewModelBase.TryUpdate` | the notice on the page; nothing is changed |
| A value outside what a field can hold | `ArgumentOutOfRangeException` | `RangeGuard`, models | nobody: view model setters clamp first, so this is a bug | the unexpected error box (`App`) |
| A fault of the program (bad index, broken invariant) | `ArgumentException`, `InvalidOperationException`, `IndexOutOfRangeException`... | anywhere | `App.OnUnhandledException` | "Unexpected error" (`ErrorWindow`): the message, and a folded "Details" section with the exception and its stack trace that can be selected and copied (button "Copy details"); the open game is kept so it can be saved |

## Rules

- **A value the editor does not know never stops a load.** Deserializers read it as a stand-in (or as nothing) and the serializer keeps the original byte (see `CLAUDE.md`).
  Each is logged at Trace with the record as its subject (see `LOGGING.md`).
- **Invalid standard game definitions stop startup.** Both games' shipped JSON is read and validated before the editor is available. A missing file, unsupported schema, malformed value or inconsistent reference is not an unknown savefile value: it throws `GameDefinitionException` and the application reports the file and cause. There is no hardcoded fallback, partial catalog, reset-to-default operation or automatic reload. Correct or restore the JSON and restart the editor.
- **Parsing only slices bytes; the models are read when a page needs them.** `EditorViewModelBase.SetGame` builds what reads the new game (`PrepareGame`: the overview, the owners) before it lets go of the open game, so a game that cannot be shown leaves the open one as it was.
- **A save is all or nothing.** `SavegameService<Game>.SaveGame` works out the bytes, makes the backup, then the codec's `Write` writes a `.saving` file and moves it over the savefile.
  Only after that is the game tied to the file (`SavedTo`) and marked saved.
  If any step throws, the savefile, the file name of the game and the pending changes are as they were.
  A backup that fails stops the save.
- **A settings file that cannot be read stops the start, and the user decides.** A missing file means the defaults; a file that exists but cannot be read or understood (not JSON, a `LogLevel` that means nothing) throws `SettingsException` with its path. `App.StartServices` shows it and offers *Reset* (the file is kept as `settings.json.bad` and the editor starts with the defaults) or *Exit*, so a damaged file never locks the user out. A settings change that cannot be written throws `SettingsException` too and reaches the user as an unexpected error; the change is kept for the session.
- **A Serilog configuration that cannot be used stops the start, and the user decides.** `serilog.json` (beside `settings.json`, see `LOGGING.md`) that cannot be read or parsed, or names a sink, formatter or argument that cannot be built, throws `LoggingException` with its path; `App.StartServices` offers *Reset* (the file is kept as `serilog.json.bad` and the shipped one is written again) or *Exit*. A missing file is written from the shipped copy; if that cannot be done, the shipped text is used as it is.
- **A log folder the user chose that cannot be used stops the start** (`LoggingException`, named folder, shown by `App.StartServices`). The default folder never does: without it the editor logs to the debug output only.
- **Opening the browser never throws**: `ShellUrlLauncher.TryOpen` returns false and the window shows the address instead.
- **The file commands go through `SavegameFileCommands`** (open, save, save as, reload, export, load bytes, close): it asks for files through `IFileDialogs`, runs the work in `Run`, and is the one place that words a failure ("Failed to ..." or the message of a savefile exception).
- **Dialogs go through `IMessageService`** (`WpfMessageService`), so they are owned by the window the user is in and tests can replace them.
  `HexEditor` is a control without a service and uses the window that holds it.

## Games without a file

`Savegame.FileName` is the file a game is tied to: where it was loaded from or last saved to, and `null` for a game loaded from a dump.
Reload needs it (`ShellViewModel.CanReload`, true when the shell has a file).
