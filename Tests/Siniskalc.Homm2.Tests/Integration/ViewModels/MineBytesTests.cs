namespace Siniskalc.Homm2.Tests.Integration.ViewModels
{
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.ViewModels;
    using Siniskalc.Heroes.Wpf.ViewModels;

    /// <summary>
    /// The bytes of the mine that is picked on the Mines tab of a player and of Neutral, on a Broken Alliance savegame loaded into the editor.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class MineBytesTests
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
        public void TheMinesTabOfAPlayerShowsTheBytesOfThePickedMineWithTheirFields()
        {
            var color = PlayerColor.All.First(c => this.OwnedMines(c) > 0);
            this.editor.ShowPlayer(color, OwnerTabs.Mines);
            var page = (PlayerViewModel)this.editor.CurrentPage!;

            using var target = page.CreateMineByteEditTarget();

            Assert.That(target, Is.Not.Null);
            Assert.That(target!.Regions.Select(r => r.Name), Does.Contain("Owner").And.Contain("Kind"));
            Assert.That(target.Title, Does.Contain(page.Mines.SelectedMine!.Kind));
        }

        [Test]
        public void ChangingTheOwnerByteOfAMineMovesItToTheListOfTheNewOwner()
        {
            var color = PlayerColor.All.First(c => this.OwnedMines(c) > 0);
            this.editor.ShowPlayer(color, OwnerTabs.Mines);
            var page = (PlayerViewModel)this.editor.CurrentPage!;
            var mine = page.Mines.SelectedMine!;
            var before = page.Mines.Rows.Count;

            using (var target = page.CreateMineByteEditTarget()!)
            {
                var owner = target.Regions.Single(r => r.Name == "Owner").Start;
                Assert.That(target.Update(bytes => { bytes[owner] = 0xFF; return bytes; }), Is.True);
            }

            Assert.That(page.Mines.Rows, Has.Count.EqualTo(before - 1));
            Assert.That(page.Mines.Rows.Select(m => m.Index), Does.Not.Contain(mine.Index));
        }

        [Test]
        public void TheMinesTabOfNeutralShowsTheBytesOfThePickedMine()
        {
            this.editor.ShowNeutral(NeutralTabs.Mines);
            var page = (NeutralViewModel)this.editor.CurrentPage!;

            using var target = page.CreateMineByteEditTarget();

            Assert.That(target, Is.Not.Null);
            Assert.That(target!.Regions.Select(r => r.Name), Does.Contain("Owner"));
        }

        private int OwnedMines(PlayerColor color) => this.editor.CurrentGame!.History.Current.Mines().GetModels().Count(m => m.Owner == color);
    }
}
