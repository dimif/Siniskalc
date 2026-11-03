using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Runtime;

using Siniskalc.Homm3.Tests.Data;

namespace Siniskalc.Homm3.Tests.Integration.ViewModels
{
    /// <summary>
    /// The owner picker of the castle page (<c>CastleViewModel.SelectedOwner</c>) on the saves of <c>TestData/Hota/Banishing</c>, in which
    /// Blue owns one town, 2_Rampart.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class CastleOwnerPickerTests
    {
        private static Game Load() => SavedGames.Get(Path.Combine("TestData", "Hota", "Banishing", "banishing_00_baseline.GM2"));

        [Test]
        public void PickingNeutralForTheOnlyTownOfAPlayerStartsItsCountdown()
        {
            var history = Load().History;
            using var rampart = TestServices.CreateCastleViewModel(history.CreateCastleSession(history.Current.Castles().FindByName("2_Rampart")));

            rampart.SelectedOwner = "Neutral";

            var players = history.Current.Players();
            Assert.That(players.GetModel(players.FindByColor(PlayerColor.Blue)).DaysLeft, Is.EqualTo(Constants.Player.MaxDaysLeft));
            Assert.That(rampart.HasNotice, Is.False);
        }

        [Test]
        public void ResettingTheCastlePageAfterAnOwnerChangePutsBackTheDaysLeftOfThePlayers()
        {
            var history = Load().History;
            var original = history.Current;
            using var rampart = TestServices.CreateCastleViewModel(history.CreateCastleSession(original.Castles().FindByName("2_Rampart")));
            rampart.SelectedOwner = "Neutral";

            rampart.ResetCommand.Execute(null);

            Assert.That(rampart.SelectedOwner, Is.EqualTo("Blue"));
            Assert.That(history.Current.ToBytes(), Is.EqualTo(original.ToBytes()), "the castle and the days left of Blue are as loaded");
            Assert.That(history.GetPendingChanges(), Is.Empty);
        }
    }
}
