namespace Siniskalc.Homm3.Data;

using System;
using System.Collections.Generic;
using System.Linq;
using Siniskalc.Heroes.Entities;
using Siniskalc.Homm3.Entities;

/// <summary>Validates representability and references before publishing any runtime entity.</summary>
public static class CatalogValidator
{
    /// <summary>
    /// Checks the definitions of a version of the game before any entity is made from them: that they can be represented in a savefile and that they refer to each other correctly.
    /// </summary>
    /// <param name="data">The definitions.</param>
    /// <param name="version">The version of the game.</param>
    /// <param name="directory">The folder the definitions were read from, for the messages.</param>
    public static void Validate(CatalogDefinitions data, GameVersion version, string directory)
    {
        void Check(bool condition, string domain, string message)
        {
            var id = message.Split('.', ':', ' ')[0];
            var path = data.SourcePaths.TryGetValue(domain + ":" + id, out var source) || data.SourcePaths.TryGetValue(domain, out source)
                ? source : directory + "\\" + domain + ".json";
            GameCatalogLoader.Require(condition, path, $"{version}: {message}");
        }

        Dictionary<string, T> Index<T>(T[] records, string domain, bool contextualCodes = false)
            where T : EntityDefinition
        {
            Check(records.Length > 0, domain, "The domain must not be empty.");
            var ids = new Dictionary<string, T>(StringComparer.Ordinal);
            var codes = new HashSet<byte>();
            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var d in records)
            {
                Check(!string.IsNullOrWhiteSpace(d.Name), domain, $"{d.Id}.name must not be empty.");
                Check(contextualCodes || names.Add(d.Name), domain, $"{d.Id}.name duplicates another selectable name.");
                Check(Enum.IsDefined(d.AvailableFrom) && d.AvailableFrom != GameVersion.Unknown, domain, $"{d.Id}.availableFrom is not a supported version.");
                Check(ids.TryAdd(d.Id, d), domain, $"{d.Id}: duplicate id.");
                Check(contextualCodes || codes.Add(d.Code), domain, $"{d.Id}.code duplicates {d.Code}.");
            }

            return ids;
        }

        var factions = Index(data.Factions, "factions");
        var abilities = Index(data.Abilities, "abilities");
        var buildings = Index(data.Buildings, "buildings", contextualCodes: true);
        var creatures = Index(data.Creatures, "creatures");
        var schools = Index(data.SpellSchools, "spellSchools");
        var spells = Index(data.Spells, "spells");
        var classes = Index(data.HeroClasses, "heroClasses");
        var skills = Index(data.SecondarySkills, "secondarySkills");
        Index(data.WarMachines, "warMachines");
        var artifacts = Index(data.Artifacts, "artifacts");
        Index(data.VisitablePlaces, "visitablePlaces");
        Check(data.VisitablePlaces.Select(p => p.Slot).Distinct().Count() == data.VisitablePlaces.Length, "visitablePlaces", "Visit storage slots must be unique.");
        foreach (var p in data.VisitablePlaces)
        {
            Check(Enum.IsDefined(p.Slot) && p.Description is not null && p.NumberMask > 0 && p.ObjectType is null or >= 0, "visitablePlaces", $"{p.Id}: invalid slot, description, mask or objectType.");
        }

