namespace Siniskalc.Homm2.Data
{
    using Siniskalc.Heroes.Data;
    using Siniskalc.Homm2.Entities;

    /// <summary>
    /// Resolves creature codes using the document's catalog and its explicitly declared safe fallback.
    /// </summary>
    public sealed class CreatureCodes : ICreatureCodes<Creature>
    {
        private readonly GameCatalog catalog;

        public CreatureCodes(GameCatalog catalog)
        {
            this.catalog = catalog;
        }

        /// <inheritdoc/>
        public Creature Fallback => this.catalog.FallbackCreature;

        /// <inheritdoc/>
        public Creature? Find(uint code) => code > byte.MaxValue ? null : this.catalog.Creatures.Find((byte)code);
    }
}
