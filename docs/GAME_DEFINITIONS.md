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
HoMM3 war machines are separate from artifacts: Catapult is not an artifact definition, and the editable Ballista, Ammo Cart, First Aid Tent and Cannon live in `warMachines.json`.
HoMM2's Ammo Cart and Ballista of Quickness remain artifacts, as they are in that game.

An entity's `id` is a stable, case-sensitive reference, not its name, list position or save code.
Keep existing IDs when changing wording. JSON array order controls presentation; there is no `order` property. `code` is a format-specific binary value.
Do not reuse codes where a domain requires uniqueness.
HoMM3 building codes are contextual to a faction, not globally unique.
Do not infer mechanics from the display name: role and policy fields express the supported behaviour.
Only exceptional roles need to be written. Artifacts use `spellBook` or `scroll` for their special encoding; ordinary artifacts omit `role`.
HoMM3 skills may specify `morale`, `luck` or `spellPointMultiplier` to connect them to those mechanics; other skills omit it. HoMM2 skills have no role property.
Fallback selection is editor business logic, never an entity property: unknown required entities use the entity with code 0, independent of order or display name.
An unknown owner or faction still uses Neutral where the model represents no known affiliation, and a genuinely absent value remains absent.
The well-known `Neutral` faction ID identifies the only neutral faction. There is no `neutral`/`isNeutral` flag, and renaming its display text does not change its behavior.
Likewise, HoMM3's well-known `all` spell-school ID identifies the universal school; there is no authored `universal` flag.
The `Ranged` ability stands alone in the ability list; no ability has an authored `isRanged` flag.

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
    "sw": "standard",
    "pol": "standard",
    "unknown": "standard"
  },
  "sets": {
    "standard": { "parent": null }
  }
}
```

The named set is its directory. Each set contains `abilities.json`, `artifacts.json`, `buildings.json`, `creatures.json`, `factions.json`, `spellTypes.json`, `mapObjects.json`, `mines.json`, `visitablePlaces.json`, `skills.json`, `spells.json` and `visitEffects.json`.
`spellTypes.json` names the existing spell categories (such as Mind and Elemental), referenced by a spell's `group`; the separate `type` field still distinguishes combat from adventure spells.
`schema.json` describes the domain shapes; runtime validation also checks references, encoding limits and consistency.
Version identifiers are `sw` (The Succession Wars) and `pol` (The Price of Loyalty).

Each domain has an envelope:

```json
{
  "entries": [],
  "replacements": []
}
```

`entries` adds new IDs. `replacements` supplies complete replacement records for inherited IDs, not partial property patches.
Inherited records retain their positions when replaced; new entries append in their JSON array order. HoMM2 creature upgrades must appear after their base creature.
A child set names its `parent` in the manifest and has every domain file; an empty envelope inherits that domain unchanged.
Sets are resolved before use, and versions sharing a set share its immutable snapshot.

A new playable faction record can look like this:

```json
{
  "id": "Observatory",
  "name": "Observatory",
  "code": 6,
  "library": true,
  "growthBuildingName": "Astral Garden",
  "specialBuildingName": "Star Library",
  "specialBuildingDescription": "Teaches one additional spell at each built Mage Guild level.",
  "heroIds": [0],
  "guildSpellCounts": [3, 3, 2, 2, 1]
}
```

Put it in `factions.json`'s `entries`, choose an unused representable code, and assign hero IDs explicitly.
In this example hero 0 must first be removed from its old faction's mapping using a complete replacement record; mappings cannot overlap.
Add related creatures, abilities and other content with references to `Observatory`, using the game's existing dwelling slots and encoding policies.
Library capability, guild counts and faction-specific building names are data, not branches on faction identity.
The complete JSON-only example is in the HoMM2 test project's `TestDefinitions` fixtures and `JsonFactionTests`.

## HoMM3

`GameData\Homm3\manifest.json` explicitly maps every supported version to the same `standard` directory.
The JSON version identifiers are `roe`, `ab`, `sod`, `chronicles` and `hota`.
Set entries give both a directory and an optional parent:

```json
{
  "standard": { "parent": null, "directory": "standard" }
}
```

These are entries in the manifest's `sets` object, not a standalone manifest.
The directory contains `factions`, `abilities`, `buildings`, `creatures`, `spellSchools`, `spells`, `heroClasses`, `secondarySkills`, `warMachines`, `artifacts` and `visitablePlaces` JSON files.
Map metadata lives in the selected set alongside the other definitions, not at the game data root.
The `schemas` directory contains the per-file JSON schemas.

Each domain file is an array. A child array adds records; `"replaces": true` on a record replaces the entire inherited record with that ID.
An empty child array inherits the domain unchanged. Duplicate local IDs and replacements of nonexistent IDs are errors.
Replacements retain the inherited position, and additions append in array order. Shipped definitions need no child set.
`availableFrom` controls availability in the supported versions; manifests still explicitly select the standard set.
There is no shipped `hota` data directory. Creature stat differences are in an optional `versionInfo` object on the creature: for example, `"versionInfo": { "hota": { ...complete stats... } }`.
The creature's `info` remains its normal stats, so the Faerie Dragon keeps its original AI value in older versions and its changed value in HotA.
Version overrides select exact supported versions; they do not imply that every later mod inherits them.
Code lookup retains definitions needed for historical reading even when they are not offered in a version's pickers.
An unknown save version keeps its historical restricted picker behaviour; it does not select a user-defined pack.

A HotA-only faction addition in the standard faction array can be:

```json
{
  "id": "FixtureTown",
  "name": "Astral Town",
  "code": 12,
  "availableFrom": "hota",
  "companionBits": "fort"
}
```

This is the record used by `Tests\Siniskalc.Homm3.Tests\CatalogData\Synthetic`.
Its related files add a creature, buildings, hero class, spell school, artifact effects and war machine without a production C# identity.
Use unused representable codes and add all referenced records.
Building definitions express prerequisites, upgrades and automatic buildings; a faction's companion-bit policy is one of `none`, `halls`, `fort` or `fortAndHalls`.
These policies reserve encoding bits, so their required trigger buildings and collision rules must be satisfied.

`wearable` and `backpack` default to `true`; specify them only for exceptions, such as the spell book.
Spells default `heroOnly` to `false`; specify `true` for the exception.
Map-object metadata can name and parameterize supported storage kinds; adding a new physical map-object encoding still requires Data-layer code.

## Map-building organization in both games

Both `standard` directories use the same domain names:

| File | Purpose |
|---|---|
| `mines.json` | Mine names and the resources they produce; HoMM3 also supplies daily production. |
| `visitablePlaces.json` | Permanent visits remembered separately for individual places, with `name` and `description`. |
| `visitEffects.json` | Visits remembered through hero flags, with `name`, `description`, morale and luck parameters. |
| `mapObjects.json` | Presentation metadata for other supported map objects. |

Castle buildings remain in `buildings.json`; they are not adventure-map buildings.
The files share responsibilities and camelCase naming, not an artificial common record type.
HoMM2 retains its `schemaVersion`/`entries`/`replacements` envelopes and its `kind`, `flag` and visit `behaviour` fields.
HoMM3 retains arrays and its own storage slots, object types and editable flags; `mapObjects.json` holds its merchant and dwelling labels, while HoMM2 has per-role name formats for shrines and sphinxes.
HoMM3's mine and visit-effect arrays describe complete fixed sets of supported encodings: a nonempty child array replaces the whole metadata domain, and an empty child array inherits it. Each set supplies its own map-object labels.
These differences reflect what each game stores and what the editor can currently read, rather than imposing unused fields on the other game.

## Artifact effects

Both games keep effects on the artifact rather than in a separate entity table. The shape is sparse:

```json
{
  "effectInfo": {
    "modifiers": [
      { "attribute": "attack", "amount": 3 },
      { "attribute": "knowledge", "amount": 2 }
    ]
  }
}
```

Omitted attributes contribute zero. Each attribute can appear at most once.
HoMM2 currently implements `attack`, `defense`, `spellPower` and `knowledge`; other effects require a verified implementation, not just another JSON entry.
HoMM3 additionally supports `luck`, `morale`, `landMovement`, `waterMovement`, `gold`, `ore`, `wood`, `crystal`, `gems`, `mercury` and `sulfur`.
HoMM3 `effectInfo` may also contain `spellGrants` (`named`, `school` or `level`, with corresponding parameters and descriptions), `grantsSpellBook` and `other` for the existing behavior and descriptive text.
Empty modifier/grant lists, false `grantsSpellBook` and empty `other` can be omitted. HoMM2 artifacts with no modifiers can omit `effectInfo` entirely.

## Errors and contributor checks

The shared reader rejects unknown properties, duplicate JSON properties, numeric enum values, null roots and paths outside the game definition root.
The game loaders check schema versions, explicit version mappings, inheritance cycles, required files, IDs, codes, references, roles, effect parameters and format capacities before exposing catalogs.
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
