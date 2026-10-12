# Changelog

All notable changes to Siniskalc.
Versions follow [Semantic Versioning](https://semver.org/); a release is made by pushing a tag `vMAJOR.MINOR.PATCH` (see `.github/workflows/release.yml`).

## [Unreleased]

- **Heroes of Might and Magic II** (work in progress): the new project `Siniskalc.Homm2` reads and writes the savegames of the base game (`.GM1`) and of the Price of Loyalty (`.GX1`): heroes, players and castles, with undo and pending changes. The application opens them next to the HoMM3 ones: the shell picks the game from the file and the window title follows it. The editor has the same layout as the HoMM3 one (navigation tree of players, heroes and towns, tabs, "edited" marks and Reset); for now its pages only show the savegame, the editing comes next. The format is in `docs/HOMM2_SAVE_FORMAT.md` (2026-10-12).
- `RawSavefileCodec` in `Siniskalc.Core` reads and writes a savegame that is not compressed.

- The HoMM3 code moved from the exe into the library `Siniskalc.Homm3` (namespaces `Siniskalc.Homm3.*`), and its tests into `Siniskalc.Homm3.Tests`; the exe `Siniskalc` keeps `App` and `AppServices`. No change in behaviour (2026-10-12).
  This prepares support for Heroes of Might and Magic II (`docs/HOMM2_PLAN.md`).
- The window, the menus and the file commands are a game-neutral shell in `Siniskalc.Wpf` (`ShellWindow`, `ShellViewModel`, `SavegameFileCommands`); a game is an `IGameModule` with an `IGameEditor`, and the HoMM3 editor is one of them. The menu icons moved to `Siniskalc.Wpf/Resources`. No change in behaviour (2026-10-12).

- Test savegames were renamed to the rules in `TestData/README.md` (2026-10-12).
  Old names were like `0_roe_tests_baseline`, `4_roe_cita`, `15_dwell_3`, `1_arts`, `111` to `214` or `2PHotseat/_GAME_BEGIN`; `git log --follow` on a save shows its old name.
  Where the new name is a guess, the folder's `README.md` says so.

## [1.0.0]

- First release.
