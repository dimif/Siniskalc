# Future Improvements

What is planned or open.
What is already built is in `CHANGELOG.md` and `docs/SINISKALC_ARCHITECTURE.md`; what the savefiles showed is in `docs/KNOWLEDGE_BASE.md`.

## Project structure

- **Split the HoMM3 code from the application.** When HoMM2 work starts, move the HoMM3 code into `Siniskalc.Homm3` and keep the exe (`AppServices`, `MainWindow`, `MainViewModel`) as a thin host that picks the game from the savefile.
  Only then is it clear which hero, castle and creature types the two games share; expect to fork more than to share.
- `NavNodeViewModel` and `NavigationTree` are HoMM3-shaped (hero, player and castle indexes).
  Make a generic node when the second game has a tree of its own.
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