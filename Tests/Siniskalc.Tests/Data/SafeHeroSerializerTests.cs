using System.Buffers.Binary;
using Siniskalc;
using Siniskalc.Data;
using Siniskalc.Entities;
using Siniskalc.Models;

namespace Siniskalc.Tests.Data
{
    [TestFixture]
    internal sealed class SafeHeroSerializerTests
    {
        private static readonly HeroOffsets HotaOffsets = new(GameVersion.HornOfTheAbyss);

        private HeroDeserializer deserializer;
        private SafeHeroSerializer serializer;

        [SetUp]
        public void SetUp()
        {
            var entities = TestServices.VersionEntities.For(HotaOffsets.GameVersion);
            this.deserializer = new HeroDeserializer(HotaOffsets, entities);
            this.serializer = new SafeHeroSerializer(HotaOffsets, entities);
        }

        // The short dumps end at the backpack, so only the classic layouts fit; that still covers their offsets.
        [TestCase("hero_maximus_hota.bin", GameVersion.HornOfTheAbyss)]
        [TestCase("hero_maximus_hota.bin", GameVersion.ShadowOfDeath)]
        [TestCase("hero_orrin_with_spellbook.bin", GameVersion.ShadowOfDeath)]
        [TestCase("hero_orrin_no_spellbook.bin", GameVersion.ShadowOfDeath)]
        [TestCase("hero_orrin_no_spellbook.bin", GameVersion.RestorationOfErathia)]
        [TestCase("hero_maximus_pre_hota.bin", GameVersion.ArmageddonsBlade)]
        public void RoundTripKeepsBytesIdentical(string file, GameVersion version)
        {
            var offsets = new HeroOffsets(version);
            var bytes = ReadDump(file);
            var chunk = new HeroChunk("hero", bytes, 0);

            var model = new HeroDeserializer(offsets, TestServices.VersionEntities.For(offsets.GameVersion)).Deserialize(chunk);
            var result = new SafeHeroSerializer(offsets, TestServices.VersionEntities.For(offsets.GameVersion)).Serialize(chunk, model);

            Assert.That(result.ToArray(), Is.EqualTo(bytes));
        }

        [Test]
        public void AClassThisEditorDoesNotKnowReadsAsKnightAndIsKeptWhileTheHeroStaysAKnight()
        {
            var bytes = ReadDump("hero_maximus_hota.bin");
            bytes[HotaOffsets[HeroOffsetName.Class]] = 0xC8;
            var chunk = new HeroChunk("Maximus", bytes, 0);

            var model = this.deserializer.Deserialize(chunk);
            Assert.That(model.HeroClass, Is.EqualTo(HeroClass.Knight));
            Assert.That(this.serializer.Serialize(chunk, model).ToArray(), Is.EqualTo(bytes));

            var changed = this.serializer.Serialize(chunk, model with { HeroClass = HeroClass.Cleric });
            Assert.That(this.deserializer.DeserializeClass(changed), Is.EqualTo(HeroClass.Cleric));
        }

        [Test]
        public void AFacingThatIsNoDirectionReadsAsNorthAndIsKeptWhileTheHeroKeepsFacingNorth()
        {
            var bytes = ReadDump("hero_maximus_hota.bin");
            bytes[HotaOffsets[HeroOffsetName.Facing]] = 0x63;
            var chunk = new HeroChunk("Maximus", bytes, 0);

            var model = this.deserializer.Deserialize(chunk);
            Assert.That(model.Position.FacingDirection, Is.EqualTo(FacingDirection.North));
            Assert.That(this.serializer.Serialize(chunk, model).ToArray(), Is.EqualTo(bytes));

            var turned = this.serializer.Serialize(chunk, model with { Position = model.Position with { FacingDirection = FacingDirection.West } });
            Assert.That(this.deserializer.DeserializePosition(turned).FacingDirection, Is.EqualTo(FacingDirection.West));
        }

        [Test]
        public void EditedPrimarySkillsAreWritten()
        {
            var bytes = ReadDump("hero_maximus_pre_hota.bin");
            var chunk = new HeroChunk("Maximus", bytes, 0);

            var result = this.serializer.SerializePrimarySkills(chunk, new HeroPrimarySkills(1, 2, 3, 4));

            Assert.That(this.deserializer.DeserializePrimarySkills(result), Is.EqualTo(new HeroPrimarySkills(1, 2, 3, 4)));
            Assert.That(chunk.ToArray(), Is.EqualTo(bytes));
        }

