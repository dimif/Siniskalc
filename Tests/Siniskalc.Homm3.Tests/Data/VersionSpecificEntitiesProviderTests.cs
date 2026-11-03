namespace Siniskalc.Homm3.Tests.Data;

using Moq;
using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Models;

[TestFixture]
[Category(TestKinds.Unit)]
internal sealed class VersionSpecificEntitiesProviderTests
{
    [Test]
    public void TheProviderKeepsTheCatalogAndTheResolverItWasGiven()
    {
        var catalog = TestCatalog.For(GameVersion.ArmageddonsBlade);
        var resolver = Mock.Of<IGrantedSpellsResolver>();
        var provider = new VersionSpecificEntitiesProvider(catalog, resolver);
        Assert.That(provider.Catalog, Is.SameAs(catalog));
        Assert.That(provider.GrantedSpellsResolver, Is.SameAs(resolver));
    }

    [Test]
    public void OnlyHornOfTheAbyssOffersTheCannon()
    {
        var factory = new VersionSpecificEntitiesProviderFactory(TestCatalog.Catalogs);
        Assert.That(factory.For(GameVersion.HornOfTheAbyss).WarMachines.Select(m => m.Encoding), Does.Contain(WarMachineEncoding.Cannon));
        Assert.That(factory.For(GameVersion.ShadowOfDeath).WarMachines.Select(m => m.Encoding), Does.Not.Contain(WarMachineEncoding.Cannon));
    }

    [Test]
    public void SecondarySkillsAreFilteredWithoutSharingVersionIdentities()
    {
        var factory = new VersionSpecificEntitiesProviderFactory(TestCatalog.Catalogs);
        var sod = factory.For(GameVersion.ShadowOfDeath).SecondarySkills;
        var hota = factory.For(GameVersion.HornOfTheAbyss).SecondarySkills;
        Assert.That(sod, Has.Count.EqualTo(28));
        Assert.That(hota.Select(s => s.Id), Is.SupersetOf(sod.Select(s => s.Id)));
        Assert.That(hota, Is.Not.SupersetOf(sod));
    }

    [Test]
    public void TheFactoryReturnsTheSamePreloadedProviderAndResolverForEachVersion()
    {
        var factory = new VersionSpecificEntitiesProviderFactory(TestCatalog.Catalogs);
        var first = factory.For(GameVersion.ShadowOfDeath);
        var second = factory.For(GameVersion.ShadowOfDeath);
        Assert.That(second, Is.SameAs(first));
        Assert.That(second.GrantedSpellsResolver, Is.SameAs(first.GrantedSpellsResolver));
        Assert.That(first.Catalog, Is.SameAs(TestCatalog.For(GameVersion.ShadowOfDeath)));
    }
}
