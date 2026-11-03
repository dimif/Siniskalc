namespace Siniskalc.Heroes.Tests.Models
{
    using Siniskalc.Heroes.Models;

    /// <summary>
    /// Tests the words a change uses for yes, no, empty and none.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class ChangeTextTests
    {
        [Test]
        public void ABooleanIsWordedYesOrNo()
        {
            Assert.That((ChangeText.YesNo(true), ChangeText.YesNo(false)), Is.EqualTo(("yes", "no")));
        }

        [Test]
        public void ANumberIsWordedAsItIsAndNoNumberAsNone()
        {
            Assert.That(ChangeText.Number(12), Is.EqualTo("12"));
            Assert.That(ChangeText.Number(0), Is.EqualTo("0"));
            Assert.That(ChangeText.Number(-1), Is.EqualTo("-1"));
            Assert.That(ChangeText.Number(null), Is.EqualTo("none"));
        }

        [Test]
        public void TheWordsForNothingAreEmptyAndNone()
        {
            Assert.That((ChangeText.Empty, ChangeText.None), Is.EqualTo(("empty", "none")));
        }
    }
}
