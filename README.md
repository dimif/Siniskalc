# Siniskalc

## What
A Windows desktop savegame editor for **Heroes of Might and Magic III** (HoMM2 and other titles planned).
It edits savegames (`.GM1`, `.CGM`, and the other save extensions the game writes).
With it you open a save, browse the game by player, hero and town, change what you want, and the editor writes it back.

Currently supports Restoration of Erathia, Armageddon's Blade, Shadow of Death and Horn of the Abyss (HotA).
Do note that Heroes Chronicles support is entirely speculative and extracted from the work of Erki Suurjaak on h3sed, as I do not own a copy of the game.
Only its heroes and towns are read: there are no players, date or game setup.

Siniskalc allows you to edit the following entities in a savegame:
* **Players**: resources, tavern, heroes, towns, visited obelisks.
* **Heroes**: player control, class, gender, level, experience, primary and secondary skills, spell points, movement points, position on the map and facing direction, spell book, creatures, artifacts, backpack.
* **Towns**: faction, owner, position, built and buildable structures, garrison creatures, dwellings, spells in mage guild.

As of writing (Oct 2026), the tool has been tested with Horn of The Abyss version 1.8.1 and Shadow of Death from gog.com

## Why
Heroes of Might and Magic III is one of the greatest games of all time, and still very popular and alive well in its 3rd decade, and under active development from fans.
I couldn't find an editor that supported all of the features I wanted, nor as a user, nor as a developer, so I wrote another one.
Siniskalc offers features to make further development easier and to even allow edits beyond the scope of the tool, with an HEX view of the entire file and of modelled entities, and the ability to export and import the entire savegame as a binary file.

## Who is this for
Players, map creators and tinkerers in general.

## How
>**Always keep a backup of your savegames.** This is a hobby tool that patches bytes of a file format nobody documented officially.
> While it does take precautions to avoid damaging your saves (for example by only patching the bytes that have changed, and keeping a backup of the save before each save), it is likely still possible to break a save if you make invalid changes.
> The original save file is not modified until you click **Save**; and the editor keeps a copy of the save file before it is replaced (same folder, `.bak` extension; later saves add a counter: `.bak1`, `.bak2`, ... and never overwrite an earlier copy). 
> This can be turned off under **Settings > Back up on save**.

### Download and run

Download the latest zip from the Releases section of the repository, unzip it anywhere and run `Siniskalc.exe`.
No installation needed, but it requires the [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0) (x64).

## Features

The left-hand tree shows the **game**, then one node per **player**, each with a *Heroes* group and a *Towns* group.

**Game overview**
* Map and game setup information read from the save (difficulty, game date, player control, and so on).

**Players**
* Edit the seven resources: wood, mercury, ore, sulfur, crystal, gems and gold.
* Tavern heroes, and which obelisks each colour has visited.

**Heroes**
* Primary skills (attack, defense, spell power, knowledge), with the bonus from equipped artifacts shown.
* Level, experience, class, gender, spell points, movement points, position on the map and facing.
* Secondary skills and skill levels (HotA skills included).
* Spell book, including the spells granted by artifacts.
* Army: creature type and count for each of the seven slots.
* War machines: ballista, ammo cart, first aid tent and, in HotA, the cannon.
* Equipped artifacts and the backpack, with effect descriptions.
* Biography.

**Towns**
* Faction, owner, position, mage guild level, garrison creatures.
* Built and buildable structures, with their requirements.
* Creatures waiting in the dwellings, upgraded or not.

**Editing**
* Pending-changes list showing every byte-level change before saving, undo, and revert all.
* Recent files menu.
* Backup of the savegame before each save (on by default, numbered, see **Settings**).
* Advanced mode (**Settings > Advanced mode**): a raw hex editor page for anything the editor does not model yet, and the Advanced menu to export the save bytes to a file and load them back, useful for diffing saves.

New to the editor?
Read the [user guide](docs/USER_GUIDE.md); it is also behind **Help > User guide** (F1).

See [FUTURE_IMPROVEMENTS.md](docs/FUTURE_IMPROVEMENTS.md) for what is planned.

## How it works

The editing model is layered: a save is split into chunks (heroes, players, castles, ...), each deserialised into an immutable model.
Edits go through an *edit session* that records pending changes against the original bytes, and only those bytes are patched on save, so unknown parts of the file are left untouched.

See `CLAUDE.md` for the repo rules and `docs/SINISKALC_ARCHITECTURE.md` for how the view models, models and edit sessions fit together.

## Building

Requires the .NET 8 SDK on Windows.

```
dotnet build Siniskalc.sln
dotnet test
```

## Contributing

