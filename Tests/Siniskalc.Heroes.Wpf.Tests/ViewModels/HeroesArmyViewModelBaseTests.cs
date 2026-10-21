namespace Siniskalc.Heroes.Wpf.Tests.ViewModels
{
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// Tests the army editor that every game shares.
    /// </summary>
    [TestFixture]
    internal sealed class HeroesArmyViewModelBaseTests
    {
        private sealed class Army : HeroesArmyViewModelBase<TestCreature, StackSlotViewModel>
        {
            public Army(Func<IArmy<TestCreature>> getArmy, Action<Func<IArmy<TestCreature>, IArmy<TestCreature>>> update)
                : base(
                    [TestCreature.Peasant, TestCreature.Archer, TestCreature.Marksman],
                    _ => default,
                    3,
                    1,
                    999,
                    getArmy,
                    update,
                    (i, changed, move, clear) => new StackSlotViewModel(i, changed, move, clear))
            {
                this.Refresh();
            }

            protected override IReadOnlyList<StackFact> FactsOf(TestCreature creature, int amount) => [new("Creature", creature.Name)];
        }

        private IArmy<TestCreature> army = null!;
        private Army view = null!;

        [SetUp]
        public void StartWithAnEmptyArmy()
        {
            this.army = new Army<TestCreature>(3);
            this.view = new Army(() => this.army, edit => this.army = edit(this.army));
        }

        [Test]
        public void ACreatureIsAddedToTheFirstFreeSlot()
        {
            this.army = this.army.WithSlot(0, TestCreature.Peasant, 5);

            this.view.AddCreature(TestCreature.Archer, 7);

            Assert.That(this.army.Slots[1], Is.EqualTo(new ArmySlot<TestCreature>(TestCreature.Archer, 7)));
        }

        [Test]
        public void ACountOutsideWhatAStackHoldsIsKept()
        {
            this.view.SendToSlot(TestCreature.Peasant, 1, 5_000);
            this.view.SendToSlot(TestCreature.Archer, 2, 0);

            Assert.That(this.army.Slots.Select(s => s?.Count), Is.EqualTo(new long?[] { 999, 1, null }));
        }

        [Test]
        public void AFullArmyTakesNoMoreCreatures()
        {
            this.army = this.army.WithSlot(0, TestCreature.Peasant, 1).WithSlot(1, TestCreature.Peasant, 1).WithSlot(2, TestCreature.Peasant, 1);

            this.view.AddCreature(TestCreature.Archer, 5);

            Assert.That(this.army.Filled.Select(s => s.Creature), Is.All.EqualTo(TestCreature.Peasant));
        }

        [Test]
        public void PickingANameInASlotPutsThatCreatureThereAndTheEmptyEntryEmptiesIt()
        {
            this.view.CreatureSlots[0].SelectedCreature = "Archer";
            this.view.CreatureSlots[0].CreatureAmount = 4;
            Assert.That(this.army.Slots[0], Is.EqualTo(new ArmySlot<TestCreature>(TestCreature.Archer, 4)));

            this.view.CreatureSlots[0].SelectedCreature = string.Empty;

            Assert.That(this.army.Slots[0], Is.Null);
        }

        [Test]
        public void AStackSaysWhichCreaturesItCanBeUpgradedTo()
        {
            this.army = this.army.WithSlot(0, TestCreature.Archer, 3);
            this.view.Refresh();

            Assert.That(this.view.CreatureSlots[0].Upgrades, Is.EqualTo(new[] { "Marksman" }));
        }
    }
}