        [Test]
        public void LevelIsASingleByte()
        {
            var bytes = ReadDump("hero_maximus_hota.bin");
            var levelOffset = HotaOffsets[HeroOffsetName.Level];
            bytes[levelOffset + 1] = 0x7F;
            var chunk = new HeroChunk("Maximus", bytes, 0);

            Assert.That(this.deserializer.DeserializeLevel(chunk).Level, Is.EqualTo((short)32));

            var result = this.serializer.SerializeLevel(chunk, new HeroLevel { Level = 10, Experience = 1000 });

            Assert.That(this.deserializer.DeserializeLevel(result).Level, Is.EqualTo((short)10));
            Assert.That(result.GetByteAt(levelOffset + 1), Is.EqualTo(0x7F));
        }

        [Test]
        public void EditedCreaturesAreWritten()
        {
            var chunk = new HeroChunk("Maximus", ReadDump("hero_maximus_hota.bin"), 0);
            var creatures = new Army(
            [
                new ArmySlot(2, Creature.Archangel.Code, 7),
                new ArmySlot(3, Creature.RoyalGriffin.Code, 5),
            ]);

            var result = this.serializer.SerializeCreatures(chunk, creatures);

            Assert.That(this.deserializer.DeserializeCreatures(result).Slots, Is.EquivalentTo(creatures.Slots));
            Assert.That(result.GetUInt32At(HotaOffsets[HeroOffsetName.Creatures]), Is.EqualTo(Constants.BlankId));
        }

        [Test]
        public void RemovingASecondarySkillClearsItsSlotAndLevel()
        {
            var chunk = new HeroChunk("Maximus", ReadDump("hero_maximus_hota.bin"), 0);
            var skills = this.deserializer.DeserializeSecondarySkills(chunk);
            var removed = skills.GetSkillInSlot(skills.SlotCount - 1)!;

            var levels = SecondarySkill.All
                .Where(s => s.IsAvailableIn(GameVersion.HornOfTheAbyss))
                .ToDictionary(s => s, s => s == removed ? null : skills.GetSkillLevel(s));
            var slots = Enumerable.Range(0, skills.SlotCount - 1).Select(skills.GetSkillInSlot).OfType<SecondarySkill>().ToArray();

            var result = this.serializer.SerializeSecondarySkills(chunk, new SecondarySkills(levels, slots));
            var read = this.deserializer.DeserializeSecondarySkills(result);

            Assert.That(read.SlotCount, Is.EqualTo(skills.SlotCount - 1));
            Assert.That(read.GetSkillLevel(removed), Is.Null);
            Assert.That(result.GetByteAt(HotaOffsets[HeroOffsetName.SkillSlots] + removed.Code), Is.EqualTo(0));
            Assert.That(Enumerable.Range(0, read.SlotCount).Select(read.GetSkillInSlot), Is.EqualTo(slots));
        }

        [Test]
        public void ArtifactAddedToBackpackHasBlankSpellId()
        {
            var chunk = new HeroChunk("Maximus", ReadDump("hero_maximus_hota.bin"), 0);
            var backpack = this.deserializer.DeserializeBackpack(chunk);
            var emptySlot = Enumerable.Range(0, backpack.All.Count).Last(i => backpack.All[i] == null);

            var result = this.serializer.SerializeBackpack(chunk, backpack.WithSlot(emptySlot, Artifact.HelmOfChaos));

            Assert.That(this.deserializer.DeserializeBackpack(result).All[emptySlot]?.Art, Is.EqualTo(Artifact.HelmOfChaos));
            var offset = HotaOffsets[HeroOffsetName.Backpack] + emptySlot * Constants.BackpackSlotSize;
            Assert.That(result.GetUInt32At(offset + 4), Is.EqualTo(Constants.BlankId));
        }

