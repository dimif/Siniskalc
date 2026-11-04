namespace Siniskalc.Homm2.Tests.Integration.ViewModels
{
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.ViewModels;

    /// <summary>
    /// The page of the map's shrines and sphinxes (Threedom).
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class MapViewModelTests
    {
        [Test]
        public void ThePageListsTheShrinesAndTheSphinxOfTheMap()
        {
            var page = new MapViewModel(TestServices.LoadThreedom(71));

            Assert.That(page.Shrines, Has.Count.EqualTo(13));
            Assert.That(page.Sphinxes, Has.Count.EqualTo(1));
            Assert.That(page.MapSummary, Is.EqualTo("13 shrines, 1 sphinx"));
        }

        [TestCase(13, 1, "13 shrines, 1 sphinx")]
        [TestCase(1, 2, "1 shrine, 2 sphinxes")]
        [TestCase(0, 3, "3 sphinxes")]
        [TestCase(4, 0, "4 shrines")]
        [TestCase(0, 0, "")]
        public void TheSummaryLeavesOutWhatTheMapDoesNotHave(int shrines, int sphinxes, string expected)
        {
            Assert.That(MapViewModel.Summarize(shrines, sphinxes), Is.EqualTo(expected));
        }

        [Test]
        public void TheBytesTabListsEveryRecordOfTheMapAndShowsTheFirst()
        {
            var game = TestServices.LoadThreedom(71);
            var document = game.History.Current;
            var page = new MapViewModel(game, new ByteEditTargetFactory());

            Assert.That(page.Records, Has.Count.EqualTo(document.Mines().Count + document.Shrines().Count + document.Sphinxes().Count + 3));
            Assert.That(page.Records.Select(r => r.Name), Does.Contain("Obelisks").And.Contain(document.Turn().GetName(0)).And.Contain(document.BuiltToday().GetName(0)));
            Assert.That(page.SelectedRecord, Is.SameAs(page.Records[0]));

            page.SelectedRecord = page.Records.Single(r => r.Name == "Obelisks");
            using var target = page.CreateByteEditTarget()!;
            Assert.That(target.Title, Is.EqualTo("Obelisks"));
        }

        [Test]
        public void WithoutAFactoryThePageHasNoBytes()
        {
            var page = new MapViewModel(TestServices.LoadThreedom(71));

            Assert.That(page.Records, Is.Empty);
            Assert.That(page.CreateByteEditTarget(), Is.Null);
        }

        [Test]
        public void TheShrinesAreSortedByCircleLocationOrSpellFromTheHeadings()
        {
            var page = new MapViewModel(TestServices.LoadThreedom(77));

            page.ShrineList.Columns["Name"].SortCommand.Execute(null);
            Assert.That(page.Shrines.Select(s => s.CircleNumber), Is.Ordered);

            page.ShrineList.Columns["Spell"].SortCommand.Execute(null);
            Assert.That(page.Shrines.Select(s => s.SpellName), Is.Ordered);

            page.ShrineList.Columns["X"].SortCommand.Execute(null);
            Assert.That(page.Shrines.Select(s => s.X), Is.Ordered);
        }

        [Test]
        public void ThePickedSpellOfAShrineIsTheOneItTeachesAndAHiddenShrineCannotBeChanged()
        {
            var game = TestServices.LoadThreedom(78);
            var page = new MapViewModel(game);

            var shield = page.Shrines.Single(s => s.PositionText == "at 45, 7");
            Assert.That(shield.SpellField.Selected, Is.EqualTo("Shield"));
            shield.SpellField.Selected = "Fireball";
            Assert.That(game.History.Current.Shrines().GetModels().Single(s => s.Position == new MapPosition(45, 7)).Spell, Is.EqualTo(TestCatalog.Standard.Spells["Fireball"]));

            var hidden = page.Shrines.Single(s => s.PositionText == "at 29, 42");
            Assert.That(hidden.SpellField.IsEnabled, Is.False);
            Assert.That(hidden.HasNote, Is.True);
        }

        [Test]
        public void TheSphinxAnswersAreEditedOnePlaceAtATimeAndAnEmptiedPlaceTakesTheAnswerAway()
        {
            var game = TestServices.LoadThreedom(74);
            var sphinx = new MapViewModel(game).Sphinxes.Single();

            Assert.That(sphinx.SolvedToggle.IsChecked, Is.True);
            Assert.That(sphinx.Answers.Select(a => a.Text).Take(3), Is.EqualTo(new[] { "darkness", "shadow", string.Empty }));

            sphinx.SolvedToggle.IsChecked = false;
            sphinx.Answers[2].Text = "shade";
            sphinx.Answers[0].Text = string.Empty;
            sphinx.Reward.Single(r => r.Name == "Gold").Value = 5000;

            var model = game.History.Current.Sphinxes().GetModel(0);
            Assert.That(model.IsActive, Is.True);
            Assert.That(model.Answers, Is.EqualTo(new[] { "shadow", "shade" }));
            Assert.That(model.Reward[6], Is.EqualTo(5000));
            Assert.That(sphinx.Riddle.Counter, Does.StartWith("197 of 198").Or.Contain(" of "));
        }
    }
}
