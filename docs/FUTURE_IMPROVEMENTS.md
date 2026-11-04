# Future Improvements

Open implementation work, in-game verification and focused savefile probes. Completed work and intentional design decisions do not belong here.
What is already built is in `CHANGELOG.md` and `docs/SINISKALC_ARCHITECTURE.md`; what the savefiles showed is in `docs/KNOWLEDGE_BASE.md` (HoMM3 and HoMM2); what is shared between the games and what is kept apart is in `.claude/agents/game-reconciler.md`.

# Improvements

## Game Features

1. **Editing the date.** Define the effects of moving time forward or backward before implementing it. At minimum, preserve consistency with HoMM2's "built today" set and HoMM3's current turn; establish how daily/weekly state should behave rather than assuming a date-byte edit is sufficient.
2. **Swapping the garrisoned and the visiting hero** of a castle (and moving a hero in or out): changes the castle, two hero chunks and the player's list of heroes together, which no edit session does today. Giving a hero from a tavern to a player is refused with a message (the game refills the slot by a random draw), and so is anything that needs the owner of a garrisoned hero; see `docs/KNOWLEDGE_BASE.md`, "The table of hero owners".
3. **Teams.** The team table of the header is found (Shadow of Death and Restoration of Erathia, not Horn of the Abyss) but not read. Reading it would let the obelisk card mark a whole team at once.
4. **HoMM2 mine guardian editing.** The elemental and saved count are displayed read-only. Editing remains after establishing valid count bounds (including counts above 255) and deciding how to handle unverified partial losses; see `KNOWLEDGE_BASE.md`.
5. **Dug tiles and the visited-object table:** compare saves before and after digging at incorrect and correct Grail locations to establish the tile-number encoding and table updates before exposing edits.
6. **Places a HoMM3 hero visited**, what is left: the **mineral spring** (a Horn of the Abyss object, type 144 subtype 4: +1 luck for the next battle and +600 movement points until the end of the day; where a hero keeps that it visited one is not known; probe: Tazar visits a mineral spring, then Orrin a turn later, and a save a day later).
7. **HoMM3 Dwellings:** establish and expose editable guard/state data for neutral dwellings and generator types 18..20. Decode refugee-camp stock using `TestData/Hota/VisitedPlaces` save 30 and a new before/after-hire pair. Existing format evidence is in `KNOWLEDGE_BASE.md`.
8. **Daily income (HoMM3)**: to include what `DailyIncome` leaves out, in this order:
   - Equipped artifacts: Endless Purse/Bag/Sack of Gold (+500/+750/+1000 gold) and Cornucopia (+5 of each precious resource) are in the `Artifact` table; `DailyIncome.Of` needs the player's heroes (through a hero collection class, not chunks) and must count equipped slots only, not the backpack.
   - Estates skill: gold per day by skill level, on the heroes of the player; check version-specific values in controlled saves first.
   - Resource silo: establish a faction-to-resource table and apply it when `Building.ResourceSilo` is built.
   - Grail building (unclear): its daily effect was never checked in a save (see the note in `CastleModel`).
   - Verify each value with a pair of saves (before/after equipping or building), add tests that name the save, then change the wording from "at least" to "estimated". HoMM2 stores its income in the player record.

## Editor features
1. **Log viewer** in the UI, for users to report issues. Logging itself is in place (`docs/LOGGING.md`).
2. **A second savefile format of a game we have** (fheroes2 for HoMM2, VCMI for HoMM3): the entities, models and pages are the same, the format is not. Split `Siniskalc.Homm2` / `Siniskalc.Homm3` into the rules and the format (locator, chunks, (de)serializers) when the first of them is started.
3. **Cross platform support.** The core logic is platform-agnostic; the WPF UI could be replaced with Avalonia or MAUI. The view models in `Siniskalc.Wpf` and `Siniskalc.Heroes.Wpf` use little of WPF (`ICommand`, `INotifyPropertyChanged`); moving them to `net8.0` projects would leave only the views to rewrite.

## Limits of what is shown

