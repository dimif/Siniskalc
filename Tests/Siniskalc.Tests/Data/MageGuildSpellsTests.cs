using Siniskalc.Data;
using Siniskalc.Entities;
using Siniskalc.Models;
using Siniskalc.Runtime;

namespace Siniskalc.Tests.Data
{
    /// <summary>
    /// The spells of the Mage Guild, which follow the castle chunk. The expected spells are the ones read from the guild screen in the game:
    /// Tower and Dungeon in <c>Roe/roe_46_mage_guild_5</c>, Tower before and after its Library in <c>Roe/roe_11_city_hall</c> and <c>Roe/roe_12_unique_1</c>, and
    /// Bulwark's Jotunngard in <c>Hota/Bulwark/bulwark_33_mage_guild_5__nothing</c>.
    /// </summary>
    [TestFixture]
    internal sealed class MageGuildSpellsTests
    {
        private static CastleModel Castle(string folder, string save, string name)
        {
            Game game = TestServices.GameService.LoadGameFromFile(Path.Combine("TestData", folder, save + ".GM1"));
            var castles = game.History.Current.Castles();
            return TestServices.CastleDeserializer.Deserialize(castles[castles.FindByName(name)]);
        }

        private static string[] Names(CastleModel castle, int level) => [.. castle.MageGuildSpells[level - 1].Select(s => s.Name)];

        [Test]
        public void TheTowerOfShadowOfDeathHoldsTheSpellsTheGameShowsWithItsLibrary()
        {
            var tower = Castle("Roe", "roe_46_mage_guild_5", "3_Tower");

            Assert.That(Names(tower, 1), Is.EqualTo(new[] { "Magic Arrow", "Dispel", "Curse", "Summon Boat", "Cure", "Stone Skin" }));
            Assert.That(Names(tower, 2), Is.EqualTo(new[] { "Visions", "Scuttle Boat", "Lightning Bolt", "Precision", "Ice Bolt" }));
            Assert.That(Names(tower, 3), Is.EqualTo(new[] { "Destroy Undead", "Teleport", "Air Shield", "Anti Magic" }));
            Assert.That(Names(tower, 4), Is.EqualTo(new[] { "Clone", "Chain Lightning", "Fire Shield" }));
            Assert.That(Names(tower, 5), Is.EqualTo(new[] { "Fly", "Implosion" }));
        }

        [Test]
        public void TheDungeonOfShadowOfDeathHoldsTheSpellsTheGameShowsAndNotTheOnesAfterThem()
        {
            var dungeon = Castle("Roe", "roe_46_mage_guild_5", "6_Dung");

            Assert.That(Names(dungeon, 1), Is.EqualTo(new[] { "View Air", "Summon Boat", "Dispel", "Magic Arrow", "View Earth" }));
            Assert.That(Names(dungeon, 2), Is.EqualTo(new[] { "Lightning Bolt", "Scuttle Boat", "Fire Wall", "Ice Bolt" }));
            Assert.That(Names(dungeon, 3), Is.EqualTo(new[] { "Land Mine", "Force Field", "Hypnotize" }));
            Assert.That(Names(dungeon, 4), Is.EqualTo(new[] { "Town Portal", "Armageddon" }));
            Assert.That(Names(dungeon, 5), Is.EqualTo(new[] { "Summon Earth Elemental" }));
        }

        [TestCase("roe_11_city_hall", 5)]
        [TestCase("roe_12_unique_1", 6)]
        public void TheLibraryOfTheTowerAddsTheStoneSkinToTheFirstLevel(string save, int count)
        {
            var tower = Castle("Roe", save, "3_Tower");

            Assert.That(tower.SpellCounts[0], Is.EqualTo(count));
            Assert.That(Names(tower, 1), Has.Length.EqualTo(count));
            Assert.That(Names(tower, 1).Contains("Stone Skin"), Is.EqualTo(count == 6));
        }

        [Test]
        public void ACastleWithoutAMageGuildHoldsTheSpellsItGetsWhenItIsBuilt()
        {
            var dungeon = Castle("Roe", "roe_00_baseline", "6_Dung");

            Assert.That(dungeon.SpellCounts, Is.All.EqualTo(0));
            Assert.That(dungeon.MageGuildSpells.Select(level => level.Count), Is.EqualTo(new[] { 5, 4, 3, 2, 1 }));
            Assert.That(Names(dungeon, 1), Is.EqualTo(new[] { "View Air", "Summon Boat", "Dispel", "Magic Arrow", "View Earth" }));
        }

        [Test]
        public void ALevelThatTheFactionCannotBuildHoldsNoSpells()
        {
            var stronghold = Castle("Roe", "roe_46_mage_guild_5", "7_Strong");

            Assert.That(stronghold.MageGuildSpells.Select(level => level.Count), Is.EqualTo(new[] { 5, 4, 3, 0, 0 }));
        }

        [Test]
        public void BulwarkOfHornOfTheAbyssHoldsTheSpellsTheGameShows()
        {
            var bulwark = Castle(Path.Combine("Hota", "Bulwark"), "bulwark_33_mage_guild_5__nothing", "Jotunngard");

            Assert.That(Names(bulwark, 1), Is.EqualTo(new[] { "Protection from Water", "Magic Arrow", "View Air", "Bless", "Summon Boat" }));
            Assert.That(Names(bulwark, 2), Is.EqualTo(new[] { "Ice Bolt", "Disguise", "Remove Obstacle", "Lightning Bolt" }));
            Assert.That(Names(bulwark, 3), Is.EqualTo(new[] { "Air Shield", "Earthquake", "Anti Magic" }));
            Assert.That(Names(bulwark, 4), Is.EqualTo(new[] { "Slayer", "Sorrow" }));
            Assert.That(Names(bulwark, 5), Is.Empty);
        }

        [Test]
        public void EveryCastleOfEverySavefileOfTheTowerSeriesKeepsItsSpellsWhenSavedAgainUnchanged()
        {
            var castles = TestServices.GameService.LoadGameFromFile(Path.Combine("TestData", "Roe", "roe_46_mage_guild_5.GM1")).History.Current.Castles();

            foreach (var chunk in castles)
            {
                var model = TestServices.CastleDeserializer.Deserialize(chunk);
                var written = TestServices.SerializeCastle(chunk, model);

                Assert.That(written.ToArray(), Is.EqualTo(chunk.ToArray()), chunk.Name);
            }
        }
    }
}
