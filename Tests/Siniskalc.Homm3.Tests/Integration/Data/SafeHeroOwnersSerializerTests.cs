using Siniskalc.Homm3.Tests.Data;
using Siniskalc.Core.Data;
using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Models;

namespace Siniskalc.Homm3.Tests.Integration.Data
{
    /// <summary>
    /// Writing the table of hero owners: a byte is left alone while it reads as what the model has.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class SafeHeroOwnersSerializerTests
    {
        private static readonly byte[] Bytes = [0x01, 0xFF, 0xFF, 0x41, 0x40, 0x00, 0x99];

        private static RecordChunk Chunk() => new("Hero owners", Bytes.ToArray(), 0, 0);

        private static HeroOwnersModel Read() => new HeroOwnersDeserializer().Deserialize(Chunk());

        private static IEnumerable<string> EverySavefile() => Directory.GetFiles("TestData", "*.GM?", SearchOption.AllDirectories).OrderBy(f => f, StringComparer.Ordinal);

        [TestCaseSource(nameof(EverySavefile))]
        public void TheTableOfEverySavefileIsWrittenBackAsItWasRead(string path)
        {
            var owners = ReadOnlySaves.Load(path).History.Current.HeroOwners();

            for (var i = 0; i < owners.Count; i++)
            {
                Assert.That(owners.Serializer.Serialize(owners[i], owners.GetModel(i)).ToArray(), Is.EqualTo(owners[i].ToArray()));
            }
        }

        [Test]
        public void ATableThatWasNotEditedIsWrittenAsItWas()
        {
            Assert.That(new SafeHeroOwnersSerializer().Serialize(Chunk(), Read()).ToArray(), Is.EqualTo(Bytes));
        }

        [Test]
        public void AHeroThatLeavesItsPlayerIsWrittenAsFfAndTheOtherBytesStay()
        {
            var written = new SafeHeroOwnersSerializer().Serialize(Chunk(), Read().WithOwner(0, PlayerColor.Neutral).WithOwner(5, PlayerColor.Neutral)).ToArray();

            Assert.That(written, Is.EqualTo(new byte[] { 0xFF, 0xFF, 0xFF, 0x41, 0x40, 0xFF, 0x99 }));
        }

        [Test]
        public void AHeroGivenToAPlayerIsWrittenAsTheColour()
        {
            var written = new SafeHeroOwnersSerializer().Serialize(Chunk(), Read().WithOwner(1, PlayerColor.Pink).WithOwner(4, PlayerColor.Red)).ToArray();

            Assert.That(written[1], Is.EqualTo(0x07));
            Assert.That(written[4], Is.EqualTo(0x00));
        }

        [Test]
        public void ATableOfAnotherLengthIsRefused()
        {
            Assert.That(() => new SafeHeroOwnersSerializer().Serialize(Chunk(), new HeroOwnersModel([HeroOwnership.Free])), Throws.TypeOf<ArgumentOutOfRangeException>());
        }
    }
}
