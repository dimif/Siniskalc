using System.Text;
using Siniskalc;
using Siniskalc.Data;
using Siniskalc.Entities;
using Siniskalc.Models;
using Siniskalc.Runtime;

namespace Siniskalc.Tests.Data
{
    /// <summary>
    /// Tests against two real Shadow of Death savefiles (from the complete edition) of the same game.
    /// In sod_game_00_baseline Red is the player, with Marius as the only hero. In sod_game_01_red_gives_blue_resources Red has given resources to Blue.
    /// </summary>
    [TestFixture]
    internal sealed class SodSaveTests
    {
        private static readonly string Save1 = Path.Combine("TestData", "Sod", "sod_game_00_baseline.GM1");
        private static readonly string Save2 = Path.Combine("TestData", "Sod", "sod_game_01_red_gives_blue_resources.GM1");

        private static PlayerResources RedBefore => new(20, 10, 20, 10, 10, 10, 20000);

        private static PlayerResources RedAfter => new(19, 5, 17, 3, 2, 1, 0);

        private static PlayerResources BlueBefore => new(10, 4, 10, 4, 10, 4, 7500);

        private static Game Load(string path) => TestServices.GameService.LoadGameFromFile(path);

        private static PlayerResources Resources(Game game, PlayerColor player) => game.History.CreatePlayerSession(game.History.Current.Players().FindByColor(player)).Current.Resources;

        [TestCase("sod_game_00_baseline")]
        [TestCase("sod_game_01_red_gives_blue_resources")]
        public void TheSavefileIsDetectedAsShadowOfDeathWithAllItsHeroes(string name)
        {
            var game = Load(Path.Combine("TestData", "Sod", name + ".GM1"));

            Assert.That(game.GameVersion, Is.EqualTo(GameVersion.ShadowOfDeath));
            Assert.That(game.History.Current.Heroes(), Has.Count.EqualTo(156));
        }

        [TestCase("sod_game_00_baseline")]
        [TestCase("sod_game_01_red_gives_blue_resources")]
        public void EveryHeroDeserializesAndSerializesBackToTheSameBytes(string name)
        {
            var bytes = TestServices.SavefileCodec.Read(Path.Combine("TestData", "Sod", name + ".GM1"));
            var loader = new HeroBytesLoader(Encoding.Latin1);
            var chunks = loader.LoadHeroChunks(bytes, new SavegameVersionDetector().Detect(bytes));
            var offsets = new HeroOffsets(new SavegameVersionDetector().Detect(bytes));
            var entities = TestServices.VersionEntities.For(offsets.GameVersion);
            var deserializer = new HeroDeserializer(offsets, entities);
            var serializer = new SafeHeroSerializer(offsets, entities);

            foreach (var chunk in chunks)
            {
                var result = serializer.Serialize(chunk, deserializer.Deserialize(chunk));
                Assert.That(result.HasSameBytes(chunk), Is.True, chunk.Name);
            }
        }

        [Test]
        public void MariusIsTheLevelOneDemoniacOfRed()
        {
            var game = Load(Save1);
            var hero = game.History.CreateHeroSession(game.History.Current.Heroes().FindByName("Marius")).Current;

            Assert.That(hero.Owner, Is.EqualTo(PlayerColor.Red));
            Assert.That(hero.HeroClass, Is.EqualTo(HeroClass.Demoniac));
            Assert.That(hero.HeroLevel.Level, Is.EqualTo((short)1));
            Assert.That(hero.PrimarySkills, Is.EqualTo(new HeroPrimarySkills(2, 2, 1, 1)));
            Assert.That(
                hero.Army.Slots,
                Is.EquivalentTo(new[]
                {
                    new ArmySlot(1, Creature.Imp.Code, 20u),
                    new ArmySlot(2, Creature.Gog.Code, 7u),
                }));
        }

