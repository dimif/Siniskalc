using Siniskalc.Core;

namespace Siniskalc.Core.Tests
{
    [TestFixture]
    internal sealed class RangeGuardTests
    {
        [Test]
        public void IsInRangeIncludesBothEnds()
        {
            Assert.That(RangeGuard.IsInRange(1, 1, 5), Is.True);
            Assert.That(RangeGuard.IsInRange(5, 1, 5), Is.True);
            Assert.That(RangeGuard.IsInRange(0, 1, 5), Is.False);
            Assert.That(RangeGuard.IsInRange(6, 1, 5), Is.False);
        }

        [Test]
        public void ClampKeepsTheValueInsideTheRange()
        {
            Assert.That(RangeGuard.Clamp(-7, 0, 9), Is.EqualTo(0));
            Assert.That(RangeGuard.Clamp(12L, 0L, 9L), Is.EqualTo(9L));
            Assert.That(RangeGuard.Clamp(4, 0, 9), Is.EqualTo(4));
        }

        [Test]
        public void ThrowIfOutOfRangeThrowsArgumentOutOfRangeWithTheGivenMessage()
        {
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => RangeGuard.ThrowIfOutOfRange(10, 1, 7, "slot", "Slots are 1 to 7."));

            Assert.That(exception!.ParamName, Is.EqualTo("slot"));
            Assert.That(exception.Message, Does.StartWith("Slots are 1 to 7."));
            Assert.DoesNotThrow(() => RangeGuard.ThrowIfOutOfRange(7, 1, 7, "slot"));
        }

        [Test]
        public void ThrowIfNotIndexRejectsNegativeAndTooLargeIndexes()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => RangeGuard.ThrowIfNotIndex(-1, 3, "index"));
            Assert.Throws<ArgumentOutOfRangeException>(() => RangeGuard.ThrowIfNotIndex(3, 3, "index"));
            Assert.DoesNotThrow(() => RangeGuard.ThrowIfNotIndex(2, 3, "index"));
        }
    }
}
