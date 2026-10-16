namespace Siniskalc.Heroes.Tests.ViewModels
{
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.ViewModels;

    /// <summary>
    /// Tests the row of the creature finder, which both games show.
    /// </summary>
    [TestFixture]
    internal sealed class CreatureChoiceViewModelTests
    {
        private static readonly CreatureAbility Shooting = new("Ranged", true);
        private static readonly CreatureAbility Flying = new("Flying", false);

        [Test]
        public void ARowShowsTheStatsOfTheCreature()
        {
            var info = new CreatureInfo(5, 3, 2, 3, 10, 4, 8, 0, 0, 150, 0, 0, 0, 0, 0, 0, 12, [Shooting]);
            var row = new CreatureChoiceViewModel(new FakeCreature("Archer", 2, false), info);

            Assert.That(row.Name, Is.EqualTo("Archer"));
            Assert.That(row.Level, Is.EqualTo(2));
            Assert.That(row.Attack, Is.EqualTo(5));
            Assert.That(row.Defense, Is.EqualTo(3));
            Assert.That(row.DamageRange, Is.EqualTo("2-3"));
            Assert.That(row.Health, Is.EqualTo(10));
            Assert.That(row.Speed, Is.EqualTo(4));
            Assert.That(row.Growth, Is.EqualTo(8));
            Assert.That(row.Cost, Is.EqualTo(150));
            Assert.That(row.Form, Is.EqualTo("Basic"));
        }

        [Test]
        public void ADamageThatDoesNotVaryIsOneNumber()
        {
            var info = new CreatureInfo(1, 1, 7, 7, 1, 1, 1, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, null);

            Assert.That(new CreatureChoiceViewModel(new FakeCreature("Gremlin", 1, false), info).DamageRange, Is.EqualTo("7"));
        }

        [Test]
        public void TheCostBesidesGoldAndTheAbilitiesAreListedAndRangedShowsItsShots()
        {
            var info = new CreatureInfo(1, 1, 1, 1, 1, 1, 1, 0, 0, 750, 0, 0, 0, 0, 1, 2, 24, [Shooting, Flying]);
            var row = new CreatureChoiceViewModel(new FakeCreature("Titan", 6, true), info);

            Assert.That(row.Resources, Is.EqualTo("1 crystal, 2 gems"));
            Assert.That(row.Special, Is.EqualTo("Ranged (24 shots), Flying"));
            Assert.That(row.Details, Does.Contain("Growth: 1 a week").And.Contain("Shots: 24"));
            Assert.That(row.Form, Is.EqualTo("Upgraded"));
        }

        [Test]
        public void ANeutralCreatureIsListedLast()
        {
            var info = default(CreatureInfo);

            Assert.That(new CreatureChoiceViewModel(new FakeCreature("Rogue", 1, false, null), info).Faction, Is.EqualTo("Neutral"));
            Assert.That(new CreatureChoiceViewModel(new FakeCreature("Rogue", 1, false, null), info).FactionOrder, Is.EqualTo(int.MaxValue));
            Assert.That(new CreatureChoiceViewModel(new FakeCreature("Peasant", 1, false), info).FactionOrder, Is.EqualTo(3));
        }

        private sealed class CreatureAbility(string name, bool isRanged) : ICreatureAbility
        {
            public string Name { get; } = name;

            public bool IsRanged { get; } = isRanged;
        }

        private sealed record FakeFaction(string Name, byte Code) : IFaction;

        private sealed record FakeCreature(string Name, int Tier, bool IsUpgrade, IFaction? Faction) : ICreature
        {
            public FakeCreature(string name, int tier, bool isUpgrade)
                : this(name, tier, isUpgrade, new FakeFaction("Knight", 3))
            {
            }

            public byte Code => 0;
        }
    }
}
