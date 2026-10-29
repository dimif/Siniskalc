using Moq;
using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Models;

namespace Siniskalc.Homm3.Tests.Data
{
    /// <summary>
    /// The skills, war machines and spells a game version has, and the factory that makes one provider for each version.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class VersionSpecificEntitiesProviderTests
    {
        private static VersionSpecificEntitiesProvider Provider(GameVersion version) => new(version, Mock.Of<IGrantedSpellsResolver>());

        [Test]
        public void TheProviderKeepsTheVersionAndTheResolverItWasGiven()
        {
            var resolver = Mock.Of<IGrantedSpellsResolver>();

            var provider = new VersionSpecificEntitiesProvider(GameVersion.ArmageddonsBlade, resolver);

            Assert.That(provider.GameVersion, Is.EqualTo(GameVersion.ArmageddonsBlade));
            Assert.That(provider.GrantedSpellsResolver, Is.SameAs(resolver));
        }

        [Test]
        public void OnlyHornOfTheAbyssHasTheCannon()
        {
            Assert.That(Provider(GameVersion.HornOfTheAbyss).WarMachines, Does.Contain(WarMachine.Cannon));
            Assert.That(Provider(GameVersion.ShadowOfDeath).WarMachines, Does.Not.Contain(WarMachine.Cannon));
            Assert.That(Provider(GameVersion.RestorationOfErathia).WarMachines, Does.Not.Contain(WarMachine.Cannon));
        }

        [Test]
        public void ShadowOfDeathHasTwentyEightSecondarySkillsAndHornOfTheAbyssHasThoseAndMore()
        {
            var sod = Provider(GameVersion.ShadowOfDeath).SecondarySkills;
            var hota = Provider(GameVersion.HornOfTheAbyss).SecondarySkills;

            Assert.That(sod, Has.Count.EqualTo(28));
            Assert.That(hota, Is.SupersetOf(sod));
            Assert.That(hota.Count, Is.GreaterThan(sod.Count));
        }

        [Test]
        public void EverythingTheProviderListsIsAvailableInItsVersion()
        {
            var provider = Provider(GameVersion.ShadowOfDeath);

            Assert.That(provider.SecondarySkills.All(s => s.IsAvailableIn(GameVersion.ShadowOfDeath)), Is.True);
            Assert.That(provider.Spells.All(s => s.IsAvailableIn(GameVersion.ShadowOfDeath)), Is.True);
            Assert.That(provider.WarMachines.All(w => w.IsAvailableIn(GameVersion.ShadowOfDeath)), Is.True);
        }

        [Test]
        public void TheFactoryMakesTheProviderOfTheVersionAskedAndAResolverMadeForThatVersion()
        {
            var resolvers = new Dictionary<GameVersion, IGrantedSpellsResolver>();
            var factory = new VersionSpecificEntitiesProviderFactory(version => resolvers[version] = Mock.Of<IGrantedSpellsResolver>());

            var hota = factory.For(GameVersion.HornOfTheAbyss);
            var sod = factory.For(GameVersion.ShadowOfDeath);

            Assert.That(hota.GameVersion, Is.EqualTo(GameVersion.HornOfTheAbyss));
            Assert.That(sod.GameVersion, Is.EqualTo(GameVersion.ShadowOfDeath));
            Assert.That(hota.GrantedSpellsResolver, Is.SameAs(resolvers[GameVersion.HornOfTheAbyss]));
            Assert.That(sod.GrantedSpellsResolver, Is.SameAs(resolvers[GameVersion.ShadowOfDeath]));
        }

        [Test]
        public void TheFactoryMakesOneProviderAndOneResolverForEachVersionHoweverOftenItIsAsked()
        {
            var made = new List<GameVersion>();
            var factory = new VersionSpecificEntitiesProviderFactory(version =>
            {
                made.Add(version);
                return Mock.Of<IGrantedSpellsResolver>();
            });

            var first = factory.For(GameVersion.ShadowOfDeath);
            var second = factory.For(GameVersion.ShadowOfDeath);
            factory.For(GameVersion.HornOfTheAbyss);

            Assert.That(second, Is.SameAs(first));
            Assert.That(made, Is.EqualTo(new[] { GameVersion.ShadowOfDeath, GameVersion.HornOfTheAbyss }));
        }
    }
}
