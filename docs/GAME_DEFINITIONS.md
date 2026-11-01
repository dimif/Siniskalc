# Editing game definitions

The editor ships standard definitions for its supported HoMM2 and HoMM3 versions.
Source files live in `Siniskalc.Homm2\GameData` and `Siniskalc.Homm3\GameData`; builds and publishes copy them to `GameData\Homm2` and `GameData\Homm3` beside the executable.
Distribute the whole directories, including manifests and schemas.

The module factories load and validate all standard sets once at startup.
Documents, readers, independent safe serializers, reducers, finders and pages use immutable typed catalogs from that snapshot.
**Restart the editor after editing deployed JSON.** Reopening a save does not reload definitions.
There are no custom profiles, override folders, live reload or in-app definition editor.

## What belongs here

Definitions supply creatures, abilities, artifacts and supported effects, factions, castle buildings, supported map-object metadata, spells, hero classes or hero/faction mappings, secondary skills, spell schools or groups, and war machines where the game has them.
Savegame instances (a particular hero, town or mine) still come from the savefile.
Player colours and genuinely fixed encoding enums remain code.

An entity's `id` is a stable, case-sensitive reference, not its name, list position or save code.
Keep existing IDs when changing wording. `order` controls presentation; `code` is a format-specific binary value.
Do not reuse codes or orders where a domain requires uniqueness.
HoMM3 building codes are contextual to a faction, not globally unique.
Do not infer mechanics from the display name: role and policy fields express the supported behaviour.

Definitions are data, not scripts. They can choose existing effect implementations, their parameters, names and descriptions.
They cannot introduce new binary layouts, enlarge fixed tables or masks, create a new effect implementation, or make the game executable understand new content.
For example, HoMM2 still has 54 hero records, 14 secondary skill slots, 65 spell codes and five guild levels with four slots per level.
HoMM3 spell and skill codes must fit their existing bitsets, and each faction's buildings must fit its existing mask.
The loaders reject unrepresentable or ambiguous definitions rather than silently truncating them.

## HoMM2

`GameData\Homm2\manifest.json` has schema version 1:

```json
{
  "schemaVersion": 1,
  "versions": {
    "BaseGame": "standard",
    "PriceOfLoyalty": "standard",
    "Unknown": "standard"
  },
  "sets": {
    "standard": { "parent": null }
  }
}
```

The named set is its directory. Each set contains `abilities.json`, `artifacts.json`, `buildings.json`, `creatures.json`, `effects.json`, `factions.json`, `groups.json`, `map-objects.json`, `mines.json`, `places.json`, `skills.json`, `spells.json` and `visit-effects.json`.
`schema.json` describes the domain shapes; runtime validation also checks references, encoding limits and consistency.

Each domain has an envelope:

```json
{
  "entries": [],
  "replacements": []
}
```

`entries` adds new IDs. `replacements` supplies complete replacement records for inherited IDs, not partial property patches.
A child set names its `parent` in the manifest and has every domain file; an empty envelope inherits that domain unchanged.
Sets are resolved before use, and versions sharing a set share its immutable snapshot.

A new playable faction record can look like this:

```json
{
  "id": "Observatory",
  "name": "Observatory",
  "code": 6,
  "order": 7,
  "neutral": false,
  "library": true,
  "growthBuildingName": "Astral Garden",
  "specialBuildingName": "Star Library",
  "specialBuildingDescription": "Teaches one additional spell at each built Mage Guild level.",
  "heroIds": [0],
  "heroFallback": false,
  "guildSpellCounts": [3, 3, 2, 2, 1]
}
```

Put it in `factions.json`'s `entries`, choose unused representable codes and orders, and assign hero IDs explicitly.
In this example hero 0 must first be removed from its old faction's mapping using a complete replacement record; mappings cannot overlap.
Add related creatures, abilities and other content with references to `Observatory`, using the game's existing dwelling slots and encoding policies.
Library capability, guild counts and faction-specific building names are data, not branches on faction identity.
The complete JSON-only example is in the HoMM2 test project's `CatalogData` fixtures and `JsonFactionTests`.

