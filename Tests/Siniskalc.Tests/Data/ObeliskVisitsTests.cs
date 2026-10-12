using System.Text;
using System.Text.RegularExpressions;
using Siniskalc.Data;
using Siniskalc.Entities;
using Siniskalc.Models;
using Siniskalc.Runtime;
using Siniskalc.ViewModels;

namespace Siniskalc.Tests.Data
{
    /// <summary>
    /// The obelisks of the map that have been visited, found in front of the table of players. Found in the Restoration of Erathia saves 67 to 77, where
    /// Red visited the five obelisks one at a time; the count is checked against the obelisks of the maps of the other savefiles of the repository.
    /// </summary>
    [TestFixture]
    internal sealed class ObeliskVisitsTests
    {
        private static Game Load(string save) => TestServices.GameService.LoadGameFromFile(Path.Combine("TestData", "Roe", save + ".GM1"));

        private static IEnumerable<string> EverySavefile() => Directory.GetFiles("TestData", "*.GM?", SearchOption.AllDirectories).OrderBy(f => f, StringComparer.Ordinal);

        private static ObeliskVisits Visits(Game game) => game.History.Current.Obelisks().GetModel()!;

        [TestCase("roe_00_baseline", "")]
        [TestCase("roe_67_damacon_defeated", "")]
        [TestCase("roe_68_tazar_obelisk", "1")]
        [TestCase("roe_72_tazar_obelisk_2", "12")]
        [TestCase("roe_73_tazar_obelisk_3", "125")]
        [TestCase("roe_74_tazar_obelisk_4", "1245")]
        [TestCase("roe_75_tazar_all_obelisks", "12345")]
        [TestCase("roe_77_tazar_grail_found", "12345")]
        public void TheFiveObelisksOfTheMapAreVisitedInTheOrderTheSavefileListsThem(string save, string visited)
        {
            var visits = Visits(Load(save));

            Assert.That(visits.Count, Is.EqualTo(5));
            Assert.That(Enumerable.Range(1, 5).Where(n => visits.IsVisited(n - 1, PlayerColor.Red)).Select(n => n.ToString()), Is.EqualTo(visited.Select(c => c.ToString())));
            Assert.That(visits.VisitedCount(PlayerColor.Red), Is.EqualTo(visited.Length));
        }

        [Test]
        public void TheObeliskIndexOfTheHeroOnAnObeliskIsTheIndexOfTheFlag()
        {
            // The hero stands on obelisk 1, 2, 5, 4 and 3 (indexes 0, 1, 4, 3, 2) in the saves 68 and 72 to 75, and the flag of that obelisk is the one that is set.
            var order = new[] { ("roe_68_tazar_obelisk", 0), ("roe_72_tazar_obelisk_2", 1), ("roe_73_tazar_obelisk_3", 4), ("roe_74_tazar_obelisk_4", 3), ("roe_75_tazar_all_obelisks", 2) };
            var visited = new List<int>();
            foreach (var (save, index) in order)
            {
                visited.Add(index);
                Assert.That(Enumerable.Range(0, 5).Where(i => Visits(Load(save)).IsVisited(i, PlayerColor.Red)), Is.EquivalentTo(visited), save);
            }
        }

        [Test]
        public void ASavefileWithoutTheMarkerHasNoObelisksToShow()
        {
            var game = TestServices.GameService.LoadGameFromFile(Path.Combine("TestData", "Sod", "sod_game_00_baseline.GM1"));

            Assert.That(game.History.Current.Obelisks().Any, Is.False);
            Assert.That(game.History.Current.Obelisks().GetModel(), Is.Null);
        }

        [TestCaseSource(nameof(EverySavefile))]
        public void TheNumberOfObelisksIsTheNumberOfObeliskObjectsOfTheMap(string path)
        {
            var game = TestServices.GameService.LoadGameFromFile(path);
            if (!game.History.Current.Obelisks().Any)
            {
                Assert.Pass("no obelisks found in this savefile");
            }

            // An obelisk object on the map is its type (57), its definition and its number among the obelisks. A hero standing on one hides it behind the hero.
            var text = Encoding.Latin1.GetString(game.Bytes, 0, game.History.Current.Obelisks()[0].StartIndex);
            var numbers = Regex.Matches(text, "9\0\0\0[\u0000-ÿ]{2}([\u0000-/])\0\0\0", RegexOptions.Singleline).Select(m => (int)m.Groups[1].Value[0]).Distinct().ToList();
            var count = game.History.Current.Obelisks().GetModel()!.Count;

            Assert.That(numbers.Count, Is.InRange(count - 1, count), path);
            Assert.That(numbers, Has.All.LessThan(count), path);
        }

