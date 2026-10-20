namespace Siniskalc.Heroes.Wpf.Tests.ViewModels
{
    using Moq;
    using Siniskalc.Core.Runtime;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.Wpf.ViewModels;

    /// <summary>
    /// Tests the words the overview page of any game says about the savefile that is open.
    /// </summary>
    [TestFixture]
    internal sealed class OverviewViewModelBaseTests
    {
        private static AbstractSavegame Savegame(string? name)
        {
            // The overview only reads the name of the savegame; the bytes and the history are never touched.
            return new Mock<AbstractSavegame>("ignored.GM1", Array.Empty<byte>(), null!, name).Object;
        }

        [Test]
        public void ThePageNamesTheFileOfTheSavegameWithoutItsFolder()
        {
            Assert.That(new TestOverview(Savegame(@"C:\saves\Arrax.GM1")).FileName, Is.EqualTo("Arrax.GM1"));
        }

        [Test]
        public void AnUnnamedSavegameIsCalledSavegame()
        {
            Assert.That(new TestOverview(Savegame(string.Empty)).FileName, Is.EqualTo("Savegame"));
        }

        [Test]
        public void WhatTheSavefileDoesNotTellIsLeftEmpty()
        {
            var page = new TestOverview(Savegame("a.GM1"));

            Assert.That(
                new[] { page.MapName, page.MapDescription, page.MapSizeText, page.MapDifficultyText, page.DateText, page.TurnText },
                Is.All.Empty);
            Assert.That(page.HasMapDescription, Is.False);
        }

        [Test]
        public void TheMapIsDescribedWithItsNameDescriptionSizeAndRating()
        {
            var page = new TestOverview(Savegame("a.GM1")) { MapHeader = new MapHeader(" Arrax the Jerk ", " A map. ", GameDifficulty.Rook, 36) };

            Assert.That(
                (page.MapName, page.MapDescription, page.HasMapDescription, page.MapSizeText, page.MapDifficultyText),
                Is.EqualTo(("Arrax the Jerk", "A map.", true, "Small (36 x 36)", "Rating: Rook (130%)")));
        }

        [Test]
        public void AGameCanAddToTheSizeOfTheMap()
        {
            var page = new TestOverview(Savegame("a.GM1")) { MapHeader = new MapHeader("m", string.Empty, GameDifficulty.Pawn, 36), MoreSize = " · 2 levels" };

            Assert.That(page.MapSizeText, Is.EqualTo("Small (36 x 36) · 2 levels"));
        }

        [Test]
        public void TheDateTheDifficultyAndTheTurnAreSaidInWords()
        {
            var page = new TestOverview(Savegame("a.GM1")) { GameDate = new GameDate(5, 4, 6), GameDifficulty = GameDifficulty.Queen, Player = "Tan" };

            Assert.That((page.DateText, page.DifficultyText, page.TurnText), Is.EqualTo(("Month 5, Week 4, Day 6", "Difficulty: Queen (160%)", "Current turn: Tan")));
        }

        [Test]
        public void TheSummaryCountsThePlayersAndTheHeroesAndSkipsWhatIsEmpty()
        {
            Assert.That(TestOverview.Summarize(1, 3, string.Empty, "Difficulty: King (200%)"), Is.EqualTo("1 player · 3 heroes · Difficulty: King (200%)"));
        }

        private sealed class TestOverview(AbstractSavegame savegame) : OverviewViewModelBase(savegame)
        {
            public MapHeader? MapHeader { get; init; }

            public GameDate? GameDate { get; init; }

            public GameDifficulty? GameDifficulty { get; init; }

            public string? Player { get; init; }

            public string MoreSize { get; init; } = string.Empty;

            public override string VersionName => "Test";

            protected override MapHeader? Map => this.MapHeader;

            protected override GameDate? Date => this.GameDate;

            protected override GameDifficulty? Difficulty => this.GameDifficulty;

            protected override string? CurrentPlayerName => this.Player;

            public static string Summarize(int players, int heroes, params string[] more) => SummaryOf(players, heroes, more);

            protected override string SizeOf(MapHeader map) => map.SizeText + this.MoreSize;
        }
    }
}
