namespace Siniskalc.Tests.Models
{
    using Siniskalc.Entities;
    using Siniskalc.Models;

    [TestFixture]
    internal sealed class OwnerNamesTests
    {
        [Test]
        public void AnOwnerIsNamedAfterItsColourAndNoOwnerIsNeutral()
        {
            Assert.That(OwnerNames.Of(PlayerColor.Teal), Is.EqualTo("Teal"));
            Assert.That(OwnerNames.Of(null), Is.EqualTo("Neutral"));
        }

        [Test]
        public void ANameIsReadBackAsItsColourAndAnythingElseIsNoOwner()
        {
            Assert.That(OwnerNames.Parse("Pink"), Is.EqualTo(PlayerColor.Pink));
            Assert.That(OwnerNames.Parse("Neutral"), Is.Null);
            Assert.That(OwnerNames.Parse("99"), Is.Null);
            Assert.That(OwnerNames.Parse(null), Is.Null);
        }

        [Test]
        public void TheChoicesAreNeutralAndEveryColour()
        {
            Assert.That(OwnerNames.Choices, Is.EqualTo(new[] { "Neutral", "Red", "Blue", "Tan", "Green", "Orange", "Purple", "Teal", "Pink" }));
        }
    }
}
