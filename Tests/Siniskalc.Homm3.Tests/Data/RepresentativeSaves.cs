using System.Text.RegularExpressions;

namespace Siniskalc.Homm3.Tests.Data
{
    /// <summary>
    /// A few savefiles of each series of TestData, for the tests that check the same rule of a map on every save: the saves of one series are
    /// made on one map and differ by one step, so the first, the middle and the last one of each series show the rule as well as all of them
    /// (the first has nothing done, the last everything). A save that belongs to no series (a map of its own) is always kept.
    /// </summary>
    internal static partial class RepresentativeSaves
    {
        [GeneratedRegex("^(?<series>[a-z][a-z0-9_]*?)_[0-9]{2}[a-z]?(_|$)")]
        private static partial Regex Numbered();

        /// <summary>The first, the middle and the last save (by name) of each series found under <paramref name="root"/>, and every save that is in no series.</summary>
        public static List<string> Of(string root, string pattern = "*.GM?") =>
            Directory.GetFiles(root, pattern, SearchOption.AllDirectories)
                .OrderBy(f => f, StringComparer.Ordinal)
                .GroupBy(Series)
                .SelectMany(Pick)
                .OrderBy(f => f, StringComparer.Ordinal)
                .ToList();

        private static string Series(string path)
        {
            var directory = Path.GetDirectoryName(path) ?? string.Empty;
            var match = Numbered().Match(Path.GetFileNameWithoutExtension(path));
            if (match.Success)
            {
                return directory + "|" + match.Groups["series"].Value;
            }

            // The variants of a start (cove_inland_pub, ...) share the folder of their series; a loose save of the top folder is a map of its own.
            return Path.GetFileName(directory) is "Roe" or "Sod" or "Hota" ? path : directory + "|";
        }

        private static IEnumerable<string> Pick(IGrouping<string, string> series)
        {
            var saves = series.ToList();
            return saves.Count <= 3 ? saves : new[] { saves[0], saves[saves.Count / 2], saves[^1] };
        }
    }
}
