namespace Siniskalc.Heroes.Tests.Models
{
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;

    /// <summary>
    /// Tests where a hero stands and which way it faces, which both games keep in the same way.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class HeroPositionTests
    {
        [Test]
        public void AHeroOnTheSurfaceIsAtLevelZero()
        {
            var position = new HeroPosition(3, 4, FacingDirection.East);

            Assert.That((position.X, position.Y, position.Level, position.FacingDirection), Is.EqualTo((3, 4, MapPosition.Above, FacingDirection.East)));
        }

        [Test]
        public void ThePositionIsWrittenAsCoordinatesWhateverWayTheHeroFaces()
        {
            Assert.That(new HeroPosition(3, 4, FacingDirection.West).ToString(), Is.EqualTo("3,4"));
            Assert.That(new HeroPosition(3, 4, MapPosition.Underground, FacingDirection.West).ToString(), Is.EqualTo("3,4,1"));
        }

        [Test]
        public void TurningAHeroLeavesItsTileAlone()
        {
            var turned = new HeroPosition(3, 4, FacingDirection.North) with { FacingDirection = FacingDirection.South };

            Assert.That((turned.X, turned.Y, turned.FacingDirection), Is.EqualTo((3, 4, FacingDirection.South)));
        }
    }
}
