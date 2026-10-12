using System.Text;
using Siniskalc.Data;
using Siniskalc.Models;
using Siniskalc.Runtime;

namespace Siniskalc.Tests.Data
{
    /// <summary>
    /// bios_standard is a HoTA savefile in which no hero has a biography. bios_custom is the same game after giving Alkin (Purple)
    /// a long biography and Ivor (Green) a short one. Nothing else was changed.
    /// </summary>
    [TestFixture]
    internal sealed class BiographyTests
    {
        private const string IvorBiography = "A much shorter custom biography.";

        private static readonly string StandardPath = Path.Combine("TestData", "Hota", "bios_standard.GM1");
        private static readonly string CustomPath = Path.Combine("TestData", "Hota", "bios_custom.GM1");

        private static Game Load(string path) => TestServices.GameService.LoadGameFromFile(path);

        private static HeroModel Hero(Game game, string name) => game.History.CreateHeroSession(game.History.Current.Heroes().FindByName(name)).Current;

        private static object Summary(HeroModel hero) => new
        {
            hero.Name,
            hero.Owner,
            hero.Position,
            hero.MarkerPosition,
            hero.HeroClass,
        };

        [Test]
        public void NoHeroOfTheStandardSaveHasABiography()
        {
            var game = Load(StandardPath);

            Assert.That(game.History.Current.Heroes().Select(c => c.Biography), Has.All.Null);
        }

        [Test]
        public void OnlyTheHeroesWithACustomBiographyHaveOne()
        {
            var game = Load(CustomPath);

            var withBiography = game.History.Current.Heroes().Where(c => c.Biography != null).ToList();
            Assert.That(withBiography.Select(c => c.Name), Is.EquivalentTo(new[] { "Alkin", "Ivor" }));
            Assert.That(Hero(game, "Ivor").Biography, Is.EqualTo(IvorBiography));

            var alkin = Hero(game, "Alkin").Biography;
            Assert.That(alkin, Does.StartWith("This hero has a long biography."));
            Assert.That(alkin, Does.EndWith("heroherohero"));
            Assert.That(alkin.Length, Is.GreaterThan(300));
        }

        [Test]
        public void AddingBiographiesDoesNotLoseOrChangeAnyHero()
        {
            var standard = Load(StandardPath);
            var custom = Load(CustomPath);

            Assert.That(custom.History.Current.Heroes().Select(c => c.Name), Is.EqualTo(standard.History.Current.Heroes().Select(c => c.Name)));
            for (var i = 0; i < standard.History.Current.Heroes().Count; i++)
            {
                var before = standard.History.CreateHeroSession(i).Current;
                var after = custom.History.CreateHeroSession(i).Current;

                Assert.That(Summary(after), Is.EqualTo(Summary(before)), before.Name);
            }
        }

        [Test]
        public void ThePositionOfAHeroWithABiographyIsTheOneInFrontOfTheBiography()
        {
            var standard = Load(StandardPath);
            var custom = Load(CustomPath);

            Assert.That(Hero(custom, "Alkin").Position, Is.EqualTo(Hero(standard, "Alkin").Position));
            Assert.That(Hero(custom, "Ivor").Position, Is.EqualTo(Hero(standard, "Ivor").Position));
        }

        [Test]
        public void ChangingThePositionOfAHeroWithABiographyDoesNotTouchTheBiography()
        {
            var game = Load(CustomPath);
            var index = game.History.Current.Heroes().FindByName("Alkin");
            var session = game.History.CreateHeroSession(index);
            var original = session.Current;

            session.Update(h => h with { Position = h.Position with { X = 3, Y = 4 } });

            var chunk = game.History.Current.Heroes()[index];
            Assert.That(chunk.Biography, Is.EqualTo(original.Biography));
            var bytes = game.GetBytesToSave();
            var saved = new HeroBytesLoader(Encoding.Latin1).LoadHeroChunks(bytes, new SavegameVersionDetector().Detect(bytes));
            Assert.That(saved.Single(c => c.Name == "Alkin").Biography, Is.EqualTo(original.Biography));
        }

        [Test]
        public void TheBiographyIsKeptWhenTheChunkIsEdited()
        {
            var chunk = new HeroChunk("hero", new byte[40], 0, "text", new byte[26], -26);

            var edited = chunk.WithBytes(new byte[40]);

            Assert.That(edited.Biography, Is.EqualTo("text"));
            Assert.That(edited.OutsidePositionBytes, Is.EqualTo(new byte[6]));
        }
    }
}
