namespace Siniskalc.Heroes.Wpf.Tests.ViewModels
{
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.Wpf.ViewModels;

    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class PrimarySkillsCardViewModelTests
    {
        private PrimarySkills skills = null!;
        private int edits;
        private PrimarySkillsCardViewModel card = null!;

        [SetUp]
        public void MakeTheCard()
        {
            this.skills = PrimarySkills.FromBase(5, 4, 3, 2, new PrimarySkillBonus(3, 0, 0, -1));
            this.edits = 0;
            this.card = new PrimarySkillsCardViewModel(
                () => this.skills,
                edit =>
                {
                    this.edits++;
                    this.skills = edit(this.skills);
                },
                new NumberRange(0, 99));
        }

        [Test]
        public void EachSkillShowsItsBaseItsTotalAndWhatTheArtifactsAdd()
        {
            Assert.That(this.card.Rows.Select(r => r.Label), Is.EqualTo(new[] { "Attack", "Defense", "Spell power", "Knowledge" }));
            Assert.That((this.card.Attack.Base, this.card.Attack.Total, this.card.Attack.ArtifactNote), Is.EqualTo((5L, 8, "+3 from artifacts")));
            Assert.That((this.card.Defense.Base, this.card.Defense.Total, this.card.Defense.ArtifactNote), Is.EqualTo((4L, 4, string.Empty)));
            Assert.That(this.card.Knowledge.ArtifactNote, Is.EqualTo("-1 from artifacts"));
        }

        [Test]
        public void EditingTheBaseKeepsWhatTheArtifactsAddOnTopOfIt()
        {
            this.card.Attack.Base = 10;

            Assert.That((this.skills.Attack, this.skills.BaseAttack, this.skills.Bonus.Attack), Is.EqualTo((13, 10, 3)));
        }

        [Test]
        public void ABaseOutOfTheLimitsIsBroughtWithinThem()
        {
            this.card.Defense.Base = 255;

            Assert.That(this.skills.BaseDefense, Is.EqualTo(99));
        }

        [Test]
        public void SettingTheBaseItAlreadyHasMakesNoEdit()
        {
            this.card.SpellPower.Base = 3;

            Assert.That(this.edits, Is.Zero);
        }

        [Test]
        public void RefreshingTellsTheRowsToShowTheHeroAgain()
        {
            var changed = new List<string?>();
            this.card.Attack.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

            this.card.Refresh();

            Assert.That(changed, Is.Not.Empty);
        }
    }
}
