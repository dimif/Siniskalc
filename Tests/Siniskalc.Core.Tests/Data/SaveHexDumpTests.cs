namespace Siniskalc.Core.Tests.Data
{
    using Moq;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Tests.Runtime;

    /// <summary>
    /// Tests the dump of a whole savefile for the hex editor: the bytes now, the bytes the savefile had and the regions asked of the provider.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class SaveHexDumpTests
    {
        [Test]
        public void TheDumpHoldsTheTitleTheBytesNowAndTheBytesTheSavefileHad()
        {
            var game = new TestSavegame();
            game.Edit(9);
            var regions = new Mock<ISaveRegionProvider>();
            regions.Setup(r => r.GetRegions(It.IsAny<SaveDocument>(), It.IsAny<byte[]>())).Returns([]);

            var dump = SaveHexDump.Create("Savefile", game.History, regions.Object);

            Assert.That(dump.Title, Is.EqualTo("Savefile"));
            Assert.That(dump.Bytes, Is.EqualTo(new byte[] { 0, 0, 9, 2, 3, 0, 0 }));
            Assert.That(dump.OriginalBytes, Is.EqualTo(new byte[] { 0, 0, 1, 2, 3, 0, 0 }));
        }

        [Test]
        public void TheRegionsAreThoseTheProviderGivesForTheCurrentDocumentAndItsBytes()
        {
            var game = new TestSavegame();
            game.Edit(9);
            var region = new ByteRegion("Value", 2, 1, ByteRegionKind.Other);
            SaveDocument? askedDocument = null;
            byte[]? askedBytes = null;
            var regions = new Mock<ISaveRegionProvider>();
            regions.Setup(r => r.GetRegions(It.IsAny<SaveDocument>(), It.IsAny<byte[]>()))
                .Callback<SaveDocument, byte[]>((document, bytes) => (askedDocument, askedBytes) = (document, bytes))
                .Returns([region]);

            var dump = SaveHexDump.Create("Savefile", game.History, regions.Object);

            Assert.That(dump.Regions, Is.EqualTo(new[] { region }));
            Assert.That(askedDocument, Is.SameAs(game.History.Current));
            Assert.That(askedBytes, Is.EqualTo(new byte[] { 0, 0, 9, 2, 3, 0, 0 }));
        }

        [Test]
        public void ANewDumpAfterSavingHasTheSameBytesNowAndOriginally()
        {
            var game = new TestSavegame();
            game.Edit(9);
            game.History.MarkSaved();
            var regions = Mock.Of<ISaveRegionProvider>(r => r.GetRegions(It.IsAny<SaveDocument>(), It.IsAny<byte[]>()) == new List<ByteRegion>());

            var dump = SaveHexDump.Create("Savefile", game.History, regions);

            Assert.That(dump.OriginalBytes, Is.EqualTo(dump.Bytes));
        }
    }
}
