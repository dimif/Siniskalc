namespace Siniskalc.Homm3.Tests.ViewModels
{
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Homm3.Data;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.ViewModels;

    /// <summary>
    /// The page of the mines of the map (<c>TestData/Hota/Mines</c>).
    /// </summary>
    [TestFixture]
    internal sealed class MapViewModelTests
    {
        private static Siniskalc.Homm3.Runtime.Game Load(string name) => TestServices.GameService.LoadGameFromFile(Path.Combine("TestData", "Hota", "Mines", name + ".GM2"));

        [Test]
        public void ThePageListsTheMinesWithTheirOwnerAndSaysHowManyAreOwned()
        {
            var page = new MapViewModel(Load("mines_08_red_abandoned_mine_sulfur"), TestServices.EffectLines);

            Assert.That(page.Mines, Has.Count.EqualTo(18));
            Assert.That(page.MinesSummary, Is.EqualTo("18 mines, 8 of them owned"));
            Assert.That(page.Mines.Select(m => m.ResourceName), Does.Contain("Sulfur").And.Contain("Mercury").And.Contain("Wood"));
            Assert.That(page.Mines.Single(m => m.PositionText == "X: 51, Y: 33").OwnerField.Selected, Is.EqualTo("Red"));
        }

        [Test]
        public void PickingAnOwnerChangesTheMineAndTheCountAndCanBeReset()
        {
            var game = Load("mines_00_game_begin");
            var page = new MapViewModel(game, TestServices.EffectLines);

            page.Mines.Single(m => m.PositionText == "X: 26, Y: 17").OwnerField.Selected = "Green";

            Assert.That(game.History.Current.Mines().GetModels().Count(m => m.Owner == PlayerColor.Green), Is.EqualTo(1));
            Assert.That(page.MinesSummary, Is.EqualTo("18 mines, 1 of them owned"));
            Assert.That(page.HasPendingChanges, Is.True);

            page.ResetCommand.Execute(null);

            Assert.That(page.MinesSummary, Is.EqualTo("18 mines, 0 of them owned"));
            Assert.That(page.HasPendingChanges, Is.False);
        }

        [Test]
        public void TheMinesAreSortedByResourceOwnerOrLocationFromTheHeadings()
        {
            var page = new MapViewModel(Load("mines_08_red_abandoned_mine_sulfur"), TestServices.EffectLines);

            page.MineList.Columns["X"].SortCommand.Execute(null);
            Assert.That(page.Mines.Select(m => m.X), Is.Ordered);

            page.MineList.Columns["Y"].SortCommand.Execute(null);
            Assert.That(page.Mines.Select(m => m.Y), Is.Ordered);

            page.MineList.Columns["Resource"].SortCommand.Execute(null);
            Assert.That(page.Mines.Select(m => m.ResourceName), Is.Ordered);

            page.MineList.Columns["Owner"].SortCommand.Execute(null);
            Assert.That(page.Mines.Select(m => m.OwnerField.Selected), Is.Ordered);
        }

        [Test]
        public void ThePickedMineShowsItsGuardInTheArmyEditorAndEditingItChangesTheMine()
        {
            var game = Load("mines_00_game_begin");
            var page = new MapViewModel(game, TestServices.EffectLines);
            var guarded = page.Mines.Single(m => m.PositionText == "X: 1, Y: 14");

            page.SelectedMine = guarded;

            Assert.That(guarded.Summary, Is.EqualTo("1 of 7 slots"));
            Assert.That(page.GuardEditor, Is.SameAs(guarded.GuardEditor));
            Assert.That(page.GuardHeading, Does.Contain("X: 1, Y: 14"));
            Assert.That(page.GuardEditor!.CreatureSlots[0].HasCreature, Is.True);

            page.GuardEditor.CreatureSlots[0].ClearCommand.Execute(null);

            Assert.That(guarded.Summary, Is.EqualTo("Unguarded"));
            Assert.That(game.History.GetPendingChanges().Single().Change.Area, Is.EqualTo(Siniskalc.Heroes.Models.ChangeAreas.Garrison));
        }

        private static Siniskalc.Homm3.Runtime.Game LoadDwellings(string name) => TestServices.GameService.LoadGameFromFile(Path.Combine("TestData", "Hota", "Dwellings", $"dwellings_{name}.GM2"));

        [Test]
        public void ThePageListsTheDwellingsWithTheirCreatureOwnerAndTheCreaturesToHire()
        {
            var page = new MapViewModel(LoadDwellings("05_tazar_flags_portal_of_glory"), TestServices.EffectLines);

            Assert.That(page.Dwellings, Has.Count.EqualTo(31));
            var guardhouse = page.Dwellings.Single(d => d.PositionText == "X: 31, Y: 56");
            Assert.That(guardhouse.CreatureName, Is.EqualTo("Pikeman"));
            Assert.That(guardhouse.OwnerField.Selected, Is.EqualTo("Blue"));
            Assert.That(guardhouse.StockText, Is.EqualTo("28"));
            Assert.That(page.MapSummary, Does.Contain("31 dwellings, 7 of them owned"));
        }

        [Test]
        public void TypingTheCreaturesToHireAndPickingAnOwnerChangeTheDwellingAndCanBeReset()
        {
            var game = LoadDwellings("05_tazar_flags_portal_of_glory");
            var page = new MapViewModel(game, TestServices.EffectLines);
            var guardhouse = page.Dwellings.Single(d => d.PositionText == "X: 31, Y: 56");

            guardhouse.StockText = "3";
            guardhouse.OwnerField.Selected = "Green";

            var model = game.History.Current.Dwellings().GetModels().Single(d => d.Position.X == 31 && d.Position.Y == 56);
            Assert.That((model.Stock, model.Owner), Is.EqualTo((3L, PlayerColor.Green)));
            Assert.That(page.HasPendingChanges, Is.True);

            page.ResetCommand.Execute(null);

            Assert.That(guardhouse.StockText, Is.EqualTo("28"));
            Assert.That(guardhouse.OwnerField.Selected, Is.EqualTo("Blue"));
            Assert.That(page.HasPendingChanges, Is.False);
        }

        [Test]
        public void AStockThatIsNotANumberLeavesTheDwellingAloneAndOneThatIsTooBigIsTakenAsTheMost()
        {
            var game = LoadDwellings("05_tazar_flags_portal_of_glory");
            var guardhouse = new MapViewModel(game, TestServices.EffectLines).Dwellings.Single(d => d.PositionText == "X: 31, Y: 56");

            guardhouse.StockText = "many";
            Assert.That(guardhouse.StockText, Is.EqualTo("28"));

            guardhouse.StockText = "99999999999";
            Assert.That(guardhouse.Stock, Is.EqualTo(Siniskalc.Homm3.Constants.Army.MaxStackSize));
        }

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
