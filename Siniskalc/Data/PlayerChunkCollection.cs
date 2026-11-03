namespace Siniskalc.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Models;
    using Siniskalc.Entities;
    using Siniskalc.Models;

    /// <summary>
    /// The players of a savefile, in colour order. Empty for savefile versions whose players are not known.
    /// </summary>
    public sealed class PlayerChunkCollection : ChunkCollection<PlayerChunk>
    {
        private readonly IPlayerDiffer differ;
        private readonly Func<int, string> heroName;
        private readonly ChunkKind kind;

        /// <param name="heroName">Gives the name of the hero with an id, so that a change of the heroes of the tavern can name them.</param>
        public PlayerChunkCollection(IEnumerable<PlayerChunk> chunks, IPlayerDeserializer deserializer, IPlayerSerializer serializer, IPlayerDiffer differ, Func<int, string> heroName, ChunkKind? kind = null)
            : base(chunks)
        {
            this.Deserializer = deserializer;
            this.Serializer = serializer;
            this.differ = differ;
            this.heroName = heroName;
            this.kind = kind ?? Homm3ChunkKinds.Player;
        }

        /// <summary>
        /// Gets what reads a player from a chunk.
        /// </summary>
        public IPlayerDeserializer Deserializer { get; }

        /// <summary>
        /// Gets what writes a player to a chunk.
        /// </summary>
        public IPlayerSerializer Serializer { get; }

        public override PlayerChunkCollection WithChunk(int index, PlayerChunk chunk) => new(this.Replace(index, chunk), this.Deserializer, this.Serializer, this.differ, this.heroName, this.kind);

        /// <summary>
        /// Returns the colour of each player, in the order of the collection.
        /// </summary>
        public IReadOnlyList<PlayerColor> GetColors() => this.Select(this.Deserializer.DeserializeColor).ToList();

        /// <summary>
        /// Returns the colour of the player at <paramref name="index"/>.
        /// </summary>
        public PlayerColor GetColor(int index) => this.Deserializer.DeserializeColor(this[index]);

        /// <summary>
        /// Returns the player at <paramref name="index"/>.
        /// </summary>
        public PlayerModel GetModel(int index) => this.Deserializer.Deserialize(this[index]);

        /// <summary>
        /// Returns the index of the player with the given colour, or -1 if the savefile has no such player.
        /// </summary>
        public int FindByColor(PlayerColor color) => this.IndexOf(c => this.Deserializer.DeserializeColor(c) == color);

        protected override IEnumerable<PendingChange> DiffChunk(int index, PlayerChunk original, PlayerChunk current)
        {
            foreach (var change in this.differ.Diff(index, this.Deserializer.Deserialize(original), this.Deserializer.Deserialize(current), this.heroName))
            {
                yield return change;
            }

            var beforeCount = original.GetByteAt(PlayerLayout.TownCount);
            var afterCount = current.GetByteAt(PlayerLayout.TownCount);
            if (beforeCount != afterCount || beforeCount > Constants.Player.MaxTownCount)
            {
                yield break;
            }

            var before = original.GetBytesAt(PlayerLayout.TownIds, beforeCount);
            var after = current.GetBytesAt(PlayerLayout.TownIds, afterCount);

            // Membership changes belong to castle ownership; a round trip can leave only an order change.
            if (!before.SequenceEqual(after) && before.Order().SequenceEqual(after.Order()))
            {
                yield return new PendingChange(
                    current.Name,
                    new Change(ChangeAreas.General, "Town order", string.Join(", ", before), string.Join(", ", after)),
                    this.kind,
                    index,
                    history =>
                    {
                        var chunk = history.Current.GetChunks<PlayerChunk>(this.kind)[index];
                        var count = chunk.GetByteAt(PlayerLayout.TownCount);
                        var ids = chunk.GetBytesAt(PlayerLayout.TownIds, count);
                        var ordered = before.Where(ids.Contains).Concat(ids.Except(before)).ToArray();
                        var bytes = chunk.ToArray();
                        ordered.CopyTo(bytes, PlayerLayout.TownIds);
                        history.Apply(history.Current.WithChunk(this.kind, index, chunk.WithBytes(bytes)));
                    });
            }
        }
    }
}