        void Reference<T>(Dictionary<string, T> index, string? id, string domain, string property)
            => Check(id is not null && index.ContainsKey(id), domain, $"{property}: unknown reference '{id}'.");
        void One<T>(IEnumerable<T> values, string domain, string role) => Check(values.Count() == 1, domain, $"Exactly one {role} role is required.");
        One(data.Factions.Where(d => d.Id == "Neutral"), "factions", "Neutral faction");
        One(data.Creatures.Where(d => d.Code == 0), "creatures", "code 0");
        One(data.HeroClasses.Where(d => d.Code == 0), "heroClasses", "code 0");
        One(data.Artifacts.Where(d => d.Role == ArtifactRole.Scroll), "artifacts", "scroll");
        One(data.Artifacts.Where(d => d.Role == ArtifactRole.SpellBook), "artifacts", "spellBook");
        One(data.Artifacts.Where(d => d.Code == 0), "artifacts", "code 0");
        One(data.SpellSchools.Where(d => d.Id == "all"), "spellSchools", "well-known all school");
        foreach (var (entity, domain) in new (EntityDefinition, string)[]
        {
            (data.Factions.Single(f => f.Id == "Neutral"), "factions"),
            (data.Creatures.Single(c => c.Code == 0), "creatures"),
            (data.HeroClasses.Single(c => c.Code == 0), "heroClasses"),
            (data.Artifacts.Single(a => a.Role == ArtifactRole.Scroll), "artifacts"),
            (data.Artifacts.Single(a => a.Role == ArtifactRole.SpellBook), "artifacts"),
            (data.SpellSchools.Single(s => s.Id == "all"), "spellSchools"),
        })
            Check(entity.AvailableFrom == GameVersion.RestorationOfErathia, domain, $"{entity.Id}: a reserved role must be available in every supported version.");
        foreach (var school in data.SpellSchools)
        {
            Check(school.Description is not null, "spellSchools", $"{school.Id}.description must not be null.");
        }

        void Available(EntityDefinition source, EntityDefinition target, string domain, string property)
            => Check(source.AvailableFrom > version || target.AvailableFrom <= version, domain, $"{source.Id}.{property}: '{target.Id}' is unavailable in this version.");
        foreach (var c in data.Creatures)
        {
            Reference(factions, c.Faction, "creatures", c.Id + ".faction");
            Available(c, factions[c.Faction], "creatures", "faction");
        }

        foreach (var faction in data.Factions)
        {
            Check(Enum.IsDefined(faction.CompanionBits), "factions", $"{faction.Id}.companionBits is unsupported.");
            Check(faction.Id == "Neutral" || faction.Code < byte.MaxValue, "factions", $"{faction.Id}.code uses the reserved absent byte.");
            var applicable = data.Buildings.Where(b => b.Faction == faction.Id || (b.Faction is null && b.Except is not null && !b.Except.Contains(faction.Id))).ToArray();
            Check(applicable.Select(b => b.Code).Distinct().Count() == applicable.Length, "buildings", $"{faction.Id}: ambiguous building codes.");
            Check(applicable.Select(b => b.Type).Distinct().Count() == applicable.Length, "buildings", $"{faction.Id}: ambiguous building types.");
            var reserved = faction.CompanionBits switch
            {
                CompanionBitPolicy.Halls => new[] { 27, 28, 29 },
                CompanionBitPolicy.Fort => [27],
                CompanionBitPolicy.FortAndHalls => [22, 27, 28, 29],
                _ => [],
            };
            Check(applicable.All(b => !reserved.Contains(b.Code)), "buildings", $"{faction.Id}: a building code collides with its companion-bit policy.");
            var triggers = faction.CompanionBits switch
            {
                CompanionBitPolicy.Halls => new[] { BuildingType.TownHall, BuildingType.CityHall, BuildingType.Capitol },
                CompanionBitPolicy.Fort => [BuildingType.Fort],
                CompanionBitPolicy.FortAndHalls => [BuildingType.Fort, BuildingType.TownHall, BuildingType.CityHall, BuildingType.Capitol],
                _ => [],
            };
            Check(triggers.All(type => applicable.Any(b => b.Type == type)), "buildings", $"{faction.Id}: the companion-bit policy is missing a trigger building.");
        }

