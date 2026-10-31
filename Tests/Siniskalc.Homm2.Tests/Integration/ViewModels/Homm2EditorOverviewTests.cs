namespace Siniskalc.Homm2.Tests.Integration.ViewModels
{
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;
    using Siniskalc.Homm2.ViewModels;

    /// <summary>
    /// Tests the overview page of the editor, on a Broken Alliance savegame loaded into the editor.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class Homm2EditorOverviewTests
    {
        private Homm2EditorHarness harness = null!;
        private Homm2EditorViewModel editor = null!;

        [SetUp]
        public void LoadASavegame()
        {
            this.harness = new Homm2EditorHarness();
            this.editor = this.harness.Editor;
        }

        [TearDown]
        public void DeleteFolder() => this.harness.Dispose();

        [Test]
        public void EachPlayerCardOfTheOverviewHoldsTheResourcesHeroesAndCastlesOfThePlayer()
        {
            var document = this.editor.CurrentGame!.History.Current;
            var blue = this.editor.Overview!.Players.Single(p => p.Name == "Blue");
            var model = document.Players().GetModel(PlayerColor.Blue.Code);

            Assert.That(blue.HasResources, Is.True);
            Assert.That(blue.Gold, Is.EqualTo(model.Resources.Gold.ToString("N0")));
            Assert.That(blue.Heroes.Select(h => h.Name), Is.EquivalentTo(document.Heroes().GetModels().Where(h => h.Owner == PlayerColor.Blue).Select(h => h.Name)));
            Assert.That(blue.Castles.Select(c => c.Name), Is.EquivalentTo(document.Castles().GetModels().Where(c => c.IsUsed && c.Owner == PlayerColor.Blue).Select(c => c.Name)));
            Assert.That(blue.Castles.Select(c => c.Status), Has.All.Not.Empty);
        }

        [Test]
        public void TheOverviewOpensThePageOfAPlayer()
        {
            this.editor.Overview!.Players.Single(p => p.Name == "Green").OpenCommand.Execute(null);

            Assert.That(((PlayerViewModel)this.editor.CurrentPage!).Color, Is.EqualTo(PlayerColor.Green));
        }

        [Test]
        public void TheOverviewRefreshesWhileShownAndAfterUndoAndRedoAwayFromIt()
        {
            var original = this.editor.Overview!.Players.Single(p => p.Name == "Blue").Gold;
            var session = this.editor.CurrentGame!.History.CreatePlayerSession(PlayerColor.Blue.Code);

            session.Update(player => player with { Resources = player.Resources.With(Resource.Gold, 7) });
            Assert.That(this.editor.Overview.Players.Single(p => p.Name == "Blue").Gold, Is.EqualTo("7"));

            this.editor.ShowPlayer(PlayerColor.Blue);
            this.editor.Undo();
            this.editor.ShowOverview();
            Assert.That(this.editor.Overview.Players.Single(p => p.Name == "Blue").Gold, Is.EqualTo(original));

            this.editor.ShowPlayer(PlayerColor.Blue);
            this.editor.Redo();
            this.editor.ShowOverview();
            Assert.That(this.editor.Overview.Players.Single(p => p.Name == "Blue").Gold, Is.EqualTo("7"));
        }
    }
}
