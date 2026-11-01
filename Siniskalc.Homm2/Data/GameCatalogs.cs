namespace Siniskalc.Homm2.Data
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Runtime;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Homm2.Entities;

    /// <summary>Loads all standard definitions once. Selecting a version never performs I/O.</summary>
    public sealed class GameCatalogs
    {
        private const string Root = @"GameData\Homm2\";
        private readonly IReadOnlyDictionary<GameVersion, GameCatalog> catalogs;

        public GameCatalogs(IGameDefinitionReader reader)
        {
            var manifest = reader.Read<DefinitionManifest>(Root + "manifest.json");
            Require(manifest.SchemaVersion == 1 && manifest.Versions is not null, "manifest.json", "schemaVersion/versions", "Expected schema version 1 and explicit version mappings.");
            Require(Enum.GetValues<GameVersion>().All(v => manifest.Versions!.ContainsKey(v)) && manifest.Versions!.Count == Enum.GetValues<GameVersion>().Length, "manifest.json", "versions", "Every supported version, including Unknown fallback, must be mapped exactly once.");
            Require(manifest.Sets is not null && manifest.Sets.Count > 0 && manifest.Versions!.Values.All(s => s is not null && manifest.Sets.ContainsKey(s)), "manifest.json", "sets", "Every version must name a declared standard set.");
            var sets = new Dictionary<string, CatalogDefinitions>(StringComparer.Ordinal);
            var resolving = new HashSet<string>(StringComparer.Ordinal);
            foreach (var set in manifest.Sets!.Keys)
            {
                Resolve(set);
            }

            var snapshots = sets.ToDictionary(p => p.Key, p => new GameCatalog(p.Key, p.Value));
            this.catalogs = new ReadOnlyDictionary<GameVersion, GameCatalog>(manifest.Versions!.ToDictionary(p => p.Key, p => snapshots[p.Value]));

            CatalogDefinitions Resolve(string set)
            {
                if (sets.TryGetValue(set, out var existing))
                {
                    return existing;
                }

                Require(!string.IsNullOrWhiteSpace(set) && set.All(c => char.IsAsciiLetterOrDigit(c) || c == '-'), "manifest.json", "versions", "A set is a simple directory name.");
                Require(manifest.Sets.TryGetValue(set, out var declaration) && declaration is not null, "manifest.json", set, "Unknown standard parent set.");
                Require(resolving.Add(set), "manifest.json", set, "Standard parent cycle.");
                var mappedVersions = string.Join("/", manifest.Versions!.Where(v => v.Value == set).Select(v => v.Key));
                var versions = mappedVersions.Length == 0 ? $"parent set {set}" : mappedVersions;
                var parent = declaration!.Parent is null ? null : Resolve(declaration.Parent);
                var data = new CatalogDefinitions(
                    Read("factions", parent?.Factions),
                    Read("abilities", parent?.Abilities),
                    Read("skills", parent?.Skills),
                    Read("artifacts", parent?.Artifacts),
                    Read("creatures", parent?.Creatures),
                    Read("spells", parent?.Spells),
                    Read("spellTypes", parent?.SpellTypes),
                    Read("buildings", parent?.Buildings),
                    Read("visitablePlaces", parent?.Places),
                    Read("mines", parent?.Mines),
                    Read("visitEffects", parent?.VisitEffects),
                    Read("mapObjects", parent?.MapObjects));
                Validate(data, set, versions);
                sets.Add(set, data);
                resolving.Remove(set);
                return data;

                T[] Read<T>(string domain, T[]? inherited)
                    where T : EntityDefinition
                {
                    var path = set + "\\" + domain + ".json";
                    void Require(bool condition, string file, string property, string message) =>
                        GameCatalogs.Require(condition, file, $"{versions} {property}", message);

                    var file = reader.Read<DefinitionFile<T>>(Root + path);
                    Require(file.SchemaVersion == 1 && file.Entries is not null && file.Replacements is not null, path, domain, "Expected schemaVersion 1 and non-null entries/replacements.");
                    Require(inherited is not null || file.Replacements!.Length == 0, path, "replacements", "A root set cannot replace an entity.");
                    var declared = file.Entries!.Concat(file.Replacements!).ToArray();
                    Require(declared.All(e => e is not null), path, "entries", "Entity records cannot be null.");
                    Require(declared.Select(e => e.Id).Distinct(StringComparer.Ordinal).Count() == declared.Length, path, "id", "Duplicate IDs within a definition file.");
                    var merged = (inherited ?? []).ToDictionary(e => e.Id, StringComparer.Ordinal);
                    foreach (var addition in file.Entries!)
                    {
                        Require(addition.Id is not null && !merged.ContainsKey(addition.Id), path, addition.Id + ".id", "An addition cannot overwrite an inherited entity; declare a replacement.");
                        merged.Add(addition.Id!, addition);
                    }

                    foreach (var replacement in file.Replacements!)
                    {
                        Require(replacement.Id is not null && inherited is not null && inherited.Any(e => e.Id == replacement.Id), path, replacement.Id + ".id", "A replacement must target an inherited entity.");
                        merged[replacement.Id!] = replacement;
                    }

                    var entries = merged.Values.ToArray();
                    Require(entries.Length > 0, path, "entries", "The resolved domain cannot be empty.");
                    foreach (var entry in entries)
                    {
                        Require(!string.IsNullOrWhiteSpace(entry.Id) && entry.Id.All(c => char.IsAsciiLetterOrDigit(c) || c is '-' or '_'), path, entry.Id + ".id", "A stable nonempty identifier is required.");
                        Require(!string.IsNullOrWhiteSpace(entry.Name), path, entry.Id + ".name", "A display name is required.");
                    }

                    Require(entries.Select(e => e.Id).Distinct(StringComparer.Ordinal).Count() == entries.Length, path, "id", "Duplicate IDs.");
                    Require(entries.Select(e => e.Code).Distinct().Count() == entries.Length, path, "code", "Duplicate save codes.");
                    Require(entries.Select(e => e.Name).Distinct(StringComparer.OrdinalIgnoreCase).Count() == entries.Length, path, "name", "Ambiguous display names.");
                    return entries;
                }
            }
        }

        public GameCatalog For(GameVersion version) => this.catalogs.TryGetValue(version, out var catalog) ? catalog : throw new ArgumentOutOfRangeException(nameof(version));

        private static void Require(bool condition, string file, string property, string message)
        {
            if (!condition)
            {
                throw new GameDefinitionException(Root + file, $"HoMM2 {property}: {message}");
            }
        }

        private static void Validate(CatalogDefinitions d, string set, string versions)
        {
            void Check(bool condition, string domain, string property, string message) => Require(condition, set + "\\" + domain + ".json", $"{versions} {property}", message);
            void Reference<T>(string? id, T[] values, string domain, string property)
                where T : EntityDefinition => Check(id is not null && values.Any(v => v.Id == id), domain, property, $"Unknown reference '{id}'.");

            Check(d.Factions.Any(f => f.Id == "Neutral"), "factions", "Neutral", "The well-known Neutral faction is required.");
            Check(d.Factions.Any(f => f.Code == 0 && f.Id != "Neutral"), "factions", "code", "A non-neutral faction with code 0 is required.");
            var heroes = new HashSet<int>();
            foreach (var f in d.Factions)
            {
                Check(f.Code != Constants.Empty && f.HeroIds is not null && !string.IsNullOrWhiteSpace(f.GrowthBuildingName) && !string.IsNullOrWhiteSpace(f.SpecialBuildingName), "factions", f.Id, "Faction codes, building names and hero mappings are required.");
                Check(!string.IsNullOrWhiteSpace(f.SpecialBuildingDescription), "factions", f.Id + ".specialBuildingDescription", "A special-building description is required.");
                Check(f.GuildSpellCounts is not null && f.GuildSpellCounts.Length == Constants.Castle.MageGuildLevels && f.GuildSpellCounts.All(n => n >= 0 && n + (f.Library ? 1 : 0) <= Constants.Castle.MageGuildSlotsPerLevel), "factions", f.Id + ".guildSpellCounts", "Guild counts, including the optional library spell, must fit the five levels and four slots per level.");
                foreach (var id in f.HeroIds!)
                {
                    Check(id >= 0 && id < Constants.Table.HeroCount && heroes.Add(id), "factions", f.Id + ".heroIds", "Hero IDs must be unique and fit the fixed hero table.");
                }
            }

            Check(heroes.Count > 0, "factions", "heroIds", "At least one hero faction mapping is required.");
            Check(d.Creatures.Any(c => c.Code == 0), "creatures", "code", "A creature with code 0 is required.");
            foreach (var c in d.Creatures)
            {
                Check(c.Code != Constants.Empty && c.Tier is >= 1 and <= 6 && c.Stats is not null && c.Abilities is not null, "creatures", c.Id, "Creature code, tier, stats and abilities must fit the format.");
                var s = c.Stats!;
                Check(s.Atk >= 0 && s.Def >= 0 && s.MinDamage >= 0 && s.MaxDamage >= s.MinDamage && s.Health > 0 && s.Speed >= 0 && s.Growth >= 0 && s.AiValue >= 0 && s.FightValue >= 0 && s.Gold >= 0 && s.Shots >= 0 && s.Wood >= 0 && s.Ore >= 0 && s.Mercury >= 0 && s.Sulfur >= 0 && s.Crystal >= 0 && s.Gems >= 0, "creatures", c.Id + ".stats", "Stats and costs cannot be negative; health must be positive and damage ordered.");
                foreach (var ability in c.Abilities!)
                {
                    Reference(ability, d.Abilities, "creatures", c.Id + ".abilities");
                }

                Check(c.Abilities.Distinct().Count() == c.Abilities.Length, "creatures", c.Id + ".abilities", "Duplicate ability.");
                if (c.Faction is not null)
                {
                    Reference(c.Faction, d.Factions, "creatures", c.Id + ".faction");
                    Check(c.Dwelling is not null && Enum.IsDefined(c.Dwelling.Value), "creatures", c.Id + ".dwelling", "A faction creature requires one supported dwelling flag.");
                }
                else
                {
                    Check(c.Dwelling is null && c.BaseCreature is null, "creatures", c.Id, "A neutral creature cannot have a town dwelling or upgrade.");
                }

                var seen = new HashSet<string> { c.Id };
                var current = c;
                var upgrades = 0;
                while (current.BaseCreature is { } baseId)
                {
                    Reference(baseId, d.Creatures, "creatures", c.Id + ".baseCreature");
                    Check(seen.Add(baseId), "creatures", c.Id + ".baseCreature", "Upgrade cycle.");
                    var parent = d.Creatures.Single(v => v.Id == baseId);
                    Check(parent.Faction == c.Faction && parent.Tier == c.Tier, "creatures", c.Id + ".baseCreature", "An upgrade must keep its faction and tier.");
                    Check(Array.IndexOf(d.Creatures, parent) < Array.IndexOf(d.Creatures, current), "creatures", c.Id, "An upgrade must be ordered after its base creature.");
                    upgrades++;
                    current = parent;
                }

                if (c.Faction is not null)
                {
                    var expectedDwelling = upgrades == 0 ? (Dwellings)(0x08u << (c.Tier - 1))
                        : upgrades == 1 && c.Tier >= 2 ? (Dwellings)(0x100u << (c.Tier - 1))
                        : upgrades == 2 && c.Tier == Constants.Castle.DwellingCount ? Dwellings.Dwelling6UpgradedTwice
                        : Dwellings.None;
                    Check(expectedDwelling != Dwellings.None && c.Dwelling == expectedDwelling, "creatures", c.Id + ".dwelling", "The dwelling flag must match the tier and upgrade depth of the supported format.");
                }
            }

            foreach (var group in d.Creatures.Where(c => c.Faction is not null).GroupBy(c => (c.Faction, c.Tier)))
            {
                Check(group.Count() <= (group.Key.Tier == 6 ? 3 : group.Key.Tier == 1 ? 1 : 2), "creatures", group.Key.ToString(), "Too many upgrades for this dwelling.");
                Check(group.Select(c => c.Dwelling).Distinct().Count() == group.Count(), "creatures", group.Key.ToString(), "Ambiguous dwelling flags.");
            }

            foreach (var spell in d.Spells)
            {
                Check(spell.Code is >= 1 and <= Constants.Hero.SpellCount && spell.Level is >= 1 and <= 5 && spell.SpellPoints >= 0 && Enum.IsDefined(spell.Type), "spells", spell.Id, "Spell code, level, cost or type is outside the supported format.");
                Reference(spell.Group, d.SpellTypes, "spells", spell.Id + ".group");
            }

            Check(d.SpellTypes.Count(g => g.Hidden) == 1, "spellTypes", "hidden", "Exactly one no-group role is required.");
            Check(d.Skills.All(s => s.Code < Constants.Hero.SecondarySkillCount), "skills", "code", "Skills must fit their fixed table.");
            Check(d.Artifacts.Count(a => a.Role == ArtifactRole.SpellBook) == 1 && d.Artifacts.Count(a => a.Role == ArtifactRole.Scroll) == 1, "artifacts", "role", "Exactly one spell book and scroll is required.");
            Check(d.Artifacts.Any(a => a.Code == 0 && a.AvailableFrom == GameVersion.BaseGame), "artifacts", "code", "An artifact with code 0 must exist in the base game.");
            Check(d.Artifacts.Where(a => a.Role == ArtifactRole.SpellBook).All(a => a.AvailableFrom == GameVersion.BaseGame), "artifacts", "role", "The spell book must exist in the base game.");
            foreach (var a in d.Artifacts)
            {
                Check(a.Code != Constants.Empty && Enum.IsDefined(a.Role) && a.AvailableFrom is GameVersion.BaseGame or GameVersion.PriceOfLoyalty && a.EffectInfo is not null && a.EffectInfo.Modifiers is not null, "artifacts", a.Id, "Unsupported artifact role, version, code or effects.");
                var modifiers = a.EffectInfo!.Modifiers!;
                Check(modifiers.All(e => e is not null && e.Attribute is ArtifactAttribute.Attack or ArtifactAttribute.Defense or ArtifactAttribute.SpellPower or ArtifactAttribute.Knowledge && e.Amount is >= -255 and <= 255), "artifacts", a.Id + ".effectInfo.modifiers", "Only primary-skill modifiers from -255 to 255 are currently implemented for HoMM2.");
                Check(modifiers.Select(e => e.Attribute).Distinct().Count() == modifiers.Length, "artifacts", a.Id + ".effectInfo.modifiers", "An attribute must occur at most once.");
            }

            Check(d.Buildings.Select(b => b.Flag).Distinct().Count() == d.Buildings.Length, "buildings", "flag", "Building flags are ambiguous.");
            foreach (var b in d.Buildings)
            {
                Check(Enum.IsDefined(b.Flag) && b.Flag != TownBuildings.None && Enum.IsDefined(b.Role) && b.Description is not null, "buildings", b.Id, "Unsupported building flag, role or description.");
                Check((b.Role != BuildingRole.Growth || b.Flag == TownBuildings.GrowthBuilding) && (b.Role != BuildingRole.Special || b.Flag == TownBuildings.Special), "buildings", b.Id, "A building capability must use its supported encoding slot.");
                Check(b.Growth >= 0 && b.Growth <= Constants.Castle.MaxWaiting, "buildings", b.Id + ".growth", "Weekly growth must fit the supported creature stock.");
            }

            Check(d.Buildings.Count(b => b.Role == BuildingRole.Growth) == 1 && d.Buildings.Count(b => b.Role == BuildingRole.Special) == 1, "buildings", "role", "Exactly one growth-building and one faction-special-building role are required.");

            Check(d.Places.Select(p => p.Kind).Distinct().Count() == d.Places.Length && d.Places.Length == Enum.GetValues<PlaceKind>().Length, "visitablePlaces", "kind", "Every supported visit encoding must have exactly one metadata record.");
            Check(d.Places.All(p => Enum.IsDefined(p.Kind) && !string.IsNullOrWhiteSpace(p.Description)), "visitablePlaces", "kind/description", "Unknown place behaviour or missing description.");
            Check(d.Mines.Length == Enum.GetValues<MineKind>().Length && d.Mines.Select(m => m.Kind).Distinct().Count() == d.Mines.Length, "mines", "kind", "Every supported mine encoding must have exactly one definition.");
            Check(d.Mines.All(m => Enum.IsDefined(m.Kind) && Enum.IsDefined(m.Resource) && m.Code == (byte)m.Kind), "mines", "kind/resource", "Unknown mine kind, resource or mismatched encoding.");
            Check(d.VisitEffects.Select(e => e.Flag).Distinct().Count() == d.VisitEffects.Length, "visitEffects", "flag", "Duplicate visit effect flags.");
            foreach (var e in d.VisitEffects)
            {
                Check(Enum.IsDefined(e.Flag) && e.Flag != HeroEffects.None && Enum.IsDefined(e.Behaviour) && !string.IsNullOrWhiteSpace(e.Description), "visitEffects", e.Id, "Unknown behaviour, flag or missing description.");
                Check(e.Morale >= Constants.Hero.MinBattleBonus && e.Morale <= Constants.Hero.MaxBattleBonus && e.Luck >= Constants.Hero.MinBattleBonus && e.Luck <= Constants.Hero.MaxBattleBonus, "visitEffects", e.Id, "Battle bonuses are outside the supported range.");
                Check(e.Behaviour == VisitBehaviour.NextBattle || (e.Morale == 0 && e.Luck == 0), "visitEffects", e.Id, "Only nextBattle effects can add a battle bonus.");
            }

            var roles = Enum.GetValues<MapObjectRole>();
            Check(d.MapObjects.Length == roles.Length && roles.All(role => d.MapObjects.Count(o => o.Role == role) == 1), "mapObjects", "role", "Every supported map-object presentation role must occur exactly once.");
            foreach (var o in d.MapObjects)
            {
                Check(!string.IsNullOrWhiteSpace(o.NameFormat), "mapObjects", o.Id + ".nameFormat", "A name format is required.");
                try
                {
                    _ = string.Format(System.Globalization.CultureInfo.InvariantCulture, o.NameFormat, 1, 2);
                }
                catch (FormatException)
                {
                    Check(false, "mapObjects", o.Id + ".nameFormat", "Only the supported coordinate/index placeholders {0} and {1} are allowed.");
                }
            }
        }
    }
}
