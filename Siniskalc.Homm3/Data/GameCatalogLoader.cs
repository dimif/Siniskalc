namespace Siniskalc.Homm3.Data;

using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using Siniskalc.Core.Data;
using Siniskalc.Core.Runtime;
using Siniskalc.Homm3.Entities;

/// <summary>Reads all shipped sets at module construction; version selection never performs I/O.</summary>
public sealed class GameCatalogLoader
{
    private const string Root = @"GameData\Homm3\";
    private readonly IGameDefinitionReader reader;

    public GameCatalogLoader(IGameDefinitionReader reader) => this.reader = reader;

    public IReadOnlyDictionary<GameVersion, EntityCatalog> Load()
    {
        var manifest = this.reader.Read<CatalogManifest>(Root + "manifest.json");
        var mapMetadata = this.reader.Read<MapMetadataDefinition>(Root + "mapObjects.json");
        Require(manifest.SchemaVersion == 1, "manifest.json", "schemaVersion must be 1.");
        Require(manifest.Versions is not null && manifest.Sets is not null, "manifest.json", "versions and sets must not be null.");
        foreach (var (version, setId) in manifest.Versions!)
        {
            Require(
                Enum.IsDefined(version) && version != GameVersion.Unknown && !string.IsNullOrWhiteSpace(setId),
                "manifest.json", $"Version '{version}' must name a supported standard set.");
        }

        foreach (var (id, set) in manifest.Sets!)
        {
            Require(
                !string.IsNullOrWhiteSpace(id) && set is not null && !string.IsNullOrWhiteSpace(set.Directory),
                "manifest.json", $"Set '{id}' must have an identity and directory.");
            Require(
                !System.IO.Path.IsPathRooted(set!.Directory)
                && !set.Directory.Split('\\', '/').Any(part => part is "." or ".."),
                "manifest.json", $"Set '{id}' must use a directory within this game's data.");
            Require(
                set.Parent is null || !string.IsNullOrWhiteSpace(set.Parent),
                "manifest.json", $"Set '{id}' has an empty parent.");
        }

        var sets = new Dictionary<string, CatalogDefinitions>(StringComparer.Ordinal);
        var visiting = new HashSet<string>();
        CatalogDefinitions Resolve(string id)
        {
            if (sets.TryGetValue(id, out var existing))
            {
                return existing;
            }

            Require(manifest.Sets!.ContainsKey(id), "manifest.json", $"Unknown standard set '{id}'.");
            Require(visiting.Add(id), "manifest.json", $"Parent cycle at set '{id}'.");
            var set = manifest.Sets[id];
            Require(set is not null && !string.IsNullOrWhiteSpace(set.Directory), "manifest.json", $"Set '{id}' must have a directory.");
            var parent = set!.Parent is null ? null : Resolve(set.Parent);
            var sources = parent?.SourcePaths.ToDictionary(p => p.Key, p => p.Value, StringComparer.Ordinal) ?? new Dictionary<string, string>(StringComparer.Ordinal);
            T[] Read<T>(string file, T[]? inherited)
                where T : EntityDefinition
            {
                var path = set.Directory + "\\" + file + ".json";
                var entries = this.reader.Read<T[]>(Root + path);
                Require(entries is not null, path, "The entity array must not be null.");
                var result = (inherited ?? []).ToDictionary(d => d.Id, StringComparer.Ordinal);
                var local = new HashSet<string>(StringComparer.Ordinal);
                foreach (var entry in entries!)
                {
                    Require(entry is not null && !string.IsNullOrWhiteSpace(entry.Id), path, "Every record must have a nonempty id.");
                    Require(local.Add(entry!.Id), path, $"Duplicate id '{entry.Id}'.");
                    Require(entry.Replaces == result.ContainsKey(entry.Id), path, $"{entry.Id}: replacements must target an existing parent record; additions must have a new id.");
                    result[entry.Id] = entry;
                    sources[file + ":" + entry.Id] = path;
                }

                return result.Values.ToArray();
            }

            var result = new CatalogDefinitions(
                Read("factions", parent?.Factions), Read("abilities", parent?.Abilities),
                Read("buildings", parent?.Buildings), Read("creatures", parent?.Creatures),
                Read("spellSchools", parent?.SpellSchools), Read("spells", parent?.Spells),
                Read("heroClasses", parent?.HeroClasses), Read("secondarySkills", parent?.SecondarySkills),
                Read("warMachines", parent?.WarMachines), Read("artifacts", parent?.Artifacts),
                Read("visitedPlaces", parent?.VisitedPlaces), mapMetadata) { SourcePaths = sources };
            visiting.Remove(id);
            sets.Add(id, result);
            return result;
        }

        var catalogs = new Dictionary<GameVersion, EntityCatalog>();
        foreach (var version in Enum.GetValues<GameVersion>().Where(v => v != GameVersion.Unknown))
        {
            Require(manifest.Versions!.TryGetValue(version, out var set), "manifest.json", $"Missing version '{version}'.");
            var definitions = Resolve(set!);
            CatalogValidator.Validate(definitions, version, manifest.Sets![set!].Directory);
            catalogs.Add(version, new EntityCatalog(version, definitions));
        }

        Require(manifest.Versions!.Count == catalogs.Count && !manifest.Versions.ContainsKey(GameVersion.Unknown), "manifest.json", "Only the five supported standard versions may be mapped.");
        foreach (var set in manifest.Sets!.Keys)
        {
            CatalogValidator.Validate(Resolve(set), GameVersion.HornOfTheAbyss, manifest.Sets[set].Directory);
        }

        // Unknown is a detection state, not another editable set. Its historical empty picker availability is retained.
        catalogs.Add(GameVersion.Unknown, new EntityCatalog(GameVersion.Unknown, Resolve(manifest.Versions[GameVersion.RestorationOfErathia])));
        return catalogs.ToFrozenDictionary();
    }

    internal static void Require(bool condition, string path, string message)
    {
        if (!condition)
        {
            throw new GameDefinitionException(Root + path, "HoMM3: " + message);
        }
    }
}
