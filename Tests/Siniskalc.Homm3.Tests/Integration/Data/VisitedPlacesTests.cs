using Siniskalc.Homm3.Tests.Data;
namespace Siniskalc.Homm3.Tests.Integration.Data
{
    using Siniskalc.Homm3.Data;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Runtime;

    /// <summary>
    /// The places a hero visited, read from the saves of <c>TestData/Hota/VisitedPlaces</c> in which Tazar and, a turn behind, Orrin visit two
    /// places of each kind.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class VisitedPlacesTests
    {
        private static Game Load(string name) => SavedGames.Get(Path.Combine("TestData", "Hota", "VisitedPlaces", $"visited_places_{name}.GM2"));

        private static uint Mask(Game game, string hero, VisitedPlaceKind kind)
        {
            var heroes = game.History.Current.Heroes();
            return heroes.GetModel(heroes.FindByName(hero)).PlaceVisits.Of(kind);
        }

        // Each first visit sets the bit of the place's number, the second visit of the other place sets the other bit.
        [TestCase("03_tazar_school_of_war_1", "04_tazar_school_of_war_2", "SchoolOfWar", 2u, 3u)]
        [TestCase("05_tazar_school_of_magic_1", "06_tazar_school_of_magic_2", "SchoolOfMagic", 1u, 3u)]
        [TestCase("07_tazar_learning_stone_1", "08_tazar_learning_stone_2", "LearningStone", 2u, 3u)]
        [TestCase("09_tazar_garden_of_revelation_1", "10_tazar_garden_of_revelation_2", "GardenOfRevelation", 1u, 3u)]
        [TestCase("11_tazar_star_axis_1", "12_tazar_star_axis_2", "StarAxis", 2u, 3u)]
        [TestCase("13_tazar_marletto_tower_1", "14_tazar_marletto_tower_2", "MarlettoTower", 1u, 3u)]
        [TestCase("15_tazar_mercenary_camp_1", "16_tazar_mercenary_camp_2", "MercenaryCamp", 2u, 3u)]
        [TestCase("17_tazar_colossus_1", "18_tazar_colossus_2", "ColossusOfTheMagi", 1u, 3u)]
        [TestCase("19_tazar_arena_1", "20_tazar_arena_2", "Arena", 2u, 3u)]
        [TestCase("21_tazar_gazebo_1", "22_tazar_gazebo_2", "Gazebo", 1u, 3u)]
        [TestCase("23_tazar_tree_of_knowledge_1", "24_tazar_tree_of_knowledge_2", "TreeOfKnowledge", 2u, 3u)]
        [TestCase("25_tazar_library_of_enlightenment_1", "26_tazar_library_of_enlightenment_2", "LibraryOfEnlightenment", 1u, 3u)]
        public void AHeroVisitingAPlaceSetsTheBitOfItsNumberInTheNumberOfItsKind(string first, string second, string kindName, uint afterFirst, uint afterSecond)
        {
            var kind = TestCatalog.Hota.VisitablePlaces.ById(kindName);

            Assert.That(Mask(Load(first), "Tazar", kind), Is.EqualTo(afterFirst));
            Assert.That(Mask(Load(second), "Tazar", kind), Is.EqualTo(afterSecond));
            Assert.That(Mask(Load("00_baseline"), "Tazar", kind), Is.EqualTo(0u));
        }

        [Test]
        public void TheHeroesOfTheSameMapRememberTheirOwnVisitsAndOrrinGetsTheBitsATurnAfterTazar()
        {
            Assert.That(Mask(Load("04_tazar_school_of_war_2"), "Tazar", TestCatalog.For(GameVersion.HornOfTheAbyss).VisitablePlaces.ById("SchoolOfWar")), Is.EqualTo(3u));
            Assert.That(Mask(Load("04_tazar_school_of_war_2"), "Orrin", TestCatalog.For(GameVersion.HornOfTheAbyss).VisitablePlaces.ById("SchoolOfWar")), Is.EqualTo(2u));
            Assert.That(Mask(Load("05_tazar_school_of_magic_1"), "Orrin", TestCatalog.For(GameVersion.HornOfTheAbyss).VisitablePlaces.ById("SchoolOfWar")), Is.EqualTo(3u));
        }

        [Test]
        public void ThePlacesOfAKindAreFoundOnTheMapWithTheirNumber()
        {
            var places = Load("04_tazar_school_of_war_2").Places;

            Assert.That(places.Of(TestCatalog.For(GameVersion.HornOfTheAbyss).VisitablePlaces.ById("SchoolOfMagic")).Select(p => (p.Number, p.Position.X, p.Position.Y)), Is.EqualTo(new[] { (0, 17, 26), (1, 17, 28) }));
            Assert.That(places.Of(TestCatalog.For(GameVersion.HornOfTheAbyss).VisitablePlaces.ById("LearningStone")).Select(p => p.Number), Does.Contain(0).And.Contain(1));
        }

        [Test]
        public void TheTreesOfKnowledgeAreFoundWhateverFlagsTheirCellsHold()
        {
            // The cell of a tree has 0 or 1 for its number before a visit and 0x60 or 0x61 after, and a payment after it instead of zeros.
            Assert.That(Load("00_baseline").Places.Of(TestCatalog.For(GameVersion.HornOfTheAbyss).VisitablePlaces.ById("TreeOfKnowledge")).Select(p => (p.Number, p.Position.X, p.Position.Y)), Is.EqualTo(new[] { (0, 2, 20), (1, 2, 23) }));
            Assert.That(Load("26_tazar_library_of_enlightenment_2").Places.Of(TestCatalog.For(GameVersion.HornOfTheAbyss).VisitablePlaces.ById("TreeOfKnowledge")).Select(p => (p.Number, p.Position.X, p.Position.Y)), Does.Contain((0, 2, 20)));
        }

        [Test]
        public void ATreeAHeroStandsOnIsNumberedWithoutTheFlagsTheHeroReads()
        {
            // Tazar stands next to the tree at 2, 23 and reads 0x21 for its number; the number of the tree is 1.
            var trees = Load("23_tazar_tree_of_knowledge_1").Places.Of(TestCatalog.For(GameVersion.HornOfTheAbyss).VisitablePlaces.ById("TreeOfKnowledge"));

            Assert.That(trees.Select(p => p.Number), Has.All.LessThan(2));
            Assert.That(trees.Select(p => p.Number), Does.Contain(1));
        }

        [Test]
        public void APlaceAHeroStandsOnIsFoundFromTheRecordOfTheHero()
        {
            // In this save Tazar stands on the first school of war and Orrin on the second: the map holds neither, the heroes name both.
            var places = Load("04_tazar_school_of_war_2").Places.Of(TestCatalog.For(GameVersion.HornOfTheAbyss).VisitablePlaces.ById("SchoolOfWar"));

            Assert.That(places.Select(p => p.Number), Is.EqualTo(new[] { 0, 1 }));
            Assert.That(places[0].Position.X, Is.EqualTo(21));
        }

        [Test]
        public void ForgettingAPlaceWritesTheNumberOfItsKindAndCanBePutBack()
        {
            var game = Load("04_tazar_school_of_war_2");
            var heroes = game.History.Current.Heroes();
            var index = heroes.FindByName("Tazar");
            var chunk = heroes[index];

            game.History.CreateHeroSession(index).Update(h => h with { PlaceVisits = h.PlaceVisits.With(TestCatalog.For(GameVersion.HornOfTheAbyss).VisitablePlaces.ById("SchoolOfWar"), 2) });

            var written = game.GetBytesToSave();
            Assert.That(BitConverter.ToUInt32(written, chunk.StartIndex + VisitedPlaceOffsets.For(VisitSlot.PaidAttackOrDefense)), Is.EqualTo(2u));
            var change = game.History.GetPendingChanges().Single();
            Assert.That(change.Change.Description, Is.EqualTo("Visited: schools of war"));
            Assert.That((change.Change.Before, change.Change.After), Is.EqualTo(("2", "1")));

            change.Revert(game.History);
            Assert.That(Mask(game, "Tazar", TestCatalog.For(GameVersion.HornOfTheAbyss).VisitablePlaces.ById("SchoolOfWar")), Is.EqualTo(3u));
        }

        [Test]
        public void EveryHeroOfEverySaveIsWrittenBackAsTheSameBytes()
        {
            foreach (var file in Directory.GetFiles(Path.Combine("TestData", "Hota", "VisitedPlaces"), "*.GM2"))
            {
                var game = SavedGames.Get(file);
                var heroes = game.History.Current.Heroes();
                foreach (var chunk in heroes)
                {
                    var again = heroes.Serializer.Serialize(chunk, heroes.Deserializer.Deserialize(chunk));
                    Assert.That(again.HasSameBytes(chunk), Is.True, $"{Path.GetFileName(file)}: {chunk.Name}");
                }
            }
        }
    }
}