        foreach (var b in data.Buildings)
        {
            Check(b.GoldIncome >= 0, "buildings", $"{b.Id}.goldIncome must be nonnegative.");
            Check(b.Code < 64 && Enum.IsDefined(b.Type), "buildings", $"{b.Id}: code/type is not representable in a building mask.");
            Check(b.Requires is not null && b.Except is not null, "buildings", $"{b.Id}: requires/except must not be null.");
            if (b.Faction is not null)
            {
                Reference(factions, b.Faction, "buildings", b.Id + ".faction");
            }

            if (b.Upgrades is not null)
            {
                Reference(buildings, b.Upgrades, "buildings", b.Id + ".upgrades");
            }

            foreach (var id in b.Requires!)
            {
                Reference(buildings, id, "buildings", b.Id + ".requires");
            }

            foreach (var id in b.Except!)
            {
                Reference(factions, id, "buildings", b.Id + ".except");
            }

            Check(!b.Automatic || (b.Upgrades is not null && b.Requires.Length > 0), "buildings", $"{b.Id}: automatic buildings require an upgrade and prerequisites.");
            Check(b.Requires.Distinct().Count() == b.Requires.Length && b.Except.Distinct().Count() == b.Except.Length, "buildings", $"{b.Id}: requires/except must not contain duplicates.");
            foreach (var faction in data.Factions.Where(f => b.Faction == f.Id || (b.Faction is null && !b.Except.Contains(f.Id))))
            {
                foreach (var prerequisite in b.Upgrades is null ? b.Requires : b.Requires.Prepend(b.Upgrades))
                {
                    var target = buildings[prerequisite];
                    Check(
                        target.Faction == faction.Id || (target.Faction is null && !target.Except.Contains(faction.Id)),
                        "buildings",
                        $"{b.Id}: prerequisite '{prerequisite}' is unavailable to faction '{faction.Id}'.");
                    Available(b, target, "buildings", "requires");
                }
            }
        }

        void Acyclic<T>(Dictionary<string, T> records, Func<T, IEnumerable<string>> edges, string domain)
        {
            var done = new HashSet<string>();
            var active = new HashSet<string>();
            void Visit(string id)
            {
                if (done.Contains(id))
                {
                    return;
                }

                Check(active.Add(id), domain, $"{id}: dependency cycle.");
                foreach (var next in edges(records[id]))
                {
                    Visit(next);
                }

                active.Remove(id);
                done.Add(id);
            }

            foreach (var id in records.Keys)
            {
                Visit(id);
            }
        }

        Acyclic(buildings, b => b.Upgrades is null ? b.Requires : b.Requires.Prepend(b.Upgrades), "buildings");
        foreach (var c in data.Creatures)
        {
            Reference(factions, c.Faction, "creatures", c.Id + ".faction");
            Check(c.Tier is >= 1 and <= Building.DwellingLevelCount, "creatures", $"{c.Id}: tier is not representable.");
            Check(c.Info is not null && c.Info.Special is not null, "creatures", $"{c.Id}.info/special must not be null.");
            Check(c.VersionInfo is not null && c.VersionInfo.All(pair => Enum.IsDefined(pair.Key) && pair.Key != GameVersion.Unknown && pair.Key >= c.AvailableFrom && pair.Value is not null), "creatures", $"{c.Id}.versionInfo must name supported versions and non-null statistics.");
            foreach (var stats in c.VersionInfo!.Values.Prepend(c.Info!))
            {
                Check(stats.Special is not null, "creatures", $"{c.Id}.info/versionInfo.special must not be null.");
                Check(stats.Health > 0 && stats.Speed > 0 && stats.Growth >= 0 && stats.MinDamage >= 0 && stats.MaxDamage >= stats.MinDamage && stats.Gold >= 0 && stats.Shots >= 0, "creatures", $"{c.Id}.info/versionInfo contains invalid stats.");
                Check(
                    stats.Atk >= 0 && stats.Def >= 0 && stats.AiValue >= 0 && stats.FightValue >= 0 && stats.Wood >= 0 && stats.Ore >= 0 && stats.Mercury >= 0 && stats.Sulfur >= 0 && stats.Crystal >= 0 && stats.Gems >= 0,
                    "creatures",
                    $"{c.Id}.info/versionInfo contains a negative statistic or resource cost.");
                foreach (var ability in stats.Special!)
                {
                    Reference(abilities, ability, "creatures", c.Id + ".info/versionInfo.special");
                }
            }

            var selected = c.VersionInfo.GetValueOrDefault(version) ?? c.Info!;
            foreach (var ability in selected.Special)
            {
                Available(c, abilities[ability], "creatures", "info.special");
            }

            if (c.BaseCreature is not null)
            {
                Reference(creatures, c.BaseCreature, "creatures", c.Id + ".baseCreature");
                var previous = creatures[c.BaseCreature];
                Check(previous.Faction == c.Faction && previous.Tier == c.Tier, "creatures", $"{c.Id}.baseCreature must share its faction and tier.");
                Available(c, previous, "creatures", "baseCreature");
            }

            if (c.Dwelling is not null)
            {
                Reference(buildings, c.Dwelling, "creatures", c.Id + ".dwelling");
                var b = buildings[c.Dwelling];
                Available(c, b, "creatures", "dwelling");
                Check(
                    b.Faction == c.Faction && (b.Type == BuildingType.Dwelling1 + c.Tier - 1 || b.Type == BuildingType.UpgradedDwelling1 + c.Tier - 1
                    || b.Type is >= BuildingType.Special1 and <= BuildingType.Special4),
                    "creatures",
                    $"{c.Id}.dwelling does not match faction/tier or a supported additional dwelling slot.");
            }
        }

