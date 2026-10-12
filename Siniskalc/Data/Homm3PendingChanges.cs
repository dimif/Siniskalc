namespace Siniskalc.Data
{
    using System;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Models;
    using Siniskalc.Models;

    /// <summary>
    /// Creates the <see cref="PendingChange"/> of a Heroes of Might and Magic III hero, castle or player.
    /// </summary>
    public static class Homm3PendingChanges
    {
        public static PendingChange ForHero(string heroName, int heroIndex, HeroChange change)
        {
            return new PendingChange(heroName, change, Homm3ChunkKinds.Hero, heroIndex, history => history.CreateHeroSession(heroIndex).Update(change.Revert));
        }

        public static PendingChange ForCastle(string castleName, int castleIndex, Change change, Func<CastleModel, CastleModel> revert)
        {
            return new PendingChange(castleName, change, Homm3ChunkKinds.Castle, castleIndex, history => history.CreateCastleSession(castleIndex).Update(revert));
        }

        public static PendingChange ForObelisks(int index, Change change, Func<ObeliskVisits, ObeliskVisits> revert)
        {
            return new PendingChange("Obelisks", change, Homm3ChunkKinds.Obelisks, index, history => history.CreateObeliskSession(index).Update(revert));
        }

        public static PendingChange ForPlayer(string playerName, int playerIndex, Change change, Func<PlayerModel, PlayerModel> revert)
        {
            return new PendingChange(playerName, change, Homm3ChunkKinds.Player, playerIndex, history => history.CreatePlayerSession(playerIndex).Update(revert));
        }
    }
}
