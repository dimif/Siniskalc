namespace Siniskalc.Homm2.Tests.Data
{
    using System.Runtime.CompilerServices;
    using System.Text;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;

    /// <summary>
    /// The third set of savegames for the write test with the game, made from Threedom (TestData/Homm2/out, not in the repository): what
    /// they hold is checked here, and the explicit test <see cref="WriteTheThreedomSavesMadeWithTheEditor"/> writes the missing ones.
    /// </summary>
    [TestFixture]
    internal sealed class ThreedomEditorSavesTests
    {
        private static IReadOnlyList<EditedSave> Built { get; } = ThreedomEditorSaves.Build();

        private static string ThisFile([CallerFilePath] string file = "") => file;

        private static Siniskalc.Homm2.Runtime.Game Open(string name) => TestServices.Parser.Parse(Built.Single(s => s.Name == name).Bytes, null);

        [Test]
        public void EveryNameIsOneTheGameListsAndIsNotOneOfTheOtherSets()
        {
            var all = Built.Select(s => s.Name).Concat(EditorSaves.Build().Select(s => s.Name)).Concat(EditedSaves.Build().Select(s => s.Name)).ToList();

            Assert.That(all, Is.Unique);
            Assert.That(all, Is.All.Matches("^[A-Z][A-Z0-9]{0,7}$"));
        }

        [Test]
        public void EverySaveLoadsAgainWithTheSizeOfThreedom()
        {
            var original = File.ReadAllBytes(Saves.ThreedomSave(71));

            foreach (var save in Built)
            {
                Assert.That(save.Bytes, Has.Length.EqualTo(original.Length), save.Name);
                Assert.That(() => TestServices.Parser.Parse(save.Bytes, null), Throws.Nothing, save.Name);
            }
        }

        [Test]
        public void TheBuildingsAreBuiltAndTakenDownAsTheEditSays()
        {
            var castles = Open("T01BLDS").History.Current.Castles();

            var whiteshield = castles.GetModel(castles.FindByName("Whiteshield")).Town;
            Assert.That(whiteshield.HasFlag(TownBuildings.ThievesGuild | TownBuildings.Marketplace | TownBuildings.LeftTurret | TownBuildings.RightTurret | TownBuildings.CaptainsQuarters), Is.True);
            var hillstone = castles.GetModel(castles.FindByName("Hillstone")).Town;
            Assert.That(hillstone & (TownBuildings.Statue | TownBuildings.Moat | TownBuildings.CaptainsQuarters), Is.EqualTo(TownBuildings.None));
            Assert.That(hillstone.HasFlag(TownBuildings.Marketplace | TownBuildings.Special), Is.True, "what was not edited stays");
        }

        [Test]
        public void TheHeroesForgetTheirPlacesAndHaveTheBonusesOfTheNextBattle()
        {
            var forgotten = Open("T02FORGT").History.Current.Heroes();
            Assert.That(forgotten.GetModel(50).Visited.Count, Is.EqualTo(0));
            Assert.That(forgotten.GetModel(49).Visited.Count, Is.EqualTo(0));

            var bonus = Open("T03BONUS").History.Current.Heroes();
            Assert.That(bonus.GetModel(50).NextBattle, Is.EqualTo(new Siniskalc.Homm2.Models.NextBattleBonus(3, 3)));
            Assert.That(bonus.GetModel(49).NextBattle, Is.EqualTo(new Siniskalc.Homm2.Models.NextBattleBonus(-3, -3)));
        }

        /// <summary>
        /// Writes the savegames of this set that are not in TestData/Homm2/out yet, and a README of their own for the ones it wrote.
        /// Run it by hand: <c>dotnet test Tests/Siniskalc.Homm2.Tests --filter Name=WriteTheThreedomSavesMadeWithTheEditor</c>.
        /// </summary>
        [Test]
        [Explicit("Writes TestData/Homm2/out for the write test with the game")]
        public void WriteTheThreedomSavesMadeWithTheEditor()
        {
            var root = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(ThisFile())!, "..", "..", ".."));
            var folder = Path.Combine(root, "TestData", "Homm2", "out");
            Directory.CreateDirectory(folder);

            var readme = new StringBuilder();
            readme.AppendLine("# Savegames made with the editor from Threedom, for the write test with Heroes of Might and Magic II (Price of Loyalty)");
            readme.AppendLine();
            readme.AppendLine("Made by `ThreedomEditorSavesTests.WriteTheThreedomSavesMadeWithTheEditor` (not in the repository).");
            readme.AppendLine("All start from `threedom_71_hillstone_captains_quarters_whiteshield_moat` (`71_BUILD.GX1` in the game). Copy them into the save folder of the game,");
            readme.AppendLine("load each one and write what you see in the last column.");
            readme.AppendLine();
            readme.AppendLine("| Save | Edit | Look for | Test result |");
            readme.AppendLine("|---|---|---|---|");

            var written = 0;
            foreach (var save in ThreedomEditorSaves.Build())
            {
                var path = Path.Combine(folder, save.Name + ".GX1");
                if (File.Exists(path))
                {
                    continue;
                }

                File.WriteAllBytes(path, save.Bytes);
                readme.AppendLine($"| {save.Name}.GX1 | {save.Edit} | {save.Check} | |");
                written++;
            }

            if (written > 0)
            {
                File.WriteAllText(Path.Combine(folder, $"README_EDITOR_new_{DateTime.Now:yyyyMMdd_HHmmss}.md"), readme.ToString());
            }

            Assert.That(Directory.GetFiles(folder, "T0*.GX1").Length, Is.GreaterThanOrEqualTo(Built.Count));
        }
    }
}
