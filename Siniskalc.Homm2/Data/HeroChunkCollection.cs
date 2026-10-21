namespace Siniskalc.Homm2.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Data;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// The 54 heroes of a savegame. The index of a hero is its number.
    /// </summary>
    public sealed class HeroChunkCollection : ModelChunkCollection<HeroChunk, HeroModel, IHeroDeserializer, IHeroSerializer>, IHeroSummaries<PlayerColor>
    {
        private readonly IHeroDiffer differ;

        public HeroChunkCollection(IEnumerable<HeroChunk> chunks, IHeroDeserializer deserializer, IHeroSerializer serializer, IHeroDiffer differ)
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
        public IReadOnlyList<HeroSummary<PlayerColor>> GetSummaries() => this.GetModels().Select(hero => hero.Summary).ToList();

        /// <summary>
        /// Returns the summary of the hero with the given number (which is its place in the table), or null if there is no such hero.
        /// </summary>
        public HeroSummary<PlayerColor>? FindById(int heroId) => heroId >= 0 && heroId < this.Count ? this.GetModel(heroId).Summary : null;
    }
}
