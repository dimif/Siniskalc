using Siniskalc.Homm3.Tests.Data;
using Moq;
using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Models;

namespace Siniskalc.Homm3.Tests.Integration.Data
{
    /// <summary>
    /// Finding the table of hero owners from the date of the game, and what it says in every savefile of the repository.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class HeroOwnersBytesLoaderTests
    {
        private const int Length = 156;
        private const int Date = 1000;

        private static IEnumerable<string> EverySavefile() => Directory.GetFiles("TestData", "*.GM?", SearchOption.AllDirectories).OrderBy(f => f, StringComparer.Ordinal);

        // A savefile of Shadow of Death with the table in front of the date at 1000: Blue owns hero 1, hero 2 is in a tavern.
        private static byte[] Savefile(byte tailSecond = HeroOwnersLayout.TailSecond, byte secondTable = 0xFF)
        {
            var bytes = new byte[Date + 50];
            var start = Date - HeroOwnersLayout.TailLength - (2 * Length);
            Array.Fill(bytes, (byte)0xFF, start, 2 * Length);
            bytes[start + 1] = 0x01;
            bytes[start + 2] = 0x40;
            bytes[start + Length + 5] = secondTable;
            bytes[Date - HeroOwnersLayout.TailLength] = HeroOwnersLayout.TailFirst;
            bytes[Date - HeroOwnersLayout.TailLength + 1] = tailSecond;
            return bytes;
        }

        private static HeroOwnersBytesLoader Loader()
        {
            var dates = new Mock<IGameDateLocator>();
            dates.Setup(d => d.FindOffset(It.IsAny<byte[]>(), It.IsAny<GameVersion>(), It.IsAny<int>())).Returns(Date);
            return new HeroOwnersBytesLoader(dates.Object);
        }

        [Test]
        public void TheTableEndsTwoTablesAndElevenBytesBeforeTheDate()
        {
            var chunks = Loader().LoadHeroOwnerChunks(Savefile(), GameVersion.ShadowOfDeath, 0);

            Assert.That(chunks, Has.Count.EqualTo(1));
            Assert.That(chunks[0].StartIndex, Is.EqualTo(Date - HeroOwnersLayout.TailLength - (2 * Length)));
            Assert.That(chunks[0].Length, Is.EqualTo(Length));
            Assert.That(chunks[0].GetByteAt(1), Is.EqualTo(0x01));
        }

        [Test]
        public void BytesThatAreNotTheTableGiveNoTable()
        {
            Assert.That(Loader().LoadHeroOwnerChunks(Savefile(tailSecond: 0x09), GameVersion.ShadowOfDeath, 0), Is.Empty);
            Assert.That(Loader().LoadHeroOwnerChunks(Savefile(secondTable: 0x00), GameVersion.ShadowOfDeath, 0), Is.Empty);
        }

        [Test]
        public void AVersionWhoseTableWasNotSeenGivesNoTable()
        {
            Assert.That(Loader().LoadHeroOwnerChunks(Savefile(), GameVersion.ArmageddonsBlade, 0), Is.Empty);
        }

        [Test]
        public void ASavefileWithNoDateGivesNoTable()
        {
            var dates = new Mock<IGameDateLocator>();
            dates.Setup(d => d.FindOffset(It.IsAny<byte[]>(), It.IsAny<GameVersion>(), It.IsAny<int>())).Returns(-1);

            Assert.That(new HeroOwnersBytesLoader(dates.Object).LoadHeroOwnerChunks(Savefile(), GameVersion.ShadowOfDeath, 0), Is.Empty);
        }

        [TestCase("hero_owners_00_baseline", 543453)]
        [TestCase("hero_owners_02_m4w2d1", 543492)]
        public void TheTableOfAHornOfTheAbyssSaveHasAByteForEach215HeroesInFrontOfTheDate(string save, int offset)
        {
            var owners = TestServices.GameService.LoadGameFromFile(Path.Combine("TestData", "Hota", "HeroOwners", save + ".GM2")).History.Current.HeroOwners();

            Assert.That(owners[0].StartIndex, Is.EqualTo(offset));
            Assert.That(owners[0].Length, Is.EqualTo(215));

            // Fiona (205) and hero 210 have bytes of their own.
            Assert.That(owners.GetModel(0).Heroes[210], Is.EqualTo(HeroOwnership.Reserved));
        }

        [TestCaseSource(nameof(EverySavefile))]
        public void EverySavefileHasATableThatAgreesWithTheHeroesAndThePlayers(string path)
        {
            var document = SavedGames.Get(path).History.Current;
            var owners = document.HeroOwners();
            Assert.That(owners, Has.Count.EqualTo(1), "the table is found");

            var table = owners.GetModel(0);
            var heroes = document.Heroes();
            for (var i = 0; i < heroes.Count; i++)
            {
                var hero = heroes.GetModel(i);
                Assert.That(table.Heroes[hero.Id].Owner, Is.EqualTo(hero.Owner), $"the owner of {hero.Name}");
            }

            var players = document.Players();
            Assert.That(players, Is.Not.Empty, $"{path}: the save contains players");
            var listed = new HashSet<int>();
            for (var p = 0; p < players.Count; p++)
            {
                var player = players.GetModel(p);
                Assert.That(player.HeroIds.Select(id => table.Heroes[id].Owner), Has.All.EqualTo(player.Color), $"the heroes of {player.Color}");
                Assert.That(player.Tavern.Ids.OfType<int>().Select(id => table.Heroes[id].IsReserved), Has.All.True, $"the tavern of {player.Color}");
                listed.UnionWith(player.HeroIds);
            }

            // A hero a player owns that is in no list is garrisoned in a castle.
            var unlisted = Enumerable.Range(0, table.Heroes.Count).Where(id => !table.Heroes[id].Owner.IsNeutral && !listed.Contains(id));
            Assert.That(unlisted.Select(document.Castles().FindByGarrisonedHero), Has.All.GreaterThanOrEqualTo(0), "the owned heroes in no list");
        }
    }
}
