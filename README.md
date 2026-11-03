# Siniskalc

## What

A Windows desktop savegame editor for **Heroes of Might and Magic III** and **Heroes of Might and Magic II**.
For HoMM3 it supports the following versions/expansions: 
  - Base, AKA: Restoration of Erathia
  - Armageddon's Blade
  - Shadow of Death 
  - and Horn of the Abyss (HotA) (v1.8.1)
  - Heroes Chronicles. *Do note that this is entirely speculative and extracted from the work of Erki Suurjaak on h3sed, as I do not own a copy of the game, and only for a limited subset of entities (heroes and towns).*

For HoMM2:
  - Base, AKA: The Succession Wars
  - The Price of Loyalty.

All the common file types used by the 2 titles are supported, for single player and hotseat sessions: `.GM1`, `.GM*`, `.GX1`, `.GX*`, `.CGM` etc.

Saves have been tested with Horn of The Abyss version 1.8.1, HoMM3 Complete and HoMM2 Complete from gog.com

## Features

Siniskalc allows you to many of the entities modelled in HoMM2 and HoMM3 savegame. They are all modelled in an easy-to-navigate UI that tracks all the pending changes.

The left-hand tree shows the **game**, then one node per **player**, each with a *Heroes* group and a *Towns* group.
A non extensive list of entities extracted from the save files:

**Game overview**
* Map and game setup information read from the save (difficulty, game date, player control, and so on).
* Whose turn it is (hotseat games only).

**Players**
* Edit the seven resources: wood, mercury, ore, sulfur, crystal, gems and gold.
* Tavern heroes, and which obelisks each colour has visited.
* Banish countdown (how many turns before players that have lost their last castle are kicked from the game).

**Heroes**
* Primary skills (attack, defense, spell power, knowledge), with the bonus from equipped artifacts shown.
* Level, experience, class, gender, spell points, movement points, position on the map and facing.
* Secondary skills and skill levels (HotA skills included).
* Spell book, including the spells granted by artifacts.
* Army: creature type and count for each of the seven slots.
* War machines: ballista, ammo cart, first aid tent and, in HotA, the cannon.
* Equipped artifacts and the backpack, with effect descriptions.
* Visited buildings.

**Towns**
* Faction, owner, position, mage guild level, garrison creatures.
* Built and buildable structures, with their requirements.
* Creatures waiting in the dwellings, upgraded or not.

**Map objects**
* Mines, dwellings.

**Editing**
* Pending-changes list showing every byte-level change before saving, undo, and revert all.
* Recent files menu.
* Backup of the savegame before each save (on by default, numbered, see **Settings**).
* Advanced mode (**Settings... > Tools > Advanced mode**): a raw hex editor page for anything the editor does not model yet, and the Advanced menu to export the save bytes to a file and load them back, useful for diffing saves.

New to the editor?
Read the [user guide](docs/USER_GUIDE.md); it is also behind **Help > User guide** (F1).

See [FUTURE_IMPROVEMENTS.md](docs/FUTURE_IMPROVEMENTS.md) for what is planned.

## Download and run

Download the latest zip from the Releases section of the repository, unzip it anywhere and run `Siniskalc.exe`.
Keep the shipped `GameData` folders beside the executable; they contain the entity definitions required at startup.
No installation needed, but it requires the [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0) (x64).

## Warning

>**Always keep a backup of your savegames.** This is a hobbist tool that patches bytes of reverse-engineerd file formats.
> While it does take precautions to avoid damaging your saves (for example by only patching the bytes that have changed, and keeping a backup of the save before each save), do not blindly trust it.

> The original save file is not modified until you click **Save**; and the editor keeps a copy of the save file before it is replaced (same folder, `.bak` extension; later saves add a counter: `.bak1`, `.bak2`, ... and never overwrite an earlier copy). 

> This can be turned off under **Settings... > Saving > Back up on save**.

### Game entity definitions

Siniskalc makes modelling of additional in-game entities not known by the editor possible: standard game entities are supplied as JSON in `GameData\Homm2` and `GameData\Homm3` beside the application (Keep these folders with the executable when distributing or moving it!). 

Definitions are loaded once at startup: after editing them, **restart the editor** to apply changes. Missing or invalid definitions stop startup with an error identifying the file; restore or correct it rather than expecting a hardcoded fallback.

Adding entities, such as a faction with its creature, should in theory require only amending the JSON files. Do note that you must know the matching byte codes for the entities, which you should be able to use Sinikalc's built in HEX viewer to ascertain.

