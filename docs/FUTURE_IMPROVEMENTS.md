# Future Improvements

Open implementation work, in-game verification and focused savefile probes. Completed work and intentional design decisions do not belong here.
What is already built is in `CHANGELOG.md` and `docs/SINISKALC_ARCHITECTURE.md`; what the savefiles showed is in `docs/KNOWLEDGE_BASE.md` (HoMM3 and HoMM2); what is shared between the games and what is kept apart is in `.claude/agents/game-reconciler.md`. Put established facts in `KNOWLEDGE_BASE.md`, not in this checklist.

# Bugs
- HoMM3: temporary modifiers for morale, luck don't work properly.

# Improvements

## Game Features

1. **Editing the date.** Define the effects of moving time forward or backward before implementing it. At minimum, preserve consistency with HoMM2's "built today" set and HoMM3's current turn; establish how daily/weekly state should behave rather than assuming a date-byte edit is sufficient.
2. **Swapping the garrisoned and the visiting hero of a castle** , and moving a hero in or out to allow changing ownership of a castle with a visitor and/or garrison.
3. **Viewing and editing teams.** The team table of the header is found (Shadow of Death and Restoration of Erathia, HoTA pending) but not read. Reading it would also let the obelisk card mark a whole team at once.
4. **Extract the Grail location.** The save only shows whether the Grail has been dug up (see `KNOWLEDGE_BASE.md`); where the map hides it is not known.
5. **Places a HoMM3 hero visited**, what is left: the **mineral spring** (a Horn of the Abyss object, type 144 subtype 4: +1 luck for the next battle and +600 movement points until the end of the day; where a hero keeps that it visited one is not known; probe: Tazar visits a mineral spring, then Orrin a turn later, and a save a day later).
6. **HoMM3 Dwellings:** establish and expose editable guard/state data for neutral dwellings and generator types 18..20. Decode refugee-camp stock using `TestData/Hota/VisitedPlaces` save 30 and a new before/after-hire pair. Existing format evidence is in `KNOWLEDGE_BASE.md`.
7. **Model creature stack luck and morale** They can differ from the hero morale.

## Editor features
1. **Support for alternative HoMM2 and 3 clients** (fheroes2 for HoMM2, VCMI for HoMM3): the entities, models and pages are the same, the serialization stack will differ. Split `Siniskalc.Homm2` / `Siniskalc.Homm3` into the rules and the format (locator, chunks, (de)serializers) when the first of them is started.
2. **Cross platform support.** The core logic is platform-agnostic; the WPF UI could be replaced with Avalonia or MAUI. The view models in `Siniskalc.Wpf` and `Siniskalc.Heroes.Wpf` use little of WPF (`ICommand`, `INotifyPropertyChanged`); moving them to `net8.0` projects would leave only the views to rewrite.

## Limits of what is shown

