# Error model

What can go wrong, what the code throws, who catches it and what the user is told.
There is no logging yet: every place that should log is marked `// TODO:LOG <level>: what to log`, so `grep -rn "TODO:LOG"` lists what a logger has to cover.

## Kinds of failure

| Kind | Type | Thrown by | Caught by | The user sees |
|---|---|---|---|---|
| The file is not a compressed savegame, or is damaged | `CorruptSavefileException` (`SavefileException`) | `GzipSavefileCodec.Read` | `SavegameFileCommands.Run` | its message, as it is |
| The file is a savegame of nothing this editor knows (another game, a map, an unknown version) | `UnsupportedSavefileException` (`SavefileException`) | `SavegameParser.Parse` | `SavegameFileCommands.Run` | its message, as it is |
| The file cannot be opened, read or written (missing, locked, disk full) | `IOException` and kin | the codec, `NumberedSavefileBackup` | `SavegameFileCommands.Run` | "Failed to ...: message" |
| An edit the rules of the game do not allow | `GameRuleException` (an `InvalidOperationException`) | `CastleBuilder` | `CastleViewModel.Edit` | the notice on the castle page; nothing is changed |
| A value outside what a field can hold | `ArgumentOutOfRangeException` | `RangeGuard`, models | nobody: view model setters clamp first, so this is a bug | the unexpected error box (`App`) |
| A fault of the program (bad index, broken invariant) | `ArgumentException`, `InvalidOperationException`, `IndexOutOfRangeException`... | anywhere | `App.OnUnhandledException` | "Unexpected error" (`ErrorWindow`): the message, and a folded "Details" section with the exception and its stack trace that can be selected and copied (button "Copy details"); the open game is kept so it can be saved |

## Rules

- **A value the editor does not know never stops a load.** Deserializers read it as a stand-in (or as nothing) and the serializer keeps the original byte (see `CLAUDE.md`).
  These are the places marked `TODO:LOG`.
- **Parsing only slices bytes; the models are read when a page needs them.** `MainViewModel.SetGame` builds what reads the new game (the overview, the owners) before it lets go of the open game, so a game that cannot be shown leaves the open one as it was.
- **A save is all or nothing.** `GameService.SaveGame` works out the bytes, makes the backup, then `GzipSavefileCodec.Write` writes a `.saving` file and moves it over the savefile.
  Only after that is the game tied to the file (`Game.SavedTo`) and marked saved.
  If any step throws, the savefile, `Game.File` and the pending changes are as they were.
  A backup that fails stops the save.
- **Settings never interrupt the user.** `UserSettings.Load` falls back to defaults for a missing or corrupt file (and the next save replaces it), and `Save` ignores a write that fails.
- **Opening the browser never throws**: `ShellUrlLauncher.TryOpen` returns false and the window shows the address instead.
- **The file commands go through `SavegameFileCommands`** (open, save, save as, reload, export, load bytes, close): it asks for files through `IFileDialogs`, runs the work in `Run`, and is the one place that words a failure ("Failed to ..." or the message of a savefile exception).
- **Dialogs go through `IMessageService`** (`WpfMessageService`), so they are owned by the window the user is in and tests can replace them.
  `HexEditor` is a control without a service and uses the window that holds it.

## Games without a file

`Game.File` is the file a game is tied to: where it was loaded from or last saved to, and `null` for a game loaded from a dump.
Reload needs it (`MainViewModel.CanReload`); `Game.Name` is what to call the game either way.
