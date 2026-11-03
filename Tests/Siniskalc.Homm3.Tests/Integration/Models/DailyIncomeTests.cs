using Siniskalc.Homm3.Tests.Data;

namespace Siniskalc.Homm3.Tests.Integration.Models
{
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Homm3.Data;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// Tests what a player earns each day from its mines and the town halls of its towns (<c>TestData/Hota/Mines</c> and <c>TestData/Roe</c>).
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class DailyIncomeTests
    {
        [Test]
        public void EachMineGivesItsYieldToItsOwnerAndTheOthersGetNothingFromIt()
        {
            var game = SavedGames.Get(Path.Combine("TestData", "Hota", "Mines", "mines_08_red_abandoned_mine_sulfur.GM2"));
            var document = game.History.Current;
            var mines = document.Mines().GetModels();

            var red = DailyIncome.Of(PlayerColor.Red, mines, document.Castles().GetModels());

            var owned = mines.Where(m => m.Owner == PlayerColor.Red).ToList();
            Assert.That(owned, Is.Not.Empty);
            Assert.That(red[Resource.Sulfur], Is.EqualTo(owned.Count(m => m.Kind == MineKind.SulfurMine)));
            Assert.That(red[Resource.Ore], Is.EqualTo(2 * owned.Count(m => m.Kind == MineKind.OreMine)));
            Assert.That(red[Resource.Wood], Is.EqualTo(2 * owned.Count(m => m.Kind == MineKind.Sawmill)));
            Assert.That(DailyIncome.Of(PlayerColor.Neutral, [], []), Is.EqualTo(ResourceSet.None));
        }

        [Test]
        public void ATownGivesGoldForItsBestHallAndOnlyThatOne()
        {
            var path = Path.Combine("TestData", "Roe", "roe_61_inferno_garrison_and_visiting.GM1");
            var main = TestServices.CreateHomm3Editor();
            main.LoadGameFromBytes(TestServices.SavefileCodec.Read(path), path);
            var castles = main.LoadedGame().History.Current.Castles().GetModels().Where(c => c.IsUsed).ToList();

            foreach (var castle in castles)
            {
                var expected = castle.HasBuilt(BuildingType.Capitol) ? 4000 : castle.HasBuilt(BuildingType.CityHall) ? 2000 : castle.HasBuilt(BuildingType.TownHall) ? 1000 : castle.HasBuilt(BuildingType.VillageHall) ? 500 : 0;
                Assert.That(DailyIncome.HallGold(castle), Is.EqualTo(expected), castle.Name);
                Assert.That(DailyIncome.Of(castle.Owner, [], [castle]).Gold, Is.EqualTo(expected), castle.Name);
            }
        }
    }
}
