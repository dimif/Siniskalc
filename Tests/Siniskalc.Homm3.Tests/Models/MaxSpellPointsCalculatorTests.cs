using Siniskalc.Heroes.Entities;
using Siniskalc.Heroes.Models;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Models;

namespace Siniskalc.Homm3.Tests.Models
{
    /// <summary>
    /// The most spell points a hero can have in each version of the game, from the skills of a hero made by hand.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class MaxSpellPointsCalculatorTests
    {
        private static int Maximum(GameVersion version, int knowledge, SkillLevel intelligence)
        {
            var hero = HeroModels.Christian();
            var primary = hero.PrimarySkills.WithBaseKnowledge((byte)Math.Max(0, knowledge));
            var secondary = hero.SecondarySkills.WithSkillLevel(SecondarySkill.Intelligence, intelligence);
            return new MaxSpellPointsCalculator(version).GetMaximum(primary, secondary);
        }

        [Test]
        public void SpellPointsFollowKnowledgeAndIntelligenceInTheOriginalGames()
        {
            Assert.That(Maximum(GameVersion.ShadowOfDeath, 10, SkillLevel.None), Is.EqualTo(100));
            Assert.That(Maximum(GameVersion.ShadowOfDeath, 10, SkillLevel.Beginner), Is.EqualTo(125));
            Assert.That(Maximum(GameVersion.ShadowOfDeath, 10, SkillLevel.Expert), Is.EqualTo(200));
        }

        [Test]
        public void IntelligenceAddsLessInHornOfTheAbyss()
        {
            Assert.That(Maximum(GameVersion.HornOfTheAbyss, 10, SkillLevel.Intermediate), Is.EqualTo(135));
            Assert.That(Maximum(GameVersion.HornOfTheAbyss, 10, SkillLevel.Expert), Is.EqualTo(150));
        }
    }
}
