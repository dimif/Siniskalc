namespace Siniskalc.Homm3.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core.Data;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// The players of a savefile, in colour order. Empty for savefile versions whose players are not known. Besides its model, the record of
    /// a player keeps the list of heroes it has (<see cref="PlayerLists"/>), which follows the owners of the heroes.
    /// </summary>
    public sealed class PlayerChunkCollection : ModelChunkCollection<PlayerChunk, PlayerModel, IPlayerDeserializer, ISerializer<PlayerChunk, PlayerModel>>
    {
        private readonly IPlayerDiffer differ;
        private readonly Func<int, string> heroName;

        /// <param name="chunks">The records of the players, in colour order.</param>
        /// <param name="deserializer">Reads the model of a player.</param>
        /// <param name="serializer">Writes the model of a player.</param>
        /// <param name="listsDeserializer">Reads the list of heroes of a player.</param>
        /// <param name="listsSerializer">Writes the list of heroes of a player.</param>
        /// <param name="differ">Lists what an edit changed in a player.</param>
        /// <param name="heroName">Gives the name of the hero with an id, so that a change of the heroes of the tavern can name them.</param>
        public PlayerChunkCollection(
            IEnumerable<PlayerChunk> chunks,
            IPlayerDeserializer deserializer,
            ISerializer<PlayerChunk, PlayerModel> serializer,
            IDeserializer<PlayerChunk, PlayerLists> listsDeserializer,
            ISerializer<PlayerChunk, PlayerLists> listsSerializer,
            IPlayerDiffer differ,
            Func<int, string> heroName)
            : base(chunks, deserializer, serializer, (index, before, after) => differ.Diff(index, before, after, heroName))
        {
            this.ListsDeserializer = listsDeserializer;
            this.ListsSerializer = listsSerializer;
            this.differ = differ;
            this.heroName = heroName;
        }

        /// <summary>
        /// Gets the reader of the list of heroes of a player.
        /// </summary>
        public IDeserializer<PlayerChunk, PlayerLists> ListsDeserializer { get; }

        /// <summary>
        /// Gets the writer of the list of heroes of a player.
        /// </summary>
        public ISerializer<PlayerChunk, PlayerLists> ListsSerializer { get; }

        /// <inheritdoc/>
        public override PlayerChunkCollection WithChunk(int index, PlayerChunk chunk) =>
            new(this.Replace(index, chunk), this.Deserializer, this.Serializer, this.ListsDeserializer, this.ListsSerializer, this.differ, this.heroName);

        /// <summary>
        /// Returns the list of heroes of the player at <paramref name="index"/>, as its record keeps it.
        /// </summary>
        /// <param name="index">The place of the player in the collection.</param>
        /// <returns>The lists.</returns>
        public PlayerLists GetLists(int index) => this.ListsDeserializer.Deserialize(this[index]);

        /// <summary>
        /// Returns the colour of each player, in the order of the collection.
        /// </summary>
        /// <returns>The colours, in the order of the records.</returns>
        public IReadOnlyList<PlayerColor> GetColors() => this.Select(this.Deserializer.DeserializeColor).ToList();

        /// <summary>
        /// Returns the colour of the player at <paramref name="index"/>.
        /// </summary>
        /// <param name="index">The place in its list, from 0.</param>
        /// <returns>The colour.</returns>
        public PlayerColor GetColor(int index) => this.Deserializer.DeserializeColor(this[index]);

        /// <summary>
        /// Returns the index of the player with the given colour, or -1 if the savefile has no such player.
        /// </summary>
        /// <param name="color">The colour.</param>
        /// <returns>The place of the record, or -1 when no record has the colour.</returns>
        public int FindByColor(PlayerColor color) => this.IndexOf(c => this.Deserializer.DeserializeColor(c) == color);
    }
}
