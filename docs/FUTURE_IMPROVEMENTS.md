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
2. **Castle creatures growth**.
   - **HoMM3**: we think we have found the growth table, but it isn't read.
   - **HoMM2** (done 2026-10-22, `CastleModel.WeeklyGrowth`, shown as "+N a week" on each dwelling): the growth of the creature, +2 from the Well in every built dwelling, +8 from the second well of the faction in the first dwelling only, nothing in an unbuilt dwelling (`docs/HOMM2_SAVE_FORMAT.md`, "The Well and the second well").
     Left: the **Warlock** (Waterfall) was in none of the saves, so only five factions are confirmed; whether a **dwelling built or upgraded today gets its first creatures at once or at the next week** (the editor leaves the count alone; Threedom saves 15 and 16 suggest next week, not proven); and **none of it is tried in the game**.
   - **HoMM2: "Can build this turn"** (done 2026-10-22, `BuiltTodayModel`; a bit for each of the 72 castle slots, 0x48 bytes after the castle table; saves of `TestData/Homm2/BuiltThisTurn`).
     Left: the page writes the bit but **it is not tried in the game**; **changing the date must clear the whole set**, as the game does on a new day (belongs to item 3); and the low bits `0x3` of Xabran's dwelling mask (`+0x1A`) in `built_this_turn_13_corackston_well__xabran_well` are **unexplained** (nothing else in the saves sets them; ask what was done in that step).
3. **Editing of the date, and whose turn it is**
   - HoMM2: the turn is editable (card "Order of play"; the turn byte is DateStart-14), **not tried in the game**. Editing the human/CPU seats is deliberately not built.
   - **HoMM3: the Order of play buttons do nothing yet.** Probe saves needed: change the current player in the setup block and load it in the game.
4. **Resource generators (mines etc.)** (mines, dwellings and the stock of the merchants are on the Map page of HoMM3; see below for what is open): as we're starting to understand tile structure, we could extract all the buildings from the save and make the owner editeable.
   - **HoMM3, found in the saves of `Island of HOTA tests 6p` (2026-10-22):** a table of 62-byte records, one for each mine object of the map, in the order of the map-object list; a record is x, y, z, owner (FF none, 0 Red, 1 Blue), kind (0 sawmill, 1 alchemist's lab, 2 ore, 3 sulfur, 4 crystal, 5 gems, 6 gold), one byte, then **the guard: seven creature slots (u32 each, FF FF FF FF for none) and seven counts (u32 each)**. An abandoned mine has its record in the same table (its owner FF and its guard filled until it is taken, then its owner, the resource it turned into and an empty guard).
   - **The garrison of a mine is to be modelled too** (the seven slots, as the army of a hero or a castle is, with the shared army editor), together with the owner. The record's own x, y are about 5 tiles left of the object's position and are not where the mine is entered: where a hero stands on the entrance is the object's position minus (1, 3) in the seven mines checked, so the location comes from the map-object list (record k belongs to the k-th mine object).
   - **HoMM2 (to be figured out later): a mine can hold a few creatures that a hero put there with a spell** (Set Guardian, the elementals of Set Earth/Air/Fire/Water Guardian, spell numbers 62 to 65), not a full garrison. Where the savegame keeps the kind and the number of the guardians of a mine is not known (no save has one yet: the mines record of 7 bytes has no room for it, so it is probably in the tile or in a separate list). Probe saves: a hero casts each of the four spells on a mine, one save each, and one with the guardians killed by an enemy.
5. **Teams**: the team table of the header is found (Shadow of Death and Restoration of Erathia; not in Horn of the Abyss) but not read.
   Reading it would let the obelisk card mark a whole team at once.
6. **Dug tiles and the visited-object table**: readable, but the meaning of the number and of the table is unclear.
   The Grail flag beside the date (`7F xx 02 yy`) is the same.
7. **A defeated hero and the tavern**: what keeps a defeated hero from coming back is not yet known.
   The tavern has table about 250 bytes before the date that changes when a hero is defeated, so it's probably that one.
9. **Places a HoMM3 hero visited** (built 2026-10-22: Places tab of the hero page on the shared `VisitedPlacesCard`; a 32 bit number per kind in the hero record, bit = number of the place among its kind, positions from the map cells; see `docs/KNOWLEDGE_BASE.md`). Open:
   - Per-place lists of **trees of knowledge, colossi and gazebos** (the per-place cells are not found) and the **dragon utopia "visited for Red only"** mark.
   - Offsets verified only in **Horn of the Abyss**; Shadow of Death and Restoration of Erathia are unchecked.
   - Forgetting a visited place is **not tried in the game**.
10. **HoMM3 Map page, what is left** (Mines, Dwellings and Merchants tabs; built 2026-10-22):
   - **Nothing is tried in the game**: owner and stock edits of mines and dwellings, guard edits, merchant stock edits, the destination fix.
   - **Position versus entrance**: the record tile of a mine or dwelling is not always the entrance; the hero's destination marker equals the entrance and may be the better source. Test it against the mine and dwelling saves.
   - **Dwellings**: the guard and state of a neutral dwelling, the stock of the refugee camp (`TestData/Hota/VisitedPlaces` save 30 is filed, undecoded; a before/after-hire pair is wanted), creature codes of other factions, non-Castle dwellings. The table finder works only when the table directly follows the mines, so maps like the Arts map show none.
   - **Merchants**: only one list pair was found on the test map; maps with several merchants of a kind are unknown. Merchants never sell spell scrolls (the picker excludes them); the picker could be limited to treasure/minor/major artifacts if that rule is confirmed.
   - Unfiled saves from the user's folder: 237, 242, 243 and 311 to 313.
8. **Banishing, what is left** (the counter itself is found: byte 22 of the player struct, shown on the player page and editable; see `docs/KNOWLEDGE_BASE.md`, "Players").
   - Check in the game that a lower or higher number of days makes the game banish the player earlier or later.
   - The other bytes banishing changes are not decoded: the hero chunk of the player (owner `FF`, army emptied), a counter and an 8-byte record near the end of the save, and header bytes near 1313, 1356 and 12492.
     Until they are, a banished player (0) is read-only and no one can be banished or revived by the editor.
   - The list of the indices of a player's towns that starts at byte 25 is not read.
