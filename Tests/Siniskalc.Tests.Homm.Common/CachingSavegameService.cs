namespace Siniskalc.Tests.Homm.Common
{
    using System;
    using System.IO;
    using System.Linq;
    using Siniskalc.Core.Runtime;

    /// <summary>
    /// Puts a <see cref="SaveCache{TGame}"/> in front of the service a game's editor loads its savefiles with, so an editor test that opens a
    /// file of <c>TestData</c> gets a copy of the game parsed earlier instead of parsing the file again.
    /// </summary>
    /// <remarks>
    /// Only the files of <c>TestData</c> are served from the cache (not those under <c>out</c>, which the manual tests write), and bytes are
    /// served from it only when they are the bytes of the file the name points to, so a test that edits or breaks the bytes first still
    /// reaches the real parser.
    /// </remarks>
    /// <typeparam name="TGame">The game of the project that uses the service.</typeparam>
    public class CachingSavegameService<TGame> : ISavegameService<TGame>
        where TGame : AbstractSavegame
    {
        private const string TestDataFolder = "TestData";
        private const string OutputFolder = "out";

        private readonly ISavegameService<TGame> inner;
        private readonly SaveCache<TGame> cache;

        /// <summary>
        /// Initializes a new instance of the <see cref="CachingSavegameService{TGame}"/> class.
        /// </summary>
        /// <param name="inner">The service that parses what the cache does not have.</param>
        /// <param name="cache">The parsed savefiles, which must load through <paramref name="inner"/> or a service of the same game.</param>
        public CachingSavegameService(ISavegameService<TGame> inner, SaveCache<TGame> cache)
        {
            this.inner = inner;
            this.cache = cache;
        }

        /// <inheritdoc/>
        public TGame LoadGameFromFile(string filePath) => IsCacheable(filePath) ? this.cache.Get(filePath) : this.inner.LoadGameFromFile(filePath);

        /// <inheritdoc/>
        public TGame LoadGameFromBytes(byte[] bytes, string? sourceName = null)
        {
            if (sourceName is not null && IsCacheable(sourceName))
            {
                var copy = this.cache.GetFromBytes(sourceName, bytes, sourceName);
                if (copy is not null)
                {
                    return copy;
                }
            }

            return this.inner.LoadGameFromBytes(bytes, sourceName);
        }

        /// <inheritdoc/>
        public TGame ReloadGame(TGame game) => this.inner.ReloadGame(game);

        /// <inheritdoc/>
        public void SaveGame(TGame game, string filePath)
        {
            this.inner.SaveGame(game, filePath);
            this.cache.Remove(filePath);
        }

        private static bool IsCacheable(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                return false;
            }

            var parts = Path.GetFullPath(path).Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var testData = Array.LastIndexOf(parts, TestDataFolder);
            return testData >= 0 && !parts.Skip(testData + 1).Contains(OutputFolder);
        }
    }
}
