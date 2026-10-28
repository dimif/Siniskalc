using Siniskalc.Core.Data;
using Siniskalc.Homm3.Tests.Data;
using Siniskalc;
using Siniskalc.Heroes.Models;
using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Models;
using Siniskalc.Homm3.Runtime;

namespace Siniskalc.Homm3.Tests.Integration.Data
{
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class SafeCastleSerializerTests
    {
        [Test]
        public void EveryCastleOfEverySampleSaveSerializesToTheBytesItWasReadFrom()
        {
            var castles = 0;
            foreach (var path in CastleSamples.All())
            {
                foreach (var chunk in CastleSamples.Load(path).History.Current.Castles())
                {
                    var written = TestServices.SerializeCastle(chunk, TestServices.CastleDeserializer.Deserialize(chunk));

                    Assert.That(written.HasSameBytes(chunk), Is.True, $"{chunk.Name} in {path}");
                    castles++;
                }
            }

            Assert.That(castles, Is.GreaterThan(100));
        }

        [Test]
        public void SerializingEveryCastleKeepsTheWholeSavefileIdentical()
        {
            foreach (var path in CastleSamples.All().Where(p => p.Contains("Bulwark") || p.Contains("castles_2")))
            {
                var game = CastleSamples.Load(path);
                var document = game.History.Current;
                for (var i = 0; i < document.Castles().Count; i++)
                {
                    document = document.WithCastle(i, TestServices.SerializeCastle(document.Castles()[i], TestServices.CastleDeserializer.Deserialize(document.Castles()[i])));
                }

                Assert.That(document.ToBytes(), Is.EqualTo(TestServices.SavefileCodec.Read(path)), path);
            }
        }

        [Test]
        public void OwnerAndBuiltThisTurnAreWrittenAndNothingElseChanges()
        {
            var chunk = CastleSamples.Load(Path.Combine(CastleSamples.Folder("Bulwark").First())).History.Current.Castles().First(c => c.Name == "Still Water");
            var castle = TestServices.CastleDeserializer.Deserialize(chunk);

            var written = TestServices.SerializeCastle(chunk, castle with { Owner = PlayerColor.Teal, BuiltThisTurn = true });
            var changed = Enumerable.Range(0, chunk.Length).Where(i => chunk.GetByteAt(i) != written.GetByteAt(i)).ToList();

            Assert.That(changed, Is.EquivalentTo(new[] { CastleLayout.Owner, CastleLayout.BuiltThisTurn }));
            Assert.That(TestServices.CastleDeserializer.Deserialize(written), Is.EqualTo(castle with { Owner = PlayerColor.Teal, BuiltThisTurn = true }).Using<CastleModel>(SameCastle));
        }

        [Test]
        public void ANeutralCastleIsWrittenAsTheByteFF()
        {
            var chunk = CastleSamples.Load(CastleSamples.Folder("Bulwark").First()).History.Current.Castles().First(c => c.Name == "Still Water");

            var written = TestServices.SerializeCastle(chunk, TestServices.CastleDeserializer.Deserialize(chunk) with { Owner = PlayerColor.Neutral });

            Assert.That(written.GetByteAt(CastleLayout.Owner), Is.EqualTo(Constants.ByteOff));
            Assert.That(TestServices.CastleDeserializer.DeserializeOwner(written), Is.SameAs(PlayerColor.Neutral));
        }

        [Test]
        public void TheGarrisonIsWrittenAndEmptiedSlotsGoBackToMinusOne()
        {
            var chunk = CastleSamples.Load(CastleSamples.Folder("Bulwark").First()).History.Current.Castles().First(c => c.Name == "Still Water");
            var castle = TestServices.CastleDeserializer.Deserialize(chunk);

            var filled = TestServices.SerializeCastle(chunk, castle with { Garrison = castle.Garrison.WithSlot(1, Creature.Centaur, 1234).WithSlot(6, Creature.GoldDragon, 3) });
            var garrison = TestServices.CastleDeserializer.DeserializeGarrison(filled);
            Assert.That(garrison.Slots[1], Is.EqualTo(new ArmySlot<Creature>(Creature.Centaur, 1234)));
            Assert.That(garrison.Slots[6], Is.EqualTo(new ArmySlot<Creature>(Creature.GoldDragon, 3)));
            Assert.That(garrison.Filled.Count(), Is.EqualTo(2));

            var emptied = TestServices.SerializeCastle(filled, TestServices.CastleDeserializer.Deserialize(filled) with { Garrison = new Army<Creature>(Constants.CreatureSlotCount) });
            Assert.That(emptied.HasSameBytes(chunk), Is.True);
        }

        [Test]
        public void TheGarrisonIsWrittenAsLittleEndianTypeAndCountIntegersInTheirOwnTables()
        {
            var chunk = StillWater();
            var castle = TestServices.CastleDeserializer.Deserialize(chunk);

            var written = TestServices.SerializeCastle(chunk, castle with { Garrison = castle.Garrison.WithSlot(1, Creature.Centaur, 1234).WithSlot(6, Creature.GoldDragon, 3) });

            Assert.That(written.GetBytesAt(CastleLayout.GarrisonTypes + 4, 4), Is.EqualTo(new byte[] { 0x0E, 0, 0, 0 }));
            Assert.That(written.GetBytesAt(CastleLayout.GarrisonCounts + 4, 4), Is.EqualTo(new byte[] { 0xD2, 0x04, 0, 0 }));
            Assert.That(written.GetBytesAt(CastleLayout.GarrisonTypes + 24, 4), Is.EqualTo(new byte[] { 0x1B, 0, 0, 0 }));
            Assert.That(written.GetBytesAt(CastleLayout.GarrisonCounts + 24, 4), Is.EqualTo(new byte[] { 3, 0, 0, 0 }));
        }

        [Test]
        public void AnOwnerByteThatIsNoColourIsKeptWhileTheCastleStaysNeutral()
        {
            var chunk = StillWater();
            var odd = chunk.WithBytes(WithByte(chunk, CastleLayout.Owner, 0x09));

            var written = TestServices.SerializeCastle(odd, TestServices.CastleDeserializer.Deserialize(odd));

            Assert.That(written.GetByteAt(CastleLayout.Owner), Is.EqualTo(0x09));
            Assert.That(TestServices.SerializeCastle(odd, TestServices.CastleDeserializer.Deserialize(odd) with { Owner = PlayerColor.Red }).GetByteAt(CastleLayout.Owner), Is.EqualTo(PlayerColor.Red.Code));
        }

        [Test]
        public void ABuiltThisTurnByteThatIsNotOneIsKeptWhileTheCastleStaysBuilt()
        {
            var chunk = StillWater();
            var odd = chunk.WithBytes(WithByte(chunk, CastleLayout.BuiltThisTurn, 2));

            var written = TestServices.SerializeCastle(odd, TestServices.CastleDeserializer.Deserialize(odd));

            Assert.That(written.GetByteAt(CastleLayout.BuiltThisTurn), Is.EqualTo(2));
            Assert.That(TestServices.SerializeCastle(odd, TestServices.CastleDeserializer.Deserialize(odd) with { BuiltThisTurn = false }).GetByteAt(CastleLayout.BuiltThisTurn), Is.EqualTo(0));
        }

        [Test]
        public void AGarrisonSlotWithACreatureTheEditorDoesNotKnowReadsAsTheFirstCreatureAndKeepsItsNumberUntilTheModelChanges()
        {
            var unknown = WithUnknownCreature(StillWater());
            var model = TestServices.CastleDeserializer.Deserialize(unknown);

            var written = TestServices.SerializeCastle(unknown, model);

            Assert.That(model.Garrison.Slots[3]?.Creature, Is.EqualTo(Creature.All[0]));
            Assert.That(written.ToArray(), Is.EqualTo(unknown.ToArray()));
            Assert.That(TestServices.SerializeCastle(unknown, model with { Garrison = model.Garrison.WithSlot(3, Creature.Angel, 2) }).GetInt32At(CastleLayout.GarrisonTypes + (3 * 4)), Is.EqualTo(Creature.Angel.Code));
        }

        [Test]
        public void AGarrisonSlotWithACreatureTheEditorDoesNotKnowIsEmptiedWhenTheModelEmptiesIt()
        {
            var unknown = WithUnknownCreature(StillWater());

            var written = TestServices.SerializeCastle(unknown, TestServices.CastleDeserializer.Deserialize(unknown) with { Garrison = new Army<Creature>(Constants.CreatureSlotCount) });

            Assert.That(written.GetInt32At(CastleLayout.GarrisonTypes + (3 * 4)), Is.EqualTo(-1));
        }

        private static CastleChunk WithUnknownCreature(CastleChunk chunk)
        {
            var bytes = chunk.ToArray();
            System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(CastleLayout.GarrisonTypes + (3 * 4)), 250);
            return chunk.WithBytes(bytes);
        }