        [Test]
        public void SpellScrollKeepsItsSpellThroughSerialization()
        {
            var chunk = new HeroChunk("Maximus", ReadDump("hero_maximus_hota.bin"), 0);
            var backpack = this.deserializer.DeserializeBackpack(chunk);
            var emptySlot = Enumerable.Range(0, backpack.All.Count).Last(i => backpack.All[i] == null);
            var offset = HotaOffsets[HeroOffsetName.Backpack] + emptySlot * Constants.BackpackSlotSize;

            var written = this.serializer.SerializeBackpack(chunk, backpack.WithSlot(emptySlot, Artifact.SpellScroll, Spell.TownPortal));

            Assert.That(written.GetBytesAt(offset, 8), Is.EqualTo(new byte[] { Artifact.SpellScroll.Code, 0, 0, 0, Spell.TownPortal.Code, 0, 0, 0 }));
            var read = this.deserializer.DeserializeBackpack(written).All[emptySlot];
            Assert.That(read?.Art, Is.EqualTo(Artifact.SpellScroll));
            Assert.That(read?.Spell, Is.EqualTo(Spell.TownPortal));

            // Writing the same backpack back changes nothing, and another spell replaces only the spell.
            Assert.That(this.serializer.SerializeBackpack(written, this.deserializer.DeserializeBackpack(written)).ToArray(), Is.EqualTo(written.ToArray()));
            var changed = this.serializer.SerializeBackpack(written, backpack.WithSlot(emptySlot, Artifact.SpellScroll, Spell.Fly));
            Assert.That(this.deserializer.DeserializeBackpack(changed).All[emptySlot]?.Spell, Is.EqualTo(Spell.Fly));

            // Swapping the scroll for another artifact leaves no spell behind.
            var replaced = this.serializer.SerializeBackpack(changed, backpack.WithSlot(emptySlot, Artifact.HelmOfChaos));
            Assert.That(replaced.GetUInt32At(offset + 4), Is.EqualTo(Constants.BlankId));
            Assert.That(this.deserializer.DeserializeBackpack(replaced).All[emptySlot]?.Spell, Is.Null);
        }

        [Test]
        public void AWornSpellScrollKeepsItsSpellThroughSerialization()
        {
            var chunk = new HeroChunk("Maximus", ReadDump("hero_maximus_hota.bin"), 0);
            var offset = HotaOffsets[HeroOffsetName.Item5];
            var equipped = this.deserializer.DeserializeEquippedArtifacts(chunk).WithUnequipped(ArtifactSlots.Misc5);

            var written = this.serializer.SerializeEquippedArtifacts(chunk, equipped.WithEquipped(ArtifactSlots.Misc5, Artifact.SpellScroll, Spell.TownPortal));

            Assert.That(written.GetBytesAt(offset, 8), Is.EqualTo(new byte[] { Artifact.SpellScroll.Code, 0, 0, 0, Spell.TownPortal.Code, 0, 0, 0 }));
            var read = this.deserializer.DeserializeEquippedArtifacts(written);
            Assert.That(read.GetArtifact(ArtifactSlots.Misc5), Is.EqualTo(Artifact.SpellScroll));
            Assert.That(read.GetScrollSpell(ArtifactSlots.Misc5), Is.EqualTo(Spell.TownPortal));

            // Writing the same artifacts back changes nothing, and another spell replaces only the spell.
            Assert.That(this.serializer.SerializeEquippedArtifacts(written, read).ToArray(), Is.EqualTo(written.ToArray()));
            var changed = this.serializer.SerializeEquippedArtifacts(written, read.WithEquipped(ArtifactSlots.Misc5, Artifact.SpellScroll, Spell.Fly));
            Assert.That(this.deserializer.DeserializeEquippedArtifacts(changed).GetScrollSpell(ArtifactSlots.Misc5), Is.EqualTo(Spell.Fly));

            // A scroll without a known spell keeps the spell bytes it has.
            var unknown = this.serializer.SerializeEquippedArtifacts(written, read.WithEquipped(ArtifactSlots.Misc5, Artifact.SpellScroll));
            Assert.That(unknown.ToArray(), Is.EqualTo(written.ToArray()));
        }

        [Test]
        public void EquippingIntoBlankSlotWritesAValidArtifactSlot()
        {
            var bytes = ReadDump("hero_maximus_hota.bin");
            var offset = HotaOffsets[HeroOffsetName.Item5];

            // A blank slot may hold anything after its 4 blank id bytes.
            BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(offset), Constants.BlankId);
            BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(offset + 4), 0x12345678);
            var chunk = new HeroChunk("Maximus", bytes, 0);
            var equipped = this.deserializer.DeserializeEquippedArtifacts(chunk);
            Assert.That(equipped.GetArtifact(ArtifactSlots.Misc5), Is.Null);

            var result = this.serializer.SerializeEquippedArtifacts(chunk, equipped.WithEquipped(ArtifactSlots.Misc5, Artifact.CharmOfSpellPoints));

