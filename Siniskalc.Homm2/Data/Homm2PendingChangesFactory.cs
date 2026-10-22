namespace Siniskalc.Homm2.Data
{
    using System;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Models;
    using Siniskalc.Heroes.Data;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// Makes the pending changes of a hero, a player, a castle or the obelisks: a change and the way to put it back.
    /// </summary>
    public sealed class Homm2PendingChangesFactory : IPendingChangesFactory<HeroModel, PlayerModel, CastleModel, ObeliskVisits<PlayerColor>>
    {
        /// <inheritdoc/>
        public PendingChange ForHero(string heroName, int heroIndex, Change change, Func<HeroModel, HeroModel> revert)
        {
            return ChunkPendingChange.For<HeroChunk, HeroModel>(Homm2ChunkKinds.Hero, heroName, heroIndex, change, revert);
        }

        /// <inheritdoc/>
        public PendingChange ForPlayer(string playerName, int playerIndex, Change change, Func<PlayerModel, PlayerModel> revert)
        {
            return ChunkPendingChange.For<PlayerChunk, PlayerModel>(Homm2ChunkKinds.Player, playerName, playerIndex, change, revert);
        }

        /// <inheritdoc/>
        public PendingChange ForCastle(string castleName, int castleIndex, Change change, Func<CastleModel, CastleModel> revert)
        {
            return ChunkPendingChange.For<CastleChunk, CastleModel>(Homm2ChunkKinds.Castle, castleName, castleIndex, change, revert);
        }

        /// <inheritdoc/>
        public PendingChange ForObelisks(int index, Change change, Func<ObeliskVisits<PlayerColor>, ObeliskVisits<PlayerColor>> revert)
        {
            return new PendingChange("Obelisks", change, Homm2ChunkKinds.Obelisks, index, history => history.CreateObeliskSession(index).Update(revert));
        }
    }
}
