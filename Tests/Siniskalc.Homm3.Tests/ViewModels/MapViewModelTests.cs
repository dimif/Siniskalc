namespace Siniskalc.Homm3.Tests.ViewModels
{
    using Siniskalc.Homm3.Data;
    using Siniskalc.Homm3.ViewModels;

    /// <summary>
    /// The page of the map: what the savegame keeps about its merchants.
    /// </summary>
    [TestFixture]
    internal sealed class MapViewModelTests
    {
        [Test]
        public void TheMerchantsTabHasASlotPickerForEachOfTheSevenArtifactsOfEachMerchantAndClearingOneMarksItSold()
        {
            var game = TestServices.GameService.LoadGameFromFile(Path.Combine("TestData", "Hota", "VisitedPlaces", "visited_places_27_tazar_artifact_merchant.GM2"));
            var page = new MapViewModel(game, TestServices.EffectLines);

            Assert.That(page.Merchants.Select(m => m.Name), Is.EqualTo(new[] { "Black market 1", "Black market 2" }));
            Assert.That(page.Merchants[0].Slots.Select(s => s.ArtifactName), Is.EqualTo(new[] { "Bow of Elven Cherrywood", "Pendant of Total Recall", "Clover of Fortune", "Inexhaustible Cart of Ore", "Scales of the Greater Basilisk", "Vampire's Cowl", "Everflowing Crystal Cloak" }));

            page.Merchants[1].Slots[0].ClearCommand.Execute(null);

            Assert.That(page.Merchants[1].Slots[0].HasArtifact, Is.False);
            Assert.That(game.History.Current.Merchants().GetModel(1).Slots[0], Is.Null);
            Assert.That(page.HasPendingChanges, Is.True);

            page.ResetCommand.Execute(null);

            Assert.That(page.Merchants[1].Slots[0].ArtifactName, Is.EqualTo(Siniskalc.Homm3.Entities.Artifact.All[0x49].Name));
            Assert.That(page.HasPendingChanges, Is.False);
        }

        [Test]
        public void AMapWithFourBlackMarketsShowsTheStockAndTheTileOfEachAndEditsOnlyTheOnePicked()
        {
            var game = TestServices.GameService.LoadGameFromFile(Path.Combine("TestData", "Hota", "multi_merchants.GM1"));
            var page = new MapViewModel(game, TestServices.EffectLines);

            Assert.That(page.Merchants.Select(m => m.Name), Is.EqualTo(new[] { "Black market 1", "Black market 2", "Black market 3", "Black market 4" }));
            Assert.That(page.Merchants.Select(m => m.Location), Is.EqualTo(new[] { "At 28, 34", "At 28, 37", "At 31, 34", "At 31, 37" }));
            Assert.That(page.Merchants.Select(m => string.Join(",", m.Slots.Select(s => s.ArtifactName))).Distinct().Count(), Is.EqualTo(4));
            var before = game.History.Current.Merchants().GetModels().Select(m => m.Slots.ToList()).ToList();

            page.Merchants[2].Slots[0].ClearCommand.Execute(null);

            var after = game.History.Current.Merchants().GetModels();
            Assert.That(after[2].Slots[0], Is.Null);
            Assert.That(after.Select((m, i) => i == 2 ? m.Slots.Skip(1).SequenceEqual(before[i].Skip(1)) : m.Slots.SequenceEqual(before[i])), Has.All.True);
        }
    }
}
