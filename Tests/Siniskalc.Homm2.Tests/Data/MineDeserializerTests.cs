namespace Siniskalc.Homm2.Tests.Data
{
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;

    /// <summary>
    /// What the bytes of a mine of the map read as, on records made by hand.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class MineDeserializerTests
    {
        private static MapObjectChunk Mine(byte owner, byte kind) =>
            new("Mine", [owner, kind, 0xFF, 0xFF, 0, 0, 0], 100, new MapPosition(14, 15), 2);

        [Test]
        public void AMineIsItsKindItsTileItsPlaceInTheTableAndItsOwner()
        {
            var mine = new MineDeserializer(TestCatalog.Standard).Deserialize(Mine(2, (byte)MineKind.GoldMine));

            Assert.That((mine.Index, mine.Kind, mine.Position, mine.Owner), Is.EqualTo((2, MineKind.GoldMine, new MapPosition(14, 15), PlayerColor.Red)));
        }

        [TestCase(0xFF)]
        [TestCase(0x09)]
        [TestCase(0x80)]
        public void AnOwnerByteThatIsTheNeutralByteOrNoColourReadsAsNeutral(int owner)
        {
            var mine = new MineDeserializer(TestCatalog.Standard).Deserialize(Mine((byte)owner, (byte)MineKind.Sawmill));

            Assert.That(mine.Owner, Is.EqualTo(PlayerColor.Neutral));
        }

        [TestCase(62, "EarthElemental", 20)]
        [TestCase(63, "AirElemental", 20)]
        [TestCase(64, "FireElemental", 20)]
        [TestCase(65, "WaterElemental", 20)]
        [TestCase(62, "EarthElemental", 40)]
        [TestCase(65, "WaterElemental", 255)]
        public void AGuardianSpellReadsAsTheCorrespondingElementalAndTheSavedCount(int spell, string creatureId, int count)
        {
            var chunk = new MapObjectChunk("Mine", [0, 6, (byte)spell, (byte)count, 43, 23, 0xFF], 100, new MapPosition(43, 23), 2);

            var guardian = new MineDeserializer(TestCatalog.Standard).Deserialize(chunk).Guardian;

            Assert.That(guardian, Is.Not.Null);
            Assert.That((guardian!.SpellCode, guardian.Count, guardian.Creature), Is.EqualTo((spell, count, TestCatalog.Standard.Creatures[creatureId])));
        }

        [TestCase(0)]
        [TestCase(0xFF)]
        public void BothObservedNoGuardianCountsReadAsNoGuardian(int count)
        {
            var chunk = new MapObjectChunk("Mine", [0, 6, 0xFF, (byte)count, 43, 23, 0xFF], 100, new MapPosition(43, 23), 2);

            Assert.That(new MineDeserializer(TestCatalog.Standard).Deserialize(chunk).Guardian, Is.Null);
        }

        [TestCase(0, 20)]
        [TestCase(61, 20)]
        [TestCase(66, 20)]
        [TestCase(0xFF, 20)]
        [TestCase(62, 0)]
        public void AnUnrecognizedGuardianKeepsItsSpellAndCountWithoutClaimingToBeAnElemental(int spell, int count)
        {
            var chunk = new MapObjectChunk("Mine", [0, 6, (byte)spell, (byte)count, 43, 23, 0xFF], 100, new MapPosition(43, 23), 2);

            var guardian = new MineDeserializer(TestCatalog.Standard).Deserialize(chunk).Guardian;

            Assert.That(guardian, Is.Not.Null);
            Assert.That((guardian!.SpellCode, guardian.Count, guardian.Creature), Is.EqualTo((spell, count, (Creature?)null)));
        }
    }
}
