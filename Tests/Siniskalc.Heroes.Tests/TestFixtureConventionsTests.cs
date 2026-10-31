namespace Siniskalc.Heroes.Tests
{
    /// <summary>
    /// Checks the fixtures of this project against the conventions of <c>TestFixtureConventions</c>: every one has a kind, and the folder fits it.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class TestFixtureConventionsTests
    {
        [Test]
        public void EveryFixtureHasOneKindAndSitsInTheFolderOfThatKind()
        {
            Assert.That(Siniskalc.Tests.Homm.Common.TestFixtureConventions.Violations(typeof(TestFixtureConventionsTests).Assembly), Is.Empty);
        }
    }
}
