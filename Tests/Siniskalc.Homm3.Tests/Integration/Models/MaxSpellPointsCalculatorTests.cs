using Siniskalc.Heroes.Entities;
using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Models;
using Siniskalc.Homm3.Runtime;

namespace Siniskalc.Homm3.Tests.Integration.Models
{
    /// <summary>
    /// The most spell points a hero can have in a savefile of each version of the game.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class MaxSpellPointsCalculatorTests
    {
        private static HeroModel HeroWith(int knowledge, SkillLevel intelligence)
        {
            var game = TestServices.GameService.LoadGameFromFile(Path.Combine("TestData", "Hota", "Arts", "arts_00_before_arts.GM1"));
            var hero = game.History.CreateHeroSession(game.History.Current.Heroes().FindByName("Valeska")).Current;
            return hero with
            {
                PrimarySkills = hero.PrimarySkills.WithBaseKnowledge((byte)Math.Max(0, knowledge)),
                SecondarySkills = hero.SecondarySkills.WithSkillLevel(SecondarySkill.Intelligence, intelligence),
            };
        }

        private static int Maximum(MaxSpellPointsCalculator calculator, HeroModel hero) => calculator.GetMaximum(hero.PrimarySkills, hero.SecondarySkills);

        [Test]
        public void SpellPointsFollowKnowledgeAndIntelligenceInTheOriginalGames()
        {
            var calculator = new MaxSpellPointsCalculator(GameVersion.ShadowOfDeath);

            Assert.That(Maximum(calculator, HeroWith(10, SkillLevel.None)), Is.EqualTo(100));
            Assert.That(Maximum(calculator, HeroWith(10, SkillLevel.Beginner)), Is.EqualTo(125));
            Assert.That(Maximum(calculator, HeroWith(10, SkillLevel.Expert)), Is.EqualTo(200));
        }

        [Test]
        public void IntelligenceAddsLessInHornOfTheAbyss()
        {
            var calculator = new MaxSpellPointsCalculator(GameVersion.HornOfTheAbyss);

            Assert.That(Maximum(calculator, HeroWith(10, SkillLevel.Intermediate)), Is.EqualTo(135));
            Assert.That(Maximum(calculator, HeroWith(10, SkillLevel.Expert)), Is.EqualTo(150));
        }
    }
}
