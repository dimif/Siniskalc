namespace Siniskalc.Heroes.Tests.Models
{
    using Siniskalc.Heroes.Models;

    [TestFixture]
    internal sealed class OwnerNamesTests
    {
        private enum Colour : byte
        {
            Red,
            Blue,
        }

        [Test]
        public void AnOwnerIsNamedAfterItsColourAndNoOwnerIsNeutral()
        {
            Assert.That(OwnerNames<Colour>.Of(Colour.Blue), Is.EqualTo("Blue"));
            Assert.That(OwnerNames<Colour>.Of(null), Is.EqualTo("Neutral"));
        }

        [Test]
        public void ANameIsReadBackAsItsColourAndAnythingElseIsNoOwner()
        {
            Assert.That(OwnerNames<Colour>.Parse("Blue"), Is.EqualTo(Colour.Blue));
            Assert.That(OwnerNames<Colour>.Parse("Neutral"), Is.Null);
            Assert.That(OwnerNames<Colour>.Parse("99"), Is.Null);
            Assert.That(OwnerNames<Colour>.Parse(null), Is.Null);
        }

        [Test]
        public void TheChoicesAreNeutralAndEveryColour()
        {
            Assert.That(OwnerNames<Colour>.Choices, Is.EqualTo(new[] { "Neutral", "Red", "Blue" }));
        }
    }
}
