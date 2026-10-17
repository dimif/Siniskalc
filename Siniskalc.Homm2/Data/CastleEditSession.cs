namespace Siniskalc.Homm2.Data
{
    using System;
    using System.Collections.Generic;
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
        public void ChangeOwner(Color? owner)
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
            if (castle.Owner is { } previous)
            {
                edits.Add(((int)previous, players.GetModel((int)previous).WithoutCastle(this.CastleIndex)));
            }

            if (owner is { } next)
            {
                edits.Add(((int)next, players.GetModel((int)next).WithCastle(this.CastleIndex)));
            }

            var updated = document.WithChunk(Homm2ChunkKinds.Castle, this.CastleIndex, castles.Serializer.Serialize(castles[this.CastleIndex], castle with { Owner = owner }));
            foreach (var (player, model) in edits)
            {
                updated = updated.WithChunk(Homm2ChunkKinds.Player, player, players.Serializer.Serialize(players[player], model));
            }

            this.History.Apply(updated);
        }
    }
}
