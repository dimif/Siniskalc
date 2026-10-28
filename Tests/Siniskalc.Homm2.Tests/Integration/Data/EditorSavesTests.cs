namespace Siniskalc.Homm2.Tests.Integration.Data
{
    using System.Runtime.CompilerServices;
    using System.Text;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Core.Models;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// The second set of savegames for the write test with the game, made with the view models of the editor (TestData/Homm2/out, not in
    /// the repository): what they hold is checked here, and the explicit test <see cref="WriteTheSavesMadeWithTheEditor"/> writes them.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class EditorSavesTests
    {
        private static IReadOnlyList<EditedSave> Built { get; } = EditorSaves.Build();

        private static string ThisFile([CallerFilePath] string file = "") => file;

        private static EditedSave Named(string name) => Built.Single(s => s.Name == name);

        private static SaveDocumentOf Open(string name) => new(TestServices.Parser.Parse(Named(name).Bytes, null).History.Current);

        [Test]
        public void EveryNameIsOneTheGameListsAndIsNotOneOfTheFirstSet()
        {
            var all = Built.Select(s => s.Name).Concat(EditedSaves.Build().Select(s => s.Name)).ToList();

            Assert.That(all, Is.Unique);
            Assert.That(all, Is.All.Matches("^[A-Z][A-Z0-9]{0,7}$"));
            Assert.That(Built.Select(s => s.Name), Does.Not.Contain("E13ARTSW"), "the user's own save keeps its name");
        }

        [Test]
        public void EverySaveIsTheSizeOfItsBaseLoadsAgainAndChangesOnlyTheRecordsOfPlayersHeroesAndCastles()
        {
            var original = File.ReadAllBytes(Saves.BrokenAllianceSave(24));
            var game = TestServices.LoadBrokenAlliance(24);
            var layout = game.Layout;
            var records = new List<(int Start, int End)>();
            records.AddRange(Enumerable.Range(0, Constants.Table.PlayerCount).Select(i => (layout.PlayerStart(i), layout.PlayerStart(i) + PlayerLayout.RecordSize)));
            records.AddRange(Enumerable.Range(0, Constants.Table.HeroCount).Select(i => (layout.HeroStart(i), layout.HeroStart(i) + layout.HeroStride)));
            records.AddRange(Enumerable.Range(0, Constants.Table.CastleSlotCount).Select(i => (layout.CastleStart(i), layout.CastleStart(i) + CastleLayout.RecordSize)));

            foreach (var save in Built)
            {
                var differences = Enumerable.Range(0, original.Length).Where(i => original[i] != save.Bytes[i]).ToList();

                Assert.That(save.Bytes, Has.Length.EqualTo(original.Length), save.Name);
                Assert.That(() => TestServices.Parser.Parse(save.Bytes, null), Throws.Nothing, save.Name);
                Assert.That(differences, Is.Not.Empty, save.Name);
                Assert.That(differences.All(d => records.Any(r => d >= r.Start && d < r.End)), Is.True, save.Name);
                Assert.That(save.Check, Is.Not.Empty, save.Name);
            }
        }

        [Test]
        public void ACastleThatChangesHandsIsInTheListsOfTheNewOwnerAndOutOfTheOldOnes()
        {
            var give = Open("E14GIVE");
            var corackston = give.Document.Castles().FindByName("Corackston");
            Assert.That(give.Document.Castles().GetModel(corackston).Owner, Is.EqualTo(PlayerColor.Orange));
            Assert.That(give.Document.Players().GetModel(PlayerColor.Orange.Code).CastleIds, Does.Contain(corackston));
            Assert.That(give.Document.Players().GetModel(PlayerColor.Blue.Code).CastleIds, Does.Not.Contain(corackston));

            var last = Open("E15LAST").Document;
            Assert.That(last.Players().GetModel(PlayerColor.Red.Code).CastleIds, Is.Empty);
            Assert.That(last.Players().GetModel(PlayerColor.Red.Code).DaysLeft, Is.EqualTo(7));
        }

        [Test]
        public void WhatEachSaveSaysItChangedIsWhatIsInIt()
        {
            var xabran = Open("E16BLDG").Document.Castles().GetModel(Open("E16BLDG").Document.Castles().FindByName("Xabran"));
            Assert.That(xabran.Town.HasFlag(TownBuildings.Shipyard), Is.True);
            Assert.That(xabran.Town.HasFlag(TownBuildings.Statue) || xabran.Town.HasFlag(TownBuildings.Well), Is.False);

            var whiteshield = Open("E17DWEL").Document.Castles().GetModel(Open("E17DWEL").Document.Castles().FindByName("Whiteshield"));
            Assert.That(whiteshield.DwellingLevel(4), Is.EqualTo(1), "the Minotaur King went back to the Minotaur");
            Assert.That(whiteshield.DwellingLevel(6), Is.EqualTo(2), "the Green Dragon became the Red Dragon");
            Assert.That(whiteshield.CreaturesAvailable[3], Is.EqualTo(30));

            var upgraded = Open("E27DWUPG").Document;
            Assert.That(Enumerable.Range(1, 6).Select(d => upgraded.Castles().GetModel(upgraded.Castles().FindByName("Xabran")).DwellingLevel(d)), Is.EqualTo(new[] { 1, 2, 1, 2, 1, 2 }));
            Assert.That(upgraded.Castles().GetModel(upgraded.Castles().FindByName("Whiteshield")).DwellingLevel(6), Is.EqualTo(3));

            var five = Open("E28GLD5").Document.Castles().GetModel(Open("E28GLD5").Document.Castles().FindByName("Xabran"));
            Assert.That((five.MageGuildLevel, five.MageGuildSpellCounts), Is.EqualTo((5, Numbers.Of(3, 3, 2, 2, 1))));

            var library = Open("E29LIBRY").Document.Castles().GetModel(Open("E29LIBRY").Document.Castles().FindByName("Corackston"));
            Assert.That((library.HasLibrary, library.MageGuildLevel, library.MageGuildSpellCounts), Is.EqualTo((true, 3, Numbers.Of(4, 4, 3, 0, 0))));

            Assert.That(Open("E30XP50K").Document.Heroes().GetModel(1), Has.Property("HeroLevel").EqualTo(new HeroLevel(50_000L, 1)));
            Assert.That(Open("E31LV20").Document.Heroes().GetModel(1), Has.Property("HeroLevel").EqualTo(new HeroLevel(0L, 20)));
            Assert.That(Open("E32BIGXP").Document.Heroes().GetModel(1).HeroLevel.Experience, Is.EqualTo(200_000L));
            Assert.That(Open("E33STACK").Document.Heroes().GetModel(1).Army.Slots.Select(s => s?.Count), Is.EqualTo(new int?[] { 32_767, 9_999, 1, null, null }));

            var guild = Open("E18GUILD").Document.Castles().GetModel(Open("E18GUILD").Document.Castles().FindByName("Xabran"));
            Assert.That(guild.MageGuildLevel, Is.EqualTo(2));
            Assert.That(guild.MageGuildSpell(1, 0), Is.EqualTo(Spell.Fireball));
            Assert.That(guild.MageGuildSpell(2, 0), Is.EqualTo(Spell.ChainLightning));

            var artifacts = Open("E19ARTS").Document.Heroes().GetModel(7);
            Assert.That(artifacts.Artifacts.IsFull, Is.True);
            Assert.That(artifacts.PrimarySkills.Attack, Is.GreaterThan(TestServices.LoadBrokenAlliance(24).History.Current.Heroes().GetModel(7).PrimarySkills.Attack + 12));

            var before = TestServices.LoadBrokenAlliance(24).History.Current.Heroes().GetModel(7);
            var taken = Open("E20TAKE").Document.Heroes().GetModel(7);
            Assert.That(taken.PrimarySkills, Is.EqualTo(before.PrimarySkills with { Defense = before.PrimarySkills.Defense + 12 }));
            Assert.That(taken.Artifacts.Slots, Has.Member(Artifact.UltimateCloakOfProtection));
            Assert.That(taken.Artifacts.Slots, Has.No.Member(Artifact.UltimateSwordOfDominion));

            var skills = Open("E21SKILL").Document.Heroes().GetModel(7).SecondarySkills;
            Assert.That(skills.SlotCount, Is.EqualTo(8));
            Assert.That(skills.Entries.All(e => e.Level == SkillLevel.Expert), Is.True);

            var gallant = Open("E22BOOK").Document.Heroes().GetModel(1);
            Assert.That(gallant.HasSpellBook, Is.True);
            Assert.That(gallant.Spells.Contains(Spell.Fireball), Is.False);
            Assert.That(gallant.Spells.Contains(Spell.Cure), Is.True);

            var blue = Open("E23LIMIT").Document.Players().GetModel(0).Resources;
            Assert.That(blue, Is.EqualTo(new ResourceSet(9_999, 9_999, 9_999, 9_999, 9_999, 9_999, Constants.Player.MaxGold)));

            var army = Open("E24STACK").Document.Heroes().GetModel(1).Army;
            Assert.That(army.Slots[0], Is.EqualTo(new ArmySlot<Creature>(Creature.Peasant, (int)Constants.MaxCreatureCount)));
            Assert.That(army.Slots[1], Is.EqualTo(new ArmySlot<Creature>(Creature.Rogue, 500)));
            Assert.That(army.Slots[2], Is.EqualTo(new ArmySlot<Creature>(Creature.BlackDragon, 1)));

            var stats = Open("E25STATS").Document.Heroes().GetModel(1);
            Assert.That((stats.HeroLevel.Level, stats.HeroLevel.Experience, stats.SpellPoints, stats.MovementPoints.Current), Is.EqualTo((17, 50_000L, 99, stats.MovementPoints.Max)), "the level follows the experience");
            Assert.That(stats.PrimarySkills, Is.EqualTo(new PrimarySkills(20, 20, 20, 20)));
        }

        [Test]
        public void TheSaveWithEverythingHoldsWhatTheOthersHold()
        {
            var all = Open("E26ALL").Document;

            Assert.That(all.Castles().GetModel(all.Castles().FindByName("Corackston")).Owner, Is.EqualTo(PlayerColor.Orange));
            Assert.That(all.Castles().GetModel(all.Castles().FindByName("Bloodreign")).Owner, Is.EqualTo(PlayerColor.Blue));
            Assert.That(all.Players().GetModel(PlayerColor.Red.Code).DaysLeft, Is.EqualTo(7));
            Assert.That(all.Heroes().GetModel(7).SecondarySkills.SlotCount, Is.EqualTo(8));
            Assert.That(all.Heroes().GetModel(7).Artifacts.IsFull, Is.True);
            Assert.That(all.Heroes().GetModel(1).Army.Slots[0]!.Count, Is.EqualTo((int)Constants.MaxCreatureCount));
            Assert.That(all.Players().GetModel(0).Resources.Gold, Is.EqualTo(Constants.Player.MaxGold));
        }

        [Test]
        public void TheGoldLimitSaveHoldsTheMostGoldAndLeavesTheOtherResourcesAlone()
        {
            var before = new SaveDocumentOf(TestServices.LoadBrokenAlliance(24).History.Current).Document.Players().GetModel(0).Resources;
            var after = Open("E39GOLDX").Document.Players().GetModel(0).Resources;

            Assert.That(after, Is.EqualTo(before with { Gold = Constants.Player.MaxGold }));
            Assert.That(after.Gold, Is.EqualTo(int.MaxValue));
        }

        /// <summary>
        /// Writes the savegames of the second set that are not in TestData/Homm2/out yet, and a README of their own
        /// (README_EDITOR_new_*.md) for the ones it wrote. What is there is left as it is, the saves that were tried in the game and the READMEs
        /// that hold what was seen, even if the code that made them has changed since. Run it by hand:
        /// <c>dotnet test Tests/Siniskalc.Homm2.Tests --filter Name=WriteTheSavesMadeWithTheEditor</c>.
        /// </summary>
        [Test]
        [Explicit("Writes TestData/Homm2/out for the write test with the game")]
        [Category(TestKinds.Manual)]
        public void WriteTheSavesMadeWithTheEditor()
        {
            var root = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(ThisFile())!, "..", "..", "..", ".."));
            var folder = Path.Combine(root, "TestData", "Homm2", "out");
            Directory.CreateDirectory(folder);

            var readme = new StringBuilder();
            readme.AppendLine("# More savegames made with the editor, for the write test with Heroes of Might and Magic II");
            readme.AppendLine();
            readme.AppendLine("Made by `EditorSavesTests.WriteTheSavesMadeWithTheEditor` (not in the repository), after the results of E14 to E26 (README_EDITOR.md).");
            readme.AppendLine("All start from `broken_alliance_24_castles_recaptured__whiteshield_green_tower` (`24_GREEN` in the game). Copy them into the save folder of the game,");
            readme.AppendLine("load each one and write what you see in the last column.");
            readme.AppendLine();
            readme.AppendLine("| Save | Edit | Look for | Test result |");
            readme.AppendLine("|---|---|---|---|");

            var written = 0;
            foreach (var save in EditorSaves.Build())
            {
                var path = Path.Combine(folder, save.Name + EditedSaves.Extension);
                if (File.Exists(path))
                {
                    continue;
                }

                File.WriteAllBytes(path, save.Bytes);
                readme.AppendLine($"| {save.Name}{EditedSaves.Extension} | {save.Edit} | {save.Check} | |");
                written++;
            }

            if (written > 0)
            {
                File.WriteAllText(Path.Combine(folder, $"README_EDITOR_new_{DateTime.Now:yyyyMMdd_HHmmss}.md"), readme.ToString()); // never over a README that holds results
            }

            Assert.That(Directory.GetFiles(folder, "*.GM1").Length, Is.GreaterThanOrEqualTo(EditorSaves.Build().Count));
        }

        private sealed record SaveDocumentOf(Siniskalc.Core.Data.SaveDocument Document);
    }
}
