using Siniskalc.Homm3.Data;

namespace Siniskalc.Homm3.Tests.Data
{
    /// <summary>
    /// The offsets worked out from where the setup of the players was found: the difficulty after the eight colours, the current player
    /// after the eight 32-bit human flags, and the length of the marker, which depends on the game.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class PlayerSetupLocationTests
    {
        private const int HotaToMarker = 40;
        private const int ClassicToMarker = 36;

        [Test]
        public void TheDifficultyIsTheByteAfterTheEightBytesOfTheColours()
        {
            var location = new PlayerSetupLocation(100, 120, 12, 200);

            Assert.That(location.DifficultyAt, Is.EqualTo(108));
        }

        [Test]
        public void TheCurrentPlayerIsAfterTheEightHumanFlagsOfFourBytes()
        {
            var location = new PlayerSetupLocation(100, 120, 12, 200);

            Assert.That(location.CurrentPlayerAt, Is.EqualTo(232));
        }

        [Test]
        public void TheHumanFlagsAreFoundWhenTheirOffsetIsZeroOrMoreAndNotWhenItIsMinusOne()
        {
            Assert.That(new PlayerSetupLocation(100, 120, 12, 0).HasHumanFlags, Is.True);
            Assert.That(new PlayerSetupLocation(100, 120, 12, 200).HasHumanFlags, Is.True);
            Assert.That(new PlayerSetupLocation(100, 120, 12, -1).HasHumanFlags, Is.False);
        }

        [Test]
        public void WithNoDistanceGivenTheMarkerIsThatOfHornOfTheAbyss()
        {
            var location = new PlayerSetupLocation(100, 120, 12, 200);

            Assert.That(location.HumanFlagsToMarker, Is.EqualTo(HotaToMarker));
            Assert.That(location.MarkerLength, Is.EqualTo(2));
        }

        [Test]
        public void TheMarkerOfTheOtherVersionsIsFourBytesAndFartherFromTheFlagsByFourLess()
        {
            var location = new PlayerSetupLocation(100, 120, 12, 200, ClassicToMarker);

            Assert.That(location.MarkerLength, Is.EqualTo(4));
            Assert.That(HotaToMarker - ClassicToMarker, Is.EqualTo(4));
        }
    }
}
