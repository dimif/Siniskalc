using Siniskalc.Homm3.Tests.Data;
namespace Siniskalc.Homm3.Tests.Integration.Data
{
    using Siniskalc.Homm3.Data;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// What the artifact merchants and the black market have in store, from the saves of <c>TestData/Hota/VisitedPlaces</c> in which Tazar and
    /// Orrin visited them and bought from them.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class MerchantStockTests
    {
        private const string Visited = "27_tazar_artifact_merchant";
        private const string Bought = "29_tazar_black_market_first_4__orrin_artifact_merchant_last_4";

        private static Siniskalc.Homm3.Runtime.Game Load(string name) => SavedGames.Get(Path.Combine("TestData", "Hota", "VisitedPlaces", $"visited_places_{name}.GM2"));

        private static string Names(MerchantStockModel stock) => string.Join(", ", stock.Slots.Select(a => a?.Name ?? "-"));

        [Test]
        public void TheStockOfTheArtifactMerchantsAndOfTheBlackMarketIsReadFromTheSavegame()
        {
            var merchants = Load(Visited).History.Current.Merchants();

            Assert.That(merchants, Has.Count.EqualTo(2));
            Assert.That(Names(merchants.GetModel(0)), Is.EqualTo("Bow of Elven Cherrywood, Pendant of Total Recall, Clover of Fortune, Inexhaustible Cart of Ore, Scales of the Greater Basilisk, Vampire's Cowl, Everflowing Crystal Cloak"));
            Assert.That(merchants.GetModel(1).Slots.Select(a => a!.Code), Is.EqualTo(new byte[] { 0x49, 0x4B, 0x55, 0x62, 0x3D, 0x15, 0x96 }));
            Assert.That(merchants.GetModel(1).Slots[3], Is.EqualTo(Artifact.BootsOfSpeed));
        }

        [Test]
        public void ABoughtArtifactLeavesItsSlotEmpty()
        {
            var merchants = Load(Bought).History.Current.Merchants();

            Assert.That(merchants.GetModel(0).Slots.Select(a => a is null), Is.EqualTo(new[] { false, false, false, true, true, true, true }));
            Assert.That(merchants.GetModel(1).Slots.Select(a => a is null), Is.EqualTo(new[] { true, true, true, true, false, false, false }));
        }

        [Test]
        public void PuttingAnotherArtifactInASlotWritesThatNumberAndNothingElseAndCanBePutBack()
        {
            var game = Load(Visited);
            var chunk = game.History.Current.Merchants()[1];

            game.History.CreateMerchantSession(1).Update(stock => stock.WithSlot(2, Artifact.TitansThunder).WithSlot(6, null));

            var saved = TestServices.SavefileCodec.Read(Path.Combine("TestData", "Hota", "VisitedPlaces", $"visited_places_{Visited}.GM2"));
            var written = game.GetBytesToSave();
            var changed = Enumerable.Range(0, saved.Length).Where(i => saved[i] != written[i]).Select(i => i - chunk.StartIndex).ToList();
            Assert.That(BitConverter.ToUInt32(written, chunk.StartIndex + 8), Is.EqualTo((uint)Artifact.TitansThunder.Code));
            Assert.That(BitConverter.ToUInt32(written, chunk.StartIndex + 24), Is.EqualTo(0xFFFFFFFFu));
            Assert.That(changed.All(i => i >= 8 && i < 28), Is.True);
            Assert.That(game.History.GetPendingChanges().Select(c => c.Change.Description), Is.EquivalentTo(new[] { "Slot 3", "Slot 7" }));

            foreach (var change in game.History.GetPendingChanges().ToList())
            {
                change.Revert(game.History);
            }

            Assert.That(game.History.Current.Merchants().GetModel(1).Slots[2], Is.EqualTo(Artifact.All[0x55]));
        }

        [Test]
        public void ASlotWithANumberTheEditorDoesNotKnowIsLeftAloneWhileTheModelIsNotChanged()
        {
            var game = Load(Visited);
            var chunk = game.History.Current.Merchants()[0];
            var bytes = chunk.ToArray();
            BitConverter.GetBytes(0xFEu).CopyTo(bytes, 0);
            var odd = chunk.WithBytes(bytes);

            var again = game.History.Current.Merchants().Serializer.Serialize(odd, game.History.Current.Merchants().Deserializer.Deserialize(odd));

            Assert.That(again.HasSameBytes(odd), Is.True);
        }

        [Test]
        public void EveryMerchantOfEverySaveOfTheSeriesIsWrittenBackAsTheSameBytes()
        {
            foreach (var file in Directory.GetFiles(Path.Combine("TestData", "Hota", "VisitedPlaces"), "*.GM2"))
            {
                var merchants = SavedGames.Get(file).History.Current.Merchants();
                foreach (var chunk in merchants)
                {
                    Assert.That(merchants.Serializer.Serialize(chunk, merchants.Deserializer.Deserialize(chunk)).HasSameBytes(chunk), Is.True, $"{file}: {chunk.Name}");
                }
            }
        }
    }
}
