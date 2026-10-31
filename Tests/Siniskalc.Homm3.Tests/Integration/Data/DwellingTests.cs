using Siniskalc.Homm3.Tests.Data;
namespace Siniskalc.Homm3.Tests.Integration.Data
{
    using Siniskalc.Homm3.Data;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// The dwellings of the test map in <c>TestData/Hota/Dwellings</c>: Tazar flags the creature generators of the Castle and the Rampart,
    /// Orrin flags them to Blue and hires their creatures.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class DwellingTests
    {
        private const string AfterTheCastle = "05_tazar_flags_portal_of_glory";
        private const string AfterTheHires = "07_orrin_hires_all_castle_dwellings_but_guardhouse";

        private static Siniskalc.Homm3.Runtime.Game Load(string name) => SavedGames.Get(Path.Combine("TestData", "Hota", "Dwellings", $"dwellings_{name}.GM2"));

        private static DwellingModel At(string save, int x, int y) => Load(save).History.Current.Dwellings().GetModels().Single(d => d.Position.X == x && d.Position.Y == y);

        [Test]
        public void TheDwellingsOfTheTestMapAreFoundWithTheirCreatureOwnerPositionAndStock()
        {
            var dwellings = Load(AfterTheCastle).History.Current.Dwellings().GetModels();

            Assert.That(dwellings, Has.Count.EqualTo(31));
            var guardhouse = At(AfterTheCastle, 31, 56);
            Assert.That(guardhouse.Creature, Is.EqualTo(Creature.Pikeman));
            Assert.That(guardhouse.Owner, Is.EqualTo(PlayerColor.Blue), "Tazar flagged it and Orrin flagged it to Blue");
            Assert.That(guardhouse.Stock, Is.EqualTo(28), "two weeks of the 14 pikemen it grows each week");
            Assert.That(At(AfterTheCastle, 27, 56).Creature, Is.EqualTo(Creature.Archer));
            Assert.That(At(AfterTheCastle, 28, 62).Creature, Is.EqualTo(Creature.Angel));
            Assert.That(At(AfterTheCastle, 28, 62).Owner, Is.EqualTo(PlayerColor.Red));
            Assert.That(At(AfterTheCastle, 31, 62).Creature?.Name, Is.EqualTo("Centaur"));
            Assert.That(At(AfterTheCastle, 31, 62).Owner, Is.EqualTo(PlayerColor.Neutral));
        }

        [Test]
        public void HiringEveryCreatureOfADwellingEmptiesItsStockAndFlaggingItChangesItsOwner()
        {
            Assert.That(At(AfterTheCastle, 25, 56).Stock, Is.EqualTo(14));
            Assert.That(At(AfterTheHires, 25, 56).Stock, Is.EqualTo(0));
            Assert.That(At(AfterTheHires, 31, 56).Stock, Is.EqualTo(28), "the guardhouse was not hired from");
            Assert.That(At(AfterTheCastle, 28, 62).Owner, Is.EqualTo(PlayerColor.Red));
            Assert.That(At(AfterTheHires, 28, 62).Owner, Is.EqualTo(PlayerColor.Blue));
        }

        [Test]
        public void ChangingTheOwnerAndTheStockWritesThoseBytesAndCanBePutBack()
        {
            var game = Load(AfterTheCastle);
            var dwellings = game.History.Current.Dwellings();
            var index = dwellings.GetModels().ToList().FindIndex(d => d.Position.X == 31 && d.Position.Y == 56);
            var chunk = dwellings[index];

            game.History.CreateDwellingSession(index).Update(d => d with { Owner = PlayerColor.Green, Stock = 100 });

            var saved = TestServices.SavefileCodec.Read(Path.Combine("TestData", "Hota", "Dwellings", $"dwellings_{AfterTheCastle}.GM2"));
            var written = game.GetBytesToSave();
            var changed = Enumerable.Range(0, saved.Length).Where(i => saved[i] != written[i]).ToList();
            Assert.That(changed, Is.EqualTo(new[] { chunk.StartIndex + DwellingLayout.Owner, chunk.StartIndex + DwellingLayout.Stock }));
            Assert.That(game.History.GetPendingChanges().Select(c => c.Change.Description), Is.EquivalentTo(new[] { "Owner", "Creatures to hire" }));

            foreach (var change in game.History.GetPendingChanges().ToList())
            {
                change.Revert(game.History);
            }

            Assert.That(game.History.Current.Dwellings().GetModel(index).Owner, Is.EqualTo(PlayerColor.Blue));
            Assert.That(game.History.Current.Dwellings().GetModel(index).Stock, Is.EqualTo(28));
        }

        [Test]
        public void TheStockOfADwellingCannotBeMoreThanAStackCanHold()
        {
            var game = Load(AfterTheCastle);

            Assert.That(() => game.History.CreateDwellingSession(0).Update(d => d with { Stock = long.MaxValue }), Throws.InstanceOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void EveryDwellingOfEverySaveIsWrittenBackAsTheSameBytes()
        {
            foreach (var file in Directory.GetFiles(Path.Combine("TestData", "Hota", "Dwellings"), "*.GM2"))
            {
                var dwellings = SavedGames.Get(file).History.Current.Dwellings();
                Assert.That(dwellings, Is.Not.Empty, file);
                foreach (var chunk in dwellings)
                {
                    Assert.That(dwellings.Serializer.Serialize(chunk, dwellings.Deserializer.Deserialize(chunk)).HasSameBytes(chunk), Is.True, $"{file}: {chunk.Name}");
                }
            }
        }

        [Test]
        public void EveryDwellingGrowsByItsWeeklyGrowthAtTheStartOfAWeek()
        {
            // The guardhouse and the centaur dwelling were left alone for weeks: 14 pikemen and 14 centaurs a week.
            Assert.That(At(AfterTheCastle, 31, 56).Stock, Is.EqualTo(28));
            Assert.That(At("09_tazar_flags_pegasus_dwelling_new_week", 31, 56).Stock, Is.EqualTo(42));
            Assert.That(At("16_tazar_visits_alehouse_new_week", 31, 56).Stock, Is.EqualTo(56));
            Assert.That(At("23_tazar_visits_sulfurous_lair_new_week", 31, 56).Stock, Is.EqualTo(70));
            Assert.That(At("23_tazar_visits_sulfurous_lair_new_week", 31, 62).Stock, Is.EqualTo(56));
        }

        [Test]
        public void VisitingANeutralDwellingChangesNothingInTheTableOfDwellings()
        {
            var before = At("12_tazar_flags_green_dragon_dwelling", 43, 56);
            var after = At("13_tazar_visits_rogue_cavern", 43, 56);

            Assert.That(after, Is.EqualTo(before));
            Assert.That(before.Creature?.Name, Is.EqualTo("Rogue"));
            Assert.That(before.Owner, Is.EqualTo(PlayerColor.Neutral));
        }
    }
}
