using Moq;
using Siniskalc.Core.Data;
using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Models;

namespace Siniskalc.Homm3.Tests.Data
{
    /// <summary>
    /// Tests what the collection of the castles adds to a plain list of chunks: finding the castle a hero is garrisoned in, the owners,
    /// and a copy when a chunk is replaced. The deserializer is a fake that reads the owner byte.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class CastleChunkCollectionTests
    {
        private static CastleChunk Castle(byte garrisoned, byte owner)
        {
            var bytes = new byte[CastleLayout.FixedLength];
            bytes[CastleLayout.GarrisonedHero] = garrisoned;
            bytes[CastleLayout.Owner] = owner;
            return new CastleChunk("Castle", bytes, 0);
        }

        private static CastleChunkCollection Castles()
        {
            var reader = new Mock<ICastleDeserializer>();
            reader.Setup(d => d.DeserializeOwner(It.IsAny<CastleChunk>())).Returns((CastleChunk c) => PlayerColor.FromByte(c.GetByteAt(CastleLayout.Owner)));
            return new CastleChunkCollection(
                [Castle(5, 0), Castle(0xFF, 1), Castle(9, 0xFF)],
                reader.Object,
                Mock.Of<ISerializer<CastleChunk, CastleModel>>(),
                Mock.Of<ICastleDiffer>());
        }

        [Test]
        public void TheCastlesAreFoundByTheHeroGarrisonedInThemAndTheirOwnersAreListed()
        {
            var castles = Castles();

            Assert.That((castles.FindByGarrisonedHero(5), castles.FindByGarrisonedHero(9), castles.FindByGarrisonedHero(77)), Is.EqualTo((0, 2, -1)));
            Assert.That(castles.GetOwners(), Is.EqualTo(new[] { PlayerColor.Red, PlayerColor.Blue, PlayerColor.Neutral }));
        }

        [Test]
        public void ReplacingACastleKeepsTheOthers()
        {
            var castles = Castles();
            var replacement = Castle(1, 2);

            var changed = castles.WithChunk(2, replacement);

            Assert.That(changed[2], Is.SameAs(replacement));
            Assert.That((changed[0], changed[1]), Is.EqualTo((castles[0], castles[1])));
            Assert.That(changed.FindByGarrisonedHero(1), Is.EqualTo(2));
            Assert.That(changed.FindByGarrisonedHero(9), Is.EqualTo(-1));
            Assert.That(changed.Deserializer, Is.SameAs(castles.Deserializer));
        }
    }
}