        Acyclic(creatures, c => c.BaseCreature is null ? [] : new[] { c.BaseCreature }, "creatures");
        foreach (var c in data.HeroClasses)
        {
            Reference(factions, c.Faction, "heroClasses", c.Id + ".faction");
            Check(Enum.IsDefined(c.Type), "heroClasses", $"{c.Id}.type is unsupported.");
            Available(c, factions[c.Faction], "heroClasses", "faction");
        }

        foreach (var s in data.Spells)
        {
            Reference(schools, s.School, "spells", s.Id + ".school");
            Available(s, schools[s.School], "spells", "school");
            Check(s.Code < Constants.Catalog.SpellCapacity && s.Level is >= 1 and <= Building.MageGuildLevelCount && Enum.IsDefined(s.Type), "spells", $"{s.Id}: level/code/type is not representable.");
        }

        foreach (var s in data.SecondarySkills)
        {
            Check(s.Code < Constants.Catalog.SkillCapacity && Enum.IsDefined(s.Role), "secondarySkills", $"{s.Id}: code/role is not representable.");
        }

        foreach (var role in Enum.GetValues<SkillRole>().Where(r => r != SkillRole.None))
        {
            One(data.SecondarySkills.Where(s => s.Role == role), "secondarySkills", role.ToString());
        }

        foreach (var m in data.WarMachines)
        {
            Check(
                Enum.IsDefined(m.Encoding) && Enum.IsDefined(m.Slot) && m.Slot == m.Encoding switch
                {
                    WarMachineEncoding.AmmoCart => WarMachineSlot.AmmoCart,
                    WarMachineEncoding.FirstAidTent => WarMachineSlot.FirstAidTent,
                    _ => WarMachineSlot.Artillery,
                },
                "warMachines",
                $"{m.Id}: unsupported encoding/slot.");
        }

        const ArtifactSlots allSlots = ArtifactSlots.Helmet | ArtifactSlots.Necklace | ArtifactSlots.Torso | ArtifactSlots.Feet | ArtifactSlots.Weapon | ArtifactSlots.Shield | ArtifactSlots.Cape | ArtifactSlots.Rings | ArtifactSlots.Misc;
        foreach (var a in data.Artifacts)
        {
            Check(Enum.IsDefined(a.Role) && Enum.IsDefined(a.Class) && a.Cost >= 0 && a.Description is not null && a.EffectInfo is not null, "artifacts", $"{a.Id}: invalid role/class/cost/description/effects.");
            Check((a.Slot & ~allSlots) == 0 && (a.BlockedSlots & ~allSlots) == 0 && (!a.Wearable || a.Slot != ArtifactSlots.None), "artifacts", $"{a.Id}: invalid equipment slots.");
            Check(a.Role != ArtifactRole.SpellBook || (a.Code == Constants.ByteOn && !a.Wearable && !a.Backpack && a.Slot == ArtifactSlots.None), "artifacts", $"{a.Id}: the spellBook role uses the dedicated zero book encoding, not an equipment/backpack slot.");
            Check(a.Role != ArtifactRole.Scroll || (a.Wearable && a.Backpack), "artifacts", $"{a.Id}: the scroll role must be available in equipment and backpack.");
            if (a.CombinesInto is not null)
            {
                Reference(artifacts, a.CombinesInto, "artifacts", a.Id + ".combinesInto");
            }

            var e = a.EffectInfo!;
            Check(e.Modifiers is not null && e.Modifiers.All(modifier => modifier is not null && Enum.IsDefined(modifier.Attribute)), "artifacts", $"{a.Id}.effectInfo.modifiers contains an unsupported/null attribute.");
            Check(e.Modifiers!.Select(modifier => modifier.Attribute).Distinct().Count() == e.Modifiers!.Length, "artifacts", $"{a.Id}.effectInfo.modifiers: an attribute must occur at most once.");
            Check(e.SpellGrants is not null && e.Other is not null, "artifacts", $"{a.Id}.effectInfo: spellGrants/other must not be null.");
            foreach (var g in e.SpellGrants!)
            {
                Check(g is not null && Enum.IsDefined(g.Kind) && g.Description is not null && g.Spells is not null, "artifacts", $"{a.Id}.spellGrants: unsupported/null grant.");
                Check(
                    g!.Kind switch
                    {
                        SpellGrantKind.Named => g.Spells!.Length > 0 && g.School is null && g.Level is null,
                        SpellGrantKind.School => g.School is not null && g.Spells!.Length == 0 && g.Level is null,
                        SpellGrantKind.Level => g.Level is >= 1 and <= Building.MageGuildLevelCount && g.School is null && g.Spells!.Length == 0,
                        _ => false,
                    },
                    "artifacts",
                    $"{a.Id}.spellGrants: incompatible parameters.");
                foreach (var id in g.Spells!)
                {
                    Reference(spells, id, "artifacts", a.Id + ".spellGrants.spells");
                }

                if (g.School is not null)
                {
                    Reference(schools, g.School, "artifacts", a.Id + ".spellGrants.school");
                }
            }
        }

