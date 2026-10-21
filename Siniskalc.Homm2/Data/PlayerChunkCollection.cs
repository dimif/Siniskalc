namespace Siniskalc.Homm2.Data
{
    using System.Collections.Generic;
    using Siniskalc.Core.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// The six players of a savegame, one for each colour. The index of a player is its colour.
    /// </summary>
    public sealed class PlayerChunkCollection : ModelChunkCollection<PlayerChunk, PlayerModel, IPlayerDeserializer, IPlayerSerializer>
    {
        private readonly IPlayerDiffer differ;

        public PlayerChunkCollection(IEnumerable<PlayerChunk> chunks, IPlayerDeserializer deserializer, IPlayerSerializer serializer, IPlayerDiffer differ)
            : base(chunks, deserializer, serializer, differ.Diff)
        {
            this.differ = differ;
        }

        /// <inheritdoc/>
        public override PlayerChunkCollection WithChunk(int index, PlayerChunk chunk) => new(this.Replace(index, chunk), this.Deserializer, this.Serializer, this.differ);

        public PlayerColor GetColor(int index) => PlayerColor.All[index];

        public int FindByColor(PlayerColor color) => color.Code;
    }
}
