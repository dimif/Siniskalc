# Changelog

All notable changes to Siniskalc.
Versions follow [Semantic Versioning](https://semver.org/); a release is made by pushing a tag `vMAJOR.MINOR.PATCH` (see `.github/workflows/release.yml`).

## [Unreleased]

- The HoMM3 code moved from the exe into the library `Siniskalc.Homm3` (namespaces `Siniskalc.Homm3.*`), and its tests into `Siniskalc.Homm3.Tests`; the exe `Siniskalc` keeps `App` and `AppServices`. No change in behaviour (2026-10-12).
  This prepares support for Heroes of Might and Magic II (`docs/HOMM2_PLAN.md`).

- Test savegames were renamed to the rules in `TestData/README.md` (2026-10-12).
  Old names were like `0_roe_tests_baseline`, `4_roe_cita`, `15_dwell_3`, `1_arts`, `111` to `214` or `2PHotseat/_GAME_BEGIN`; `git log --follow` on a save shows its old name.
  Where the new name is a guess, the folder's `README.md` says so.

## [1.0.0]

- First release.
