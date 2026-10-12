# Siniskalc user guide

Siniskalc edits savegames of Heroes of Might and Magic III (Restoration of Erathia, Armageddon's Blade, Shadow of Death, Horn of the Abyss, and *maybe* even Heroes Chronicles, for heroes and towns only.
It needs Windows; there is nothing to install, unzip a release and run `Siniskalc.exe`.

## Before you start: back up your saves

The editor patches bytes of a file format nobody documented officially.
Copy your savegames somewhere safe first.
The editor also keeps a copy next to the save before it saves over it (see [Backups](#backups)), but a copy of your own is the one you can trust.

Nothing is written until you choose **File > Save**.

## Open, edit, save

1. **File > Open** (Ctrl+O) and pick a savegame.
   **File > Recent** lists the last ones.
2. Pick what to edit in the tree on the left.
   The tree shows the game, then one node per player, each with its heroes and towns.
3. Change values on the page.
   A dot next to a node in the tree means it has changes that are not saved.
4. **File > Save** (Ctrl+S).
   **File > Reload** throws away every change and reads the file again.

Pages:

- **Game overview**: map and game setup read from the save (difficulty, date, who controls each player).
- **Player**: the seven resources, the heroes in the tavern, and the obelisks the player has visited (on maps that have obelisks).
- **Hero**: primary and secondary skills, level, experience, class, spell points, movement, position, army, war machines, spell book, artifacts, backpack and biography.
- **Town**: owner, faction, mage guild level, garrison, built and buildable structures, creatures in the dwellings.

## Undo, redo and pending changes

- **Edit > Undo** (Ctrl+Z) and **Redo** (Ctrl+Y) step through your edits.
- **View > Pending changes** lists every value that differs from the file, before and after, each with an Undo button.
  Look at it before you save.
- **Edit > Revert all changes** puts everything back to what the savegame had.

## Settings

The settings are kept per user in `settings.json` in the `Siniskalc` folder of your AppData (`%APPDATA%\Siniskalc`), and are applied as soon as you change them.

- **Settings > Back up on save** (on by default): keeps a copy of the savegame before **Save** replaces it.
  See below.
- **Settings > Advanced mode** (off by default): shows the tools for working with the raw bytes.
  See below.

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
