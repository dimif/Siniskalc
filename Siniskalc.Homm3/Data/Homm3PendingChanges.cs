namespace Siniskalc.Homm3.Data
{
    using System;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Models;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// Creates the <see cref="PendingChange"/> of a Heroes of Might and Magic III hero, castle or player.
    /// </summary>
    public static class Homm3PendingChanges
    {
        public static PendingChange ForHero(string heroName, int heroIndex, Change change, Func<HeroModel, HeroModel> revert)
        {
            return ChunkPendingChange.For<HeroChunk, HeroModel>(Homm3ChunkKinds.Hero, heroName, heroIndex, change, revert);
        }

        public static PendingChange ForCastle(string castleName, int castleIndex, Change change, Func<CastleModel, CastleModel> revert)
        {
            return ChunkPendingChange.For<CastleChunk, CastleModel>(Homm3ChunkKinds.Castle, castleName, castleIndex, change, revert);
        }

        public static PendingChange ForObelisks(int index, Change change, Func<ObeliskVisits, ObeliskVisits> revert)
        {
            return new PendingChange("Obelisks", change, Homm3ChunkKinds.Obelisks, index, history => history.CreateObeliskSession(index).Update(revert));
        }

        public static PendingChange ForPlayer(string playerName, int playerIndex, Change change, Func<PlayerModel, PlayerModel> revert)
        {
            return ChunkPendingChange.For<PlayerChunk, PlayerModel>(Homm3ChunkKinds.Player, playerName, playerIndex, change, revert);
        }
    }
}
