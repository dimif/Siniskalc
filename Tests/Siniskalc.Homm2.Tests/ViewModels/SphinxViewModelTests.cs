namespace Siniskalc.Homm2.Tests.ViewModels
{
    using Moq;
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;
    using Siniskalc.Homm2.ViewModels;

    /// <summary>
    /// Tests the page of a sphinx on the map of HoMM2: whether it is solved, the reward in resources and in an artifact, the answers it
    /// accepts and its riddle.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.MultiUnit)]
    internal sealed class SphinxViewModelTests
    {
        private static SphinxModel Sphinx(bool active = true, IReadOnlyList<string>? answers = null, int artifact = SphinxModel.NoArtifact) =>
            new(2, new MapPosition(7, 8), active, [10, 20, 30, 40, 50, 60, 7000], artifact, answers ?? ["Time", "Moon"], "What is it?", 50);

        private static Mock<IEditSession<SphinxModel>> Session(SphinxModel sphinx)
        {
            var current = sphinx;
            var session = new Mock<IEditSession<SphinxModel>>();
            session.SetupGet(s => s.Index).Returns(sphinx.Index);
            session.SetupGet(s => s.Current).Returns(() => current);
            session.SetupGet(s => s.Original).Returns(sphinx);
            session.Setup(s => s.Update(It.IsAny<Func<SphinxModel, SphinxModel>>())).Callback((Func<SphinxModel, SphinxModel> edit) => current = edit(current));
            return session;
        }

        private static SphinxViewModel Page(Mock<IEditSession<SphinxModel>> session) => new(session.Object, GameVersion.BaseGame);

        [Test]
        public void ThePageHasTheNameAndIndexOfTheSphinx()
        {
            var page = Page(Session(Sphinx()));

            Assert.That((page.Index, page.Title), Is.EqualTo((2, "Sphinx at X: 7, Y: 8")));
        }

        [Test]
        public void ASphinxIsSolvedWhenItIsNoLongerActiveAndTickingItWritesTheOpposite()
        {
            var session = Session(Sphinx(active: false));
            var page = Page(session);

            Assert.That(page.SolvedToggle.IsChecked, Is.True);

            page.SolvedToggle.IsChecked = false;
            Assert.That(session.Object.Current.IsActive, Is.True);

            page.SolvedToggle.IsChecked = true;
            Assert.That(session.Object.Current.IsActive, Is.False);
        }

        [Test]
        public void TheRewardHasARowForEachResourceGoldFirstWithTheValueOfThatResourceInTheSavegame()
        {
            var page = Page(Session(Sphinx()));

            Assert.That(page.Reward.Select(r => r.Name), Is.EqualTo(new[] { "Gold", "Wood", "Ore", "Mercury", "Sulfur", "Crystal", "Gems" }));
            Assert.That(page.Reward.Select(r => r.Value), Is.EqualTo(new long[] { 7000, 10, 30, 20, 40, 50, 60 }));
            Assert.That((page.Reward[0].Maximum, page.Reward[0].Step), Is.EqualTo((Constants.Player.MaxGold, Constants.Player.GoldStep)));
            Assert.That((page.Reward[1].Maximum, page.Reward[1].Step), Is.EqualTo((Constants.Player.MaxOther, Constants.Player.OtherStep)));
        }

        [Test]
        public void ChangingAResourceOfTheRewardWritesOnlyThatOne()
        {
            var session = Session(Sphinx());
            var page = Page(session);

            page.Reward[3].Value = 9;

            Assert.That(session.Object.Current.Reward, Is.EqualTo(new[] { 10, 9, 30, 40, 50, 60, 7000 }));
        }

        [Test]
        public void TheArtifactIsPickedByNameAndClearedToNone()
        {
            var session = Session(Sphinx());
            var page = Page(session);

            var finder = (ArtifactFinderViewModel)page.ArtifactPicker.Finder;
            finder.Choose(finder.Items.Cast<ArtifactChoiceViewModel>().First(c => c.Name == Artifact.UltimateSwordOfDominion.Name));
            Assert.That(session.Object.Current.ArtifactCode, Is.EqualTo(Artifact.UltimateSwordOfDominion.Code));

            page.ArtifactPicker.ClearCommand.Execute(null);
            Assert.That(session.Object.Current.ArtifactCode, Is.EqualTo(SphinxModel.NoArtifact));
        }

        [Test]
        public void AnAnswerIsWrittenTrimmedInItsPlaceAndEmptyingItRemovesIt()
        {
            var session = Session(Sphinx());
            var page = Page(session);

            Assert.That(page.Answers.Select(a => a.Text), Is.EqualTo(new[] { "Time", "Moon", string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty }));

            page.Answers[2].Text = "  Sun ";
            Assert.That(session.Object.Current.Answers, Is.EqualTo(new[] { "Time", "Moon", "Sun" }));
            Assert.That(page.Answers.Select(a => a.MaxLength), Is.All.EqualTo(MapObjectLayout.SphinxAnswerSize - 1), "an answer has room for twelve characters");

            page.Answers[0].Text = string.Empty;
            Assert.That(session.Object.Current.Answers, Is.EqualTo(new[] { "Moon", "Sun" }), "an empty place drops out");
        }

        [Test]
        public void TheLastAnswerCannotBeEmptied()
        {
            var session = Session(Sphinx(answers: ["Only"]));
            var page = Page(session);

            page.Answers[0].Text = string.Empty;

            Assert.That(session.Object.Current.Answers, Is.EqualTo(new[] { "Only" }));
        }

        [Test]
        public void TheRiddleIsWrittenAndHasTheRoomOfTheOriginalOne()
        {
            var session = Session(Sphinx());
            var page = Page(session);

            Assert.That((page.Riddle.Text, page.Riddle.MaxLength), Is.EqualTo(("What is it?", 50)));

            page.Riddle.Text = "Who am I?";

            Assert.That(session.Object.Current.Riddle, Is.EqualTo("Who am I?"));
        }

        [Test]
        public void RefreshShowsWhatTheSessionNowHoldsIncludingTheArtifact()
        {
            var session = Session(Sphinx());
            var page = Page(session);
            var raised = new List<string?>();
            page.PropertyChanged += (_, e) => raised.Add(e.PropertyName);
            session.Object.Update(s => s with { IsActive = false, ArtifactCode = Artifact.UltimateBookOfKnowledge.Code, Riddle = "New", Answers = ["Sun"] });
            var told = new List<string>();
            page.SolvedToggle.PropertyChanged += (_, _) => told.Add("solved");
            page.Riddle.PropertyChanged += (_, _) => told.Add("riddle");
            page.Answers[0].PropertyChanged += (_, _) => told.Add("answer");
            page.Reward[0].PropertyChanged += (_, _) => told.Add("reward");

            page.Refresh();

            Assert.That(told.Distinct(), Is.EquivalentTo(new[] { "solved", "riddle", "answer", "reward" }));
            Assert.That(page.SolvedToggle.IsChecked, Is.True);
            Assert.That(page.ArtifactPicker.ArtifactName, Is.EqualTo(Artifact.UltimateBookOfKnowledge.Name));
            Assert.That(page.Answers[0].Text, Is.EqualTo("Sun"));
            Assert.That(page.Answers[1].Text, Is.Empty);
            Assert.That(page.Riddle.Text, Is.EqualTo("New"));
            Assert.That(raised, Is.EqualTo(new[] { string.Empty }));
        }

        [Test]
        public void AFreshPageOfASphinxWithNoArtifactShowsNoArtifact()
        {
            var page = Page(Session(Sphinx()));

            page.Refresh();

            Assert.That(page.ArtifactPicker.HasArtifact, Is.False);
        }
    }
}