## HoMM3

`GameData\Homm3\manifest.json` explicitly maps Restoration of Erathia, Armageddon's Blade, Shadow of Death and Heroes Chronicles to `standard`, and Horn of the Abyss to `hota`.
Set entries give both a directory and an optional parent:

```json
{
  "standard": { "parent": null, "directory": "standard" },
  "hota": { "parent": "standard", "directory": "hota" }
}
```

These are entries in the manifest's `sets` object, not a standalone manifest.
Each directory contains `factions`, `abilities`, `buildings`, `creatures`, `spellSchools`, `spells`, `heroClasses`, `secondarySkills`, `warMachines`, `artifacts` and `visitedPlaces` JSON files.
`mapObjects.json` is shared metadata at the game data root, outside the sets.
The `schemas` directory contains the per-file JSON schemas.

Each domain file is an array. A child array adds records; `"replaces": true` on a record replaces the entire inherited record with that ID.
An empty child array inherits the domain unchanged. Duplicate local IDs and replacements of nonexistent IDs are errors.
`availableFrom` controls availability in the supported versions; manifests still explicitly select the standard set.
Code lookup retains definitions needed for historical reading even when they are not offered in a version's pickers.
An unknown save version keeps its historical restricted picker behaviour; it does not select a user-defined pack.

A faction addition in the HotA array can be:

```json
{
  "id": "FixtureTown",
  "order": 13,
  "name": "Astral Town",
  "code": 12,
  "availableFrom": "hornOfTheAbyss",
  "isNeutral": false,
  "companionBits": "fort"
}
```

This is the record used by `Tests\Siniskalc.Homm3.Tests\CatalogData\Synthetic`.
Its related files add a creature, buildings, hero class, spell school, artifact effects and war machine without a production C# identity.
Use unused representable codes/orders and add all referenced records.
Building definitions express prerequisites, upgrades and automatic buildings; a faction's companion-bit policy is one of `none`, `halls`, `fort` or `fortAndHalls`.
These policies reserve encoding bits, so their required trigger buildings and collision rules must be satisfied.

Artifact effects are nested in artifact records.
Spell grants select the existing `named`, `school` or `level` behaviour with the corresponding parameters and descriptions.
Special entities such as the spell book, scroll and unknown-value stand-ins are identified by validated roles, not names or array positions.
Map-object metadata can name and parameterize supported storage kinds; adding a new physical map-object encoding still requires Data-layer code.

## Errors and contributor checks

The shared reader rejects unknown properties, duplicate JSON properties, numeric enum values, null roots and paths outside the game definition root.
The game loaders check schema versions, explicit version mappings, inheritance cycles, required files, IDs, codes, orders, references, roles, effect parameters and format capacities before exposing catalogs.
Invalid or missing standard data stops application startup with `GameDefinitionException`; the message identifies the file and cause.
Correct or restore the named file and restart. There is no hardcoded fallback catalog.
JSON schemas aid editing; the runtime typed reader and semantic validators are authoritative.

This is separate from an unknown byte in a savefile: that still reads safely as the catalog's stand-in, and a safe serializer preserves the original byte until an edit disagrees.

For a definition change, run the game's catalog validation, parity and JSON-only faction tests first.
The fixtures exercise discovery, pages, exact save bytes, undo/redo and reload; parity baselines are independent of the runtime definitions.
Update a parity baseline only after independently establishing why the expected data changed.
Run `dotnet test Siniskalc.sln --artifacts-path <another folder>` once for the completed batch.
Publish the application and check that both `GameData` trees are present without an extra nested `GameData` directory.
The application integration tests also verify that opening files does not read definitions again and that missing definitions prevent construction.
