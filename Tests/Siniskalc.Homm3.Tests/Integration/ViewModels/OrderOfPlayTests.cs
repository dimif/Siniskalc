using Siniskalc.Homm3.Tests.Data;

namespace Siniskalc.Homm3.Tests.Integration.ViewModels
{
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.ViewModels;

    /// <summary>
    /// The "Order of play" card of the overview of a Shadow of Death hotseat game (Red and Blue human).
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class OrderOfPlayTests
    {
        [Test]
        public void TheCardListsThePlayersWithTheTurnLitAndPressingAHumanWhoHasNotTheTurnGivesItToHim()
        {
            var path = Path.Combine("TestData", "Sod", "Hotseat", "hotseat_00_baseline_200_diff.GM2");
            var game = SavedGames.Get(path);
            var overview = TestServices.CreateOverviewViewModel(game);

            Assert.That(overview.HasOrderOfPlay, Is.True);
            var current = overview.OrderOfPlay.Where(i => i.IsCurrent).ToList();
            Assert.That(current.Select(i => i.Name), Is.EqualTo(new[] { "Red" }));
            Assert.That(current[0].Command, Is.Null, "the player who has the turn cannot be given it");
            var pressable = overview.OrderOfPlay.Where(i => i.Command != null).ToList();
            Assert.That(pressable.Select(i => i.Name), Is.EqualTo(new[] { "Blue" }), "only a human who has not the turn can be given it");
            Assert.That(pressable[0].Command!.CanExecute(null), Is.True);
            Assert.That(pressable[0].ToolTip, Is.EqualTo(OverviewViewModel.GiveTheTurnToolTip));

            pressable[0].Command!.Execute(null);
            overview.Refresh();

            Assert.That(game.CurrentPlayer, Is.EqualTo(PlayerColor.Blue));
            Assert.That(game.History.GetPendingChanges().Single().Change.After, Is.EqualTo("Blue"));
            Assert.That(overview.OrderOfPlay.Single(i => i.IsCurrent).Name, Is.EqualTo("Blue"));
            Assert.That(overview.TurnText, Is.EqualTo("Current turn: Blue"));
        }

        [Test]
        public void TheCachedRedAndBluePagesFollowTheTurnInTheSodHotseatBaselineThroughUndoAndRedo()
        {
            var path = Path.Combine("TestData", "Sod", "Hotseat", "hotseat_00_baseline_200_diff.GM2");
            var editor = TestServices.CreateHomm3Editor();
            editor.LoadGameFromBytes(TestServices.SavefileCodec.Read(path), path);
            editor.ShowPlayer(0);
            var red = (PlayerViewModel)editor.CurrentPage!;
            editor.ShowPlayer(1);
            var blue = (PlayerViewModel)editor.CurrentPage!;
            var redNotifications = 0;
            var blueNotifications = 0;
            red.PropertyChanged += (_, args) => redNotifications += args.PropertyName == nameof(PlayerViewModel.ControlText) ? 1 : 0;
            blue.PropertyChanged += (_, args) => blueNotifications += args.PropertyName == nameof(PlayerViewModel.ControlText) ? 1 : 0;

            AssertLabels("Human · current turn", "Human");
            editor.Overview!.OrderOfPlay.Single(i => i.Name == "Blue").Command!.Execute(null);
            AssertLabels("Human", "Human · current turn");
            Assert.That((redNotifications, blueNotifications), Is.EqualTo((1, 1)));

            editor.Undo();
            AssertLabels("Human · current turn", "Human");
            Assert.That((redNotifications, blueNotifications), Is.EqualTo((2, 2)));

            editor.Redo();
            AssertLabels("Human", "Human · current turn");
            Assert.That((redNotifications, blueNotifications), Is.EqualTo((3, 3)));

            void AssertLabels(string redText, string blueText)
            {
                editor.ShowPlayer(0);
                Assert.That(editor.CurrentPage, Is.SameAs(red));
                Assert.That(red.ControlText, Is.EqualTo(redText));
                editor.ShowPlayer(1);
                Assert.That(editor.CurrentPage, Is.SameAs(blue));
                Assert.That(blue.ControlText, Is.EqualTo(blueText));
            }
        }
    }
}
