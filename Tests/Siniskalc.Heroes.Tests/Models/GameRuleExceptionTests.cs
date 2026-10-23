namespace Siniskalc.Heroes.Tests.Models
{
    using Siniskalc.Heroes.Models;

    /// <summary>
    /// Tests the exception for a rule of the game that an edit would break.
    /// </summary>
    [TestFixture]
    internal sealed class GameRuleExceptionTests
    {
        [Test]
        public void ARuleOfTheGameBrokenIsAnInvalidOperationWithTheMessageOfTheRule()
        {
            var exception = new GameRuleException("Every artifact slot is taken.");

            Assert.That(exception, Is.InstanceOf<InvalidOperationException>());
            Assert.That(exception.Message, Is.EqualTo("Every artifact slot is taken."));
        }
    }
}
