using Siniskalc.Runtime;

namespace Siniskalc.Tests.Data
{
    [TestFixture]
    internal sealed class MapHeaderTests
    {
        private static Game Load(string folder, string name) => TestServices.GameService.LoadGameFromFile(Path.Combine("TestData", folder, name + ".GM1"));

        [TestCase("Hota", "empire_of_the_world_01_m5w4d6", "The empire of the world")]
        [TestCase("Hota", "map_difficulty_edited_to_expert", "Adventures of Jared Haret")]
        [TestCase("Sod", "sod_game_00_baseline", "Arrogance")]
        public void TheMapNameIsRead(string folder, string save, string expected)
        {
            Assert.That(Load(folder, save).Map?.Name.Trim(), Is.EqualTo(expected));
        }

        [Test]
        public void TheMapDescriptionIsRead()
        {
            Assert.That(Load("Hota", "empire_of_the_world_01_m5w4d6").Map?.Description, Does.StartWith("You are just a simple man in the Roman empire."));
            Assert.That(Load("Sod", "sod_game_00_baseline").Map?.Description, Does.StartWith("Once, centuries ago"));
        }

        [TestCase("Hota", "empire_of_the_world_01_m5w4d6", 144, "Extra large (144 x 144)", true)]
        [TestCase("Hota", "bios_custom", 72, "Medium (72 x 72)", false)]
        [TestCase("Hota", "map_difficulty_edited_to_expert", 36, "Small (36 x 36)", true)]
        [TestCase("Sod", "sod_game_00_baseline", 36, "Small (36 x 36)", true)]
        public void TheMapSizeAndLevelsAreRead(string folder, string save, int size, string sizeText, bool underground)
        {
            var map = Load(folder, save).Map ?? throw new AssertionException("No map header.");

            Assert.That(map.Size, Is.EqualTo(size));
            Assert.That(map.SizeText, Is.EqualTo(sizeText));
            Assert.That(map.HasUnderground, Is.EqualTo(underground));
        }
    }
}
