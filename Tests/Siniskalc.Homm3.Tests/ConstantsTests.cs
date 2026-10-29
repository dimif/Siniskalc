using Siniskalc.Homm3.Models;

namespace Siniskalc.Homm3.Tests
{
    [TestFixture]
    internal sealed class ConstantsTests
    {
        [Test]
        public void TheLevelLimitsMatchTheExperienceTable()
        {
            Assert.That(LevelCalculator.LevelToExperienceThresholds.Keys.Min(), Is.EqualTo((short)Constants.Hero.MinLevel));
            Assert.That(LevelCalculator.LevelToExperienceThresholds.Keys.Max(), Is.EqualTo((short)Constants.Hero.MaxLevel));
            Assert.That(LevelCalculator.LevelToExperienceThresholds.Values.Max(), Is.LessThanOrEqualTo(Constants.Hero.MaxExperience));
        }

        [Test]
        public void TheColourCountIsTheNumberOfPlayerColours()
        {
            Assert.That(Constants.Player.ColorCount, Is.EqualTo(Siniskalc.Homm3.Entities.PlayerColor.All.Count));
        }

        [Test]
        public void TheLastMapCoordinateIsOneLessThanTheBiggestMap()
        {
            Assert.That(Constants.Map.MaxCoordinate, Is.EqualTo(Constants.Map.MaxSize - 1));
            Assert.That(Constants.Map.MinCoordinate, Is.EqualTo(Constants.Map.NoCoordinate));
        }
    }
}
