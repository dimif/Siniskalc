using Siniskalc.Data;
using Siniskalc.Entities;
using Siniskalc.Models;

namespace Siniskalc.Tests.Data
{
    [TestFixture]
    internal sealed class HeroDeserializerTests
    {
        private HeroDeserializer deserializer;

        [SetUp]
        public void SetUp()
        {
            this.deserializer = new HeroDeserializer(new HeroOffsets(Siniskalc.Entities.GameVersion.HornOfTheAbyss), TestServices.VersionEntities.For(Siniskalc.Entities.GameVersion.HornOfTheAbyss));
        }

        [Test]
        public void DeserializingHeroDeserializes()
        {
            var bytes = File.ReadAllBytes(Path.Combine("TestData", "Heroes", "hero_maximus_pre_hota.bin"));
            var bytesChunk = new HeroChunk("Maximus", bytes, 0);

            var expectedSkills = new HeroPrimarySkills(27, 22, 16, 11);

            var primarySkills = this.deserializer.DeserializePrimarySkills(bytesChunk);

            Assert.That(() => primarySkills, Is.EqualTo(expectedSkills));

            var ExpectedClass = HeroClass.Knight;

            var heroClass = this.deserializer.DeserializeClass(bytesChunk);

            Assert.That(() => heroClass, Is.EqualTo(ExpectedClass));

            const bool ExpectedHasSpellBook = true;

            var hasSpellBook = this.deserializer.DeserializeHasSpellBook(bytesChunk);

            Assert.That(() => hasSpellBook, Is.EqualTo(ExpectedHasSpellBook));

            // 0x0D Arch Angel
            var expectedCreatureSlots = new List<ArmySlot>
            {
                new ArmySlot(4, 0x0D, 42),
                new ArmySlot(5, 0x0D, 41)
            };

            var creatures = this.deserializer.DeserializeCreatures(bytesChunk);

            Assert.That(() => creatures.Slots, Has.Length.EqualTo(2));

            Assert.That(() => creatures.Slots[0], Is.EqualTo(expectedCreatureSlots[0]));
            Assert.That(() => creatures.Slots[1], Is.EqualTo(expectedCreatureSlots[1]));
        }

        [Test]
        public void DeserializingFullMaximusDumpDeserializesHero()
        {
            var bytes = File.ReadAllBytes(Path.Combine("TestData", "Heroes", "hero_maximus_hota.bin"));
            var bytesChunk = new HeroChunk("Maximus", bytes, 0);

            var hero = this.deserializer.Deserialize(bytesChunk);

            Assert.That(hero.Owner, Is.EqualTo(PlayerColor.Purple));
            Assert.That(hero.MovementPoints.CurrentMovementPoints, Is.EqualTo(0u));
            Assert.That(hero.MovementPoints.MaxMovementPoints, Is.EqualTo(1760u));
            Assert.That(hero.SpellPoints, Is.EqualTo((ushort)175));

            Assert.That(hero.Position.X, Is.EqualTo((short)69));
            Assert.That(hero.Position.Y, Is.EqualTo((short)7));
            Assert.That(hero.Position.Z, Is.EqualTo((short)0));
            Assert.That(hero.Position.FacingDirection, Is.EqualTo(FacingDirection.East));

            Assert.That(hero.HeroLevel.Level, Is.EqualTo((short)32));
            Assert.That(hero.HeroLevel.Experience, Is.EqualTo(779943));

            Assert.That(hero.HeroClass, Is.EqualTo(HeroClass.Knight));

            Assert.That(hero.PrimarySkills, Is.EqualTo(new HeroPrimarySkills(32, 32, 35, 23)));

            var expectedSkills = new[]
            {
                (SecondarySkill.Wisdom, SkillLevel.Expert),
                (SecondarySkill.EarthMagic, SkillLevel.Expert),
                (SecondarySkill.Ballistics, SkillLevel.Expert),
                (SecondarySkill.AirMagic, SkillLevel.Expert),
                (SecondarySkill.Armorer, SkillLevel.Expert),
                (SecondarySkill.FireMagic, SkillLevel.Expert),
                (SecondarySkill.WaterMagic, SkillLevel.Expert),
                (SecondarySkill.Necromancy, SkillLevel.Expert),
            };

            for (var i = 0; i < expectedSkills.Length; i++)
            {
                var slotSkill = hero.SecondarySkills.GetSkillInSlot(i);
                Assert.That(slotSkill, Is.EqualTo(expectedSkills[i].Item1), $"Skill slot {i + 1}");
                Assert.That(hero.SecondarySkills.GetSkillLevel(expectedSkills[i].Item1), Is.EqualTo(expectedSkills[i].Item2), $"Skill level slot {i + 1}");
            }

            var expectedCreatures = new[]
            {
                new ArmySlot(1, Creature.Skeleton.Code, 15),
                new ArmySlot(3, Creature.RoyalGriffin.Code, 832),
                new ArmySlot(4, Creature.Archangel.Code, 145),
                new ArmySlot(5, Creature.Zealot.Code, 411),
                new ArmySlot(6, Creature.Champion.Code, 276),
                new ArmySlot(7, Creature.GoldDragon.Code, 63),
            };

            Assert.That(hero.Army.Slots, Is.EqualTo(expectedCreatures));

            var expectedArtifacts = new Dictionary<ArtifactSlots, Artifact>
            {
                [ArtifactSlots.Helmet] = Artifact.HelmOfChaos,
                [ArtifactSlots.Weapon] = Artifact.SwordOfJudgement,
                [ArtifactSlots.Ring1] = Artifact.RingOfTheWayfarer,
                [ArtifactSlots.Necklace] = Artifact.NecklaceOfSwiftness,
                [ArtifactSlots.Ring2] = Artifact.EquestriansGloves,
                [ArtifactSlots.Shield] = Artifact.BucklerOfTheGnollKing,
                [ArtifactSlots.Torso] = Artifact.TitansCuirass,
                [ArtifactSlots.Feet] = Artifact.SandalsOfTheSaint,
                [ArtifactSlots.Cape] = Artifact.VampiresCowl,
                [ArtifactSlots.Misc1] = Artifact.HideousMask,
                [ArtifactSlots.Misc2] = Artifact.RunesOfImminency,
                [ArtifactSlots.Misc3] = Artifact.CardsOfProphecy,
                [ArtifactSlots.Misc4] = Artifact.MysticOrbOfSpellPoints,
                [ArtifactSlots.Misc5] = Artifact.CharmOfSpellPoints,
            };

            var actualArtifacts = hero.EquippedArtifacts.GetEquippedArtifacts().ToDictionary(t => t.Item1, t => t.Item2);

            Assert.That(actualArtifacts, Is.EquivalentTo(expectedArtifacts));

            Assert.That(hero.WarMachines, Is.EquivalentTo(new[] { WarMachine.FirstAidTent }));
        }

