using Siniskalc.Core.Data;
using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;

using Siniskalc.Homm3.Tests.Data;

namespace Siniskalc.Homm3.Tests.Integration.ViewModels
{
    /// <summary>
    /// The owner picker of the hero page (<c>HeroViewModel.SelectedOwner</c>) on the saves of <c>TestData/Hota/HeroOwners</c>.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class HeroOwnerPickerTests
    {
        private static SaveDocumentHistory Load(string save) => SavedGames.Get(Path.Combine("TestData", "Hota", "HeroOwners", save + ".GM2")).History;

        [Test]
        public void PickingNeutralTakesTheHeroFromTheListOfItsPlayer()
        {
            var history = Load("hero_owners_00_baseline");
            using var clancy = TestServices.CreateHeroViewModel(history.CreateHeroSession(history.Current.Heroes().IndexOfId(22)));

            clancy.SelectedOwner = "Neutral";

            var blue = history.Current.Players().GetLists(history.Current.Players().FindByColor(PlayerColor.Blue));
            Assert.That(clancy.SelectedOwner, Is.EqualTo("Neutral"));
            Assert.That(blue.HeroIds, Does.Not.Contain(22));
            Assert.That(clancy.HasNotice, Is.False);
        }

        [Test]
        public void AnOwnerTheEditorCannotGiveIsNotPickedAndTheNoticeSaysWhy()
        {
            var history = Load("hero_owners_02_m4w2d1");
            using var vokial = TestServices.CreateHeroViewModel(history.CreateHeroSession(history.Current.Heroes().IndexOfId(65)));

            vokial.SelectedOwner = "Red";

            Assert.That(vokial.SelectedOwner, Is.EqualTo("Neutral"));
            Assert.That(vokial.HasNotice, Is.True);
            Assert.That(vokial.Notice, Does.Contain("tavern"));
            Assert.That(history.CanUndo, Is.False);
        }

        [Test]
        public void ResettingTheHeroPageAfterAnOwnerChangePutsBackTheListsOfThePlayersAndTheTableOfHeroOwners()
        {
            var history = Load("hero_owners_00_baseline");
            var original = history.Current;
            using var clancy = TestServices.CreateHeroViewModel(history.CreateHeroSession(original.Heroes().IndexOfId(22)));
            clancy.SelectedOwner = "Tan";
            Assert.That(clancy.HasPendingChanges, Is.True);

            clancy.ResetCommand.Execute(null);

            Assert.That(clancy.SelectedOwner, Is.EqualTo("Blue"));
            Assert.That(history.Current.ToBytes(), Is.EqualTo(original.ToBytes()), "the hero, the lists of Blue and Tan and the table are as loaded");
            Assert.That(history.GetPendingChanges(), Is.Empty);
        }
    }
}
