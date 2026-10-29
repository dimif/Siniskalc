namespace Siniskalc.Homm3.Tests.Integration.Data
{
    using Siniskalc.Homm3.Data;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.ViewModels;

    /// <summary>
    /// The hex view of one mine and of one dwelling of the map (<c>TestData/Hota/Mines</c> and <c>TestData/Hota/Dwellings</c>).
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class MapObjectByteEditTargetTests
    {
        private static Siniskalc.Homm3.Runtime.Game LoadMines() => TestServices.GameService.LoadGameFromFile(Path.Combine("TestData", "Hota", "Mines", "mines_08_red_abandoned_mine_sulfur.GM2"));

        private static Siniskalc.Homm3.Runtime.Game LoadDwellings() => TestServices.GameService.LoadGameFromFile(Path.Combine("TestData", "Hota", "Dwellings", "dwellings_01_tazar_flags_guardhouse.GM2"));

        [Test]
        public void TheBytesOfAMineAreNamedByTheirFieldsAndAnEditChangesTheMine()
        {
            var game = LoadMines();
            var mine = game.History.Current.Mines().GetModel(0);

            using var target = TestServices.ByteEditTargets.ForMine(game.History, 0);

            Assert.That(target.Title, Is.EqualTo(mine.Name));
            Assert.That(target.Regions.Select(r => r.Name), Is.EqualTo(new[] { "Pos X", "Pos Y", "Pos Lvl", "Owner", "Kind", "Abandoned", "Guard creatures", "Guard count" }));
            var owner = target.Regions.Single(r => r.Name == "Owner").Start;
            Assert.That(target.Update(bytes => { bytes[owner] = (byte)PlayerColor.Blue.Code; return bytes; }), Is.True);
            Assert.That(game.History.Current.Mines().GetModel(0).Owner, Is.EqualTo(PlayerColor.Blue));
        }

        [Test]
        public void TheBytesOfADwellingAreNamedByTheirFieldsAndAnEditChangesTheDwelling()
        {
            var game = LoadDwellings();
            var dwelling = game.History.Current.Dwellings().GetModel(0);

            using var target = TestServices.ByteEditTargets.ForDwelling(game.History, 0);

            Assert.That(target.Title, Is.EqualTo(dwelling.Name));
            Assert.That(target.Regions.Select(r => r.Name), Does.Contain("Owner").And.Contain("Creature").And.Contain("Creatures to hire"));
            var stock = target.Regions.Single(r => r.Name == "Creatures to hire").Start;
            Assert.That(target.Update(bytes => { bytes[stock] = 77; return bytes; }), Is.True);
            Assert.That(game.History.Current.Dwellings().GetModel(0).Stock % 256, Is.EqualTo(77));
        }

        [Test]
        public void AnOwnerPicksADwellingAndTheBytesAreThoseOfThePickedOne()
        {
            var game = LoadDwellings();
            var objects = new OwnedMapObjectsViewModel(game.History, PlayerColor.Neutral, TestServices.ByteEditTargets);
            Assert.That(objects.Dwellings, Has.Count.GreaterThan(1));
            Assert.That(objects.SelectedDwelling, Is.SameAs(objects.Dwellings[0]));

            objects.SelectedDwelling = objects.Dwellings[1];
            using var target = objects.CreateDwellingByteEditTarget()!;

            Assert.That(target.Title, Is.EqualTo(game.History.Current.Dwellings().GetModel(objects.Dwellings[1].Index).Name));
        }

        [Test]
        public void ADwellingGivenToAPlayerLeavesTheListAndThePickedOneMovesOn()
        {
            var game = LoadDwellings();
            var objects = new OwnedMapObjectsViewModel(game.History, PlayerColor.Neutral, TestServices.ByteEditTargets);
            var first = objects.SelectedDwelling!;

            first.OwnerField.Selected = "Blue";
            objects.Refresh();

            Assert.That(objects.Dwellings, Does.Not.Contain(first));
            Assert.That(objects.SelectedDwelling, Is.Not.SameAs(first));
            Assert.That(objects.SelectedDwelling, Is.SameAs(objects.Dwellings.FirstOrDefault()));
        }
    }
}
