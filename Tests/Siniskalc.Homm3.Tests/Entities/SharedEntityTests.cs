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
            Assert.That(PlayerColor.All.Cast<IPlayerColor>().Select(c => (c.Name, c.Code)), Is.EqualTo(PlayerColor.All.Select(c => (c.ToString(), c.Code))));
            Assert.That(PlayerColor.All.Select(PlayerPalette.Of), Is.Unique.And.None.EqualTo(PlayerPalette.NoPlayer));
            Assert.That(PlayerPalette.Of(null), Is.EqualTo(PlayerPalette.NoPlayer));
        }

        [Test]
        public void AColourIsFoundByItsCodeAndANumberThatIsNoColourIsNeutral()
        {
            Assert.That(PlayerColor.FromByte(PlayerColor.All[1].Code), Is.SameAs(PlayerColor.All[1]));
            Assert.That(PlayerColor.FromByte(200), Is.SameAs(PlayerColor.Neutral));
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
