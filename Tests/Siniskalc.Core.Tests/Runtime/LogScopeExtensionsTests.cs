namespace Siniskalc.Core.Tests.Runtime
{
    using System.Collections.Generic;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Siniskalc.Core.Runtime;

    /// <summary>
    /// Tests the scope that tells in the log which hero, castle or player a line is about.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class LogScopeExtensionsTests
    {
        [Test]
        public void ASubjectScopeHoldsTheKindAndTheNameUnderTheSubjectProperty()
        {
            object? scope = null;
            var logger = new Mock<ILogger>();
            logger.Setup(l => l.BeginScope(It.IsAny<It.IsAnyType>())).Callback(new InvocationAction(i => scope = i.Arguments[0]));

            logger.Object.BeginSubject("Hero", "Orrin");

            Assert.That(scope, Is.InstanceOf<Dictionary<string, object>>());
            Assert.That(((Dictionary<string, object>)scope!)[LogScopeExtensions.SubjectProperty], Is.EqualTo("Hero Orrin"));
            Assert.That(LogScopeExtensions.SubjectProperty, Is.EqualTo("Subject"));
        }

        [Test]
        public void TheScopeTheLoggerGivesIsTheOneReturned()
        {
            var disposable = Mock.Of<IDisposable>();
            var logger = new Mock<ILogger>();
            logger.Setup(l => l.BeginScope(It.IsAny<It.IsAnyType>())).Returns(disposable);

            Assert.That(logger.Object.BeginSubject("Castle", "Haret"), Is.SameAs(disposable));
        }
    }
}
