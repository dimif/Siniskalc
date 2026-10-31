# Future Improvements

What is planned or open.
What is already built is in `CHANGELOG.md` and `docs/SINISKALC_ARCHITECTURE.md`; what the savefiles showed is in `docs/KNOWLEDGE_BASE.md`.

## State of the HoMM2 / HoMM3 comparison (2026-10-24)

The two editors now have the same pages and blocks (overview, players, heroes, castles, map, neutral); `docs/SHARING_REVIEW.md` and `docs/SHARING_REVIEW_UI.md` say what is shared and how.
A feature audit of both games found these gaps; the cheap ones are closed (see `CHANGELOG.md`). What is left, and why:

- **Needs a probe save or a test in the game before anything is written:**
  - HoMM2 building rules (a turret or moat on a town without a castle, a shipyard inland, one building a day) and the guild's spell restrictions (any spell can be put in any slot today).
  - HoMM2 map objects with state (daemon cave, windmills, water mills; known in `docs/HOMM2_SAVE_FORMAT.md`), map dwellings (the docs say none; the bytes are undecoded) and the guardians of mines.
  - HoMM3 morale, luck and the visited-building bits (built 2026-10-24, **not tried in the game**); the faerie ring, fountain and idol of fortune are shown but not editable because the saves do not say what they give.
  - Giving a hero from a tavern to a player (the game refills the slot by a random draw), and how a garrisoned hero's owner is stored; both are refused with a message (`docs/KNOWLEDGE_BASE.md`, "The table of hero owners").
  - Banishing or reviving a player; editing the date (which in HoMM2 must clear "built today").
- **HoMM3 town count and list:** after a castle changes owner the player's town count (byte 23) and the list of towns from byte 25 are not written, although the count follows ownership in every save seen; the order of the list is not established.
- **A defeated hero and the tavern:** a defeated hero goes back to the free pool and another player can hire it (found in `TestData/Hota/HeroOwners`); nothing seen keeps it out of the same player's tavern, and 12 draws per week are too few to tell. Hero 3 holds `0x41` in the table of hero owners in every save, unexplained. Probe: several week changes with defeated heroes away from their players.
- **HoMM2 tree:** a player's countdown is in the node subtitle and is not refreshed after an edit (HoMM3 shows it as a note and refreshes it); the Map node exists only when the map has a shrine or a sphinx, so the Bytes tab of the map records cannot be reached on a map with neither.
- **Weekly growth in HoMM3** shows the base growth only; the castle, horde-building and Grail bonuses are not documented.
- **Housekeeping:** both games now use `SaveMetadata<TColor>` / `HeroesSavegameBase<TColor>` (completed 2026-10-23, integrated with current turn editing 2026-10-31). Historical issues: commit `ba3468b` has a `Co-Authored-By` trailer and a few commits (`d3ae097`, `15994da`, `54e4acf`, `2811099`, `9520538` to `0552fb1` for two test lines) do not build or pass on their own; `GzipSavefileCodecTests.WritingASmallerSavefileLeavesNothingOfTheLargerOne` failed twice in full runs and passed alone.
- **Open design decision:** whether the lists of heroes and castles in a player's record stay in the HoMM2 model, or a document-level step keeps them in step (options A, B, C in `docs/SHARING_REVIEW.md`, M9a); test in the game first.

## Project structure

- **Game modules** (done): the HoMM3 code is in `Siniskalc.Homm3` and the exe is a thin host with a shell (`IGameModule`, `IGameEditor`, `ShellViewModel`, `ShellWindow`); see `docs/HOMM2_PLAN.md`.
  What the two games share is decided (see `Siniskalc.Heroes` and "Adding a game" in `docs/SINISKALC_ARCHITECTURE.md`): the values that mean the same (`Resource`, `GameDate`, `TavernHeroes`, `ChangeAreas`), interfaces for the entities each game has its own set of (colours, factions, skills, skill levels), the blocks the pages are made of, and the shell, the navigation tree and the editor base.
  Creature, spell and artifact tables remain game-specific; `MapPosition`, `HeroPosition` and `MapHeader` are shared values.
- The navigation tree, its node and the editor view are shared (`Siniskalc.Wpf`); each game has a `NavigationBuilder`.
- **A second savefile format of a game we have** (fheroes2 for HoMM2, VCMI for HoMM3): the entities, models and pages are the same, the format is not.
  Split `Siniskalc.Homm2` / `Siniskalc.Homm3` into the rules (entities, models, reducers, pages) and the format (locator, chunks, (de)serializers) when the first of them is started; the editor view models and pages already work with models and the collection classes, never with chunks.
