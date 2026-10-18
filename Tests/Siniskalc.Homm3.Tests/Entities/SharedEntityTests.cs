namespace Siniskalc.Homm3.Tests.Entities
{
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.ViewModels;
    using Siniskalc.Homm3.Entities;

    /// <summary>
    /// Tests that the colours, factions, skills and skill levels of the game are what the editor's shared pages ask for.
    /// </summary>
    [TestFixture]
    internal sealed class SharedEntityTests
    {
        [Test]
        public void EveryColourOfTheGameIsAnIPlayerColorWithItsNameAndCodeAndItsOwnHexValue()
        {
            Assert.That(PlayerColors<PlayerColor>.All.Select(c => (c.Name, c.Code)), Is.EqualTo(Enum.GetValues<PlayerColor>().Select(c => (c.ToString(), (byte)c))));
            Assert.That(PlayerColors<PlayerColor>.All.Select(PlayerPalette.Of), Is.Unique.And.None.EqualTo(PlayerPalette.NoPlayer));
            Assert.That(PlayerColors<PlayerColor>.Of(null), Is.Null);
        }

        [Test]
        public void EveryFactionOfTheGameIsAnIFaction()
        {
            Assert.That(Faction.All.Cast<IFaction>().Select(f => f.Name), Is.EqualTo(Faction.All.Select(f => f.Name)));
        }

        [Test]
        public void AnArtifactTellsWhatItAddsToThePrimarySkills()
        {
            Assert.That(((IArtifact)Artifact.TitansThunder).PrimarySkillBonus, Is.EqualTo(new PrimarySkillBonus(9, 9, 8, 8)));
            Assert.That(Artifact.ArmageddonsBlade.PrimarySkillBonus, Is.EqualTo(new PrimarySkillBonus(3, 3, 3, 6)));
            Assert.That(Artifact.SeaCaptainsHat.PrimarySkillBonus, Is.EqualTo(PrimarySkillBonus.None));
        }

        [Test]
        public void EverySecondarySkillOfTheGameIsAnISecondarySkill()
        {
            Assert.That(SecondarySkill.All.Cast<ISecondarySkill>().Select(s => s.Code), Is.EqualTo(SecondarySkill.All.Select(s => s.Code)));
        }
    }
}
