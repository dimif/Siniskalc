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
            .WithSkill(TestCatalog.Standard.Skills["Pathfinding"], SkillLevel.Beginner)
            .WithSkill(TestCatalog.Standard.Skills["Archery"], SkillLevel.Beginner)
            .WithSkill(TestCatalog.Standard.Skills["Logistics"], SkillLevel.Beginner)
            .WithSkill(TestCatalog.Standard.Skills["Scouting"], SkillLevel.Beginner)
            .WithSkill(TestCatalog.Standard.Skills["Diplomacy"], SkillLevel.Beginner)
            .WithSkill(TestCatalog.Standard.Skills["Navigation"], SkillLevel.Beginner)
            .WithSkill(TestCatalog.Standard.Skills["Leadership"], SkillLevel.Beginner)
            .WithSkill(TestCatalog.Standard.Skills["Wisdom"], SkillLevel.Beginner);

        [Test]
        public void ALevelInASkillTheHeroHasChangesThatLevel()
        {
            var skills = Full().WithSkillLevel(TestCatalog.Standard.Skills["Archery"], SkillLevel.Expert);

            Assert.That(skills.GetSkillLevel(TestCatalog.Standard.Skills["Archery"]), Is.EqualTo(SkillLevel.Expert));
        }

        [Test]
        public void ALevelOfNoneRemovesTheSkillAndMovesTheLaterOnesUp()
        {
            var skills = Full().WithSkillLevel(TestCatalog.Standard.Skills["Archery"], SkillLevel.None);

            Assert.That((skills.SlotCount, skills.GetSlotOf(TestCatalog.Standard.Skills["Logistics"])), Is.EqualTo((7, 1)));
        }

        [Test]
        public void AHeroWithEightSkillsDoesNotLearnAnotherOne()
        {
            var skills = Full();

            Assert.That(skills.WithSkillLevel(TestCatalog.Standard.Skills["Mysticism"], SkillLevel.Beginner), Is.SameAs(skills));
        }

        [Test]
        public void ALevelOfNoneInASkillTheHeroLacksChangesNothing()
        {
            var skills = SecondarySkills.None;

            Assert.That(skills.WithSkillLevel(TestCatalog.Standard.Skills["Archery"], SkillLevel.None), Is.SameAs(skills));
        }

        [Test]
        public void SecondarySkillsCanBeSwappedReplacedAndRemovedBySlot()
        {
            var skills = new SecondarySkills(
            [
                new SkillEntry(TestCatalog.Standard.Skills["Archery"], SkillLevel.Beginner),
                new SkillEntry(TestCatalog.Standard.Skills["Wisdom"], SkillLevel.Expert),
                new SkillEntry(TestCatalog.Standard.Skills["Luck"], SkillLevel.Intermediate),
            ]);

            Assert.That(skills.Swap(0, 2).Entries.Select(e => e.Skill), Is.EqualTo(new[] { TestCatalog.Standard.Skills["Luck"], TestCatalog.Standard.Skills["Wisdom"], TestCatalog.Standard.Skills["Archery"] }));
            Assert.That(skills.WithoutSlot(1).Entries.Select(e => e.Skill), Is.EqualTo(new[] { TestCatalog.Standard.Skills["Archery"], TestCatalog.Standard.Skills["Luck"] }));
            Assert.That(skills.WithSkillInSlot(1, TestCatalog.Standard.Skills["Mysticism"]).Entries[1], Is.EqualTo(new SkillEntry(TestCatalog.Standard.Skills["Mysticism"], SkillLevel.Expert)));
            Assert.That(skills.WithSkillInSlot(1, TestCatalog.Standard.Skills["Luck"]), Is.EqualTo(skills), "a skill the hero has already is not put twice");
        }
    }
}
