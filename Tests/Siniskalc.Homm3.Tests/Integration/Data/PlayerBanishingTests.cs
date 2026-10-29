using Siniskalc.Homm3.Tests.Data;
using Siniskalc.Heroes.Models;
using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Models;
using Siniskalc.Homm3.Runtime;

namespace Siniskalc.Homm3.Tests.Integration.Data
{
    /// <summary>
    /// The saves in TestData/Hota/Banishing: Blue loses his last town (7 days left), regains one, loses it again, and is banished after
    /// the 7 days. Byte 22 of the player struct holds the days left: FF while the player owns a town, then 7 down to 1, and 0 once banished.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class PlayerBanishingTests
    {
        private static Game Load(string save) =>
            TestServices.GameService.LoadGameFromFile(Directory.GetFiles(Path.Combine("TestData", "Hota", "Banishing"), save + ".GM?").Single());

        private static PlayerChunk BlueChunk(Game game) => game.History.Current.Players()[game.History.Current.Players().FindByColor(PlayerColor.Blue)];

        private static PlayerModel Blue(Game game) => TestServices.PlayerDeserializer.Deserialize(BlueChunk(game));

        [TestCase("banishing_00_baseline", null)]
        [TestCase("banishing_01_blue_7_days_left", 7)]
        [TestCase("banishing_02_m1w1d1", null)]
        [TestCase("banishing_03_blue_with_town", null)]
        [TestCase("banishing_04_m1w1d2", null)]
        [TestCase("banishing_05_m1w1d3", 7)]
        [TestCase("banishing_06_blue_6_days_left", 6)]
        [TestCase("banishing_07_m1w1d4", 6)]
        [TestCase("banishing_08_m1w1d5", 5)]
        [TestCase("banishing_09_m1w1d6", 4)]
        [TestCase("banishing_10_m1w1d7", 3)]
        [TestCase("banishing_11_m1w2d1", 2)]
        [TestCase("banishing_12_m1w2d2", 1)]
        [TestCase("banishing_13_blue_banished", 0)]
        public void TheDaysBlueHasLeftAreReadFromEachSave(string save, int? daysLeft)
        {
            Assert.That(Blue(Load(save)).DaysLeft, Is.EqualTo(daysLeft));
        }

        [TestCase("banishing_00_baseline", 1)]
        [TestCase("banishing_01_blue_7_days_left", 0)]
        [TestCase("banishing_03_blue_with_town", 1)]
        [TestCase("banishing_13_blue_banished", 0)]
        public void ABluePlayerWithNoTownCountsDownAndOneWithATownDoesNot(string save, int towns)
        {
            var blue = BlueChunk(Load(save));

            Assert.That(blue.GetByteAt(PlayerLayout.TownCount), Is.EqualTo(towns));
            Assert.That(Blue(Load(save)).DaysLeft is null, Is.EqualTo(towns > 0));
        }

        [Test]
        public void NoOtherPlayerHasAnyDaysLeftInAnySave()
        {
            foreach (var path in Directory.GetFiles(Path.Combine("TestData", "Hota", "Banishing")))
            {
                var game = ReadOnlySaves.Load(path);
                foreach (var chunk in game.History.Current.Players().Where(c => c.Color != PlayerColor.Blue))
                {
                    Assert.That(TestServices.PlayerDeserializer.Deserialize(chunk).DaysLeft, Is.Null, $"{Path.GetFileName(path)} {chunk.Color}");
                }
            }
        }

        [Test]
        public void TheDaysInTheNamesOfTheAutosavesAreTheDaysOfTheGame()
        {
            foreach (var (save, month, week, day) in new[] { ("banishing_02_m1w1d1", 1, 1, 1), ("banishing_05_m1w1d3", 1, 1, 3), ("banishing_10_m1w1d7", 1, 1, 7), ("banishing_11_m1w2d1", 1, 2, 1), ("banishing_12_m1w2d2", 1, 2, 2) })
            {
                var date = Load(save).Date!.Value;

                Assert.That((date.Month, date.Week, date.Day), Is.EqualTo((month, week, day)), save);
            }
        }

        [Test]
        public void ReadingAndWritingAPlayerBackLeavesItsBytesAsTheyWere()
        {
            var serializer = new SafePlayerSerializer();
            foreach (var path in Directory.GetFiles(Path.Combine("TestData", "Hota", "Banishing")))
            {
                foreach (var chunk in ReadOnlySaves.Load(path).History.Current.Players())
                {
                    Assert.That(serializer.Serialize(chunk, TestServices.PlayerDeserializer.Deserialize(chunk)).ToArray(), Is.EqualTo(chunk.ToArray()), $"{Path.GetFileName(path)} {chunk.Color}");
                }
            }
        }

        [Test]
        public void ChangingTheDaysLeftChangesOnlyThatByte()
        {
            var chunk = BlueChunk(Load("banishing_01_blue_7_days_left"));

            var written = new SafePlayerSerializer().Serialize(chunk, TestServices.PlayerDeserializer.Deserialize(chunk).WithDaysLeft(3));

            var changed = Enumerable.Range(0, chunk.ToArray().Length).Where(i => written.ToArray()[i] != chunk.ToArray()[i]).ToList();
            Assert.That(changed, Is.EqualTo(new[] { PlayerLayout.DaysLeft }));
            Assert.That(written.GetByteAt(PlayerLayout.DaysLeft), Is.EqualTo(3));
        }

        [Test]
        public void ABanishedPlayerStaysBanished()
        {
            var banished = Blue(Load("banishing_13_blue_banished"));

            Assert.That(banished.IsBanished, Is.True);
            Assert.That(banished.HasCountdown, Is.False);
            Assert.That(banished.WithDaysLeft(5), Is.SameAs(banished));
        }

        [Test]
        public void AByteThatIsNoCountOfDaysReadsAsNoneAndIsLeftAloneWhileTheModelHasNone()
        {
            var chunk = BlueChunk(Load("banishing_03_blue_with_town"));
            var bytes = chunk.ToArray();
            bytes[PlayerLayout.DaysLeft] = 0x40;
            var odd = chunk.WithBytes(bytes);
            var serializer = new SafePlayerSerializer();

            var model = TestServices.PlayerDeserializer.Deserialize(odd);

            Assert.That(model.DaysLeft, Is.Null);
            Assert.That(serializer.Serialize(odd, model).GetByteAt(PlayerLayout.DaysLeft), Is.EqualTo(0x40));
            Assert.That(serializer.Serialize(odd, model with { DaysLeft = 4 }).GetByteAt(PlayerLayout.DaysLeft), Is.EqualTo(4));
        }

        [Test]
        public void GivingBackNoDaysWritesFFOverACountdown()
        {
            var chunk = BlueChunk(Load("banishing_01_blue_7_days_left"));

            var written = new SafePlayerSerializer().Serialize(chunk, TestServices.PlayerDeserializer.Deserialize(chunk) with { DaysLeft = null });

            Assert.That(written.GetByteAt(PlayerLayout.DaysLeft), Is.EqualTo(0xFF));
        }
    }
}
