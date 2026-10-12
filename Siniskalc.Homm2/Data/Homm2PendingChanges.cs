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
        public static PendingChange ForHero(string heroName, int heroIndex, Change change, Func<Homm2HeroModel, Homm2HeroModel> revert)
        {
            return new PendingChange(heroName, change, Homm2ChunkKinds.Hero, heroIndex, history => history.CreateHeroSession(heroIndex).Update(revert));
        }

        public static PendingChange ForPlayer(string playerName, int playerIndex, Change change, Func<Homm2PlayerModel, Homm2PlayerModel> revert)
        {
            return new PendingChange(playerName, change, Homm2ChunkKinds.Player, playerIndex, history => history.CreatePlayerSession(playerIndex).Update(revert));
        }

        public static PendingChange ForCastle(string castleName, int castleIndex, Change change, Func<Homm2CastleModel, Homm2CastleModel> revert)
        {
            return new PendingChange(castleName, change, Homm2ChunkKinds.Castle, castleIndex, history => history.CreateCastleSession(castleIndex).Update(revert));
        }
    }
}
