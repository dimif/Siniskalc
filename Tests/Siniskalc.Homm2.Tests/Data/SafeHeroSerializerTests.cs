namespace Siniskalc.Homm2.Tests.Data
{
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// The bytes a hero model writes into the record of a hero, stated one field at a time on a record made by hand: a change of the
    /// model moves exactly the bytes of that field, and a byte the model reads the same way is left as the savegame has it.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class SafeHeroSerializerTests
    {
        private readonly SafeHeroSerializer serializer = new();

        private static HeroChunk Record(Action<byte[]>? prepare = null)
        {
            var bytes = new byte[HeroLayout.BaseRecordSize];
            prepare?.Invoke(bytes);
            return new HeroChunk("Maximus", bytes, 0);
        }

        private byte[] Written(HeroChunk chunk, HeroModel model) => this.serializer.Serialize(chunk, model).ToArray();

        /// <summary>
        /// The record as the hero of <see cref="DifferSamples"/> writes it into a blank one, with <paramref name="prepare"/> applied to it afterwards: the starting point of each test.
        /// </summary>
        private HeroChunk Baseline(Action<byte[]>? prepare = null)
        {
            var blank = Record();
            return Record(bytes =>
            {
                this.Written(blank, DifferSamples.Hero()).CopyTo(bytes, 0);
                prepare?.Invoke(bytes);
            });
        }

        [Test]
        public void TheSpellPointsAreWrittenInTwoBytesLeastSignificantFirst()
        {
            var chunk = this.Baseline();

            var written = this.Written(chunk, DifferSamples.Hero() with { SpellPoints = 300 });

            Assert.That(ByteDiff.Of(chunk.ToArray(), written), Is.EqualTo(new[] { (HeroLayout.SpellPoints, (byte)10, (byte)0x2C), (HeroLayout.SpellPoints + 1, (byte)0, (byte)0x01) }));
        }

        [Test]
        public void TheTileIsWrittenAsTwoNumbersOfFourBytes()
        {
            var chunk = this.Baseline();

            var written = this.Written(chunk, DifferSamples.Hero() with { Position = DifferSamples.Hero().Position with { X = 12, Y = 34 } });

            Assert.That(ByteDiff.Of(chunk.ToArray(), written), Is.EqualTo(new[] { (HeroLayout.X, (byte)10, (byte)12), (HeroLayout.Y, (byte)12, (byte)34) }));
        }

        [Test]
        public void ACoordinateTooBigForTheModelIsKeptWhileTheModelSaysTheLargestItCanHold()
        {
            var chunk = this.Baseline(bytes => BitConverter.GetBytes(0xFFFF_FFF0u).CopyTo(bytes, HeroLayout.X));
            var model = DifferSamples.Hero() with { SpellPoints = 11, Position = DifferSamples.Hero().Position with { X = int.MaxValue } };

            var written = this.Written(chunk, model);

            Assert.That(ByteDiff.Of(chunk.ToArray(), written).Select(d => d.Offset), Is.EqualTo(new[] { HeroLayout.SpellPoints }));
            Assert.That(BitConverter.ToUInt32(written, HeroLayout.X), Is.EqualTo(0xFFFF_FFF0u));
        }

        [Test]
        public void AFacingTheGameDoesNotKnowIsKeptWhileTheModelSaysNorthAndReplacedWhenItSaysAnother()
        {
            var chunk = this.Baseline(bytes => bytes[HeroLayout.Facing] = 0x55);

            Assert.That(ByteDiff.Of(chunk.ToArray(), this.Written(chunk, DifferSamples.Hero())), Is.Empty);

            var turned = this.Written(chunk, DifferSamples.Hero() with { Position = DifferSamples.Hero().Position with { FacingDirection = FacingDirection.South } });
            Assert.That(ByteDiff.Of(chunk.ToArray(), turned), Is.EqualTo(new[] { (HeroLayout.Facing, (byte)0x55, (byte)4) }));
        }

        [Test]
        public void AnOwnerByteThatIsNoColourIsKeptWhileTheModelSaysNeutralAndReplacedWhenItSaysAColour()
        {
            var chunk = this.Baseline(bytes => bytes[HeroLayout.Owner] = 0x09);

            Assert.That(ByteDiff.Of(chunk.ToArray(), this.Written(chunk, DifferSamples.Hero() with { Owner = PlayerColor.Neutral })), Is.Empty);
            Assert.That(ByteDiff.Of(chunk.ToArray(), this.Written(chunk, DifferSamples.Hero() with { Owner = PlayerColor.Green })), Is.EqualTo(new[] { (HeroLayout.Owner, (byte)0x09, PlayerColor.Green.Code) }));
        }

        [Test]
        public void MoraleAndLuckForTheNextBattleAreSignedBytes()
        {
            var chunk = this.Baseline();

            var written = this.Written(chunk, DifferSamples.Hero() with { NextBattle = new NextBattleBonus(-1, 3) });

            Assert.That(ByteDiff.Of(chunk.ToArray(), written), Is.EqualTo(new[] { (HeroLayout.BattleMorale, (byte)0, (byte)0xFF), (HeroLayout.BattleLuck, (byte)0, (byte)3) }));
        }

        [Test]
        public void AnArtifactIsWrittenAsItsCodeInItsSlotAndAnEmptiedSlotGetsTheEmptyMark()
        {
            var chunk = this.Baseline();

            var worn = this.Written(chunk, DifferSamples.Hero() with { Artifacts = Artifacts.None.WithSlot(2, Artifact.UltimateSwordOfDominion) });
            Assert.That(ByteDiff.Of(chunk.ToArray(), worn), Is.EqualTo(new[] { (HeroLayout.Artifacts + 2, (byte)0xFF, (byte)1) }));

            var taken = this.Written(this.serializer.Serialize(chunk, DifferSamples.Hero() with { Artifacts = Artifacts.None.WithSlot(2, Artifact.UltimateSwordOfDominion) }), DifferSamples.Hero());
            Assert.That(taken, Is.EqualTo(chunk.ToArray()));
        }

        [Test]
        public void ASpellOfTheSpellBookIsMarkedByOneInItsPlace()
        {
            var chunk = this.Baseline();

            var written = this.Written(chunk, DifferSamples.Hero() with { Spells = SpellSet<Spell>.None.With(Spell.Fireball) });

            Assert.That(ByteDiff.Of(chunk.ToArray(), written), Is.EqualTo(new[] { (HeroLayout.Spells + 1, (byte)0, (byte)1) }));
        }

        [Test]
        public void ASecondarySkillIsWrittenWithItsLevelItsOrderAndTheCountOfSkills()
        {
            var chunk = this.Baseline();

            var written = this.Written(chunk, DifferSamples.Hero() with { SecondarySkills = SecondarySkills.None.WithSkill(SecondarySkill.Archery, SkillLevel.Expert) });

            Assert.That(ByteDiff.Of(chunk.ToArray(), written), Is.EqualTo(new[]
            {
                (HeroLayout.SkillLevels + 1, (byte)0, (byte)3),
                (HeroLayout.SkillOrder + 1, (byte)0, (byte)1),
                (HeroLayout.SkillCount, (byte)0, (byte)1),
            }));
        }

        [Test]
        public void ANewLevelIsWrittenWithItsExperienceAndNothingElseMoves()
        {
            var chunk = this.Baseline();
            var model = DifferSamples.Hero().WithLevel(5);

            var written = this.Written(chunk, model);

            Assert.That(written[HeroLayout.Level], Is.EqualTo(5));
            Assert.That(BitConverter.ToUInt32(written, HeroLayout.Experience), Is.EqualTo((uint)model.HeroLevel.Experience));
            Assert.That(ByteDiff.Of(chunk.ToArray(), written).Select(d => d.Offset), Is.All.InRange(HeroLayout.Experience, HeroLayout.Level));
        }

        [Test]
        public void ANumberOutsideWhatTheRecordHoldsIsRefusedAndTheRecordIsNotChanged()
        {
            var chunk = this.Baseline();
            var before = chunk.ToArray();

            Assert.That(() => this.serializer.Serialize(chunk, DifferSamples.Hero() with { SpellPoints = 70_000 }), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(chunk.ToArray(), Is.EqualTo(before));
        }
    }
}
