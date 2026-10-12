using Siniskalc;
using Siniskalc.Entities;

namespace Siniskalc.Tests.Entities;

public sealed class CreatureInfoTests
{
    [Test]
    public void EveryCreatureHasStats()
    {
        foreach (var creature in Creature.All)
        {
            var info = creature.Info;
            Assert.That(info.Health, Is.GreaterThan(0), creature.Name);
            Assert.That(info.Speed, Is.GreaterThan(0), creature.Name);
            Assert.That(info.Gold, Is.GreaterThan(0), creature.Name);
            Assert.That(info.MaxDamage, Is.GreaterThanOrEqualTo(info.MinDamage), creature.Name);
        }
    }

    [Test]
    public void ResourceCostIsImported()
    {
        Assert.That(Creature.Angel.Info.Gems, Is.EqualTo(1));
        Assert.That(Creature.Archangel.Info.Gems, Is.EqualTo(3));
        Assert.That(Creature.AzureDragon.Info.Mercury, Is.EqualTo(20));
    }

    [Test]
    public void ShotsAreImported()
    {
        Assert.That(Creature.Marksman.Info.Shots, Is.EqualTo(24));
        Assert.That(Creature.Pikeman.Info.Shots, Is.EqualTo(0));
    }

    [Test]
    public void StatsDifferingBetweenVersionsFollowTheSaveVersion()
    {
        Assert.That(Creature.Firebird.GetInfo(GameVersion.ShadowOfDeath).Gold, Is.EqualTo(1500));
        Assert.That(Creature.Firebird.GetInfo(GameVersion.HornOfTheAbyss).Gold, Is.EqualTo(2000));
        Assert.That(Creature.Pikeman.GetInfo(GameVersion.HornOfTheAbyss), Is.EqualTo(Creature.Pikeman.GetInfo(GameVersion.ShadowOfDeath)));
    }
}
