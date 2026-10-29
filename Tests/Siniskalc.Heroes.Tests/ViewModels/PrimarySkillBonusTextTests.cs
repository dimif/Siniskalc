namespace Siniskalc.Heroes.Tests.ViewModels
{
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.ViewModels;

    /// <summary>
    /// Tests the words for what an artifact adds to the primary skills.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class PrimarySkillBonusTextTests
    {
        [Test]
        public void OnlyTheSkillsTheBonusChangesGetALine()
        {
            var facts = PrimarySkillBonusText.Facts(new PrimarySkillBonus(6, 0, -2, 0));

            Assert.That(facts, Is.EqualTo(new[] { new FactRow("Attack", "+6"), new FactRow("Spell power", "-2") }));
        }

        [Test]
        public void TheBonusInOneLineListsTheSkillsInOrderAndNothingForNoBonus()
        {
            Assert.That(PrimarySkillBonusText.Describe(new PrimarySkillBonus(6, 6, 0, 0)), Is.EqualTo("Attack +6, Defense +6"));
            Assert.That(PrimarySkillBonusText.Describe(PrimarySkillBonus.None), Is.Empty);
        }
    }
}
