namespace Siniskalc.Core.Data
{
    using System;

    /// <summary>
    /// A kind of struct a <see cref="SaveDocument"/> holds chunks of (a hero, a player, a castle).
    /// A game defines its kinds as values of this type, a <see cref="ChunkCollection{TChunk}"/> for each, and registers them when it creates the document.
    /// </summary>
    /// <param name="Name">
    /// What the kind is called.
    /// </param>
    /// <param name="Order">
    /// The position of the kind among the kinds of a game. Pending changes and chunks are listed in this order.
    /// </param>
    public sealed record ChunkKind(string Name, int Order) : IComparable<ChunkKind>
    {
        /// <inheritdoc/>
        public int CompareTo(ChunkKind? other) => other is null ? 1 : this.Order.CompareTo(other.Order);

        /// <inheritdoc/>
        public override string ToString() => this.Name;
    }
}
