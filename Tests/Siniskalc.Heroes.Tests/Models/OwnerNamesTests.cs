namespace Siniskalc.Heroes.Tests.Models
{
    using Siniskalc.Heroes.Tests.Entities;
    using Siniskalc.Heroes.Models;

    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class OwnerNamesTests
    {
        [Test]
        public void AnOwnerIsNamedAfterItsTestColorAndNeutralIsNamedNeutral()
        {
            Assert.That(OwnerNames<TestColor>.Of(TestColor.Blue), Is.EqualTo("Blue"));
            Assert.That(OwnerNames<TestColor>.Of(TestColor.Neutral), Is.EqualTo("Neutral"));
        }

        [Test]
        public void ANameIsReadBackAsItsTestColorAndAnythingElseIsNeutral()
        {
            Assert.That(OwnerNames<TestColor>.Parse("Blue"), Is.EqualTo(TestColor.Blue));
            Assert.That(OwnerNames<TestColor>.Parse("Neutral"), Is.EqualTo(TestColor.Neutral));
            Assert.That(OwnerNames<TestColor>.Parse("99"), Is.EqualTo(TestColor.Neutral));
            Assert.That(OwnerNames<TestColor>.Parse(null), Is.EqualTo(TestColor.Neutral));
        }

        [Test]
        public void TheChoicesAreNeutralAndEveryTestColor()
        {
            Assert.That(OwnerNames<TestColor>.Choices, Is.EqualTo(new[] { "Neutral", "Red", "Blue" }));
        }
    }
}
