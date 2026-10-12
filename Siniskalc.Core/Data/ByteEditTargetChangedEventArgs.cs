namespace Siniskalc.Core.Data
{
    using System;

    /// <summary>
    /// Describes a change to an <see cref="IByteEditTarget"/>.
    /// </summary>
    public sealed class ByteEditTargetChangedEventArgs : EventArgs
    {
        public ByteEditTargetChangedEventArgs(bool isLocal)
        {
            this.IsLocal = isLocal;
        }

        /// <summary>
        /// Gets whether the change was made through the target's own <see cref="IByteEditTarget.Update"/>.
        /// A change that is not local came from elsewhere: another page, undo or redo.
        /// </summary>
        public bool IsLocal { get; }
    }
}