- **A third game** (HoMM1 first, as the test that the page interfaces are right): see "Adding a game" in the architecture document.
- **Names behind a provider.** Names of creatures, spells and artifacts are string literals in the entity tables.
  Before a third game, one injected name provider (like `EffectLineProvider`) so a creature is named once and the UI can be localized.
- **Shared page view models.** The pages of the two games look and work the same and bind to the same interfaces (`IPlayerPage`, `ICastlePage`, `IHeroPage`), but each game's view model is its own.
  The tabs of the two heroes still hold content only their game has (HoMM3: backpack, war machines, hero class); the secondary skills model is not shared, see "Secondary skills (M3)" below.
- **View models free of WPF.** The view models in `Siniskalc.Wpf` and `Siniskalc.Heroes.Wpf` use little of WPF (`ICommand`, `INotifyPropertyChanged`); moving them to `net8.0` projects would leave only the views to rewrite for Avalonia.
- **Cross platform support.** The core logic is already platform-agnostic; the WPF UI could be replaced with Avalonia or MAUI.
- **State store for hero edits.** The hero edits are reducers on immutable models.
  A single store holding the document and applying reducers would replace the per-view-model `Update` callbacks and the `Refresh()` calls that follow `History.Changed`.
- **Editable store for game entities** Entities (heroes, castles, creatures) are hardcoded in the editor.
  A store of entities that can be edited and saved would let the editor support mods and future games.
- **Log viewer** A log viewer in the UI for users to report issues. Logging itself is in place (`docs/LOGGING.md`).

## User features

1. **Swapping the garrisoned and the visiting hero** of a castle (and moving a hero in or out): changes the castle, two hero chunks and the player's list of heroes together, which no edit session does today.
2. **Castle creatures growth**.
   - **HoMM3**: the base growth of the dwelling's creature is shown (2026-10-24); the growth table is still not read, so the bonuses are missing.
   - **HoMM2** (done 2026-10-22, `CastleModel.WeeklyGrowth`, shown as "+N a week" on each dwelling): the growth of the creature, +2 from the Well in every built dwelling, +8 from the second well of the faction in the first dwelling only, nothing in an unbuilt dwelling (`docs/HOMM2_SAVE_FORMAT.md`, "The Well and the second well").
     Left: the **Warlock** (Waterfall) was in none of the saves, so only five factions are confirmed; whether a **dwelling built or upgraded today gets its first creatures at once or at the next week** (the editor leaves the count alone; Threedom saves 15 and 16 suggest next week, not proven); and **none of it is tried in the game**.
   - **HoMM2: "Can build this turn"** (done 2026-10-22, `BuiltTodayModel`; a bit for each of the 72 castle slots, 0x48 bytes after the castle table; saves of `TestData/Homm2/BuiltThisTurn`).
     Left: the page writes the bit but **it is not tried in the game**; **changing the date must clear the whole set**, as the game does on a new day (belongs to item 3); and the low bits `0x3` of Xabran's dwelling mask (`+0x1A`) in `built_this_turn_13_corackston_well__xabran_well` are **unexplained** (nothing else in the saves sets them; ask what was done in that step).
3. **Editing of the date, and whose turn it is**
   - HoMM2: the turn is editable (card "Order of play"; the turn byte is DateStart-14), **not tried in the game**. Editing the human/CPU seats is deliberately not built.
   - **HoMM3: the Order of play buttons give the turn (enabled 2026-10-31) but this NEEDS TESTING IN THE GAME: no edited savegame has been opened in the game yet.** Done: the editor reads and writes whose turn it is ( `TurnModel`, the turn chunk is the eight human flags and the 32-bit current player after them, `TurnBytesLoader` / `TurnDeserializer` / `SafeTurnSerializer` / `TurnDiffer`; only a human can be given the turn, as `PlayerControlLocator` needs the current player to be human to find the setup again). In 21 pairs of consecutive saves (see `docs/KNOWLEDGE_BASE.md`) only this value of the setup changed with the turn. If the game does not start the turn cleanly, switch the buttons off again with `protected override bool CanGiveTurn => false;` in the HoMM3 `OverviewViewModel` (the shared `OverviewViewModelBase` supports it). **Open:** (1) **not tried in the game**: whether the game starts the new player's turn cleanly when it loads an edited save (probe: edit `TestData/Sod/Hotseat/hotseat_11_sir_mullich_magic_well.GM2` to Blue's turn, load it in the game, and see whether the game opens on Blue's turn with Blue's heroes and towns to move and nothing out of place, and whether ending Blue's turn goes on to the next day with Red as it should); (2) whether anything else changes with the turn: `hotseat_12_blue_turn` also changes the players' selected hero (`PlayerLayout.SelectedHero`: Red's becomes FF and Blue's 0), which the editor leaves alone.
