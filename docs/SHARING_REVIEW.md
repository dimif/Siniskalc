# What can move into Siniskalc.Heroes: review of `Entities` and `Models`

A review, component by component, of `Siniskalc.Homm2` and `Siniskalc.Homm3` (`Entities` and `Models`), for what could be one abstraction in `Siniskalc.Heroes` (or `Siniskalc.Core` when it knows no game at all).
Each item has a verdict, what it would look like, and the reasons, so it can be accepted, changed or refused one at a time.
The work was done in the order at the end; **"Status after the work"** below says what was done, what was not and why, and what the assessment looks like now. The sections further down are the review as it was written, kept for the reasoning.

Read from the working tree of 2026-10-16, which already has the work of `6f06ab7`, `aeb17a6` and what is not committed yet.
What that work moved into `Heroes` is listed under "Already shared" and is not proposed again.

**Verdicts**

- **SHARE**: one type in `Heroes` (or Core) that both games use.
- **GENERIC**: one generic type or helper that each game instantiates with its own types (as `PlayerColors<TColor>` and `OwnerNames<TColor>` already do).
- **INTERFACE**: the games keep their types, and implement an interface a shared consumer needs.
- **KEEP**: stays in the game, with the reason.

**What counted as a reason to share:** the two types hold the same data with the same meaning, or a shared consumer (a page block, the navigation, a view model base) would be simpler with one type.
**What counted as a reason to keep:** the games differ in what they store, the rule is the game's own, or no shared code would ever use the shared type.

## Already shared (not proposed again)

`Resource`, `GameDate`, `TavernHeroes`, `ChangeAreas`, `ChangeText`, `OwnerNames<TColor>`, `PlayerColors<TColor>`, `PlayerColorInfo`, `SkillLevelInfo`, `GameDifficulty`, `SpellType`, `CreatureInfo`, `ICreature`, `ICreatureAbility`, `IPlayerColor`, `IFaction`, `ISecondarySkill`, `ISkillLevel`, `ILink`, `IOwnerChoice`, `PlayerPalette`.

## Status after the work (2026-10-17)

Done, each in its own commit with the whole suite run after it:

| Group | Items | Commit |
|---|---|---|
| 1 | E1 `VersionedEnumeration<TVersion>` (Core), E2 `Factions<TFaction>`, M7 `ResourceSet`, M8 `Numbers` (Core), M15 `GameRuleException`, E10 `HeroLevel` and `MovementPoints` | `371facd` and the ones before it |
| 2 | M2 `PrimarySkills` and `PrimarySkillBonus`, E7 `IArtifact`, M6 (the HoMM2 bonus table moved into its `Artifact`) | `6758615` |
| 3 | M1 `Army<TCreature>` and `ArmySlot<TCreature>` | `0e77a03` |
| 4 | M9 `PlayerModelBase<TColor>`, M11 `HeroSummary<TColor>`, M14 `ExperienceTable` | `fae02c8` |
| 5 | M4 `SpellSet<TSpell>` | `e488f67` |
| 6 | E9 `MapPosition` | `da9bb07` |
| 7 | M13 `MapHeader` | next commit after `62252ce` |

**Not done, and why**

- **E3 (skill level) and M3 (secondary skills):** left for later, as you decided. M3 needs E3.
- **M13 (`MapHeader`):** done once the HoMM2 header work landed (`MapHeader(Name, Description, Difficulty, Size, HasUnderground)` in `Heroes`; HoMM2 reads its square width as `Size`). `GameSetup` stays HoMM3's until the HoMM2 control masks are decoded (E12).
- **M12 (`MageGuild`, `CreatureStock`):** looked at again, and it is not the small step the review said. The two castles do not store the same thing in a way a value can hide: HoMM2 keeps the guild level inside the `Dwellings` flags and the guild spells as 15 spell numbers, HoMM3 a level field and lists of `Spell`; HoMM2's upgraded-creature numbers carry a marker for the upgrade state that HoMM3's do not. A shared `MageGuild<TSpell>` would first need HoMM2's castle to hold the guild as a value (a model change with its serializer, differ and tests), and its only reader would be the two castle pages. Do it together with the shared mage guild view model, as the review said, not before. No new decoding is needed for it.
- **E5 (`ISpell`):** still waits for a consumer (a shared spell row).

**What the assessment looks like now**

