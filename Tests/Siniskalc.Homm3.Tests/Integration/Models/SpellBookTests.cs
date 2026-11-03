using Siniskalc.Homm3.Data;
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
            var book = new SpellBook(Set(TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("Haste")), Set(TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("Fly")))
                .WithGrants(new Dictionary<Spell, string> { [TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("Armageddon")] = "Armageddon's Blade" }, false);

            Assert.That(book.IsAvailable(TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("Haste")), Is.True);
            Assert.That(book.IsAvailable(TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("Armageddon")), Is.True);
            Assert.That(book.IsAvailable(TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("Fly")), Is.True);
            Assert.That(book.IsAvailable(TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("Slow")), Is.False);
            Assert.That(book.Source(TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("Armageddon")), Is.EqualTo("Armageddon's Blade"));
            Assert.That(book.Source(TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("Fly")), Is.EqualTo("Unknown source"));
            Assert.That(book.Source(TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("Haste")), Is.Empty);
            Assert.That(book.Source(TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("Slow")), Is.Empty);
        }

        [Test]
        public void LearningAndForgettingLeaveTheGrantsAlone()
        {
            var book = new SpellBook(Set(), Set())
                .WithGrants(new Dictionary<Spell, string> { [TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("Prayer")] = "Tome" }, false)
                .WithLearned(TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("Prayer"), true)
                .WithLearned(TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("Prayer"), false);

            Assert.That(book.IsLearned(TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("Prayer")), Is.False);
            Assert.That(book.IsAvailable(TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("Prayer")), Is.True);
        }

        [Test]
        public void WithGrantsReturnsTheSameBookWhenNothingChanges()
        {
            var grants = new Dictionary<Spell, string> { [TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("Prayer")] = "Tome" };
            var book = new SpellBook(Set(), Set()).WithGrants(grants, false);

            Assert.That(book.WithGrants(new Dictionary<Spell, string>(grants), false), Is.SameAs(book));
            Assert.That(book.WithGrants(grants, true), Is.Not.SameAs(book));
            Assert.That(book.WithHasSpellBook(false), Is.SameAs(book));
        }
    }
}
