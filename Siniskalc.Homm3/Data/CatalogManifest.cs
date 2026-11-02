namespace Siniskalc.Homm3.Data;

using System.Collections.Generic;
using Siniskalc.Homm3.Entities;

public sealed record CatalogManifest
{
    /// <summary>
    /// Gets the version of the format of the manifest.
    /// </summary>
    public required int SchemaVersion { get; init; }

    /// <summary>
    /// Gets the set each version of the game uses.
    /// </summary>
    public required Dictionary<GameVersion, string> Versions { get; init; }

    /// <summary>
    /// Gets the sets of definitions by name.
    /// </summary>
    public required Dictionary<string, StandardSetDefinition> Sets { get; init; }
}
