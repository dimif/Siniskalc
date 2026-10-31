namespace Siniskalc.Homm2.Tests.Integration.Data
{
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Runtime;

    /// <summary>
    /// Who plays each colour and whose turn it is, read from the saves of <c>TestData/Homm2/HumanCpu</c>.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class PlayerControlTests
    {
        private static Game Load(string name) => TestServices.Load(Directory.GetFiles(Path.Combine("TestData", "Homm2", "HumanCpu"), name + ".*").Single());

        private static string Controls(Game game) => string.Join(" ", PlayerColor.All.Select(c => game.GetControl(c) switch { PlayerType.Human => "H", PlayerType.Computer => "C", _ => "-" }));

        [TestCase("human_cpu_02_blue_human", "H C C C C C", "Blue")]
        [TestCase("human_cpu_03_green_human", "C H C C C C", "Green")]
        [TestCase("human_cpu_04_orange_human", "C C C C H C", "Orange")]
        [TestCase("human_cpu_01_four_humans", "H H H H C C", "Blue")]
        public void TheHumanPlayersAndTheCurrentOneAreReadFromTheHeader(string save, string controls, string currentName)
        {
            var current = PlayerColor.All.Single(c => c.Name == currentName);
            var game = Load(save);

            Assert.That(Controls(game), Is.EqualTo(controls));
            Assert.That(game.CurrentPlayer, Is.EqualTo(current));
        }

        // The header and the turn record are the same byte: the number of the record of the human whose turn it is. The reader of the
        // header read it too (as the human the mask marks at that record); on every save it named the same colour as the turn record
        // (Game.CurrentPlayer), so only the turn record, which can be edited, is kept.
        [Test]
        public void TheCurrentPlayerOfEverySaveIsTheHumanWhoseRecordTheTurnByteNames()
        {
            foreach (var path in Saves.All())
            {
                var game = TestServices.Load(path);
                var bytes = game.Bytes.ToArray();
                var turn = bytes[game.Layout.CurrentHumanStart];
                var expected = turn < PlayerColor.All.Count && bytes[game.Layout.HumanMaskStart + turn] != 0
                    ? PlayerColor.All[PlayerRecordOrder.ColorsOf(bytes, game.Layout)[turn]]
                    : null;

                Assert.That(game.CurrentPlayer, Is.EqualTo(expected), Path.GetFileName(path));
            }
        }

        [TestCase("human_cpu_12_two_humans_blue_turn_day_1", "Blue")]
        [TestCase("human_cpu_13_two_humans_green_turn_day_1", "Green")]
        [TestCase("human_cpu_14_two_humans_blue_turn_day_2", "Blue")]
        [TestCase("human_cpu_15_two_humans_green_turn_day_2", "Green")]
        [TestCase("human_cpu_16_two_humans_blue_turn_day_3", "Blue")]
        [TestCase("human_cpu_17_autosave_green_turn_day_2", "Green")]
        public void InAHotSeatGameOfTwoHumansTheTurnPassesFromOneToTheOther(string save, string currentName)
        {
            var current = PlayerColor.All.Single(c => c.Name == currentName);
            var game = Load(save);

            Assert.That(Controls(game), Is.EqualTo("H H C C C C"));
            Assert.That(game.CurrentPlayer, Is.EqualTo(current));
        }

        [TestCase("human_cpu_18_green_orange_green_turn_day_1", "Green")]
        [TestCase("human_cpu_19_green_orange_orange_turn_day_1", "Orange")]
        [TestCase("human_cpu_20_green_orange_green_turn_day_2", "Green")]
        [TestCase("human_cpu_21_green_orange_orange_turn_day_2", "Orange")]
        [TestCase("human_cpu_22_green_orange_green_turn_day_3", "Green")]
        public void TheCurrentPlayerIsTheHumanWhoseNumberTheByteHoldsNotTheColour(string save, string currentName)
        {
            var current = PlayerColor.All.Single(c => c.Name == currentName);
            var game = Load(save);

            Assert.That(Controls(game), Is.EqualTo("C H C C H C"), "Green is human 0 and Orange human 1: the turn byte is 0 and 1, not 1 and 4");
            Assert.That(game.CurrentPlayer, Is.EqualTo(current));
        }

        [TestCase("human_cpu_05_six_humans_blue_turn", "Blue")]
        [TestCase("human_cpu_06_green_turn", "Green")]
        [TestCase("human_cpu_07_red_turn", "Red")]
        [TestCase("human_cpu_08_yellow_turn", "Yellow")]
        [TestCase("human_cpu_09_orange_turn", "Orange")]
        [TestCase("human_cpu_10_purple_turn", "Purple")]
        [TestCase("human_cpu_11_autosave_orange_turn", "Orange")]
        public void InAGameOfSixHumansTheTurnGoesRoundTheColours(string save, string currentName)
        {
            var current = PlayerColor.All.Single(c => c.Name == currentName);
            var game = Load(save);

            Assert.That(Controls(game), Is.EqualTo("H H H H H H"));
            Assert.That(game.CurrentPlayer, Is.EqualTo(current));
        }

        [TestCase("human_cpu_04_orange_human", "Orange", 0)]
        [TestCase("human_cpu_03_green_human", "Green", 0)]
        [TestCase("human_cpu_18_green_orange_green_turn_day_1", "Orange", 1)]
        public void TheRecordsOfThePlayersAreInTheOrderTheyPlayAndEachSaysItsColour(string save, string colorName, int record)
        {
            var color = PlayerColor.All.Single(c => c.Name == colorName);
            var game = Load(save);
            var players = game.History.Current.Players();

            Assert.That(players.GetModel(color.Code).Color, Is.EqualTo(color));
            Assert.That(players[color.Code].StartIndex, Is.EqualTo(game.Layout.PlayerStart(record)), "the human colours come first in the table");
            Assert.That(players.GetModels().Select(p => p.Color), Is.EqualTo(PlayerColor.All));
        }

        [Test]
        public void WhoPlaysIsReadFromTheMaskTheGamePlaysFromNotFromTheControlsOfTheSummary()
        {
            var bytes = File.ReadAllBytes(Directory.GetFiles(Path.Combine("TestData", "Homm2", "HumanCpu"), "human_cpu_18*").Single());
            var layout = TestServices.Parser.Parse(bytes, null).Layout;
            bytes[layout.HumanMaskStart + 1] = 0;

            var game = TestServices.Parser.Parse(bytes, null);

            Assert.That(Controls(game), Is.EqualTo("C H C C C C"), "the controls at 0x1BE still say Orange is human; the game, which follows the mask, plays Green alone");
        }

        [Test]
        public void EveryPlayerOfEverySaveIsFoundByItsColour()
        {
            foreach (var path in Saves.All())
            {
                var players = TestServices.Load(path).History.Current.Players();

                Assert.That(players.GetModels().Select(p => p.Color), Is.EqualTo(PlayerColor.All), path);
            }
        }

        [Test]
        public void TheOrderOfPlayIsTheSuccessionOfTheColoursAndGivingTheTurnWritesTheRecordAndCanBeReset()
        {
            var game = Load("human_cpu_18_green_orange_green_turn_day_1");
            var overview = new Siniskalc.Homm2.ViewModels.OverviewViewModel(game, new Siniskalc.Heroes.Wpf.ViewModels.BanishingTextProvider(), _ => { }, _ => { }, _ => { });

            Assert.That(overview.OrderOfPlay.Select(i => i.Name), Is.EqualTo(new[] { "Blue", "Green", "Red", "Yellow", "Orange", "Purple" }));
            Assert.That(overview.OrderOfPlay.Where(i => i.IsCurrent).Select(i => i.Name), Is.EqualTo(new[] { "Green" }));
            Assert.That(overview.OrderOfPlay.Where(i => i.Command != null).Select(i => i.Name), Is.EqualTo(new[] { "Orange" }), "only a human who has not the turn can be given it");

            overview.OrderOfPlay.Single(i => i.Name == "Orange").Command!.Execute(null);
            overview.Refresh();

            Assert.That(game.CurrentPlayer, Is.EqualTo(PlayerColor.Orange));
            Assert.That(overview.OrderOfPlay.Single(i => i.IsCurrent).Name, Is.EqualTo("Orange"));
            Assert.That(overview.TurnText, Is.EqualTo("Current turn: Orange"));
            Assert.That(game.GetBytesToSave()[game.Layout.CurrentHumanStart], Is.EqualTo(1), "Orange is the second record");
            Assert.That(game.History.GetPendingChanges().Single().Change.After, Is.EqualTo("Orange"));
            Assert.That(TestServices.Parser.Parse(game.GetBytesToSave(), null).CurrentPlayer, Is.EqualTo(PlayerColor.Orange));

            game.History.GetPendingChanges().Single().Revert(game.History);
            Assert.That(game.CurrentPlayer, Is.EqualTo(PlayerColor.Green));
        }

        [Test]
        public void EverySaveWithAHumanSaysWhoseTurnItIsAndTheOtherFormatHasTheTurnByteFiveFurtherOn()
        {
            foreach (var path in Saves.All())
            {
                var game = TestServices.Load(path);

                Assert.That(game.CurrentPlayer, Is.Not.Null, path);
                Assert.That(game.GetControl(game.CurrentPlayer!), Is.EqualTo(PlayerType.Human), path);
            }

            Assert.That(TestServices.LoadThreedom(1).CurrentPlayer, Is.EqualTo(PlayerColor.Blue));
        }
    }
}
