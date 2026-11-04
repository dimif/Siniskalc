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
- **Creature banks (HotA)**: shared overviews and cards show guards, rewards and defeated state, with guard presets and a Reset for another battle. See [Creature banks](#creature-banks-horn-of-the-abyss) for the reset workflow and verification limits.
- **Hero**: primary and secondary skills, level, experience, class, spell points, movement, position, army, war machines, spell book, artifacts, backpack and biography.
- **Town**: owner, faction, mage guild level, garrison, built and buildable structures, creatures in the dwellings. In HoMM3 a town with an Artifact Merchant built also has an **Artifact merchant** tab. Its seven slots are shared by every town and owner, separate from the Map page's Black Markets. Clear a slot to mark it sold. **Reset shared stock** restores that stock only; the town's **Reset** does not change it. Unsupported save layouts show an unavailable notice. Editor-written stock has not yet been checked in the game.

## Creature banks (Horn of the Abyss)

Open **Map → Creature banks** and choose a type. All six supported types use the same overview and detail cards.
The overview lists tier, entrance location, **Defeated/Undefeated** and retained rewards. Pick a row or a location in the navigation tree to highlight its card and scroll it into view.
Each card has its name and location beside **Defeated** or **Uncleared** and **Reset**, followed by three columns: **Garrison**, **Change guard strength and rewards**, and **Rewards**.

| Bank | Guards | Rewards |
|---|---|---|
| Dragon Utopia | Green, Red, Gold and Black Dragons | 20,000–50,000 gold and four editable artifact slots |
| Cyclops Stockpile | Cyclopes, optionally a Cyclops Kings centre | Each of the six non-gold resources |
| Dwarven Treasury | Dwarves, optionally a Battle Dwarves centre | Crystal and gold |
| Imp Cache | Imps, optionally a Familiars centre | Mercury and gold |
| Medusa Stores | Medusas, optionally a Medusa Queens centre | Sulfur and gold |
| Naga Bank | Nagas, optionally a Naga Queens centre | Gems and gold |

### Resetting a defeated bank

1. Select the bank and check the displayed **Garrison** and **Rewards**. On a defeated card, Garrison means the guards that Reset will restore, not surviving guards.
2. Keep the current tier to retain its resource reward, or choose another of the four guard/reward presets. For a non-Utopia bank, choose **Upgraded centre stack** if wanted.
3. Click the card's **Reset**, or select **Undefeated** in its overview row.
4. Choose **File > Save**, then load the edited save in Heroes to fight the bank again.

A defeated non-Utopia record has erased its original centre upgrade. The card explains this and defaults to unupgraded guards; choosing an upgrade while defeated alone writes nothing until Reset.
Reset re-arms the bank and its entrance without granting rewards immediately or taking away anything already earned.
Changing strength alone does not reset a defeated bank or reroll Utopia artifacts. Empty artifact slots show **(empty)**; overview rewards use compact gold amounts such as **50k gold**.
Selecting **Defeated** clears the guards and updates the entrance without awarding anything.

**The two Resets have different purposes:** a bank card's Reset prepares another battle; the Map header's Reset undoes unsaved merchant and bank edits.
All edits support Undo, Redo and individual reverts through Pending changes. Changing a bank does not change the player's resources.

**Verification limits:** original-content Utopia resets were confirmed in-game for all four tiers. The exact original strongest resets of the other five types were also successfully re-fought.
Arbitrary tier/upgrade changes, edited rewards, marking an undefeated bank as defeated, and non-Utopia resets while a hero occupies the entrance still need in-game confirmation.
Unknown entrance flags and unsupported layouts remain uneditable rather than guessed; bank editing is HotA-only.

With **Advanced mode** on, selecting a bank shows a hex editor below the overview for that bank record's guards, resources and, for Utopias, artifacts.
The separate entrance and occupying hero are not part of this byte view; changing defeated state through the normal controls updates the entrance automatically.
Switching selection or turning Advanced mode off releases the byte editor without discarding edits.

## Mine garrisons (HoMM3)

Pick a mine on its owner's **Mines** tab to edit the guard. An **edited** label beside its garrison heading marks unsaved changes for that mine and updates on selection, Undo and Redo.
The mine editor uses the individual army-slot pickers rather than a separate **Find creatures** card; hero and town army finders are unchanged.

## Undo, redo and pending changes

- **Edit > Undo** (Ctrl+Z) and **Redo** (Ctrl+Y) step through your edits.
- **View > Pending changes** lists every value that differs from the file, before and after, each with an Undo button.
  Look at it before you save.
- **View > View Artifacts** and **View > View Creatures** open a read-only list of every artifact or creature of the game, with the byte code the savefile uses (in hex, such as `0x2A`) left of the name. Search by name, effect, town or level as in the finders, or type a code (`2A`, `0x2A` or `42`) to find it. Nothing here changes the savegame. Both need a game to be open.
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
