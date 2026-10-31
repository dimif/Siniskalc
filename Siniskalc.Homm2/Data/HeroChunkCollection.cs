namespace Siniskalc.Homm2.Data
{
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Data;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// The 54 heroes of a savegame. References to heroes use their table indexes, not the scenario portrait byte.
    /// </summary>
    public sealed class HeroChunkCollection : ModelChunkCollection<HeroChunk, HeroModel, IHeroDeserializer, ISerializer<HeroChunk, HeroModel>>, IHeroSummaries<PlayerColor>
    {
        private readonly IDiffer<HeroModel> differ;

        public HeroChunkCollection(IEnumerable<HeroChunk> chunks, IHeroDeserializer deserializer, ISerializer<HeroChunk, HeroModel> serializer, IDiffer<HeroModel> differ)
            : base(chunks, deserializer, serializer, differ.Diff)
        {
            this.differ = differ;
        }

        /// <inheritdoc/>
        public override HeroChunkCollection WithChunk(int index, HeroChunk chunk) => new(this.Replace(index, chunk), this.Deserializer, this.Serializer, this.differ);

        /// <summary>
        /// Returns the player each hero serves (<c>null</c> for the ones that serve nobody), by hero number.
        /// </summary>
        public IReadOnlyList<PlayerColor> GetOwners() => this.Select(this.Deserializer.DeserializeOwner).ToList();

        /// <summary>
        /// Returns a short summary of every hero, by hero number.
        /// </summary>
        public IReadOnlyList<HeroSummary<PlayerColor>> GetSummaries() => this.GetModels().Select((hero, index) => hero.Summary with { Index = index, HeroId = index }).ToList();

        /// <inheritdoc/>
        public int IndexOfId(int heroId) => heroId >= 0 && heroId < this.Count ? heroId : -1;

        /// <inheritdoc/>
        public HeroSummary<PlayerColor>? FindById(int heroId)
        {
            var index = this.IndexOfId(heroId);
            return index < 0 ? null : this.GetModel(index).Summary with { Index = index, HeroId = index };
        }
    }
}
