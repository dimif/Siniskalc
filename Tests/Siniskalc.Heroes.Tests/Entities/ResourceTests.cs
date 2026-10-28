namespace Siniskalc.Heroes.Tests.Entities
{
    using Siniskalc.Heroes.Entities;

    /// <summary>
    /// Tests the resources the games share.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class ResourceTests
    {
        [Test]
        public void TheResourcesAreInTheOrderOfTheSavegameTablesWithGoldLast()
        {
            Assert.That(Enum.GetValues<Resource>(), Is.EqualTo(new[] { Resource.Wood, Resource.Mercury, Resource.Ore, Resource.Sulfur, Resource.Crystal, Resource.Gems, Resource.Gold }));
            Assert.That((byte)Resource.Gold, Is.EqualTo(6));
        }
    }
}
