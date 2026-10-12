using Siniskalc.Runtime;
using Siniskalc.ViewModels;

namespace Siniskalc.Tests.ViewModels
{
    /// <summary>
    /// Tests the castle pages and how castles sit in the navigation tree and the overview, on the castles_01_blacksmith_garrison_tumlac_to_blue savegame:
    /// Castle Haret (Castle, tan, with a garrison) and Castle Tumlac (Tower, blue).
    /// </summary>
    [TestFixture]
    internal sealed class CastleViewModelTests
    {
        private static readonly string SavePath = Path.Combine("TestData", "Hota", "castles_01_blacksmith_garrison_tumlac_to_blue.GM1");

        private MainViewModel main = null!;

        [SetUp]
        public void LoadGame()
        {
            this.main = TestServices.CreateMainViewModel();
            this.main.LoadGameFromBytes(TestServices.SavefileCodec.Read(SavePath), SavePath);
        }

        private NavNodeViewModel CastleNode(string color, string name) =>
            this.main.PlayerNodes().Single(n => n.Title == color).Descendants().Single(c => c.Kind == NavNodeKind.Castle && c.Title == name);

        [Test]
        public void CastlesSitInTheTownsGroupOfTheirOwner()
        {
            var tan = this.main.PlayerNodes().Single(n => n.Title == "Tan");
            var towns = tan.Children.Single(c => c.Title == "Towns");

            Assert.That(tan.Kind, Is.EqualTo(NavNodeKind.Player));
            Assert.That(towns.Kind, Is.EqualTo(NavNodeKind.PlayerTab));
            Assert.That(towns.Children[0].Kind, Is.EqualTo(NavNodeKind.Castle));
            Assert.That(towns.Children[0].Title, Is.EqualTo("Castle Haret"));
            Assert.That(towns.Children[0].Subtitle, Is.EqualTo("Castle"));
            Assert.That(tan.Subtitle, Does.Contain("1 town"));
            Assert.That(this.CastleNode("Blue", "Castle Tumlac").Subtitle, Is.EqualTo("Tower"));
        }

        [Test]
        public void SelectingACastleNodeShowsTheCastle()
        {
            this.CastleNode("Tan", "Castle Haret").IsSelected = true;

            var castle = this.main.CurrentPageOf<CastleViewModel>();
            Assert.That(castle.Name, Is.EqualTo("Castle Haret"));
            Assert.That(castle.FactionText, Is.EqualTo("Castle"));
            Assert.That(castle.OwnerText, Is.EqualTo("Tan"));
            Assert.That(castle.PositionText, Is.EqualTo("X: 33, Y: 23, Surface (0)"));
        }

        [Test]
        public void TheGarrisonListsAllSevenSlots()
        {
            this.main.ShowCastle(this.CastleNode("Tan", "Castle Haret").CastleIndex);

            var castle = this.main.CurrentPageOf<CastleViewModel>();
            Assert.That(castle.Garrison.Select(s => s.CreatureName), Is.EqualTo(new[] { "Pikeman", "Archer", "Griffin", "Swordsman", "Monk", "Cavalier", "Angel" }));
            Assert.That(castle.Garrison.Select(s => s.CountText), Has.All.EqualTo("123"));
            Assert.That(castle.Garrisoned, Is.EqualTo("7 of 7 slots in use"));
        }

        [Test]
        public void TheGarrisonOfAnEmptyCastleShowsEmptySlots()
        {
            this.main.ShowCastle(this.CastleNode("Blue", "Castle Tumlac").CastleIndex);

            var castle = this.main.CurrentPageOf<CastleViewModel>();
            Assert.That(castle.Garrison, Has.Count.EqualTo(7));
            Assert.That(castle.Garrison, Has.All.Matches<GarrisonSlotRowViewModel>(s => s.IsEmpty && s.CreatureName == "Empty"));
            Assert.That(castle.Garrisoned, Is.EqualTo("No garrison"));
        }