        [Test]
        public void MariusIsTheOnlyHeroWithAnOwner()
        {
            var game = Load(Save1);
            var deserializer = new HeroDeserializer(game.History.Current.Heroes().Offsets, TestServices.VersionEntities.For(game.GameVersion));

            var owned = game.History.Current.Heroes().Where(c => deserializer.DeserializeOwner(c) == PlayerColor.Red).Select(c => c.Name);

            Assert.That(owned, Is.EqualTo(new[] { "Marius" }));
        }

        [Test]
        public void TheFourPlayersInPlayAreFoundInColourOrder()
        {
            var game = Load(Save1);

            // The savefile holds a struct for each of the eight colours, but Tan, Green, Orange and Pink have all resources at zero,
            // so they are not in play and are left out.
            var colors = game.History.Current.Players().Select(c => TestServices.PlayerDeserializer.DeserializeColor(c)).ToList();

            Assert.That(colors, Is.EqualTo(new[] { PlayerColor.Red, PlayerColor.Blue, PlayerColor.Purple, PlayerColor.Teal }));
            Assert.That(game.History.Current.Players().Select(c => c.Name), Is.EqualTo(colors.Select(c => c.ToString())));

            // The castle after the table starts inside the space the last (unused) player takes, so only that chunk is cut short.
            Assert.That(game.History.Current.Players().Select(c => c.Length), Has.All.EqualTo(PlayerLayout.ShadowOfDeath.Stride));
        }

        [TestCase(GameVersion.RestorationOfErathia)]
        [TestCase(GameVersion.ArmageddonsBlade)]
        public void RoeAndArmageddonsBladeAreAssumedToHaveTheShadowOfDeathLayout(GameVersion version)
        {
            Assert.That(PlayerLayout.For(version), Is.SameAs(PlayerLayout.ShadowOfDeath));
            Assert.That(new PlayerBytesLoader().LoadPlayerChunks(TestServices.SavefileCodec.Read(Save1), version), Has.Count.EqualTo(4));
        }

        [Test]
        public void RedStartsWithTheResourcesOfTheFirstGame()
        {
            Assert.That(Resources(Load(Save1), PlayerColor.Red), Is.EqualTo(RedBefore));
        }

        [Test]
        public void RedHasTheResourcesOfTheSecondGameAndBlueGotTheDifference()
        {
            var first = Load(Save1);
            var second = Load(Save2);

            Assert.That(second.History.Current.Players(), Has.Count.EqualTo(first.History.Current.Players().Count));
            Assert.That(Resources(first, PlayerColor.Blue), Is.EqualTo(BlueBefore));
            Assert.That(Resources(second, PlayerColor.Red), Is.EqualTo(RedAfter));
            Assert.That(
                Resources(second, PlayerColor.Blue),
                Is.EqualTo(new PlayerResources(
                    BlueBefore.Wood + RedBefore.Wood - RedAfter.Wood,
                    BlueBefore.Mercury + RedBefore.Mercury - RedAfter.Mercury,
                    BlueBefore.Ore + RedBefore.Ore - RedAfter.Ore,
                    BlueBefore.Sulfur + RedBefore.Sulfur - RedAfter.Sulfur,
                    BlueBefore.Crystal + RedBefore.Crystal - RedAfter.Crystal,
                    BlueBefore.Gems + RedBefore.Gems - RedAfter.Gems,
                    BlueBefore.Gold + RedBefore.Gold - RedAfter.Gold)));
        }

        [Test]
        public void TheOtherPlayersAndAllHeroesAreTheSameInBothGames()
        {
            var first = Load(Save1);
            var second = Load(Save2);

            for (var i = 2; i < first.History.Current.Players().Count; i++)
            {
                Assert.That(second.History.Current.Players()[i].HasSameBytes(first.History.Current.Players()[i]), Is.True, first.History.Current.Players()[i].Name);
            }

            for (var i = 0; i < first.History.Current.Heroes().Count; i++)
            {
                Assert.That(second.History.Current.Heroes()[i].HasSameBytes(first.History.Current.Heroes()[i]), Is.True, first.History.Current.Heroes()[i].Name);
            }
        }