Bug reports, savegames that break the editor and pull requests are welcome. Open an [issue](https://github.com/dimif/Siniskalc/issues) first for anything big.

* **Read first**: [`CLAUDE.md`](CLAUDE.md) has the repo rules (one type per file, immutable models, no byte chunks in view models, limits in `Constants.cs`, naming of tests).
  [`docs/SINISKALC_ARCHITECTURE.md`](docs/SINISKALC_ARCHITECTURE.md) shows how the projects, edit sessions, models and view models fit together.
* **Where to start**: [`docs/FUTURE_IMPROVEMENTS.md`](docs/FUTURE_IMPROVEMENTS.md) lists open work and the saves that would settle open questions.
* **Finding a new field in a savefile**: see "Finding a new field" in `docs/FUTURE_IMPROVEMENTS.md`, [`docs/LOCATING_HOMM3_ENTITIES.md`](docs/LOCATING_HOMM3_ENTITIES.md) for how things are located, and
  [`docs/KNOWLEDGE_BASE.md`](docs/KNOWLEDGE_BASE.md) for what is already known. Record new findings there.
* **Test savegames**: name and place them by [`TestData/README.md`](TestData/README.md) and add a row to the README of their folder.
* **Errors and logging**: see [`docs/ERROR_MODEL.md`](docs/ERROR_MODEL.md).
* **Before opening a PR**: run the whole suite (`dotnet test`); if the editor is running and locks the build output, add `--artifacts-path <another folder>`. Note user-visible changes in [`CHANGELOG.md`](CHANGELOG.md).

## Credits and acknowledgements

* **[cysun/Heroes3Editor](https://github.com/cysun/Heroes3Editor)** by Chengyu Sun, the original HoMM3 savegame editor in WPF, which inspired this project and showed how to unzip the savegame files.
  This project began as a fork to add additional fixes and features (initially hero movement, position, marker, level, spellpoints, combination artifacts and more), but has since been virtually entirely rewritten.
* **[shuvava/Heroes3Editor](https://github.com/shuvava/Heroes3Editor)**, a fork of the cysun project that added the Factory and Bulwark data.
* **[h3sed](https://github.com/suurjaak/h3sed)** by Erki Suurjaak, a Python savegame editor (MIT).
  For the Hero locating regex, the offsets for combination artifacts, and anything to do with Heroes Chronicles.
* [TacticDevGit](https://github.com/TacticDevGit/WPF-Sidebar-Navigation) for the sidebar navigation idea.
* [SharpZipLib](https://github.com/icsharpcode/SharpZipLib) (MIT), of which a trimmed and patched copy is used for the GZip code.
* [Editing heroes in memory](http://heroescommunity.com/viewthread.php3?TID=18817) on Heroes Community, and [HoMM3: Hex editing issue](https://www.gog.com/forum/heroes_of_might_and_magic_series/homm3_hex_editing_issue) on the GOG forum, for early savefile findings.
* [HoMM3 Wiki](https://heroes.thelazy.net/index.php/Main_Page) for game information.
  The creature AI and Fight values come from its [AI/Fight Value](https://heroes.thelazy.net/index.php/AI/Fight_Value) page, last fetched on 2026-10-09.
* [Visual Studio Image Library](https://www.microsoft.com/en-us/download/details.aspx?id=35825) (Visual Studio 2026 edition) for the menu and toolbar icons in `Siniskalc/Resources/VS2026`.
  These are © Microsoft and are **not** covered by this project's MIT license; they are used under the Microsoft Visual Studio Image Library license (see [Third-party notices](#third-party-notices)).
  And [Exhumed](http://www.iconarchive.com/artist/3xhumed.html) for the original application icon.
* [Claude](https://claude.com/claude-code) by Anthropic, for the hilarious app.icon and a lot of the heavy lifting in the UI, parsing of towns/player data and writing tests.

Heroes of Might and Magic is a trademark of Ubisoft.
This project is not affiliated with or endorsed by Ubisoft.

## License

Released under the [MIT License](LICENSE), with the exception of the Microsoft icons described below.

### Third-party notices

The icons in [`Siniskalc/Resources/VS2026`](Siniskalc/Resources/VS2026) come from the Microsoft Visual Studio 2026 Image Library and remain the property of Microsoft.
They are licensed under the [Visual Studio Image Library EULA](Siniskalc/Resources/VS2026/Visual%20Studio%20Image%20Library%20EULA.rtf), not the MIT license.
They may only be used in applications developed with the Visual Studio family of products, and anyone redistributing this application, or the icons in it, must agree to terms that protect the images at least as much as that EULA.
If you fork this project, do not reuse the icons outside the terms of that license.
