namespace Siniskalc.Homm3.Tests.Integration.Runtime
{
    using Siniskalc.Core.Runtime;

    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class SavegameParserTests
    {
        [Test]
        public void BytesThatAreNoSavegameAreAnUnsupportedSavefile()
        {
            var bytes = new byte[50_000];
            new Random(3).NextBytes(bytes);

            Assert.That(() => TestServices.GameService.LoadGameFromBytes(bytes, "random.bin"), Throws.TypeOf<UnsupportedSavefileException>());
        }

        [Test]
        public void NoBytesAreAnUnsupportedSavefile()
        {
            Assert.That(() => TestServices.GameService.LoadGameFromBytes([], "empty.bin"), Throws.TypeOf<UnsupportedSavefileException>());
        }
    }
}
