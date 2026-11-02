namespace Siniskalc.Homm2.Data
{
    using System.Collections.Generic;
    using Siniskalc.Homm2.Entities;

    /// <summary>
    /// The manifest of the definitions of the game (<c>manifest.json</c>): which set of definitions each version of the game uses, and the sets.
    /// </summary>
    public sealed record DefinitionManifest
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
        public required Dictionary<string, DefinitionSet> Sets { get; init; }
    }
}
