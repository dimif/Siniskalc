using Siniskalc.Homm3.Entities;

namespace Siniskalc.Homm3.Tests.Entities
{
    /// <summary>
    /// Tests what an artifact can grant a hero to cast: some named spells, all the spells of a school or all of a level, as the game version
    /// has them.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class SpellGrantProviderTests
    {
        [Test]
        public void SomeNamedSpellsAreGrantedAndDescribedByTheirNames()
        {
            var grant = SpellGrantProvider.Of(TestCatalog.For(GameVersion.ShadowOfDeath).Spells.ById("Haste"), TestCatalog.For(GameVersion.ShadowOfDeath).Spells.ById("Bless"));

            Assert.That(grant.Description, Is.EqualTo("Haste, Bless"));
            Assert.That(grant.Resolve(GameVersion.ShadowOfDeath), Is.EquivalentTo(new[] { TestCatalog.For(GameVersion.ShadowOfDeath).Spells.ById("Haste"), TestCatalog.For(GameVersion.ShadowOfDeath).Spells.ById("Bless") }));
        }

        [Test]
        public void ASchoolGrantsItsSpellsAndThoseOfAllSchools()
        {
            var grant = SpellGrantProvider.OfSchool(TestCatalog.For(GameVersion.ShadowOfDeath).SpellSchools.ById("fire"));

            var spells = grant.Resolve(GameVersion.ShadowOfDeath);

            Assert.That(grant.Description, Is.EqualTo($"all {TestCatalog.For(GameVersion.ShadowOfDeath).SpellSchools.ById("fire")} spells"));
            Assert.That(spells, Does.Contain(TestCatalog.For(GameVersion.ShadowOfDeath).Spells.ById("MagicArrow")), "a spell of all schools is of every school");
            Assert.That(spells.Where(s => s.School != TestCatalog.For(GameVersion.ShadowOfDeath).SpellSchools.ById("all")), Is.All.Property("School").EqualTo(TestCatalog.For(GameVersion.ShadowOfDeath).SpellSchools.ById("fire")));
            Assert.That(spells, Does.Not.Contain(TestCatalog.For(GameVersion.ShadowOfDeath).Spells.ById("Haste")));
        }

        [Test]
        public void ASchoolOrALevelDoesNotGrantTheSpellOnlyATitanCasts()
        {
            Assert.That(SpellGrantProvider.OfSchool(TestCatalog.For(GameVersion.ShadowOfDeath).Spells.ById("TitansLightningBolt").School).Resolve(GameVersion.ShadowOfDeath), Does.Not.Contain(TestCatalog.For(GameVersion.ShadowOfDeath).Spells.ById("TitansLightningBolt")));
            Assert.That(SpellGrantProvider.OfLevel(TestCatalog.For(GameVersion.ShadowOfDeath), TestCatalog.For(GameVersion.ShadowOfDeath).Spells.ById("TitansLightningBolt").Level).Resolve(GameVersion.ShadowOfDeath), Does.Not.Contain(TestCatalog.For(GameVersion.ShadowOfDeath).Spells.ById("TitansLightningBolt")));
            Assert.That(SpellGrantProvider.Of(TestCatalog.For(GameVersion.ShadowOfDeath).Spells.ById("TitansLightningBolt")).Resolve(GameVersion.ShadowOfDeath), Is.EqualTo(new[] { TestCatalog.For(GameVersion.ShadowOfDeath).Spells.ById("TitansLightningBolt") }), "but a grant that names it gives it");
        }

        [Test]
        public void ALevelGrantsExactlyTheSpellsOfThatLevel()
        {
            var grant = SpellGrantProvider.OfLevel(TestCatalog.For(GameVersion.ShadowOfDeath), 3);

            var spells = grant.Resolve(GameVersion.ShadowOfDeath);

            Assert.That(grant.Description, Is.EqualTo("all level 3 spells"));
            Assert.That(spells, Is.Not.Empty);
            Assert.That(spells.Select(s => s.Level), Is.All.EqualTo(3));
        }
    }
}