- **HoMM3 weekly growth:** establish castle, horde-building and Grail contributions with controlled weekly transitions, then include them rather than showing only base creature growth.
- **HoMM3 faerie ring, fountain and idol of fortune:** isolate each reward in before/after saves before enabling edits.
- **HoMM3 visit history beyond the Utopia's defeated state:** compare the same Dragon Utopia with a second hero, another player and after a new day to distinguish any player-specific visit marks from the entrance state used by Restore. Locate the colossus and gazebo per-place marks with separate visits by two heroes and players.
- **HoMM3 merchants locator:** Replace heuristic Black Market discovery with a structural locator that also finds nearly sold-out tables and preserves unknown artifact codes.
- **Position versus entrance:** compare record coordinates with destination markers across mine/dwelling kinds before using them as exact "send to" destinations.
- **A defeated hero and the tavern.** A defeated hero cannot be rehired by its owner; it will not appear in its tavern rotation until it has been hired by another player and defeated again. The tavern has a table about 250 bytes before the date that changes when a hero is defeated, and might be what models the behaviour.
- **Banishing is partial.** The counter (player byte 22) is read and edited. The other bytes banishing changes are not decoded (the player's hero chunk gets owner `FF` and an emptied army, a counter and an 8-byte record near the end of the save, header bytes near 1313, 1356 and 12492), so a player whose countdown has reached 0 (banished) is read-only, and the editor can neither banish nor revive anyone.

## In-game verification of implemented edits

- HoMM3: morale, luck and visited-building bits; owner and stock edits of mines and dwellings, guard edits, merchant stock edits, the destination fix; forgetting a visited place; the banishing counter (does a lower or higher number of days make the game banish earlier or later); editing the tavern. 
- HoMM2: building rules (a turret or moat on a town without a castle, a shipyard inland, one building a day); the turn; "Can build this turn"; the growth rules (and whether a dwelling built or upgraded today gets its first creatures at once or next week; Threedom saves 15 and 16 suggest next week); T01BLDS (buildings), T02FORGT (a hero visits a fort or gazebo again after forgetting it), T03BONUS (morale and luck of the next battle), T04OBELS, T05FORT, T06SRC; that a changed owner behaves as expected now that `PlayerListsReconciler` keeps the players' lists in step; the Warlock (Waterfall) growth, since it was in none of the saves and only five factions are confirmed.
- Remove the relevant "Not yet tried in the game" notes only after the corresponding edited saves have been checked.

### HoMM3

- **Mine/dwelling formats outside HotA.** Repeat zero/one/two-mine and one/two-dwelling capture controls in SoD, including a no-mine map. Establish count widths in actual RoE, AB and Chronicles saves rather than assuming SoD's; the current `Roe` fixtures identify as SoD.
- **Find the cheat-used flag.** Test whether the date-marker byte identified in `KNOWLEDGE_BASE.md` persists, changes for other cheat types and affects scoring.
- **Temporary bonuses until the next battle** (hero chunk `+28`, `+29`, `+135..138`; the watering hole, swan pond, stables, crypt, faerie ring, fountain of fortune and idol of fortune are known). Missing bits: Temple, Oasis, Rally Flag, Mermaids, Fountain of Youth, Buoy, Shrine of Magic, Mystic Garden, Warrior's Tomb, Shipwreck survivor, Sirens. For each: visit, save; fight a battle, save; end the day, save. Also Stables a second week, and two places that may share a bit (Oasis + Temple, for `+135 0x10`).
- **Permanent-bonus places a hero visited:** the Horn of the Abyss kinds are decoded (`KNOWLEDGE_BASE.md`); the open question is the same for Shadow of Death. Series `TestData/Sod/VisitedPlaces/visited_places_<NN>_<what changed>.GM1`, one map, one human player, two heroes and two copies of the same place: baseline (both heroes in one town); hero A visits the first Arena; A visits it again (refused); B visits it; A visits the second Arena; a new week; then one save per kind (Marletto Tower, Star Axis by A and then B, Learning Stone, Mercenary Camp, Witch Hut with the skill it gave, Library with the skill levels). Add a README row for each.
- **Map-object state:** compare first and repeat visits, then a new week, to distinguish reset rules for Windmill, Water Wheel, Mystical Garden, Wagon, Campfire and Lean To. Separately compare before/after use of Sphinx, Pandora's Box, Seer's Hut, Genie's Lamp, Keymaster's tent and Border guard. The editor now shares the Utopia flow with Cyclops Stockpiles, Dwarven Treasuries, Imp Caches, Medusa Stores and Naga Banks: all five strongest exact resets have controlled in-game re-fights. Other entrance flag combinations stay unavailable pending a focused control; arbitrary preset/upgrade changes and resets under an occupying hero still need in-game confirmation for these five kinds.
- **Unnamed hero-chunk bytes** (see "Unnamed bytes of the hero chunk" in `KNOWLEDGE_BASE.md`): two heroes in two different towns, one in the first town and one in the third, then both outside (`+51/+52` as a town index); several battles with one hero, a save after each (`+53`, `+55`, `+44`, `+225` counters); a hero that fights, flees, is defeated and is hired again (`+1088` bits and the tavern memory); a boat boarded, sailed, landed, built, and HotA boats (`+30` and the HotA tail `+1102..1144`); one copy with only the level byte edited and one with only the experience (does the game ignore the level byte, as in HoMM2?); a new day, week and month with nobody moved (the 4282-byte block and `+77..79`); digging for the Grail at a wrong and at the right spot (the dug-tile number).

### HoMM2

- **Guardians of a mine:** spell codes, counts and clearing after an enemy capture have been established (see `KNOWLEDGE_BASE.md`). Needs figuring out: are partial guardian losses possible? What is the max value (seems to be 255).
- **Town ownership flags:** locate the town's map flag and update it alongside an editor ownership change; currently it stays stale.
- **Daemon cave, windmill and water mill:** isolate first use, repeat use and reset transitions to locate their state before enabling edits.

## Architecture and code

- **Measure UI stalls during file operations.** Time opening, reloading and saving the largest fixtures and a slow-drive scenario. If blocking is significant, move the operation off the UI thread with a busy state and overlapping-command protection; apply UI changes on the UI thread. Consider using async in I/O operations.
