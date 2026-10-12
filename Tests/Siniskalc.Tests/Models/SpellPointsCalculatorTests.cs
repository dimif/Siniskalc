using Siniskalc;
using Siniskalc.Data;
using Siniskalc.Entities;
using Siniskalc.Models;

namespace Siniskalc.Tests.Models
{
    [TestFixture]
    internal sealed class SpellPointsCalculatorTests
    {
        [Test]
        public void SpellPointsFollowKnowledgeAndIntelligence()
        {
            var calculator = new SpellPointsCalculator();

            Assert.That(calculator.GetMaximum(GameVersion.ShadowOfDeath, 10, null), Is.EqualTo(100));
            Assert.That(calculator.GetMaximum(GameVersion.ShadowOfDeath, 10, SkillLevel.Beginner), Is.EqualTo(125));
            Assert.That(calculator.GetMaximum(GameVersion.ShadowOfDeath, 10, SkillLevel.Expert), Is.EqualTo(200));
            Assert.That(calculator.GetMaximum(GameVersion.HornOfTheAbyss, 10, SkillLevel.Intermediate), Is.EqualTo(135));
            Assert.That(calculator.GetMaximum(GameVersion.HornOfTheAbyss, -3, SkillLevel.Expert), Is.EqualTo(0));
        }
    }
}
