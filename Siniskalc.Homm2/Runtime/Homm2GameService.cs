namespace Siniskalc.Homm2.Runtime
{
    using System;
    using Siniskalc.Core.Runtime;

    /// <summary>
    /// Loads, reloads and saves <see cref="Homm2Game"/> instances: the file I/O. Building a game from the bytes of a savefile is the job of
    /// <see cref="IHomm2SavegameParser"/>. The savefiles of the game are not compressed.
    /// </summary>
    public sealed class Homm2GameService : IHomm2GameService
    {
        private readonly IHomm2SavegameParser parser;
        private readonly ISavefileCodec codec;
        private readonly ISavefileBackup backup;

        public Homm2GameService(IHomm2SavegameParser parser, ISavefileCodec codec, ISavefileBackup backup)
        {
            this.parser = parser;
            this.codec = codec;
            this.backup = backup;
        }

        /// <inheritdoc/>
        /// <exception cref="ArgumentException">The path is empty.</exception>
        /// <exception cref="SavefileException">The file is not a savegame this editor can edit.</exception>
        public Homm2Game LoadGameFromFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("File path cannot be null or empty", nameof(filePath));
            }

            return this.parser.Parse(this.codec.Read(filePath), filePath);
        }

        /// <inheritdoc/>
        public Homm2Game LoadGameFromBytes(byte[] bytes, string? sourceName = null) => this.parser.Parse(bytes, null, sourceName);

        /// <inheritdoc/>
        /// <exception cref="InvalidOperationException">The game has no file to read again.</exception>
        public Homm2Game ReloadGame(Homm2Game game)
        {
            if (string.IsNullOrWhiteSpace(game.File))
            {
                throw new InvalidOperationException("Cannot reload game without an associated file");
            }

            return this.LoadGameFromFile(game.File);
        }

        /// <inheritdoc/>
        public void SaveGame(Homm2Game game, string filePath)
        {
            // Worked out before the file is touched: nothing is replaced if the edits cannot be applied.
            var bytes = game.GetBytesToSave();

            // Keep a copy of whatever is being replaced, so a bad save can be undone (when the user has not turned that off).
            this.backup.Backup(filePath);

            this.codec.Write(filePath, bytes);

            // TODO:LOG info: the savegame was saved to {filePath} ({bytes.Length} bytes, {pending changes} pending changes, backup {path or none}).
            game.SavedTo(filePath);

            // Only once everything is written and the file is closed.
            game.History.MarkSaved();
        }
    }
}
