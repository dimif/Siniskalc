namespace Siniskalc.Homm2.Tests.Data
{
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Homm2.Data;

    [TestFixture]
    internal sealed class HeaderReaderTests
    {
        // Every save we have was started at the middle level; the other levels are covered by editing the header.
        [Test]
        public void TheDifficultyOfEverySavegameIsRead()
        {
            foreach (var path in Saves.All())
            {
                Assert.That(TestServices.Load(path).Difficulty, Is.EqualTo(GameDifficulty.Knight), path);
            }
        }

        [TestCase(0, GameDifficulty.Pawn)]
        [TestCase(1, GameDifficulty.Knight)]
        [TestCase(2, GameDifficulty.Rook)]
        [TestCase(3, GameDifficulty.Queen)]
        [TestCase(4, GameDifficulty.King)]
        [TestCase(9, GameDifficulty.Pawn)]
        public void TheDifficultyCodeOfTheHeaderGivesTheLevel(int code, GameDifficulty expected)
        {
            var bytes = Saves.Read(Saves.BrokenAllianceSave(1));
            bytes[HeaderLayout.Difficulty] = (byte)code;
            bytes[HeaderLayout.Difficulty + 1] = 0;
            var layout = new SaveLayoutLocator().Locate(bytes);

            Assert.That(new HeaderReader().Read(bytes, layout!).Difficulty, Is.EqualTo(expected));
        }

        [Test]
        public void TheDifficultyOfAPriceOfLoyaltySavegameIsBehindTheMarker()
        {
            var bytes = Saves.Read(Saves.Path(Saves.ArraxTheJerk, "arrax_the_jerk_01_start"));
            bytes[HeaderLayout.Difficulty + HeaderLayout.MarkerLength] = 4;
            var layout = new SaveLayoutLocator().Locate(bytes);

            Assert.That(new HeaderReader().Read(bytes, layout!).Difficulty, Is.EqualTo(GameDifficulty.King));
        }
    }
}
