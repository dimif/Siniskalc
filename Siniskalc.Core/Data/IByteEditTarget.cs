namespace Siniskalc.Core.Data
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// A fixed-size run of bytes that the hex editor can show and edit: a hero, a player, a town and so on.
    /// Edits go through the savefile's undo history, so each <see cref="Update"/> is a single undo step.
    /// Dispose the target when it is no longer shown, so it stops listening to the history.
    /// </summary>
    public interface IByteEditTarget : IDisposable
    {
        /// <summary>
        /// Raised when the bytes change, whether through <see cref="Update"/> or from anywhere else.
        /// </summary>
        event EventHandler<ByteEditTargetChangedEventArgs>? Changed;

        /// <summary>
        /// Gets a short name for what is being edited, e.g. the hero's name.
        /// </summary>
        string Title { get; }

        /// <summary>
        /// Gets the known regions of the bytes. They stay the same for the life of the target.
        /// </summary>
        IReadOnlyList<ByteRegion> Regions { get; }

        /// <summary>
        /// Returns a copy of the current bytes.
        /// </summary>
        byte[] GetBytes();

        /// <summary>
        /// Returns a copy of the bytes as they were when the savefile was loaded.
        /// </summary>
        byte[] GetOriginalBytes();

        /// <summary>
        /// Replaces the bytes with the result of <paramref name="edit"/>, which gets a copy of the current bytes and
        /// returns the new ones, of the same length. Records one undo step. Returns false, and records nothing,
        /// if the bytes end up unchanged.
        /// </summary>
        bool Update(Func<byte[], byte[]> edit);
    }
}
