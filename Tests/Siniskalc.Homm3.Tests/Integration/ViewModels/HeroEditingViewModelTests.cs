using Siniskalc.Heroes.Entities;
using Siniskalc.Heroes.Models;
using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Models;
using Siniskalc.Homm3.Runtime;
using Siniskalc.Homm3.ViewModels;
using Siniskalc.Homm3.Tests.Integration.Models;

namespace Siniskalc.Homm3.Tests.Integration.ViewModels
{
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class HeroEditingViewModelTests
    {
        private Game game;
        private HeroViewModel hero;

        [SetUp]
        public void SetUp()
        {
            this.game = HeroEditingTests.LoadPrepared();
            this.hero = TestServices.CreateHeroViewModel(this.game.History.CreateHeroSession(this.game.History.Current.Heroes().FindByName("Christian")));
        }

        [TearDown]
        public void TearDown()
        {
            this.hero.Dispose();
        }

        [Test]
        public void EditsFromThePageLandInThePendingChanges()
        {
            var general = this.hero.GeneralInfoViewModel;

            general.PrimarySkills.Attack.Base = 42;
            general.MovementAndSpellPoints.CurrentSpellPoints = 99;
            general.UpdateWarMachine("Ammo_Cart", true);

            Assert.That(this.hero.HeroModel.PrimarySkills.Attack, Is.EqualTo((byte)42));
            Assert.That(general.WarMachineChoices.Single(c => c.Name == "Ammo Cart").IsChecked, Is.True);
            Assert.That(this.game.History.GetPendingChanges().Select(c => c.Change.Description), Is.SupersetOf(new[] { "Attack", "Spell points", "War machines" }));
        }

        [Test]
        public void MakingAHeroNeutralTakesItOffTheMap()
        {
            this.hero.SelectedOwner = "Neutral";

            Assert.That(this.hero.HeroModel.Owner, Is.EqualTo(PlayerColor.Neutral));
            Assert.That(this.hero.HeroModel.Position, Is.EqualTo(new HeroPosition(-1, -1, -1, FacingDirection.East)));
            Assert.That(this.hero.HeroModel.Destination, Is.EqualTo(new MapPosition(-1, -1, -1)));

            this.hero.SelectedOwner = "Blue";
            Assert.That(this.hero.HeroModel.Owner, Is.EqualTo(PlayerColor.Blue));
        }
    }
}
