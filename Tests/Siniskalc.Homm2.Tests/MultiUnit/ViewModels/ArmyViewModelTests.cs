namespace Siniskalc.Homm2.Tests.MultiUnit.ViewModels
{
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.ViewModels;

    /// <summary>
    /// The upgrade button of a stack of the army.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.MultiUnit)]
    internal sealed class ArmyViewModelTests
    {
        private static (ArmyViewModel View, Func<IArmy<Creature>> Army) Create(Creature creature)
        {
            var army = new Army<Creature>(5).WithSlot(0, creature, 7);
            var view = new ArmyViewModel(GameVersion.PriceOfLoyalty, () => army, change => army = change(army));
            return (view, () => army);
        }

        [Test]
        public void AStackOfABasicCreatureCanBeUpgradedAndKeepsItsSize()
        {
            var (view, army) = Create(Creature.Pikeman);
            var slot = view.CreatureSlots[0];

            Assert.That(slot.Upgrades, Is.EqualTo(new[] { "Veteran Pikeman" }));
            Assert.That(slot.HasUpgrade, Is.True);

            slot.UpgradeCommand.Execute(null);
            view.Refresh();

            Assert.That(army().Slots[0]!.Creature, Is.EqualTo(Creature.VeteranPikeman));
            Assert.That(army().Slots[0]!.Count, Is.EqualTo(7));
            Assert.That(view.CreatureSlots[0].HasUpgrade, Is.False, "the veteran pikeman has no further form");
        }

        [Test]
        public void ADragonThatIsUpgradedOnceCanStillBeUpgradedAgain()
        {
            var (view, _) = Create(Creature.RedDragon);

            Assert.That(view.CreatureSlots[0].Upgrades, Is.EqualTo(new[] { "Black Dragon" }));
        }

        [Test]
        public void AnEmptySlotHasNothingToUpgrade()
        {
            var (view, _) = Create(Creature.Pikeman);

            Assert.That(view.CreatureSlots[1].HasUpgrade, Is.False);
        }
    }
}
