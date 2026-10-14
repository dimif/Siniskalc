namespace Siniskalc.Homm2.Data
{
    using System;

    /// <summary>
    /// Finds out whether a file is a Heroes of Might and Magic II savegame, and where the parts of it are.
    /// </summary>
    public interface ISaveLayoutLocator
    {
        /// <summary>
        /// Gets how many bytes from the start of a file <see cref="Recognizes"/> needs.
        /// </summary>
        int HeadLength { get; }

        /// <summary>
        /// Returns whether a file starts like a savegame of the game: the width and the height of a square map and the number of the map
        /// format, after the marker of the Price of Loyalty when the file has one. Only the start of the file is looked at.
        /// </summary>
        bool Recognizes(ReadOnlySpan<byte> head);

        /// <summary>
        /// Returns where the tables of the savegame are, or <c>null</c> when the bytes are not a savegame this editor knows: the tables
        /// are checked (every hero and castle holds its own number), so a savegame of another layout is refused and not misread.
        /// </summary>
        SaveLayout? Locate(byte[] bytes);
    }
}