        [Test]
        public void DeserializingHasSpellBookForHeroWithDeserializes()
        {
            var bytes = File.ReadAllBytes(Path.Combine("TestData", "Heroes", "hero_orrin_with_spellbook.bin"));
            var bytesChunk = new HeroChunk("Orrin", bytes, 0);

            const bool ExpectedHasSpellBook = true;

            var hasSpellBook = this.deserializer.DeserializeHasSpellBook(bytesChunk);

            Assert.That(() => hasSpellBook, Is.EqualTo(ExpectedHasSpellBook));
        }

        [Test]
        public void DeserializingHasSpellBookForHeroWithoutDeserializes()
        {
            var bytes = File.ReadAllBytes(Path.Combine("TestData", "Heroes", "hero_orrin_no_spellbook.bin"));
            var bytesChunk = new HeroChunk("Orrin", bytes, 0);

            const bool ExpectedHasSpellBook = false;

            var hasSpellBook = this.deserializer.DeserializeHasSpellBook(bytesChunk);

            Assert.That(() => hasSpellBook, Is.EqualTo(ExpectedHasSpellBook));
        }

        [Test]
        public void SpellBookKeepsOnlyWhatNothingExplainsAsOtherAvailable()
        {
            var offsets = new HeroOffsets(Siniskalc.Entities.GameVersion.HornOfTheAbyss);
            var bytes = File.ReadAllBytes(Path.Combine("TestData", "Heroes", "hero_maximus_pre_hota.bin"));
            foreach (var spell in Spell.All)
            {
                bytes[offsets[HeroOffsetName.SpellBookSpells] + spell.Code] = 0;
                bytes[offsets[HeroOffsetName.AvailableSpells] + spell.Code] = 0;
            }

            bytes[offsets[HeroOffsetName.SpellBookSpells] + Spell.Haste.Code] = 1;
            foreach (var spell in new[] { Spell.Haste, Spell.Fireball, Spell.Fly })
            {
                bytes[offsets[HeroOffsetName.AvailableSpells] + spell.Code] = 1;
            }

            var tome = new EquippedArtifacts(new Dictionary<ArtifactSlots, Artifact>()).WithEquipped(ArtifactSlots.Misc1, Artifact.TomeOfFireMagic);
            var book = this.deserializer.DeserializeSpellBook(new HeroChunk("Maximus", bytes, 0), tome)
                .WithGrants(new GrantedSpellsResolver(Siniskalc.Entities.GameVersion.HornOfTheAbyss).ResolveGrantedSpells(tome), false);

            Assert.That(book.Source(Spell.Fireball), Does.Contain("Tome of Fire Magic"));
            Assert.That(book.Source(Spell.Fly), Is.EqualTo("Unknown source"));
            Assert.That(book.WithGrants(new Dictionary<Spell, string>(), false).IsAvailable(Spell.Fireball), Is.False);
        }
    }
}
