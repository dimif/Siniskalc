namespace Siniskalc.Core.Runtime
{
    using System;
    using System.Collections.ObjectModel;
    using Siniskalc.Core.Data;

    /// <summary>
    /// What the application shell needs from the editor of one game: loading and saving its savefile, the history of edits (undo, redo,
    /// what is pending), and the bytes it would save. The pages that edit the game, and everything else that is specific to it, are the
    /// game's own; the shell shows them through a data template of the editor's type.
    /// </summary>
    public interface IGameEditor
    {
        /// <summary>
        /// Gets the savegame that is loaded, or <c>null</c> when none is. Whether a game is loaded, the file it is tied to (<c>null</c> when it was
        /// loaded from bytes, a dump), what to call it and whether it can be read again are read from it.
        /// </summary>
        ISavegame? Savegame { get; }

        /// <summary>
        /// Gets a value indicating whether there is an edit to undo.
        /// </summary>
        bool CanUndo { get; }

        /// <summary>
        /// Gets a value indicating whether there is an undone edit to redo.
        /// </summary>
        bool CanRedo { get; }

        /// <summary>
        /// Gets what has been edited since the savefile was loaded or saved. Updated after every edit, undo and redo.
        /// </summary>
        ObservableCollection<PendingChange> PendingChanges { get; }

        /// <summary>
        /// Gets a value indicating whether anything has been edited since the savefile was loaded or saved.
        /// </summary>
        bool HasPendingChanges { get; }

        /// <summary>
        /// Loads a game from a savefile. What was open is let go.
        /// </summary>
        /// <param name="filePath">The path of the savefile.</param>
        /// <exception cref="SavefileException">The file is not a savefile this editor can edit.</exception>
        void LoadGame(string filePath);

        /// <summary>
        /// Loads a game from the bytes of a savefile as the game wrote them, before any compression (a dump).
        /// </summary>
        /// <param name="bytes">The bytes of the savefile.</param>
        /// <param name="sourceName">What to call the game, as it has no file; a generic name when omitted.</param>
        /// <exception cref="SavefileException">The bytes are not a savefile this editor can edit.</exception>
        void LoadGameFromBytes(byte[] bytes, string? sourceName = null);

        /// <summary>
        /// Reads the game again from its file, discarding unsaved edits.
        /// </summary>
        /// <exception cref="InvalidOperationException">No game is loaded, or it has no file to read again.</exception>
        void ReloadGame();

        /// <summary>
        /// Saves over the file the game has.
        /// </summary>
        /// <exception cref="InvalidOperationException">No game is loaded, or it has no file.</exception>
        void SaveGame();

        /// <summary>
        /// Saves the game to <paramref name="filePath"/>, which is then its file.
        /// </summary>
        /// <param name="filePath">The path of the file to write.</param>
        /// <exception cref="InvalidOperationException">No game is loaded.</exception>
        /// <exception cref="System.IO.IOException">The backup or the file could not be written.</exception>
        void SaveGame(string filePath);

        /// <summary>
        /// Returns the bytes the savefile would have, uncompressed, with every edit applied.
        /// </summary>
        /// <returns>The bytes of the savefile as it would be saved.</returns>
        /// <exception cref="InvalidOperationException">No game is loaded.</exception>
        byte[] GetBytesToSave();

        /// <summary>
        /// Takes the bytes of the whole savefile as they would be saved, with its known regions, for the hex view.
        /// </summary>
        /// <returns>The dump of the savefile.</returns>
        /// <exception cref="InvalidOperationException">No game is loaded.</exception>
        SaveHexDump CreateSaveHexDump();

        /// <summary>
        /// Goes back to the state before the last edit. Does nothing when there is none to undo.
        /// </summary>
        void Undo();

        /// <summary>
        /// Goes forward again to the edit that was undone last. Does nothing when there is none to redo.
        /// </summary>
        void Redo();

        /// <summary>
        /// Puts one pending change back to its loaded value. It can be undone.
        /// </summary>
        /// <param name="change">The change to put back; nothing happens when it is <see langword="null"/>.</param>
        void RevertChange(PendingChange? change);

        /// <summary>
        /// Puts every edit back to the loaded values. It can be undone.
        /// </summary>
        void RevertAll();
    }
}
