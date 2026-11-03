using Moq;
using Siniskalc.Core.Data;
using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Models;

namespace Siniskalc.Homm3.Tests.Data
{
    /// <summary>
    /// Tests what the collection of the players adds to a plain list of chunks: finding a player by its colour and a copy that keeps the
    /// settings when a chunk is replaced. The deserializer is a fake that reads the colour byte, so no savefile is needed.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class PlayerChunkCollectionTests
    {
        private static PlayerChunk Player(PlayerColor color) => PlayerRecords.Blank(PlayerLayout.ShadowOfDeath, color, (byte)color.Code);

        private static PlayerChunkCollection Players(IPlayerDiffer? differ = null)
        {
            var deserializer = new Mock<IPlayerDeserializer>();
            deserializer.Setup(d => d.DeserializeColor(It.IsAny<PlayerChunk>())).Returns((PlayerChunk c) => PlayerColor.FromByte(c.GetByteAt(PlayerLayout.ShadowOfDeath.ColorOffset)));
            return new PlayerChunkCollection(
                [Player(PlayerColor.Red), Player(PlayerColor.Blue), Player(PlayerColor.Teal)],
                deserializer.Object,
                Mock.Of<ISerializer<PlayerChunk, PlayerModel>>(),
                Mock.Of<IDeserializer<PlayerChunk, PlayerLists>>(),
                Mock.Of<ISerializer<PlayerChunk, PlayerLists>>(),
                differ ?? Mock.Of<IPlayerDiffer>(),
                id => $"Hero {id}");
        }

        [Test]
        public void ThePlayersAreFoundByTheirColourAndTheColoursAreListedInOrder()
        {
            var players = Players();

            Assert.That(players.GetColors(), Is.EqualTo(new[] { PlayerColor.Red, PlayerColor.Blue, PlayerColor.Teal }));
            Assert.That(players.GetColor(1), Is.SameAs(PlayerColor.Blue));
            Assert.That((players.FindByColor(PlayerColor.Red), players.FindByColor(PlayerColor.Teal), players.FindByColor(PlayerColor.Green)), Is.EqualTo((0, 2, -1)));
        }

        [Test]
        public void ReplacingAPlayerKeepsTheOtherChunksAndTheReaders()
        {
            var players = Players();
            var replacement = Player(PlayerColor.Orange);

            var changed = players.WithChunk(1, replacement);

            Assert.That(changed, Is.Not.SameAs(players));
            Assert.That(changed[1], Is.SameAs(replacement));
            Assert.That((changed[0], changed[2]), Is.EqualTo((players[0], players[2])));
            Assert.That(changed.Deserializer, Is.SameAs(players.Deserializer));
            Assert.That(changed.Serializer, Is.SameAs(players.Serializer));
            Assert.That(players[1], Is.Not.SameAs(replacement), "the collection is immutable");
        }

        [Test]
        public void TheDifferOfAReplacedCollectionStillNamesHeroesLikeTheOriginal()
        {
            Func<int, string>? names = null;
            var differ = new Mock<IPlayerDiffer>();
            differ.Setup(d => d.Diff(It.IsAny<int>(), It.IsAny<PlayerModel>(), It.IsAny<PlayerModel>(), It.IsAny<Func<int, string>>()))
                .Callback((int _, PlayerModel _, PlayerModel _, Func<int, string> heroName) => names = heroName)
                .Returns([]);
            var players = Players(differ.Object);
            var bytes = Player(PlayerColor.Blue).ToArray();
            bytes[0] ^= 1;

            var changed = players.WithChunk(1, new PlayerChunk(PlayerColor.Blue, bytes, 1000, PlayerLayout.ShadowOfDeath));
            changed.GetPendingChanges(players);

            Assert.That(names, Is.Not.Null);
            Assert.That(names!(5), Is.EqualTo("Hero 5"));
        }
    }
}
