using System.Text.RegularExpressions;

namespace Siniskalc.Tests.Data
{
    /// <summary>
    /// The naming rules of the savefiles in TestData (see TestData/README.md), so a new save that breaks them fails here.
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

        private static IEnumerable<string> Files() => Directory.GetFiles("TestData", "*.*", SearchOption.AllDirectories);

        [Test]
        public void EverySavefileIsNamedInLowerCaseSnakeCase()
        {
            var bad = Files().Where(f => !SnakeCase().IsMatch(Path.GetFileNameWithoutExtension(f))).ToList();

            Assert.That(bad, Is.Empty);
        }

        [Test]
        public void EverySavefileHasTheExtensionOfTheGameOrOfARawChunk()
        {
            var bad = Files().Where(f => Path.GetExtension(f) is not (".GM1" or ".GM2" or ".GM3" or ".bin")).ToList();

            Assert.That(bad, Is.Empty);
        }

        [Test]
        public void EveryFolderIsNamedInPascalCase()
        {
            var bad = Directory.GetDirectories("TestData", "*", SearchOption.AllDirectories).Where(d => !PascalCase().IsMatch(Path.GetFileName(d))).ToList();

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

        [TestCase("roe_15b_dwelling_3", "roe", 15)]
        [TestCase("conflux_00_baseline", "conflux", 0)]
        [TestCase("hotseat_07_sir_mullich_fountain_of_fortune", "hotseat", 7)]
        [TestCase("sleep_garrison_visiting_04_akka_garrisoned_oidana_visiting", "sleep_garrison_visiting", 4)]
        public void ANumberedSaveBelongsToTheSeriesBeforeItsNumber(string name, string series, int number)
        {
            Assert.That(SeriesSaves.IsInSeries(name, series), Is.True);
            Assert.That(SeriesSaves.Number(name), Is.EqualTo(number));
        }

        [TestCase("cove_inland_pub", "cove_inland")]
        [TestCase("roe_15b_dwelling_3", "conflux")]
        public void ASaveWithoutANumberOrOfAnotherSeriesIsNotInTheSeries(string name, string series)
        {
            Assert.That(SeriesSaves.IsInSeries(name, series), Is.False);
        }
    }
}
