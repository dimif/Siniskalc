namespace Siniskalc.Data
{
    using System;
    using Siniskalc.Core.Data;

    /// <summary>
    /// Creates the byte edit targets of heroes, players and castles with the region providers they describe their bytes with.
    /// </summary>
    public sealed class ByteEditTargetFactory : IByteEditTargetFactory
    {
        private readonly IHeroRegionProvider heroRegions;
        private readonly IPlayerRegionProvider playerRegions;
        private readonly ICastleRegionProvider castleRegions;

        public ByteEditTargetFactory(IHeroRegionProvider heroRegions, IPlayerRegionProvider playerRegions, ICastleRegionProvider castleRegions)
        {
            this.heroRegions = heroRegions;
            this.playerRegions = playerRegions;
            this.castleRegions = castleRegions;
        }

        public IByteEditTarget ForHero(HeroEditSession session) => new HeroByteEditTarget(session, this.heroRegions);

        public IByteEditTarget ForPlayer(PlayerEditSession session) => new PlayerByteEditTarget(session, this.playerRegions);

        public IByteEditTarget ForCastle(SaveDocumentHistory history, int castleIndex) => new CastleByteEditTarget(history, castleIndex, this.castleRegions);
    }
}
