using System.Collections.Concurrent;
using Siniskalc.Homm3.Runtime;

namespace Siniskalc.Homm3.Tests.Data
{
    /// <summary>
    /// Loads a savefile once for every test that only reads it, so the contracts that run over every savefile of TestData do not each parse the same files again.
    /// A test that edits, undoes or serializes a changed document loads its own copy through <c>TestServices.GameService</c> instead.
    /// </summary>
    internal static class ReadOnlySaves
    {
        private static readonly ConcurrentDictionary<string, Lazy<Game>> Loaded = new();

        public static Game Load(string path)
        {
            return Loaded.GetOrAdd(Path.GetFullPath(path), p => new Lazy<Game>(() => TestServices.GameService.LoadGameFromFile(p))).Value;
        }

        /// <summary>
        /// Loads many savefiles at once (up to eight at a time), so a test that walks every save parses them side by side instead of one after the other.
        /// Later calls of <see cref="Load"/> for the same paths find them ready.
        /// </summary>
        public static void LoadAll(IEnumerable<string> paths)
        {
            Parallel.ForEach(paths, new ParallelOptions { MaxDegreeOfParallelism = Math.Min(Environment.ProcessorCount, 8) }, path => Load(path));
        }
    }
}
