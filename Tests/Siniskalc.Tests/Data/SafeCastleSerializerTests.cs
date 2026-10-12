using Siniskalc;
using Siniskalc.Data;
using Siniskalc.Entities;
using Siniskalc.Models;
using Siniskalc.Runtime;

namespace Siniskalc.Tests.Data
{
    [TestFixture]
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

            var written = TestServices.SerializeCastle(chunk, TestServices.CastleDeserializer.Deserialize(chunk) with { Owner = null });

            Assert.That(written.GetByteAt(CastleLayout.Owner), Is.EqualTo(Constants.ByteOff));
            Assert.That(TestServices.CastleDeserializer.DeserializeOwner(written), Is.Null);
        }

        [Test]
        public void TheGarrisonIsWrittenAndEmptiedSlotsGoBackToMinusOne()
        {
            var chunk = CastleSamples.Load(CastleSamples.Folder("Bulwark").First()).History.Current.Castles().First(c => c.Name == "Still Water");
            var castle = TestServices.CastleDeserializer.Deserialize(chunk);

            var filled = TestServices.SerializeCastle(chunk, castle with { Garrison = castle.Garrison.WithSlot(2, Creature.Centaur, 1234).WithSlot(7, Creature.GoldDragon, 3) });
            var garrison = TestServices.CastleDeserializer.DeserializeGarrison(filled);
            Assert.That(garrison.GetSlot(2), Is.EqualTo(new ArmySlot(2, Creature.Centaur.Code, 1234)));
            Assert.That(garrison.GetSlot(7), Is.EqualTo(new ArmySlot(7, Creature.GoldDragon.Code, 3)));
            Assert.That(garrison.Slots, Has.Length.EqualTo(2));

            var emptied = TestServices.SerializeCastle(filled, TestServices.CastleDeserializer.Deserialize(filled) with { Garrison = new Army([]) });
            Assert.That(emptied.HasSameBytes(chunk), Is.True);
        }

        [Test]
        public void AnOwnerByteThatIsNoColourIsKeptWhileTheCastleStaysNeutral()
        {
            var chunk = StillWater();
            var odd = chunk.WithBytes(WithByte(chunk, CastleLayout.Owner, 0x09));

            var written = TestServices.SerializeCastle(odd, TestServices.CastleDeserializer.Deserialize(odd));

            Assert.That(written.GetByteAt(CastleLayout.Owner), Is.EqualTo(0x09));
            Assert.That(TestServices.SerializeCastle(odd, TestServices.CastleDeserializer.Deserialize(odd) with { Owner = PlayerColor.Red }).GetByteAt(CastleLayout.Owner), Is.EqualTo((byte)PlayerColor.Red));
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
        public void AGarrisonSlotWithACreatureTheEditorDoesNotKnowIsKept()
        {
            var chunk = StillWater();
            var bytes = chunk.ToArray();
            System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(CastleLayout.GarrisonTypes + (3 * 4)), 250);
            var unknown = chunk.WithBytes(bytes);

            var written = TestServices.SerializeCastle(unknown, TestServices.CastleDeserializer.Deserialize(unknown) with { Garrison = new Army([]) });

            Assert.That(written.GetInt32At(CastleLayout.GarrisonTypes + (3 * 4)), Is.EqualTo(250));
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
            && a.MageGuildLevel == b.MageGuildLevel && a.Buildings.SetEquals(b.Buildings) && a.CurrentBuildings.SetEquals(b.CurrentBuildings)
            && a.EnabledBuildings.SetEquals(b.EnabledBuildings) && a.CreaturesAvailable.SequenceEqual(b.CreaturesAvailable)
            && a.UpgradedCreaturesAvailable.SequenceEqual(b.UpgradedCreaturesAvailable) && a.SpellCounts.SequenceEqual(b.SpellCounts)
            && a.Garrison.Slots.OrderBy(s => s.SlotNumber).SequenceEqual(b.Garrison.Slots.OrderBy(s => s.SlotNumber))
            && a.GarrisonedHeroId == b.GarrisonedHeroId && a.VisitingHeroId == b.VisitingHeroId;
    }
}
