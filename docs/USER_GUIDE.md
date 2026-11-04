# Siniskalc user guide

Siniskalc edits savegames of Heroes of Might and Magic III (Restoration of Erathia, Armageddon's Blade, Shadow of Death and Horn of the Abyss) and Heroes of Might and Magic II (The Succession Wars and The Price of Loyalty).
Heroes Chronicles support is speculative and limited to heroes and towns.
HoMM2 editing has savefile regression coverage but has not yet been verified in the game.
It needs Windows; there is nothing to install, unzip a release and run `Siniskalc.exe`.
Keep both shipped `GameData` folders beside the executable.

## Standard game definitions

The editor loads entity definitions from `GameData\Homm2` and `GameData\Homm3` once at startup.
Editing these files requires restarting the editor; **File > Reload** reloads only the savegame.
Missing or invalid definitions stop startup with the file and cause; correct or restore the named file rather than expecting built-in fallback data.
There are no custom profiles or live reload.
See [Editing game definitions](GAME_DEFINITIONS.md) for the formats, supported effects and limits of JSON-only changes.

## Before you start: back up your saves

The editor patches bytes of a file format nobody documented officially.
Copy your savegames somewhere safe first.
The editor also keeps a copy next to the save before it saves over it (see [Backups](#backups)), but a copy of your own is the one you can trust.

Nothing is written until you choose **File > Save**.

You can also open a savegame through Windows **Open with...** by choosing `Siniskalc.exe`,
or run `Siniskalc.exe "C:\Games\My save.GM1"` from a command prompt.
The editor opens the first file path passed at startup in a new window and detects its game automatically.
The portable ZIP does not register file associations with Windows.

## Open, edit, save

1. **File > Open** (Ctrl+O) and pick a savegame.
   **File > Recent** lists the last ones.
2. Pick what to edit in the tree on the left.
   The tree shows the game, then one node per player, each with its heroes, towns, mines and dwellings, then **Neutral** and the **Map**.
   Below **Mines** the mines are grouped by the resource they give (Gold, Wood, ...), and below **Dwellings** (HoMM3) the dwellings are grouped by the faction of their creature, so `Red > Mines > Gold` holds the gold mines Red owns.
   A player with no mine or dwelling has no such node.
3. Change values on the page.
   A dot next to a node in the tree means it has changes that are not saved.
4. **File > Save** (Ctrl+S).
   **File > Reload** throws away every change and reads the file again.

Pages:

- **Game overview**: map and game setup read from the save (difficulty, date, who controls each player).
- **Player**: the seven resources (tab **General**), the heroes in the tavern, and the obelisks the player has visited (on maps that have obelisks).
  A player who owns no town has a "No towns: n days left" pill beside the name, in the overview and in the navigation tree, and a Banishing card on the General tab with the buttons 7 to 1 to change the days left. A player who has been banished stays in the game as he is and cannot be edited.
  Its **Mines** tab lists the mines the player owns (pick one to see and edit its guard, and change its owner) and its **Dwellings** tab (HoMM3) the dwellings it owns, with the creatures that can be hired there. A mine or dwelling given to another player moves to that player's tabs.
- **Neutral**: the heroes and towns nobody owns, and the mines and dwellings nobody has flagged (tabs **Heroes**, **Towns**, **Mines**, **Dwellings**). It has no resources and no Hex tab: the bytes of a neutral hero or town are on its own page, and those of a mine or dwelling below its list.
- **Map**: what the map keeps that no player owns: the adventure-map Black Markets and supported creature banks (HoMM3), the shrines and sphinxes (HoMM2). HoMM3's town Artifact Merchants share separate stock that is not shown or edited here.
- **Map navigation (HoMM3)**: **Map → Merchants** lists all located adventure-map Black Markets. **Map → Creature banks** groups **Dragon Utopias, Cyclops Stockpiles, Dwarven Treasuries, Imp Caches, Medusa Stores and Naga Banks**. Select a type for its summary and list, or a location below it to highlight that bank and scroll it into view. Types not yet supported are not listed.
- **Creature-bank cards (HotA)**: all six types share the same overview and compact cards: tier, location, **Defeated/Undefeated**, guards and retained rewards. **Reset** restores the displayed guards and entrance for another battle without granting resources to the player. Changing strength also chooses that preset's resource reward, but does not automatically reset a defeated bank or reroll Utopia artifacts. All edits use normal history, pending changes and undo/redo.
- **Upgraded centre**: the five non-Utopia kinds may upgrade their centre stack. An undefeated bank shows its actual choice. A defeated record has erased its guards, so the card explains that its original upgrade is unknown; choose **Upgraded centre stack** before Reset if wanted (the default is unupgraded). Choosing it on a defeated card alone writes nothing until Reset. Only the exact original strongest resets have been re-fought in game; arbitrary tier/upgrade changes and occupied non-Utopia resets remain unconfirmed in game. Unestablished entrance flags are left uneditable rather than guessed. No additional saves are required for these supported presets.
- **Utopia overview**: the **Dragon Utopias** node shows a mines-style list above the detail cards: tier, entrance location, **Defeated/Undefeated**, and rewards (for example, **50k gold** followed by the four artifact names). Selecting **Undefeated** restores the guards and entrance for another battle, just like the card's **Reset**; selecting **Defeated** clears them without awarding the player anything. Both are undoable. Empty artifact slots show **(empty)**.
- **Utopia bytes (Advanced mode)**: select a Utopia in the overview or navigation tree to show its hex editor below the overview. It edits that bank record, with labelled guard types/counts, resources and artifacts. The separate map entrance or occupying hero is not part of this byte view; a defeated-state change reconciles the entrance automatically. Turning Advanced mode off releases the byte editor.
- **Other bank bytes (Advanced mode)**: the same hex view works for all supported banks, showing only their own record and fields. Selecting another type or location releases the old target. These five bank kinds have no artifact or creature reward; their resource rewards remain visible after defeat.
- **Dragon Utopias (HotA)**: cards match the overview list's width and show the Utopia number and entrance coordinates in one heading, with **Defeated** or **Uncleared** beside **Reset**. Three aligned columns show **Garrison**, always-visible **Change guard strength and rewards** controls, and **Rewards**, keeping each card compact vertically. For a defeated bank, the garrison is what Reset will restore, not living guards. Its **Reset** re-arms it and permits another battle for its rewards; previously awarded gold and artifacts are not taken away. The four strength presets award 20,000–50,000 gold; changing strength leaves artifacts alone. Editing a defeated bank's contents does not restore it until you click its **Reset** or select **Undefeated** in the overview. Use Undo or the pending changes list to revert individual edits; the Map header's reset undoes unsaved map edits. Restoring original contents was confirmed in-game for all four tiers; edited presets, rewards and marking a previously undefeated bank as defeated remain unverified in-game. Other game versions or unsupported bank/entrance layouts show an unavailable notice.
- **Hero**: primary and secondary skills, level, experience, class, spell points, movement, position, army, war machines, spell book, artifacts, backpack and biography.
- **Town**: owner, faction, mage guild level, garrison, built and buildable structures, creatures in the dwellings. In HoMM3 a town with an Artifact Merchant built also has an **Artifact merchant** tab. Its seven slots are shared by every town and owner, separate from the Map page's Black Markets. Clear a slot to mark it sold. **Reset shared stock** restores that stock only; the town's **Reset** does not change it. Unsupported save layouts show an unavailable notice. Editor-written stock has not yet been checked in the game.

## Undo, redo and pending changes

- **Edit > Undo** (Ctrl+Z) and **Redo** (Ctrl+Y) step through your edits.
- **View > Pending changes** lists every value that differs from the file, before and after, each with an Undo button.
  Look at it before you save.
- **Edit > Revert all changes** puts everything back to what the savegame had.

## Settings

The settings are kept per user in `settings.json` in the `Siniskalc` folder of your AppData (`%APPDATA%\Siniskalc`), and are applied as soon as you change them. The first time the editor starts it writes `settings.json` with the defaults, and `serilog.json` (how and where the log is written: the template, the rolling interval and the days kept) beside it; both can be edited by hand.

- **Settings... > Saving > Back up on save** (on by default): keeps a copy of the savegame before **Save** replaces it.
  See below.
- **Settings... > Tools > Advanced mode** (off by default): shows the tools for working with the raw bytes.
  See below.
- **Settings... > Logging > Log level and Log folder**: how much the editor writes to its log file and where the file goes.
  See below.

### The log

The editor writes a plain text log of what it did and of anything unexpected it met (a value it does not know, a file it could not read, a save that failed).
It is meant for bug reports, and nothing in it leaves your computer.

- **Help > Show logs** opens the folder of the log files.
  There is one file for each day, `siniskalc-<date>.log`, and the last 7 days are kept.
  The default folder is `%LOCALAPPDATA%\Siniskalc\logs` (on Linux and macOS, `~/.local/share/Siniskalc/logs`).
- **Settings... > Logging** sets the **Log level** and the **Log folder** (Browse... picks one, Use default empties the box).
  The level says how much is written: *Information* (the usual) lists what you opened and saved and anything unexpected; *Debug* and *Trace* add detail (for example, each value in a
  savegame that the editor did not know and read as a stand-in); *Warning*, *Error* and *Critical* write less. *Default* leaves the level to `serilog.json` (Information, as shipped).
  A folder is checked when you press OK: it has to be a full path the editor can write to.
  The choices apply **the next time the editor starts**.
- Every line starts with the time and the level, then the part of the editor that wrote it, then what it is about when it is about one hero, castle or player
  (`Siniskalc.Homm3.Data.HeroDeserializer (Hero Sandro): Class byte FE read as Knight and kept until the class is changed`).
- **How and where the log is written** (the template of a line, the sinks, the rolling interval, how many days are kept) is in `serilog.json`, beside `settings.json`
  in `%APPDATA%\Siniskalc`. The editor writes it with the defaults the first time it starts; edit it with a text editor, for example `"retainedFileCountLimit": 30` to keep a month, and restart.
  The log folder is `{LogFolder}` in its `path`, so leave that as it is unless you want a fixed path.
  If the file cannot be read, the editor says so when it starts and offers to put it aside (`serilog.json.bad`) and start with the default one.
  The details are in [LOGGING.md](LOGGING.md).

### Backups

When **Back up on save** is on, **Save** first copies the file it is about to replace into the same folder:

| Save number | Copy of the previous contents |
|---|---|
| 1st save of `save1.GM1` | `save1.GM1.bak` |
| 2nd | `save1.GM1.bak1` |
| 3rd | `save1.GM1.bak2` |

A copy is never overwritten: each save adds the next number after the highest one in the folder, so every earlier state of the file is kept.
The editor never deletes them, so remove the ones you no longer need yourself.
To go back, close the editor (or just copy), delete or rename the savegame and rename the copy you want to the name of the savegame.

When it is off, **Save** replaces the file without keeping anything.
The first save of a file that does not exist yet has nothing to copy.

### Advanced mode and the hex tools

Advanced mode is for finding out where things are in a savefile and for editing what the editor does not model yet.
It shows:

- a **Hex** tab on every player, hero and town, which edits the raw bytes of that part of the savefile;
- a **Hex** card below the mine (and, in HoMM3, the dwelling) that is picked on the Mines and Dwellings tabs, which edits the bytes of that one record;
- a **Hex** card for the selected supported HoMM3 creature bank below its overview list;
- the **Advanced** menu: **View save hex** shows the whole file with its known regions, **Export save bytes** writes the decompressed savegame to a `.bin` file and **Load save from bytes** reads one back.
  These files are useful to compare two saves in a diff tool and to attach to a bug report.

Turning it off hides them again; if a Hex tab is showing at that moment the editor goes back to the first tab.
Nothing that was edited is lost.

## Things you may notice

- Heroes, creatures or towns the editor does not know (from a mod or a newer version of the game) should still load.
  They will show up as the first entity in the game list (e.g. a hero class the editor does not know shows as Knight, the first hero class) and the file keeps its original byte until you change that field yourself.

## Troubleshooting and reporting a problem

- The save does not load: the editor says when the file is not compressed like a savegame (or is damaged), or is not a savegame of a version it knows.
  Check it is a real savegame (`.GM1`, `.CGM`, ...) and not a map.
  Make a copy, then try again.
- **File > Reload** is off for a game loaded from a dump until you save it, as a dump has no savegame to read again.
- The game rejects or crashes on an edited save: restore a copy (`.bak`, `.bak1`, ... see [Backups](#backups)) and tell us what you changed.

To report a problem use **Help > Report an issue**, or <https://github.com/dimif/Siniskalc/issues>.
Say which game and version it is and what you did, and attach the savegame (or a dump from the Advanced menu) if you can.
Attach the log of the day too (**Help > Show logs**); set the level to *Debug* first and repeat what you did if the problem is about a value the editor misread.
The log names the savegame files but does not hold their contents; a full path appears only when the path itself is the problem (the settings file, the log folder).