        [Test]
        public void UpgradingADwellingMovesItsCreaturesToTheSlotOfTheUpgradeInTheBytes()
        {
            var (_, upgraded) = WithBuiltAndUpgradedStables();
            var waiting = TestServices.CastleDeserializer.Deserialize(upgraded).CreaturesAvailable[0];

            Assert.That(waiting, Is.GreaterThan(0));
            Assert.That(upgraded.GetUInt16At(upgraded.NameEnd + CastleLayout.UpgradedCreaturesAvailableFromNameEnd), Is.EqualTo(waiting));
            Assert.That(upgraded.GetUInt16At(upgraded.NameEnd + CastleLayout.CreaturesAvailableFromNameEnd), Is.EqualTo(0));
        }

        [Test]
        public void ACountLeftInTheSlotOfTheDwellingBesideItsUpgradeIsReadFromTheUpgradeAndKept()
        {
            var (_, upgraded) = WithBuiltAndUpgradedStables();
            var bytes = upgraded.ToArray();
            ByteBufferHelper.WriteUInt16(bytes, upgraded.NameEnd + CastleLayout.CreaturesAvailableFromNameEnd, 5);
            var both = upgraded.WithBytes(bytes);
            var model = TestServices.CastleDeserializer.Deserialize(both);

            Assert.That(model.CreaturesAvailable[0], Is.EqualTo(upgraded.GetUInt16At(upgraded.NameEnd + CastleLayout.UpgradedCreaturesAvailableFromNameEnd)));
            Assert.That(TestServices.SerializeCastle(both, model).ToArray(), Is.EqualTo(both.ToArray()));
        }

