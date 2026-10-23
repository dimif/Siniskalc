namespace Siniskalc.Core.Runtime
{
    using System;
    using System.IO;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Loads, reloads and saves the open games of one game: the file I/O. Building a game from the bytes of a savefile is the job of
    /// <see cref="ISavegameParser{TGame}"/>.
    /// </summary>
    /// <typeparam name="TGame">What the game calls an open savefile.</typeparam>
    public abstract class SavegameService<TGame> : ISavegameService<TGame>
        where TGame : ISavegame
    {
        private readonly ISavegameParser<TGame> parser;
        private readonly ISavefileCodec codec;
        private readonly IFileBackupServiceProvider backups;
        private readonly ILogger logger;

        protected SavegameService(ISavegameParser<TGame> parser, ISavefileCodec codec, IFileBackupServiceProvider backups, ILogger logger)
        {
            this.logger = logger;
            this.parser = parser;
            this.codec = codec;
            this.backups = backups;
        }

        /// <inheritdoc/>
        /// <exception cref="ArgumentException">The path is empty.</exception>
        /// <exception cref="SavefileException">The file is not a savegame this editor can edit.</exception>
        public TGame LoadGameFromFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("File path cannot be null or empty", nameof(filePath));
            }

            return this.parser.Parse(this.codec.Read(filePath), filePath);
        }

        /// <inheritdoc/>
        public TGame LoadGameFromBytes(byte[] bytes, string? sourceName = null) => this.parser.Parse(bytes, null, sourceName);

        /// <inheritdoc/>
        /// <exception cref="InvalidOperationException">The game has no file to read again.</exception>
        public TGame ReloadGame(TGame game)
        {
            if (string.IsNullOrWhiteSpace(game.FileName))
            {
                throw new InvalidOperationException("Cannot reload game without an associated file");
            }

            return this.LoadGameFromFile(game.FileName);
        }

        /// <inheritdoc/>
        public void SaveGame(TGame game, string filePath)
        {
            // Worked out before the file is touched: nothing is replaced if the edits cannot be applied.
            var bytes = game.GetBytesToSave();

            // Keep a copy of whatever is being replaced, so a bad save can be undone (when the user has not turned that off).
            // A failed backup stops the save: the exception goes up to the caller, which shows it to the user.
            string? backup = null;
            if (File.Exists(filePath))
            {
                backup = this.backups.GetService().Backup(filePath);
            }

            this.codec.Write(filePath, bytes);

            this.logger.LogInformation("Saved {File} ({Length} bytes, backup {Backup})", Path.GetFileName(filePath), bytes.Length, backup is null ? "none" : Path.GetFileName(backup));

            // Reloading reads what was just saved, not the file the game was opened from.
            game.SavedTo(filePath);

            // Only once everything is written and the file is closed.
            game.History.MarkSaved();
        }
    }
}