See [Standard game definitions](docs/SINISKALC_ARCHITECTURE.md#standard-game-definitions) for the architecture and boundaries.
See [Editing game definitions](docs/GAME_DEFINITIONS.md) for file formats, version inheritance and adding a faction.

## Troubleshooting

* **Logs**: the editor writes a daily log file, `siniskalc-<date>.log`, to `%LOCALAPPDATA%\Siniskalc\logs` (the last 7 days are kept). **Help > Show logs** opens the folder. **Settings... > Logging** changes the folder and the level; choose Debug or Trace for more detail before reproducing a problem.
* **Settings**: `settings.json` and `serilog.json` (logging configuration) are in `%APPDATA%\Siniskalc`. If the editor reports that one of them cannot be read at startup, it offers to put it aside as `.bad` and write a fresh default.
* **Startup error naming a file in `GameData`**: the entity definitions are missing or invalid. Keep the `GameData` folders beside `Siniskalc.exe`, or restore the file named in the message.
* **Won't start at all**: check that the [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0) (x64) is installed.
* **A save that fails to load or looks wrong**: your save is not modified until you click **Save**, and a `.bak` copy is kept beside it. When reporting it, attach the log file and, if you can, the savegame in an [issue](https://github.com/dimif/Siniskalc/issues).

## Who is this for

Players, map creators and tinkerers in general.

## Why

I am very fond of Heroes of Might and Magic III and II; III in particular is widely regarded as of the greatest games of all time, and is still very popular its 3rd decade, and under active development from fans.

I couldn't find an editor that supported all of the features I wanted, nor as a user, nor as a developer, so I wrote one.

Siniskalc offers features to make further development easier and to even allow edits beyond the scope of the tool, with an HEX view of the entire file and of modelled entities, and the ability to export and import the entire savegame as a binary file.

## Contributor info: How it works

The editing model is layered: a save is split into chunks (heroes, players, castles, ...), each deserialised into an immutable model.
Edits go through an *edit session* that records pending changes against the original bytes, and only those bytes are patched on save, so unknown parts of the file are left untouched.

See `CLAUDE.md` for the repo rules and `docs/SINISKALC_ARCHITECTURE.md` for how the view models, models and edit sessions fit together.

## Building

Requires the .NET 8 SDK on Windows.

```
dotnet build Siniskalc.sln
dotnet test Siniskalc.sln --artifacts-path .artifacts
```

## Contributing

Bug reports, savegames that break the editor and pull requests are welcome. Open an [issue](https://github.com/dimif/Siniskalc/issues) first for anything big.

* **Read first**: [`CLAUDE.md`](CLAUDE.md) has the repo rules (one type per file, immutable models, no byte chunks in view models, limits in `Constants.cs`, naming of tests).
  [`docs/SINISKALC_ARCHITECTURE.md`](docs/SINISKALC_ARCHITECTURE.md) shows how the projects, edit sessions, models and view models fit together.
* **Where to start**: [`docs/FUTURE_IMPROVEMENTS.md`](docs/FUTURE_IMPROVEMENTS.md) lists open work and the saves that would settle open questions.
* **Finding a new field in a savefile**: see the "Probes" section of `docs/FUTURE_IMPROVEMENTS.md`, [`docs/LOCATING_ENTITIES.md`](docs/LOCATING_ENTITIES.md) for how things are located, and
  [`docs/KNOWLEDGE_BASE.md`](docs/KNOWLEDGE_BASE.md) for what is already known. Record new findings there.
* **Test savegames**: name and place them by [`TestData/README.md`](TestData/README.md) and add a row to the README of their folder.
* **Errors and logging**: see [`docs/ERROR_MODEL.md`](docs/ERROR_MODEL.md) and [`docs/LOGGING.md`](docs/LOGGING.md) (the logger, the levels, `serilog.json`).
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
* [Visual Studio Image Library](https://www.microsoft.com/en-us/download/details.aspx?id=35825) (Visual Studio 2026 edition) for the menu and toolbar icons in `Siniskalc.Wpf/Resources/VS2026`.
  These are © Microsoft and are **not** covered by this project's MIT license; they are used under the Microsoft Visual Studio Image Library license (see [Third-party notices](#third-party-notices)).
  And [Exhumed](http://www.iconarchive.com/artist/3xhumed.html) for the original application icon.
* [Claude](https://claude.com/claude-code) by Anthropic, for the hilarious app.icon and a lot of the heavy lifting in the UI, parsing of towns/player data and writing tests.

Heroes of Might and Magic is a trademark of Ubisoft.
This project is not affiliated with or endorsed by Ubisoft.

## License

Released under the [MIT License](LICENSE), with the exception of the Microsoft icons described below.

### Third-party notices

The icons in [`Siniskalc.Wpf/Resources/VS2026`](Siniskalc.Wpf/Resources/VS2026) come from the Microsoft Visual Studio 2026 Image Library and remain the property of Microsoft.
They are licensed under the [Visual Studio Image Library EULA](Siniskalc.Wpf/Resources/VS2026/Visual%20Studio%20Image%20Library%20EULA.rtf), not the MIT license.
They may only be used in applications developed with the Visual Studio family of products, and anyone redistributing this application, or the icons in it, must agree to terms that protect the images at least as much as that EULA.
If you fork this project, do not reuse the icons outside the terms of that license.
