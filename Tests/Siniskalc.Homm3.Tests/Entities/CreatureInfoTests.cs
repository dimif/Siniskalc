using Siniskalc.Homm3.Entities;

namespace Siniskalc.Homm3.Tests.Entities;

[Category(TestKinds.Unit)]
public sealed class CreatureInfoTests
{
    [Test]
    public void EveryCreatureHasHealthSpeedAndGoldAndANonInvertedDamageRange()
    {
        foreach (var creature in TestCatalog.For(GameVersion.HornOfTheAbyss).Creatures.All)
        {
            var info = creature.Info;
            Assert.That(info.Health, Is.GreaterThan(0), creature.Name);
            Assert.That(info.Speed, Is.GreaterThan(0), creature.Name);
            Assert.That(info.Gold, Is.GreaterThan(0), creature.Name);
            Assert.That(info.MaxDamage, Is.GreaterThanOrEqualTo(info.MinDamage), creature.Name);
        }
    }

    [Test]
    public void TheGemsAndMercuryACreatureCostsAreImported()
    {
        Assert.That(TestCatalog.For(GameVersion.HornOfTheAbyss).Creatures.ById("Angel").Info.Gems, Is.EqualTo(1));
        Assert.That(TestCatalog.For(GameVersion.HornOfTheAbyss).Creatures.ById("Archangel").Info.Gems, Is.EqualTo(3));
        Assert.That(TestCatalog.For(GameVersion.HornOfTheAbyss).Creatures.ById("AzureDragon").Info.Mercury, Is.EqualTo(20));
    }

    [Test]
    public void ARangedCreatureHasItsShotsAndAMeleeCreatureHasNone()
    {
        Assert.That(TestCatalog.For(GameVersion.HornOfTheAbyss).Creatures.ById("Marksman").Info.Shots, Is.EqualTo(24));
        Assert.That(TestCatalog.For(GameVersion.HornOfTheAbyss).Creatures.ById("Pikeman").Info.Shots, Is.EqualTo(0));
    }

    [Test]
    public void StatsDifferingBetweenVersionsFollowTheSaveVersion()
    {
        Assert.That(TestCatalog.For(GameVersion.ShadowOfDeath).Creatures.ById("Firebird").Info.Gold, Is.EqualTo(1500));
        Assert.That(TestCatalog.For(GameVersion.HornOfTheAbyss).Creatures.ById("Firebird").Info.Gold, Is.EqualTo(2000));
        Assert.That(TestCatalog.For(GameVersion.HornOfTheAbyss).Creatures.ById("Pikeman").Info with { Special = null },
            Is.EqualTo(TestCatalog.For(GameVersion.ShadowOfDeath).Creatures.ById("Pikeman").Info with { Special = null }));
    }
}
