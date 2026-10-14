namespace Siniskalc.Homm2.Tests.Data
{
    using System.Text.RegularExpressions;

    /// <summary>
    /// The naming rules of the savefiles in TestData/Homm2 (see TestData/README.md), so a new save that breaks them fails here.
    /// The saves of Heroes of Might and Magic II are not checked by the tests of the HoMM3 saves, which read every savefile as a HoMM3 one.
    /// </summary>
    [TestFixture]
    internal sealed partial class TestDataNamesTests
    {
        [GeneratedRegex("^[a-z][a-z0-9]*(_[a-z0-9]+)*(__[a-z0-9]+(_[a-z0-9]+)*)*$")]
        private static partial Regex SnakeCase();

        [GeneratedRegex("^[A-Z][A-Za-z0-9]*$")]
        private static partial Regex PascalCase();

        [GeneratedRegex("^(?<series>[a-z][a-z0-9_]*?)_(?<number>[0-9]{2})(?<branch>[a-z]?)(_|$)")]
        private static partial Regex Numbered();

        // The saves of the write test (EditedSaves) are generated into out/, are not in the repository, and are named for the game's 8 character limit.
        private static IEnumerable<string> Files() => Directory.GetFiles(Path.Combine("TestData", "Homm2"), "*.*", SearchOption.AllDirectories).Where(f => !IsGenerated(f));

        private static IEnumerable<string> Folders() => Directory.GetDirectories(Path.Combine("TestData", "Homm2"), "*", SearchOption.AllDirectories).Where(d => !IsGenerated(d));

        private static bool IsGenerated(string path) => path.Split(Path.DirectorySeparatorChar).Contains("out");

        [Test]
        public void EverySavefileIsNamedInLowerCaseSnakeCase()
        {
            var bad = Files().Where(f => !SnakeCase().IsMatch(Path.GetFileNameWithoutExtension(f))).ToList();

            Assert.That(bad, Is.Empty);
        }

        [Test]
        public void EverySavefileHasTheExtensionOfTheBaseGameOrOfThePriceOfLoyalty()
        {
            var bad = Files().Where(f => Path.GetExtension(f) is not (".GM1" or ".GX1")).ToList();

            Assert.That(bad, Is.Empty);
        }

        [Test]
        public void EveryFolderIsNamedInPascalCase()
        {
            var bad = Folders().Where(d => !PascalCase().IsMatch(Path.GetFileName(d))).ToList();

            Assert.That(bad, Is.Empty);
        }

        [Test]
        public void NoTwoSavesOfASeriesShareANumber()
        {
            var duplicates = Files()
                .Select(f => (Folder: Path.GetDirectoryName(f), Match: Numbered().Match(Path.GetFileNameWithoutExtension(f)), File: f))
                .Where(x => x.Match.Success)
                .GroupBy(x => (x.Folder, Series: x.Match.Groups["series"].Value, Number: x.Match.Groups["number"].Value, Branch: x.Match.Groups["branch"].Value))
                .Where(g => g.Count() > 1)
                .Select(g => string.Join(", ", g.Select(x => Path.GetFileName(x.File))))
                .ToList();

            Assert.That(duplicates, Is.Empty);
        }

        [Test]
        public void EverySaveOfASeriesIsNumberedAndTheSeriesFolderHoldsOnlyItsSaves()
        {
            foreach (var folder in Directory.GetDirectories(Path.Combine("TestData", "Homm2")).Where(d => !IsGenerated(d)))
            {
                var saves = Directory.GetFiles(folder);

                Assert.That(saves.Select(s => Numbered().IsMatch(Path.GetFileNameWithoutExtension(s))), Is.All.True, folder);
                Assert.That(saves.Select(s => Path.GetExtension(s)).Distinct().Count(), Is.EqualTo(1), folder);
            }
        }
    }
}
