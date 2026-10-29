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

        public ByteEditTargetFactory(IHeroRegionProvider heroRegions, IPlayerRegionProvider playerRegions, ICastleRegionProvider castleRegions, IMineRegionProvider mineRegions, IDwellingRegionProvider dwellingRegions)
        {
            this.mineRegions = mineRegions;
            this.dwellingRegions = dwellingRegions;
            this.heroRegions = heroRegions;
            this.playerRegions = playerRegions;
            this.castleRegions = castleRegions;
        }

        public IByteEditTarget ForHero(HeroEditSession session) => new HeroByteEditTarget(session, this.heroRegions);

        public IByteEditTarget ForPlayer(PlayerEditSession session) => new PlayerByteEditTarget(session, this.playerRegions);

        public IByteEditTarget ForCastle(CastleEditSession session) => new CastleByteEditTarget(session, this.castleRegions);

        public IByteEditTarget ForMine(SaveDocumentHistory history, int mineIndex) => new MineByteEditTarget(history, mineIndex, this.mineRegions);

        public IByteEditTarget ForDwelling(SaveDocumentHistory history, int dwellingIndex) => new DwellingByteEditTarget(history, dwellingIndex, this.dwellingRegions);
    }
}
