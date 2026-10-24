namespace Siniskalc.Heroes.Wpf.Tests.ViewModels
{
    using Moq;
    using Siniskalc.Core.Runtime;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.Runtime;
    using Siniskalc.Heroes.Wpf.ViewModels;

    /// <summary>
    /// Tests the words the overview page of any game says about the savefile that is open.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class OverviewViewModelBaseTests
    {
        private static AbstractSavegame Savegame(string? name)
        {
            // The overview only reads the name of the savegame; the bytes and the history are never touched.
            return new Mock<AbstractSavegame>("ignored.GM1", Array.Empty<byte>(), null!, name!).Object;
        }

        private static IGameInfo<TestColor> Info(MapHeader? map = null, GameDate? date = null, GameDifficulty? difficulty = null, TestColor? player = null)
        {
            var info = new Mock<IGameInfo<TestColor>>();
            info.Setup(i => i.Map).Returns(map);
            info.Setup(i => i.Date).Returns(date);
            info.Setup(i => i.Difficulty).Returns(difficulty);
            info.Setup(i => i.CurrentPlayer).Returns(player);
            return info.Object;
        }

        [Test]
        public void ThePageNamesTheFileOfTheSavegameWithoutItsFolder()
        {
            Assert.That(new TestOverview(Savegame(@"C:\saves\Arrax.GM1"), Info()).FileName, Is.EqualTo("Arrax.GM1"));
        }

        [Test]
        public void AnUnnamedSavegameIsCalledSavegame()
        {
            Assert.That(new TestOverview(Savegame(string.Empty), Info()).FileName, Is.EqualTo("Savegame"));
        }

        [Test]
        public void WhatTheSavefileDoesNotTellIsLeftEmpty()
        {
            var page = new TestOverview(Savegame("a.GM1"), Info());

            Assert.That(
                new[] { page.MapName, page.MapDescription, page.MapSizeText, page.MapDifficultyText, page.DateText, page.TurnText },
                Is.All.Empty);
            Assert.That(page.HasMapDescription, Is.False);
        }

        [Test]
        public void TheMapIsDescribedWithItsNameDescriptionSizeAndRating()
        {
            var page = new TestOverview(Savegame("a.GM1"), Info(map: new MapHeader(" Arrax the Jerk ", " A map. ", GameDifficulty.Rook, 36)));

            Assert.That(
                (page.MapName, page.MapDescription, page.HasMapDescription, page.MapSizeText, page.MapDifficultyText),
                Is.EqualTo(("Arrax the Jerk", "A map.", true, "Small (36 x 36)", "Rating: Rook (130%)")));
        }

        [Test]
        public void AGameCanAddToTheSizeOfTheMap()
        {
            var page = new TestOverview(Savegame("a.GM1"), Info(map: new MapHeader("m", string.Empty, GameDifficulty.Pawn, 36))) { MoreSize = " · 2 levels" };

            Assert.That(page.MapSizeText, Is.EqualTo("Small (36 x 36) · 2 levels"));
        }

        [Test]
        public void TheDateTheDifficultyAndTheTurnAreSaidInWords()
        {
            var page = new TestOverview(Savegame("a.GM1"), Info(date: new GameDate(5, 4, 6), difficulty: GameDifficulty.Queen, player: TestColor.Blue));

            Assert.That((page.DateText, page.DifficultyText, page.TurnText), Is.EqualTo(("Month 5, Week 4, Day 6", "Difficulty: Queen (160%)", "Current turn: Blue")));
        }

        [Test]
        public void TheSummaryCountsThePlayersAndTheHeroesAndSkipsWhatIsEmpty()
        {
            Assert.That(TestOverview.Summarize(1, 3, string.Empty, "Difficulty: King (200%)"), Is.EqualTo("1 player · 3 heroes · Difficulty: King (200%)"));
        }

        [Test]
        public void AHumanWhoHasNotTheTurnCanBeGivenItAndThePlayerWhoHasItCannot()
        {
            var info = new Mock<IGameInfo<TestColor>>();
            info.Setup(i => i.CurrentPlayer).Returns(TestColor.Red);
            info.Setup(i => i.GetControl(It.IsAny<TestColor>())).Returns(PlayerType.Human);
            var page = new TestOverview(Savegame("a.GM1"), info.Object);

            var red = page.ItemOf(TestColor.Red);
            var blue = page.ItemOf(TestColor.Blue);
            blue.Command!.Execute(null);

            Assert.That((red.Name, red.IsCurrent, red.Command, red.ToolTip), Is.EqualTo(("Red", true, (System.Windows.Input.ICommand?)null, "It is this player's turn")));
            Assert.That((blue.Name, blue.IsCurrent, blue.ToolTip), Is.EqualTo(("Blue", false, "Give the turn to this player")));
            Assert.That(page.GivenTo, Is.EqualTo(new[] { TestColor.Blue }));
        }

        [Test]
        public void AGameCanSwitchTheButtonOffAndPressingItThenChangesNothing()
        {
            var info = new Mock<IGameInfo<TestColor>>();
            info.Setup(i => i.CurrentPlayer).Returns(TestColor.Red);
            info.Setup(i => i.GetControl(It.IsAny<TestColor>())).Returns(PlayerType.Human);
            var page = new TestOverview(Savegame("a.GM1"), info.Object) { GivingTheTurnIsOn = false };

            var blue = page.ItemOf(TestColor.Blue);

            Assert.That(blue.Command!.CanExecute(null), Is.False);
            Assert.That(page.GivenTo, Is.Empty);
        }

        [Test]
        public void AComputerPlayerCannotBeGivenTheTurnAndHasTheTooltipTheGameGivesIt()
        {
            var info = new Mock<IGameInfo<TestColor>>();
            info.Setup(i => i.CurrentPlayer).Returns(TestColor.Red);
            info.Setup(i => i.GetControl(TestColor.Red)).Returns(PlayerType.Human);
            info.Setup(i => i.GetControl(TestColor.Blue)).Returns(PlayerType.Computer);

            var blue = new TestOverview(Savegame("a.GM1"), info.Object).ItemOf(TestColor.Blue);

            Assert.That((blue.IsCurrent, blue.Command, blue.ToolTip), Is.EqualTo((false, (System.Windows.Input.ICommand?)null, "A computer")));
        }

        private sealed class TestOverview(AbstractSavegame savegame, IGameInfo<TestColor> info) : OverviewViewModelBase<TestColor>(savegame, info)
        {
            public string MoreSize { get; init; } = string.Empty;

            public override string VersionName => "Test";

            public List<TestColor> GivenTo { get; } = [];

            public bool GivingTheTurnIsOn { get; init; } = true;

            protected override bool CanGiveTurn => this.GivingTheTurnIsOn;

            protected override string ComputerTurnToolTip => "A computer";

            public static string Summarize(int players, int heroes, params string[] more) => SummaryOf(players, heroes, more);

            public OrderOfPlayItemViewModel ItemOf(TestColor color) => this.OrderItemOf(color);

            protected override string SizeOf(MapHeader map) => map.SizeText + this.MoreSize;

            protected override void GiveTurn(TestColor color) => this.GivenTo.Add(color);
        }
    }
}
