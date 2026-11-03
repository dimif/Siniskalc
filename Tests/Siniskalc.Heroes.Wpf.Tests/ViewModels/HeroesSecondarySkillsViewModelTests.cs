namespace Siniskalc.Heroes.Wpf.Tests.ViewModels
{
    using System.Linq;
    using Moq;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.Wpf.ViewModels;

    /// <summary>
    /// Tests the secondary skills page every game builds, on a hero whose skills are a fake: what it shows from them and which call each
    /// choice of the user makes.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class HeroesSecondarySkillsViewModelTests
    {
        private Mock<ISkill> archery = null!;
        private Mock<ISkill> wisdom = null!;
        private Mock<ISkill> luck = null!;
        private Mock<ISkills> skills = null!;
        private Func<ISkills, ISkills>? lastEdit;
        private HeroesSecondarySkillsViewModel<ISkill, ISkills> page = null!;

        public interface ISkill : ISecondarySkill
        {
        }

        public interface ISkills : ISecondarySkills<ISkill, ISkills>
        {
        }

        [SetUp]
        public void ShowAHeroWithArcheryBasicAndWisdomExpert()
        {
            this.archery = Skill("Archery");
            this.wisdom = Skill("Wisdom");
            this.luck = Skill("Luck");
            this.skills = new Mock<ISkills>();
            this.skills.SetupGet(s => s.SlotCount).Returns(2);
            this.skills.Setup(s => s.GetSkillInSlot(0)).Returns(this.archery.Object);
            this.skills.Setup(s => s.GetSkillInSlot(1)).Returns(this.wisdom.Object);
            this.skills.Setup(s => s.GetSkillLevel(this.archery.Object)).Returns(SkillLevel.Beginner);
            this.skills.Setup(s => s.GetSkillLevel(this.wisdom.Object)).Returns(SkillLevel.Expert);
            this.skills.Setup(s => s.GetSkillLevel(this.luck.Object)).Returns(SkillLevel.None);
            this.skills.Setup(s => s.GetSlotOf(this.archery.Object)).Returns(0);
            this.skills.Setup(s => s.GetSlotOf(this.wisdom.Object)).Returns(1);
            this.skills.Setup(s => s.GetSlotOf(this.luck.Object)).Returns(-1);
            this.lastEdit = null;
            this.page = new HeroesSecondarySkillsViewModel<ISkill, ISkills>(
                [this.archery.Object, this.luck.Object, this.wisdom.Object],
                3,
                () => this.skills.Object,
                edit => this.lastEdit = edit);
        }

        private static Mock<ISkill> Skill(string name)
        {
            var skill = new Mock<ISkill>();
            skill.SetupGet(s => s.Name).Returns(name);
            return skill;
        }

        private SkillRowViewModel Row(string name) => this.page.AllSkills.Single(r => r.Name == name);

        [Test]
        public void ThePageShowsTheSkillsInTheirSlotsAndTheLevelOfEverySkillOnceBuilt()
        {
            Assert.That(this.page.Slots.Select(s => (s.SkillName, s.LevelName)), Is.EqualTo(new (string?, string)[] { ("Archery", "Basic"), ("Wisdom", "Expert"), (null, "Basic") }));
            Assert.That(this.page.AllSkills.Select(r => (r.Name, r.LevelName, r.IsInSlot)), Is.EqualTo(new[] { ("Archery", "Basic", true), ("Luck", "None", false), ("Wisdom", "Expert", true) }));
            Assert.That(this.page.SlotSummary, Is.EqualTo("2 of 3 slots used"));
        }

        [Test]
        public void ReplacingTheSkillOfASlotAsksTheHeroForTheSkillInThatSlot()
        {
            this.page.Slots[0].SkillName = "Luck";
            var edited = Mock.Of<ISkills>();
            this.skills.Setup(s => s.WithSkillInSlot(0, this.luck.Object)).Returns(edited);

            Assert.That(this.lastEdit!(this.skills.Object), Is.SameAs(edited));
        }

        [Test]
        public void PickingASkillInTheFirstFreeSlotAddsItAtBasicLevel()
        {
            this.page.Slots[2].SkillName = "Luck";
            var edited = Mock.Of<ISkills>();
            this.skills.Setup(s => s.WithSkillLevel(this.luck.Object, SkillLevel.Beginner)).Returns(edited);

            Assert.That(this.lastEdit!(this.skills.Object), Is.SameAs(edited));
        }

        [Test]
        public void TheLevelOfASlotIsSetOnTheSkillInThatSlot()
        {
            this.page.Slots[1].LevelName = "Advanced";
            var edited = Mock.Of<ISkills>();
            this.skills.Setup(s => s.WithSkillLevel(this.wisdom.Object, SkillLevel.Intermediate)).Returns(edited);

            Assert.That(this.lastEdit!(this.skills.Object), Is.SameAs(edited));
        }

        [Test]
        public void TheLevelOfASlotThatIsGoneByTheTimeOfTheEditLeavesTheSkillsAlone()
        {
            this.page.Slots[1].LevelName = "Advanced";
            var fewer = new Mock<ISkills>();
            fewer.SetupGet(s => s.SlotCount).Returns(1);

            Assert.That(this.lastEdit!(fewer.Object), Is.SameAs(fewer.Object));
        }

        [Test]
        public void TheLevelOfARowIsSetOnItsSkillAndNoneTakesTheSkillAway()
        {
            this.Row("Luck").LevelName = "Expert";
            var expert = Mock.Of<ISkills>();
            this.skills.Setup(s => s.WithSkillLevel(this.luck.Object, SkillLevel.Expert)).Returns(expert);
            Assert.That(this.lastEdit!(this.skills.Object), Is.SameAs(expert));

            this.Row("Wisdom").LevelName = "None";
            var none = Mock.Of<ISkills>();
            this.skills.Setup(s => s.WithSkillLevel(this.wisdom.Object, SkillLevel.None)).Returns(none);
            Assert.That(this.lastEdit!(this.skills.Object), Is.SameAs(none));
        }

        [Test]
        public void MovingASkillSwapsItsSlotWithTheNextOne()
        {
            this.page.MoveSkill(0, 1);
            var edited = Mock.Of<ISkills>();
            this.skills.Setup(s => s.Swap(0, 1)).Returns(edited);

            Assert.That(this.lastEdit!(this.skills.Object), Is.SameAs(edited));
        }

        [Test]
        public void ClearingASkillRemovesItsSlot()
        {
            this.page.ClearSkill(0);
            var edited = Mock.Of<ISkills>();
            this.skills.Setup(s => s.WithoutSlot(0)).Returns(edited);

            Assert.That(this.lastEdit!(this.skills.Object), Is.SameAs(edited));
        }

        [Test]
        public void ASlotLevelOfNoneAndASkillTheGameDoesNotKnowChangeNothing()
        {
            this.page.Slots[0].LevelName = "None";
            this.page.Slots[0].SkillName = "Diplomacy";

            Assert.That(this.lastEdit, Is.Null);
        }
    }
}
