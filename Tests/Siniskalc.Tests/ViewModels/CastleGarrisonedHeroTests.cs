using System.Windows;
using Siniskalc.Data;
using Siniskalc.Models;
using Siniskalc.Runtime;
using Siniskalc.ViewModels;

namespace Siniskalc.Tests.ViewModels
{
    /// <summary>
    /// The garrison tab of a castle on the roe_61_inferno_garrison_and_visiting savegame, where Nymus is garrisoned in "4_Inferno" and
    /// Tazar visits it: the army of a garrisoned hero takes the place of the castle's own creatures.
    /// </summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    internal sealed class CastleGarrisonedHeroTests
    {
        private MainViewModel main = null!;

        [SetUp]
        public void LoadGame()
        {
            var path = Path.Combine("TestData", "Roe", "roe_61_inferno_garrison_and_visiting.GM1");
            this.main = TestServices.CreateMainViewModel();
            this.main.LoadGameFromBytes(TestServices.SavefileCodec.Read(path), path);
        }

        private CastleViewModel Show(string name)
        {
            this.main.ShowCastle(this.main.LoadedGame().History.Current.Castles().FindByName(name));
            return this.main.CurrentPageOf<CastleViewModel>();
        }

        [Test]
        public void TheGarrisonTabShowsTheArmyOfTheGarrisonedHeroAndCannotBeEdited()
        {
            var castle = this.Show("4_Inferno");
            var heroes = this.main.LoadedGame().History.Current.Heroes();
            var nymus = heroes.GetModel(heroes.FindByName("Nymus")).Army;

            Assert.That(castle.HasGarrisonedHero, Is.True);
            Assert.That(castle.Garrisoned, Is.EqualTo("Garrisoned by Nymus"));
            Assert.That(castle.GarrisonEditor.IsEditable, Is.False);
            Assert.That(
                castle.GarrisonEditor.CreatureSlots.Select(s => s.SelectedCreature),
                Is.EqualTo(Enumerable.Range(1, 7).Select(i => nymus.FindSlot(i) is { } s ? Siniskalc.Entities.Creature.FromByte(s.CreatureCode)?.Name : null)));
        }

        [Test]
        public void TheStoredSlotsOfACastleAreLeftAloneWhileAHeroIsGarrisoned()
        {
            var castle = this.Show("4_Inferno");

            castle.GarrisonEditor.CreatureSlots[0].SelectedCreature = "Pikeman";
            castle.GarrisonEditor.CreatureSlots[0].CreatureAmount = 40;

            Assert.That(this.main.PendingChanges, Is.Empty);
            Assert.That(this.main.LoadedGame().History.Current.Castles().GetModel(castle.CastleIndex).Garrison.Slots.Length, Is.EqualTo(0));
        }

        [Test]
        public void ACastleWithoutAGarrisonedHeroHasAnEditableGarrison()
        {
            var castle = this.Show("7_Strong");

            Assert.That(castle.HasGarrisonedHero, Is.False);
            Assert.That(castle.GarrisonEditor.IsEditable, Is.True);
        }

        [Test]
        public void TheGarrisonOfACastleCannotBeSetWhileAHeroIsGarrisoned()
        {
            var castles = this.main.LoadedGame().History.Current.Castles();
            var garrisoned = castles.GetModel(castles.FindByName("4_Inferno"));

            Assert.That(() => garrisoned.WithGarrison(garrisoned.Garrison), Throws.TypeOf<GameRuleException>());
            Assert.That(() => (garrisoned with { GarrisonedHeroId = null }).WithGarrison(garrisoned.Garrison), Throws.Nothing);
        }
    }
}
