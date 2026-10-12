# TestData: savefiles and how they are named

Real savegames (and a few raw chunks) that the tests read.
**Read this before adding or renaming a file.** `TestDataNamesTests` fails when a file breaks the rules below, and `SeriesSaves` (in `Tests/Siniskalc.Tests/Data`) finds the saves of a series by these names.

Each folder has its own `README.md` that says what the saves hold and which tests use them.
Add a row there for every save you add.

## Layout

```
TestData/<Version>/<Series folder>/<series>_<NN>_<what changed>.GM1
```

- **`Roe`, `Sod`, `Hota`** hold the saves of that game version.
  `Heroes` holds raw hero chunks (`.bin`), not savegames.
- **A series is a run of saves made one after the other** in one game, each differing from the one before by one thing.
  A series with many saves gets its own folder (`Hota/Rampart`, `Hota/Bulwark`); a short one may stay in the version folder (`Hota/castles_00_baseline`).
  The version folders `Roe` and `Sod` hold several series, told apart by their prefix (`roe_`, `sanya_`, `conflux_`, `sod_game_`).
- **Folder names are PascalCase**, with no leading digit (`Hotseat`, not `2PHotseat`).
- A save that stands alone is `<what it is>.GM1` (`bios_custom`, `map_difficulty_edited_to_expert`).

## File names

- **Lower-case snake_case**: letters, digits and single underscores.
  No spaces, no capitals, no leading or trailing underscore.
- **The extension is the game's own, in capitals**: `.GM1` (Roe, Sod, Hota), `.GM2` (hotseat), `.GM3`.
  Raw chunks are `.bin`.
- **The prefix is the series.** In a series folder it is the folder name in snake_case (`OtherFactions` is `other_factions`, `SleepGarrisonVisiting` is `sleep_garrison_visiting`); in `Roe`, `Sod` and `Hota` it is the name of the series.
  The prefix may contain digits only inside a word (`m5w4d6`); a number that orders the saves is always the two digits that follow the prefix.
- **`NN` is two digits and orders the saves of a series**: `rampart_07_mage_guild_1`.
  - A baseline (the unchanged start, which later saves are compared with) is `00_baseline`.
    A series with no baseline starts at `01`.
  - Two saves made at the same step share the number and the second gets a letter: `roe_15_dwelling_2`, `roe_15b_dwelling_3`.
    Numbers are unique in a series, with or without the letter (a test checks this).
  - **Never renumber a save to close a gap or to start at `00`.** Tests, READMEs and `docs/KNOWLEDGE_BASE.md` refer to saves by number (`Roe` has no 28 and 69, `Factory` starts at 07).
    A new series starts at `00` or `01`.
- **`<what changed>` names the one thing that differs from the previous save**, the way the game or the code names it, in full: `mage_guild_2`, `town_hall`, `tazar_gets_spell_book`, not `mageguild2`, `townh`, `tazar_gets_spellbook`.
  A hero or castle comes first when it is the subject (`tazar_obelisk`, `akka_asleep`).
  Upgraded buildings start with `upg_`.
  - Two things built in one step are joined with a double underscore: `centaur_stables__colliery`.
    `nothing` stands for a slot where nothing was built.
  - A save made only to keep a state (it adds nothing to the one before) is named after the state: `all_dwellings`, `before_lighthouse`.
  - A date is written `m5w4d6` (month 5, week 4, day 6).
- **Milestones** that are not a step of the series: `<series>_milestone_<description>_after_<NN>` (`bulwark_milestone_castle_built_after_03`).
- **Variants** of one start, with no order between them, are `<series>_<variant>` with the start called `<series>_baseline` (`cove_inland_baseline`, `cove_inland_pub`, `cove_inland_nest`).
  Use this only when the saves really have no order.
- **Companion files** carry the name of their save: `empire_of_the_world_01_m5w4d6_expected_info.md`.
  The folder's `README.md` is the only other markdown name.

## Raw hero chunks (`Heroes/`)

`hero_<hero>_<state>.bin`, for example `hero_orrin_with_spellbook.bin`.
They are not savegames and have no number.

## Adding a save

1. Name it by the rules above and put it in the folder of its series (or start a series folder when there are several).
2. Add its row to the folder's `README.md`: what it is, which tests use it.
3. Refer to it in a test by its full name (`"roe_42_tazar_garrisoned"`), never by its number alone.
   A test that walks a series uses `SeriesSaves.Names(folder, series)`, which orders by number and branch letter; it does not parse the name itself.
4. If the test project does not already copy the folder (`Tests/Siniskalc.Tests/Siniskalc.Tests.csproj`), add it there.
5. Run the whole suite, as the test of one area can be affected by a save added to another (some tests read every file in `TestData`).
