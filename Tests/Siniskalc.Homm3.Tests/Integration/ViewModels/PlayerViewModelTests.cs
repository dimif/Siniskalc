using Siniskalc.Heroes.Wpf.ViewModels;
using Siniskalc.Homm3.ViewModels;
using Siniskalc.Wpf.ViewModels;
using Siniskalc.Homm3.Tests.ViewModels;

namespace Siniskalc.Homm3.Tests.Integration.ViewModels
{
    /// <summary>
    /// Tests the page of a player: its resources, the quick buttons, reset and its pending changes.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class PlayerViewModelTests
    {
        private static readonly string SavePath = Path.Combine("TestData", "Hota", "empire_of_the_world_01_m5w4d6.GM1");

        private Homm3EditorViewModel main = null!;

        [SetUp]
        public void LoadGame()
        {
            this.main = TestServices.CreateHomm3Editor();
            this.main.LoadGameFromBytes(TestServices.SavefileCodec.Read(SavePath), SavePath);
        }

        private NavNodeViewModel PlayerNode(string color) =>
            this.main.PlayerNodes().Single(n => n.Title == color && n.Kind == NavNodeKind.Player);

        [Test]
        public void EditingAResourceIsUndoableAndMarksThePlayerAndItsNode()
        {
            this.PlayerNode("Purple").IsSelected = true;
            var player = this.main.CurrentPageOf<PlayerViewModel>();
            var gold = player.Resources.Single(r => r.IsGold);

            gold.Value = 5_000_000;

            Assert.That(gold.Value, Is.EqualTo(5_000_000));
            Assert.That(gold.IsChanged, Is.True);
            Assert.That(gold.DeltaText, Is.EqualTo("+4,779,024 from save").Or.EqualTo(string.Format("+{0:N0} from save", 5_000_000 - 220976)));
            Assert.That(player.HasPendingChanges, Is.True);
            Assert.That(this.PlayerNode("Purple").HasChanges, Is.True);
            Assert.That(this.main.PlayerNodes().Single(n => n.Title == "Purple").HasChanges, Is.True);
            Assert.That(this.PlayerNode("Red").HasChanges, Is.False);
            Assert.That(this.main.PendingChanges.Single().Path, Is.EqualTo("Purple › Resources"));

            this.main.Undo();

            Assert.That(gold.Value, Is.EqualTo(220976));
            Assert.That(gold.IsChanged, Is.False);
            Assert.That(gold.DeltaText, Is.Empty);
            Assert.That(player.HasPendingChanges, Is.False);
            Assert.That(this.main.PendingChanges, Is.Empty);
        }

        [Test]
        public void ResourceAmountsStayInsideTheirLimits()
        {
            this.PlayerNode("Purple").IsSelected = true;
            var player = this.main.CurrentPageOf<PlayerViewModel>();

            player.Resources.Single(r => r.IsGold).Value = long.MaxValue;
            player.Resources.Single(r => r.Name == "Wood").Value = -4;
            player.Resources.Single(r => r.Name == "Gems").Value = 5_000_000;

            Assert.That(player.Resources.Single(r => r.IsGold).Value, Is.EqualTo(Constants.Resources.MaxGold));
            Assert.That(player.Resources.Single(r => r.Name == "Wood").Value, Is.EqualTo(0));
            Assert.That(player.Resources.Single(r => r.Name == "Gems").Value, Is.EqualTo(Constants.Resources.MaxOther));
        }

        [Test]
        public void TheQuickSetButtonsAddToTheResourcesAndResetPutsThePlayerBack()
        {
            this.PlayerNode("Teal").IsSelected = true;
            var player = this.main.CurrentPageOf<PlayerViewModel>();
            var gold = player.Resources.Single(r => r.IsGold).Value;

            Assert.That(player.ResetCommand.CanExecute(null), Is.False);

            var others = player.Resources.Where(r => !r.IsGold).Select(r => r.Value).ToList();

            player.SetAllResourcesCommand.Execute(null);
            Assert.That(player.Resources.Where(r => !r.IsGold).Select(r => r.Value), Is.EqualTo(others.Select(v => Math.Min(v + 99, Constants.Resources.MaxOther))));
            Assert.That(player.Resources.Single(r => r.IsGold).Value, Is.EqualTo(gold));

            player.SetRichCommand.Execute(null);
            Assert.That(player.Resources.Single(r => r.IsGold).Value, Is.EqualTo(Math.Min(gold + Constants.Resources.GoldIncrease, Constants.Resources.MaxGold)));
            Assert.That(player.ResetCommand.CanExecute(null), Is.True);

            player.ResetCommand.Execute(null);
            Assert.That(player.HasPendingChanges, Is.False);
            Assert.That(player.Resources.Single(r => r.IsGold).Value, Is.EqualTo(gold));
            Assert.That(this.main.PendingChanges, Is.Empty);
        }

        [Test]
        public void PendingPlayerChangesCanBeRevertedOneByOne()
        {
            this.PlayerNode("Red").IsSelected = true;
            var player = this.main.CurrentPageOf<PlayerViewModel>();
            var original = player.Resources.ToDictionary(r => r.Name, r => r.Value);
            player.Resources.Single(r => r.Name == "Ore").Value = 1;
            player.Resources.Single(r => r.Name == "Sulfur").Value = 2;

            this.main.RevertChange(this.main.PendingChanges.Single(c => c.Change.Description == "Ore"));

            Assert.That(player.Resources.Single(r => r.Name == "Ore").Value, Is.EqualTo(original["Ore"]));
            Assert.That(player.Resources.Single(r => r.Name == "Sulfur").Value, Is.EqualTo(2));
            Assert.That(this.main.PendingChanges.Select(c => c.Change.Description), Is.EqualTo(new[] { "Sulfur" }));
        }
    }
}