4. **Resource generators (mines etc.)** (mines, dwellings and the stock of the merchants are on the Map page of HoMM3; see below for what is open): as we're starting to understand tile structure, we could extract all the buildings from the save and make the owner editeable.
   - **HoMM3, found in the saves of `Island of HOTA tests 6p` (2026-10-22):** a table of 62-byte records, one for each mine object of the map, in the order of the map-object list; a record is x, y, z, owner (FF none, 0 Red, 1 Blue), kind (0 sawmill, 1 alchemist's lab, 2 ore, 3 sulfur, 4 crystal, 5 gems, 6 gold), one byte, then **the guard: seven creature slots (u32 each, FF FF FF FF for none) and seven counts (u32 each)**. An abandoned mine has its record in the same table (its owner FF and its guard filled until it is taken, then its owner, the resource it turned into and an empty guard).
   - **The garrison of a mine is to be modelled too** (the seven slots, as the army of a hero or a castle is, with the shared army editor), together with the owner. The record's own x, y are about 5 tiles left of the object's position and are not where the mine is entered: where a hero stands on the entrance is the object's position minus (1, 3) in the seven mines checked, so the location comes from the map-object list (record k belongs to the k-th mine object).
   - **HoMM2 (to be figured out later): a mine can hold a few creatures that a hero put there with a spell** (Set Guardian, the elementals of Set Earth/Air/Fire/Water Guardian, spell numbers 62 to 65), not a full garrison. Where the savegame keeps the kind and the number of the guardians of a mine is not known (no save has one yet: the mines record of 7 bytes has no room for it, so it is probably in the tile or in a separate list). Probe saves: a hero casts each of the four spells on a mine, one save each, and one with the guardians killed by an enemy.
5. **Teams**: the team table of the header is found (Shadow of Death and Restoration of Erathia; not in Horn of the Abyss) but not read.
   Reading it would let the obelisk card mark a whole team at once.
6. **Dug tiles and the visited-object table**: readable, but the meaning of the number and of the table is unclear.
   The Grail flag beside the date (`7F xx 02 yy`) is the same.
7. **A defeated hero and the tavern**: what keeps a defeated hero from coming back is not yet known (see the state section above: defeated heroes are free again and the table of hero owners and the player's list are now kept in step).
   The tavern has table about 250 bytes before the date that changes when a hero is defeated, so it's probably that one.
9. **Places a HoMM3 hero visited** (built 2026-10-22: Places tab of the hero page on the shared `VisitedPlacesCard`; a 32 bit number per kind in the hero record, bit = number of the place among its kind, positions from the map cells; see `docs/KNOWLEDGE_BASE.md`). Open:
   - Per-place lists of the **colossi and gazebos** (they are objects of the type 144 whose cells carry no number; the subtype in the definition tells a gazebo, 6, from other objects, but no colossus was identified) and the **dragon utopia "visited for Red only"** mark. The trees of knowledge have theirs (2026-10-24).
   - **Mineral spring** (a Horn of the Abyss object, type 144 subtype 4: +1 luck for the next battle and +600 movement points until the end of the day): where a hero keeps that it visited one is not known. Probe saves wanted: Tazar visits a mineral spring, then Orrin a turn later, and a save a day later.
   - Offsets verified only in **Horn of the Abyss**; Shadow of Death and Restoration of Erathia are unchecked.
   - Forgetting a visited place is **not tried in the game**.
10. **HoMM3 Map page, what is left** (Mines, Dwellings and Merchants tabs; built 2026-10-22):
   - **Nothing is tried in the game**: owner and stock edits of mines and dwellings, guard edits, merchant stock edits, the destination fix.
   - **Position versus entrance**: the record tile of a mine or dwelling is not always the entrance; the hero's destination marker equals the entrance and may be the better source. Test it against the mine and dwelling saves.
   - **Dwellings**: the guard and state of a neutral dwelling, the stock of the refugee camp (`TestData/Hota/VisitedPlaces` save 30 is filed, undecoded; a before/after-hire pair is wanted), creature codes of other factions, non-Castle dwellings. The table finder works only when the table directly follows the mines, so maps like the Arts map show none.
   - **Merchants**: the number in front of the lists is the number of merchants and each has a list (`multi_merchants`, four black markets; 2026-10-24). That the lists follow the merchants in the order of their numbers is confirmed only for the first two. Merchants never sell spell scrolls (the picker excludes them); the picker could be limited to treasure/minor/major artifacts if that rule is confirmed.
   - Unfiled saves from the user's folder: 237, 242, 243 and 311 to 313.
8. **Banishing, what is left** (the counter itself is found: byte 22 of the player struct, shown on the player page and editable; see `docs/KNOWLEDGE_BASE.md`, "Players").
   - Check in the game that a lower or higher number of days makes the game banish the player earlier or later.
   - The other bytes banishing changes are not decoded: the hero chunk of the player (owner `FF`, army emptied), a counter and an 8-byte record near the end of the save, and header bytes near 1313, 1356 and 12492.
     Until they are, a banished player (0) is read-only and no one can be banished or revived by the editor.
   - The list of the indices of a player's towns that starts at byte 25 is not read.

## Other sharing left apart (2026-10-31)

- **The turn model** (`TurnModel<TColor>`, `TurnDiffer<TColor>`, one `CurrentPlayer`): HoMM2's model holds the record number and the colour, which a view model works out (`Game.RecordOf`). Moving the record into the deserializer and serializer would reverse the 2026-10-24 decision, change what is written and read an unknown record as Neutral. Do it with a save that has an odd turn record, to check what `chunk.Tag` carries.
- **Visited places models** (`VisitedPlaces` against `PlaceVisits`, and the two `MapPlaces`): the row builder and the change wording are shared; the models differ in shape (fixed fields against a number per kind) and sharing them would remove little. HoMM3 still guesses the HotA-only places from `HeroOffset > 1000` in three places (`HeroDeserializer`, `SafeHeroSerializer`, `VisitedPlacesViewModel`); a version tier on `VisitedPlaceKind` would replace it.
- **The Mage Guild views** and the **pending-changes factory, player differ, player edit session and player page base**: see `docs/SHARING_REVIEW.md` (2026-10-31, streamlining pass).

## Secondary skills (M3): not shared, and why (2026-10-31)

Both games already implement `ISecondarySkills<TSkill, TSkills>` and share `SecondarySkillsViewModelBase`; what is left is the model and the serializer, which differ in what they can hold.

- **HoMM3** (`Siniskalc.Homm3/Models/SecondarySkills.cs`): a level for every skill plus the skill slots in order. A skill with a level and no slot is an "extra", which the game still applies; the editor keeps it and can show or hide it.
- **HoMM2** (`Siniskalc.Homm2/Models/SecondarySkills.cs`, read by `Bytes.SecondarySkills`): the savefile has a table of levels and a table of the order the skills were learned in. The model lists the skills that have a level, those with an order first (in that order), then those with none by skill number, **at most eight** (`Constants.Hero.MaxSecondarySkills`). There are no extras.
- `SafeHeroSerializer.WriteSecondarySkills` leaves both tables alone while the model equals what they read as. When the model differs it **clears both tables and writes the model's entries again**, with the order 1 to n and the skill count.

What a shared model would change, and what to decide first:

1. **A ninth skill in a HoMM2 savefile.** The reader cuts the list at eight, so a hero whose level table holds more would lose the rest as soon as its skills are edited (the writer clears the tables). No save seen has more than eight; it is not known whether the game can produce one. A model with extras would keep them.
2. **A skill with a level and no order byte** is read as learned after the ordered ones and gets an order number when the skills are written. A shared model with extras would show it as an extra, and the writer would not give it an order number unless it was moved into a slot.
3. **The UI of HoMM2 would gain** the show and hide of extra skills.
4. Both serializers change, so each needs a round trip of every save of that game (unchanged model: identical bytes) and a test with a level byte but no order byte.

Do it when one of the points above matters (a save with more than eight skills, or a second game with extras); until then the two models stay apart.
