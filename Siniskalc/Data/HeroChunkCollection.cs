namespace Siniskalc.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core.Data;
    using Siniskalc.Entities;
    using Siniskalc.Models;

    /// <summary>
    /// The heroes of a savefile.
    /// </summary>
    public sealed class HeroChunkCollection : ChunkCollection<HeroChunk>
    {
        private readonly IHeroDeserializer deserializer;
        private readonly IHeroDiffer differ;

        /// <param name="chunks">The heroes.</param>
        /// <param name="offsets">Where the hero fields are, for the savefile's game version.</param>
        /// <param name="deserializer">Reads a hero from a chunk; made for <paramref name="offsets"/>.</param>
        /// <param name="serializer">Writes a hero to a chunk; made for <paramref name="offsets"/>.</param>
        /// <param name="differ">Tells what differs between two versions of a hero.</param>
        public HeroChunkCollection(IEnumerable<HeroChunk> chunks, HeroOffsets offsets, IHeroDeserializer deserializer, IHeroSerializer serializer, IHeroDiffer differ)
            : base(chunks)
        {
            this.Offsets = offsets;
            this.deserializer = deserializer;
            this.Serializer = serializer;
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
        /// Gets what reads a hero from a chunk.
        /// </summary>
        public IHeroDeserializer Deserializer => this.deserializer;

        /// <summary>
        /// Gets what writes a hero to a chunk.
        /// </summary>
        public IHeroSerializer Serializer { get; }

        public override HeroChunkCollection WithChunk(int index, HeroChunk chunk) => new(this.Replace(index, chunk), this.Offsets, this.deserializer, this.Serializer, this.differ);

        /// <summary>
        /// Returns the index of the first hero with the given name, or -1.
        /// </summary>
        public int FindByName(string name) => this.IndexOf(c => string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase));

        /// <summary>
        /// Returns who owns each hero, in the order of the collection. Reads the owner byte only.
        /// </summary>
        /// <summary>
        /// Returns the hero at <paramref name="index"/> as a model.
        /// </summary>
        public HeroModel GetModel(int index) => this.deserializer.Deserialize(this[index]);

        public IReadOnlyList<PlayerColor?> GetOwners() => this.Select(this.deserializer.DeserializeOwner).ToList();

        /// <summary>
        /// Returns a short summary of every hero. Reads only what the summary needs, so it is cheap enough to run for every hero.
        /// </summary>
        public IReadOnlyList<HeroSummary> GetSummaries()
        {
            var summaries = new List<HeroSummary>();

            for (var i = 0; i < this.Count; i++)
            {
                summaries.Add(this.Summarize(i));
            }

            return summaries;
        }

        /// <summary>
        /// Returns the index of the hero with the given id (see <see cref="HeroModel.HeroId"/>), or -1 if the savefile has no such hero.
        /// </summary>
        public int IndexOfId(int heroId) => this.IndexOf(c => this.deserializer.DeserializeHeroId(c) == heroId);

        /// <summary>
        /// Returns the summary of the hero with the given id (see <see cref="HeroModel.HeroId"/>), or null if the savefile has no such hero.
        /// </summary>
        public HeroSummary? FindById(int heroId)
        {
            var index = this.IndexOfId(heroId);
            return index < 0 ? null : this.Summarize(index);
        }

        /// <summary>
        /// Returns the summary of the hero at <paramref name="index"/>.
        /// </summary>
        public HeroSummary GetSummary(int index) => this.Summarize(index);

        protected override IEnumerable<PendingChange> DiffChunk(int index, HeroChunk original, HeroChunk current)
        {
            var before = this.deserializer.Deserialize(original);
            var after = this.deserializer.Deserialize(current);
            return this.differ.Diff(before, after).Select(change => Homm3PendingChanges.ForHero(after.Name, index, change));
        }

        private HeroSummary Summarize(int index)
        {
            var chunk = this[index];
            return new HeroSummary(
                index,
                this.deserializer.DeserializeHeroId(chunk),
                chunk.Name,
                this.deserializer.DeserializeOwner(chunk),
                this.deserializer.DeserializeClass(chunk),
                this.deserializer.DeserializeLevel(chunk).Level);
        }
    }
}
