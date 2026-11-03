namespace Siniskalc.Homm2.Tests.Integration.ViewModels
{
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.ViewModels;

    /// <summary>
    /// The list of the mines an owner has on the map (Threedom).
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class MinesListsTests
    {
        [Test]
        public void EveryMineIsInTheListOfExactlyOneOwner()
        {
            var game = TestServices.LoadThreedom(71);

            var all = MinesLists.Create(game, TestServices.MineGuardianText);
            var owners = PlayerColor.All.Append(PlayerColor.Neutral).Distinct().ToList();

            Assert.That(all.Rows, Has.Count.EqualTo(41));
            Assert.That(all.Summary, Is.EqualTo("41 mines, 22 of them owned"));
            Assert.That(all.Rows.Select(m => m.Kind), Does.Contain("Sawmill").And.Contain("Gold mine").And.Contain("Alchemist's lab"));
            Assert.That(owners.Sum(c => MinesLists.Create(game, TestServices.MineGuardianText, c).Rows.Count), Is.EqualTo(41));
            Assert.That(MinesLists.Create(game, TestServices.MineGuardianText, PlayerColor.Neutral).Rows, Has.Count.EqualTo(19));
        }

        [Test]
        public void PickingAnOwnerForAMineChangesItsRecordAndFlagAndMovesItToTheListOfTheNewOwner()
        {
            var game = TestServices.LoadThreedom(71);
            var blue = MinesLists.Create(game, TestServices.MineGuardianText, PlayerColor.Blue);
            var green = MinesLists.Create(game, TestServices.MineGuardianText, PlayerColor.Green);
            var mine = blue.Rows.Single(m => m.PositionText == "at 34, 53");
            var greenBefore = green.Rows.Count;

            mine.OwnerField.Selected = "Green";
            blue.Refresh();
            green.Refresh();

            Assert.That(game.History.Current.Mines().GetModels().Single(m => m.Position == new MapPosition(34, 53)).Owner, Is.EqualTo(PlayerColor.Green));
            Assert.That(blue.Rows, Does.Not.Contain(mine));
            Assert.That(green.Rows, Has.Count.EqualTo(greenBefore + 1));
            Assert.That(game.History.GetPendingChanges(), Is.Not.Empty);

            foreach (var change in game.History.GetPendingChanges().ToList())
            {
                change.Revert(game.History);
            }

            Assert.That(game.History.Current.Mines().GetModels().Single(m => m.Position == new MapPosition(34, 53)).Owner, Is.EqualTo(PlayerColor.Blue));
            Assert.That(game.History.GetPendingChanges(), Is.Empty);
        }

        [Test]
        public void TheEarthGuardianSaveKeepsItsGuardianWhenTheMineMovesToNeutralAndWhenOwnershipIsUndoneOrReverted()
        {
            var game = TestServices.LoadNamed("MineGuardians", "mine_guardians_01_earth");
            var blue = MinesLists.Create(game, TestServices.MineGuardianText, PlayerColor.Blue);
            var neutral = MinesLists.Create(game, TestServices.MineGuardianText, PlayerColor.Neutral);
            var mine = blue.Rows.Single(m => m.PositionText == "at 43, 23");

            mine.OwnerField.Selected = "Neutral";
            blue.Refresh();
            neutral.Refresh();

            Assert.That(blue.Rows, Does.Not.Contain(mine));
            Assert.That(neutral.Rows.Single(m => m.Index == mine.Index).Summary, Is.EqualTo("Earth Elemental × 20"));
            Assert.That(game.History.Current.Mines()[mine.Index].ToArray(), Is.EqualTo(new byte[] { 0xFF, 6, 62, 20, 43, 23, 0xFF }));

            game.History.Undo();
            blue.Refresh();
            neutral.Refresh();
            Assert.That(blue.Rows.Single(m => m.Index == mine.Index).Summary, Is.EqualTo("Earth Elemental × 20"));
            Assert.That(neutral.Rows.Select(m => m.Index), Does.Not.Contain(mine.Index));

            game.History.Redo();
            foreach (var change in game.History.GetPendingChanges().ToList())
            {
                change.Revert(game.History);
            }

            blue.Refresh();
            Assert.That(blue.Rows.Single(m => m.Index == mine.Index).Summary, Is.EqualTo("Earth Elemental × 20"));
            Assert.That(game.History.Current.Mines()[mine.Index].ToArray(), Is.EqualTo(new byte[] { 0, 6, 62, 20, 43, 23, 0xFF }));
            Assert.That(game.History.GetPendingChanges(), Is.Empty);
        }
    }
}
