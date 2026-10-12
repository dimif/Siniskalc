using Siniskalc.Runtime;

namespace Siniskalc.Tests.Data
{
    /// <summary>
    /// Finds the Horn of the Abyss sample savefiles that hold castles, wherever the test data is copied to.
    /// </summary>
    internal static class CastleSamples
    {
        public static string Root => new[] { Path.Combine("TestData", "Hota"), Path.Combine("Data", "Hota") }.First(Directory.Exists);

        public static IEnumerable<string> All()
        {
            var root = Root;
            return Directory.GetFiles(root, "*.GM?", SearchOption.AllDirectories).OrderBy(f => f, StringComparer.Ordinal);
        }

        public static IEnumerable<string> Folder(string name) => All().Where(f => Path.GetFileName(Path.GetDirectoryName(f)) == name);

        public static Game Load(string path) => TestServices.GameService.LoadGameFromFile(path);
    }
}
