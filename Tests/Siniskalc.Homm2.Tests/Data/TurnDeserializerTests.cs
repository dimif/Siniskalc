namespace Siniskalc.Homm2.Tests.Data
{
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;

    /// <summary>
    /// What the byte of the current turn reads as, on records made by hand: the number of the record of the player whose turn it is, and the
    /// colour the savegame gives that record.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class TurnDeserializerTests
    {
        private static MapObjectChunk Turn(byte record, int[] colorsOfTheRecords) =>
            new("Turn", [record], 600, null, 3, PlayerRecordOrder.Pack(colorsOfTheRecords));

        [Test]
        public void TheRecordOfThePlayerWhoseTurnItIsGetsTheColourTheSavegameGivesIt()
        {
            var turn = new TurnDeserializer().Deserialize(Turn(0, [3, 1, 2, 0, 4, 5]));

            Assert.That((turn.Index, turn.Record, turn.Color), Is.EqualTo((3, 0, PlayerColor.Yellow)));
        }

        [Test]
        public void WhenTheRecordsAreInTheOrderOfTheColoursTheRecordIsTheColour()
        {
            var turn = new TurnDeserializer().Deserialize(Turn(4, [0, 1, 2, 3, 4, 5]));

            Assert.That(turn.Color, Is.EqualTo(PlayerColor.Orange));
        }

        [TestCase(6)]
        [TestCase(0xFF)]
        public void ARecordPastTheSixPlayersHasNoColourAndIsKeptAsTheNumberItIs(int record)
        {
            var turn = new TurnDeserializer().Deserialize(Turn((byte)record, [0, 1, 2, 3, 4, 5]));

            Assert.That((turn.Record, turn.Color), Is.EqualTo((record, (PlayerColor?)null)));
        }
    }
}
