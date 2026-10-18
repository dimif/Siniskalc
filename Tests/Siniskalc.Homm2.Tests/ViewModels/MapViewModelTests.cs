namespace Siniskalc.Homm2.Tests.ViewModels
{
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.ViewModels;

    /// <summary>
    /// The page of the map's mines, shrines and sphinxes (Threedom).
    /// </summary>
    [TestFixture]
    internal sealed class MapViewModelTests
    {
        [Test]
        public void ThePageListsTheMinesTheShrinesAndTheSphinxOfTheMap()
        {
            var page = new MapViewModel(TestServices.LoadThreedom(71));

            Assert.That(page.Mines, Has.Count.EqualTo(41));
            Assert.That(page.Shrines, Has.Count.EqualTo(13));
            Assert.That(page.Sphinxes, Has.Count.EqualTo(1));
            Assert.That(page.MinesSummary, Is.EqualTo("41 mines, 22 of them owned"));
            Assert.That(page.Mines.Select(m => m.Kind), Does.Contain("Sawmill").And.Contain("Gold mine").And.Contain("Alchemist's lab"));
        }

        [Test]
        public void TheMinesAreSortedByResourceOwnerOrLocationFromTheHeadings()
        {
            var page = new MapViewModel(TestServices.LoadThreedom(71));

            page.MineList.Columns["X"].SortCommand.Execute(null);
            Assert.That(page.Mines.Select(m => m.X), Is.Ordered);
            Assert.That(page.Mines[0].X, Is.LessThanOrEqualTo(page.Mines[1].X));

            page.MineList.Columns["X"].SortCommand.Execute(null);
            Assert.That(page.Mines.Select(m => m.X), Is.Ordered.Descending);

            page.MineList.Columns["Resource"].SortCommand.Execute(null);
            Assert.That(page.Mines.Select(m => m.ResourceName), Is.Ordered);

            page.MineList.Columns["Owner"].SortCommand.Execute(null);
            Assert.That(page.Mines.Select(m => m.OwnerField.Selected), Is.Ordered);
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
        public void PickingAnOwnerForAMineChangesItsRecordAndFlagAndTheCountAndCanBeReset()
        {
            var game = TestServices.LoadThreedom(71);
            var page = new MapViewModel(game);
            var mine = page.Mines.Single(m => m.PositionText == "X: 34, Y: 53");

            mine.OwnerField.Selected = "Green";

            Assert.That(game.History.Current.Mines().GetModels().Single(m => m.Position == new MapPosition(34, 53)).Owner, Is.EqualTo(Color.Green));
            Assert.That(page.HasPendingChanges, Is.True);
            page.ResetCommand.Execute(null);
            Assert.That(game.History.Current.Mines().GetModels().Single(m => m.Position == new MapPosition(34, 53)).Owner, Is.EqualTo(Color.Blue));
            Assert.That(page.HasPendingChanges, Is.False);
        }

        [Test]
        public void ThePickedSpellOfAShrineIsTheOneItTeachesAndAHiddenShrineCannotBeChanged()
        {
            var game = TestServices.LoadThreedom(78);
            var page = new MapViewModel(game);

            var shield = page.Shrines.Single(s => s.PositionText == "X: 45, Y: 7");
            Assert.That(shield.SpellField.Selected, Is.EqualTo("Shield"));
            shield.SpellField.Selected = "Fireball";
            Assert.That(game.History.Current.Shrines().GetModels().Single(s => s.Position == new MapPosition(45, 7)).Spell, Is.EqualTo(Spell.Fireball));

            var hidden = page.Shrines.Single(s => s.PositionText == "X: 29, Y: 42");
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
