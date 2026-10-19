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
        public void TheObelisksAreAllVisitedByBlueOnlyOneFortIsForgottenAndTheSourcesSetTheirBonus()
        {
            var obelisks = Open("T04OBELS").History.Current.Obelisks().GetModel()!;
            Assert.That(obelisks.AllVisited(PlayerColor.Blue), Is.True);
            Assert.That(obelisks.IsVisited(3, PlayerColor.Green), Is.True, "the others keep theirs");

            Assert.That(Open("T05FORT").History.Current.Heroes().GetModel(50).Visited.Forts, Is.EqualTo(34u), "he had visited three forts and the second of the map is forgotten");

            var heroes = Open("T06SRC").History.Current.Heroes();
            Assert.That(heroes.GetModel(50).NextBattle, Is.EqualTo(new Siniskalc.Homm2.Models.NextBattleBonus(2, 1)));
            Assert.That(heroes.GetModel(50).Effects.HasFlag(HeroEffects.Temple | HeroEffects.Fountain), Is.True);
            Assert.That(heroes.GetModel(49).Effects & (HeroEffects.Arena | HeroEffects.Stables), Is.EqualTo(HeroEffects.None));
        }

        [Test]
        public void TheMinesTheShrinesAndTheSphinxAreEditedAsTheEditSays()
        {
            var mines = Open("T07MINES").History.Current.Mines().GetModels();
            Assert.That(mines.Single(m => m.Position == new Siniskalc.Heroes.Models.MapPosition(22, 29)).Owner, Is.EqualTo(Siniskalc.Homm2.Entities.PlayerColor.Blue));
            Assert.That(mines.Single(m => m.Position == new Siniskalc.Heroes.Models.MapPosition(44, 6)).Owner, Is.EqualTo(Siniskalc.Homm2.Entities.PlayerColor.Red));

            var shrines = Open("T08SHRIN").History.Current.Shrines().GetModels();
            Assert.That(shrines.Single(s => s.Position == new Siniskalc.Heroes.Models.MapPosition(45, 7)).Spell, Is.EqualTo(Siniskalc.Homm2.Entities.Spell.Fireball));

            var sphinx = Open("T09SPHNX").History.Current.Sphinxes().GetModel(0);
            Assert.That(sphinx.IsActive, Is.True);
            Assert.That(sphinx.Answers, Does.Contain("shade"));
            Assert.That(sphinx.Reward, Is.EqualTo(new[] { 0, 0, 0, 0, 0, 5, 5000 }));
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
