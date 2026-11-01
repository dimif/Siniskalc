namespace Siniskalc.Homm3.Data;

using System.Collections.Generic;
using Siniskalc.Homm3.Entities;

public sealed record CatalogManifest
{
    public required int SchemaVersion { get; init; }
    public required Dictionary<GameVersion, string> Versions { get; init; }

    public required Dictionary<string, StandardSetDefinition> Sets { get; init; }
}
