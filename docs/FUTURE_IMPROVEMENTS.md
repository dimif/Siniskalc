# Future Improvements

This is work that is not yet complete. Some items build on existing read-only or partial support; those foundations and the remaining gap are called out below. What is already built is in `CHANGELOG.md` and `docs/SINISKALC_ARCHITECTURE.md`; what the savefiles showed is in `docs/KNOWLEDGE_BASE.md`.

## Project structure

- **Split the HoMM3 code from the application.** The HoMM3 code is still in `Siniskalc`; there is no `Siniskalc.Homm3` project yet. When HoMM2 work starts, move it into that project and keep the exe (`AppServices`, `MainWindow`, `MainViewModel`) as a thin host that picks the game from the savefile.
  Only then is it clear which hero, castle and creature types the two games share; expect to fork more than to share.
- `NavNodeViewModel` and `NavigationTree` still represent HoMM3 heroes, players and castles.
  Make a generic node when the second game has a tree of its own.
- **Cross-platform UI.** `Siniskalc.Core` is platform-agnostic, but the app and reusable UI use WPF on Windows. Supporting other platforms would require replacing or porting the UI, for example to Avalonia or MAUI.
- **State store for hero edits.** Hero edits already use reducers on immutable models, and `SaveDocumentHistory` already owns document history and raises change events. View models still pass per-view-model `Update` callbacks and refresh after history changes; a shared editing store could own reducer application and coordinate those updates.
- **Data-driven game definitions.** Creatures, artifacts, spells, skills, buildings and other game definitions are code-defined, not loaded from editable mod data. A data-driven definition source could enable mods and simplify support for future games.
- **Logging** The project has `TODO:LOG` markers but no logging implementation or log viewer. Add structured logging (for example Serilog) and a way to include useful logs in problem reports.

## User features

1. **Coordinated castle-hero changes.** Castle models already read and write garrisoned and visiting hero IDs, and the UI can show and open those heroes. The editor cannot yet swap them or move a hero in or out while keeping the castle, hero records and player's hero list consistent; these changes span multiple edit sessions.
2. **Castle creature growth over time.** The editor reads and lets users change creatures currently waiting in built dwellings; building a dwelling also initializes its stock from the creature's base growth. It does not calculate the weekly growth added by the game.
3. **Editing the date and current player.** The overview displays these when the save contains them, but they are currently read-only; there is no edit model or serializer for this save metadata.
4. **Resource generators (mines etc.).** Map-object positions are used to locate obelisks, but general map objects and their owners are not modeled for editing. Investigate the tile/object data before adding support.
5. **Teams.** Savefile research identified a team table in Shadow of Death and Restoration of Erathia (not Horn of the Abyss), but the parser does not read it.
   Reading it would let the obelisk card mark a whole team at once.
6. **Dug tiles and visited-object data.** Savefile research identified the dug-tile list and a possible visited-object table, but neither is modeled by the editor and the table's meaning is unclear. The meaning of the Grail-related marker near the date (`7F xx 02 yy`) also needs confirmation.
7. **Hero availability after fleeing or defeat.** The savefile data that prevents a defeated hero from returning is still unknown. A nearby table changes when a hero flees or is defeated, but its relationship to that behavior has not been confirmed; see [the knowledge base](KNOWLEDGE_BASE.md#heroes).