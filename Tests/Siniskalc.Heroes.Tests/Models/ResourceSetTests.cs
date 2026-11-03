namespace Siniskalc.Heroes.Tests.Models
{
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;

    /// <summary>
    /// Tests the amount of each resource.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class ResourceSetTests
    {
        [Test]
        public void EachResourceHasItsOwnAmountAndCanBeReplacedWithoutChangingTheOthers()
        {
            var set = ResourceSet.None;

            foreach (var resource in ResourceSet.All)
            {
                set = set.With(resource, 10 + (long)resource);
            }

            Assert.That(ResourceSet.All.Select(r => set[r]), Is.EqualTo(new long[] { 10, 11, 12, 13, 14, 15, 16 }));
            Assert.That(set.With(Resource.Gold, 5).Wood, Is.EqualTo(10));
            Assert.That(set.Gold, Is.EqualTo(16));
        }

        [Test]
        public void TheResourcesAreInTheOrderOfTheSavegameTablesAndEqualSetsAreEqual()
        {
            Assert.That(ResourceSet.All, Is.EqualTo(new[] { Resource.Wood, Resource.Mercury, Resource.Ore, Resource.Sulfur, Resource.Crystal, Resource.Gems, Resource.Gold }));
            Assert.That(new ResourceSet(1, 2, 3, 4, 5, 6, 7), Is.EqualTo(new ResourceSet(1, 2, 3, 4, 5, 6, 7)));
        }
    }
}