- **HoMM3 weekly growth:** establish castle, horde-building and Grail contributions with controlled weekly transitions, then include them rather than showing only base creature growth.
- **HoMM3 faerie ring, fountain and idol of fortune:** isolate each reward in before/after saves before enabling edits.
- **HoMM2 guild restrictions:** establish which spells are valid at each guild level and apply those rules to the picker without discarding existing unexpected spells.
- **Dragon utopia "visited for Red only", colossi and gazebos:** locate per-player/per-place visit data using separate visits by two heroes and players before exposing those edits.
- **HoMM3 merchants:** expose the shared town stock on towns with an Artifact Merchant built, using one document collection/history for every town's tab and `TownMerchantStockLocator` (date +38). Validate the alternate date prefix of the five `empire_of_the_world` / `new_day` saves before enabling those. Establish allowed artifact classes before tightening the picker. Replace heuristic Black Market discovery with a structural locator that also finds nearly sold-out tables and preserves unknown artifact codes.
- **Position versus entrance:** compare record coordinates with destination markers across mine/dwelling kinds before using them as exact "send to" destinations.
- **A defeated hero and the tavern.** A defeated hero cannot be rehired by its owner; it will not appear in its tavern rotation until it has been hired by another player and defeated again. The tavern has a table about 250 bytes before the date that changes when a hero is defeated, and might be what models the behaviour. Probe: several week changes with defeated heroes away from their players.
- **Banishing is partial.** The counter (player byte 22) is read and edited. The other bytes banishing changes are not decoded (the player's hero chunk gets owner `FF` and an emptied army, a counter and an 8-byte record near the end of the save, header bytes near 1313, 1356 and 12492), so a banished player (0) is read-only and no one can be banished or revived by the editor.

## In-game verification of implemented edits

- HoMM3: morale, luck and visited-building bits; owner and stock edits of mines and dwellings, guard edits, merchant stock edits, the destination fix; forgetting a visited place; the banishing counter (does a lower or higher number of days make the game banish earlier or later); editing the tavern.
- HoMM2: building rules (a turret or moat on a town without a castle, a shipyard inland, one building a day); the turn; "Can build this turn"; the growth rules (and whether a dwelling built or upgraded today gets its first creatures at once or next week; Threedom saves 15 and 16 suggest next week); T01BLDS (buildings), T02FORGT (a hero visits a fort or gazebo again after forgetting it), T03BONUS (morale and luck of the next battle), T04OBELS, T05FORT, T06SRC; that a changed owner behaves as expected now that `PlayerListsReconciler` keeps the players' lists in step; the Warlock (Waterfall) growth, since it was in none of the saves and only five factions are confirmed.
- Remove the relevant "Not yet tried in the game" notes only after the corresponding edited saves have been checked.

## Savefile probes

Use controlled before/after saves with one action per comparison where possible. Record hero names, object coordinates and game version; put established facts in `KNOWLEDGE_BASE.md`, not in this checklist.

### HoMM3

- **Mine/dwelling formats outside HotA.** Repeat zero/one/two-mine and one/two-dwelling capture controls in SoD, including a no-mine map. Establish count widths in actual RoE, AB and Chronicles saves rather than assuming SoD's; the current `Roe` fixtures identify as SoD.
- **Cheat-used flag candidate.** Test whether the date-marker byte identified in `KNOWLEDGE_BASE.md` persists, changes for other cheat types and affects scoring.
- **Temporary bonuses until the next battle** (hero chunk `+28`, `+29`, `+135..138`; the watering hole, swan pond, stables, crypt, faerie ring, fountain of fortune and idol of fortune are known). Missing bits: Temple, Oasis, Rally Flag, Mermaids, Fountain of Youth, Buoy, Shrine of Magic, Mystic Garden, Warrior's Tomb, Shipwreck survivor, Sirens. For each: visit, save; fight a battle, save; end the day, save. Also Stables a second week, and two places that may share a bit (Oasis + Temple, for `+135 0x10`).
- **Permanent-bonus places a hero visited:** the Horn of the Abyss kinds are decoded (`KNOWLEDGE_BASE.md`); the open question is the same for Shadow of Death. Series `TestData/Sod/VisitedPlaces/visited_places_<NN>_<what changed>.GM1`, one map, one human player, two heroes and two copies of the same place: baseline (both heroes in one town); hero A visits the first Arena; A visits it again (refused); B visits it; A visits the second Arena; a new week; then one save per kind (Marletto Tower, Star Axis by A and then B, Learning Stone, Mercenary Camp, Witch Hut with the skill it gave, Library with the skill levels). Add a README row for each.
- **Map-object state:** compare first and repeat visits, then a new week, to distinguish reset rules for Windmill, Water Wheel, Mystical Garden, Wagon, Campfire and Lean To. Separately compare before/after use of Sphinx, Pandora's Box, Seer's Hut, Genie's Lamp, Keymaster's tent and Border guard, and before/after clearing a creature bank.
- **Unnamed hero-chunk bytes** (see "Unnamed bytes of the hero chunk" in `KNOWLEDGE_BASE.md`): two heroes in two different towns, one in the first town and one in the third, then both outside (`+51/+52` as a town index); several battles with one hero, a save after each (`+53`, `+55`, `+44`, `+225` counters); a hero that fights, flees, is defeated and is hired again (`+1088` bits and the tavern memory); a boat boarded, sailed, landed, built, and HotA boats (`+30` and the HotA tail `+1102..1144`); one copy with only the level byte edited and one with only the experience (does the game ignore the level byte, as in HoMM2?); a new day, week and month with nobody moved (the 4282-byte block and `+77..79`); digging for the Grail at a wrong and at the right spot (the dug-tile number).

### HoMM2

- **Level and experience:** resolve the level-14 versus level-20 discrepancy at 50,000 experience using saves E30 to E32 and fresh in-game checks before changing the experience table.
- **The guardians of a mine:** spell codes, counts and clearing after an enemy capture are established (see `KNOWLEDGE_BASE.md`). Remaining probes: partial guardian losses if possible; count encoding above 255 before allowing such edits.
- **Town ownership flags:** locate the town's map flag and update it alongside an editor ownership change; currently it stays stale.
- **Daemon cave, windmill and water mill:** isolate first use, repeat use and reset transitions to locate their state before enabling edits.

## Architecture and code

- **Measure UI stalls during file operations.** Time opening, reloading and saving the largest fixtures and a slow-drive scenario. If blocking is significant, move the operation off the UI thread with a busy state and overlapping-command protection; apply UI changes on the UI thread. Keep codecs, parsers and serializers synchronous rather than spreading async through the data stack.
