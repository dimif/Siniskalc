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

        [Test]
        public void SecondarySkillsCanBeSwappedReplacedAndRemovedBySlot()
        {
            var skills = new SecondarySkills(
            [
                new SkillEntry(SecondarySkill.Archery, SkillLevel.Beginner),
                new SkillEntry(SecondarySkill.Wisdom, SkillLevel.Expert),
                new SkillEntry(SecondarySkill.Luck, SkillLevel.Intermediate),
            ]);

            Assert.That(skills.Swap(0, 2).Entries.Select(e => e.Skill), Is.EqualTo(new[] { SecondarySkill.Luck, SecondarySkill.Wisdom, SecondarySkill.Archery }));
            Assert.That(skills.WithoutSlot(1).Entries.Select(e => e.Skill), Is.EqualTo(new[] { SecondarySkill.Archery, SecondarySkill.Luck }));
            Assert.That(skills.WithSkillInSlot(1, SecondarySkill.Mysticism).Entries[1], Is.EqualTo(new SkillEntry(SecondarySkill.Mysticism, SkillLevel.Expert)));
            Assert.That(skills.WithSkillInSlot(1, SecondarySkill.Luck), Is.EqualTo(skills), "a skill the hero has already is not put twice");
        }
    }
}
