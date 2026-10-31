using Siniskalc.Heroes.Models;
using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;

namespace Siniskalc.Homm3.Tests.Data
{
    /// <summary>
    /// What the bytes of a record of a player read as, one field at a time on records made by hand, for both layouts of the record,
    /// including the bytes that are no known entity.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class PlayerDeserializerTests
    {
        private readonly PlayerDeserializer deserializer = new();

        private static IEnumerable<PlayerLayout> Layouts => PlayerRecords.Layouts;

        [TestCaseSource(nameof(Layouts))]
        public void AColourByteFromOneToEightReadsAsTheColourBeforeItInTheOrderOfTheColours(PlayerLayout layout)
        {
            var colors = Enumerable.Range(1, 8).Select(b => this.deserializer.DeserializeColor(PlayerRecords.Blank(layout, PlayerColor.Neutral, (byte)b)));

            Assert.That(colors, Is.EqualTo(new[] { PlayerColor.Red, PlayerColor.Blue, PlayerColor.Tan, PlayerColor.Green, PlayerColor.Orange, PlayerColor.Purple, PlayerColor.Teal, PlayerColor.Pink }));
        }

        [TestCaseSource(nameof(Layouts))]
        public void AColourByteThatIsNoColourReadsAsTheColourTheLoaderGaveTheChunk(PlayerLayout layout)
        {
            foreach (var colorByte in new byte[] { 0, 9, 0x80, 0xFF })
            {
                var player = this.deserializer.Deserialize(PlayerRecords.Blank(layout, PlayerColor.Teal, colorByte));

                Assert.That(player.Color, Is.EqualTo(PlayerColor.Teal), $"colour byte {colorByte:X2}");
            }
        }

        [TestCaseSource(nameof(Layouts))]
        public void TheResourcesAreSignedFourByteNumbersInTheOrderOfTheResourceSet(PlayerLayout layout)
        {
            var chunk = PlayerRecords.Blank(layout, PlayerColor.Red, 1, b =>
            {
                for (var i = 0; i < 7; i++)
                {
                    BitConverter.GetBytes(100 * (i + 1)).CopyTo(b, PlayerLayout.ResourcesOffset + (i * 4));
                }

                BitConverter.GetBytes(-5).CopyTo(b, PlayerLayout.ResourcesOffset + 24);
            });

            Assert.That(this.deserializer.DeserializeResources(chunk), Is.EqualTo(new ResourceSet(100, 200, 300, 400, 500, 600, -5)));
        }

        [TestCaseSource(nameof(Layouts))]
        public void TheTavernHeroesAreTheIdsThatAreNotTheEmptyMarkAndNoneForTheEmptyMark(PlayerLayout layout)
        {
            var empty = this.deserializer.Deserialize(PlayerRecords.Blank(layout, PlayerColor.Red, 1));
            var set = this.deserializer.Deserialize(PlayerRecords.Blank(layout, PlayerColor.Red, 1, b =>
            {
                b[PlayerLayout.TavernHeroes] = 5;
                b[PlayerLayout.TavernHeroes + 1] = 6;
            }));

            Assert.That(empty.Tavern, Is.EqualTo(new TavernHeroes(null, null)));
            Assert.That(set.Tavern, Is.EqualTo(new TavernHeroes(5, 6)));
        }

        [TestCaseSource(nameof(Layouts))]
        public void TheTavernHeroWithIdZeroIsAHeroAndNotNone(PlayerLayout layout)
        {
            var player = this.deserializer.Deserialize(PlayerRecords.Blank(layout, PlayerColor.Red, 1, b => b[PlayerLayout.TavernHeroes] = 0));

            Assert.That(player.Tavern, Is.EqualTo(new TavernHeroes(0, null)));
        }

        [TestCaseSource(nameof(Layouts))]
        public void TheDaysLeftAreTheByteUpToTheMostAPlayerCanHaveAndAnyOtherByteReadsAsNone(PlayerLayout layout)
        {
            int? Days(byte value) => this.deserializer.DeserializeDaysLeft(PlayerRecords.Blank(layout, PlayerColor.Red, 1, b => b[PlayerLayout.DaysLeft] = value));

            Assert.That((Days(0), Days(3), Days(7)), Is.EqualTo(((int?)0, (int?)3, (int?)7)));
            Assert.That((Days(8), Days(0x40), Days(0xFF)), Is.EqualTo(((int?)null, (int?)null, (int?)null)));
        }
    }
}
