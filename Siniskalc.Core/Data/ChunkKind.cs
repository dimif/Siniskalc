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
        /// <summary>
        /// Compares this kind with <paramref name="other"/> by <see cref="Order"/>.
        /// </summary>
        /// <param name="other">The kind to compare with, or <see langword="null"/>.</param>
        /// <returns>A negative number if this kind comes first, 0 if they have the same order, a positive number if it comes after or <paramref name="other"/> is <see langword="null"/>.</returns>
        public int CompareTo(ChunkKind? other) => other is null ? 1 : this.Order.CompareTo(other.Order);

        /// <summary>
        /// Returns the name of the kind.
        /// </summary>
        /// <returns><see cref="Name"/>.</returns>
        public override string ToString() => this.Name;
    }
}
