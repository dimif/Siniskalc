using Siniskalc.Core.Data;
using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Tests.Data;

namespace Siniskalc.Homm3.Tests.Integration.Data
{
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class TownMerchantStockLocatorTests
    {
        private static byte[] Block(uint code = uint.MaxValue)
        {
            var bytes = Enumerable.Repeat((byte)0xCC, 130).ToArray();
            new byte[] { 0x7F, 0, 2, 0, 1, 0, 1, 0, 1, 0 }.CopyTo(bytes, 60);
            Array.Clear(bytes, 70, 32);
            for (var slot = 0; slot < 7; slot++)
            {
                BitConverter.GetBytes(code).CopyTo(bytes, 102 + (4 * slot));
            }

            return bytes;
        }

        [TestCase(GameVersion.HornOfTheAbyss)]
        [TestCase(GameVersion.ShadowOfDeath)]
        public void EmptyAndUnknownStockValuesDoNotPreventStructuralDiscovery(GameVersion version)
        {
            foreach (var code in new[] { uint.MaxValue, 0x12345678u, 0u })
            {
                Assert.That(TestServices.TownMerchantStockLocator.FindOffset(Block(code), version), Is.EqualTo(102));
            }
        }

        [TestCase(GameVersion.Unknown)]
        [TestCase(GameVersion.RestorationOfErathia)]
        [TestCase(GameVersion.ArmageddonsBlade)]
        [TestCase(GameVersion.HeroesChronicles)]
        public void VersionsWithoutAnEstablishedLayoutAreNotGuessed(GameVersion version)
        {
            Assert.That(TestServices.TownMerchantStockLocator.FindOffset(Block(), version), Is.EqualTo(-1));
        }

        [TestCase(60)]
        [TestCase(61)]
        [TestCase(62)]
        [TestCase(63)]
        [TestCase(64)]
        [TestCase(66)]
        [TestCase(69)]
        [TestCase(70)]
        public void ADamagedDateOrPrefixOrZeroBlockDoesNotBecomeAStockMatch(int index)
        {
            var bytes = Block();
            bytes[index] = 0xEE;

            Assert.That(TestServices.TownMerchantStockLocator.FindOffset(bytes, GameVersion.HornOfTheAbyss), Is.EqualTo(-1));
        }

        [Test]
        public void AnUnmarkedDateIsNotUsedEvenWhenItHasSevenPlausibleArtifactsAfterIt()
        {
            var bytes = Block(19);
            bytes[60] = 0;

            Assert.That(TestServices.TownMerchantStockLocator.FindOffset(bytes, GameVersion.HornOfTheAbyss), Is.EqualTo(-1));
            var withMarkedBlock = bytes.Concat(Block()).ToArray();
            Assert.That(TestServices.TownMerchantStockLocator.FindOffset(withMarkedBlock, GameVersion.HornOfTheAbyss), Is.EqualTo(232));
        }

        [Test]
        public void CompetingMarkedDatesAreRefusedInsteadOfPickingTheFirst()
        {
            var bytes = Block().Concat(Block()).ToArray();

            Assert.That(TestServices.TownMerchantStockLocator.FindOffset(bytes, GameVersion.HornOfTheAbyss), Is.EqualTo(-1));
            Assert.That(TestServices.TownMerchantStockLocator.FindOffset(bytes, GameVersion.HornOfTheAbyss, 130), Is.EqualTo(232));
        }

        [Test]
        public void TruncatedBlocksAndSearchesAfterTheDateGiveNoStock()
        {
            foreach (var length in new[] { 0, 63, 69, 101, 102, 129 })
            {
                Assert.That(TestServices.TownMerchantStockLocator.FindOffset(Block().Take(length).ToArray(), GameVersion.HornOfTheAbyss), Is.EqualTo(-1));
            }

            Assert.That(TestServices.TownMerchantStockLocator.FindOffset(Block(), GameVersion.HornOfTheAbyss, 65), Is.EqualTo(-1));
        }

        [Test]
        public void BothPrefixFlagsAndTheFullSupportedMonthRangeAreAccepted()
        {
            var bytes = Block();
            bytes[61] = 1;
            bytes[63] = 1;
            bytes[68] = 255;

            Assert.That(TestServices.TownMerchantStockLocator.FindOffset(bytes, GameVersion.HornOfTheAbyss), Is.EqualTo(102));
        }

        [Test]
        public void MovingTheBlockAndChangingEveryStockValueDoesNotChangeHowItIsLocated()
        {
            var prefix = Enumerable.Repeat((byte)0xDD, 347).ToArray();
            var bytes = prefix.Concat(Block(0x12345678)).ToArray();

            Assert.That(TestServices.TownMerchantStockLocator.FindOffset(bytes, GameVersion.HornOfTheAbyss, prefix.Length), Is.EqualTo(449));
            Array.Fill(bytes, (byte)0xFF, 449, 28);
            Assert.That(TestServices.TownMerchantStockLocator.FindOffset(bytes, GameVersion.HornOfTheAbyss, prefix.Length), Is.EqualTo(449));
        }

        [TestCase("00_baseline", 0x797AD, false)]
        [TestCase("01_red_buys_gloves", 0x797A5, false)]
        [TestCase("02_red_buys_arrows", 0x797A5, false)]
        [TestCase("03_red_buys_bowstring", 0x797A5, false)]
        [TestCase("04_red_buys_orb", 0x797A5, false)]
        [TestCase("05_red_buys_pendant", 0x797A5, false)]
        [TestCase("06_red_buys_bow", 0x797A5, false)]
        [TestCase("07_red_buys_town_head_of_legion", 0x797A5, true)]
        [TestCase("08_blue_turn", 0x797A5, true)]
        [TestCase("09_blue_buys_boots", 0x7979D, true)]
        [TestCase("10_blue_buys_armor", 0x7979D, true)]
        public void EachPurchaseControlLocatesTheObservedSharedTownStockDespiteEarlierDataShifting(string save, int expected, bool bought)
        {
            var path = Path.Combine("TestData", "Hota", "MerchantStock", $"merchant_stock_{save}.GM2");
            var game = SavedGames.Get(path);
            var offset = Locate(game.Bytes, game.GameVersion, game.History.Current.Players().Max(p => p.End));
            Assert.That(offset, Is.EqualTo(expected));
            var chunk = new RecordChunk("Town Artifact Merchants", game.Bytes.AsSpan(offset, 28).ToArray(), offset, 0);
            var reader = new MerchantStockDeserializer(TestCatalog.Hota);
            var stock = reader.Deserialize(chunk);
            var expectedCodes = new uint[] { 64, 48, 25, 21, 95, 38, bought ? uint.MaxValue : 122 };
            Assert.That(stock.Slots.Select(a => a == null ? uint.MaxValue : (uint)a.Code), Is.EqualTo(expectedCodes));
            Assert.That(new SafeMerchantStockSerializer(TestCatalog.Hota).Serialize(chunk, stock).ToArray(), Is.EqualTo(chunk.ToArray()));
        }

        [Test]
        public void TheFullCorpusHasOneRecognizedStockBlockExceptForTheFiveAlternateHotADatePrefixes()
        {
            var located = 0;
            var rejected = new List<string>();
            foreach (var path in Directory.GetFiles("TestData", "*.GM?", SearchOption.AllDirectories))
            {
                var game = SavedGames.Get(path);
                var afterPlayers = game.History.Current.Players().Max(p => p.End);
                var offset = Locate(game.Bytes, game.GameVersion, afterPlayers);
                if (offset < 0)
                {
                    rejected.Add(Path.GetFileNameWithoutExtension(path));
                    continue;
                }

                located++;
                var artifacts = game.History.Current.Catalog().Artifacts;
                for (var slot = 0; slot < 7; slot++)
                {
                    var code = BitConverter.ToUInt32(game.Bytes, offset + (4 * slot));
                    Assert.That(code == uint.MaxValue || (code <= byte.MaxValue && artifacts.Find((byte)code) is { Wearable: true }), Is.True, $"{path}: slot {slot + 1} ({code})");
                }
            }

            Assert.That(located, Is.GreaterThanOrEqualTo(488));
            Assert.That(rejected, Is.EquivalentTo(new[]
            {
                "empire_of_the_world_01_m5w4d6",
                "empire_of_the_world_02_m5w4d6_purple_gifts_red",
                "new_day_01_m5w4d7",
                "new_day_02_m6w1d1",
                "new_day_03_m6w1d2",
            }));
        }

        private static int Locate(byte[] bytes, GameVersion version, int searchFrom) =>
            TestServices.TownMerchantStockLocator.FindOffset(bytes, version, searchFrom);
    }
}
