namespace Siniskalc.Homm3.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core.Data;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// The players of a savefile, in colour order. Empty for savefile versions whose players are not known.
    /// </summary>
    public sealed class PlayerChunkCollection : ModelChunkCollection<PlayerChunk, PlayerModel, IPlayerDeserializer, ISerializer<PlayerChunk, PlayerModel>>
    {
        private readonly IPlayerDiffer differ;
        private readonly Func<int, string> heroName;

        /// <param name="heroName">Gives the name of the hero with an id, so that a change of the heroes of the tavern can name them.</param>
        public PlayerChunkCollection(IEnumerable<PlayerChunk> chunks, IPlayerDeserializer deserializer, ISerializer<PlayerChunk, PlayerModel> serializer, IPlayerDiffer differ, Func<int, string> heroName)
            : base(chunks, deserializer, serializer, (index, before, after) => differ.Diff(index, before, after, heroName))
        {
            this.differ = differ;
            this.heroName = heroName;
        }

        /// <inheritdoc/>
        public override PlayerChunkCollection WithChunk(int index, PlayerChunk chunk) => new(this.Replace(index, chunk), this.Deserializer, this.Serializer, this.differ, this.heroName);

        /// <summary>
        /// Returns the colour of each player, in the order of the collection.
        /// </summary>
        public IReadOnlyList<PlayerColor> GetColors() => this.Select(this.Deserializer.DeserializeColor).ToList();

        /// <summary>
        /// Returns the colour of the player at <paramref name="index"/>.
        /// </summary>
        public PlayerColor GetColor(int index) => this.Deserializer.DeserializeColor(this[index]);

        /// <summary>
        /// Returns the index of the player with the given colour, or -1 if the savefile has no such player.
        /// </summary>
        public int FindByColor(PlayerColor color) => this.IndexOf(c => this.Deserializer.DeserializeColor(c) == color);
    }
}
