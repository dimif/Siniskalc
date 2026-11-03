using Siniskalc.Core.Data;
using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Models;

namespace Siniskalc.Homm3.Tests.Data
{
    /// <summary>
    /// Reading the table of hero owners: FF nobody, a colour, 40 a hero kept from the taverns, the 41 of hero 3, and a byte never seen.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class HeroOwnersDeserializerTests
    {
        private static readonly byte[] Bytes = [0x01, 0xFF, 0xFF, 0x41, 0x40, 0x00, 0x99];

        private static HeroOwnersModel Read() => new HeroOwnersDeserializer().Deserialize(new RecordChunk("Hero owners", Bytes.ToArray(), 0, 0));

        [Test]
        public void EachByteReadsAsTheOwnerOfTheHero()
        {
            Assert.That(Read().Heroes, Is.EqualTo(new[]
            {
                new HeroOwnership(PlayerColor.Blue, false),
                HeroOwnership.Free,
                HeroOwnership.Free,
                HeroOwnership.Reserved,
                HeroOwnership.Reserved,
                new HeroOwnership(PlayerColor.Red, false),
                HeroOwnership.Free,
            }));
        }
    }
}
