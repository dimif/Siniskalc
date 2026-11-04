using Siniskalc.Homm3.Tests.Data;
namespace Siniskalc.Homm3.Tests.Integration.Data
{
    using Siniskalc.Homm3.Data;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// Black Market stock from the visits in <c>TestData/Hota/VisitedPlaces</c> and the eight identified purchases in
    /// <c>TestData/Hota/MerchantStock</c>. Town Artifact Merchants have separate shared stock.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class MerchantStockTests
    {
        private const string Visited = "27_tazar_artifact_merchant";
        private const string Bought = "29_tazar_black_market_first_4__orrin_artifact_merchant_last_4";

        private static Siniskalc.Homm3.Runtime.Game Load(string name) => SavedGames.Get(Path.Combine("TestData", "Hota", "VisitedPlaces", $"visited_places_{name}.GM2"));

        private static string Names(MerchantStockModel stock) => string.Join(", ", stock.Slots.Select(a => a?.Name ?? "-"));

        private static string ControlPath(string name) => Path.Combine("TestData", "Hota", "MerchantStock", $"merchant_stock_{name}.GM2");

        private static IEnumerable<string> Controls() => Directory.GetFiles(Path.Combine("TestData", "Hota", "MerchantStock"), "*.GM2").Order();

        [TestCase(0, "NecklaceOfSwiftness,Spyglass,PendantOfFreeWill,BucklerOfTheGnollKing,HideousMask,EquestriansGloves,PendantOfCourage")]
        [TestCase(1, "BadgeOfCourage,CrestOfValor,SkullHelmet,HelmOfChaos,CrownOfTheSupremeMagi,LoinsOfLegion,AngelFeatherArrows")]
        [TestCase(2, "DemonsHorseshoe,CollarOfConjuring,ShieldOfTheDwarvenLords,ShamansPuppet,BowstringOfTheUnicornsMane,TorsoOfLegion,RecantersCloak")]
        [TestCase(3, "RingOfSuppression,QuietEyeOfTheDragon,GlyphOfGallantry,EmblemOfCognizance,BlackshardOfTheDeadKnight,RedDragonFlameTongue,OrbOfTempestousFire")]
        [TestCase(4, "PendantOfLife,PendantOfHoliness,PendantOfTotalRecall,RibCage,GreaterKnollsFlail,InexhaustibleCartOfOre,StatesmensMedal")]
        [TestCase(5, "MysticOrbOfSpellPoints,BowOfElvenCherrywood,DragonboneGreaves,ScalesOfTheGreaterBasilisk,ShieldOfTheYawningDead,RingOfLife,NecklaceOfDragonteeth")]
        [TestCase(6, "BirdOfPerception,HelmOfTheAlabasterUnicorn,CentaursAxe,DragonWingTabard,BootsOfSpeed,VampiresCowl,GoldenBow")]
        [TestCase(7, "Speculum,StillEyeOfTheDragon,RingOfVitality,InexhaustibleCartOfLumber,RibCage,ArmorOfWonder,PendantOfSecondSight")]
        public void EachMarketStartsWithItsIdentifiedSevenArtifactsInShopOrder(int market, string artifacts)
        {
            var stock = SavedGames.Get(ControlPath("00_baseline")).History.Current.Merchants().GetModel(market);

            Assert.That(stock.Slots, Is.EqualTo(artifacts.Split(',').Select(id => TestCatalog.Hota.Artifacts.ById(id))));
        }

        [TestCase("00_baseline", "01_red_buys_gloves", 0, 5, "EquestriansGloves")]
        [TestCase("01_red_buys_gloves", "02_red_buys_arrows", 1, 6, "AngelFeatherArrows")]
        [TestCase("02_red_buys_arrows", "03_red_buys_bowstring", 2, 4, "BowstringOfTheUnicornsMane")]
        [TestCase("03_red_buys_bowstring", "04_red_buys_orb", 3, 6, "OrbOfTempestousFire")]
        [TestCase("04_red_buys_orb", "05_red_buys_pendant", 4, 2, "PendantOfTotalRecall")]
        [TestCase("05_red_buys_pendant", "06_red_buys_bow", 5, 1, "BowOfElvenCherrywood")]
        [TestCase("08_blue_turn", "09_blue_buys_boots", 6, 4, "BootsOfSpeed")]
        [TestCase("09_blue_buys_boots", "10_blue_buys_armor", 7, 5, "ArmorOfWonder")]
        public void EachIdentifiedPurchaseClearsOnlyItsSlotInTheEightBlackMarketLists(string before, string after, int market, int slot, string artifact)
        {
            var original = SavedGames.Get(ControlPath(before)).History.Current.Merchants();
            var purchased = SavedGames.Get(ControlPath(after)).History.Current.Merchants();

            Assert.That(original, Has.Count.EqualTo(8));
            Assert.That(purchased, Has.Count.EqualTo(8));
            Assert.That(original.GetModel(market).Slots[slot], Is.EqualTo(TestCatalog.Hota.Artifacts.ById(artifact)));
            Assert.That(purchased.GetModel(market).Slots[slot], Is.Null);
            Assert.That(purchased[market].GetUInt32At(slot * 4), Is.EqualTo(uint.MaxValue));
            for (var index = 0; index < 8; index++)
            {
                var expected = original[index].ToArray();
                if (index == market)
                {
                    BitConverter.GetBytes(uint.MaxValue).CopyTo(expected, slot * 4);
                }

                Assert.That(purchased[index].ToArray(), Is.EqualTo(expected), $"Market {index + 1}");
            }
        }

        [TestCaseSource(nameof(Controls))]
        public void TheEightMarketControlsRoundTripWithoutChangingAnySaveBytes(string path)
        {
            var game = SavedGames.Get(path);
            var merchants = game.History.Current.Merchants();

            Assert.That(merchants, Has.Count.EqualTo(8));
            Assert.That(merchants.Select(c => c.StartIndex), Is.EqualTo(Enumerable.Range(0, 8).Select(i => 0x87E + (28 * i))));
            foreach (var chunk in merchants)
            {
                Assert.That(merchants.Serializer.Serialize(chunk, merchants.Deserializer.Deserialize(chunk)).ToArray(), Is.EqualTo(chunk.ToArray()));
            }

            Assert.That(game.GetBytesToSave(), Is.EqualTo(TestServices.SavefileCodec.Read(path)));
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        [TestCase(6)]
        [TestCase(7)]
        public void EditingAnyOfTheEightMarketsWritesOnlyItsChosenSlotAndCanBeReloadedUndoneAndReverted(int market)
        {
            var path = ControlPath("00_baseline");
            var game = SavedGames.Get(path);
            var original = TestServices.SavefileCodec.Read(path);
            var offset = game.History.Current.Merchants()[market].StartIndex + 12;
            var expected = original.ToArray();
            BitConverter.GetBytes(uint.MaxValue).CopyTo(expected, offset);

            game.History.CreateMerchantSession(market).Update(stock => stock.WithSlot(3, null));
            var written = game.GetBytesToSave();
            Assert.That(written, Is.EqualTo(expected));
            var reloaded = TestServices.GameService.LoadGameFromBytes(written, "merchant_edited.GM2");
            Assert.That(reloaded.History.Current.Merchants().GetModel(market).Slots[3], Is.Null);
            game.History.Undo();
            Assert.That(game.GetBytesToSave(), Is.EqualTo(original));
            game.History.Redo();
            Assert.That(game.GetBytesToSave(), Is.EqualTo(expected));
            game.History.GetPendingChanges().Single().Revert(game.History);
            Assert.That(game.GetBytesToSave(), Is.EqualTo(original));
        }

        [Test]
        public void BuyingFromTheSharedTownStockDoesNotChangeTheBlackMarketsAndTheEmptyTownSlotPersistsForBlue()
        {
            var before = TestServices.SavefileCodec.Read(ControlPath("06_red_buys_bow"));
            var bought = TestServices.SavefileCodec.Read(ControlPath("07_red_buys_town_head_of_legion"));
            var blue = TestServices.SavefileCodec.Read(ControlPath("08_blue_turn"));
            var townCodes = new[] { "StoicWatchman", "LadybirdOfLuck", "BreastplateOfPetrifiedWood", "HelmOfChaos", "RingOfLife", "RedDragonFlameTongue", "HeadOfLegion" }
                .Select(id => (uint)TestCatalog.Hota.Artifacts.ById(id).Code).ToArray();
            var expected = townCodes.SelectMany(BitConverter.GetBytes).ToArray();
            const int townOffset = 0x797A5;

            Assert.That(before.Skip(townOffset).Take(28), Is.EqualTo(expected));
            BitConverter.GetBytes(uint.MaxValue).CopyTo(expected, 24);
            Assert.That(bought.Skip(townOffset).Take(28), Is.EqualTo(expected));
            Assert.That(blue.Skip(townOffset).Take(28), Is.EqualTo(expected));
            Assert.That(bought.Skip(0x87E).Take(8 * 28), Is.EqualTo(before.Skip(0x87E).Take(8 * 28)));
            Assert.That(blue.Skip(0x87E).Take(8 * 28), Is.EqualTo(before.Skip(0x87E).Take(8 * 28)));
        }

        [Test]
        public void TheStockOfBothBlackMarketsIsReadFromTheVisitedPlacesSavegame()
        {
            var merchants = Load(Visited).History.Current.Merchants();

            Assert.That(merchants, Has.Count.EqualTo(2));
            Assert.That(Names(merchants.GetModel(0)), Is.EqualTo("Bow of Elven Cherrywood, Pendant of Total Recall, Clover of Fortune, Inexhaustible Cart of Ore, Scales of the Greater Basilisk, Vampire's Cowl, Everflowing Crystal Cloak"));
            Assert.That(merchants.GetModel(1).Slots.Select(a => a!.Code), Is.EqualTo(new byte[] { 0x49, 0x4B, 0x55, 0x62, 0x3D, 0x15, 0x96 }));
            Assert.That(merchants.GetModel(1).Slots[3], Is.EqualTo(TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("BootsOfSpeed")));
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

            game.History.CreateMerchantSession(1).Update(stock => stock.WithSlot(2, TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("TitansThunder")).WithSlot(6, null));

            var saved = TestServices.SavefileCodec.Read(Path.Combine("TestData", "Hota", "VisitedPlaces", $"visited_places_{Visited}.GM2"));
            var written = game.GetBytesToSave();
            var changed = Enumerable.Range(0, saved.Length).Where(i => saved[i] != written[i]).Select(i => i - chunk.StartIndex).ToList();
            Assert.That(BitConverter.ToUInt32(written, chunk.StartIndex + 8), Is.EqualTo((uint)TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("TitansThunder").Code));
            Assert.That(BitConverter.ToUInt32(written, chunk.StartIndex + 24), Is.EqualTo(0xFFFFFFFFu));
            Assert.That(changed.All(i => i >= 8 && i < 28), Is.True);
            Assert.That(game.History.GetPendingChanges().Select(c => c.Change.Description), Is.EquivalentTo(new[] { "Slot 3", "Slot 7" }));

            foreach (var change in game.History.GetPendingChanges().ToList())
            {
                change.Revert(game.History);
            }

            Assert.That(game.History.Current.Merchants().GetModel(1).Slots[2], Is.EqualTo(TestCatalog.For(GameVersion.HornOfTheAbyss).WearableArtifacts.ToDictionary(a => a.Code)[0x55]));
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
