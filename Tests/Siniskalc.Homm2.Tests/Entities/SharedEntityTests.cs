namespace Siniskalc.Homm2.Tests.Entities
{
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.ViewModels;
    using Siniskalc.Homm2.Entities;

    /// <summary>
    /// Tests that the colours, factions, skills and skill levels of the game are what the editor's shared pages ask for.
    /// </summary>
    [TestFixture]
    internal sealed class SharedEntityTests
    {
        [Test]
        public void EveryColourOfTheGameIsAnIPlayerColorWithItsNameAndCodeAndItsOwnHexValue()
        {
            Assert.That(Color.All.Cast<IPlayerColor>().Select(c => (c.Name, c.Code)), Is.EqualTo(Color.All.Select(c => (c.ToString(), c.Code))));
            Assert.That(Color.All.Select(PlayerPalette.Of), Is.Unique.And.None.EqualTo(PlayerPalette.NoPlayer));
            Assert.That(PlayerPalette.Of(null), Is.EqualTo(PlayerPalette.NoPlayer));
        }

        [Test]
        public void AColourIsFoundByItsCodeAndANumberThatIsNoColourIsNone()
        {
            Assert.That(Color.FromByte(Color.All[1].Code), Is.SameAs(Color.All[1]));
            Assert.That(Color.FromByte(200), Is.Null);
        }

        [Test]
        public void EveryFactionOfTheGameIsAnIFactionWithItsNameAndCode()
        {
            Assert.That(Faction.All.Cast<IFaction>().Select(f => (f.Name, f.Code)), Is.EqualTo(Faction.All.Select(f => (f.ToString(), f.Code))));
            Assert.That(Faction.FromByte(4), Is.SameAs(Faction.Wizard));
        }

        [Test]
        public void AnArtifactTellsWhatItAddsToThePrimarySkills()
        {
            Assert.That(((IArtifact)Artifact.UltimateSwordOfDominion).PrimarySkillBonus, Is.EqualTo(new PrimarySkillBonus(12, 0, 0, 0)));
            Assert.That(Artifact.UltimateSwordOfDominion.PrimarySkillBonus, Is.EqualTo(new PrimarySkillBonus(12, 0, 0, 0)));
            Assert.That(Artifact.AmmoCart.PrimarySkillBonus, Is.EqualTo(PrimarySkillBonus.None));
        }

        [Test]
        public void EverySecondarySkillOfTheGameIsAnISecondarySkill()
        {
            Assert.That(SecondarySkill.All.Cast<ISecondarySkill>().Select(s => s.Code), Is.EqualTo(SecondarySkill.All.Select(s => s.Code)));
        }
    }
}
