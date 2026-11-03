namespace Siniskalc.Core.Runtime
{
    /// <summary>
    /// Loads, reloads and saves the open games of one game.
    /// </summary>
    /// <typeparam name="TGame">What the game calls an open savefile.</typeparam>
    public interface ISavegameService<TGame>
        where TGame : ISavegame
    {
        /// <summary>
        /// Reads a savefile and builds the open game from it.
        /// </summary>
        /// <param name="filePath">The path of the savefile.</param>
        /// <returns>The open game.</returns>
        /// <exception cref="System.ArgumentException">The path is empty.</exception>
        /// <exception cref="SavefileException">The file is not a savegame this editor can edit.</exception>
        TGame LoadGameFromFile(string filePath);

        /// <summary>
        /// Builds the open game from the bytes of a savefile as the game wrote them, before any compression.
        /// </summary>
        /// <param name="bytes">The bytes of the savefile.</param>
        /// <param name="sourceName">What to call the game, as it has no file.</param>
        /// <returns>The open game.</returns>
        /// <exception cref="UnsupportedSavefileException">The bytes are not a savegame this editor knows.</exception>
        TGame LoadGameFromBytes(byte[] bytes, string? sourceName = null);

        /// <summary>
        /// Reads the game again from its file, discarding unsaved edits.
        /// </summary>
        /// <param name="game">The game to read again.</param>
        /// <returns>A new open game.</returns>
        /// <exception cref="System.InvalidOperationException"><paramref name="game"/> has no file.</exception>
        /// <exception cref="SavefileException">The file is no longer a savegame this editor can edit.</exception>
        TGame ReloadGame(TGame game);

        /// <summary>
        /// Writes the game with its edits to <paramref name="filePath"/>, which is then its file, and marks the edits as saved.
        /// </summary>
        /// <param name="game">The game to save.</param>
        /// <param name="filePath">The path of the file to write.</param>
        /// <exception cref="System.IO.IOException">The backup or the file could not be written; the history is left as it was.</exception>
        void SaveGame(TGame game, string filePath);
    }
}
