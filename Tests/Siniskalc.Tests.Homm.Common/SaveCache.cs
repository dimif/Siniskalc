namespace Siniskalc.Tests.Homm.Common
{
    using System.Collections.Concurrent;
    using Siniskalc.Core.Runtime;

    /// <summary>
    /// Parses each savefile of a test run once and hands out a fresh copy of the game every time it is asked for it.
    /// </summary>
    /// <remarks>
    /// The parsed document of a game is immutable (it clones its bytes, every edit makes a new document), so the copies share it and cost a new
    /// game object and a new history. A test can edit, undo and save its copy without any other test seeing it. The cache is filled when a file
    /// is first asked for, so a run of one test parses only what it needs.
    /// </remarks>
    /// <typeparam name="TGame">The game of the project that uses the cache.</typeparam>
    public sealed class SaveCache<TGame>
        where TGame : AbstractSavegame
    {
        private readonly Func<string, TGame> load;
        private readonly Func<TGame, string?, string?, TGame> fork;
        private readonly ConcurrentDictionary<string, Lazy<TGame>> parsed = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="SaveCache{TGame}"/> class.
        /// </summary>
        /// <param name="load">Reads and parses the file at a path.</param>
        /// <param name="fork">
        /// Makes a new game around the document of a parsed one, with a history of its own and bytes of its own, the file name and the
        /// name it is given (a game loaded from bytes has no file name).
        /// </param>
        public SaveCache(Func<string, TGame> load, Func<TGame, string?, string?, TGame> fork)
        {
            this.load = load;
            this.fork = fork;
        }

        /// <summary>
        /// Gets a copy of the game in the file at <paramref name="path"/>, which is read and parsed the first time only.
        /// </summary>
        public TGame Get(string path) => this.fork(this.Parsed(path), path, null);

        /// <summary>
        /// Gets a copy of the game in the file at <paramref name="path"/> as a game loaded from <paramref name="bytes"/> is made (no file name,
        /// the given name), or <c>null</c> when <paramref name="bytes"/> are not the bytes of that file and have to be parsed by the caller.
        /// </summary>
        public TGame? GetFromBytes(string path, byte[] bytes, string? name)
        {
            var game = this.Parsed(path);
            return game.Bytes.AsSpan().SequenceEqual(bytes) ? this.fork(game, null, name) : null;
        }

        /// <summary>
        /// Parses many files at once (up to eight at a time), so a test that walks every savefile does not parse them one after the other.
        /// </summary>
        public void LoadAll(IEnumerable<string> paths)
        {
            Parallel.ForEach(paths, new ParallelOptions { MaxDegreeOfParallelism = Math.Min(Environment.ProcessorCount, 8) }, path => this.Parsed(path));
        }

        /// <summary>
        /// Forgets the file at <paramref name="path"/>, such as after a save wrote to it.
        /// </summary>
        public void Remove(string path) => this.parsed.TryRemove(System.IO.Path.GetFullPath(path), out _);

        /// <summary>
        /// Forgets every file, which frees the memory they take.
        /// </summary>
        public void Clear() => this.parsed.Clear();

        private TGame Parsed(string path)
        {
            var key = System.IO.Path.GetFullPath(path);
            var entry = this.parsed.GetOrAdd(key, _ => new Lazy<TGame>(() => this.load(path)));
            try
            {
                return entry.Value;
            }
            catch
            {
                // A file that could not be parsed is not remembered as a failure: the next call tries again.
                this.parsed.TryRemove(new KeyValuePair<string, Lazy<TGame>>(key, entry));
                throw;
            }
        }
    }
}
