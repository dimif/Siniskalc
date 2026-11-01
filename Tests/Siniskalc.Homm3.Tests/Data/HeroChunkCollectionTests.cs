using Moq;
using Siniskalc.Core.Data;
using Siniskalc.Heroes.Models;
using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Models;

namespace Siniskalc.Homm3.Tests.Data
{
    /// <summary>
    /// Tests what the collection of the heroes adds to a plain list of chunks: finding a hero by its id, summaries and owners, and a copy
    /// that keeps the offsets and readers when a chunk is replaced. The deserializer is a fake that reads the first byte.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class HeroChunkCollectionTests
    {
        private static HeroChunk Hero(string name, byte id) => new(name, [id, 0, 0], 0);

        private static HeroChunkCollection Heroes(HeroOffsets? offsets = null)
        {
            var reader = new Mock<IHeroDeserializer>();
            reader.Setup(d => d.DeserializeHeroId(It.IsAny<HeroChunk>())).Returns((HeroChunk c) => c.GetByteAt(0));
            reader.Setup(d => d.DeserializeOwner(It.IsAny<HeroChunk>())).Returns((HeroChunk c) => c.GetByteAt(0) < 10 ? PlayerColor.Red : PlayerColor.Blue);
            reader.Setup(d => d.DeserializeClass(It.IsAny<HeroChunk>())).Returns(TestCatalog.For(GameVersion.HornOfTheAbyss).HeroClasses.ById("Knight"));
            reader.Setup(d => d.DeserializeLevel(It.IsAny<HeroChunk>())).Returns((HeroChunk c) => HeroLevel.AtLevel(LevelCalculator.Table, c.GetByteAt(0)));
            return new HeroChunkCollection(
                [Hero("Orrin", 3), Hero("Gem", 12), Hero("Ash", 7)],
                offsets ?? new HeroOffsets(GameVersion.ShadowOfDeath),
                reader.Object,
                Mock.Of<ISerializer<HeroChunk, HeroModel>>(),
                Mock.Of<IDiffer<HeroModel>>(), TestCatalog.For((offsets ?? new HeroOffsets(GameVersion.ShadowOfDeath)).GameVersion));
        }

        [Test]
        public void TheHeroesAreFoundByTheirIdAndSummarisedWithTheirOwnerClassAndLevel()
        {
            var heroes = Heroes();

            Assert.That((heroes.IndexOfId(12), heroes.IndexOfId(99)), Is.EqualTo((1, -1)));
            Assert.That(heroes.GetSummary(1), Is.EqualTo(new HeroSummary<PlayerColor>(1, 12, "Gem", PlayerColor.Blue, "Knight", 12)));
            Assert.That(heroes.FindById(7)!.Name, Is.EqualTo("Ash"));
            Assert.That(heroes.FindById(99), Is.Null);
            Assert.That(heroes.GetSummaries().Select(s => s.Index), Is.EqualTo(new[] { 0, 1, 2 }));
            Assert.That(heroes.GetOwners(), Is.EqualTo(new[] { PlayerColor.Red, PlayerColor.Blue, PlayerColor.Red }));
        }

        [Test]
        public void ReplacingAHeroKeepsTheOffsetsAndTheReaders()
        {
            var offsets = new HeroOffsets(GameVersion.HornOfTheAbyss);
            var heroes = Heroes(offsets);

            var changed = heroes.WithChunk(0, Hero("New", 1));

            Assert.That(changed[0].Name, Is.EqualTo("New"));
            Assert.That(changed.Offsets, Is.SameAs(offsets));
            Assert.That(changed.Deserializer, Is.SameAs(heroes.Deserializer));
            Assert.That(changed.GetSummary(0).Name, Is.EqualTo("New"));
        }

        [Test]
        public void AHeroCanBeAsleepOnlyInTheVersionsThatHaveTheMark()
        {
            Assert.That(Heroes(new HeroOffsets(GameVersion.ShadowOfDeath)).SupportsAsleep, Is.True);
            Assert.That(Heroes(new HeroOffsets(GameVersion.RestorationOfErathia)).SupportsAsleep, Is.False);
        }
    }
}
