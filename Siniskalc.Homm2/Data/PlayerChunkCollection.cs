namespace Siniskalc.Homm2.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// The six players of a savegame, one for each colour. The index of a player is its colour. Besides its model, the record of a player
    /// keeps the lists of heroes and castles it has (<see cref="PlayerLists"/>), which follow the owners of the heroes and the castles.
    /// </summary>
    public sealed class PlayerChunkCollection : ModelChunkCollection<PlayerChunk, PlayerModel, IDeserializer<PlayerChunk, PlayerModel>, ISerializer<PlayerChunk, PlayerModel>>
    {
        private readonly IPlayerDiffer differ;
        private readonly Func<int, string> heroName;

        /// <param name="chunks">The records of the players, in the order of the colours.</param>
        /// <param name="deserializer">Reads the model of a player.</param>
        /// <param name="serializer">Writes the model of a player.</param>
        /// <param name="listsDeserializer">Reads the lists of heroes and castles of a player.</param>
        /// <param name="listsSerializer">Writes the lists of heroes and castles of a player.</param>
        /// <param name="differ">Lists what an edit changed in a player.</param>
        /// <param name="heroName">The name of a hero from its number.</param>
        public PlayerChunkCollection(
            IEnumerable<PlayerChunk> chunks,
            IDeserializer<PlayerChunk, PlayerModel> deserializer,
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
        /// Gets the reader of the lists of heroes and castles of a player.
        /// </summary>
        public IDeserializer<PlayerChunk, PlayerLists> ListsDeserializer { get; }

        /// <summary>
        /// Gets the writer of the lists of heroes and castles of a player.
        /// </summary>
        public ISerializer<PlayerChunk, PlayerLists> ListsSerializer { get; }

        /// <inheritdoc/>
        public override PlayerChunkCollection WithChunk(int index, PlayerChunk chunk) =>
            new(this.Replace(index, chunk), this.Deserializer, this.Serializer, this.ListsDeserializer, this.ListsSerializer, this.differ, this.heroName);

        public PlayerColor GetColor(int index) => PlayerColor.All[index];

        public int FindByColor(PlayerColor color) => color.Code;

        /// <summary>
        /// Returns the lists of heroes and castles of the player at <paramref name="index"/>, as its record keeps them.
        /// </summary>
        /// <param name="index">The place of the player, which is its colour.</param>
        public PlayerLists GetLists(int index) => this.ListsDeserializer.Deserialize(this[index]);

        /// <summary>
        /// Returns the players that are in the game: those that have a castle or a hero in their lists, or whose days left to own a castle
        /// are counted, in the order of the colours.
        /// </summary>
        public IReadOnlyList<PlayerModel> GetPlayersInGame() =>
            this.GetModels().Where((player, index) => player.DaysLeft != null || this.GetLists(index).OwnsAnything).ToList();
    }
}
