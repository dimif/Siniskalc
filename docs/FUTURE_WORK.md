# Future Work

A list of open items that might or might not be worked on.

# Currently known limitations

- **HoMM3 Portal of summoning:** Figure out where the targeted dwelling/creature is stored and make it editable.
- **HoMM3 town creature growth:** (UI only) The editor does not look at hero items, grail building or external dwellings.
- **HoMM3 income:** (UI only) The editor does not look at hero skills, artifacts, specializations.
- **HoMM3 faerie ring, fountain and idol of fortune:** Only partially decoded.
- **HoMM3 merchants locator:** Right now Black Market use heuristic discovery rather than a structural locator, it might not work on all maps.
- **Map object position versus entrance tile:** The entrance tile doesn't always seem to match the object tile. This needs investigating and potentially adding as explicit data to the entities files.
- **Defeated heroes and tavern rotation.** A defeated hero seems not to be rehirable by its owner, and not to appear in its tavern rotation until another player has hired and defeated it; this is not confirmed. The tavern has a table about 250 bytes before the date that changes when a hero is defeated, and might be what models the behaviour.
- **Banishing and un-banishing players.** The days left can be edited, but the editor cannot banish a player outright or bring a banished player back.

# Improvements

## Editor features
Large pieces of work that would benefit all supported games, current and future.

1. **Cross platform support.** The core logic is platform-agnostic; the WPF UI could be replaced with Avalonia or MAUI. The view models in `Siniskalc.Wpf` and `Siniskalc.Heroes.Wpf` use little of WPF (`ICommand`, `INotifyPropertyChanged`); moving them to `net8.0` projects would leave only the views to rewrite.
2. **Validation of pending changes.** Add a list of game rules that checks the pending changes and flags problematic ones, such as resources set at the limit. Only one such warning exists so far: the hero page warns when a player's hero has no position on the map (HoMM3).
3. **Warning before saving a risky edit.** Building on item 2: when a save contains potentially game-breaking data, ask the user to confirm before saving.
4. **Accessibility** Ensure the app can be entirely navigated via keyboard and read by readers.
5. **Localization.** Extract all text into resources that can be replaced, to support other languages.

## Features for both games

1. **Editing the date.** Self-explanatory.
2. **Extract the Grail location.** The save only shows whether the Grail has been dug up (see `KNOWLEDGE_BASE.md`).
3. **Model creature stack luck and morale** They can differ from the hero morale.
4. **Support for alternative HoMM2 and 3 clients** (fheroes2 for HoMM2, VCMI for HoMM3): theoretically only the serialization stack should differ.

### HoMM3 specific
1. **Swapping the garrisoned and the visiting hero of a castle** , and moving a hero in or out to allow changing ownership of a castle with a visitor and/or garrison.
2. **Viewing and editing teams.** The team table of the header has been found (Shadow of Death and Restoration of Erathia, HoTA pending) but it is not yet read. Reading it would also let the obelisk card mark a whole team at once.
3. **Places a HoMM3 hero visited**, what is left: the **mineral spring** (a Horn of the Abyss object, type 144 subtype 4: +1 luck for the next battle and +600 movement points until the end of the day); where a hero keeps that it visited one is not known.
4. **HoMM3 Dwellings:** establish and expose editable guard/state data for neutral dwellings and generator types 18..20. Decode refugee-camp stock using `visited_places_30_tazar_refugee_camp` and a new before/after-hire pair. Existing format evidence is in `KNOWLEDGE_BASE.md`.
5. **Temporary bonuses until the next battle** (hero chunk `+28`, `+29`, `+135..138`; the watering hole, swan pond, stables, crypt, faerie ring, fountain of fortune and idol of fortune are known). Some map objects still need to be worked out: Temple, Oasis (Oasis + Temple, for `+135 0x10`), Rally Flag, Mermaids, Fountain of Youth, Buoy, Shrine of Magic, Mystic Garden, Warrior's Tomb, Shipwreck survivor, Sirens.
6. **Permanent-skill-bonus buildings:** many, including the Horn of the Abyss ones, are decoded (see `KNOWLEDGE_BASE.md`); some might still be missing.
7. **Map objects:** Windmill, Water Wheel, Mystical Garden, and all of the HoTA weekly resource generators. Figure out how to read/edit content and/or state of Pandora's Box, Seer's Hut, Genie's Lamp, Keymaster's tent, Border guard, Wagon, Campfire and Lean To.
8. **Unnamed hero-chunk bytes** (see "Unnamed bytes of the hero chunk" in `KNOWLEDGE_BASE.md`): two heroes in two different towns, one in the first town and one in the third, then both outside (`+51/+52` as a town index); several battles with one hero, a save after each (`+53`, `+55`, `+44`, `+225` counters); a hero that fights, flees, is defeated and is hired again (`+1088` bits and the tavern memory); a boat boarded, sailed, landed, built, and HotA boats (`+30` and the HotA tail `+1102..1144`); one copy with only the level byte edited and one with only the experience (does the game ignore the level byte, as in HoMM2?); a new day, week and month with nobody moved (the 4282-byte block and `+77..79`); digging for the Grail at a wrong and at the right spot (the dug-tile number).
9. **Find the cheat-used flag.** Test whether the date-marker byte identified in `KNOWLEDGE_BASE.md` persists, changes for other cheat types and affects scoring.

### HoMM2 specific

1. **Guardians of a mine:** spell codes, counts and clearing after an enemy capture are established (see `KNOWLEDGE_BASE.md`). Still to find out: the largest guardian count (it seems to be 255).
2. **Daemon cave, windmill and water mill:** isolate first use, repeat use and reset transitions to locate their state before enabling edits.

## Architecture and code

- **Use Async I/O** Currently most of the I/O is synchronous. Test with large files and slow read speeds. If blocking is significant, move the operation off the UI thread with a busy state and overlapping-command protection; apply UI changes on the UI thread. Consider using async in I/O operations.
