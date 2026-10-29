namespace Siniskalc.Heroes.Wpf.Tests.ViewModels
{
    using System.Globalization;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Wpf.ViewModels;

    /// <summary>
    /// Tests the numbers shown for a whole stack of one creature: what its hit points, damage, cost and values add up to.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class StackTotalsTests
    {
        private static CreatureInfo Info(short health = 0, short minDamage = 0, short maxDamage = 0, int gold = 0, int ai = 0, int fight = 0, short wood = 0, short ore = 0, short mercury = 0, short sulfur = 0, short crystal = 0, short gems = 0) =>
            new(0, 0, minDamage, maxDamage, health, 0, 0, ai, fight, gold, wood, ore, mercury, sulfur, crystal, gems, 0, null);

        private static string N(long number) => number.ToString("N0", CultureInfo.CurrentCulture);

        [Test]
        public void TheHitPointsTheCostAndTheValuesAreThoseOfOneCreatureTimesTheAmount()
        {
            var totals = new StackTotals(Info(health: 15, gold: 200, ai: 80, fight: 60), 100);

            Assert.That((totals.Health, totals.Cost, totals.AiValue, totals.FightValue), Is.EqualTo((N(1500), N(20000), N(8000), N(6000))));
        }

        [Test]
        public void ADamageThatDoesNotVaryIsOneNumber()
        {
            Assert.That(new StackTotals(Info(minDamage: 3, maxDamage: 3), 10).Damage, Is.EqualTo(N(30)));
        }

        [Test]
        public void ADamageThatVariesIsARangeOfTheMinimumAndTheMaximumTimesTheAmount()
        {
            Assert.That(new StackTotals(Info(minDamage: 2, maxDamage: 5), 10).Damage, Is.EqualTo($"{N(20)}-{N(50)}"));
        }

        [Test]
        public void TheNumbersAreWrittenWithThousandsSeparators()
        {
            var totals = new StackTotals(Info(health: 100), 25000);

            Assert.That(totals.Health, Is.EqualTo(N(2_500_000)));
        }

        [Test]
        public void ThePriceBeyondGoldListsEachResourceTheCreatureCostsInTheOrderOfTheGame()
        {
            var totals = new StackTotals(Info(wood: 1, ore: 2, mercury: 3, sulfur: 4, crystal: 5, gems: 6), 10);

            Assert.That(totals.OtherCost, Is.EqualTo($"and {N(10)} wood, {N(20)} ore, {N(30)} mercury, {N(40)} sulfur, {N(50)} crystal, {N(60)} gems"));
        }

        [Test]
        public void OnlyTheResourcesTheCreatureCostsAreListed()
        {
            var totals = new StackTotals(Info(gems: 2), 7);

            Assert.That(totals.OtherCost, Is.EqualTo($"and {N(14)} gems"));
        }

        [Test]
        public void ACreatureThatCostsOnlyGoldHasNoOtherCost()
        {
            Assert.That(new StackTotals(Info(gold: 100), 7).OtherCost, Is.Empty);
        }

        [Test]
        public void TheFactsListHitPointsDamageCostTheOtherCostAsANoteAndTheTwoValues()
        {
            var facts = new StackTotals(Info(health: 10, minDamage: 1, maxDamage: 2, gold: 50, ai: 4, fight: 3, ore: 1), 10).ToFacts();

            Assert.That(facts.Select(f => (f.Label, f.Value, f.IsNote)), Is.EqualTo(new[]
            {
                ("HP Total", N(100), false),
                ("Damage", $"{N(10)}-{N(20)}", false),
                ("Cost", N(500), false),
                (string.Empty, $"and {N(10)} ore", true),
                ("AI value", N(40), false),
                ("Fight value", N(30), false),
            }));
        }

        [Test]
        public void TheValueFactsSayWhatTheyAreFor()
        {
            var facts = new StackTotals(Info(), 1).ToFacts();

            Assert.That(facts.Single(f => f.Label == "AI value").ToolTip, Does.Contain("computer player"));
            Assert.That(facts.Single(f => f.Label == "Fight value").ToolTip, Does.Contain("strength of an army"));
        }

        [Test]
        public void TheShortListOfFactsHasOnlyHitPointsAndDamage()
        {
            var facts = new StackTotals(Info(health: 10, minDamage: 1, maxDamage: 1), 3).ToHealthAndDamageFacts();

            Assert.That(facts.Select(f => (f.Label, f.Value)), Is.EqualTo(new[] { ("HP Total", N(30)), ("Damage", N(3)) }));
        }
    }
}
