namespace Siniskalc.Heroes.Wpf.Tests.ViewModels
{
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Runtime;
    using Siniskalc.Heroes.Data;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.ViewModels;
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// Tests what every game's navigation tree has in common: the groups of owners that are listed, the key that tells when the tree
    /// must be built again, and the lists of heroes and mines of an owner. The game's part is a fake.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class OwnerNavigationBuilderBaseTests
    {
        private sealed class Game(string? fileName = null, string? name = null) : AbstractSavegame(fileName, [0], new SaveDocumentHistory(new SaveDocument([0], new Dictionary<ChunkKind, IChunkCollection>(), new SavefileBytesPatcher())), name)
        {
            public override string VersionName => "Test";
        }

        private sealed class Builder : OwnerNavigationBuilderBase<Game, TestColor>
        {
            public Builder()
                : base(new Dictionary<NavNodeKind, ChunkKind>())
            {
            }

            public List<TestColor> Players { get; } = [];

            public List<TestColor> Castles { get; } = [];

            public List<TestColor> CastlesInUse { get; set; } = null!;

            public List<TestColor> Heroes { get; } = [];

            public List<TestColor> Mines { get; } = [];

            public List<TestColor> Dwellings { get; } = [];

            public int Extra { get; private set; }

            public static IReadOnlyList<NavEntry> HeroesOf(IHeroSummaries<TestColor> heroes, TestColor owner) => HeroEntriesOf(heroes, owner);

            public static IReadOnlyList<NavGroup> MinesOf(IEnumerable<TestMine> mines, TestColor owner, bool hasLevels) => MineGroupsOf(mines, owner, hasLevels);

            public override void RefreshMarks(NavigationTree tree, Game game)
            {
            }

            protected override IReadOnlyList<TestColor> CastleOwners(Game game) => this.Castles;

            protected override IReadOnlyList<TestColor> CastleOwnersInUse(Game game) => this.CastlesInUse ?? this.Castles;

            protected override IReadOnlyList<TestColor> HeroOwners(Game game) => this.Heroes;

            protected override IReadOnlyList<TestColor> MineOwners(Game game) => this.Mines;

            protected override IReadOnlyList<TestColor> DwellingOwners(Game game) => this.Dwellings;

            protected override IEnumerable<TestColor> PlayersInGame(Game game) => this.Players;

            protected override NavNodeViewModel CreateGroup(NavigationTree tree, Game game, TestColor owner) =>
                new(owner.Name, string.Empty, "#000000", NavNodeKind.Group);

            protected override void AddExtraNodes(NavigationTree tree, Game game, NavNodeViewModel overview)
            {
                this.Extra++;
                overview.Children.Add(new NavNodeViewModel("Extra", string.Empty, "#000000", NavNodeKind.Group));
            }
        }

        private static (NavigationTree Tree, NavNodeViewModel Overview) Built(Builder builder, Game? game = null)
        {
            var tree = new NavigationTree(_ => { }, builder.ChunkKinds);
            builder.Build(tree, game ?? new Game());
            return (tree, tree.Overview!);
        }

        [Test]
        public void TheChunkKindsAreThoseTheGameGave()
        {
            var kinds = new Dictionary<NavNodeKind, ChunkKind> { [NavNodeKind.Hero] = new("Hero", 0) };

            Assert.That(new TestKindsBuilder(kinds).ChunkKinds, Is.SameAs(kinds));
        }

        private sealed class TestKindsBuilder(IReadOnlyDictionary<NavNodeKind, ChunkKind> kinds) : OwnerNavigationBuilderBase<Game, TestColor>(kinds)
        {
            public override void RefreshMarks(NavigationTree tree, Game game)
            {
            }

            protected override IReadOnlyList<TestColor> CastleOwners(Game game) => [];

            protected override IReadOnlyList<TestColor> HeroOwners(Game game) => [];

            protected override IEnumerable<TestColor> PlayersInGame(Game game) => [];

            protected override NavNodeViewModel CreateGroup(NavigationTree tree, Game game, TestColor owner) => new(owner.Name, string.Empty, "#000000", NavNodeKind.Group);
        }

        [Test]
        public void TheOverviewIsCalledGameShowsTheFileNameOnlyAndStartsOpen()
        {
            var (tree, overview) = Built(new Builder(), new Game(name: "C:\\saves\\a.GM1"));

            Assert.That((overview.Title, overview.Subtitle, overview.Kind), Is.EqualTo(("Game", "a.GM1", NavNodeKind.Overview)));
            Assert.That((overview.IsExpanded, overview.DefaultExpanded, overview.ColorHex), Is.EqualTo((true, true, PlayerPalette.NoPlayer)));
            Assert.That(tree.Nodes, Is.EqualTo(new[] { overview }));
        }

        [Test]
        public void AGameWithNoNameHasNoSubtitle()
        {
            var game = new Game(name: string.Empty);

            Assert.That(Built(new Builder(), game).Overview.Subtitle, Is.Empty);
        }

        [Test]
        public void EveryOwnerOfThePlayersHeroesAndCastlesGetsAGroupAndOnceOnly()
        {
            var builder = new Builder();
            builder.Players.AddRange([TestColor.Red]);
            builder.Heroes.AddRange([TestColor.Blue, TestColor.Red]);
            builder.Castles.AddRange([TestColor.Blue]);

            var (_, overview) = Built(builder);

            Assert.That(overview.Children.Select(c => c.Title).Take(2), Is.EqualTo(new[] { "Red", "Blue" }));
        }

        [Test]
        public void APlayerOfTheSavefileGetsAGroupEvenWithNoHeroOrCastle()
        {
            var builder = new Builder();
            builder.Players.AddRange([TestColor.Blue, TestColor.Red]);

            var (_, overview) = Built(builder);

            Assert.That(overview.Children.Select(c => c.Title).Take(2), Is.EqualTo(new[] { "Red", "Blue" }));
        }

        [Test]
        public void TheGroupsComeInTheOrderOfTheColoursWithNeutralLast()
        {
            var builder = new Builder();
            builder.Heroes.AddRange([TestColor.Neutral, TestColor.Blue, TestColor.Red]);

            var (_, overview) = Built(builder);

            Assert.That(overview.Children.Select(c => c.Title).Take(3), Is.EqualTo(new[] { "Red", "Blue", "Neutral" }));
        }

        [Test]
        public void ACastleOwnerThatIsNotInUseGetsNoGroupButOneThatOwnsAHeroStillDoes()
        {
            var builder = new Builder { CastlesInUse = [TestColor.Red] };
            builder.Castles.AddRange([TestColor.Red, TestColor.Blue]);

            var (_, overview) = Built(builder);

            Assert.That(overview.Children.Select(c => c.Title).Take(1), Is.EqualTo(new[] { "Red" }));
            Assert.That(overview.Children.Select(c => c.Title), Does.Not.Contain("Blue"));
        }

        [Test]
        public void TheGameMayAddItsOwnNodesAfterTheGroups()
        {
            var builder = new Builder();
            builder.Heroes.Add(TestColor.Red);

            var (_, overview) = Built(builder);

            Assert.That(builder.Extra, Is.EqualTo(1));
            Assert.That(overview.Children.Select(c => c.Title), Is.EqualTo(new[] { "Red", "Extra" }));
        }

        [Test]
        public void TheKeyTellsTheOwnersOfCastlesHeroesMinesAndDwellingsInOrder()
        {
            var builder = new Builder();
            builder.Castles.AddRange([TestColor.Red, TestColor.Blue]);
            builder.Heroes.Add(TestColor.Neutral);
            builder.Mines.Add(TestColor.Blue);
            builder.Dwellings.AddRange([TestColor.Red, TestColor.Red]);

            Assert.That(builder.StructureKey(new Game()), Is.EqualTo("Red,Blue;Neutral;Blue;Red,Red"));
        }

        [Test]
        public void TheKeyChangesWhenAnyKindOfOwnerChangesAndNotOtherwise()
        {
            var builder = new Builder();
            var game = new Game();
            var before = builder.StructureKey(game);

            Assert.That(builder.StructureKey(game), Is.EqualTo(before));

            builder.Castles.Add(TestColor.Red);
            var withCastle = builder.StructureKey(game);
            builder.Heroes.Add(TestColor.Red);
            var withHero = builder.StructureKey(game);
            builder.Mines.Add(TestColor.Red);
            var withMine = builder.StructureKey(game);
            builder.Dwellings.Add(TestColor.Red);
            var withDwelling = builder.StructureKey(game);

            Assert.That(new[] { before, withCastle, withHero, withMine, withDwelling }, Is.Unique);
        }

        [Test]
        public void TheHeroesOfAnOwnerAreListedByNameWithTheirPlaceAndWhatTheyAre()
        {
            var heroes = new TestHeroes(
                new HeroSummary<TestColor>(0, 10, "Zed", TestColor.Red, "Knight", 3),
                new HeroSummary<TestColor>(1, 11, "Ann", TestColor.Blue, "Wizard", 1),
                new HeroSummary<TestColor>(2, 12, "Bob", TestColor.Red, "Cleric", 2));

            var entries = Builder.HeroesOf(heroes, TestColor.Red);

            Assert.That(entries, Is.EqualTo(new[] { new NavEntry(2, "Bob", "Cleric, Level 2"), new NavEntry(0, "Zed", "Knight, Level 3") }));
            Assert.That(Builder.HeroesOf(heroes, TestColor.Neutral), Is.Empty);
        }

        [Test]
        public void TheMinesOfAnOwnerAreGroupedByTheResourceTheyGiveInTheOrderOfTheResources()
        {
            var mines = new[]
            {
                new TestMine(0, MineKind.GoldMine, TestColor.Red, new MapPosition(1, 1, 0)),
                new TestMine(1, MineKind.Sawmill, TestColor.Red, new MapPosition(2, 2, 0)),
                new TestMine(2, MineKind.OreMine, TestColor.Blue, new MapPosition(3, 3, 0)),
                new TestMine(3, MineKind.Sawmill, TestColor.Red, new MapPosition(9, 9, 0)),
            };

            var groups = Builder.MinesOf(mines, TestColor.Red, hasLevels: true);

            Assert.That(groups.Select(g => g.Title), Is.EqualTo(new[] { "Wood", "Gold" }));
            Assert.That(groups[0].Entries.Select(e => e.Index), Is.EqualTo(new[] { 1, 3 }));
        }

        [Test]
        public void TheMinesOfAGroupAreOrderedByLevelThenXThenY()
        {
            var mines = new[]
            {
                new TestMine(0, MineKind.Sawmill, TestColor.Red, new MapPosition(5, 1, 1)),
                new TestMine(1, MineKind.Sawmill, TestColor.Red, new MapPosition(5, 9, 0)),
                new TestMine(2, MineKind.Sawmill, TestColor.Red, new MapPosition(2, 3, 0)),
                new TestMine(3, MineKind.Sawmill, TestColor.Red, new MapPosition(2, 1, 0)),
            };

            var entries = Builder.MinesOf(mines, TestColor.Red, hasLevels: true).Single().Entries;

            Assert.That(entries.Select(e => e.Index), Is.EqualTo(new[] { 3, 2, 1, 0 }));
        }

        [Test]
        public void MinesOfTheSameLevelAreOrderedByXBeforeY()
        {
            var mines = new[]
            {
                new TestMine(0, MineKind.Sawmill, TestColor.Red, new MapPosition(5, 1, 0)),
                new TestMine(1, MineKind.Sawmill, TestColor.Red, new MapPosition(1, 9, 0)),
            };

            var entries = Builder.MinesOf(mines, TestColor.Red, hasLevels: true).Single().Entries;

            Assert.That(entries.Select(e => e.Index), Is.EqualTo(new[] { 1, 0 }));
        }

        [Test]
        public void AMineIsNamedByItsPlaceWithTheLevelOnlyWhereTheMapHasLevels()
        {
            var mines = new[] { new TestMine(0, MineKind.Sawmill, TestColor.Red, new MapPosition(5, 6, 1)) };

            Assert.That(Builder.MinesOf(mines, TestColor.Red, true).Single().Entries.Single().Title, Is.EqualTo("At 5, 6, Underground"));
            Assert.That(Builder.MinesOf(mines, TestColor.Red, false).Single().Entries.Single().Title, Is.EqualTo("At 5, 6"));
        }

        [Test]
        public void AnOwnerWithNoMinesHasNoGroups()
        {
            Assert.That(Builder.MinesOf([], TestColor.Red, true), Is.Empty);
        }

        private sealed class TestHeroes(params HeroSummary<TestColor>[] summaries) : IHeroSummaries<TestColor>
        {
            public IReadOnlyList<HeroSummary<TestColor>> GetSummaries() => summaries;

            public HeroSummary<TestColor>? FindById(int heroId) => summaries.FirstOrDefault(s => s.HeroId == heroId);
        }
    }
}
