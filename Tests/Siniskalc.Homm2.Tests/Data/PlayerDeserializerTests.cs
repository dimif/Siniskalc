namespace Siniskalc.Homm2.Tests.Data
{
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;

    /// <summary>
    /// What the bytes of a record of a player read as, one field at a time on records made by hand.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class PlayerDeserializerTests
    {
        private readonly PlayerDeserializer deserializer = new();

        [Test]
        public void TheColourOfThePlayerIsTheNameOfItsRecord()
        {
            Assert.That(this.deserializer.Deserialize(BlankRecords.Player("Orange")).Color, Is.EqualTo(PlayerColor.Orange));
        }

        [Test]
        public void TheResourcesAreFourByteNumbersInTheOrderOfTheResourceSet()
        {
            var player = this.deserializer.Deserialize(BlankRecords.Player("Blue", b =>
            {
                for (var i = 0; i < 6; i++)
                {
                    BitConverter.GetBytes((uint)(100 * (i + 1))).CopyTo(b, PlayerLayout.Resources + (i * 4));
                }

                BitConverter.GetBytes(0xFFFF_FFF0u).CopyTo(b, PlayerLayout.Resources + 24);
            }));

            Assert.That(player.Resources, Is.EqualTo(new ResourceSet(100, 200, 300, 400, 500, 600, 0xFFFF_FFF0L)));
        }

        [Test]
        public void TheIncomeIsReadFromItsTableAndTheGoldIncomeFromAPlaceOfItsOwn()
        {
            var player = this.deserializer.Deserialize(BlankRecords.Player("Blue", b =>
            {
                for (var i = 0; i < 6; i++)
                {
                    BitConverter.GetBytes((uint)(10 * (i + 1))).CopyTo(b, PlayerLayout.Income + (i * 4));
                }

                BitConverter.GetBytes(1500u).CopyTo(b, PlayerLayout.GoldIncome);
            }));

            Assert.That(player.Income, Is.EqualTo(new ResourceSet(10, 20, 30, 40, 50, 60, 1500)));
        }

        [Test]
        public void TheDaysLeftAndTheTavernAreIdsOrNoneForTheEmptyMark()
        {
            var empty = this.deserializer.Deserialize(BlankRecords.Player("Blue"));
            var set = this.deserializer.Deserialize(BlankRecords.Player("Blue", b =>
            {
                b[PlayerLayout.DaysLeft] = 5;
                b[PlayerLayout.Tavern] = 12;
                b[PlayerLayout.Tavern + 1] = 13;
            }));

            Assert.That((empty.DaysLeft, empty.Tavern), Is.EqualTo(((int?)null, new TavernHeroes(null, null))));
            Assert.That((set.DaysLeft, set.Tavern), Is.EqualTo(((int?)5, new TavernHeroes(12, 13))));
        }
    }
}
