using System.Text.RegularExpressions;

namespace Siniskalc.Tests.Data
{
    /// <summary>
    /// Finds the savefiles of one series in TestData (<c>roe_15b_dwelling_3</c> is number 15, branch b, of the series <c>roe</c>).
    /// The naming rules are in TestData/README.md.
    /// </summary>
    internal static partial class SeriesSaves
    {
        [GeneratedRegex("^(?<series>[a-z][a-z0-9_]*?)_(?<number>[0-9]{2})(?<branch>[a-z]?)(_|$)")]
        private static partial Regex Pattern();

        /// <summary>Whether the name (without extension) is a numbered save of the series.</summary>
        public static bool IsInSeries(string name, string series)
        {
            var match = Pattern().Match(name);
            return match.Success && match.Groups["series"].Value == series;
        }

        /// <summary>The number of a save in its series, <c>15</c> for <c>roe_15b_dwelling_3</c>.</summary>
        public static int Number(string name)
        {
            var match = Pattern().Match(name);
            Assert.That(match.Success, Is.True, $"{name} is not a numbered save");
            return int.Parse(match.Groups["number"].Value);
        }

        /// <summary>The names (without extension) of the numbered saves of a series in a folder of TestData, in the order they were made.</summary>
        public static List<string> Names(string folder, string series, string extension = ".GM1") =>
            Directory.GetFiles(Path.Combine("TestData", folder), "*" + extension)
                .Select(f => Path.GetFileNameWithoutExtension(f))
                .Where(n => IsInSeries(n, series))
                .OrderBy(Number)
                .ThenBy(n => n, StringComparer.Ordinal)
                .ToList();
    }
}
