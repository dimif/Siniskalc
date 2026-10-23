# Error model

What can go wrong, what the code throws, who catches it and what the user is told.
What is written to the log file, and when, is in `LOGGING.md`. Every place that was marked `// TODO:LOG` now logs; a class that gets no logger yet may still carry the marker, and `grep -rn "TODO:LOG"` lists them.

## Kinds of failure

| Kind | Type | Thrown by | Caught by | The user sees |
|---|---|---|---|---|
| The file is not a compressed savegame, or is damaged | `CorruptSavefileException` (`SavefileException`) | `GzipSavefileCodec.Read` | `SavegameFileCommands.Run` | its message, as it is |
| The file is a savegame of nothing this editor knows (another game, a map, an unknown version) | `UnsupportedSavefileException` (`SavefileException`) | `SavegameParser.Parse` | `SavegameFileCommands.Run` | its message, as it is |
| The file cannot be opened, read or written (missing, locked, disk full) | `IOException` and kin | the codec, `NumberedSavefileBackupService` | `SavegameFileCommands.Run` | "Failed to ...: message" |
| An edit the rules of the game do not allow | `GameRuleException` (an `InvalidOperationException`) | `CastleBuilder` | `CastleViewModel.Edit` | the notice on the castle page; nothing is changed |
| A value outside what a field can hold | `ArgumentOutOfRangeException` | `RangeGuard`, models | nobody: view model setters clamp first, so this is a bug | the unexpected error box (`App`) |
| A fault of the program (bad index, broken invariant) | `ArgumentException`, `InvalidOperationException`, `IndexOutOfRangeException`... | anywhere | `App.OnUnhandledException` | "Unexpected error" (`ErrorWindow`): the message, and a folded "Details" section with the exception and its stack trace that can be selected and copied (button "Copy details"); the open game is kept so it can be saved |

## Rules

- **A value the editor does not know never stops a load.** Deserializers read it as a stand-in (or as nothing) and the serializer keeps the original byte (see `CLAUDE.md`).
  Each is logged at Trace with the record as its subject (see `LOGGING.md`).
- **Parsing only slices bytes; the models are read when a page needs them.** `Homm3EditorViewModel.SetGame` builds what reads the new game (the overview, the owners) before it lets go of the open game, so a game that cannot be shown leaves the open one as it was.
- **A save is all or nothing.** `GameService.SaveGame` works out the bytes, makes the backup, then `GzipSavefileCodec.Write` writes a `.saving` file and moves it over the savefile.
  Only after that is the game tied to the file (`Game.SavedTo`) and marked saved.
  If any step throws, the savefile, `Game.File` and the pending changes are as they were.
  A backup that fails stops the save.
- **A settings file that cannot be read stops the start, and the user decides.** A missing file means the defaults; a file that exists but cannot be read or understood (not JSON, a `LogLevel` that means nothing) throws `SettingsException` with its path. `App.StartServices` shows it and offers *Reset* (the file is kept as `settings.json.bad` and the editor starts with the defaults) or *Exit*, so a damaged file never locks the user out. A settings change that cannot be written throws `SettingsException` too and reaches the user as an unexpected error; the change is kept for the session.
- **A log folder the user chose that cannot be used stops the start** (`LoggingException`, named folder, shown by `App.StartServices`). The default folder never does: without it the editor logs to the debug output only.
- **Opening the browser never throws**: `ShellUrlLauncher.TryOpen` returns false and the window shows the address instead.
- **The file commands go through `SavegameFileCommands`** (open, save, save as, reload, export, load bytes, close): it asks for files through `IFileDialogs`, runs the work in `Run`, and is the one place that words a failure ("Failed to ..." or the message of a savefile exception).
- **Dialogs go through `IMessageService`** (`WpfMessageService`), so they are owned by the window the user is in and tests can replace them.
  `HexEditor` is a control without a service and uses the window that holds it.

## Games without a file

`Game.File` is the file a game is tied to: where it was loaded from or last saved to, and `null` for a game loaded from a dump.
Reload needs it (`Homm3EditorViewModel.CanReload`, which the shell mirrors); `Game.Name` is what to call the game either way.