        [Test]
        public void MarkingAnObeliskIsPendingSavedAndRevertedLikeAnyOtherChange()
        {
            var game = Load("roe_72_tazar_obelisk_2");
            var original = game.History.Current.ToBytes();
            var session = game.History.CreateObeliskSession();

            session.Update(v => v.WithVisited(2, PlayerColor.Red, true));

            var change = game.History.GetPendingChanges().Single();
            Assert.That((change.OwnerName, change.Change.Description, change.Change.Before, change.Change.After), Is.EqualTo(("Obelisks", "Obelisk 3, Red", "not visited", "visited")));
            var after = game.History.Current.ToBytes();
            var changed = Enumerable.Range(0, original.Length).Where(i => original[i] != after[i]).ToList();
            Assert.That(changed, Is.EqualTo(new[] { game.History.Current.Obelisks()[0].StartIndex + ObeliskLayout.FlagsOffset + 2 }));
            Assert.That(Visits(TestServices.GameService.LoadGameFromBytes(after, "edited.GM1")).VisitedCount(PlayerColor.Red), Is.EqualTo(3));

            change.Revert(game.History);
            Assert.That(game.History.Current.ToBytes(), Is.EqualTo(original));
        }

        [Test]
        public void VisitingEveryObeliskAndClearingThemAgainRoundTrips()
        {
            var game = Load("roe_68_tazar_obelisk");
            var session = game.History.CreateObeliskSession();

            session.Update(v => v.WithAll(PlayerColor.Red, true));
            Assert.That(Visits(game).AllVisited(PlayerColor.Red), Is.True);
            Assert.That(game.History.GetPendingChanges(), Has.Count.EqualTo(4));

            session.Update(v => v.WithAll(PlayerColor.Red, false));
            Assert.That(Visits(game).VisitedCount(PlayerColor.Red), Is.EqualTo(0));
            Assert.That(game.History.GetPendingChanges().Single().Change.Description, Is.EqualTo("Obelisk 1, Red"));
            Assert.Throws<ArgumentOutOfRangeException>(() => Visits(game).WithVisited(5, PlayerColor.Red, true));
        }

        [Test]
        public void ThePageOfAPlayerShowsAndEditsItsObelisks()
        {
            var game = Load("roe_74_tazar_obelisk_4");
            using var player = new PlayerViewModel(game.History.CreatePlayerSession(game.History.Current.Players().FindByColor(PlayerColor.Red)), TestServices.ByteEditTargets);
            var obelisks = player.Obelisks;

            Assert.That(obelisks.HasObelisks, Is.True);
            Assert.That(obelisks.Summary, Is.EqualTo("4 of 5 visited"));
            Assert.That(obelisks.Obelisks.Select(o => o.IsVisited), Is.EqualTo(new[] { true, true, false, true, true }));

            obelisks.Obelisks[2].IsVisited = true;
            obelisks.Refresh();
            Assert.That(obelisks.Summary, Is.EqualTo("5 of 5 visited"));
            Assert.That(obelisks.VisitAllCommand.CanExecute(null), Is.False);

            obelisks.ClearAllCommand.Execute(null);
            Assert.That(obelisks.Summary, Is.EqualTo("0 of 5 visited"));
            Assert.That(obelisks.ClearAllCommand.CanExecute(null), Is.False);

            game.History.Undo();
            Assert.That(game.History.Current.Obelisks().GetModel()!.VisitedCount(PlayerColor.Red), Is.EqualTo(5));
        }

        [Test]
        public void ThePageOfAPlayerOfASavefileWithoutObelisksHidesTheCard()
        {
            var game = TestServices.GameService.LoadGameFromFile(Path.Combine("TestData", "Sod", "sod_game_00_baseline.GM1"));
            using var player = new PlayerViewModel(game.History.CreatePlayerSession(0), TestServices.ByteEditTargets);

            Assert.That(player.Obelisks.HasObelisks, Is.False);
            Assert.That(player.Obelisks.Obelisks, Is.Empty);
            Assert.That(player.Obelisks.Summary, Is.Empty);
        }

        [Test]
        public void TheOverviewNamesTheNumberOfObelisksAfterTheDifficultyAndNothingWithoutThem()
        {
            var with = new OverviewViewModel(Load("roe_74_tazar_obelisk_4"), _ => { }, _ => { });
            var without = new OverviewViewModel(TestServices.GameService.LoadGameFromFile(Path.Combine("TestData", "Sod", "sod_game_00_baseline.GM1")), _ => { }, _ => { });

            Assert.That(with.Summary, Does.EndWith("· 5 obelisks"));
            Assert.That(without.Summary, Does.Not.Contain("obelisk"));
        }

        [Test]
        public void TheObelisksOfTheMapAreLocatedByTheirRecordInTheListOfTheObjects()
        {
            // Where the hero stood on each obelisk in the saves 68 and 72 to 75: 1 (5,28), 2 (34,58), 5 (34,69), 4 (54,66) and 3 (9,61).
            var visits = Visits(Load("roe_00_baseline"));

            Assert.That(Enumerable.Range(0, 5).Select(i => visits.PositionOf(i)!.X + "," + visits.PositionOf(i)!.Y), Is.EqualTo(new[] { "5,28", "34,58", "9,61", "54,66", "34,69" }));
        }

        [Test]
        public void AnObeliskThatAHeroStandsOnHasNoPositionBecauseItsRecordIsHidden()
        {
            var visits = Visits(Load("roe_68_tazar_obelisk"));

            Assert.That(visits.PositionOf(0), Is.Null);
            Assert.That(visits.PositionOf(1), Is.Not.Null);
        }
    }
}
