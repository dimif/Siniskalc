using Siniskalc.Homm3.Entities;

namespace Siniskalc.Homm3.Tests.Entities
{
    /// <summary>
    /// Tests the effects of an artifact as a value: the spells it grants for a game version, and when two effects are the same.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class ArtifactEffectInfoTests
    {
        [Test]
        public void AnArtifactWithNoGrantsGrantsNoSpells()
        {
            Assert.That(default(ArtifactEffectInfo).GrantedSpells(GameVersion.ShadowOfDeath), Is.Empty);
        }

        [Test]
        public void TheSpellsOfSeveralGrantsAreListedOnceEach()
        {
            var info = new ArtifactEffectInfo { SpellGrants = [SpellGrantProvider.Of(TestCatalog.For(GameVersion.ShadowOfDeath).Spells.ById("Haste"), TestCatalog.For(GameVersion.ShadowOfDeath).Spells.ById("Bless")), SpellGrantProvider.Of(TestCatalog.For(GameVersion.ShadowOfDeath).Spells.ById("Haste"))] };

            Assert.That(info.GrantedSpells(GameVersion.ShadowOfDeath), Is.EquivalentTo(new[] { TestCatalog.For(GameVersion.ShadowOfDeath).Spells.ById("Haste"), TestCatalog.For(GameVersion.ShadowOfDeath).Spells.ById("Bless") }));
        }

        [Test]
        public void TwoEffectsAreTheSameWhenEveryFieldAndEveryGrantIsTheSame()
        {
            var grant = SpellGrantProvider.Of(TestCatalog.For(GameVersion.ShadowOfDeath).Spells.ById("Haste"));
            var info = new ArtifactEffectInfo { Atk = 1, Other = "text", SpellGrants = [grant] };

            Assert.That(info.Equals(new ArtifactEffectInfo { Atk = 1, Other = "text", SpellGrants = [grant] }), Is.True);
            Assert.That(info.Equals(new ArtifactEffectInfo { Atk = 2, Other = "text", SpellGrants = [grant] }), Is.False);
            Assert.That(info.Equals(new ArtifactEffectInfo { Atk = 1, Other = "other", SpellGrants = [grant] }), Is.False);
            Assert.That(info.Equals(new ArtifactEffectInfo { Atk = 1, Other = "text", SpellGrants = [SpellGrantProvider.Of(TestCatalog.For(GameVersion.ShadowOfDeath).Spells.ById("Bless"))] }), Is.False);
            Assert.That(info.Equals(new ArtifactEffectInfo { Atk = 1, Other = "text" }), Is.False);
        }

        [TestCase("Def")]
        [TestCase("Pow")]
        [TestCase("Kno")]
        [TestCase("Luck")]
        [TestCase("Morale")]
        [TestCase("LandMovement")]
        [TestCase("WaterMovement")]
        [TestCase("Gold")]
        [TestCase("Ore")]
        [TestCase("Wood")]
        [TestCase("Crystal")]
        [TestCase("Gems")]
        [TestCase("Mercury")]
        [TestCase("Sulfur")]
        public void AnyFieldThatDiffersMakesTwoEffectsDifferent(string field)
        {
            object boxed = default(ArtifactEffectInfo);
            typeof(ArtifactEffectInfo).GetProperty(field)!.SetValue(boxed, (short)1);

            Assert.That(default(ArtifactEffectInfo).Equals((ArtifactEffectInfo)boxed), Is.False, field);
        }

        [Test]
        public void TheSpellBookFlagMakesTwoEffectsDifferent()
        {
            Assert.That(default(ArtifactEffectInfo).Equals(new ArtifactEffectInfo { GrantsSpellBook = true }), Is.False);
        }

        [Test]
        public void EqualEffectsHaveTheSameHashCode()
        {
            var a = new ArtifactEffectInfo { Atk = 3, Other = "x", SpellGrants = [SpellGrantProvider.Of(TestCatalog.For(GameVersion.ShadowOfDeath).Spells.ById("Haste"))] };
            var b = new ArtifactEffectInfo { Atk = 3, Other = "x", SpellGrants = [SpellGrantProvider.Of(TestCatalog.For(GameVersion.ShadowOfDeath).Spells.ById("Haste"))] };

            Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
        }
    }
}
