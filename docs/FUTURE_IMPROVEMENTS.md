# Future Improvements

What is open: **bugs and known defects** first, then **improvements**.
What is already built is in `CHANGELOG.md` and `docs/SINISKALC_ARCHITECTURE.md`; what the savefiles showed is in `docs/KNOWLEDGE_BASE.md` (HoMM3 and HoMM2); what is shared between the games and what is kept apart is in `.claude/agents/game-reconciler.md`.

# Bugs and known defects

## Wrong, lossy or missing behaviour

- **A defeated hero and the tavern.** A defeated hero cannot be rehired by its owner; it will not appear in its tavern rotation until it has been hired by another player and defeated again. The tavern has a table about 250 bytes before the date that changes when a hero is defeated, and might be what models the behaviour. Probe: several week changes with defeated heroes away from their players.
- **Banishing is partial.** The counter (player byte 22) is read and edited. The other bytes banishing changes are not decoded (the player's hero chunk gets owner `FF` and an emptied army, a counter and an 8-byte record near the end of the save, header bytes near 1313, 1356 and 12492), so a banished player (0) is read-only and no one can be banished or revived by the editor.

## Limits of what is shown

- **HoMM3 weekly growth** shows the base growth of the creature only; the castle, horde-building and Grail bonuses are not documented, and the growth table is not read.
- **HoMM3 daily income** (`DailyIncome`) counts only the mines and the best town hall of each town, so the amounts are "at least" values (the save does not store the income). See "Daily income" below.
- **HoMM3 faerie ring, fountain and idol of fortune** are shown but not editable because the saves do not say what they give.
- **HoMM2 guild restrictions:** any spell can be put in any Mage Guild slot.
- **HoMM3 visited-place offsets** are verified only in Horn of the Abyss; Shadow of Death and Restoration of Erathia are unchecked.
- **Dragon utopia "visited for Red only"** and the per-place lists of **colossi and gazebos** are not found.
- **HoMM3 merchants:** that the lists follow the merchants in the order of their numbers is confirmed only for the first two. Merchants never sell spell scrolls (the picker excludes them); it could be limited to treasure, minor and major artifacts if that rule is confirmed.
- **Position versus entrance:** the record tile of a mine or dwelling is not always the entrance; the hero's destination marker equals the entrance and may be the better source.

## Written but never tried in the game

Nothing here is known to be wrong. It is edited and tested against saves, but no edited savegame was opened in the game yet. Try each and delete the line when it works.

- HoMM3: the Order of play buttons that give the turn (probe: edit `TestData/Sod/Hotseat/hotseat_11_sir_mullich_magic_well.GM2` to Blue's turn, load it, check the game opens on Blue's turn with Blue's heroes and towns to move, and that ending Blue's turn goes on to the next day with Red; if it does not start cleanly, switch the buttons off with `protected override bool CanGiveTurn => false;` in the HoMM3 `OverviewViewModel`). Also `hotseat_12_blue_turn` changes the players' selected hero (`PlayerLayout.SelectedHero`), which the editor leaves alone.
- HoMM3: castle owner edits now update both players' town counts and acquisition-ordered lists, verified against the twelve `TestData/Hota/TownOwnership` saves. Load an edited save in the game to check both town lists; town selection is deliberately left alone (the user reports selection clears on reload).
- HoMM3: load owner/stock edits of the newly found dwellings on the no-mine and one/two-dwelling controls in `TestData/Hota/MineFinding`; on the Arts map, check the last dwelling and obelisk visits together.
- HoMM3: morale, luck and visited-building bits; owner and stock edits of mines and dwellings, guard edits, merchant stock edits, the destination fix; forgetting a visited place; the banishing counter (does a lower or higher number of days make the game banish earlier or later); editing the tavern.
- HoMM2: building rules (a turret or moat on a town without a castle, a shipyard inland, one building a day); the turn; "Can build this turn"; the growth rules (and whether a dwelling built or upgraded today gets its first creatures at once or next week; Threedom saves 15 and 16 suggest next week); T01BLDS (buildings), T02FORGT (a hero visits a fort or gazebo again after forgetting it), T03BONUS (morale and luck of the next battle), T04OBELS, T05FORT, T06SRC; that a changed owner behaves as expected now that `PlayerListsReconciler` keeps the players' lists in step; the Warlock (Waterfall) growth, since it was in none of the saves and only five factions are confirmed.
- Both: the moved Dwellings tab keeps its "Not yet tried in the game" note.

# Improvements

## Features

1. **Swapping the garrisoned and the visiting hero** of a castle (and moving a hero in or out): changes the castle, two hero chunks and the player's list of heroes together, which no edit session does today. Giving a hero from a tavern to a player is refused with a message (the game refills the slot by a random draw), and so is anything that needs the owner of a garrisoned hero; see `docs/KNOWLEDGE_BASE.md`, "The table of hero owners".
2. **HoMM2 mine guardian editing.** The elemental and saved count are displayed read-only. Editing remains after establishing valid count bounds (including counts above 255) and deciding how to handle unverified partial losses; see `KNOWLEDGE_BASE.md`.
3. **Teams.** The team table of the header is found (Shadow of Death and Restoration of Erathia, not Horn of the Abyss) but not read. Reading it would let the obelisk card mark a whole team at once.
4. **Dug tiles and the visited-object table:** readable, but the meaning of the number and of the table is unclear; the Grail flag beside the date (`7F xx 02 yy`) likewise.
5. **Places a HoMM3 hero visited**, what is left: the **mineral spring** (a Horn of the Abyss object, type 144 subtype 4: +1 luck for the next battle and +600 movement points until the end of the day; where a hero keeps that it visited one is not known; probe: Tazar visits a mineral spring, then Orrin a turn later, and a save a day later).
6. **HoMM3 Dwellings:** editing the guard and state of a neutral dwelling, generator types 18..20 (validated for locating the table but not exposed as single-creature dwellings), and the stock of the refugee camp (`TestData/Hota/VisitedPlaces` save 30 is filed, undecoded; a before/after-hire pair is wanted). Unfiled saves in the user's folder: 237, 242, 243 and 311 to 313.
7. **Daily income (HoMM3)**: to include what `DailyIncome` leaves out, in this order:
   - Equipped artifacts (easy): Endless Purse/Bag/Sack of Gold (+500/+750/+1000 gold) and Cornucopia (+5 of each precious resource) are in the `Artifact` table; `DailyIncome.Of` needs the player's heroes (through a hero collection class, not chunks) and must count equipped slots only, not the backpack.
   - Estates skill (easy): gold per day by skill level, on the heroes of the player; check the values in a save first.
   - Resource silo (easy once a faction-to-resource table exists): `Building.ResourceSilo` is known; the resource depends on the faction.
   - Grail building (unclear): its daily effect was never checked in a save (see the note in `CastleModel`).
   - Verify each value with a pair of saves (before/after equipping or building), add tests that name the save, then change the wording from "at least" to "estimated". HoMM2 stores its income in the player record.
8. **Editing the date.** Not built. In HoMM2 it must clear the whole set of "built today" bits (72 castle bits, cleared by the game on every new day); in HoMM3 whose turn it is has to follow it. Editing the human/computer seats of HoMM2 is deliberately not built (see the HoMM2 header notes in `KNOWLEDGE_BASE.md`).
9. **Localized entity names.** Names and data-authored descriptions live in each game's standard JSON catalog, loaded through an injected reader. Selecting a language and resolving localized catalog text remain future work; generated wording belongs to injected text providers.
10. **Entity editing UI and custom profiles** are deferred. Standard definitions load from JSON at startup, with JSON-only extensions within known mechanics and format capacities. There is no profile selector, user override directory, hot reload or in-app editor (`docs/GAME_DEFINITIONS.md`).
11. **Log viewer** in the UI, for users to report issues. Logging itself is in place (`docs/LOGGING.md`).
12. **A third game** (HoMM1 first, as the test that the page interfaces are right): see "Adding a game" and "How HoMM2 was added" in `docs/SINISKALC_ARCHITECTURE.md`.
13. **A second savefile format of a game we have** (fheroes2 for HoMM2, VCMI for HoMM3): the entities, models and pages are the same, the format is not. Split `Siniskalc.Homm2` / `Siniskalc.Homm3` into the rules and the format (locator, chunks, (de)serializers) when the first of them is started.
14. **Cross platform support.** The core logic is platform-agnostic; the WPF UI could be replaced with Avalonia or MAUI. The view models in `Siniskalc.Wpf` and `Siniskalc.Heroes.Wpf` use little of WPF (`ICommand`, `INotifyPropertyChanged`); moving them to `net8.0` projects would leave only the views to rewrite.

## Probes: saves to make, to decode what is still a guess

Each probe is one scenario saved before and after one change, named by `TestData/README.md`, so two files can be compared byte by byte.
One step between two saves and nothing else (no moves, no battles, no spending). Send the file names with the names of the heroes and the coordinates of the places.
When a probe settles an offset it goes into `HeroOffsets` (or the HoMM2 layout) with a test that names the save, and a row in `KNOWLEDGE_BASE.md`.
What HoMM2 taught: a hero keeps bits for the places it visited and the morale and luck bytes beside them, and writing the numbers without the bit shows on the troops but not on the hero screen, so bit and byte are always set together.

### HoMM3

- **Mine/dwelling formats outside HotA.** Repeat zero/one/two-mine and one/two-dwelling capture controls in SoD, including a no-mine map. Establish count widths in actual RoE, AB and Chronicles saves rather than assuming SoD's; the current `Roe` fixtures identify as SoD.
- **Cheat-used flag candidate.** Test whether the date-marker byte identified in `KNOWLEDGE_BASE.md` persists, changes for other cheat types and affects scoring.
- **Temporary bonuses until the next battle** (hero chunk `+28`, `+29`, `+135..138`; the watering hole, swan pond, stables, crypt, faerie ring, fountain of fortune and idol of fortune are known). Missing bits: Temple, Oasis, Rally Flag, Mermaids, Fountain of Youth, Buoy, Shrine of Magic, Mystic Garden, Warrior's Tomb, Shipwreck survivor, Sirens. For each: visit, save; fight a battle, save; end the day, save. Also Stables a second week, and two places that may share a bit (Oasis + Temple, for `+135 0x10`).
- **Permanent-bonus places a hero visited:** the Horn of the Abyss kinds are decoded (`KNOWLEDGE_BASE.md`); the open question is the same for Shadow of Death. Series `TestData/Sod/VisitedPlaces/visited_places_<NN>_<what changed>.GM1`, one map, one human player, two heroes and two copies of the same place: baseline (both heroes in one town); hero A visits the first Arena; A visits it again (refused); B visits it; A visits the second Arena; a new week; then one save per kind (Marletto Tower, Star Axis by A and then B, Learning Stone, Mercenary Camp, Witch Hut with the skill it gave, Library with the skill levels). Add a README row for each.
- **The map:** capture each kind of mine, and a mine taken from another player (owner byte and income at the next day); weekly objects (Windmill, Water Wheel, Mystical Garden, Wagon, Campfire, Lean To: first visitor, second visitor the same week, new week; the Magic Spring is known: player `+129` and a per-square flag); once-per-game objects (Sphinx, Pandora's Box, Seer's Hut, Genie's Lamp, Keymaster's tent, Border guard); creature banks (does a bank keep its state); the owner of a dwelling.
- **Unnamed hero-chunk bytes** (see "Unnamed bytes of the hero chunk" in `KNOWLEDGE_BASE.md`): two heroes in two different towns, one in the first town and one in the third, then both outside (`+51/+52` as a town index); several battles with one hero, a save after each (`+53`, `+55`, `+44`, `+225` counters); a hero that fights, flees, is defeated and is hired again (`+1088` bits and the tavern memory); a boat boarded, sailed, landed, built, and HotA boats (`+30` and the HotA tail `+1102..1144`); one copy with only the level byte edited and one with only the experience (does the game ignore the level byte, as in HoMM2?); a new day, week and month with nobody moved (the 4282-byte block and `+77..79`); digging for the Grail at a wrong and at the right spot (the dug-tile number).
- Suggested order: the town index and battle counters (cheap), the first four temporary bonuses, the Arena and the two-copies question, then the mines.

### HoMM2

- **Level and experience kept in step:** the game showed level 14 for level 20 and 50,000 experience, so the experience table is not verified (saves E30 to E32 were made to find the rule). The maximum movement is worked out by the game and not stored.
- **The guardians of a mine:** spell codes, counts and clearing after an enemy capture are established (see `KNOWLEDGE_BASE.md`). Remaining probes: partial guardian losses if possible; count encoding above 255 before allowing such edits.
- Still unresolved: the town's map flag remains stale after an editor ownership change; the daemon cave, windmill and water mill state is not decoded; the meaning of hero bytes `+0xE3..+0xE6` and `+0x51`; and the otherwise-unassigned `0x4000` bit in the castle-building mask. The building rules that have not been tried in-game are listed above.

## Architecture and code

- **Game modules** (done): the HoMM3 code is in `Siniskalc.Homm3`, the exe is a thin host with a shell, and the navigation tree, node and editor view are shared (`Siniskalc.Wpf`), each game with a `NavigationBuilder`.
- **Shared page view models.** The pages of the two games bind to the same interfaces (`IPlayerPage`, `ICastlePage`, `IHeroPage`), but each game's view model is its own. The tabs of the two heroes still hold content only their game has (HoMM3: backpack, war machines, hero class).
- **State store for hero edits.** The hero edits are reducers on immutable models. A single store holding the document and applying reducers would replace the per-view-model `Update` callbacks and the `Refresh()` calls that follow `History.Changed`.
- **Async file I/O.** Only the two "export bytes" paths are async (`SavegameFileCommands.ExportBytesAsync`, `HexEditor.ExportBytesAsync`); everything else is sync, and the stack below the UI is sync.
  - Keep sync: `UserSettings` and `SettingsStore` (a few hundred bytes, read once during composition; async would force async factories or a blocking `GetAwaiter().GetResult()` at startup).
  - Worth doing only if measured to matter: opening, reloading and saving a savefile from the UI (`SavegameFileCommands.Open`, `OpenRecent`, `Save`, `SaveAs`) read, decompress, parse, back up and write on the UI thread, so a big save or a slow or network drive freezes the window. Time a load and a save on the biggest `TestData` saves first.
  - How: do not make `ISerializer`, `IDeserializer`, the codecs or the loaders async (it would spread through every interface and test, and parsing is CPU work). Run the whole open or save step with `Task.Run` from the commands, show a busy state, apply the result on the UI thread, and disable the commands while one runs.
  - Export being async while open and save are not is a difference in the commands, not a decision to make I/O async.
- **Turn model** (`TurnModel<TColor>`, `TurnDiffer<TColor>`, one `CurrentPlayer`): HoMM2's model holds the record number and the colour, which a view model works out (`Game.RecordOf`). Moving the record into the deserializer and serializer would reverse an earlier decision, change what is written and read an unknown record as Neutral. Do it with a save that has an odd turn record, to check what `chunk.Tag` carries.
- **Visited places models** (`VisitedPlaces` against `PlaceVisits`, and the two `MapPlaces`): the row builder and the change wording are shared; the models differ in shape and sharing them would remove little.
- **Secondary skills models are kept apart on purpose.** Both games implement `ISecondarySkills<TSkill, TSkills>` and share `SecondarySkillsViewModelBase`. HoMM3 (`Siniskalc.Homm3/Models/SecondarySkills.cs`) holds a level for every skill plus the skill slots in order, and keeps an "extra" (a level and no slot, which the game still applies). HoMM2 (`Siniskalc.Homm2/Models/SecondarySkills.cs`, `Bytes.SecondarySkills`) supports eight skills only and does not support learned skills without slots, as confirmed by the user; those are not missing editor features. A shared model with extras is not needed for HoMM2.
- **Mage Guild views, pending-changes factory, player differ, player edit session and player page base:** kept apart on purpose, see "Kept apart on purpose" in `.claude/agents/game-reconciler.md`.
