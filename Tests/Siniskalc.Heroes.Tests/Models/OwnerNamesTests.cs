namespace Siniskalc.Heroes.Tests.Models
{
    using Siniskalc.Heroes.Tests.Entities;
    using Siniskalc.Heroes.Models;

    [TestFixture]
    internal sealed class OwnerNamesTests
    {
        [Test]
        public void AnOwnerIsNamedAfterItsTestColorAndNoOwnerIsNeutral()
        {
            Assert.That(OwnerNames<TestColor>.Of(TestColor.Blue), Is.EqualTo("Blue"));
            Assert.That(OwnerNames<TestColor>.Of(null), Is.EqualTo("Neutral"));
        }

        [Test]
        public void ANameIsReadBackAsItsTestColorAndAnythingElseIsNoOwner()
        {
            Assert.That(OwnerNames<TestColor>.Parse("Blue"), Is.EqualTo(TestColor.Blue));
            Assert.That(OwnerNames<TestColor>.Parse("Neutral"), Is.Null);
            Assert.That(OwnerNames<TestColor>.Parse("99"), Is.Null);
            Assert.That(OwnerNames<TestColor>.Parse(null), Is.Null);
        }

        [Test]
        public void TheChoicesAreNeutralAndEveryTestColor()
        {
            Assert.That(OwnerNames<TestColor>.Choices, Is.EqualTo(new[] { "Neutral", "Red", "Blue" }));
        }
    }
}
