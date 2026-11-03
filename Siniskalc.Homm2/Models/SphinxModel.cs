namespace Siniskalc.Homm2.Models
{
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Entities;

    /// <summary>
    /// A sphinx of the map: it asks a riddle and gives a reward for any of its answers, once. Its block of the savegame has a fixed size,
    /// so the answers and the riddle can only be changed within the room they have.
    /// </summary>
    /// <param name="Index">The place of the sphinx in the list of the map's sphinxes.</param>
    /// <param name="Position">The tile a hero enters the sphinx by, or <c>null</c> when it was not found.</param>
    /// <param name="IsActive">Whether the riddle has not been answered (the game sets this to 0 when it is).</param>
    /// <param name="Reward">What an answer gives: wood, mercury, ore, sulfur, crystal, gems and gold.</param>
    /// <param name="ArtifactCode">The artifact an answer gives, 96 for none.</param>
    /// <param name="Answers">The answers it accepts (up to 8, 12 characters each).</param>
    /// <param name="Riddle">The question.</param>
    /// <param name="RiddleCapacity">How many characters the riddle can have in the block.</param>
    public sealed record SphinxModel(
        int Index,
        MapPosition? Position,
        bool IsActive,
        IReadOnlyList<int> Reward,
        int ArtifactCode,
        IReadOnlyList<string> Answers,
        string Riddle,
        int RiddleCapacity,
        GameCatalog Catalog)
    {
        /// <summary>
        /// The artifact code that stands for "no artifact".
        /// </summary>
        public const int NoArtifact = 96;

        /// <summary>
        /// Gets the name the editor shows for the sphinx, which says its place when it is known.
        /// </summary>
        public string Name => this.Position is { } at ? this.Catalog.MapObjects.Single(o => o.Role == MapObjectRole.Sphinx).FormatName(at.X, at.Y) : this.Catalog.MapObjects.Single(o => o.Role == MapObjectRole.SphinxUnlocated).FormatName(this.Index + 1);

        /// <inheritdoc/>
        public bool Equals(SphinxModel? other) =>
            other is not null
            && this.Index == other.Index && this.Position == other.Position && this.IsActive == other.IsActive && this.ArtifactCode == other.ArtifactCode
            && this.Riddle == other.Riddle && this.RiddleCapacity == other.RiddleCapacity
            && System.Linq.Enumerable.SequenceEqual(this.Reward, other.Reward) && System.Linq.Enumerable.SequenceEqual(this.Answers, other.Answers);

        /// <inheritdoc/>
        public override int GetHashCode() => System.HashCode.Combine(this.Index, this.IsActive, this.ArtifactCode, this.Riddle);
    }
}
