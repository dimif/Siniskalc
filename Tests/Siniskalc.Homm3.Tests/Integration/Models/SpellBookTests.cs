using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Models;

namespace Siniskalc.Homm3.Tests.Integration.Models
{
    /// <summary>
    /// Tests when a spell is available in the spell book of a hero and how learning and grants change it.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class SpellBookTests
    {
        private static ISet<Spell> Set(params Spell[] spells) => new HashSet<Spell>(spells);

        [Test]
        public void ASpellIsAvailableWhenLearnedGrantedOrAvailableForAnotherReason()
        {
            var book = new SpellBook(Set(Spell.Haste), Set(Spell.Fly))
                .WithGrants(new Dictionary<Spell, string> { [Spell.Armageddon] = "Armageddon's Blade" }, false);

            Assert.That(book.IsAvailable(Spell.Haste), Is.True);
            Assert.That(book.IsAvailable(Spell.Armageddon), Is.True);
            Assert.That(book.IsAvailable(Spell.Fly), Is.True);
            Assert.That(book.IsAvailable(Spell.Slow), Is.False);
            Assert.That(book.Source(Spell.Armageddon), Is.EqualTo("Armageddon's Blade"));
            Assert.That(book.Source(Spell.Fly), Is.EqualTo("Unknown source"));
            Assert.That(book.Source(Spell.Haste), Is.Empty);
            Assert.That(book.Source(Spell.Slow), Is.Empty);
        }

        [Test]
        public void LearningAndForgettingLeaveTheGrantsAlone()
        {
            var book = new SpellBook(Set(), Set())
                .WithGrants(new Dictionary<Spell, string> { [Spell.Prayer] = "Tome" }, false)
                .WithLearned(Spell.Prayer, true)
                .WithLearned(Spell.Prayer, false);

            Assert.That(book.IsLearned(Spell.Prayer), Is.False);
            Assert.That(book.IsAvailable(Spell.Prayer), Is.True);
        }

        [Test]
        public void WithGrantsReturnsTheSameBookWhenNothingChanges()
        {
            var grants = new Dictionary<Spell, string> { [Spell.Prayer] = "Tome" };
            var book = new SpellBook(Set(), Set()).WithGrants(grants, false);

            Assert.That(book.WithGrants(new Dictionary<Spell, string>(grants), false), Is.SameAs(book));
            Assert.That(book.WithGrants(grants, true), Is.Not.SameAs(book));
            Assert.That(book.WithHasSpellBook(false), Is.SameAs(book));
        }
    }
}
