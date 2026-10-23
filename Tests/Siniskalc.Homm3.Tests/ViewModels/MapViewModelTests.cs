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

            Assert.That(page.Merchants.Select(m => m.Name), Is.EqualTo(new[] { "Artifact merchants", "Black market" }));
            Assert.That(page.Merchants[0].Slots.Select(s => s.ArtifactName), Is.EqualTo(new[] { "Bow of Elven Cherrywood", "Pendant of Total Recall", "Clover of Fortune", "Inexhaustible Cart of Ore", "Scales of the Greater Basilisk", "Vampire's Cowl", "Everflowing Crystal Cloak" }));

            page.Merchants[1].Slots[0].ClearCommand.Execute(null);

            Assert.That(page.Merchants[1].Slots[0].HasArtifact, Is.False);
            Assert.That(game.History.Current.Merchants().GetModel(1).Slots[0], Is.Null);
            Assert.That(page.HasPendingChanges, Is.True);

            page.ResetCommand.Execute(null);

            Assert.That(page.Merchants[1].Slots[0].ArtifactName, Is.EqualTo(Siniskalc.Homm3.Entities.Artifact.All[0x49].Name));
            Assert.That(page.HasPendingChanges, Is.False);
        }
    }
}
