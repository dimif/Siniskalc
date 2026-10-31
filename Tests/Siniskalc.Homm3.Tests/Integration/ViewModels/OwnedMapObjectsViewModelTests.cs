using Siniskalc.Homm3.Tests.Data;

namespace Siniskalc.Homm3.Tests.Integration.ViewModels
{
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Homm3.Data;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.ViewModels;

    /// <summary>
    /// The mines and dwellings one owner has on the map (<c>TestData/Hota/Mines</c> and <c>TestData/Hota/Dwellings</c>).
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class OwnedMapObjectsViewModelTests
    {
        private static Siniskalc.Homm3.Runtime.Game Load(string name) => SavedGames.Get(Path.Combine("TestData", "Hota", "Mines", name + ".GM2"));

        private static Siniskalc.Homm3.Runtime.Game LoadDwellings(string name) => SavedGames.Get(Path.Combine("TestData", "Hota", "Dwellings", $"dwellings_{name}.GM2"));

        private static OwnedMapObjectsViewModel Of(Siniskalc.Homm3.Runtime.Game game, PlayerColor owner) => new(game.History, owner, TestServices.ByteEditTargets);

        private static IEnumerable<PlayerColor> Colors() => PlayerColor.All.Append(PlayerColor.Neutral).Distinct();

        [Test]
        public void AnOwnerListsItsMinesWithTheirResourceAndPlace()
        {
            var game = Load("mines_08_red_abandoned_mine_sulfur");

            var red = Of(game, PlayerColor.Red);
            var neutral = Of(game, PlayerColor.Neutral);

            Assert.That(red.Mines.Rows.Single(m => m.PositionText == "Pos: 51, 33, Surface").OwnerField.Selected, Is.EqualTo("Red"));
            Assert.That(neutral.Mines.Rows, Has.Count.EqualTo(10));
            Assert.That(neutral.Mines.Summary, Is.EqualTo("10 mines, 0 of them owned"));
            Assert.That(Colors().Sum(c => Of(game, c).Mines.Rows.Count), Is.EqualTo(18), "every mine is in the list of exactly one owner");
        }

        [Test]
        public void PickingAnOwnerMovesTheMineToTheListOfTheNewOwnerAndAChangeCanBeReverted()
        {
            var game = Load("mines_00_game_begin");
            var neutral = Of(game, PlayerColor.Neutral);
            var green = Of(game, PlayerColor.Green);
            Assert.That(neutral.Mines.Rows, Has.Count.EqualTo(18));

            neutral.Mines.Rows.Single(m => m.PositionText == "Pos: 26, 17, Surface").OwnerField.Selected = "Green";
            neutral.Refresh();
            green.Refresh();

            Assert.That(game.History.Current.Mines().GetModels().Count(m => m.Owner == PlayerColor.Green), Is.EqualTo(1));
            Assert.That(neutral.Mines.Rows, Has.Count.EqualTo(17));
            Assert.That(green.Mines.Rows.Select(m => m.PositionText), Is.EqualTo(new[] { "Pos: 26, 17, Surface" }));
            Assert.That(game.History.GetPendingChanges(), Has.Count.EqualTo(1));

            foreach (var change in game.History.GetPendingChanges().ToList())
            {
                change.Revert(game.History);
            }

            neutral.Refresh();
            green.Refresh();
            Assert.That(neutral.Mines.Rows, Has.Count.EqualTo(18));
            Assert.That(green.Mines.HasMines, Is.False);
        }

        [Test]
        public void ThePickedMineShowsItsGuardInTheArmyEditorAndEditingItChangesTheMine()
        {
            var game = Load("mines_00_game_begin");
            var page = Of(game, PlayerColor.Neutral);
            var guarded = page.Mines.Rows.Single(m => m.PositionText == "Pos: 1, 14, Surface");

            page.Mines.SelectedMine = guarded;

            Assert.That(guarded.Summary, Is.EqualTo("1 of 7 slots"));
            Assert.That(page.GuardEditor, Is.Not.Null);
            Assert.That(page.GuardHeading, Does.Contain("Pos: 1, 14"));
            Assert.That(page.GuardEditor!.CreatureSlots[0].HasCreature, Is.True);

            page.GuardEditor.CreatureSlots[0].ClearCommand.Execute(null);

            Assert.That(guarded.Summary, Is.EqualTo("Unguarded"));
            Assert.That(game.History.GetPendingChanges().Single().Change.Area, Is.EqualTo(Siniskalc.Heroes.Models.ChangeAreas.Garrison));
        }

        [Test]
        public void TwoMinesOnTheSameTileOfTheTwoLevelsAreToldApartByTheirRowTheirGuardHeadingAndTheirChanges()
        {
            // The test map has its mines on the surface: the second record is moved under the first one, as the hex editor would do.
            var game = Load("mines_00_game_begin");
            var surface = game.History.Current.Mines().GetModel(0).Position;
            game.History.CreateMineSession(1).UpdateBytes(bytes =>
            {
                (bytes[MineLayout.X], bytes[MineLayout.Y], bytes[MineLayout.Level]) = ((byte)surface.X, (byte)surface.Y, 1);
                return bytes;
            });
            var page = Of(game, PlayerColor.Neutral);
            var above = page.Mines.Rows.Single(m => m.Index == 0);
            var below = page.Mines.Rows.Single(m => m.Index == 1);

            Assert.That(above.PositionText, Is.EqualTo($"Pos: {surface.X}, {surface.Y}, Surface"));
            Assert.That(below.PositionText, Is.EqualTo($"Pos: {surface.X}, {surface.Y}, Underground"));
            Assert.That((above.LevelText, below.LevelText), Is.EqualTo(("Surface", "Underground")));

            page.Mines.SelectedMine = above;
            var aboveHeading = page.GuardHeading;
            page.Mines.SelectedMine = below;
            Assert.That(page.GuardHeading, Does.EndWith("Underground").And.Not.EqualTo(aboveHeading));

            above.OwnerField.Selected = "Green";
            below.OwnerField.Selected = "Green";
            Assert.That(
                game.History.GetPendingChanges().Select(c => c.OwnerName),
                Is.EquivalentTo(new[] { $"{above.Kind} at {above.PositionText.Replace("Pos: ", string.Empty)}", $"{below.Kind} at {below.PositionText.Replace("Pos: ", string.Empty)}" }));
        }

        [Test]
        public void AnOwnerListsItsDwellingsWithTheirCreatureAndTheCreaturesToHire()
        {
            var game = LoadDwellings("05_tazar_flags_portal_of_glory");

            var blue = Of(game, PlayerColor.Blue);
            var neutral = Of(game, PlayerColor.Neutral);

            var guardhouse = blue.Dwellings.Single(d => d.PositionText == "Pos: 31, 56, Surface");
            Assert.That(guardhouse.CreatureName, Is.EqualTo("Pikeman"));
            Assert.That(guardhouse.OwnerField.Selected, Is.EqualTo("Blue"));
            Assert.That(guardhouse.StockText, Is.EqualTo("28"));
            Assert.That(Colors().Sum(c => Of(game, c).Dwellings.Count), Is.EqualTo(31), "every dwelling is in the list of exactly one owner");
            Assert.That(neutral.DwellingsSummary, Does.EndWith("dwellings"));
        }

        [Test]
        public void TwoDwellingsOnTheSameTileOfTheTwoLevelsAreToldApartByTheirRowAndTheirChanges()
        {
            // The test map has its dwellings on the surface: the second record is moved under the first one, as the hex editor would do.
            var game = LoadDwellings("05_tazar_flags_portal_of_glory");
            var surface = game.History.Current.Dwellings().GetModel(0).Position;
            game.History.CreateDwellingSession(1).UpdateBytes(bytes =>
            {
                (bytes[DwellingLayout.X], bytes[DwellingLayout.Y], bytes[DwellingLayout.Level]) = ((byte)surface.X, (byte)surface.Y, 1);
                return bytes;
            });
            var owner = game.History.Current.Dwellings().GetModel(0).Owner;
            var page = Of(game, owner);
            var rows = page.DwellingList.Items.Concat(Of(game, game.History.Current.Dwellings().GetModel(1).Owner).DwellingList.Items).DistinctBy(d => d.Index).ToList();
            var above = rows.Single(d => d.Index == 0);
            var below = rows.Single(d => d.Index == 1);

            Assert.That(above.PositionText, Is.EqualTo($"Pos: {surface.X}, {surface.Y}, Surface"));
            Assert.That(below.PositionText, Is.EqualTo($"Pos: {surface.X}, {surface.Y}, Underground"));
            Assert.That((above.LevelText, below.LevelText), Is.EqualTo(("Surface", "Underground")));

            above.StockText = "12345";
            below.StockText = "12345";
            Assert.That(
                game.History.GetPendingChanges().Select(c => c.OwnerName),
                Is.EquivalentTo(new[] { $"{above.CreatureName} dwelling at {above.PositionText.Replace("Pos: ", string.Empty)}", $"{below.CreatureName} dwelling at {below.PositionText.Replace("Pos: ", string.Empty)}" }));
        }

        [Test]
        public void TypingTheCreaturesToHireAndPickingAnOwnerChangeTheDwellingAndCanBeReverted()
        {
            var game = LoadDwellings("05_tazar_flags_portal_of_glory");
            var blue = Of(game, PlayerColor.Blue);
            var guardhouse = blue.Dwellings.Single(d => d.PositionText == "Pos: 31, 56, Surface");

            guardhouse.StockText = "3";
            guardhouse.OwnerField.Selected = "Green";

            var model = game.History.Current.Dwellings().GetModels().Single(d => d.Position.X == 31 && d.Position.Y == 56);
            Assert.That((model.Stock, model.Owner), Is.EqualTo((3L, PlayerColor.Green)));
            Assert.That(game.History.GetPendingChanges(), Is.Not.Empty);

            foreach (var change in game.History.GetPendingChanges().ToList())
            {
                change.Revert(game.History);
            }

            Assert.That(guardhouse.StockText, Is.EqualTo("28"));
            Assert.That(guardhouse.OwnerField.Selected, Is.EqualTo("Blue"));
            Assert.That(game.History.GetPendingChanges(), Is.Empty);
        }

        [Test]
        public void AStockThatIsNotANumberLeavesTheDwellingAloneAndOneThatIsTooBigIsTakenAsTheMost()
        {
            var game = LoadDwellings("05_tazar_flags_portal_of_glory");
            var guardhouse = Of(game, PlayerColor.Blue).Dwellings.Single(d => d.PositionText == "Pos: 31, 56, Surface");

            guardhouse.StockText = "many";
            Assert.That(guardhouse.StockText, Is.EqualTo("28"));

            guardhouse.StockText = "99999999999";
            Assert.That(guardhouse.Stock, Is.EqualTo(Siniskalc.Homm3.Constants.Army.MaxStackSize));
        }
    }
}
