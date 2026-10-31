namespace Siniskalc.Homm3.Data
{
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Data;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// The heroes of a savefile.
    /// </summary>
    public sealed class HeroChunkCollection : ModelChunkCollection<HeroChunk, HeroModel, IHeroDeserializer, ISerializer<HeroChunk, HeroModel>>, IHeroSummaries<PlayerColor>
    {
        private readonly IDiffer<HeroModel> differ;

        /// <param name="chunks">The heroes.</param>
        /// <param name="offsets">Where the hero fields are, for the savefile's game version.</param>
        /// <param name="deserializer">Reads a hero from a chunk; made for <paramref name="offsets"/>.</param>
        /// <param name="serializer">Writes a hero to a chunk; made for <paramref name="offsets"/>.</param>
        /// <param name="differ">Tells what differs between two versions of a hero.</param>
        public HeroChunkCollection(IEnumerable<HeroChunk> chunks, HeroOffsets offsets, IHeroDeserializer deserializer, ISerializer<HeroChunk, HeroModel> serializer, IDiffer<HeroModel> differ)
            : base(chunks, deserializer, serializer, differ.Diff)
        {
            this.Offsets = offsets;
            this.differ = differ;
        }

        /// <summary>
        /// Gets where the hero fields are in a chunk, for the savefile's game version.
        /// </summary>
        public HeroOffsets Offsets { get; }

        /// <summary>
        /// Gets whether the savefile's game version has the "asleep" flag of a hero found (Shadow of Death and Horn of the Abyss).
        /// </summary>
        public bool SupportsAsleep => this.Offsets.AsleepOffset != null;

        /// <summary>
        /// Gets whether the savefile's game version has the "moved today" flag of a hero found (Horn of the Abyss).
        /// </summary>
        public bool SupportsMovedToday => this.Offsets.AreDayFlagsVerified;

        /// <inheritdoc/>
        public override HeroChunkCollection WithChunk(int index, HeroChunk chunk) => new(this.Replace(index, chunk), this.Offsets, this.Deserializer, this.Serializer, this.differ);

        /// <summary>
        /// Returns who owns each hero, in the order of the collection. Reads the owner byte only.
        /// </summary>
        public IReadOnlyList<PlayerColor> GetOwners() => this.Select(this.Deserializer.DeserializeOwner).ToList();

        /// <summary>
        /// Returns a short summary of every hero. Reads only what the summary needs, so it is cheap enough to run for every hero.
        /// </summary>
        public IReadOnlyList<HeroSummary<PlayerColor>> GetSummaries() => Enumerable.Range(0, this.Count).Select(this.GetSummary).ToList();

        /// <inheritdoc/>
        public int IndexOfId(int heroId) => this.IndexOf(c => this.Deserializer.DeserializeHeroId(c) == heroId);

        /// <inheritdoc/>
        public HeroSummary<PlayerColor>? FindById(int heroId)
        {
            var index = this.IndexOfId(heroId);
            return index < 0 ? null : this.GetSummary(index);
        }

        /// <summary>
        /// Returns the summary of the hero at <paramref name="index"/>.
        /// </summary>
        public HeroSummary<PlayerColor> GetSummary(int index)
        {
            var chunk = this[index];
            return new HeroSummary<PlayerColor>(
                index,
                this.Deserializer.DeserializeHeroId(chunk),
                chunk.Name,
                this.Deserializer.DeserializeOwner(chunk),
                this.Deserializer.DeserializeClass(chunk).Name,
                this.Deserializer.DeserializeLevel(chunk).Level);
        }
    }
}
