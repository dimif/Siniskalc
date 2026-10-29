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
            var grant = SpellGrantProvider.Of(Spell.Haste, Spell.Bless);

            Assert.That(grant.Description, Is.EqualTo("Haste, Bless"));
            Assert.That(grant.Resolve(GameVersion.ShadowOfDeath), Is.EquivalentTo(new[] { Spell.Haste, Spell.Bless }));
        }

        [Test]
        public void ASchoolGrantsItsSpellsAndThoseOfAllSchools()
        {
            var grant = SpellGrantProvider.OfSchool(SpellSchool.Fire);

            var spells = grant.Resolve(GameVersion.ShadowOfDeath);

            Assert.That(grant.Description, Is.EqualTo($"all {SpellSchool.Fire} spells"));
            Assert.That(spells, Does.Contain(Spell.MagicArrow), "a spell of all schools is of every school");
            Assert.That(spells.Where(s => s.School != SpellSchool.All), Is.All.Property("School").EqualTo(SpellSchool.Fire));
            Assert.That(spells, Does.Not.Contain(Spell.Haste));
        }

        [Test]
        public void ASchoolOrALevelDoesNotGrantTheSpellOnlyATitanCasts()
        {
            Assert.That(SpellGrantProvider.OfSchool(Spell.TitansLightningBolt.School).Resolve(GameVersion.ShadowOfDeath), Does.Not.Contain(Spell.TitansLightningBolt));
            Assert.That(SpellGrantProvider.OfLevel(Spell.TitansLightningBolt.Level).Resolve(GameVersion.ShadowOfDeath), Does.Not.Contain(Spell.TitansLightningBolt));
            Assert.That(SpellGrantProvider.Of(Spell.TitansLightningBolt).Resolve(GameVersion.ShadowOfDeath), Is.EqualTo(new[] { Spell.TitansLightningBolt }), "but a grant that names it gives it");
        }

        [Test]
        public void ALevelGrantsExactlyTheSpellsOfThatLevel()
        {
            var grant = SpellGrantProvider.OfLevel(3);

            var spells = grant.Resolve(GameVersion.ShadowOfDeath);

            Assert.That(grant.Description, Is.EqualTo("all level 3 spells"));
            Assert.That(spells, Is.Not.Empty);
            Assert.That(spells.Select(s => s.Level), Is.All.EqualTo(3));
        }
    }
}
