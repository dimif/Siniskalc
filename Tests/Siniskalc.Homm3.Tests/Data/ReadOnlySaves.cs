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
    }
}
