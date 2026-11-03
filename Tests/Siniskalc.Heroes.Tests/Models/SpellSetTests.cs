namespace Siniskalc.Heroes.Tests.Models
{
    using Siniskalc.Core.Entities;
    using Siniskalc.Heroes.Models;

    /// <summary>
    /// Tests the set of spells a hero knows.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class SpellSetTests
    {
        private static readonly Spell Bless = new("Bless", 3);
        private static readonly Spell Haste = new("Haste", 1);
        private static readonly Spell Slow = new("Slow", 2);

        [Test]
        public void SpellsComeInTheOrderOfTheirCodes()
        {
            var set = new SpellSet<Spell>([Bless, Haste, Slow]);

            Assert.That(set, Is.EqualTo(new[] { Haste, Slow, Bless }));
        }

        [Test]
        public void AddingAndRemovingASpellLeavesTheOriginalAlone()
        {
            var set = SpellSet<Spell>.None.With(Haste);

            Assert.That(set.Contains(Haste), Is.True);
            Assert.That(set.Without(Haste).Count, Is.EqualTo(0));
            Assert.That(set.Count, Is.EqualTo(1));
            Assert.That(set.With(Haste), Is.SameAs(set));
        }

        [Test]
        public void SetsWithTheSameSpellsAreEqualWhateverTheOrder()
        {
            var one = new SpellSet<Spell>([Haste, Slow]);
            var other = new SpellSet<Spell>([Slow, Haste]);

            Assert.That(one, Is.EqualTo(other));
            Assert.That(one.GetHashCode(), Is.EqualTo(other.GetHashCode()));
            Assert.That(one, Is.Not.EqualTo(other.With(Bless)));
        }

        private sealed record Spell(string Name, byte Code) : IGameEntity;
    }
}
