namespace Siniskalc.Homm2.Tests.ViewModels
{
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;
    using Siniskalc.Homm2.ViewModels;

    /// <summary>
    /// Tests the page of secondary skills of a hero of HoMM2: the skills of the game it lists, the eight slots and the note that says how
    /// many a hero can learn. The behaviour every game shares is tested with the base page.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class SecondarySkillsViewModelTests
    {
        private SecondarySkills skills = SecondarySkills.None;

        [SetUp]
        public void StartWithNoSkills()
        {
            this.skills = SecondarySkills.None;
        }

        private SecondarySkillsViewModel Page(GameVersion version = GameVersion.BaseGame)
        {
            var page = new SecondarySkillsViewModel(version, () => this.skills, edit => this.skills = edit(this.skills));
            page.Refresh();
            return page;
        }

        [Test]
        public void ThePageListsEverySkillOfTheVersionOfTheGameAndTheNoteSaysEightOfFourteen()
        {
            var page = this.Page();

            Assert.That(page.AllSkills.Select(r => r.Name), Is.EqualTo(SecondarySkill.All.Where(s => s.IsAvailableIn(GameVersion.BaseGame)).Select(s => s.Name)));
            Assert.That(page.AllSkillsNote, Is.EqualTo("A hero can learn eight of the fourteen skills. They are shown in the order they were learned."));
        }

        [Test]
        public void ThePageHasTheMostSlotsAHeroCanFill()
        {
            Assert.That(this.Page().Slots, Has.Count.EqualTo(Constants.Hero.MaxSecondarySkills));
        }

        [Test]
        public void ASkillOfTheHeroIsShownInItsSlotWithItsLevel()
        {
            this.skills = SecondarySkills.None.WithSkill(SecondarySkill.Archery, SkillLevel.Expert);

            var page = this.Page();

            Assert.That(page.Slots[0].SkillName, Is.EqualTo("Archery"));
            Assert.That(page.Slots[0].LevelName, Is.EqualTo("Expert"));
        }
    }
}