        // Still Water with the Centaur Stables built and then upgraded, each written into the bytes.
        private static (CastleChunk Built, CastleChunk Upgraded) WithBuiltAndUpgradedStables()
        {
            var chunk = StillWater();
            var built = CastleBuilder.Build(TestServices.CastleDeserializer.Deserialize(chunk), Building.RampartCentaurStables);
            var builtChunk = TestServices.SerializeCastle(chunk, built);
            var upgradedChunk = TestServices.SerializeCastle(builtChunk, CastleBuilder.Build(built, Building.RampartUpgCentaurStables));
            return (builtChunk, upgradedChunk);
        }

        private static CastleChunk StillWater() =>
            CastleSamples.Load(CastleSamples.Folder("Bulwark").First()).History.Current.Castles().First(c => c.Name == "Still Water");

        private static byte[] WithByte(CastleChunk chunk, int offset, byte value)
        {
            var bytes = chunk.ToArray();
            bytes[offset] = value;
            return bytes;
        }

        [Test]
        public void ASessionWritesThroughTheHistoryAndUndoRestores()
        {
            var game = CastleSamples.Load(CastleSamples.Folder("Bulwark").First());
            var history = game.History;
            var index = history.Current.Castles().FindByName("Jotunngard");
            var session = history.CreateCastleSession(index);
            var before = session.Current.Owner;

            session.Update(c => c with { Owner = PlayerColor.Pink });
            Assert.That(session.Current.Owner, Is.EqualTo(PlayerColor.Pink));
            Assert.That(TestServices.CastleDeserializer.DeserializeOwner(history.Current.Castles()[index]), Is.EqualTo(PlayerColor.Pink));

            history.Undo();
            Assert.That(session.Current.Owner, Is.EqualTo(before));
        }

        [Test]
        public void ASessionIgnoresAnEditThatChangesNothing()
        {
            var history = CastleSamples.Load(CastleSamples.Folder("Bulwark").First()).History;
            var session = history.CreateCastleSession(0);

            session.Update(c => c with { });

            Assert.That(history.CanUndo, Is.False);
        }

        [Test]
        public void ASessionRejectsACastleThatDoesNotExist()
        {
            var history = CastleSamples.Load(CastleSamples.Folder("Bulwark").First()).History;

            Assert.Throws<ArgumentOutOfRangeException>(() => history.CreateCastleSession(history.Current.Castles().Count));
        }

        /// <summary>The model holds sets and lists, which a record does not compare by value.</summary>
        internal static bool SameCastle(CastleModel a, CastleModel b) =>
            a.Name == b.Name && a.Faction == b.Faction && a.Owner == b.Owner && a.Position == b.Position && a.BuiltThisTurn == b.BuiltThisTurn
            && a.MageGuildLevel == b.MageGuildLevel && a.Buildings.SetEquals(b.Buildings) && a.VisibleBuildings.SetEquals(b.VisibleBuildings)
            && a.EnabledBuildings.SetEquals(b.EnabledBuildings) && a.CreaturesAvailable.SequenceEqual(b.CreaturesAvailable)
            && a.MageGuildSpellCounts.SequenceEqual(b.MageGuildSpellCounts)
            && a.Garrison.Equals(b.Garrison)
            && a.GarrisonedHeroId == b.GarrisonedHeroId && a.VisitingHeroId == b.VisitingHeroId;
    }
}
