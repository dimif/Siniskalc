namespace Siniskalc.Homm3.Data
{
    using Siniskalc.Core.Data;

    /// <summary>
    /// Creates the byte edit targets of heroes, players, castles, mines and dwellings with the region providers they describe their bytes with.
    /// </summary>
    public sealed class ByteEditTargetFactory : IByteEditTargetFactory
    {
        private readonly IHeroRegionProvider heroRegions;
        private readonly IPlayerRegionProvider playerRegions;
        private readonly ICastleRegionProvider castleRegions;
        private readonly IMineRegionProvider mineRegions;
        private readonly IDwellingRegionProvider dwellingRegions;
        private readonly CreatureBankRegionProvider bankRegions;

        /// <summary>
        /// Initializes a new instance of the <see cref="ByteEditTargetFactory"/> class.
        /// </summary>
        /// <param name="heroRegions">The hero regions.</param>
        /// <param name="playerRegions">The player regions.</param>
        /// <param name="castleRegions">The castle regions.</param>
        /// <param name="mineRegions">The mine regions.</param>
        /// <param name="dwellingRegions">The dwelling regions.</param>
        public ByteEditTargetFactory(IHeroRegionProvider heroRegions, IPlayerRegionProvider playerRegions, ICastleRegionProvider castleRegions, IMineRegionProvider mineRegions, IDwellingRegionProvider dwellingRegions, CreatureBankRegionProvider bankRegions)
        {
            this.bankRegions = bankRegions;
            this.mineRegions = mineRegions;
            this.dwellingRegions = dwellingRegions;
            this.heroRegions = heroRegions;
            this.playerRegions = playerRegions;
            this.castleRegions = castleRegions;
        }

        /// <inheritdoc/>
        public IByteEditTarget ForHero(HeroEditSession session) => new HeroByteEditTarget(session, this.heroRegions);

        /// <inheritdoc/>
        public IByteEditTarget ForPlayer(PlayerEditSession session) => new PlayerByteEditTarget(session, this.playerRegions);

        /// <inheritdoc/>
        public IByteEditTarget ForCastle(CastleEditSession session) => new CastleByteEditTarget(session, this.castleRegions);

        /// <inheritdoc/>
        public IByteEditTarget ForMine(SaveDocumentHistory history, int mineIndex) => new MineByteEditTarget(history, mineIndex, this.mineRegions);

        /// <inheritdoc/>
        public IByteEditTarget ForDwelling(SaveDocumentHistory history, int dwellingIndex) => new DwellingByteEditTarget(history, dwellingIndex, this.dwellingRegions);

        /// <inheritdoc/>
        public IByteEditTarget ForCreatureBank(SaveDocumentHistory history, int index)
        {
            var session = new EditSession<CreatureBankChunk, Models.CreatureBankModel>(history, Homm3ChunkKinds.CreatureBanks, index);
            return new ChunkByteEditTarget<CreatureBankChunk, Models.CreatureBankModel>(session, this.bankRegions.GetRegions(session.Chunk));
        }
    }
}
