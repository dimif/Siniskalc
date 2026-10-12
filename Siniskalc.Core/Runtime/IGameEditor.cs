namespace Siniskalc.Core.Runtime
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using Siniskalc.Core.Data;

    /// <summary>
    /// What the application shell needs from the editor of one game: loading and saving its savefile, the history of edits (undo, redo,
    /// what is pending), and the bytes it would save. The pages that edit the game, and everything else that is specific to it, are the
    /// game's own; the shell shows them through a data template of the editor's type.
    /// </summary>
    public interface IGameEditor : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets a value indicating whether a savegame is loaded.
        /// </summary>
        bool IsGameLoaded { get; }

        /// <summary>
        /// Gets the file the game is tied to: where it was loaded from, or last saved to. <c>null</c> when it was loaded from bytes (a dump),
        /// which have no file to read again.
        /// </summary>
        string? File { get; }

        /// <summary>
        /// Gets what to call the loaded game: its file, or the name it was given when loaded from bytes. <c>null</c> when none is loaded.
        /// </summary>
        string? Name { get; }

        /// <summary>
        /// Gets a value indicating whether the game is tied to a file.
        /// </summary>
        bool HasFile { get; }

        /// <summary>
        /// Gets a value indicating whether the game can be read again from its file.
        /// </summary>
        bool CanReload { get; }

        bool CanUndo { get; }

        bool CanRedo { get; }

        /// <summary>
        /// Gets what has been edited since the savefile was loaded or saved. Updated after every edit, undo and redo.
        /// </summary>
        ObservableCollection<PendingChange> PendingChanges { get; }

        bool HasPendingChanges { get; }

        /// <summary>
        /// Loads a game from a savefile. What was open is let go.
        /// </summary>
        void LoadGame(string filePath);

        /// <summary>
        /// Loads a game from the bytes of a savefile as the game wrote them, before any compression (a dump).
        /// </summary>
        void LoadGameFromBytes(byte[] bytes, string? sourceName = null);

        /// <summary>
        /// Reads the game again from its file, discarding unsaved edits.
        /// </summary>
        void ReloadGame();

        /// <summary>
        /// Saves over the file the game has.
        /// </summary>
        void SaveGame();

        /// <summary>
        /// Saves the game to <paramref name="filePath"/>, which is then its file.
        /// </summary>
        void SaveGame(string filePath);

        /// <summary>
        /// Returns the bytes the savefile would have, uncompressed, with every edit applied.
        /// </summary>
        byte[] GetBytesToSave();

        /// <summary>
        /// Takes the bytes of the whole savefile as they would be saved, with its known regions, for the hex view.
        /// </summary>
        SaveHexDump CreateSaveHexDump();

        void Undo();

        void Redo();

        /// <summary>
        /// Puts one pending change back to its loaded value. It can be undone.
        /// </summary>
        void RevertChange(PendingChange? change);

        /// <summary>
        /// Puts every edit back to the loaded values. It can be undone.
        /// </summary>
        void RevertAll();
    }
}
