namespace Siniskalc.Heroes.Tests.Entities
{
    using Siniskalc.Core.Entities;
    using Siniskalc.Heroes.Entities;

    /// <summary>
    /// Tests the skill levels the games share.
    /// </summary>
    [TestFixture]
    internal sealed class SkillLevelTests
    {
        [Test]
        public void TheLevelsAreNumberedFromNoneToExpertAsTheSavegamesHoldThem()
        {
            Assert.That(
                AbstractEnumeration.GetAll<SkillLevel>().Select(l => (l.Name, l.Code)),
                Is.EqualTo(new[] { ("None", (byte)0), ("Beginner", (byte)1), ("Intermediate", (byte)2), ("Expert", (byte)3) }));
        }

        [Test]
        public void AByteThatIsNoLevelReadsAsNone()
        {
            Assert.That(
                new byte[] { 0, 1, 2, 3, 4, 255 }.Select(SkillLevel.Of),
                Is.EqualTo(new[] { SkillLevel.None, SkillLevel.Beginner, SkillLevel.Intermediate, SkillLevel.Expert, SkillLevel.None, SkillLevel.None }));
        }
    }
}
