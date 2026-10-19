namespace Siniskalc.Homm2.Tests.Data
{
    using System.Runtime.CompilerServices;
    using System.Text;
    using Siniskalc.Core.Models;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// The savegames made for the write test with the game (TestData/Homm2/out, not in the repository): what they hold is checked here, and
    /// the explicit test <see cref="WriteTheSavesForTheGame"/> writes them.
    /// </summary>
    [TestFixture]
    internal sealed class EditedSavesTests
    {
        private static IReadOnlyList<EditedSave> Built { get; } = EditedSaves.Build();

        private static string ThisFile([CallerFilePath] string file = "") => file;

        private static EditedSave Named(string name) => Built.Single(s => s.Name == name);

        private static IReadOnlyList<int> Differences(EditedSave save) => Differences(File.ReadAllBytes(Saves.BrokenAllianceSave(save.BaseSave)), save.Bytes);

        private static IReadOnlyList<int> Differences(byte[] before, byte[] after) => Enumerable.Range(0, before.Length).Where(i => before[i] != after[i]).ToList();

        [Test]
        public void EveryNameIsOneTheGameListsInUpperCaseOfAtMostEightCharacters()
        {
            Assert.That(Built.Select(s => s.Name), Is.Unique);
            Assert.That(Built.Select(s => s.Name), Is.All.Matches("^[A-Z][A-Z0-9]{0,7}$"));
        }

        [Test]
        public void EverySaveIsTheSizeOfItsBaseAndLoadsAgain()
        {
            foreach (var save in Built)
            {
                var original = File.ReadAllBytes(Saves.BrokenAllianceSave(save.BaseSave));

                Assert.That(save.Bytes, Has.Length.EqualTo(original.Length), save.Name);
                Assert.That(() => TestServices.Parser.Parse(save.Bytes, null), Throws.Nothing, save.Name);
            }
        }

        [Test]
        public void TheControlIsTheOriginalSaveByteForByte()
        {
            Assert.That(Differences(Named("E00COPY")), Is.Empty);
        }

        [Test]
        public void EveryOtherSaveDiffersFromItsBaseAndOnlyInTheRecordsOfWhatWasEdited()
        {
            foreach (var save in Built.Where(s => s.Name != "E00COPY"))
            {
                var differences = Differences(save);
                var game = TestServices.LoadBrokenAlliance(save.BaseSave);
                var layout = game.Layout;
                var records = new List<(int Start, int End)>();
                records.AddRange(Enumerable.Range(0, Constants.Table.PlayerCount).Select(i => (layout.PlayerStart(i), layout.PlayerStart(i) + PlayerLayout.RecordSize)));
                records.AddRange(Enumerable.Range(0, Constants.Table.HeroCount).Select(i => (layout.HeroStart(i), layout.HeroStart(i) + layout.HeroStride)));
                records.AddRange(Enumerable.Range(0, Constants.Table.CastleSlotCount).Select(i => (layout.CastleStart(i), layout.CastleStart(i) + CastleLayout.RecordSize)));

                Assert.That(differences, Is.Not.Empty, save.Name);
                Assert.That(differences.All(d => records.Any(r => d >= r.Start && d < r.End)), Is.True, save.Name);
            }
        }

        [Test]
        public void TheGoldSaveChangesOnlyTheBytesOfTheGold()
        {
            var differences = Differences(Named("E01GOLD"));

            var goldAt = TestServices.LoadBrokenAlliance(5).Layout.PlayerStart(0) + PlayerLayout.Resources + (6 * 4);
            Assert.That(differences, Is.Not.Empty.And.All.InRange(goldAt, goldAt + 3));
        }

        [Test]
        public void WhatEachSaveSaysItChangedIsWhatIsInIt()
        {
            HeroModel Maximus(string name) => TestServices.Parser.Parse(Named(name).Bytes, null).History.Current.Heroes().GetModel(7);

            Assert.That(TestServices.Parser.Parse(Named("E01GOLD").Bytes, null).History.Current.Players().GetModel(0).Resources.Gold, Is.EqualTo(99_999));
            Assert.That(TestServices.Parser.Parse(Named("E02RESRC").Bytes, null).History.Current.Players().GetModel(0).Resources, Is.EqualTo(new ResourceSet(500, 500, 500, 500, 500, 500, 50_000)));
            Assert.That(Maximus("E03ARMY").Army.Slots.Select(s => s!.Creature.Name), Is.EqualTo(new[] { "Black Dragon", "Titan", "Phoenix", "Paladin", "Peasant" }));
            Assert.That(Maximus("E04SKILL").PrimarySkills, Is.EqualTo(new PrimarySkills(15, 15, 15, 15)));
            Assert.That(Maximus("E05SKL2").SecondarySkills.SlotCount, Is.EqualTo(8));
            Assert.That(Maximus("E06ARTI").Artifacts.Slots.Count(a => a != null), Is.EqualTo(6));
            Assert.That(Maximus("E06ARTI").HasSpellBook, Is.True);
            Assert.That(Maximus("E07BOOK").HasSpellBook, Is.True);
            Assert.That(Maximus("E07BOOK").Spells.Count, Is.EqualTo(65));
            Assert.That((Maximus("E08LEVEL").HeroLevel.Level, Maximus("E08LEVEL").HeroLevel.Experience, Maximus("E08LEVEL").MovementPoints.Current), Is.EqualTo((10, 14_700L, 1000L)));

            var castle = TestServices.Parser.Parse(Named("E09CAST").Bytes, null).History.Current.Castles().GetModel(3);
            Assert.That(castle.HasDwelling(4), Is.True);
            Assert.That(castle.CreaturesAvailable, Is.EqualTo(Numbers.Of(20, 20, 20, 20, 20, 20)));
            Assert.That(TestServices.Parser.Parse(Named("E10GARR").Bytes, null).History.Current.Castles().GetModel(3).Garrison.Slots.Count(s => s != null), Is.EqualTo(3));

            var owned = TestServices.Parser.Parse(Named("E11OWNR").Bytes, null).History.Current;
            Assert.That(owned.Castles().GetModel(2).Owner, Is.EqualTo(PlayerColor.Blue));
            Assert.That(owned.Players().GetModel(0).CastleIds, Is.EqualTo(Numbers.Of(3, 2)));
        }

        /// <summary>
        /// Writes the savegames for the write test into TestData/Homm2/out with a README that says what to look at in the game. Run it
        /// by hand: <c>dotnet test Tests/Siniskalc.Homm2.Tests --filter Name=WriteTheSavesForTheGame</c>.
        /// </summary>
        [Test]
        [Explicit("Writes TestData/Homm2/out for the write test with the game")]
        public void WriteTheSavesForTheGame()
        {
            var root = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(ThisFile())!, "..", "..", ".."));
            var folder = Path.Combine(root, "TestData", "Homm2", "out");
            Directory.CreateDirectory(folder);

            var readme = new StringBuilder();
            readme.AppendLine("# Savegames for the write test with Heroes of Might and Magic II");
            readme.AppendLine();
            readme.AppendLine("Made by `EditedSavesTests.WriteTheSavesForTheGame` (not in the repository). Copy them into the save folder of the game,");
            readme.AppendLine("start HoMM2, load each one and tick what is told here. The names have at most 8 characters, which is what the game lists.");
            readme.AppendLine("Each save is an edit of one of the saves of Broken Alliance (the third column), so load the original first if the display is in doubt.");
            readme.AppendLine();
            readme.AppendLine("| Save | Edit | From | Look for |");
            readme.AppendLine("|---|---|---|---|");

            foreach (var save in EditedSaves.Build())
            {
                File.WriteAllBytes(Path.Combine(folder, save.Name + EditedSaves.Extension), save.Bytes);
                readme.AppendLine($"| {save.Name}{EditedSaves.Extension} | {save.Edit} | save {save.BaseSave:00} | {save.Check} |");
            }

            File.WriteAllText(Path.Combine(folder, "README.md"), readme.ToString());
            Assert.That(Directory.GetFiles(folder, "*.GM1"), Has.Length.EqualTo(EditedSaves.Build().Count));
        }
    }
}
