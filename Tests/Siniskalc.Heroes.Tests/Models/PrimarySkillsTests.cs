namespace Siniskalc.Heroes.Tests.Models
{
    using Siniskalc.Heroes.Models;

    /// <summary>
    /// Tests the primary skills of a hero, which the savefile holds with the bonus of the worn artifacts in them.
    /// </summary>
    [TestFixture]
    internal sealed class PrimarySkillsTests
    {
        private static readonly PrimarySkillBonus Sword = new(12, 0, 0, 0);

        [Test]
        public void TheBaseIsWhatIsLeftOfTheSavedValuesWithoutTheBonus()
        {
            var skills = new PrimarySkills(14, 2, 1, 1) { Bonus = Sword };

            Assert.That((skills.BaseAttack, skills.BaseDefense, skills.BaseSpellPower, skills.BaseKnowledge), Is.EqualTo((2, 2, 1, 1)));
        }

        [Test]
        public void ABaseThatWouldBeBelowNothingIsNothing()
        {
            var skills = new PrimarySkills(3, 0, 0, 0) { Bonus = Sword };

            Assert.That(skills.BaseAttack, Is.Zero);
        }

        [Test]
        public void ChangingTheBaseKeepsTheBonusOnTop()
        {
            var skills = new PrimarySkills(14, 2, 1, 1) { Bonus = Sword };

            var stronger = skills.WithBaseAttack(10).WithBaseKnowledge(20);

            Assert.That((stronger.Attack, stronger.Defense, stronger.SpellPower, stronger.Knowledge), Is.EqualTo((22, 2, 1, 20)));
            Assert.That(stronger.BaseAttack, Is.EqualTo(10));
        }

        [Test]
        public void PuttingAnArtifactOnAddsItsBonusAndTakingItOffRemovesIt()
        {
            var bare = new PrimarySkills(2, 2, 1, 1);

            var armed = bare.WithBonus(Sword);
            var disarmed = armed.WithBonus(PrimarySkillBonus.None);

            Assert.That(armed.Attack, Is.EqualTo(14));
            Assert.That(armed.Bonus, Is.EqualTo(Sword));
            Assert.That(disarmed, Is.EqualTo(bare));
            Assert.That(disarmed.Bonus, Is.EqualTo(PrimarySkillBonus.None));
        }

        [Test]
        public void ShiftingByTheDifferenceOfTwoBonusesDoesWhatWearingTheOtherArtifactDoes()
        {
            var skills = new PrimarySkills(14, 2, 1, 1) { Bonus = Sword };
            var shield = new PrimarySkillBonus(0, 12, 0, 0);

            var swapped = skills.Shifted(shield - Sword);

            Assert.That((swapped.Attack, swapped.Defense), Is.EqualTo((2, 14)));
            Assert.That(swapped.Bonus, Is.EqualTo(shield));
        }

        [Test]
        public void ASkillStaysWithinWhatTheSavefileCanHold()
        {
            var skills = new PrimarySkills(250, 3, 1, 1);

            Assert.That(skills.Shifted(new PrimarySkillBonus(12, -12, 0, 0)).Attack, Is.EqualTo(255));
            Assert.That(skills.Shifted(new PrimarySkillBonus(12, -12, 0, 0)).Defense, Is.Zero);
            Assert.That(skills.WithBaseAttack(900).Attack, Is.EqualTo(255));
        }

        [Test]
        public void TwoSetsOfSkillsAreEqualWhenTheirSavedValuesAreWhateverTheirBonus()
        {
            Assert.That(new PrimarySkills(5, 5, 4, 7) { Bonus = new PrimarySkillBonus(3, 3, 3, 6) }, Is.EqualTo(new PrimarySkills(5, 5, 4, 7)));
            Assert.That(new PrimarySkills(5, 5, 4, 7), Is.Not.EqualTo(new PrimarySkills(5, 5, 4, 8)));
        }

        [Test]
        public void SkillsBuiltFromTheBaseAndTheBonusAreSavedWithTheBonusInThem()
        {
            var skills = PrimarySkills.FromBase(2, 2, 1, 1, Sword);

            Assert.That(skills, Is.EqualTo(new PrimarySkills(14, 2, 1, 1)));
            Assert.That(skills.Bonus, Is.EqualTo(Sword));
        }
    }
}
