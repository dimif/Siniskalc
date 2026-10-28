using Siniskalc;
using Siniskalc.Core.Data;
using Siniskalc.Homm3.Data;
using Siniskalc.Heroes.Entities;
using Siniskalc.Heroes.Models;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Runtime;

namespace Siniskalc.Homm3.Tests.Integration.Data
{
    /// <summary>
    /// The savegame for the write test with the game that gives every player the most gold the editor allows (TestData/Homm3/out, not in the
    /// repository): what it holds is checked here, and the explicit test <see cref="WriteTheGoldLimitSave"/> writes it.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class GoldLimitSavesTests
    {
        private const string Name = "gold_limit";

        private static readonly string Source = Path.Combine("TestData", "Hota", "empire_of_the_world_01_m5w4d6.GM1");

        private static Game Load(string path) => TestServices.GameService.LoadGameFromFile(path);

        private static byte[] Build()
        {
            var game = Load(Source);
            for (var index = 0; index < game.History.Current.Players().Count; index++)
            {
                game.History.CreatePlayerSession(index).Update(p => p with { Resources = p.Resources with { Gold = Constants.Resources.MaxGold } });
            }

            return game.GetBytesToSave();
        }

        [Test]
        public void EveryPlayerHasTheMostGoldAndKeepsTheOtherResources()
        {
            var before = Load(Source).History.Current.Players().Select(c => TestServices.PlayerDeserializer.Deserialize(c).Resources).ToList();
            var path = Path.Combine(TestContext.CurrentContext.WorkDirectory, Name + ".GM1");
            TestServices.SavefileCodec.Write(path, Build());

            var after = Load(path).History.Current.Players().Select(c => TestServices.PlayerDeserializer.Deserialize(c).Resources).ToList();

            Assert.That(after, Is.EqualTo(before.Select(r => r with { Gold = int.MaxValue })));
        }

        /// <summary>
        /// Writes the savegame to TestData/Homm3/out (ignored by git), made from <c>empire_of_the_world_01_m5w4d6</c>, with a README of what to look
        /// at in the game. Run it by hand: <c>dotnet test Tests/Siniskalc.Homm3.Tests --filter Name=WriteTheGoldLimitSave</c>.
        /// </summary>
        [Test]
        [Explicit("Writes TestData/Homm3/out for the write test with the game")]
        [Category(TestKinds.Manual)]
        public void WriteTheGoldLimitSave()
        {
            var folder = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(ThisFile())!, "..", "..", "..", "..", "TestData", "Homm3", "out"));
            Directory.CreateDirectory(folder);

            TestServices.SavefileCodec.Write(Path.Combine(folder, Name + ".GM1"), Build());
            File.WriteAllText(
                Path.Combine(folder, "README.md"),
                $"# Gold at the limit of the editor{Environment.NewLine}{Environment.NewLine}" +
                $"`{Name}.GM1` is `empire_of_the_world_01_m5w4d6` with 2,147,483,647 gold for every player and the other resources as they were.{Environment.NewLine}" +
                $"Load it, look at how the screens show the gold, then end the turn: does the gold stay, or does the income make it negative?{Environment.NewLine}");
        }

        private static string ThisFile([System.Runtime.CompilerServices.CallerFilePath] string file = "") => file;
    }
}
