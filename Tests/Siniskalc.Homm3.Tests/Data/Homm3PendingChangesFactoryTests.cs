namespace Siniskalc.Homm3.Tests.Data
{
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Models;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm3.Data;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// Tests the pending changes the editor of Heroes of Might and Magic III makes.
    /// </summary>
    [TestFixture]
    internal sealed class Homm3PendingChangesFactoryTests
    {
        private static readonly Change SomeChange = new(ChangeAreas.General, "Owner", "Neutral", "Red");

        [Test]
        public void TheChangeOfAHeroIsFiledUnderTheHeroChunksAtItsIndex()
        {
            var change = new Homm3PendingChangesFactory().ForHero("Christian", 4, SomeChange, hero => hero);

            Assert.That((change.OwnerName, change.Kind, change.Index, change.Change), Is.EqualTo(("Christian", Homm3ChunkKinds.Hero, 4, SomeChange)));
        }

        [Test]
        public void MapObjectsAreNotYetSupportedAndSayWhy()
        {
            var factory = new Homm3PendingChangesFactory();

            var thrown = Assert.Throws<NotImplementedException>(() => factory.ForMapObject<PlayerModel>(Homm3ChunkKinds.Player, "Mine", 0, SomeChange, model => model));

            Assert.That(thrown!.Message, Does.Contain("not yet supported in HoMM3"));
        }
    }
}