- **Hero.** After E10, M1, M2, M4 and M14 the two hero models hold the same *parts* (level, movement, primary skills, army, spell set). What is left that differs is secondary skills (M3, deferred) and the glue (artifact rules, class, backpack, war machines, flags). M10 stays KEEP; the next step is a hero page interface (`IHeroPage`, a view-model question) and, after E3 and M3, the secondary skills page.
- **Player and map.** The player is now a shared base plus each game's lists. What remains different is real: HoMM2 keeps lists of hero and castle numbers and an income, HoMM3 does not. `GameSetup` (who controls each colour, whose turn) stays HoMM3's until the HoMM2 header masks are decoded (test 2 in `HOMM2_PLAN.md`); then `PlayerType` and `GameSetup` move together (E12 and M13).
- **M17, obelisks: revised.** The review said `ObeliskVisits` had no HoMM2 counterpart. HoMM2 has obelisks that work like HoMM3's, and no teams. The model is already free of teams: it is a byte per obelisk with a bit per colour; "the whole team gets the bit" is done by the game when it writes and is only described in a comment. So `ObeliskVisits` can become `ObeliskVisits<TColor>` in `Heroes` (its positions are `MapPosition?` now), together with the obelisk view models. The blocker is decoding, not design: test 3 in `HOMM2_PLAN.md`. For HoMM2 the "team" case is simply not offered. M17 is therefore no longer KEEP for the obelisks; granted spells, the castle builder and `PrimarySkillBonus` stay as they were (the last moved with M2).
- **Entities.** E4 (secondary skills), E8 (buildings), E11 (HoMM3 hero vocabulary), M5 (artifacts and backpack), M16 (spell points rule): unchanged, for the reasons given. E11 has one new note: `FacingDirection` moves first when the HoMM2 facing byte is decoded (test 4).
- **Pattern that held.** Every shared type that came out of this was either a value both games store in the same way (`ResourceSet`, `MapPosition`, `PrimarySkills`), or a generic over the game's own entity (`Army<TCreature>`, `SpellSet<TSpell>`, `HeroSummary<TColor>`, `PlayerModelBase<TColor>`), or a mechanism with the game's data passed in (`ExperienceTable`, `Factions<TFaction>`). What did not move is exactly what each game's rules own.
- **Small leftovers.** `NameOf(code)` in every entity class (E6) is still six one-liners; the HoMM3 `Position` properties on the view models (`PositionX`, `PositionY`) are `short` for the steppers and clamp the model's `int`.

## Summary

| # | Component | HoMM2 | HoMM3 | Verdict | Size |
|---|---|---|---|---|---|
| E1 | Enumeration base and game version | `Homm2Enumeration`, `GameVersion` | `Homm3Enumeration`, `GameVersion` | GENERIC (Core) | small |
| E2 | Faction | enum + `FactionInfo` + `Factions` | class | GENERIC catalog | small |
| E3 | Skill level | enum with `None` + `SkillLevels` | enum without `None` + `SkillLevels` | GENERIC catalog; one enum is an option | small / medium |
| E4 | Secondary skill | class, 14 | class, 30 | KEEP | none |
| E5 | Spell | class, cost | class, school, level | INTERFACE (`ISpell`) | small |
| E6 | Creature, ability | done | done | KEEP the rest | none |
| E7 | Artifact | name and code | name, code, slot, class, cost, effects | INTERFACE (`IArtifact` + bonus) | small |
| E8 | Buildings and dwellings | flags | building graph | KEEP | none |
| E9 | Position | struct (x, y) | record (x, y, z), `HeroPosition` | SHARE (`MapPosition`) | medium |
| E10 | Hero level and movement | four loose numbers | `HeroLevel`, `HeroMovementPoints` | SHARE | small |
| E11 | Hero vocabulary of HoMM3 | none | class, gender, facing, war machines, ... | KEEP | none |
| E12 | Player type | none | `PlayerType` | KEEP (revisit) | none |
| M1 | Army | fixed array of slots | dictionary of slots | GENERIC (`Army<TCreature>`) | medium |
| M2 | Primary skills | `PrimarySkills` | `HeroPrimarySkills`, `PrimarySkillBonus` | SHARE | medium |
| M3 | Secondary skills | ordered entries | skill map + slots + extras | GENERIC / SHARE | large |
| M4 | Spell book | set of learned spells | learned + granted + other | GENERIC (`SpellSet<TSpell>`) | small |
| M5 | Artifacts, backpack | 14 positional slots | slot flags, backpack | KEEP (share the bonus only) | none |
| M6 | Artifact bonus table | `ArtifactBonuses` | `ArtifactEffectInfo` | follows E7 | small |
| M7 | Resources | `Resources` (long) | `PlayerResources` (int) | SHARE (`ResourceSet`) | small |
| M8 | `Numbers` | value-equality int list | none (plain lists) | SHARE (Core) | small |
| M9 | Player | colour, resources, income, lists, days left, tavern | colour, resources, tavern, days left | GENERIC base | medium |
| M10 | Hero | flat record | class with reconcile | KEEP (share the parts) | none |
| M11 | Hero summary | none (in the navigation builder) | `HeroSummary` | SHARE | small |
| M12 | Castle | flags and numbers | building set | KEEP; share `MageGuild`, `CreatureStock` | medium |
| M13 | Map header, game setup | name, size | name, difficulty, size, underground | SHARE (`MapHeader`) | small |
| M14 | Experience table | `HeroLevels` | `LevelCalculator` | GENERIC (`ExperienceTable`) | small |
| M15 | `GameRuleException` | none | class | SHARE | trivial |
| M16 | Spell points rule | none | `SpellPointsCalculator` | KEEP | none |
| M17 | Granted spells, castle builder, obelisks | none | yes | KEEP | none |

The order I would do them in is at the end.

---

## Entities

