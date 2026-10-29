namespace Siniskalc.Homm2.Data
{
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// Edits one castle of a savegame through its history.
    /// </summary>
    public sealed class CastleEditSession : EditSession<CastleChunk, CastleModel>
    {
        public CastleEditSession(SaveDocumentHistory history, int castleIndex)
            : base(history, Homm2ChunkKinds.Castle, castleIndex, history.Current.Castles().Deserializer, history.Current.Castles().Serializer)
        {
        }

        public int CastleIndex => this.Index;

        /// <summary>
        /// Gives the castle to a player, or to nobody, in one step of the history. The castle, the list of castles of the player that had it and
        /// the list of the player that has it now change together, as they do in the game. Does nothing when the owner is the same.
        /// </summary>
        /// <exception cref="GameRuleException">The new owner has the most castles the list holds.</exception>
        public void ChangeOwner(PlayerColor owner)
        {
            var document = this.History.Current;
            var castles = document.Castles();
            var players = document.Players();
            var castle = this.Current;
            if (castle.Owner == owner)
            {
                return;
            }

            // The new list is made before anything is written, so a full list leaves the savegame as it was.
            var edits = new List<(int Player, PlayerModel Model)>();
            if (!castle.Owner.IsNeutral)
            {
                edits.Add((castle.Owner.Code, players.GetModel(castle.Owner.Code).WithoutCastle(this.CastleIndex)));
            }

            if (!owner.IsNeutral)
            {
                edits.Add((owner.Code, players.GetModel(owner.Code).WithCastle(this.CastleIndex)));
            }

            var updated = document.WithChunk(Homm2ChunkKinds.Castle, this.CastleIndex, castles.Serializer.Serialize(castles[this.CastleIndex], castle with { Owner = owner }));
            foreach (var (player, model) in edits)
            {
                updated = updated.WithChunk(Homm2ChunkKinds.Player, player, players.Serializer.Serialize(players[player], model));
            }

            this.History.Apply(updated);
        }

        /// <summary>
        /// Puts the owner of the castle back to what the savegame had, in one step of the history, with the lists of castles of the players and
        /// their days left to own a castle (<see cref="PlayerModel.WithCastleAsIn"/>). It is how the change of the owner is reverted.
        /// </summary>
        /// <exception cref="GameRuleException">The castle goes back to a list of castles that has become full.</exception>
        public void RestoreOwner()
        {
            var updated = WithOwnerAsIn(this.History.Current, this.History.Original, this.CastleIndex);
            if (!ReferenceEquals(updated, this.History.Current))
            {
                this.History.Apply(updated);
            }
        }

        /// <summary>
        /// Returns the document with the owner of the castle as it is in <paramref name="original"/>, in the castle and in the lists of castles
        /// of the players, with their days left. The new records are all made before any is written, so a refused change leaves the document as it was.
        /// </summary>
        /// <exception cref="GameRuleException">The castle goes back to a list of castles that has become full.</exception>
        internal static SaveDocument WithOwnerAsIn(SaveDocument document, SaveDocument original, int castleIndex)
        {
            var castles = document.Castles();
            var castle = castles.GetModel(castleIndex);
            var originalCastle = original.Castles().GetModel(castleIndex);

            var players = document.Players();
            var originalPlayers = original.Players();
            var edits = new List<(int Player, PlayerModel Model)>();
            for (var index = 0; index < players.Count && index < originalPlayers.Count; index++)
            {
                var player = players.GetModel(index);
                var originalPlayer = originalPlayers.GetModel(index);
                if (player.CastleIds.Contains(castleIndex) || originalPlayer.CastleIds.Contains(castleIndex))
                {
                    var restored = player.WithCastleAsIn(castleIndex, originalPlayer);
                    if (restored != player)
                    {
                        edits.Add((index, restored));
                    }
                }
            }

            var updated = castle.Owner == originalCastle.Owner
                ? document
                : document.WithChunk(Homm2ChunkKinds.Castle, castleIndex, castles.Serializer.Serialize(castles[castleIndex], castle with { Owner = originalCastle.Owner }));
            foreach (var (player, model) in edits)
            {
                updated = updated.WithChunk(Homm2ChunkKinds.Player, player, players.Serializer.Serialize(players[player], model));
            }

            return updated;
        }
    }
}