            Assert.That(result.GetBytesAt(offset, 8), Is.EqualTo(new byte[] { Artifact.CharmOfSpellPoints.Code, 0, 0, 0, 0xFF, 0xFF, 0xFF, 0xFF }));
            Assert.That(this.deserializer.DeserializeEquippedArtifacts(result).GetArtifact(ArtifactSlots.Misc5), Is.EqualTo(Artifact.CharmOfSpellPoints));
        }

        [Test]
        public void UnequippingWritesABlankSlot()
        {
            var chunk = new HeroChunk("Maximus", ReadDump("hero_maximus_hota.bin"), 0);
            var equipped = this.deserializer.DeserializeEquippedArtifacts(chunk);

            var result = this.serializer.SerializeEquippedArtifacts(chunk, equipped.WithUnequipped(ArtifactSlots.Helmet));

            Assert.That(result.GetBytesAt(HotaOffsets[HeroOffsetName.Helmet], 8), Is.All.EqualTo(0xFF));
            Assert.That(this.deserializer.DeserializeEquippedArtifacts(result).GetArtifact(ArtifactSlots.Helmet), Is.Null);
        }

        [Test]
        public void CombinationArtifactReservesItsOtherSlots()
        {
            var chunk = new HeroChunk("Maximus", ReadDump("hero_maximus_hota.bin"), 0);
            var empty = new EquippedArtifacts(new Dictionary<ArtifactSlots, Artifact>());
            var counters = HotaOffsets.ReservedSlotCounters.ToDictionary(c => c.Group, c => c.Offset);

            var cornucopia = this.serializer.SerializeEquippedArtifacts(chunk, empty.WithEquipped(ArtifactSlots.Misc1, Artifact.Cornucopia));

            Assert.That(cornucopia.GetByteAt(counters[ArtifactSlots.Cape]), Is.EqualTo(1));
            Assert.That(cornucopia.GetByteAt(counters[ArtifactSlots.Rings]), Is.EqualTo(2));
            Assert.That(cornucopia.GetByteAt(counters[ArtifactSlots.Misc]), Is.EqualTo(0));
            Assert.That(cornucopia.GetByteAt(counters[ArtifactSlots.Helmet]), Is.EqualTo(0));

            var alliance = this.serializer.SerializeEquippedArtifacts(chunk, empty.WithEquipped(ArtifactSlots.Weapon, Artifact.AngelicAlliance));

            Assert.That(alliance.GetByteAt(counters[ArtifactSlots.Weapon]), Is.EqualTo(0));
            Assert.That(
                new[] { ArtifactSlots.Helmet, ArtifactSlots.Necklace, ArtifactSlots.Torso, ArtifactSlots.Shield, ArtifactSlots.Feet }.Select(g => alliance.GetByteAt(counters[g])),
                Is.All.EqualTo(1));
            Assert.That(alliance.GetByteAt(counters[ArtifactSlots.Cape]), Is.EqualTo(0));

            var sharpshooter = this.serializer.SerializeEquippedArtifacts(chunk, empty.WithEquipped(ArtifactSlots.Misc1, Artifact.BowOfTheSharpshooter));

            Assert.That(sharpshooter.GetByteAt(counters[ArtifactSlots.Misc]), Is.EqualTo(2));
        }

        [Test]
        public void ClassicVersionsHaveNoReservedCounters()
        {
            Assert.That(new HeroOffsets(GameVersion.RestorationOfErathia).ReservedSlotCounters, Is.Empty);
            Assert.That(new HeroOffsets(GameVersion.ArmageddonsBlade).ReservedSlotCounters, Is.Empty);
        }

        [Test]
        public void WarMachinesSharingAnOffsetReplaceEachOther()
        {
            var chunk = new HeroChunk("Maximus", ReadDump("hero_maximus_hota.bin"), 0);

            var withBallista = this.serializer.SerializeWarMachines(chunk, [WarMachine.Ballista, WarMachine.AmmoCart]);
            Assert.That(this.deserializer.DeserializeWarMachines(withBallista), Is.EquivalentTo(new[] { WarMachine.Ballista, WarMachine.AmmoCart }));

            var withCannon = this.serializer.SerializeWarMachines(withBallista, [WarMachine.Cannon]);
            Assert.That(this.deserializer.DeserializeWarMachines(withCannon), Is.EquivalentTo(new[] { WarMachine.Cannon }));
        }

        [Test]
        public void AddingTheSpellBookWritesAnArtifactSlot()
        {
            var classic = new HeroOffsets(GameVersion.ShadowOfDeath);
            var chunk = new HeroChunk("Orrin", ReadDump("hero_orrin_no_spellbook.bin"), 0);

            var result = new SafeHeroSerializer(classic, TestServices.VersionEntities.For(classic.GameVersion)).SerializeHasSpellBook(chunk, true);

            Assert.That(new HeroDeserializer(classic, TestServices.VersionEntities.For(classic.GameVersion)).DeserializeHasSpellBook(result), Is.True);
            Assert.That(result.GetBytesAt(classic[HeroOffsetName.SpellBook], 8), Is.EqualTo(new byte[] { 0, 0, 0, 0, 0xFF, 0xFF, 0xFF, 0xFF }));
        }

        private static byte[] ReadDump(string file) => File.ReadAllBytes(Path.Combine("TestData", "Heroes", file));
    }
}