### E1. Enumeration base and game version: GENERIC (Core)

**Now.** `Homm2Enumeration` and `Homm3Enumeration` are the same class line by line.
Each derives from `AbstractEnumeration`, takes a `GameVersion` of its own game, defaults it to the first real version (`BaseGame`, `RestorationOfErathia`), and exposes `GameVersion` and `IsAvailableIn(GameVersion)` by casting `AvailableFrom` (an `int` in Core) to the enum.
The two `GameVersion` enums are `Unknown` plus an ordered list of releases: they are the game's, and differ (`BaseGame`, `PriceOfLoyalty` against five HoMM3 releases).
`GameVersionExtensions.HasGameSetup` is HoMM3's.

**Proposal.** `VersionedEnumeration<TVersion> where TVersion : struct, Enum` in `Siniskalc.Core.Entities`, with the default version as a constructor argument.
`Homm2Enumeration : VersionedEnumeration<GameVersion>` and `Homm3Enumeration : VersionedEnumeration<GameVersion>` stay as the thin per-game names (they are in the rules of `CLAUDE.md` as the wrappers of generic Core types), so no entity changes.
Core is right because it needs no Heroes concept: a version is a game's own enum.

**Not shared:** the `GameVersion` enums. Their members are what a savefile of that game can be; a shared enum would list every game's releases.

### E2. Faction: GENERIC catalog

**Now.** HoMM3's `Faction` is a class (with `Neutral`, version tiers, `Conflux`, HotA's three). HoMM2's is an enum of six with `FactionInfo` and `Factions.Of` to give the view models an `IFaction`.
That is the same shape as the colour: an enum in the models and an info record for the interface.

**Proposal.** `Factions<TFaction> where TFaction : struct, Enum` in `Heroes.Entities` next to `PlayerColors<TColor>`, using `FactionInfo` (moved to Heroes like `PlayerColorInfo`). `Homm2.Entities.FactionInfo` and `Factions` go away.
HoMM3 needs nothing: its class implements `IFaction`.

