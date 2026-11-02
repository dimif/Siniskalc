namespace Siniskalc.Core.Data
{
    using System;
    using Siniskalc.Core.Models;

    /// <summary>
    /// A <see cref="Change"/> together with the hero, player or castle it was made to, and how to put it back.
    /// </summary>
    /// <param name="OwnerName">The name of the hero or castle, or of the player's colour for a player's change.</param>
    /// <param name="Change">What changed.</param>
    /// <param name="Kind">What kind of struct the change was made to.</param>
    /// <param name="Index">The index of the hero, player or castle among the savefile's structs of the same <paramref name="Kind"/>.</param>
    /// <param name="Revert">Puts the loaded value back through the history, leaving every other edit alone.</param>
    public sealed record PendingChange(string OwnerName, Change Change, ChunkKind Kind, int Index, Action<SaveDocumentHistory> Revert)
    {
        /// <summary>
        /// Gets the place the change was made, e.g. "Orrin › Creatures".
        /// </summary>
        public string Path => $"{this.OwnerName} › {this.Change.Area.DisplayName}";

        /// <summary>
        /// Returns whether <paramref name="other"/> says the same about the same place. The revert action is not compared: it is made
        /// again each time the changes are worked out.
        /// </summary>
        /// <param name="other">The pending change to compare with.</param>
        /// <returns><see langword="true"/> if the kind, the index, the owner and the change are all equal.</returns>
        /// <exception cref="NullReferenceException"><paramref name="other"/> is <see langword="null"/>.</exception>
        public bool IsSameChangeAs(PendingChange other) =>
            this.Kind == other.Kind && this.Index == other.Index && this.OwnerName == other.OwnerName && this.Change == other.Change;
    }
}
