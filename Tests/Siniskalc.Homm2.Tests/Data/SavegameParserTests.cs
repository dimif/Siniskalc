namespace Siniskalc.Homm2.Tests.Data
{
    using Siniskalc.Core.Runtime;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    [TestFixture]
    internal sealed class SavegameParserTests
    {
        [Test]
        public void EverySavegameLoadsAndTheHeaderSaysWhatTheMapIs()
        {
            foreach (var path in Saves.AllOfBrokenAlliance())
            {
                var game = TestServices.Load(path);

                Assert.That(game.FileName, Is.EqualTo(path));
                Assert.That(game.GameVersion, Is.EqualTo(GameVersion.BaseGame), path);
                Assert.That(game.Map, Is.EqualTo(new MapHeader("Broken Alliance", game.Map.Description, GameDifficulty.Knight, 72)), path);
                Assert.That(game.Map.Description, Does.StartWith("The alliance has crumbled"), path);
                Assert.That(game.History.Current.Heroes(), Has.Count.EqualTo(54), path);
                Assert.That(game.History.Current.Players(), Has.Count.EqualTo(6), path);
                Assert.That(game.History.Current.Castles(), Has.Count.EqualTo(72), path);
                Assert.That(game.History.CanUndo, Is.False, path);
            }
        }

        [Test]
        public void ThePriceOfLoyaltySavegameLoadsWithItsOwnHeroesAndCastles()
        {
            var game = TestServices.Load(Saves.Path(Saves.ArraxTheJerk, "arrax_the_jerk_01_start"));
            var document = game.History.Current;

            Assert.That(game.GameVersion, Is.EqualTo(GameVersion.PriceOfLoyalty));
            Assert.That(game.Map.Name, Is.EqualTo("Arrax the Jerk"));
            Assert.That(game.Map.Size, Is.EqualTo(36));
            Assert.That(document.Heroes().GetName(16), Is.EqualTo("Bikko"));
            Assert.That(document.Heroes().GetName(17), Is.EqualTo("King Anthrax"));
            Assert.That(document.Heroes().GetOwners().Select((owner, index) => (owner, index)).Where(x => !x.owner.IsNeutral).Select(x => x.index), Is.EqualTo(new[] { 16, 17 }));

            var bikko = document.Heroes().GetModel(16);
            Assert.That(bikko.Owner, Is.EqualTo(PlayerColor.Blue));
            Assert.That(bikko.Faction, Is.EqualTo(Faction.Barbarian));
            Assert.That((bikko.Position.X, bikko.Position.Y), Is.EqualTo((17, 12)));
            Assert.That(bikko.Army.Slots.Take(2).Select(s => s!.Creature), Is.EqualTo(new[] { Creature.Goblin, Creature.Orc }));

            var blue = document.Players().GetModel(0);
            Assert.That(blue.Resources, Is.EqualTo(ResourceSet.None));
            Assert.That(blue.HeroIds, Is.EqualTo(new[] { 16 }));
            Assert.That(document.Players().GetModel(1).Resources, Is.EqualTo(new ResourceSet(30, 10, 30, 10, 10, 10, 10000)));

            var xabran = document.Castles().GetModel(2);
            Assert.That(xabran.Name, Is.EqualTo("Xabran"));
            Assert.That(xabran.Owner, Is.EqualTo(PlayerColor.Blue));
            Assert.That(xabran.Faction, Is.EqualTo(Faction.Barbarian));
            Assert.That((xabran.Position.X, xabran.Position.Y), Is.EqualTo((17, 12)));
            Assert.That(document.Castles().UsedIndexes(), Is.EqualTo(new[] { 0, 1, 2, 3, 4 }));
        }

        [Test]
        public void TheDateOfEachSavegameIsRead()
        {
            Assert.That(TestServices.LoadBrokenAlliance(1).Date, Is.EqualTo(new GameDate(1, 1, 1)));
            Assert.That(TestServices.LoadBrokenAlliance(6).Date, Is.EqualTo(new GameDate(1, 1, 2)));
            Assert.That(TestServices.LoadBrokenAlliance(12).Date, Is.EqualTo(new GameDate(1, 2, 1)));
            Assert.That(TestServices.LoadBrokenAlliance(21).Date, Is.EqualTo(new GameDate(1, 4, 6)));
            Assert.That(TestServices.LoadBrokenAlliance(22).Date, Is.EqualTo(new GameDate(2, 1, 2)));
            Assert.That(TestServices.LoadBrokenAlliance(22).Date.ToString(), Is.EqualTo("Month 2, Week 1, Day 2"));
        }

        [Test]
        public void AFileThatIsNoHeroes2SavegameIsRefusedWithAMessageForTheUser()
        {
            var gzip = new byte[400_000];
            gzip[0] = 0x1F;
            gzip[1] = 0x8B;

            var thrown = Assert.Throws<UnsupportedSavefileException>(() => TestServices.Parser.Parse(gzip, "other.GM1"));

            Assert.That(thrown!.Message, Does.Contain("Heroes of Might and Magic II"));
        }

        [Test]
        public void TheBytesToSaveAreTheBytesLoadedWhileNothingIsEdited()
        {
            foreach (var path in Saves.All())
            {
                var game = TestServices.Load(path);

                Assert.That(game.GetBytesToSave(), Is.EqualTo(File.ReadAllBytes(path)), path);
            }
        }
    }
}