**Option, not recommended now.** Make HoMM2's `Faction` a class like HoMM3's and drop the catalog. It removes the enum/catalog split, but `Faction` is used in about 85 places (switches, casts, `Enum.GetValues`, the hero's `HeroId / 9`), and the generic catalog costs nothing.

**Not shared:** the factions themselves. HoMM2's six towns do not map one to one to HoMM3's nine; a shared list would be a lie in one of them.

### E3. Skill level: GENERIC catalog; one enum is an option

**Now.** HoMM2: `SkillLevel { None, Basic, Advanced, Expert }`. HoMM3: `SkillLevel { Beginner, Intermediate, Expert }`, where "no level" is `null` and the serializer reads a zero byte as absent (`ByteEnum.TryRead`).
Each game has a `SkillLevels` class with the same three `SkillLevelInfo` records (rank 1 to 3, names "Basic", "Advanced", "Expert") and an `Of` method.
The rank is the byte value in both games, so the two classes differ only in the enum they switch on.

**Proposal.** `SkillLevels<TLevel> where TLevel : struct, Enum` in `Heroes.Entities`: `Of(TLevel?)` returns the info whose rank is the value's byte (and `null` for 0 or a value outside 1 to 3). Both `SkillLevels` classes go away.

**Option B (medium).** One `SkillLevel { Basic = 1, Advanced = 2, Expert = 3 }` in `Heroes`, used by both games, with "no level" as `null` everywhere. HoMM2 changes `SkillLevel.None` into `SkillLevel?` in `SecondarySkills`, `SkillEntry`, the byte reader and a few view models; HoMM3 renames `Beginner` and `Intermediate` (about 40 lines). This is the precondition of M3 and removes the generic catalog's reason to exist (there is then one `SkillLevels`). The cost is HoMM2 model churn for a representation question. I would do it together with M3, not alone.

### E4. Secondary skill: KEEP

**Now.** The first 14 of HoMM3's 30 skills are HoMM2's 14, with the same names and codes. HoMM3 adds Fire Magic and the rest, and two HotA skills with a version tier.
Both classes already implement `ISecondarySkill`.

**Why not share.** A shared class would need a version tier per skill, and the tier is a game's own enum (E1): either `Heroes` would know both games' releases, or the 14 shared skills would be listed in `Heroes` and each game would add its own on top, which two lists must then keep in step.
What the shared consumers need (a name and a code) is already `ISecondarySkill`. What is duplicated is 14 lines of literals whose codes the games fixed in 1996 and 1999; they cannot drift.
If a third game (HoMM1, fheroes2) reuses HoMM2's skills it takes HoMM2's class, not a copy.

### E5. Spell: INTERFACE (`ISpell`)

**Now.** HoMM2 `Spell(name, code, spellPoints, type)`, HoMM3 `Spell(name, code, version, school, level, type)`. Both have `All`, `FromByte`/`FromCode`, and HoMM2 a `NameOf(code)`.
`SpellType` is already in `Heroes`. The cost (`SpellPoints`) is HoMM2's, the school and level are HoMM3's.

**Proposal.** `ISpell : IGameEntity { SpellType Type }` in `Heroes.Entities`, implemented by both. It is the counterpart of `ICreature`; its first consumer would be a shared spell row (type shown in the spell lists). Without a consumer it is decoration, so add it with the consumer, not before.

**Not shared:** the spells and the school, level and cost: different data. `SpellGrantProvider`, `SpellScroll`, `SpellSchool` are HoMM3's rules (artifacts grant spells, scrolls hold a spell); HoMM2 has no such thing.

### E6. Creature, creature ability: already done; KEEP the rest

`ICreature`, `ICreatureAbility` and `CreatureInfo` are shared. What stays:
- `Dwelling`: `Dwellings?` flags in HoMM2, `Building?` in HoMM3. They are the keys into each game's building vocabulary (E8).
- `GetInfo(version)` and `VersionInfo` (HoMM3): HotA changes stats; HoMM2 has one table.
- `BaseCreatureCode`: both have it; it is the upgrade link, and `ICreature.IsUpgrade` is the shared view of it.
- Boilerplate in every entity class: `All`, `FromByte`, `NameOf(code)`, `ForFaction`. `NameOf` is the same line with a different noun in both games for creatures, spells and artifacts (`$"Unknown creature {code}"`). A generic `AbstractEnumeration.NameOf<T>(code, noun)` would remove it (about six one-liners); it is a tidy-up, not a design question.

### E7. Artifact: INTERFACE (`IArtifact` + the primary skill bonus)

**Now.** HoMM2 `Artifact(name, code, version)` has no data; what it gives is in `Models/ArtifactBonuses` (a table by code of the four primary skills).
HoMM3 `Artifact` has slot, blocked slots, class, cost, description, combination links and `ArtifactEffectInfo` (attack, defense, power, knowledge, luck, morale, movement, resources, spells, spell book, text).

**Proposal.** `IArtifact : IGameEntity { PrimarySkillBonus PrimarySkillBonus { get; } }` in `Heroes.Entities`, with `PrimarySkillBonus` being the shared record of M2. HoMM2 moves its bonus table into its `Artifact` entity (as HoMM3 does with `EffectInfo`), and the table stops being a model.
The consumer is real: M2's "shift the saved skills by what the artifact gives" is the same logic in both games and needs only this property.

**Not shared:** slot, class, cost, combinations, the other effects. HoMM2's artifacts have 14 positional slots and no class or cost in the savefile or in our tables. As far as I know the open source engine lists luck and morale effects for them, but we have not modelled them, and an interface member nobody fills is worse than none.

### E8. Buildings and dwellings: KEEP

**Now.** HoMM2: two `[Flags]` enums (`TownBuildings` 9 flags, `Dwellings` 15 flags that also hold the upgrade state and the Mage Guild level). HoMM3: `Building` (a class of several hundred entries, with prerequisites, upgrades and per-faction availability) and `BuildingType`, plus `CastleBuilder` (400 lines of build rules).

**Why not share.** There is no common data: HoMM2 stores a bit pattern with packed meanings, HoMM3 a set of building objects with a dependency graph.
The only common ideas are "dwelling level 1..7 serves creature tier n" and "Mage Guild level 1..5", and those are numbers, not types: they belong in the castle's shared parts (M12), not in a shared building type.
A common `IBuilding` would have a name and a flag, which the pages already get as a string and a toggle (`ToggleRowViewModel`).

### E9. Position: SHARE (`MapPosition`)

**Now.** HoMM2 `Models.Position(int X, int Y)` (a struct, with `ToString` "x,y"). HoMM3 `Entities.Position(short X, short Y, short Z)` (a record, with `Above`, `Underground`, `None`) and `HeroPosition : Position` adding the facing.
The folders disagree (`Models` against `Entities`), which is the first sign they are the same idea.

**Proposal.** `MapPosition(int X, int Y, int Level = 0)` as a `readonly record struct` in `Heroes.Models` with `ToString` ("x,y" or "x,y,level"), `Above`/`Underground` constants, and a `None` sentinel. HoMM2 always has level 0. `HeroPosition` stays in HoMM3 as `MapPosition` plus the facing (record composition, not inheritance, since a struct cannot be derived from).
The consumers: the headers of the castle and hero pages show a position; the `Destination`, `Boat`, `MarkerPosition` fields.

**Cost and risk.** About 140 uses in the two games; HoMM3's serializers narrow to `short`/`byte`, so the narrowing guards the serializers already have must cover it. The data is not at risk (the layout classes decide the bytes), and the compiler finds every use.
**Why it is only medium value:** no generic code except the display depends on it. Do it for uniformity and for a third game, not for a bug.

### E10. Hero level and movement: SHARE

**Now.** HoMM3 has `HeroLevel(int Experience, short Level)` and `HeroMovementPoints(uint Current, uint Max)`. HoMM2's `HeroModel` has the same four numbers loose: `Experience` (long), `Level`, `MaxMovement`, `MovementLeft` (long).

**Proposal.** `HeroLevel(long Experience, int Level)` and `MovementPoints(long Current, long Max)` in `Heroes.Models`, used by both hero models (HoMM2 replaces four fields by two). This matches what the pages show (General tab: level and experience; movement and "Full movement").
The reducers around them (level in step with experience, M14) then have one place.

### E11. HoMM3 hero vocabulary: KEEP

`HeroClass` (18 classes, each with a faction and a might/magic type), `HeroClassType`, `HeroGender`, `FacingDirection`, `WarMachine`, `WarMachineSlot`, `ActiveAdventureSpells`, `VisitedBuildingEffects`, `Backpack`/`BackpackSlot`, `ArtifactSlots`/`ArtifactClass`.

**Why.** None of them is in the HoMM2 model or in what the HoMM2 format notes describe: a HoMM2 hero is its faction and its number (9 per faction), with 14 positional artifact slots and no backpack, and the notes record nothing like a class, war machines, cast-today adventure spells or visited-building effects.
The format notes guess a facing byte and temporary luck/morale bytes for HoMM2 (`HOMM2_SAVE_FORMAT.md`); when they are decoded and modelled, `FacingDirection` is the first to move (it is just eight compass points).
Until then a shared type would be a type with one user.

### E12. Player type: KEEP (revisit)

`PlayerType { NotInGame, Computer, Human }` belongs to HoMM3's `GameSetup`. HoMM2's header has the equivalent as two masks (human allowed, computer allowed; marked as a guess in the notes) and is not modelled.
If HoMM2's header setup is read (M13), the shared concept is "who controls a colour", and `PlayerType` moves with `GameSetup` then.

---

## Models

### M1. Army: GENERIC (`Army<TCreature>`)

**Now.** HoMM2: `Army` is a fixed array of five nullable `ArmySlot(Creature, int Count)`, with `Empty`, `WithSlot`, value equality and `ToString`.
HoMM3: `Army` is a dictionary by slot number (1 to 7) of `ArmySlot(int SlotNumber, byte CreatureCode, uint Count)`, with `GetSlot`, `FindSlot`, `WithSlot(slot, creature, count)`, `WithoutSlot`, `Swap`.
Both are "N slots, each empty or a creature and a count", immutable, with the same reducers (set, clear, swap). The viewers already share `ArmyViewModelBase<TSlot>`, `StackSlotViewModel` and `ArmyEditorView`: the model is the only copy left.

**Proposal.** `Army<TCreature> where TCreature : IGameEntity` and `ArmySlot<TCreature>(TCreature Creature, long Count)` in `Heroes.Models`, a fixed number of slots given at construction (`Constants.ArmySlotCount` of the game), with `WithSlot`, `WithoutSlot`, `Swap`, value equality.
HoMM2's `Army` becomes `Army<Creature>`. HoMM3's changes from a creature *code* to a `Creature` in its slots (the deserializer resolves it, with the usual fallback, and the serializer leaves an unknown code alone, as `CLAUDE.md` says for HoMM2).

**Costs.**
- HoMM3 loses "the slot holds the raw code" (so an unknown creature code is read as the first known one, as every other entity is). Check against the saves that hold unknown codes before doing it.
- The count type: `long` covers HoMM2's `int` (32,767 max) and HoMM3's `uint`; the limits stay in each game's `Constants`.
- HoMM3's pages index by slot number 1..7 and HoMM2's by 0..4; one convention (0-based) at the model, the view model adds 1 where the game shows it.

**Value:** high. It is the largest model that is a pure duplicate, and the one a third game would otherwise copy again.

### M2. Primary skills: SHARE

**Now.** HoMM2 `PrimarySkills(int Attack, Defense, SpellPower, Knowledge)` plus the shifting code in `HeroModel` (`Shift`, `Limit`) and a bonus table. HoMM3 `HeroPrimarySkills(byte Atk, Def, Pow, Kno)` with `Bonus`, `BaseAtk`..., `WithBaseAtk`..., `WithBonus`, and `PrimarySkillBonus(short ...)`.
Since the fix of 2026-10-15 both mean the same: the savefile value has the artifact bonus in it, the base is saved minus bonus, and an artifact put on or taken off moves the saved value by its bonus.

**Proposal.** `PrimarySkills(int Attack, int Defense, int SpellPower, int Knowledge)` with `Bonus`, `Base...`, `WithBase...`, `WithBonus(PrimarySkillBonus)` and `PrimarySkillBonus` in `Heroes.Models`. Limits as constants in `Heroes` (a saved skill is a byte, 0 to 255); the game's "base maximum" (99 in HoMM3) stays in the game's `Constants` as the stepper limit.
Both hero models hold it. HoMM2's `Shift`/`Limit` and HoMM3's `WithBonus` become one tested implementation (the Arts saves already prove it for HoMM3, the E13 game test for HoMM2).
With E7, `IArtifact.PrimarySkillBonus` is what feeds `WithBonus`.

**Value:** high (it is where we just had a bug), and the equality rule (compare saved values, not the bonus) is one decision made once.

### M3. Secondary skills: GENERIC / SHARE (large)

**Now.**
- HoMM2: `SecondarySkills` is an ordered list of up to 8 `SkillEntry(SecondarySkill, SkillLevel)`; `With`, `Swap`, `WithoutSlot`, `WithSkillInSlot`, `LevelOf`. The savefile has 14 level bytes and 14 "order learned" bytes.
- HoMM3: `SecondarySkills` is a map of every skill to a nullable level plus an array of 8 slots, plus HotA's "extra skills" beyond the slots (`ExtraSkills`, `WithExtraSkillInSlot`, `WithSlotAsExtra`); `WithSkill`, `WithSkillLevel`, `WithSkillInSlot`, `WithoutSlot`, `Swap`. The savefile has a level byte per skill and the slot order.
It is the same file idea (levels by skill id, learning order); HoMM3's is the superset (extras).

**Proposal.** One `SecondarySkills<TSkill> where TSkill : ISecondarySkill` in `Heroes.Models` with the HoMM3 shape (levels by skill, slots, extras), where HoMM2 simply never has extras and has a smaller slot count; `SlotCount` a constructor argument. It needs one level type: Option B of E3 (`SkillLevel?`).
The `SecondarySkillsViewModelBase` in Wpf already abstracts over `Func<>`/`Action<>` and strings, so the view models do not change.

**Costs.** This is the biggest model change: it touches HoMM2's reader/writer/reducers (its order-learned semantics become slots), E3's enum change, and about 40 test lines. The behaviour must stay byte for byte (the round-trip tests of both games catch that).
**Why large and not first:** the view layer is already shared, so the gain is model code, and the risk is in the serializers. I would do it after M1 and M2 prove the pattern, and only with the E3 enum.

### M4. Spell book: GENERIC (`SpellSet<TSpell>`)

**Now.** HoMM2 `SpellBook` is a sorted set of learned spell codes (value equality, `With`, `Without`, `Contains`); the "has a book" fact is the Magic Book artifact. HoMM3 `SpellBook` is learned + granted (with the artifact named as the source) + other available + a has-a-book flag + `Source(spell)`.

**Proposal.** `SpellSet<TSpell> where TSpell : IGameEntity`: the immutable set with value equality, `With`, `Without`, `Contains`, in `Heroes.Models`. HoMM2's `SpellBook` is a `SpellSet<Spell>`; HoMM3's `SpellBook` holds one for `learned` and keeps its extra fields.
**Not shared:** granted spells, other-available, source and the has-a-book flag: they are HoMM3's artifact rules (`IGrantedSpellsResolver`).
**Value:** low to medium (about 40 lines and one equality implementation). Worth it if M1 is done, as the same "generic collection of entities" pattern; skip if not.

### M5. Artifacts and backpack: KEEP

HoMM2 `Artifacts` is 14 positional slots (`Add`, `Remove`, `WithSlot`, `HasMagicBook`). HoMM3 `EquippedArtifacts` is a map from `ArtifactSlots` flags to artifacts, with combination artifacts that block other slots, scroll spells, single/double slots (rings, misc), `GetTotalEffects`; `Backpack` is 64 slots.
The container *is* the rules: HoMM3's slots have types and blocking, HoMM2's do not. A common `IEquipment` would offer "enumerate what is worn", which `IArtifact` (E7) with a total `PrimarySkillBonus` already gives to a shared reader.
The only shared piece is the bonus (E7, M2). The page block for artifacts is already game-specific (`ArtifactsView`) for that reason.

### M6. Artifact bonus table (HoMM2): follows E7

`ArtifactBonuses` is a static table by code in `Models`. It is entity data. It moves into HoMM2's `Artifact` (`PrimarySkillBonus`), the way HoMM3 keeps it in `EffectInfo`; the table class goes. Nothing is shared by this beyond the property of `IArtifact`.

### M7. Resources: SHARE (`ResourceSet`)

**Now.** HoMM2 `Resources(long x7)` and HoMM3 `PlayerResources(int x7)`: seven named amounts, an indexer by `Resource`, `With(resource, amount)`, a `None`/zero, value equality. The code is the same `switch`.
HoMM2 also uses a second `Resources` for the daily income.

**Proposal.** `ResourceSet(long Wood, long Mercury, long Ore, long Sulfur, long Crystal, long Gems, long Gold)` in `Heroes.Models` (with `None`, `this[Resource]`, `With`, `All`). Limits (HoMM2: 999,999 gold and 9,999 others; HoMM3: its own) stay in each game's `Constants`. HoMM3 reads and writes each amount as an `Int32` (`PlayerDeserializer`, `SafePlayerSerializer`), so its serializer would need a narrowing guard that it does not have today (the stepper limits keep it in range, but the serializer should not rely on that).
`IResourcesPage` can then say `ResourceSet` and not only a list of rows. Income is a second `ResourceSet` in HoMM2.

### M8. `Numbers`: SHARE (Core)

`Numbers` (HoMM2) is an immutable list of ints with value equality, `With(index, n)`, `Of(...)`. HoMM3's `CastleModel` uses `IReadOnlyList<int>` for `CreaturesAvailable`, `UpgradedCreaturesAvailable`, `SpellCounts` inside a record: record equality compares those by reference.
That may not matter today (the pending changes come from the differs, and dirty state from bytes), but it means `castleA == castleB` is false for equal castles; **I have not checked who relies on HoMM3 castle equality, so this is a question, not a found bug.**
`Numbers` knows no game: move it to `Siniskalc.Core` (`Core.Models`), use it in HoMM3's castle. It is also what M12's shared parts hold.

### M9. Player: GENERIC base (`PlayerModelBase<TColor>`)

**Now.** HoMM2 `PlayerModel(Color, Resources, Income, HeroIds, SelectedHero, CastleIds, SelectedCastle, DaysLeft, Tavern)` with `IsOut`, `DaysToOwnACastle = 7`, `WithCastle`, `WithoutCastle`. HoMM3 `PlayerModel(PlayerColor, PlayerResources, Tavern, DaysLeft)` with `IsBanished`, `HasCountdown`, `WithDaysLeft(int)`.
The shared part is the banishing countdown (7 days in both; 0 = out) and resources and tavern. The HoMM2-only part is the lists of heroes and castles and the income (HoMM3 keeps the lists elsewhere).

**Proposal.** `abstract record PlayerModelBase<TColor>(TColor Color, ResourceSet Resources, TavernHeroes Tavern, int? DaysLeft)` in `Heroes.Models` with `IsBanished`, `HasCountdown`, `WithDaysLeft` and the constants (`MaxDaysLeft`). Each game's sealed `PlayerModel` derives from it and adds its own members. The navigation builders' two copies of the "N days left" text then read one property.
**Why a base and not a field:** record inheritance keeps `with` and positional deconstruction, so serializers and reducers change only their type names.
**Value:** medium. The banishing rule is where the games already agree, and `BanishingTextProvider` (HoMM3) could be shared with it.

### M10. Hero: KEEP (share the parts)

HoMM2 `HeroModel` is a flat 15-field record with reducers for artifacts and the book. HoMM3 `HeroModel` is a class with 25 constructor parameters, init properties and a `Reconcile` that keeps the spell book, effects and skills in step with the artifacts.
After E10, M1, M2, M3, M4 and M7 the two hold the same *parts* (level, movement, primary and secondary skills, army, spell set, spell points); what differs is what ties them together (artifact rules, class, backpack, war machines, flags). That glue is the game: a shared `HeroModel` would be a bag of optional members, and an interface to read it would have no reader that the pages do not already have (`IResettablePage`, `ILink`, `IOwnerChoice`).
So: no shared hero model. The hero page, when shared, binds to an `IHeroPage` like the player and castle pages do (not part of this review: it is a view-model question).

### M11. Hero summary: SHARE

HoMM3 `HeroSummary(Index, HeroId, Name, Owner, HeroClass, Level)` with `Description` "class, level N" is what the navigation tree and the player page's hero list are built from. HoMM2 builds the same two strings by hand in its navigation builder and its player view model (`$"{hero.Faction}, level {hero.Level}"`).
**Proposal.** `HeroSummary(int Index, int HeroId, string Name, IPlayerColor? Owner, string Kind, int Level)` in `Heroes.Models`; `Kind` is the class name (HoMM3) or the faction (HoMM2). Both collections produce it (`GetSummaries()` exists in HoMM3's collection). It also removes the two `NavEntry`/`LinkViewModel` construction copies.

### M12. Castle: KEEP; share `MageGuild` and `CreatureStock`

**Now.** HoMM2 `CastleModel`: owner, faction, position, boat, garrison, visiting hero, `TownBuildings`, `Dwellings` (flags that also carry the upgrade state and Mage Guild level), `Creatures`, `UpgradedCreatures` (6 numbers each), `GuildSpells` (15 numbers), `GuildSpellCounts` (5).
HoMM3 `CastleModel`: name, faction, owner, position, garrison, a set of `Building`s, shipyard location, garrisoned and visiting hero ids, enabled buildings, built-this-turn flag, Mage Guild level, `CreaturesAvailable`, `UpgradedCreaturesAvailable` (7 each), spell counts and `MageGuildSpells` (list of lists).

**Why the castle itself stays separate.** Buildings (E8), the rules around them (`CastleBuilder`), the second hero slot, the map's allowed buildings: the two castles are built around different building models. A base record with the common 8 fields would save little and tie both to one field order.

**What is the same and can be shared as values:**
- `MageGuild<TSpell>(int Level, spells by level, counts by level)` in `Heroes.Models`: the guild level, the spells held in each level's slots and how many slots each level shows. Both pages have a level selector and per-level slot lists (`GuildLevelViewModel` / `MageGuildLevelRowViewModel`), so a shared view model could follow.
- `CreatureStock(Numbers Base, Numbers Upgraded)`: the creatures waiting in each dwelling level, in the base and upgraded forms.
Both are values the castle record holds, so each game's castle changes one field type, not its shape.
**Value:** medium; do it when the castle page's mage guild and available-creatures cards are shared as view models.

### M13. Map header and game setup: SHARE (`MapHeader`)

**Now.** HoMM2 `MapHeader(Name, Description, Width, Height)`. HoMM3 `MapHeader(Name, Description, GameDifficulty, Size, HasUnderground)` with `SizeText`. HoMM3 also has `GameSetup(PlayerControls, Difficulty, CurrentPlayer)`.
The HoMM2 header has a difficulty (`u16` at 0x0C, guessed the same scale) and the masks for human/computer players (guessed), neither read yet. `GameDifficulty` is already in `Heroes`.

**Proposal.** `MapHeader(Name, Description, Width, Height, HasUnderground = false, GameDifficulty? Difficulty = null)` in `Heroes.Models`, with `SizeText` computed from the width (HoMM3's maps are square). Both overviews read the same record. `GameSetup` stays HoMM3's until HoMM2's header is decoded (E12).
**Cost:** HoMM3's `Size` (square) becomes `Width`/`Height`; `SizeText` is tested (`MapHeaderTests`, and the overview's `MapSizeText`), so it keeps its output.

### M14. Experience table: GENERIC (`ExperienceTable`)

**Now.** HoMM2 `HeroLevels`: a 40-entry table, extended by a formula (`+ 1.2 x last step`, rounded to 100) up to the maximum level, with `ExperienceOf(level)` and `LevelOf(experience)`. HoMM3 `LevelCalculator`: an explicit dictionary of 75 levels, `GetLevelFromXp`, `GetNextLevel`, `GetXpForLevel`.
**The tables differ** (from level 5: 4,500 against 4,600; 6,000 against 6,200; 9,000 against 10,000), so the data cannot be shared. The mechanism is the same: thresholds, level of an experience, experience of a level, next level.

**Proposal.** `ExperienceTable` in `Heroes.Models`, built from a list of thresholds (and an optional extension rule), with the three methods and `NextLevel`. Each game keeps its thresholds as data and, per `CLAUDE.md`, the table is a static of the game that builds one. HoMM2's reducer that keeps the level in step with the experience and HoMM3's `HeroLevel` then use the same lookups.
**Value:** low to medium (small, but the lookup is currently written twice, in two styles: a loop with a formula and a dictionary scan).

### M15. `GameRuleException`: SHARE

HoMM3 throws `GameRuleException` for rule violations (a building that cannot be built); HoMM2 throws plain `InvalidOperationException` ("has the most castles a player can have"). Move the exception to `Heroes.Models` and have HoMM2 use it, so a page can catch one type. Trivial.

### M16. Spell points rule: KEEP

`ISpellPointsCalculator`/`SpellPointsCalculator` (HoMM3): ten per knowledge point, plus a percentage from Intelligence that differs in HotA. HoMM2 edits spell points as a plain stepper and does not compute a maximum.
When HoMM2 gets a maximum (knowledge x 10 in fheroes2's rules, not checked against the game here) the same interface can be implemented there: `ISpellPointsCalculator.GetMaximum(knowledge, level)` would move to `Heroes` then. Until then it has one implementation.

### M17. Granted spells, castle builder, obelisks: KEEP

`IGrantedSpellsResolver`/`GrantedSpellsResolver` (artifacts grant spells), `CastleBuilder` and `CastleBuildFailureReason` (HoMM3's building rules), `ObeliskVisits` (HoMM3's puzzle map; HoMM2 has no obelisks), `PrimarySkillBonus` (moves with M2), `HeroSummary` (M11).
No HoMM2 counterpart, so nothing to abstract.

---

## Suggested order

1. **Small, mechanical, no behaviour change:** E1 (generic enumeration base), E2/E3 (generic catalogs), M7 (`ResourceSet`), M8 (`Numbers` to Core), M15 (`GameRuleException`), E10 (level and movement).
2. **M2 + E7 + M6 (primary skills, `IArtifact`):** the logic we just fixed, now in one place with the Arts and E13 tests behind it.
3. **M1 (`Army<TCreature>`):** the largest pure duplicate; check the saves with unknown creature codes first.
4. **M9, M11, M13, M14:** player base, hero summary, map header, experience table.
5. **E3 option B + M3 (secondary skills), M4 (`SpellSet`), M12 (`MageGuild`, `CreatureStock`):** the larger ones, once the pattern is proven; each needs its own round-trip check.
6. **E9 (`MapPosition`):** uniformity; any time.

## What I would not do, and why

- Merge **creature, spell, artifact tables, factions, skills, buildings**: the data differs in codes and sets, so a shared table is wrong for one game. Only the shapes (interfaces) are shared.
- A shared **`HeroModel` or `CastleModel`**: they are the glue of each game's rules (M10, M12).
- A shared **`GameVersion`**: it is the list of what a game's savefile can be (E1).

## Questions for you

1. E3 option B (one `SkillLevel` enum with `null`) changes HoMM2's model; do you want it, since M3 needs it?
2. M1 changes HoMM3's army from creature codes to creatures; are you happy with unknown codes reading as the first known creature, as for every other entity?
3. M8: does anything depend on HoMM3's castle record equality? (I did not find it; nobody has checked.)
4. M12 and M13 depend on decoding more of the HoMM2 header and castle; leave them until then?
