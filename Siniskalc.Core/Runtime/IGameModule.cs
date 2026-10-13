namespace Siniskalc.Core.Runtime
{
    using System;

    /// <summary>
    /// One game the application can edit: how its savefiles are recognised and picked, and the editor that works on them.
    /// </summary>
    public interface IGameModule
    {
        /// <summary>
        /// Gets what the game is called, for the title of the window.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the file types to offer when opening a savefile of the game, as a Windows file dialog filter entry (<c>"*.GM1;*.GM2"</c>, no
        /// description).
        /// </summary>
        string OpenPattern { get; }

        /// <summary>
        /// Gets the file types to offer when saving a savefile of the game, in the same form as <see cref="OpenPattern"/>.
        /// </summary>
        string SavePattern { get; }

        /// <summary>
        /// Gets how many bytes from the start of the file <see cref="Recognizes"/> may need.
        /// </summary>
        int HeadLength { get; }

        /// <summary>
        /// Returns whether a file is a savefile of this game, from its first bytes.
        /// </summary>
        /// <param name="filePath">The path of the file; only its name and extension are looked at.</param>
        /// <param name="head">The first bytes of the file, as many as it has up to <see cref="HeadLength"/>.</param>
        bool Recognizes(string filePath, ReadOnlySpan<byte> head);

        /// <summary>
        /// Makes the editor of the game, with nothing loaded.
        /// </summary>
        IGameEditor CreateEditor();

        /// <summary>
        /// Tells whether the game can list a savegame with this file name (the name with its extension, no folder): <c>null</c> when it can,
        /// else what is wrong, in a sentence for the user. Used before the game is saved to a new file. Any name is fine by default.
        /// </summary>
        string? ValidateSavegameName(string fileName) => null;
    }
}
