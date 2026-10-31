namespace Siniskalc.Heroes.Tests.Models
{
    using Siniskalc.Heroes.Models;

    /// <summary>
    /// Tests the wording of the notice that a list of a player is full.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class FullListNoticeTests
    {
        [TestCase("Blue", "heroes", 8, "Blue has the most heroes a player can have (8).")]
        [TestCase("Green", "castles", 6, "Green has the most castles a player can have (6).")]
        public void TheNoticeNamesTheColorWhatIsFullAndTheLimit(string color, string plural, int limit, string expected)
        {
            Assert.That(FullListNotice.For(color, plural, limit), Is.EqualTo(expected));
        }
    }
}
