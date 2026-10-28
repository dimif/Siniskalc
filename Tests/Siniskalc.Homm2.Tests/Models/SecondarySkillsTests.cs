namespace Siniskalc.Homm2.Tests.Models
{
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// Tests the secondary skills of a hero, which hold at most eight skills in the order they were learned.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class SecondarySkillsTests
    {
        private static SecondarySkills Full() => SecondarySkills.None
            .WithSkill(SecondarySkill.Pathfinding, SkillLevel.Beginner)
            .WithSkill(SecondarySkill.Archery, SkillLevel.Beginner)
            .WithSkill(SecondarySkill.Logistics, SkillLevel.Beginner)
            .WithSkill(SecondarySkill.Scouting, SkillLevel.Beginner)
            .WithSkill(SecondarySkill.Diplomacy, SkillLevel.Beginner)
            .WithSkill(SecondarySkill.Navigation, SkillLevel.Beginner)
            .WithSkill(SecondarySkill.Leadership, SkillLevel.Beginner)
            .WithSkill(SecondarySkill.Wisdom, SkillLevel.Beginner);

        [Test]
        public void ALevelInASkillTheHeroHasChangesThatLevel()
        {
            var skills = Full().WithSkillLevel(SecondarySkill.Archery, SkillLevel.Expert);

            Assert.That(skills.GetSkillLevel(SecondarySkill.Archery), Is.EqualTo(SkillLevel.Expert));
        }

        [Test]
        public void ALevelOfNoneRemovesTheSkillAndMovesTheLaterOnesUp()
        {
            var skills = Full().WithSkillLevel(SecondarySkill.Archery, SkillLevel.None);

            Assert.That((skills.SlotCount, skills.GetSlotOf(SecondarySkill.Logistics)), Is.EqualTo((7, 1)));
        }

        [Test]
        public void AHeroWithEightSkillsDoesNotLearnAnotherOne()
        {
            var skills = Full();

            Assert.That(skills.WithSkillLevel(SecondarySkill.Mysticism, SkillLevel.Beginner), Is.SameAs(skills));
        }

        [Test]
        public void ALevelOfNoneInASkillTheHeroLacksChangesNothing()
        {
            var skills = SecondarySkills.None;

            Assert.That(skills.WithSkillLevel(SecondarySkill.Archery, SkillLevel.None), Is.SameAs(skills));
        }
    }
}
