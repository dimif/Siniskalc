using Siniskalc.Heroes.Entities;
using Siniskalc.Heroes.Models;
using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Models;

namespace Siniskalc.Homm3.Tests.Integration.Data
{
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class HeroDeserializerTests
    {
        private HeroDeserializer deserializer;

        [SetUp]
        public void SetUp()
        {
            this.deserializer = new HeroDeserializer(new HeroOffsets(Siniskalc.Homm3.Entities.GameVersion.HornOfTheAbyss), TestServices.VersionEntities.For(Siniskalc.Homm3.Entities.GameVersion.HornOfTheAbyss));
        }

        [Test]
        public void TheSkillsClassSpellBookAndArmyOfAPreHotaHeroAreReadFromItsRecord()
        {
            var bytes = File.ReadAllBytes(Path.Combine("TestData", "Heroes", "hero_maximus_pre_hota.bin"));
            var bytesChunk = new HeroChunk("Maximus", bytes, 0);

            var expectedSkills = new PrimarySkills(27, 22, 16, 11);

            var primarySkills = this.deserializer.DeserializePrimarySkills(bytesChunk);

            Assert.That(() => primarySkills, Is.EqualTo(expectedSkills));

            var ExpectedClass = TestCatalog.For(GameVersion.HornOfTheAbyss).HeroClasses.ById("Knight");

            var heroClass = this.deserializer.DeserializeClass(bytesChunk);

            Assert.That(() => heroClass, Is.EqualTo(ExpectedClass));

            const bool ExpectedHasSpellBook = true;

            var hasSpellBook = this.deserializer.DeserializeHasSpellBook(bytesChunk);

            Assert.That(() => hasSpellBook, Is.EqualTo(ExpectedHasSpellBook));

            // 0x0D Arch Angel
            var expectedCreatureSlots = new ArmySlot<Creature>?[]
            {
                null,
                null,
                null,
                new ArmySlot<Creature>(TestCatalog.For(GameVersion.HornOfTheAbyss).Creatures.Find(0x0D)!, 42),
                new ArmySlot<Creature>(TestCatalog.For(GameVersion.HornOfTheAbyss).Creatures.Find(0x0D)!, 41),
                null,
                null,
            };

            var creatures = this.deserializer.DeserializeCreatures(bytesChunk);

            Assert.That(() => creatures.Filled.Count(), Is.EqualTo(2));
            Assert.That(() => creatures.Slots, Is.EqualTo(expectedCreatureSlots));
        }

        [Test]
        public void TheWholeModelOfMaximusIsReadFromADumpOfHisHotaRecord()
        {
            var bytes = File.ReadAllBytes(Path.Combine("TestData", "Heroes", "hero_maximus_hota.bin"));
            var bytesChunk = new HeroChunk("Maximus", bytes, 0);

            var hero = this.deserializer.Deserialize(bytesChunk);

            Assert.That(hero.Owner, Is.EqualTo(PlayerColor.Purple));
            Assert.That(hero.MovementPoints.Current, Is.EqualTo(0u));
            Assert.That(hero.MovementPoints.Max, Is.EqualTo(1760u));
            Assert.That(hero.SpellPoints, Is.EqualTo((ushort)175));

            Assert.That(hero.Position.X, Is.EqualTo((short)69));
            Assert.That(hero.Position.Y, Is.EqualTo((short)7));
            Assert.That(hero.Position.Level, Is.EqualTo(0));
            Assert.That(hero.Position.FacingDirection, Is.EqualTo(FacingDirection.East));

            Assert.That(hero.HeroLevel.Level, Is.EqualTo((short)32));
            Assert.That(hero.HeroLevel.Experience, Is.EqualTo(779943));

            Assert.That(hero.HeroClass, Is.EqualTo(TestCatalog.For(GameVersion.HornOfTheAbyss).HeroClasses.ById("Knight")));

            Assert.That(hero.PrimarySkills, Is.EqualTo(new PrimarySkills(32, 32, 35, 23)));

            var expectedSkills = new[]
            {
                (TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("Wisdom"), SkillLevel.Expert),
                (TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("EarthMagic"), SkillLevel.Expert),
                (TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("Ballistics"), SkillLevel.Expert),
                (TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("AirMagic"), SkillLevel.Expert),
                (TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("Armorer"), SkillLevel.Expert),
                (TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("FireMagic"), SkillLevel.Expert),
                (TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("WaterMagic"), SkillLevel.Expert),
                (TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("Necromancy"), SkillLevel.Expert),
            };

            for (var i = 0; i < expectedSkills.Length; i++)
            {
                var slotSkill = hero.SecondarySkills.GetSkillInSlot(i);
                Assert.That(slotSkill, Is.EqualTo(expectedSkills[i].Item1), $"Skill slot {i + 1}");
                Assert.That(hero.SecondarySkills.GetSkillLevel(expectedSkills[i].Item1), Is.EqualTo(expectedSkills[i].Item2), $"Skill level slot {i + 1}");
            }

            var expectedCreatures = new ArmySlot<Creature>?[]
            {
                new ArmySlot<Creature>(TestCatalog.For(GameVersion.HornOfTheAbyss).Creatures.ById("Skeleton"), 15),
                null,
                new ArmySlot<Creature>(TestCatalog.For(GameVersion.HornOfTheAbyss).Creatures.ById("RoyalGriffin"), 832),
                new ArmySlot<Creature>(TestCatalog.For(GameVersion.HornOfTheAbyss).Creatures.ById("Archangel"), 145),
                new ArmySlot<Creature>(TestCatalog.For(GameVersion.HornOfTheAbyss).Creatures.ById("Zealot"), 411),
                new ArmySlot<Creature>(TestCatalog.For(GameVersion.HornOfTheAbyss).Creatures.ById("Champion"), 276),
                new ArmySlot<Creature>(TestCatalog.For(GameVersion.HornOfTheAbyss).Creatures.ById("GoldDragon"), 63),
            };

            Assert.That(hero.Army.Slots, Is.EqualTo(expectedCreatures));

            var expectedArtifacts = new Dictionary<ArtifactSlots, Artifact>
            {
                [ArtifactSlots.Helmet] = TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("HelmOfChaos"),
                [ArtifactSlots.Weapon] = TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("SwordOfJudgement"),
                [ArtifactSlots.Ring1] = TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("RingOfTheWayfarer"),
                [ArtifactSlots.Necklace] = TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("NecklaceOfSwiftness"),
                [ArtifactSlots.Ring2] = TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("EquestriansGloves"),
                [ArtifactSlots.Shield] = TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("BucklerOfTheGnollKing"),
                [ArtifactSlots.Torso] = TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("TitansCuirass"),
                [ArtifactSlots.Feet] = TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("SandalsOfTheSaint"),
                [ArtifactSlots.Cape] = TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("VampiresCowl"),
                [ArtifactSlots.Misc1] = TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("HideousMask"),
                [ArtifactSlots.Misc2] = TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("RunesOfImminency"),
                [ArtifactSlots.Misc3] = TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("CardsOfProphecy"),
                [ArtifactSlots.Misc4] = TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("MysticOrbOfSpellPoints"),
                [ArtifactSlots.Misc5] = TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("CharmOfSpellPoints"),
            };

            var actualArtifacts = hero.EquippedArtifacts.GetEquippedArtifacts().ToDictionary(t => t.Item1, t => t.Item2);

            Assert.That(actualArtifacts, Is.EquivalentTo(expectedArtifacts));

            Assert.That(hero.WarMachines, Is.EquivalentTo(new[] { TestCatalog.For(GameVersion.HornOfTheAbyss).WarMachines.ById("FirstAidTent") }));
        }

        [Test]
        public void AHeroWhoHoldsASpellBookIsReadAsHavingOne()
        {
            var bytes = File.ReadAllBytes(Path.Combine("TestData", "Heroes", "hero_orrin_with_spellbook.bin"));
            var bytesChunk = new HeroChunk("Orrin", bytes, 0);

            const bool ExpectedHasSpellBook = true;

            var hasSpellBook = this.deserializer.DeserializeHasSpellBook(bytesChunk);

            Assert.That(() => hasSpellBook, Is.EqualTo(ExpectedHasSpellBook));
        }

        [Test]
        public void AHeroWithoutASpellBookIsReadAsHavingNone()
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
            var offsets = new HeroOffsets(Siniskalc.Homm3.Entities.GameVersion.HornOfTheAbyss);
            var bytes = File.ReadAllBytes(Path.Combine("TestData", "Heroes", "hero_maximus_pre_hota.bin"));
            foreach (var spell in TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.All)
            {
                bytes[offsets[HeroOffsetName.SpellBookSpells] + spell.Code] = 0;
                bytes[offsets[HeroOffsetName.AvailableSpells] + spell.Code] = 0;
            }

            bytes[offsets[HeroOffsetName.SpellBookSpells] + TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("Haste").Code] = 1;
            foreach (var spell in new[] { TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("Haste"), TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("Fireball"), TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("Fly") })
            {
                bytes[offsets[HeroOffsetName.AvailableSpells] + spell.Code] = 1;
            }

            var tome = new EquippedArtifacts(new Dictionary<ArtifactSlots, Artifact>()).WithEquipped(ArtifactSlots.Misc1, TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("TomeOfFireMagic"));
            var book = this.deserializer.DeserializeSpellBook(new HeroChunk("Maximus", bytes, 0), tome)
                .WithGrants(new GrantedSpellsResolver(Siniskalc.Homm3.Entities.GameVersion.HornOfTheAbyss).ResolveGrantedSpells(tome), false);

            Assert.That(book.Source(TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("Fireball")), Does.Contain("Tome of Fire Magic"));
            Assert.That(book.Source(TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("Fly")), Is.EqualTo("Unknown source"));
            Assert.That(book.WithGrants(new Dictionary<Spell, string>(), false).IsAvailable(TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("Fireball")), Is.False);
        }
    }
}
