namespace Siniskalc.Homm2.Tests.Data
{
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Data;

    /// <summary>
    /// What the bytes of a shrine of the map read as, on records made by hand: the spell is the number above the low three bits.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class ShrineDeserializerTests
    {
        private static MapObjectChunk Shrine(ushort word, int tag) =>
            new("Shrine", BitConverter.GetBytes(word), 200, new MapPosition(30, 31), 1, tag);

        [Test]
        public void TheSpellIsTheNumberAboveTheLowThreeBitsAndTheCircleIsInTheTag()
        {
            var shrine = new ShrineDeserializer().Deserialize(Shrine((1 << 3) | 5, 2));

            Assert.That((shrine.Index, shrine.Circle, shrine.Position, shrine.SpellCode), Is.EqualTo((1, 2, new MapPosition(30, 31), 1)));
        }

        [Test]
        public void TheCircleIsTheTwoLowBitsOfTheTagWhateverElseTheTagHolds()
        {
            var shrine = new ShrineDeserializer().Deserialize(Shrine(8, 3 | 0x44));

            Assert.That(shrine.Circle, Is.EqualTo(3));
        }

        [Test]
        public void AHiddenShrineHasNoSpellToShow()
        {
            var shrine = new ShrineDeserializer().Deserialize(Shrine(8 * 17, 1 | MapObjectsReader.HiddenShrine));

            Assert.That(shrine.SpellCode, Is.EqualTo(-1));
        }

        [Test]
        public void ASpellNumberTheGameDoesNotHaveIsReadAsTheNumberItIs()
        {
            var shrine = new ShrineDeserializer().Deserialize(Shrine(8 * 200, 1));

            Assert.That((shrine.SpellCode, shrine.Spell), Is.EqualTo((200, (object?)null)));
        }
    }
}