        [Test]
        public void UnchangedGameSavesIdenticalBytes()
        {
            var game = Load(Save1);

            Assert.That(game.GetBytesToSave(), Is.EqualTo(game.Bytes));
        }

        [Test]
        public void ApplyingTheTradeToTheFirstGameGivesTheBytesOfTheSecondExceptTheHeader()
        {
            var game = Load(Save1);
            var second = Load(Save2);

            game.History.CreatePlayerSession(game.History.Current.Players().FindByColor(PlayerColor.Red)).Update(p => p with { Resources = RedAfter });
            game.History.CreatePlayerSession(game.History.Current.Players().FindByColor(PlayerColor.Blue)).Update(p => p with { Resources = Resources(second, PlayerColor.Blue) });

            var saved = game.GetBytesToSave();
            var expected = second.GetBytesToSave();

            // The real files also differ in the header (save name and time), which is not edited here.
            var differences = Enumerable.Range(0, saved.Length).Where(i => saved[i] != expected[i]).ToList();
            Assert.That(differences, Is.Not.Empty);
            Assert.That(differences, Has.All.LessThan(1200));
        }

        [Test]
        public void ResourceEditsChangeOnlyThoseBytesAndSurviveASaveAndReload()
        {
            var game = Load(Save1);
            var index = game.History.Current.Players().FindByColor(PlayerColor.Blue);
            game.History.CreatePlayerSession(index).Update(p => p with { Resources = p.Resources with { Gold = 123_456, Wood = 99 } });

            var path = Path.Combine(TestContext.CurrentContext.WorkDirectory, $"{Guid.NewGuid():N}.GM1");
            try
            {
                TestServices.GameService.SaveGame(game, path);
                var saved = TestServices.SavefileCodec.Read(path);

                var first = game.History.Current.Players()[index].StartIndex;
                var changed = Enumerable.Range(0, saved.Length).Where(i => saved[i] != game.Bytes[i]).ToList();
                Assert.That(changed, Is.Not.Empty);
                Assert.That(changed, Has.All.InRange(first, first + PlayerLayout.ResourcesOffset + (7 * PlayerLayout.ResourceSize) - 1));

                var reloaded = Load(path);
                Assert.That(Resources(reloaded, PlayerColor.Blue), Is.EqualTo(BlueBefore with { Gold = 123_456, Wood = 99 }));
                Assert.That(Resources(reloaded, PlayerColor.Red), Is.EqualTo(RedBefore));
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Test]
        public void HeroEditsAreSavedAndLoadedBack()
        {
            var game = Load(Save1);
            var marius = game.History.CreateHeroSession(game.History.Current.Heroes().FindByName("Marius"));
            var chunk = marius.Chunk;

            marius.Update(h => h with
            {
                PrimarySkills = new HeroPrimarySkills(30, 20, 10, 5),
                Backpack = h.Backpack.WithSlot(0, Artifact.OrbOfInhibition),
            });

            var path = Path.Combine(TestContext.CurrentContext.WorkDirectory, $"{Guid.NewGuid():N}.GM1");
            try
            {
                TestServices.GameService.SaveGame(game, path);
                var saved = TestServices.SavefileCodec.Read(path);

                var changed = Enumerable.Range(0, saved.Length).Where(i => saved[i] != game.Bytes[i]).ToList();
                Assert.That(changed, Is.Not.Empty);
                Assert.That(changed, Has.All.InRange(chunk.StartIndex, chunk.End - 1));

                var reloaded = Load(path);
                var hero = reloaded.History.CreateHeroSession(reloaded.History.Current.Heroes().FindByName("Marius")).Current;
                Assert.That(hero.PrimarySkills, Is.EqualTo(new HeroPrimarySkills(30, 20, 10, 5)));
                Assert.That(hero.Backpack.All[0]?.Art, Is.EqualTo(Artifact.OrbOfInhibition));
                Assert.That(hero.Army.Slots, Has.Length.EqualTo(2));
            }
            finally
            {
                File.Delete(path);
            }
        }
    }
}
