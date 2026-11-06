# Changelog

All notable changes to Siniskalc.
Versions follow [Semantic Versioning](https://semver.org/); a release is made by pushing a tag `vMAJOR.MINOR.PATCH` (see `.github/workflows/release.yml`).

## [2.0.0] - 2026-11-06

- On top of **Heroes of Might and Magic III** (all official expansions + Horn of the Abyss), Siniskalc now edits **Heroes of Might and Magic II** savegames, both for base (The Succession Wars) and the expansion (The Price of Loyalty).
- The HoMM3 editor gains editing of creature banks, merchants and modelling and editing of many more entities.

### Highlights

- **Heroes of Might and Magic II support.** The editor opens the savegames of the base game (`.GM1`) and of the Price of Loyalty (`.GX1`) next to the HoMM3 ones, and the window title follows the game. The editor presents a layout similar to the HoMM3's editor: a navigation tree of players, heroes and towns, tabs, "edited" marks, Reset, undo and the pending-changes list. You can edit player resources, heroes (skills, army, spells, artifacts) and castles (owner, buildings, dwellings, garrison, Mage Guild). Each page has a hex tab in Advanced mode.
- **HoMM2 and HoMM3 game data is loaded from JSON files** Creatures, abilities, artifacts, factions, castle buildings, spells, hero classes, secondary skills and war machines come from the `GameData` folders. You can add content within the game's existing mechanics by editing those files and restarting. The editor validates them at start-up and, if a file is missing or invalid, stops and names the file and the problem. See `docs/GAME_DEFINITIONS.md`.
- **A log file and a Settings page for it.** The editor writes a plain-text log (`siniskalc-<date>.log`, the last 7 days kept) to `%LOCALAPPDATA%\Siniskalc\logs` (Linux and macOS: `~/.local/share`). It records what the editor did and every value it did not recognise and replaced with a stand-in. **Help > Show logs** opens the folder, and **Settings... > Logging** sets the level and the folder (applied at the next start). The format, destinations and number of days kept are in `serilog.json`, written next to `settings.json` on first start. If either file cannot be read, the editor says so at start-up and offers to reset it. See the user guide and `docs/LOGGING.md`.

### Added in both games

- **View > View Artifacts... and View > View Creatures...** open a read-only list with the usual finder. A **Code** column shows each item's byte code in hex, and the search also finds a code typed as `2A`, `0x2A` or `42`. Click the Code heading to sort by it.
- **An "Order of play" card on the overview** lists the players in the order they play and highlights the one whose turn it is. Pressing another human player's button gives them the turn (a computer player cannot be given it), as a pending change you can undo. **Tried in the game in both games.** In HoMM2 it writes the number of the player's record. In HoMM3 it writes the colour after the setup's human flags, the one value that changed with the turn between consecutive test saves. Where the editor cannot write the turn, the button is shown disabled with the reason.
- **A hero's destination and facing** (the tile it is heading to, and the direction it faces) can be read and edited on the hero page. HoMM3 now calls its "marker" the destination.
- **Banishing.** A player with no towns shows how many days are left before the game banishes them: a pill beside the name on the player page ("No towns: 6 days left", "Banished"), the same pill on the overview card and "6 days left" in the navigation tree. The first tab of the player page is now called **General**. For such a player it has a Banishing card with buttons 7 to 1 to change the days left. A banished player cannot be edited.
- **The primary skills card shows base, total and the artifacts' share.**
- **Pending changes read the same in both games.** A hero's experience and level are one change ("Experience 1,500 (level 3)"). The position includes the facing and the map level. The destination reads "none" or "X: 30, Y: 30". Movement shows what is left of a full day. Spell points use a thousands separator. A player's tavern lists each place with the hero's name. The places a hero visited are listed one kind at a time ("Visited: forts"). A change to the days left is listed in both games.
- **Both games look and work the same.** The header, tabs, resources, cards and owner picker are shared. A castle nobody owns is "Neutral" in both games. The HoMM2 castle page has the same tabs as HoMM3 (Buildings, Garrison, Available creatures, Mage guild, Hex) and the same header. In HoMM2 pending changes, "Army" and "Skills" are now "Creatures" and "Primary skills" / "Secondary skills".
- Changing a castle's owner in HoMM3 and a hero or castle owner in HoMM2 can be reverted in one step together with the players' lists.
- **The owner drop-down of a hero or a castle lists only Neutral and the players of the game** (and the present owner, if the savegame holds a colour that has no player), instead of every colour. The mine and dwelling owner pickers still list every colour.

### Added in Heroes of Might and Magic II

- **Who plays each colour.** Player pages and the overview say whether each colour is "Human" or "Computer" and whose turn it is. This is shown only; changing it is not offered until a save made in the game proves it is safe.
- **Mines, and a Map page for shrines and sphinxes.** The mines of a player (or of nobody) are on the Mines tab of that player or of Neutral.
  - A mine shows its kind, where it is and its owner, which can be changed. Its guardian (elemental type and count, or "Unguarded") is shown but cannot be edited, and an unrecognised guardian encoding is identified rather than shown as unguarded.
  - The Map page lists the shrines and sphinxes of the map.
  - A shrine shows its circle, position and the spell it teaches, which can be changed.
  - A sphinx shows whether its riddle can still be answered, plus the reward, the answers and the riddle, all editable.
  - In Advanced mode a Hex tab shows the raw records of the shrines and sphinxes.
- **The hero page lists the places the hero visited, with where they are** (forts, gazebos, witch doctor's huts, mercenary camps, standing stones, Xanadu). You can forget or remember them one at a time so the hero visits them again. It also shows what the hero remembers of the fountain, oasis, faerie ring, graveyard, temple, arena and stables.
- **The player page lists the map's obelisks with their coordinates**, as HoMM3 does.
- **Castles show and edit more.** Each dwelling shows its weekly growth: the creature's growth, +2 from the Well, and +8 more in the first dwelling from the faction's second well (Farm, Garbage Heap, Crystal Garden, Waterfall, Orchard, Skull Pile). An unbuilt dwelling gains nothing. A **Can build this turn** box at the top of the Buildings tab reads and writes the game's one-building-a-day flag. The marketplace, turrets, moat, captain's quarters, second well and each faction's special building (Library, Storm and so on) can be edited. A hero's morale and luck for the next battle can be edited too.
- **Mage Guild slots offer the spells of their guild's level** (level N teaches level N spells). A spell of another level that the savegame already holds there is still shown and kept. This is the editor's rule and is not proved to be the game's. A guild also shows only three spells at its first levels (a fourth only with the Library of a Wizard), whatever the savegame holds in the fourth slot. **The levels of 20 spells were corrected** to match the guilds and shrines in the saves (for example Magic Arrow is 1 and Armageddon is 5), which also changes the Level column of the spell finder.
- The player page has a Banishing card, and the tree shows a castle badge and the visiting hero.
- **Giving a castle to another player changes how it looks on the map and on the minimap.** The two flags drawn on the map and the game's table with the colour of each castle (which the minimap draws from) are written with the owner, so the castle no longer looks like the old owner's.

### Added in Heroes of Might and Magic III

- **A Map page with Mines, Dwellings and Merchants tabs.**
  - **Mines:** each mine with its kind, tile and owner (editable), and the creatures guarding it (an abandoned mine has some until it is taken), edited with the hero army editor. Sortable by resource, owner and location. The tile is not where the mine is entered; that is still being worked out.
  - **Dwellings:** each creature generator with its creature, the tile a hero stands on to flag it, its owner (editable), its weekly growth ("+N a week") and how many creatures can be hired now (editable). The list is sortable by creature, owner, location and creatures to hire for convenience.
  - Mines and dwellings also show which level of the map they are on (surface or underground).
  - **Merchants:** every Black Market on the map with its tile and seven stock slots, picked like a hero's artifact. A cleared slot is an artifact that was sold. The number in front of the list is the number of merchants.
- **Creature banks (Horn of the Abyss).** Under **Map > Creature banks** in the navigation tree: Dragon Utopias, Cyclops Stockpiles, Dwarven Treasuries, Imp Caches, Medusa Stores and Naga Banks. Each type has its own group, summary and list, and selecting a location highlights it and scrolls to that bank.
  - Cards show the guards, gold, artifacts and whether the bank was defeated. A Defeated/Undefeated selector changes the guards and the entrance state together, with undo and redo.
  - **Reset** restores the guards and the entrance state, even underneath a hero standing on it. For a defeated bank, the **Garrison** heading shows the guards that Reset will restore.
  - Guard and gold presets and artifact edits do not re-roll rewards or restore a bank by themselves. Unsupported entrance flags are kept and not offered for editing. A defeated bank cannot reveal its old centre upgrade, so the card says so rather than guessing.
  - **Tried in the game:** restoring Dragon Utopias works for all four original tiers (the gold and the four artifacts are awarded again), the strongest-tier resets of the other five bank types work, and so do changing the guard preset or tier, picking other rewards, marking an undefeated bank as defeated and resetting a bank while a hero stands on its entrance. What other players' visits and the next day do to an edited bank is not checked.
  - In Advanced mode the selected bank also has a hex editor with the known fields, undo and redo.
  - **The army editors have no separate Find creatures card** (hero armies, town garrisons, mine guards): picking a slot opens the creature finder as a popover, in both games.
- **An Artifact Merchant tab on towns that have the building.** Its seven slots edit one stock shared by all towns and owners, with undo and redo and reverting individual pending changes. Like the other cards, the card shows an "edited" mark and a **Reset** pill while the stock has unsaved changes. A save the editor cannot read shows an "unavailable" notice instead of editable guesses. Tried in the game, for the town merchants and for the Black Markets of the Map page.
- **A Places tab on the hero page** lists the places with permanent bonuses that the hero visited (learning stones, Marletto towers, gardens of revelation, mercenary camps, star axes, trees of knowledge, libraries of enlightenment, arenas, schools of magic and war, and in Horn of the Abyss the colossi and gazebos). You can forget or remember them one at a time, or use **All none** / **All visited** at the top. Trees of knowledge are listed with where they are. Tried in the game.
- **Morale and luck for the next battle**, and the buildings a hero visited today (watering hole, swan pond, crypt, stables), can be edited. Tried in the game. **Ticking the Stables also adds their movement points**: +400 in Shadow of Death and +300 in Horn of the Abyss (the values seen in the game), to the maximum of the day and to the points left, until the end of the week; unticking takes them away and ticking twice adds them once. The amounts are in `visitEffects.json` (`movement`, `versionMovement`); Restoration of Erathia, Armor of the Damned and Chronicles use the Shadow of Death amount, which is unchecked.
- **Changing a hero's owner keeps the rest of the save in step.** The players' hero lists (count, ids, selected hero) and the game's table of hero owners change together, as one undo step, and the change appears in the pending changes. A free hero who is not in a tavern can be given to a player (up to 8 heroes; a full list is refused). Heroes in a tavern, campaign heroes and garrisoned heroes are refused, with the reason. Tried in the game.
- **Changing a castle's owner keeps both players' records in step**, including the record of a hidden colour. Town counts and ordered town lists are updated: new towns are appended, lost towns are removed without re-sorting, and unused slots become `FF`. Tried in the game, with the banishing countdown.
- **Giving a castle to another player starts or ends the banishing countdown**, as the game does: a player who loses their last town gets 7 days, and one who regains a town is no longer counted. A banished player is left alone.
- **A castle's Allow / Forbid button for a building is now a pending change.**
- **Heroes the game keeps out of play have their own place.** Heroes disabled in the map editor and campaign heroes are in a "Disabled heroes" group under Neutral in the navigation tree and in a "Disabled heroes" tab of the Neutral page (shown when there are any); the Heroes tab leaves them out, a hero a tavern offers is not among them, and the Neutral summary reads "N heroes · M disabled · K towns". They are not given to a player, and the message says why.
- **A warning on the hero page when a player's hero is not on the map.** A neutral hero given to a player keeps no tile, and the game crashes on a player's hero without a valid tile, so the header says to set a position on the General tab before saving. It disappears as soon as the hero has a position.

### Fixed in Heroes of Might and Magic III

- **Primary skills of a hero.** The savefile already includes the bonus of equipped artifacts in a hero's attack, defense, spell power and knowledge (Edric goes from 2/2/1/1 to 11/11/9/9 with Titan's Thunder), and the editor used to add it a second time. The "Total" and the maximum spell points were too high, and equipping an artifact did not change the skills as the game does. Now the saved skills are what counts, the stepper edits the base value without artifacts, and equipping or removing an artifact moves the skills by what it gives. The pending changes list the skills too. The artifact bonus table is checked against the game only for Titan's Thunder and Armageddon's Blade.
- **Red and Blue had no page in some savegames** (the four-player test map). The player table was not found for them, so their General tab, banishing counter and everything else were missing.
- **Neutral towns with a non-zero byte after the Mage Guild level were skipped.** That hid two town shops and four SoD neutral towns. The unknown byte is kept.
- The Catapult is no longer listed as an artifact; the editable war machines have their own list.

### Behind the scenes

- **Game definitions are simpler to write.** Artifact effects are short attribute/amount lists, default flags can be left out, and array position sets display order. HoMM3's visitable places are named `visitablePlaces`, HoMM2 spell categories are in `spellTypes.json`, and both games keep `mines.json`, `visitablePlaces.json`, `visitEffects.json` and `mapObjects.json` in their `standard` folder. Versions in the JSON are named `roe`, `ab`, `sod`, `chronicles`, `hota` (HoMM3) and `sw`, `pol` (HoMM2).
- **Code reorganised for several games.** The HoMM3 code moved into the library `Siniskalc.Homm3` and the HoMM2 code into `Siniskalc.Homm2`. `Siniskalc.Core` and `Siniskalc.Wpf` hold what knows no game, `Siniskalc.Heroes` and `Siniskalc.Heroes.Wpf` hold what the Heroes games share, and the exe `Siniskalc` is only the host. The window, menus and file commands are a game-neutral shell, and both games use one savegame service. `RawSavefileCodec` reads and writes an uncompressed savegame. See `docs/SINISKALC_ARCHITECTURE.md`, "Adding a game".

## [1.0.0]

- First release. HoMM3 support.
