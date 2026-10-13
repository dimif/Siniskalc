# Changelog

All notable changes to Siniskalc.
Versions follow [Semantic Versioning](https://semver.org/); a release is made by pushing a tag `vMAJOR.MINOR.PATCH` (see `.github/workflows/release.yml`).

## [Unreleased]

- **Banishing**: a player who owns no town shows how many days are left before the game banishes him: a pill beside the name on the player page ("No towns: 6 days left", "Banished"), the same pill on the player's card in the game overview, and "6 days left" after the player in the navigation tree. The first tab of the player page is now called **General**, and for such a player it has a Banishing card with the buttons 7 to 1 to change the days left; a banished player cannot be edited. The days are byte 22 of the player struct (`docs/KNOWLEDGE_BASE.md`) (2026-10-13).
- **Heroes of Might and Magic II** (work in progress): the new project `Siniskalc.Homm2` reads and writes the savegames of the base game (`.GM1`) and of the Price of Loyalty (`.GX1`): heroes, players and castles, with undo and pending changes. The application opens them next to the HoMM3 ones: the shell picks the game from the file and the window title follows it. The editor has the same layout as the HoMM3 one (navigation tree of players, heroes and towns, tabs, "edited" marks and Reset); its pages edit the resources of the players, the heroes (skills, army, spells, artifacts) and the castles (owner, buildings, dwellings, garrison, Mage Guild). None of it has been tried in the game yet beyond the saves of the write test (see `docs/HOMM2_PLAN.md`). The format is in `docs/HOMM2_SAVE_FORMAT.md` (2026-10-12).
- `RawSavefileCodec` in `Siniskalc.Core` reads and writes a savegame that is not compressed.

- The HoMM3 code moved from the exe into the library `Siniskalc.Homm3` (namespaces `Siniskalc.Homm3.*`), and its tests into `Siniskalc.Homm3.Tests`; the exe `Siniskalc` keeps `App` and `AppServices`. No change in behaviour (2026-10-12).
  This prepares support for Heroes of Might and Magic II (`docs/HOMM2_PLAN.md`).
- The window, the menus and the file commands are a game-neutral shell in `Siniskalc.Wpf` (`ShellWindow`, `ShellViewModel`, `SavegameFileCommands`); a game is an `IGameModule` with an `IGameEditor`, and the HoMM3 editor is one of them. The menu icons moved to `Siniskalc.Wpf/Resources`. No change in behaviour (2026-10-12).

- Test savegames were renamed to the rules in `TestData/README.md` (2026-10-12).
  Old names were like `0_roe_tests_baseline`, `4_roe_cita`, `15_dwell_3`, `1_arts`, `111` to `214` or `2PHotseat/_GAME_BEGIN`; `git log --follow` on a save shows its old name.
  Where the new name is a guess, the folder's `README.md` says so.

## [1.0.0]

- First release.
