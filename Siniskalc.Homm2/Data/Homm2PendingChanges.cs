namespace Siniskalc.Homm2.Data
{
    using System;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Models;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// Makes the pending changes of a hero, a player or a castle: a change and the way to put it back.
    /// </summary>
    public static class Homm2PendingChanges
    {
        public static PendingChange ForHero(string heroName, int heroIndex, Change change, Func<HeroModel, HeroModel> revert)
        {
            return ChunkPendingChange.For<HeroChunk, HeroModel>(Homm2ChunkKinds.Hero, heroName, heroIndex, change, revert);
        }

        public static PendingChange ForPlayer(string playerName, int playerIndex, Change change, Func<PlayerModel, PlayerModel> revert)
        {
            return ChunkPendingChange.For<PlayerChunk, PlayerModel>(Homm2ChunkKinds.Player, playerName, playerIndex, change, revert);
        }

        /// <summary>
        /// Makes the pending change of a mine, a shrine or a sphinx (the objects of the map).
        /// </summary>
        public static PendingChange ForMapObject<TModel>(ChunkKind kind, string name, int index, Change change, Func<TModel, TModel> revert)
            where TModel : class
        {
            return new PendingChange(name, change, kind, index, history => new MapObjectEditSession<TModel>(history, kind, index).Update(revert));
        }

        public static PendingChange ForObelisks(int index, Change change, Func<ObeliskVisits, ObeliskVisits> revert)
        {
            return new PendingChange("Obelisks", change, Homm2ChunkKinds.Obelisks, index, history => history.CreateObeliskSession(index).Update(revert));
        }

        public static PendingChange ForCastle(string castleName, int castleIndex, Change change, Func<CastleModel, CastleModel> revert)
        {
            return ChunkPendingChange.For<CastleChunk, CastleModel>(Homm2ChunkKinds.Castle, castleName, castleIndex, change, revert);
        }
    }
}
