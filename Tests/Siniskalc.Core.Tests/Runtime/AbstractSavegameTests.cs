namespace Siniskalc.Core.Tests.Runtime
{
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Tests.Data;

    /// <summary>
    /// Tests what every game keeps of the savefile it was read from: its name, the file it came from and the bytes to save.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class AbstractSavegameTests
    {
        [Test]
        public void TheNameIsTheOneGivenElseTheFileNameElseSavefile()
        {
            Assert.That(new TestSavegame("C:\\saves\\a.GM1", "Given").Name, Is.EqualTo("Given"));
            Assert.That(new TestSavegame("C:\\saves\\a.GM1").Name, Is.EqualTo("C:\\saves\\a.GM1"));
            Assert.That(new TestSavegame().Name, Is.EqualTo("Savefile"));
        }

        [Test]
        public void ASavegameFromBytesHasNoFileUntilItIsSaved()
        {
            var game = new TestSavegame();

            Assert.That(game.FileName, Is.Null);

            game.SavedTo("C:\\saves\\b.GM1");

            Assert.That(game.FileName, Is.EqualTo("C:\\saves\\b.GM1"));
        }

        [Test]
        public void TheBytesToSaveAreTheOriginalBytesWhenNothingWasEdited()
        {
            var game = new TestSavegame();

            Assert.That(game.GetBytesToSave(), Is.EqualTo(new byte[] { 0, 0, 1, 2, 3, 0, 0 }));
        }

        [Test]
        public void TheBytesToSaveHaveTheEditsOfTheChunksAndNothingElseChanged()
        {
            var game = new TestSavegame();

            game.Edit(9);

            Assert.That(game.GetBytesToSave(), Is.EqualTo(new byte[] { 0, 0, 9, 2, 3, 0, 0 }));
        }

        [Test]
        public void TheOriginalBytesAreKeptAndEachCallGivesAnArrayOfItsOwn()
        {
            var game = new TestSavegame();
            game.Edit(9);

            var first = game.GetBytesToSave();
            first[0] = 99;

            Assert.That(game.Bytes, Is.EqualTo(new byte[] { 0, 0, 1, 2, 3, 0, 0 }));
            Assert.That(game.GetBytesToSave()[0], Is.EqualTo(0));
        }

        [Test]
        public void TheGameHoldsTheHistoryItWasGivenAndTheVersionNameOfTheGame()
        {
            var game = new TestSavegame();

            Assert.That(game.History.Current.GetChunks<RawBytesChunk>(TestChunkKinds.Unit), Has.Count.EqualTo(1));
            Assert.That(game.VersionName, Is.EqualTo("Test"));
        }
    }
}
