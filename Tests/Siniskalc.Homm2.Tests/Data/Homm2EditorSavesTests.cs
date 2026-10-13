namespace Siniskalc.Homm2.Tests.Data
{
    using System.Runtime.CompilerServices;
    using System.Text;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// The second set of savegames for the write test with the game, made with the view models of the editor (TestData/Homm2/out, not in
    /// the repository): what they hold is checked here, and the explicit test <see cref="WriteTheSavesMadeWithTheEditor"/> writes them.
    /// </summary>
    [TestFixture]
    internal sealed class Homm2EditorSavesTests
    {
        private static IReadOnlyList<Homm2EditedSave> Saves { get; } = Homm2EditorSaves.Build();

        private static string ThisFile([CallerFilePath] string file = "") => file;

        private static Homm2EditedSave Named(string name) => Saves.Single(s => s.Name == name);

        private static SaveDocumentOf Open(string name) => new(Homm2TestServices.Parser.Parse(Named(name).Bytes, null).History.Current);

        [Test]
        public void EveryNameIsOneTheGameListsAndIsNotOneOfTheFirstSet()
        {
            var all = Saves.Select(s => s.Name).Concat(Homm2EditedSaves.Build().Select(s => s.Name)).ToList();

            Assert.That(all, Is.Unique);
            Assert.That(all, Is.All.Matches("^[A-Z][A-Z0-9]{0,7}$"));
            Assert.That(Saves.Select(s => s.Name), Does.Not.Contain("E13ARTSW"), "the user's own save keeps its name");
        }

        [Test]
        public void EverySaveIsTheSizeOfItsBaseLoadsAgainAndChangesOnlyTheRecordsOfPlayersHeroesAndCastles()
        {
            var original = File.ReadAllBytes(Homm2Saves.BrokenAllianceSave(24));
            var game = Homm2TestServices.LoadBrokenAlliance(24);
            var layout = game.Layout;
            var records = new List<(int Start, int End)>();
            records.AddRange(Enumerable.Range(0, Constants.Table.PlayerCount).Select(i => (layout.PlayerStart(i), layout.PlayerStart(i) + Homm2PlayerLayout.RecordSize)));
            records.AddRange(Enumerable.Range(0, Constants.Table.HeroCount).Select(i => (layout.HeroStart(i), layout.HeroStart(i) + layout.HeroStride)));
            records.AddRange(Enumerable.Range(0, Constants.Table.CastleSlotCount).Select(i => (layout.CastleStart(i), layout.CastleStart(i) + Homm2CastleLayout.RecordSize)));

            foreach (var save in Saves)
            {
                var differences = Enumerable.Range(0, original.Length).Where(i => original[i] != save.Bytes[i]).ToList();

                Assert.That(save.Bytes, Has.Length.EqualTo(original.Length), save.Name);
                Assert.That(() => Homm2TestServices.Parser.Parse(save.Bytes, null), Throws.Nothing, save.Name);
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
            Assert.That(give.Document.Castles().GetModel(corackston).Owner, Is.EqualTo(Homm2Color.Orange));
            Assert.That(give.Document.Players().GetModel((int)Homm2Color.Orange).CastleIds, Does.Contain(corackston));
            Assert.That(give.Document.Players().GetModel((int)Homm2Color.Blue).CastleIds, Does.Not.Contain(corackston));

            var last = Open("E15LAST").Document;
            Assert.That(last.Players().GetModel((int)Homm2Color.Red).CastleIds, Is.Empty);
            Assert.That(last.Players().GetModel((int)Homm2Color.Red).DaysLeft, Is.EqualTo(7));
        }

        [Test]
        public void WhatEachSaveSaysItChangedIsWhatIsInIt()
        {
            var xabran = Open("E16BLDG").Document.Castles().GetModel(Open("E16BLDG").Document.Castles().FindByName("Xabran"));
            Assert.That(xabran.Town.HasFlag(Homm2TownBuildings.Shipyard), Is.True);
            Assert.That(xabran.Town.HasFlag(Homm2TownBuildings.Statue) || xabran.Town.HasFlag(Homm2TownBuildings.Well), Is.False);

            var whiteshield = Open("E17DWEL").Document.Castles().GetModel(Open("E17DWEL").Document.Castles().FindByName("Whiteshield"));
            Assert.That(whiteshield.HasDwelling(4), Is.True);
            Assert.That(whiteshield.Creatures[3], Is.EqualTo(30));

            var guild = Open("E18GUILD").Document.Castles().GetModel(Open("E18GUILD").Document.Castles().FindByName("Xabran"));
            Assert.That(guild.MageGuildLevel, Is.EqualTo(2));
            Assert.That(guild.GuildSpell(1, 0), Is.EqualTo(Homm2Spell.Fireball));
            Assert.That(guild.GuildSpell(2, 0), Is.EqualTo(Homm2Spell.ChainLightning));

            var artifacts = Open("E19ARTS").Document.Heroes().GetModel(7);
            Assert.That(artifacts.Artifacts.IsFull, Is.True);
            Assert.That(artifacts.PrimarySkills.Attack, Is.GreaterThan(Homm2TestServices.LoadBrokenAlliance(24).History.Current.Heroes().GetModel(7).PrimarySkills.Attack + 12));

            var before = Homm2TestServices.LoadBrokenAlliance(24).History.Current.Heroes().GetModel(7);
            var taken = Open("E20TAKE").Document.Heroes().GetModel(7);
            Assert.That(taken.PrimarySkills, Is.EqualTo(before.PrimarySkills with { Defense = before.PrimarySkills.Defense + 12 }));
            Assert.That(taken.Artifacts.Slots, Has.Member(Homm2Artifact.UltimateCloakOfProtection));
            Assert.That(taken.Artifacts.Slots, Has.No.Member(Homm2Artifact.UltimateSwordOfDominion));

            var skills = Open("E21SKILL").Document.Heroes().GetModel(7).SecondarySkills;
            Assert.That(skills.Count, Is.EqualTo(8));
            Assert.That(skills.Entries.All(e => e.Level == Homm2SkillLevel.Expert), Is.True);

            var gallant = Open("E22BOOK").Document.Heroes().GetModel(1);
            Assert.That(gallant.HasSpellBook, Is.True);
            Assert.That(gallant.Spells.Contains(Homm2Spell.Fireball), Is.False);
            Assert.That(gallant.Spells.Contains(Homm2Spell.Cure), Is.True);

            var blue = Open("E23LIMIT").Document.Players().GetModel(0).Resources;
            Assert.That(blue, Is.EqualTo(new Homm2Resources(999, 999, 999, 999, 999, 999, 999_999)));

            var army = Open("E24STACK").Document.Heroes().GetModel(1).Army;
            Assert.That(army.Slots[0], Is.EqualTo(new Homm2ArmySlot(Homm2Creature.Peasant, 65_535)));
            Assert.That(army.Slots[1], Is.EqualTo(new Homm2ArmySlot(Homm2Creature.Rogue, 500)));
            Assert.That(army.Slots[2], Is.EqualTo(new Homm2ArmySlot(Homm2Creature.BlackDragon, 1)));

            var stats = Open("E25STATS").Document.Heroes().GetModel(1);
            Assert.That((stats.Level, stats.Experience, stats.SpellPoints, stats.MovementLeft), Is.EqualTo((20, 50_000L, 99, stats.MaxMovement)));
            Assert.That(stats.PrimarySkills, Is.EqualTo(new Homm2PrimarySkills(20, 20, 20, 20)));
        }

        [Test]
        public void TheSaveWithEverythingHoldsWhatTheOthersHold()
        {
            var all = Open("E26ALL").Document;

            Assert.That(all.Castles().GetModel(all.Castles().FindByName("Corackston")).Owner, Is.EqualTo(Homm2Color.Orange));
            Assert.That(all.Castles().GetModel(all.Castles().FindByName("Bloodreign")).Owner, Is.EqualTo(Homm2Color.Blue));
            Assert.That(all.Players().GetModel((int)Homm2Color.Red).DaysLeft, Is.EqualTo(7));
            Assert.That(all.Heroes().GetModel(7).SecondarySkills.Count, Is.EqualTo(8));
            Assert.That(all.Heroes().GetModel(7).Artifacts.IsFull, Is.True);
            Assert.That(all.Heroes().GetModel(1).Army.Slots[0]!.Count, Is.EqualTo(65_535));
            Assert.That(all.Players().GetModel(0).Resources.Gold, Is.EqualTo(999_999));
        }

        /// <summary>
        /// Writes the second set of savegames for the write test into TestData/Homm2/out with a README of their own
        /// (README_EDITOR.md), and leaves the first set and its README, which hold what was seen in the game, as they are. Run it by hand:
        /// <c>dotnet test Tests/Siniskalc.Homm2.Tests --filter Name=WriteTheSavesMadeWithTheEditor</c>.
        /// </summary>
        [Test]
        [Explicit("Writes TestData/Homm2/out for the write test with the game")]
        public void WriteTheSavesMadeWithTheEditor()
        {
            var root = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(ThisFile())!, "..", "..", ".."));
            var folder = Path.Combine(root, "TestData", "Homm2", "out");
            Directory.CreateDirectory(folder);

            var readme = new StringBuilder();
            readme.AppendLine("# Savegames made with the editor, for the write test with Heroes of Might and Magic II");
            readme.AppendLine();
            readme.AppendLine("Made by `Homm2EditorSavesTests.WriteTheSavesMadeWithTheEditor` (not in the repository). Unlike E00 to E13, which edit the models,");
            readme.AppendLine("these are made with the view models of the editor, the way a user edits. All start from `broken_alliance_24_castles_recaptured__whiteshield_green_tower`");
            readme.AppendLine("(`24_GREEN` in the game), so load that one first to see what each save changed. Copy them into the save folder of the game, load each one and");
            readme.AppendLine("write what you see in the last column. Before the first one, load E00COPY to be sure the game is happy with the base.");
            readme.AppendLine();
            readme.AppendLine("| Save | Edit | Look for | Test result |");
            readme.AppendLine("|---|---|---|---|");

            foreach (var save in Homm2EditorSaves.Build())
            {
                File.WriteAllBytes(Path.Combine(folder, save.Name + Homm2EditedSaves.Extension), save.Bytes);
                readme.AppendLine($"| {save.Name}{Homm2EditedSaves.Extension} | {save.Edit} | {save.Check} | |");
            }

            File.WriteAllText(Path.Combine(folder, "README_EDITOR.md"), readme.ToString());
            Assert.That(Directory.GetFiles(folder, "*.GM1").Length, Is.GreaterThanOrEqualTo(Homm2EditorSaves.Build().Count));
        }

        private sealed record SaveDocumentOf(Siniskalc.Core.Data.SaveDocument Document);
    }
}
