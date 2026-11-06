using Siniskalc.Homm3.Tests.Data;

namespace Siniskalc.Homm3.Tests.Integration.ViewModels
{
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Homm3.Data;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.ViewModels;
    using Siniskalc.Wpf.ViewModels;
    using Siniskalc.Homm3.Tests.ViewModels;

    /// <summary>
    /// The heroes the game keeps out of play, in the navigation tree (<c>TestData/Hota/DisabledHeroes</c>).
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class DisabledHeroesNavigationTests
    {
        private static readonly string SavePath = Path.Combine("TestData", "Hota", "DisabledHeroes", "disabled_heroes_00_baseline.GM1");

        private Homm3EditorViewModel main = null!;
        private NavNodeViewModel neutral = null!;

        [SetUp]
        public void LoadGame()
        {
            this.main = TestServices.CreateHomm3Editor();
            this.main.LoadGameFromBytes(TestServices.SavefileCodec.Read(SavePath), SavePath);
            this.neutral = this.main.PlayerNodes().Single(n => n.Title == "Neutral");
        }

        [Test]
        public void TheHeroesDisabledInTheMapAreInAGroupOfTheirOwnUnderNeutral()
        {
            var disabled = this.neutral.Children.Single(c => c.Title == "Disabled heroes");
            var names = disabled.Children.Select(h => h.Title).ToList();

            Assert.That(disabled.Children, Has.Count.EqualTo(21));
            Assert.That(names, Is.SupersetOf(new[] { "Adrienne", "Gelu", "Giselle", "Dracon", "Xeron", "Haart Lich", "Mutare", "Mutare Drake", "Kilgor", "Boragus", "Kinkeria", "Derek", "Bidley", "Tark", "Tavin", "Murdoch", "Frederick", "Biarma" }));
            Assert.That(disabled.Children, Has.All.Matches<NavNodeViewModel>(h => h.Kind == NavNodeKind.Hero));
        }

        [Test]
        public void ADisabledHeroIsNotAlsoListedAmongTheFreeHeroesOfNeutral()
        {
            var free = this.neutral.Children.Single(c => c.Title == "Heroes").Children.Select(h => h.Title).ToList();

            Assert.That(free, Does.Not.Contain("Adrienne").And.Not.Contain("Gelu"));
            Assert.That(free, Does.Contain("Iona"), "a hero a tavern offers is not disabled");
        }

        [Test]
        public void TheSummaryOfNeutralCountsTheFreeHeroesThenTheDisabledOnesThenTheTowns()
        {
            var summary = this.neutral.Subtitle;

            Assert.That(summary, Does.Match(@"^\d+ heroes · 21 disabled( · \d+ towns?)?"));
        }

        [Test]
        public void ThePageOfNeutralHasATabForTheDisabledHeroesAndItsHeroesTabLeavesThemOut()
        {
            this.main.ShowNeutral(NeutralTabs.DisabledHeroes);
            var page = this.main.CurrentPageOf<NeutralViewModel>();

            Assert.That(page.HasDisabledHeroes, Is.True);
            Assert.That(page.DisabledHeroes, Has.Count.EqualTo(21));
            Assert.That(page.Heroes.Select(h => h.Name), Does.Not.Contain("Adrienne").And.Contain("Iona"));
            Assert.That(page.Summary, Does.Contain("21 disabled"));
        }

        [Test]
        public void ACampaignHeroHiredByRedAndBroughtBackByARetreatIsOfferedInItsTavernWithTheReservedEntry()
        {
            var retreated = SavedGames.Get(Path.Combine("TestData", "Hota", "DisabledHeroes", "disabled_heroes_01_adrienne_hired_and_retreated.GM2")).History.Current;

            Assert.That(retreated.Players().GetModel(retreated.Players().FindByColor(PlayerColor.Red)).Tavern.Ids, Is.EqualTo(new[] { 145, 119 }));
            Assert.That(retreated.HeroOwners().GetModel(0).Heroes[145].IsReserved, Is.True, "the game reserves a hero a tavern offers");
        }
    }
}
