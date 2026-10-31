namespace Siniskalc.Homm2.Tests.Data
{
    using System.Text;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;

    /// <summary>
    /// What the bytes of a record of a castle read as, one field at a time on records made by hand, including the bytes that are no known
    /// entity and so read as the stand-in the editor keeps.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class CastleDeserializerTests
    {
        private readonly CastleDeserializer deserializer = new();

        [Test]
        public void TheTileTheBoatAndTheVisitingHeroAreReadFromTheirBytes()
        {
            var castle = this.deserializer.Deserialize(BlankRecords.Castle(b =>
            {
                b[CastleLayout.X] = 20;
                b[CastleLayout.Y] = 21;
                b[CastleLayout.Boat] = 30;
                b[CastleLayout.Boat + 1] = 31;
                b[CastleLayout.VisitingHero] = 9;
            }));

            Assert.That((castle.Position, castle.ShipyardPosition, castle.VisitingHeroId), Is.EqualTo((new MapPosition(20, 21), new MapPosition(30, 31), 9)));
        }

        [Test]
        public void ACastleOffTheCoastAndWithNoVisitorHasNeitherBoatNorHero()
        {
            var castle = this.deserializer.Deserialize(BlankRecords.Castle());

            Assert.That((castle.ShipyardPosition, castle.VisitingHeroId), Is.EqualTo(((MapPosition?)null, (int?)null)));
        }

        [TestCase(0, "Blue")]
        [TestCase(3, "Yellow")]
        [TestCase(0xFF, "Neutral")]
        [TestCase(0x09, "Neutral")]
        [TestCase(0x80, "Neutral")]
        public void AnOwnerByteReadsAsItsColourAndOneThatIsNoColourReadsAsNeutral(int code, string color)
        {
            var castle = this.deserializer.Deserialize(BlankRecords.Castle(b => b[CastleLayout.Owner] = (byte)code));

            Assert.That(castle.Owner.Name, Is.EqualTo(color));
        }

        [TestCase(0, "Knight")]
        [TestCase(1, "Barbarian")]
        [TestCase(0x77, "Neutral")]
        public void AFactionByteReadsAsItsFactionAndOneThatIsNoFactionReadsAsNeutral(int code, string faction)
        {
            var castle = this.deserializer.Deserialize(BlankRecords.Castle(b => b[CastleLayout.Faction] = (byte)code));

            Assert.That(castle.Faction.Name, Is.EqualTo(faction));
        }

        [Test]
        public void TheNameIsReadUpToTheFirstZeroByte()
        {
            var castle = this.deserializer.Deserialize(BlankRecords.Castle(b => Encoding.Latin1.GetBytes("Xabran\0junk").CopyTo(b, CastleLayout.Name)));

            Assert.That(castle.Name, Is.EqualTo("Xabran"));
        }

        [Test]
        public void TheCreaturesOfEachDwellingAreReadFromTheListOfItsLevel()
        {
            var castle = this.deserializer.Deserialize(BlankRecords.Castle(b =>
            {
                b[CastleLayout.Faction] = (byte)Faction.Warlock.Code;

                // Dwelling 1 built (own list), dwelling 2 upgraded (11 in the list of the upgraded, 14 left in its own), dragons upgraded twice.
                var dwellings = (uint)(Dwellings.Dwelling1 | Dwellings.Dwelling2Upgraded | Dwellings.Dwelling6UpgradedTwice);
                BitConverter.GetBytes(dwellings).CopyTo(b, CastleLayout.Dwellings);
                BitConverter.GetBytes((ushort)5).CopyTo(b, CastleLayout.Creatures);
                BitConverter.GetBytes((ushort)14).CopyTo(b, CastleLayout.Creatures + 2);
                BitConverter.GetBytes((ushort)11).CopyTo(b, CastleLayout.UpgradedCreatures);
                BitConverter.GetBytes((ushort)3).CopyTo(b, CastleLayout.UpgradedCreatures + 10);
            }));

            Assert.That(castle.CreaturesAvailable, Is.EqualTo(new[] { 5, 11, 0, 0, 0, 3 }));
        }

        [Test]
        public void TheMageGuildHasTheCountsOfItsLevelsAndTheNumbersOfItsSpells()
        {
            var castle = this.deserializer.Deserialize(BlankRecords.Castle(b =>
            {
                new byte[] { 3, 3, 2, 0, 0 }.CopyTo(b, CastleLayout.GuildSpellCounts);
                b[CastleLayout.GuildSpells] = 0;
                b[CastleLayout.GuildSpells + 1] = 64;
                b[CastleLayout.GuildSpells + 2] = 65;
                b[CastleLayout.GuildSpells + 3] = 0xC8;
            }));

            Assert.That(castle.MageGuildSpellCounts, Is.EqualTo(new[] { 3, 3, 2, 0, 0 }));
            Assert.That(castle.MageGuildSpells.Take(5), Is.EqualTo(new[] { 1, 65, 0, 0, 0 }), "the number is the byte plus one; an empty mark, a byte past the last spell and a byte that is no spell read as none");
        }

        [Test]
        public void ACreatureCodeThatIsNoCreatureReadsAsPeasantWithItsCount()
        {
            var castle = this.deserializer.Deserialize(BlankRecords.Castle(b =>
            {
                b[CastleLayout.GarrisonTypes + 1] = 1;
                BitConverter.GetBytes((ushort)3).CopyTo(b, CastleLayout.GarrisonCounts + 2);
                b[CastleLayout.GarrisonTypes + 3] = 0xC8;
                BitConverter.GetBytes((ushort)7).CopyTo(b, CastleLayout.GarrisonCounts + 6);
            }));

            Assert.That(castle.Garrison.Slots.Select(s => s?.Creature.Name), Is.EqualTo(new[] { null, "Archer", null, "Peasant", null }));
            Assert.That(castle.Garrison.Slots.Select(s => s?.Count), Is.EqualTo(new long?[] { null, 3, null, 7, null }));
        }
    }
}
