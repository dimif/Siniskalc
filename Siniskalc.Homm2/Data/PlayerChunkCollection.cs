namespace Siniskalc.Homm2.Data
{
    using System;
    using System.Collections.Generic;
    using Siniskalc.Core.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// The six players of a savegame, one for each colour. The index of a player is its colour.
    /// </summary>
    public sealed class PlayerChunkCollection : ModelChunkCollection<PlayerChunk, PlayerModel, IDeserializer<PlayerChunk, PlayerModel>, ISerializer<PlayerChunk, PlayerModel>>
    {
        private readonly IPlayerDiffer differ;
        private readonly Func<int, string> heroName;

        public PlayerChunkCollection(IEnumerable<PlayerChunk> chunks, IDeserializer<PlayerChunk, PlayerModel> deserializer, ISerializer<PlayerChunk, PlayerModel> serializer, IPlayerDiffer differ, Func<int, string> heroName)
            : base(chunks, deserializer, serializer, (index, before, after) => differ.Diff(index, before, after, heroName))
        {
            this.differ = differ;
            this.heroName = heroName;
        }

        /// <inheritdoc/>
        public override PlayerChunkCollection WithChunk(int index, PlayerChunk chunk) => new(this.Replace(index, chunk), this.Deserializer, this.Serializer, this.differ, this.heroName);

        public PlayerColor GetColor(int index) => PlayerColor.All[index];

        public int FindByColor(PlayerColor color) => color.Code;
    }
}
