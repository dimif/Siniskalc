namespace Siniskalc.Homm2.Data
{
    using System.Collections.Generic;
    using Siniskalc.Homm2.Entities;

    public sealed record DefinitionManifest
    {
        public required int SchemaVersion { get; init; }

        public required Dictionary<GameVersion, string> Versions { get; init; }

        public required Dictionary<string, DefinitionSet> Sets { get; init; }
    }
}
