namespace Siniskalc.Core.Data
{
    using System;

    /// <summary>
    /// Describes a change to an <see cref="IByteEditTarget"/>.
    /// </summary>
    /// <param name="isLocal">Whether the change was made through the target's own <see cref="IByteEditTarget.Update"/>.</param>
    public sealed class ByteEditTargetChangedEventArgs(bool isLocal) : EventArgs
    {
        /// <summary>
        /// Gets a value indicating whether the change was made through the target's own <see cref="IByteEditTarget.Update"/>.
        /// A change that is not local came from elsewhere: another page, undo or redo.
        /// </summary>
        public bool IsLocal { get; } = isLocal;
    }
}
