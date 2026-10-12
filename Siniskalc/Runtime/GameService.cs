namespace Siniskalc.Runtime
{
    using System;
    using System.IO;
    using Siniskalc.Core.Runtime;

    /// <summary>
    /// Loads, reloads and saves <see cref="Game"/> instances: the file I/O. Building a game from the bytes of a savefile is the job of <see cref="ISavegameParser"/>.
    /// </summary>
    public sealed class GameService : IGameService
    {
        private readonly ISavegameParser parser;
        private readonly ISavefileCodec codec;
        private readonly ISavefileBackup backup;

        public GameService(ISavegameParser parser, ISavefileCodec codec, ISavefileBackup backup)
        {
            this.parser = parser;
            this.codec = codec;
            this.backup = backup;
        }

        /// <summary>
        /// Loads a game from the specified file path.
        /// </summary>
        /// <param name="filePath">The path to the game savegame file (CGM, GM1, etc.)</param>
        /// <returns>A loaded Game instance</returns>
        /// <exception cref="ArgumentException">Thrown when filePath is null or empty</exception>
        /// <exception cref="FileNotFoundException">Thrown when the file does not exist</exception>
        /// <exception cref="SavefileException">Thrown when the file is not a savegame this editor can edit</exception>
        public Game LoadGameFromFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("File path cannot be null or empty", nameof(filePath));
            }

            return this.parser.Parse(this.codec.Read(filePath), filePath);
        }

        /// <summary>
        /// Loads a game from raw bytes (e.g., from a dump file). It has no file, so it cannot be reloaded until it is saved.
        /// </summary>
        /// <param name="bytes">The raw game data bytes</param>
        /// <param name="sourceName">Optional source name for tracking (e.g., filename)</param>
        /// <returns>A loaded Game instance</returns>
        /// <exception cref="SavefileException">Thrown when the bytes are not a savegame this editor can edit</exception>
        public Game LoadGameFromBytes(byte[] bytes, string? sourceName = null)
        {
            return this.parser.Parse(bytes, null, sourceName);
        }

        /// <summary>
        /// Loads the game again from its file, discarding unsaved edits.
        /// </summary>
        /// <param name="game">The game to reload</param>
        /// <returns>The reloaded game, which replaces <paramref name="game"/></returns>
        /// <exception cref="InvalidOperationException">Thrown when the game has no file to read again (it was loaded from bytes and not saved since)</exception>
        public Game ReloadGame(Game game)
        {
            if (string.IsNullOrWhiteSpace(game.File))
            {
                throw new InvalidOperationException("Cannot reload game without an associated file");
            }

            return this.LoadGameFromFile(game.File);
        }

        /// <summary>
        /// Writes the savefile with every edit applied. Afterwards the saved state is what pending changes are measured against.
        /// </summary>
        public void SaveGame(Game game, string filePath)
        {
            // Worked out before the file is touched: nothing is replaced if the edits cannot be applied.
            var bytes = game.GetBytesToSave();

            // Keep a copy of whatever is being replaced, so a bad save can be undone (when the user has not turned that off).
            this.backup.Backup(filePath);

            this.codec.Write(filePath, bytes);

            // TODO:LOG info: the savegame was saved to {filePath} ({bytes.Length} bytes, {pending changes} pending changes, backup {path or none}).

            // Reloading reads what was just saved, not the file the game was opened from.
            game.SavedTo(filePath);

            // Only once everything is written and the file is closed.
            game.History.MarkSaved();
        }
    }
}
