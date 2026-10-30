using Siniskalc.Core.Data;
using Siniskalc.Homm3.Runtime;

namespace Siniskalc.Homm3.Tests.Data
{
    /// <summary>
    /// The savefiles of the tests, each parsed once for the whole run. <see cref="Get"/> gives every test a game of its own (its edits, its
    /// undo and its save do not reach any other test), made around the parsed document, so asking for a file again costs almost nothing.
    /// The tests whose subject is the loading itself call <c>TestServices.GameService</c> instead.
    /// </summary>
    internal static class SavedGames
    {
        private static readonly SaveCache<Game> Cache = new(
            path => TestServices.GameService.LoadGameFromFile(path),
            (game, file, name) => new Game(file, (byte[])game.Bytes.Clone(), game.GameVersion, new SaveDocumentHistory(game.History.Original), name, game.Metadata, game.Places));

        /// <summary>Gets a game of the file at <paramref name="path"/> that nobody else holds.</summary>
        public static Game Get(string path) => Cache.Get(path);

        /// <summary>Parses many files at once, so that the next calls of <see cref="Get"/> find them ready.</summary>
        public static void LoadAll(IEnumerable<string> paths) => Cache.LoadAll(paths);

        /// <summary>Forgets every file, which frees the memory they take.</summary>
        public static void Clear() => Cache.Clear();
    }
}