        Acyclic(artifacts, a => a.CombinesInto is null ? [] : new[] { a.CombinesInto }, "artifacts");
        var metadata = data.MapMetadata;
        Check(
            metadata is not null && !string.IsNullOrWhiteSpace(metadata.MerchantName) && !string.IsNullOrWhiteSpace(metadata.DwellingName),
            "mapObjects",
            "Map-object names must not be null or empty.");
        Check(data.Mines is not null && data.Mines.All(m => m is not null), "mines", "Mine entries must not be null.");
        Check(data.VisitEffects is not null && data.VisitEffects.All(e => e is not null), "visitEffects", "Visit-effect entries must not be null.");
        var mines = data.Mines!;
        var visitEffects = data.VisitEffects!;
        Check(
            mines.Select(m => m.Kind).Distinct().Count() == mines.Length
            && mines.Select(m => m.Kind).Order().SequenceEqual(Enum.GetValues<Siniskalc.Heroes.Entities.MineKind>().Order()),
            "mines",
            "Each supported mine storage kind must occur exactly once.");
        One(mines.Where(m => (int)m.Kind == 0), "mines", "code 0 mine");
        foreach (var m in mines)
        {
            Check(!string.IsNullOrWhiteSpace(m.Name) && Enum.IsDefined(m.Resource) && m.DailyAmount >= 0, "mines", $"{m.Kind}: invalid name, resource or income.");
        }

        Check(
            visitEffects.Select(e => e.Effect).Distinct().Count() == visitEffects.Length
            && visitEffects.Select(e => e.Effect).Order().SequenceEqual(Enum.GetValues<VisitedBuildingEffects>().Where(e => e != VisitedBuildingEffects.None).Order()),
            "visitEffects",
            "Each supported visit-effect storage flag must occur exactly once.");
        foreach (var e in visitEffects)
        {
            Check(
                !string.IsNullOrWhiteSpace(e.Name) && !string.IsNullOrWhiteSpace(e.Description)
                && e.Morale is >= -Constants.Hero.MaxMoraleOrLuck and <= Constants.Hero.MaxMoraleOrLuck
                && e.Luck is >= -Constants.Hero.MaxMoraleOrLuck and <= Constants.Hero.MaxMoraleOrLuck
                && e.Movement >= 0
                && e.VersionMovement.All(pair => Enum.IsDefined(pair.Key) && pair.Key != GameVersion.Unknown && pair.Value >= 0),
                "visitEffects",
                $"{e.Effect}: invalid name, description or morale/luck/movement parameters.");
        }
    }
}
