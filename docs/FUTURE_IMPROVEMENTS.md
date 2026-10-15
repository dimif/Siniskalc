# Future Improvements

What is planned or open.
What is already built is in `CHANGELOG.md` and `docs/SINISKALC_ARCHITECTURE.md`; what the savefiles showed is in `docs/KNOWLEDGE_BASE.md`.

## Project structure

- **Game modules** (done): the HoMM3 code is in `Siniskalc.Homm3` and the exe is a thin host with a shell (`IGameModule`, `IGameEditor`, `ShellViewModel`, `ShellWindow`); see `docs/HOMM2_PLAN.md`.
  What the two games share is decided (see `Siniskalc.Heroes` and "Adding a game" in `docs/SINISKALC_ARCHITECTURE.md`): the values that mean the same (`Resource`, `GameDate`, `TavernHeroes`, `ChangeAreas`), interfaces for the entities each game has its own set of (colours, factions, skills, skill levels), the blocks the pages are made of, and the shell, the navigation tree and the editor base.
  Creatures, spells, artifacts, positions and map headers are not shared: they differ in codes, sets or shape.
- The navigation tree, its node and the editor view are shared (`Siniskalc.Wpf`); each game has a `NavigationBuilder`.
- **A second savefile format of a game we have** (fheroes2 for HoMM2, VCMI for HoMM3): the entities, models and pages are the same, the format is not.
  Split `Siniskalc.Homm2` / `Siniskalc.Homm3` into the rules (entities, models, reducers, pages) and the format (locator, chunks, (de)serializers) when the first of them is started; the editor view models and pages already work with models and the collection classes, never with chunks.
- **A third game** (HoMM1 first, as the test that the page interfaces are right): see "Adding a game" in the architecture document.
- **Names behind a provider.** Names of creatures, spells and artifacts are string literals in the entity tables.
  Before a third game, one injected name provider (like `EffectLineProvider`) so a creature is named once and the UI can be localized.
- **Shared page view models.** The pages of the two games look and work the same and bind to the same interfaces (`IPlayerPage`, `ICastlePage`), but each game's view model is its own.
  There is no hero page interface yet: the tabs of the two heroes still hold content only their game has (HoMM3: backpack, war machines, hero class).
- **View models free of WPF.** The view models in `Siniskalc.Wpf` and `Siniskalc.Heroes.Wpf` use little of WPF (`ICommand`, `INotifyPropertyChanged`); moving them to `net8.0` projects would leave only the views to rewrite for Avalonia.
- **Cross platform support.** The core logic is already platform-agnostic; the WPF UI could be replaced with Avalonia or MAUI.
- **State store for hero edits.** The hero edits are reducers on immutable models.
  A single store holding the document and applying reducers would replace the per-view-model `Update` callbacks and the `Refresh()` calls that follow `History.Changed`.
- **Editable store for game entities** Entities (heroes, castles, creatures) are hardcoded in the editor.
  A store of entities that can be edited and saved would let the editor support mods and future games.
- **Logging** Add serilog to the project for debug logs, and a log viewer to the UI for users to report issues.

## User features

1. **Swapping the garrisoned and the visiting hero** of a castle (and moving a hero in or out): changes the castle, two hero chunks and the player's list of heroes together, which no edit session does today.
2. **Castle creatures growth**: we think we have found the growth table is found, but it isn't read.
3. **Editing of the date, and whose turn it is**
4. **Resource generators (mines etc.)**: as we're starting to understand tile structure, we could extract all the buildings from the save and make the owner editeable.
5. **Teams**: the team table of the header is found (Shadow of Death and Restoration of Erathia; not in Horn of the Abyss) but not read.
   Reading it would let the obelisk card mark a whole team at once.
6. **Dug tiles and the visited-object table**: readable, but the meaning of the number and of the table is unclear.
   The Grail flag beside the date (`7F xx 02 yy`) is the same.
7. **A defeated hero and the tavern**: what keeps a defeated hero from coming back is not yet known.
   The tavern has table about 250 bytes before the date that changes when a hero is defeated, so it's probably that one.
8. **Banishing, what is left** (the counter itself is found: byte 22 of the player struct, shown on the player page and editable; see `docs/KNOWLEDGE_BASE.md`, "Players").
   - Check in the game that a lower or higher number of days makes the game banish the player earlier or later.
   - The other bytes banishing changes are not decoded: the hero chunk of the player (owner `FF`, army emptied), a counter and an 8-byte record near the end of the save, and header bytes near 1313, 1356 and 12492.
     Until they are, a banished player (0) is read-only and no one can be banished or revived by the editor.
   - The list of the indices of a player's towns that starts at byte 25 is not read.
