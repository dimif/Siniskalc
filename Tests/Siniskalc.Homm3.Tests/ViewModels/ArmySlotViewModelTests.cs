using System.Globalization;
using Siniskalc.Heroes.Entities;
using Siniskalc.Homm3.ViewModels;

namespace Siniskalc.Homm3.Tests.ViewModels
{
    /// <summary>
    /// Tests the slot of an army that adds the totals of the stack (hit points, damage, cost and values) to the slot of the heroes of
    /// every game.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class ArmySlotViewModelTests
    {
        private static readonly CreatureInfo Info = new(0, 0, 2, 5, 10, 0, 0, 80, 60, 200, 1, 0, 0, 0, 0, 0, 0, null);

        private static string N(long number) => number.ToString("N0", CultureInfo.CurrentCulture);

        private static ArmySlotViewModel Slot() => new(2, (_, _, _) => { }, (_, _) => { }, _ => { });

        [Test]
        public void ASlotWithNoInfoHasNoTotals()
        {
            var slot = Slot();
            slot.InitializeSlot("Centaur", 10, true, true);

            Assert.That(new[] { slot.TotalHealth, slot.TotalDamage, slot.TotalCost, slot.TotalAiValue, slot.TotalFightValue, slot.TotalOtherCost }, Is.All.Empty);
        }

        [Test]
        public void TheTotalsAreThoseOfTheInfoTimesTheAmountOfTheSlot()
        {
            var slot = Slot();
            slot.InitializeSlot("Centaur", 10, true, true);

            slot.SetInfo(Info);

            Assert.That(slot.TotalHealth, Is.EqualTo(N(100)));
            Assert.That(slot.TotalDamage, Is.EqualTo($"{N(20)}-{N(50)}"));
            Assert.That(slot.TotalCost, Is.EqualTo(N(2000)));
            Assert.That(slot.TotalAiValue, Is.EqualTo(N(800)));
            Assert.That(slot.TotalFightValue, Is.EqualTo(N(600)));
            Assert.That(slot.TotalOtherCost, Is.Not.Empty);
        }

        [Test]
        public void ChangingTheAmountChangesTheTotals()
        {
            var slot = Slot();
            slot.InitializeSlot("Centaur", 10, true, true);
            slot.SetInfo(Info);

            slot.CreatureAmount = 20;

            Assert.That(slot.TotalHealth, Is.EqualTo(N(200)));
        }

        [Test]
        public void SettingTheInfoToNothingClearsTheTotals()
        {
            var slot = Slot();
            slot.InitializeSlot("Centaur", 10, true, true);
            slot.SetInfo(Info);

            slot.SetInfo(null);

            Assert.That(slot.TotalHealth, Is.Empty);
            Assert.That(slot.TotalCost, Is.Empty);
        }

        [Test]
        public void TheInfoAndAChangeOfAmountTellEveryTotalToTheBindings()
        {
            var slot = Slot();
            slot.InitializeSlot("Centaur", 10, true, true);
            var raised = new List<string?>();
            slot.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

            slot.SetInfo(Info);

            Assert.That(raised, Is.SupersetOf(new[] { "TotalHealth", "TotalDamage", "TotalCost", "TotalOtherCost", "TotalAiValue", "TotalFightValue", "Facts" }));

            raised.Clear();
            slot.CreatureAmount = 11;
            Assert.That(raised, Is.SupersetOf(new[] { "TotalHealth", "TotalDamage", "TotalCost", "TotalOtherCost", "TotalAiValue", "TotalFightValue" }));
        }
    }
}
