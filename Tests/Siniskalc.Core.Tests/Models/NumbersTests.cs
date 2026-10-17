namespace Siniskalc.Core.Tests.Models
{
    using Siniskalc.Core.Models;

    /// <summary>
    /// Tests the list of numbers a model keeps.
    /// </summary>
    [TestFixture]
    internal sealed class NumbersTests
    {
        [Test]
        public void TwoListsWithTheSameNumbersAreEqual()
        {
            Assert.That(Numbers.Of(1, 2, 3), Is.EqualTo(new Numbers([1, 2, 3])));
            Assert.That(Numbers.Of(1, 2, 3).GetHashCode(), Is.EqualTo(Numbers.Of(1, 2, 3).GetHashCode()));
            Assert.That(Numbers.Of(1, 2, 3), Is.Not.EqualTo(Numbers.Of(1, 2, 4)));
        }

        [Test]
        public void ChangingANumberGivesANewListAndLeavesTheOldOneAlone()
        {
            var numbers = Numbers.Of(5, 6, 7);

            var changed = numbers.With(1, 60);

            Assert.That(changed, Is.EqualTo(Numbers.Of(5, 60, 7)));
            Assert.That(numbers, Is.EqualTo(Numbers.Of(5, 6, 7)));
            Assert.That(changed.Count, Is.EqualTo(3));
        }
    }
}
