namespace Siniskalc.Heroes.Wpf.Tests.ViewModels
{
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Wpf.ViewModels;

    [TestFixture]
    internal sealed class ArmyTotalsTests
    {
        private static CreatureInfo Info(short health, short minDamage, short maxDamage, int gold = 0, int ai = 0, int fight = 0, short gems = 0) =>
            new(0, 0, minDamage, maxDamage, health, 0, 0, ai, fight, gold, 0, 0, 0, 0, 0, gems, 0, null);

        [Test]
        public void AnEmptyArmyHasNoSummary()
        {
            Assert.That(new ArmyTotals().ToSummary(withValues: true), Is.Empty);
        }

        [Test]
        public void ASummaryWithoutValuesTellsTheHitPointsAndTheDamageOfAllTheStacks()
        {
            var totals = new ArmyTotals();
            totals.Add(Info(10, 1, 3), 10);
            totals.Add(Info(20, 2, 2), 5);

            Assert.That(totals.ToSummary(withValues: false), Is.EqualTo($"HP Total {200:N0}  ·  Damage {20:N0}-{40:N0}"));
        }

        [Test]
        public void ASummaryWithValuesTellsTheCostAndTheValuesOfTheComputerPlayerAndTheGame()
        {
            var totals = new ArmyTotals();
            totals.Add(Info(10, 1, 3, gold: 100, ai: 7, fight: 5, gems: 2), 10);

            Assert.That(totals.ToSummary(withValues: true), Is.EqualTo($"HP Total {100:N0}  ·  Cost {1000:N0} and {20:N0} gems  ·  AI {70:N0}  ·  Fight {50:N0}"));
        }
    }
}
