namespace Siniskalc.Homm3.Tests.Entities
{
    using Siniskalc.Heroes.Entities;
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
            Assert.That(PlayerColors.All.Select(c => (c.Name, c.Code)), Is.EqualTo(Enum.GetValues<PlayerColor>().Select(c => (c.ToString(), (byte)c))));
            Assert.That(PlayerColors.All.Select(PlayerPalette.Of), Is.Unique.And.None.EqualTo(PlayerPalette.NoPlayer));
            Assert.That(PlayerColors.Of(null), Is.Null);
        }

        [Test]
        public void EveryFactionOfTheGameIsAnIFaction()
        {
            Assert.That(Faction.All.Cast<IFaction>().Select(f => f.Name), Is.EqualTo(Faction.All.Select(f => f.Name)));
        }

        [Test]
        public void EverySecondarySkillOfTheGameIsAnISecondarySkill()
        {
            Assert.That(SecondarySkill.All.Cast<ISecondarySkill>().Select(s => s.Code), Is.EqualTo(SecondarySkill.All.Select(s => s.Code)));
        }

        [Test]
        public void TheLevelsOfASkillAreRankedFromOneToThreeAndNoLevelIsNull()
        {
            Assert.That(new[] { SkillLevel.Beginner, SkillLevel.Intermediate, SkillLevel.Expert }.Select(l => SkillLevels.Of(l)!.Rank), Is.EqualTo(new[] { 1, 2, 3 }));
            Assert.That(SkillLevels.All.Select(l => l.Name), Is.EqualTo(new[] { "Basic", "Advanced", "Expert" }));
            Assert.That(SkillLevels.Of((SkillLevel?)null), Is.Null);
        }
    }
}