        [Test]
        public void TheBuildingsAreGroupedAndTheBuiltOnesTicked()
        {
            this.main.ShowCastle(this.CastleNode("Tan", "Castle Haret").CastleIndex);

            var castle = this.main.CurrentPageOf<CastleViewModel>();
            Assert.That(castle.BuildingGroups.Select(g => g.Title), Is.EqualTo(new[] { "Town", "Special buildings", "Creature dwellings" }));

            var built = castle.BuildingGroups.SelectMany(g => g.Rows).Where(r => r.IsBuilt).Select(r => r.Name);
            Assert.That(built, Is.EquivalentTo(new[]
            {
                "Mage Guild, level 1", "Fort", "Village Hall", "Town Hall", "City Hall", "Capitol", "Blacksmith",
                "Guardhouse", "Archers' Tower", "Griffin Tower",
            }));
            Assert.That(castle.BuildingsSummary, Does.StartWith("10 of 35 buildings built"));

            // Castle has Mage Guild up to level 4 and a shipyard, and no fifth level.
            var town = castle.BuildingGroups[0].Rows.Select(r => r.Name);
            Assert.That(town, Does.Contain("Shipyard").And.Contain("Mage Guild, level 4").And.Not.Contain("Mage Guild, level 5"));
        }

        [Test]
        public void ABuildingBuiltWithoutWhatItNeedsIsFlagged()
        {
            this.main.ShowCastle(this.CastleNode("Tan", "Castle Haret").CastleIndex);

            var rows = (this.main.CurrentPageOf<CastleViewModel>()).BuildingGroups.SelectMany(g => g.Rows).ToDictionary(r => r.Name);

            // The map editor built the town hall without a tavern.
            Assert.That(rows["Town Hall"].Requirements, Is.EqualTo("Requires Village Hall, Tavern"));
            Assert.That(rows["Town Hall"].Warning, Is.EqualTo("Built without Tavern"));
            Assert.That(rows["Capitol"].Warning, Is.EqualTo("Built without Castle"));
            Assert.That(rows["Guardhouse"].Warning, Is.Empty);
            Assert.That(rows["Tavern"].Warning, Is.Empty);
            Assert.That(rows["Tavern"].IsBuilt, Is.False);
        }

        [Test]
        public void TheOverviewListsEachCastleUnderItsOwner()
        {
            var tan = this.main.LoadedOverview().Players.Single(p => p.Name == "Tan");
            var blue = this.main.LoadedOverview().Players.Single(p => p.Name == "Blue");

            Assert.That(tan.Castles.Select(c => c.Name), Is.EqualTo(new[] { "Castle Haret" }));
            Assert.That(blue.Castles.Select(c => c.Name), Does.Contain("Castle Tumlac"));
            Assert.That(blue.Castles.Single(c => c.Name == "Castle Tumlac").Detail, Is.EqualTo("Tower"));

            blue.Castles.Single(c => c.Name == "Castle Tumlac").OpenCommand.Execute(null);

            Assert.That((this.main.CurrentPageOf<CastleViewModel>()).Name, Is.EqualTo("Castle Tumlac"));
            Assert.That(this.CastleNode("Blue", "Castle Tumlac").IsSelected, Is.True);
        }

        [Test]
        public void ACastleOfNoPlayerIsListedUnderNeutral()
        {
            var unowned = this.main.PlayerNodes().Single(n => n.Title == "Neutral");

            Assert.That(unowned.Children.Single(c => c.Title == "Towns").Children.Where(c => c.Kind == NavNodeKind.Castle), Is.Not.Empty);
        }

        [Test]
        public void ReloadingAGameClosesTheCastlePages()
        {
            this.main.ShowCastle(this.CastleNode("Tan", "Castle Haret").CastleIndex);
            var first = this.main.CurrentPage;

            this.main.LoadGameFromBytes(TestServices.SavefileCodec.Read(SavePath), SavePath);
            this.main.ShowCastle(this.CastleNode("Tan", "Castle Haret").CastleIndex);

            Assert.That(this.main.CurrentPage, Is.Not.SameAs(first));
        }
    }
}
