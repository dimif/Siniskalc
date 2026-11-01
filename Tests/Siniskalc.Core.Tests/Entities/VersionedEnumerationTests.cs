namespace Siniskalc.Core.Tests.Entities
{
    using Siniskalc.Core.Entities;

    /// <summary>
    /// Tests the entities that appear from a version of a game.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class VersionedEnumerationTests
    {
        private enum Release
        {
            Unknown,
            First,
            Second,
        }

        [Test]
        public void AnEntryKnowsTheVersionItFirstAppearsInAndIsAvailableFromThenOn()
        {
            Assert.That(Thing.Old.GameVersion, Is.EqualTo(Release.First));
            Assert.That(Thing.New.GameVersion, Is.EqualTo(Release.Second));
            Assert.That(Thing.Old.IsAvailableIn(Release.First), Is.True);
            Assert.That(Thing.New.IsAvailableIn(Release.First), Is.False);
            Assert.That(Thing.New.IsAvailableIn(Release.Second), Is.True);
        }

        [Test]
        public void TheEntriesAreFoundByCodeAndByName()
        {
            Assert.That(AbstractEnumeration.GetAll<Thing>(), Is.EquivalentTo(new[] { Thing.Old, Thing.New }));
            Assert.That(AbstractEnumeration.FromCode<Thing>(2), Is.SameAs(Thing.New));
            Assert.That(AbstractEnumeration.FromName<Thing>("old"), Is.SameAs(Thing.Old));
        }

        [Test]
        public void OnlyTheEntriesOfTheVersionAreListed()
        {
            Assert.That(AbstractEnumeration.GetAll<Thing>().AvailableIn(Release.First), Is.EqualTo(new[] { Thing.Old }));
            Assert.That(AbstractEnumeration.GetAll<Thing>().AvailableIn(Release.Second), Is.EquivalentTo(new[] { Thing.Old, Thing.New }));
        }

        private sealed class Thing : VersionedEnumeration<Release>
        {
            public static readonly Thing Old = new("Old", 1, Release.First);
            public static readonly Thing New = new("New", 2, Release.Second);

            private Thing(string name, byte code, Release release)
                : base(name, code, release)
            {
            }
        }
    }
}
