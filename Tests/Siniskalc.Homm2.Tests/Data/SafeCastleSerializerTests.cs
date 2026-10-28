namespace Siniskalc.Homm2.Tests.Data
{
    using Siniskalc.Core.Models;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// The bytes a castle model writes into the record of a castle, stated one field at a time on a record made by hand: where the
    /// creatures of a dwelling wait for each level, and that a byte the model reads the same way is left as the savegame has it.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class SafeCastleSerializerTests
    {
        private readonly SafeCastleSerializer serializer = new();

        private static CastleChunk Record(Action<byte[]>? prepare = null)
        {
            var bytes = new byte[CastleLayout.RecordSize];
            prepare?.Invoke(bytes);
            return new CastleChunk("Xabran", bytes, 0);
        }

        private byte[] Written(CastleChunk chunk, CastleModel model) => this.serializer.Serialize(chunk, model).ToArray();

        /// <summary>
        /// The record as <paramref name="model"/> writes it into a blank one, with <paramref name="prepare"/> applied to it afterwards: the starting point of a test.
        /// </summary>
        private CastleChunk Baseline(CastleModel? model = null, Action<byte[]>? prepare = null) =>
            Record(bytes =>
            {
                this.Written(Record(), model ?? DifferSamples.Castle()).CopyTo(bytes, 0);
                prepare?.Invoke(bytes);
            });

        private static IEnumerable<(int Offset, byte Before, byte After)> CreatureBytes(byte[] before, byte[] after) =>
            ByteDiff.Of(before, after).Where(d => d.Offset is >= CastleLayout.Creatures and < CastleLayout.GuildSpells);

        [Test]
        public void TheTileIsWrittenAsTwoBytes()
        {
            var chunk = this.Baseline();

            var written = this.Written(chunk, DifferSamples.Castle() with { Position = new MapPosition(21, 22) });

            Assert.That(ByteDiff.Of(chunk.ToArray(), written), Is.EqualTo(new[] { (CastleLayout.X, (byte)20, (byte)21), (CastleLayout.Y, (byte)21, (byte)22) }));
        }

        [Test]
        public void ACastleOnTheCoastHasTheTileOfItsBoatAndOneOffTheCoastHasTheEmptyMark()
        {
            var chunk = this.Baseline();

            var coast = this.Written(chunk, DifferSamples.Castle() with { ShipyardPosition = new MapPosition(30, 31) });
            Assert.That(ByteDiff.Of(chunk.ToArray(), coast), Is.EqualTo(new[] { (CastleLayout.Boat, (byte)0xFF, (byte)30), (CastleLayout.Boat + 1, (byte)0xFF, (byte)31) }));

            Assert.That(ByteDiff.Of(chunk.ToArray(), this.Written(chunk, DifferSamples.Castle())), Is.Empty);
        }

        [Test]
        public void AFactionByteThatIsNoFactionIsKeptWhileTheModelSaysNeutralAndReplacedWhenItSaysAFaction()
        {
            var chunk = this.Baseline(DifferSamples.Castle() with { Faction = Faction.Neutral }, bytes => bytes[CastleLayout.Faction] = 0x77);
            var kept = this.Written(chunk, DifferSamples.Castle() with { Faction = Faction.Neutral });

            Assert.That(kept[CastleLayout.Faction], Is.EqualTo(0x77));
            Assert.That(this.Written(chunk, DifferSamples.Castle() with { Faction = Faction.Barbarian })[CastleLayout.Faction], Is.EqualTo(Faction.Barbarian.Code));
        }

        [Test]
        public void UpgradingADwellingMovesItsCreaturesFromTheListOfTheDwellingToTheListOfTheUpgradedOnes()
        {
            var chunk = this.Baseline(DifferSamples.Castle().WithDwellingLevel(2, 1).WithCreaturesWaiting(2, 7));

            var written = this.Written(chunk, DifferSamples.Castle().WithDwellingLevel(2, 2).WithCreaturesWaiting(2, 7));

            Assert.That(ByteDiff.Of(chunk.ToArray(), written), Is.EqualTo(new[]
            {
                (CastleLayout.Dwellings, (byte)0x10, (byte)0x00),
                (CastleLayout.Dwellings + 1, (byte)0x00, (byte)0x02),
                (CastleLayout.Creatures + 2, (byte)7, (byte)0),
                (CastleLayout.UpgradedCreatures, (byte)0, (byte)7),
            }));
        }

        [Test]
        public void TakingTheUpgradeDownMovesTheCreaturesBackAndEmptiesTheListOfTheUpgradedOnes()
        {
            var chunk = this.Baseline(DifferSamples.Castle().WithDwellingLevel(2, 2).WithCreaturesWaiting(2, 7));

            var written = this.Written(chunk, DifferSamples.Castle().WithDwellingLevel(2, 1).WithCreaturesWaiting(2, 7));

            Assert.That(CreatureBytes(chunk.ToArray(), written), Is.EqualTo(new[] { (CastleLayout.Creatures + 2, (byte)0, (byte)7), (CastleLayout.UpgradedCreatures, (byte)7, (byte)0) }));
        }

        [Test]
        public void TheDragonsUpgradedTwiceWaitInTheSixthPlaceAndGetTheMarkTheGameShowsThemWith()
        {
            var warlock = DifferSamples.Castle() with { Faction = Faction.Warlock };
            var chunk = this.Baseline(warlock.WithDwellingLevel(6, 2).WithCreaturesWaiting(6, 5));

            var written = this.Written(chunk, warlock.WithDwellingLevel(6, 2).WithCreaturesWaiting(6, 5).WithDwellingLevel(6, 3));

            var mark = CastleLayout.UpgradedCreatures + (CastleLayout.SecondUpgradeMarker * 2);
            Assert.That(CreatureBytes(chunk.ToArray(), written), Is.EqualTo(new[]
            {
                (mark, (byte)5, (byte)CastleLayout.SecondUpgradeMarkerValue),
                (CastleLayout.UpgradedCreatures + 10, (byte)0, (byte)5),
            }));
        }

        [Test]
        public void ACountLeftInTheListOfTheDwellingBesideTheUpgradedOnesIsNotTouchedWhileTheLevelStays()
        {
            // As in Threedom, Hillstone: the second dwelling is upgraded, with 11 in the list of the upgraded and the 14 of before in its own.
            var upgraded = DifferSamples.Castle().WithDwellingLevel(2, 2).WithCreaturesWaiting(2, 11);
            var chunk = this.Baseline(upgraded);
            chunk = chunk.WithBytes(WithBytes(chunk.ToArray(), CastleLayout.Creatures + 2, 14));

            var same = this.Written(chunk, upgraded);
            var fewer = this.Written(chunk, upgraded.WithCreaturesWaiting(2, 9));

            Assert.That(same, Is.EqualTo(chunk.ToArray()));
            Assert.That(ByteDiff.Of(chunk.ToArray(), fewer), Is.EqualTo(new[] { (CastleLayout.UpgradedCreatures, (byte)11, (byte)9) }));
        }

        [Test]
        public void ASpellOfTheMageGuildIsWrittenAsItsNumberMinusOneAndNoSpellGetsTheEmptyMark()
        {
            var chunk = this.Baseline();

            var written = this.Written(chunk, DifferSamples.Castle().WithMageGuildLevel(1).WithMageGuildSpell(1, 0, Spell.Fireball));

            Assert.That(written[CastleLayout.GuildSpells], Is.EqualTo(0));
            Assert.That(chunk.ToArray()[CastleLayout.GuildSpells], Is.EqualTo(0xFF));
            Assert.That(written[CastleLayout.GuildSpellCounts], Is.EqualTo(3));
        }

        [Test]
        public void ACountOrACoordinateOutsideWhatTheRecordHoldsIsRefusedAndTheRecordIsNotChanged()
        {
            var chunk = this.Baseline();
            var before = chunk.ToArray();

            Assert.That(() => this.serializer.Serialize(chunk, DifferSamples.Castle().WithCreaturesWaiting(1, 70_000)), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => this.serializer.Serialize(chunk, DifferSamples.Castle() with { Position = new MapPosition(300, 0) }), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => this.serializer.Serialize(chunk, DifferSamples.Castle() with { MageGuildSpellCounts = Numbers.Of(5, 0, 0, 0, 0) }), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(chunk.ToArray(), Is.EqualTo(before));
        }

        private static byte[] WithBytes(byte[] bytes, int offset, ushort value)
        {
            BitConverter.GetBytes(value).CopyTo(bytes, offset);
            return bytes;
        }
    }
}
