namespace Siniskalc.Core.Runtime
{
    using System;

    /// <summary>
    /// The file is not a savefile of any game: it is not what the game wraps its savefiles in, or it ends before it should.
    /// </summary>
    public sealed class CorruptSavefileException(string message, Exception innerException) : SavefileException(message, innerException)
    {
    }
}
