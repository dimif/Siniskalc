namespace Siniskalc.Heroes.Tests.Runtime
{
    using Moq;
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.Runtime;
    using Siniskalc.Heroes.Tests.Entities;

    /// <summary>
    /// Tests what a savefile of any game says about the game: the date, the map, the difficulty, whose turn it is and who plays each colour.
    /// </summary>
    [TestFixture]
    internal sealed class HeroesSavegameBaseTests
    {
        private static readonly MapHeader Map = new("Broken Alliance", "Two against one", GameDifficulty.Knight, 72);

        [Test]
        public void TheGameTellsWhatItsMetadataHolds()
        {
            var setup = new GameSetup<TestColor>([PlayerType.Computer, PlayerType.Human], GameDifficulty.Rook, TestColor.Blue);
            var game = new TestSavegame(new SaveMetadata<TestColor>(new GameDate(1, 2, 3), Map, setup));

            Assert.That((game.Date, game.Map, game.Difficulty, game.CurrentPlayer), Is.EqualTo(((GameDate?)new GameDate(1, 2, 3), Map, (GameDifficulty?)GameDifficulty.Rook, TestColor.Blue)));
            Assert.That((game.GetControl(TestColor.Red), game.GetControl(TestColor.Blue)), Is.EqualTo(((PlayerType?)PlayerType.Computer, (PlayerType?)PlayerType.Human)));
        }

        [Test]
        public void WhatTheSavefileDoesNotHoldIsNotKnown()
        {
            IGameInfo<TestColor> game = new TestSavegame(SaveMetadata<TestColor>.None);

            Assert.That((game.Date, game.Map, game.Difficulty, game.CurrentPlayer, game.GetControl(TestColor.Red)), Is.EqualTo(((GameDate?)null, (MapHeader?)null, (GameDifficulty?)null, (TestColor?)null, (PlayerType?)null)));
        }

        private sealed class TestSavegame(SaveMetadata<TestColor> metadata)
            : HeroesSavegameBase<TestColor>(null, [], new SaveDocumentHistory(new SaveDocument([], new Dictionary<ChunkKind, IChunkCollection>(), Mock.Of<ISavefileBytesPatcher>())), null, metadata)
        {
            public override string VersionName => "Test";
        }
    }
}
