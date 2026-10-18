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
            Assert.That(PlayerColors<Color>.All.Select(c => (c.Name, c.Code)), Is.EqualTo(Enum.GetValues<Color>().Select(c => (c.ToString(), (byte)c))));
            Assert.That(PlayerColors<Color>.All.Select(PlayerPalette.Of), Is.Unique.And.None.EqualTo(PlayerPalette.NoPlayer));
            Assert.That(PlayerColors<Color>.Of(null), Is.Null);
        }

        [Test]
        public void EveryFactionOfTheGameIsAnIFactionWithItsNameAndCode()
        {
            Assert.That(Factions<Faction>.All.Select(f => (f.Name, f.Code)), Is.EqualTo(Enum.GetValues<Faction>().Select(f => (f.ToString(), (byte)f))));
            Assert.That(Factions<Faction>.Of(null), Is.Null);
            Assert.That(Factions<Faction>.Of(Faction.Wizard)!.Name, Is.EqualTo("Wizard"));
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
