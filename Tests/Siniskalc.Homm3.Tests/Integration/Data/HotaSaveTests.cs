using System.Text;
using Siniskalc.Heroes.Entities;
using Siniskalc.Heroes.Models;
using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Models;

namespace Siniskalc.Homm3.Tests.Integration.Data
{
    /// <summary>
    /// Tests against a real HotA savegame. The expected values come from TestData/Hota/empire_of_the_world_01_m5w4d6_expected_info.md,
    /// which was written from the in-game hero screens.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class HotaSaveTests
    {
        private static readonly string SavePath = Path.Combine("TestData", "Hota", "empire_of_the_world_01_m5w4d6.GM1");

        private byte[] bytes;
        private HeroBytesLoader loader;
        private IReadOnlyList<HeroChunk> chunks;
        private HeroOffsets offsets;
        private HeroDeserializer deserializer;

        [OneTimeSetUp]
        public void LoadSave()
        {
            this.bytes = TestServices.SavefileCodec.Read(SavePath);
            this.loader = new HeroBytesLoader(Encoding.Latin1);
            this.chunks = this.loader.LoadHeroChunks(this.bytes, new SavegameVersionDetector().Detect(this.bytes));
            this.offsets = new HeroOffsets(new SavegameVersionDetector().Detect(this.bytes));
            var entities = TestServices.VersionEntities.For(this.offsets.GameVersion);
            this.deserializer = new HeroDeserializer(this.offsets, entities);
        }

        [Test]
        public void ThePurpleHeroesAreFoundByName()
        {
            foreach (var name in new[] { "Sir Mullich", "Maximus", "Christian" })
            {
                Assert.That(this.chunks.Count(c => c.Name == name), Is.EqualTo(1), name);
            }
        }

        [Test]
        public void EveryHeroRecordOfTheSaveIsReadWithoutAnError()
        {
            foreach (var chunk in this.chunks)
            {
                Assert.DoesNotThrow(() => this.deserializer.Deserialize(chunk), chunk.Name);
            }
        }

        [Test]
        public void SirMullichIsReadWithTheOwnerClassLevelSkillsAndSpellsOfTheSave()
        {
            var hero = this.Hero("Sir Mullich");

            Assert.That(hero.Owner, Is.EqualTo(PlayerColor.Purple));
            Assert.That(hero.HeroClass, Is.EqualTo(TestCatalog.For(GameVersion.HornOfTheAbyss).HeroClasses.ById("Knight")));
            Assert.That(hero.HeroLevel.Level, Is.EqualTo((short)19));
            Assert.That(hero.HeroLevel.Experience, Is.EqualTo(71483));
            Assert.That(hero.WarMachines, Is.Empty);
            Assert.That(hero.SpellBook.HasSpellBook, Is.True);
            Assert.That(hero.PrimarySkills, Is.EqualTo(new PrimarySkills(21, 24, 11, 9)));

            AssertSecondarySkills(hero,
                (TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("Pathfinding"), SkillLevel.Expert),
                (TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("Wisdom"), SkillLevel.Expert),
                (TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("EarthMagic"), SkillLevel.Expert),
                (TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("Logistics"), SkillLevel.Expert),
                (TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("Ballistics"), SkillLevel.Expert),
                (TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("Navigation"), SkillLevel.Expert),
                (TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("WaterMagic"), SkillLevel.Expert),
                (TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("Leadership"), SkillLevel.Expert));

            AssertCreatures(hero,
                (TestCatalog.For(GameVersion.HornOfTheAbyss).Creatures.ById("Archangel"), 1u),
                (TestCatalog.For(GameVersion.HornOfTheAbyss).Creatures.ById("Crusader"), 348u),
                (TestCatalog.For(GameVersion.HornOfTheAbyss).Creatures.ById("Marksman"), 750u),
                null,
                (TestCatalog.For(GameVersion.HornOfTheAbyss).Creatures.ById("Archangel"), 1u),
                (TestCatalog.For(GameVersion.HornOfTheAbyss).Creatures.ById("Skeleton"), 1802u),
                null);

            AssertEquipped(hero, new Dictionary<ArtifactSlots, Artifact>
            {
                [ArtifactSlots.Helmet] = TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("HelmOfChaos"),
                [ArtifactSlots.Necklace] = TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("PendantOfDownfall"),
                [ArtifactSlots.Torso] = TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("RibCage"),
                [ArtifactSlots.Ring1] = TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("QuietEyeOfTheDragon"),
                [ArtifactSlots.Ring2] = TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("QuietEyeOfTheDragon"),
                [ArtifactSlots.Weapon] = TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("RedDragonFlameTongue"),
                [ArtifactSlots.Shield] = TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("ShieldOfTheYawningDead"),
                [ArtifactSlots.Misc2] = TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("StoicWatchman"),
                [ArtifactSlots.Misc3] = TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("SpiritOfOppression"),
                [ArtifactSlots.Misc5] = TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("EndlessSackOfGold"),
            });

            AssertBackpack(hero,
                TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("RingOfConjuring"),
                TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("BreastplateOfPetrifiedWood"),
                TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("OgresClubOfHavoc"),
                TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("PendantOfHoliness"),
                TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("PendantOfLife"),
                TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("PendantOfDispassion"),
                TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("PendantOfDeath"));

            AssertSpellBook(hero, [.. MullichSpells]);
        }

        [Test]
        public void MaximusIsReadWithTheOwnerClassLevelSkillsAndSpellsOfTheSave()
        {
            var hero = this.Hero("Maximus");

            Assert.That(hero.Owner, Is.EqualTo(PlayerColor.Purple));
            Assert.That(hero.HeroClass, Is.EqualTo(TestCatalog.For(GameVersion.HornOfTheAbyss).HeroClasses.ById("Knight")));
            Assert.That(hero.HeroLevel.Level, Is.EqualTo((short)32));
            Assert.That(hero.HeroLevel.Experience, Is.EqualTo(779943));
            Assert.That(hero.WarMachines, Is.EquivalentTo(new[] { TestCatalog.For(GameVersion.HornOfTheAbyss).WarMachines.ById("FirstAidTent") }));
            Assert.That(hero.SpellBook.HasSpellBook, Is.True);
            Assert.That(hero.PrimarySkills, Is.EqualTo(new PrimarySkills(32, 32, 35, 23)));

            AssertSecondarySkills(hero,
                (TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("Wisdom"), SkillLevel.Expert),
                (TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("EarthMagic"), SkillLevel.Expert),
                (TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("Ballistics"), SkillLevel.Expert),
                (TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("AirMagic"), SkillLevel.Expert),
                (TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("Armorer"), SkillLevel.Expert),
                (TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("FireMagic"), SkillLevel.Expert),
                (TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("WaterMagic"), SkillLevel.Expert),
                (TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("Necromancy"), SkillLevel.Expert));

            AssertCreatures(hero,
                (TestCatalog.For(GameVersion.HornOfTheAbyss).Creatures.ById("Skeleton"), 15u),
                null,
                (TestCatalog.For(GameVersion.HornOfTheAbyss).Creatures.ById("RoyalGriffin"), 832u),
                (TestCatalog.For(GameVersion.HornOfTheAbyss).Creatures.ById("Archangel"), 145u),
                (TestCatalog.For(GameVersion.HornOfTheAbyss).Creatures.ById("Zealot"), 411u),
                (TestCatalog.For(GameVersion.HornOfTheAbyss).Creatures.ById("Champion"), 276u),
                (TestCatalog.For(GameVersion.HornOfTheAbyss).Creatures.ById("GoldDragon"), 63u));

            AssertEquipped(hero, new Dictionary<ArtifactSlots, Artifact>
            {
                [ArtifactSlots.Helmet] = TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("HelmOfChaos"),
                [ArtifactSlots.Necklace] = TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("NecklaceOfSwiftness"),
                [ArtifactSlots.Torso] = TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("TitansCuirass"),
                [ArtifactSlots.Ring1] = TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("RingOfTheWayfarer"),
                [ArtifactSlots.Ring2] = TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("EquestriansGloves"),
                [ArtifactSlots.Weapon] = TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("SwordOfJudgement"),
                [ArtifactSlots.Shield] = TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("BucklerOfTheGnollKing"),
                [ArtifactSlots.Feet] = TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("SandalsOfTheSaint"),
                [ArtifactSlots.Cape] = TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("VampiresCowl"),
                [ArtifactSlots.Misc1] = TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("HideousMask"),
                [ArtifactSlots.Misc2] = TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("RunesOfImminency"),
                [ArtifactSlots.Misc3] = TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("CardsOfProphecy"),
                [ArtifactSlots.Misc4] = TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("MysticOrbOfSpellPoints"),
                [ArtifactSlots.Misc5] = TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("CharmOfSpellPoints"),
            });

            AssertBackpack(hero,
                TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("ShamansPuppet"),
                TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("AmuletOfTheUndertaker"),
                TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("BootsOfLevitation"),
                TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("CapeOfConjuring"),
                TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("GreaterKnollsFlail"),
                TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("BadgeOfCourage"),
                TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("BowOfElvenCherrywood"),
                TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("HourglassOfTheEvilHour"),
                TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("HelmOfTheAlabasterUnicorn"),
                TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("OrbOfFirmament"),
                TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("OrbOfInhibition"),
                TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("BootsOfPolarity"),
                TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("OrbOfTempestousFire"),
                TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("AdmiralsHat"));

            AssertSpellBook(hero, [.. MullichSpells, TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("Teleport")]);
        }

        [Test]
        public void ChristianIsReadWithTheOwnerClassLevelSkillsAndSpellsOfTheSave()
        {
            var hero = this.Hero("Christian");

            Assert.That(hero.Owner, Is.EqualTo(PlayerColor.Purple));
            Assert.That(hero.HeroClass, Is.EqualTo(TestCatalog.For(GameVersion.HornOfTheAbyss).HeroClasses.ById("Knight")));
            Assert.That(hero.HeroLevel.Level, Is.EqualTo((short)14));
            Assert.That(hero.HeroLevel.Experience, Is.EqualTo(32273));
            Assert.That(hero.WarMachines, Is.EquivalentTo(new[] { TestCatalog.For(GameVersion.HornOfTheAbyss).WarMachines.ById("Ballista") }));
            Assert.That(hero.SpellBook.HasSpellBook, Is.True);
            Assert.That(hero.PrimarySkills, Is.EqualTo(new PrimarySkills(24, 21, 19, 17)));

            AssertSecondarySkills(hero,
                (TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("Wisdom"), SkillLevel.Expert),
                (TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("EarthMagic"), SkillLevel.Expert),
                (TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("Offense"), SkillLevel.Expert),
                (TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("Ballistics"), SkillLevel.Expert),
                (TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("Navigation"), SkillLevel.Expert),
                (TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("Leadership"), SkillLevel.Beginner),
                (TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("Tactics"), SkillLevel.Intermediate),
                (TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("WaterMagic"), SkillLevel.Beginner));

            AssertCreatures(hero, (TestCatalog.For(GameVersion.HornOfTheAbyss).Creatures.ById("GoldDragon"), 1u), null, null, null, null, null, null);

            AssertEquipped(hero, new Dictionary<ArtifactSlots, Artifact>
            {
                [ArtifactSlots.Necklace] = TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("NecklaceOfDragonteeth"),
            });

            AssertBackpack(hero);
        }

        [Test]
        public void EditsAreSavedAndLoadedBack()
        {
            var game = TestServices.GameService.LoadGameFromBytes(this.bytes, SavePath);
            var christianIndex = game.History.Current.Heroes().FindByName("Christian");
            var christian = game.History.CreateHeroSession(christianIndex);
            var chunk = christian.Chunk;

            christian.Update(h => h with
            {
                PrimarySkills = new PrimarySkills(50, 40, 30, 20),
                EquippedArtifacts = h.EquippedArtifacts.WithEquipped(ArtifactSlots.Feet, TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("BootsOfLevitation")),
                Backpack = h.Backpack.WithSlot(0, TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("OrbOfInhibition")),
                WarMachines = [TestCatalog.For(GameVersion.HornOfTheAbyss).WarMachines.ById("Ballista"), TestCatalog.For(GameVersion.HornOfTheAbyss).WarMachines.ById("AmmoCart")],
            });

            var path = Path.Combine(TestContext.CurrentContext.WorkDirectory, $"{Guid.NewGuid():N}.GM1");
            try
            {
                TestServices.GameService.SaveGame(game, path);
                var savedBytes = TestServices.SavefileCodec.Read(path);

                // Only Christian's bytes changed.
                var changed = Enumerable.Range(0, savedBytes.Length).Where(i => savedBytes[i] != this.bytes[i]).ToList();
                Assert.That(changed, Is.Not.Empty);
                Assert.That(changed, Is.All.InRange(chunk.StartIndex, chunk.End - 1));

                var reloaded = TestServices.GameService.LoadGameFromFile(path);
                var hero = reloaded.History.CreateHeroSession(reloaded.History.Current.Heroes().FindByName("Christian")).Current;

                Assert.That(hero.PrimarySkills, Is.EqualTo(new PrimarySkills(50, 40, 30, 20)));
                Assert.That(hero.EquippedArtifacts.GetArtifact(ArtifactSlots.Feet), Is.EqualTo(TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("BootsOfLevitation")));
                Assert.That(hero.EquippedArtifacts.GetArtifact(ArtifactSlots.Necklace), Is.EqualTo(TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("NecklaceOfDragonteeth")));
                Assert.That(hero.Backpack.All[0]?.Art, Is.EqualTo(TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("OrbOfInhibition")));
                Assert.That(hero.WarMachines, Is.EquivalentTo(new[] { TestCatalog.For(GameVersion.HornOfTheAbyss).WarMachines.ById("Ballista"), TestCatalog.For(GameVersion.HornOfTheAbyss).WarMachines.ById("AmmoCart") }));
                Assert.That(reloaded.History.Current.Heroes(), Has.Count.EqualTo(this.chunks.Count));
            }
            finally
            {
                File.Delete(path);
            }
        }

        private static readonly Spell[] MullichSpells =
        [
            // Adventure
            TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("SummonBoat"), TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("ViewEarth"), TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("ScuttleBoat"),

            // Combat
            TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("Haste"), TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("ProtectionFromFire"), TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("Bless"), TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("Cure"), TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("Dispel"), TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("ProtectionFromWater"),
            TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("MagicArrow"), TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("Shield"), TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("Slow"), TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("DisruptingRay"), TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("Fortune"), TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("Precision"), TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("Blind"),
            TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("FireWall"), TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("IceBolt"), TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("RemoveObstacle"), TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("QuickSand"), TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("DestroyUndead"), TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("Hypnotize"),
            TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("Fireball"), TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("LandMine"), TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("Misfortune"), TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("Forgetfulness"), TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("FrostRing"), TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("ForceField"),
            TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("ChainLightning"), TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("Counterstrike"), TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("Berserk"), TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("Frenzy"), TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("Clone"), TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("Prayer"),
            TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("MeteorShower"), TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("Resurrection"), TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("MagicMirror"), TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("SummonWaterElemental"), TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("Implosion"),
        ];

        private HeroModel Hero(string name)
        {
            return this.deserializer.Deserialize(this.chunks.Single(c => c.Name == name));
        }

        private static void AssertSecondarySkills(HeroModel hero, params (SecondarySkill Skill, SkillLevel Level)[] expected)
        {
            var skills = hero.SecondarySkills;
            Assert.That(skills.SlotCount, Is.EqualTo(expected.Length));

            for (var i = 0; i < expected.Length; i++)
            {
                Assert.That(skills.GetSkillInSlot(i), Is.EqualTo(expected[i].Skill), $"Skill slot {i + 1}");
                Assert.That(skills.GetSkillLevel(expected[i].Skill), Is.EqualTo(expected[i].Level), $"Skill level {expected[i].Skill}");
            }

            var slotted = expected.Select(e => e.Skill).ToHashSet();
            var unslottedWithLevel = TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.All.Where(s => !slotted.Contains(s) && skills.GetSkillLevel(s) != SkillLevel.None);
            Assert.That(unslottedWithLevel, Is.Empty);
        }

        private static void AssertCreatures(HeroModel hero, params (Creature Creature, uint Count)?[] expectedBySlot)
        {
            var expected = expectedBySlot
                .Select(e => e is { } slot ? new ArmySlot<Creature>(slot.Creature, slot.Count) : null);

            Assert.That(hero.Army.Slots, Is.EqualTo(expected));
        }

        private static void AssertEquipped(HeroModel hero, Dictionary<ArtifactSlots, Artifact> expected)
        {
            var actual = hero.EquippedArtifacts.GetEquippedArtifacts().ToDictionary(t => t.Item1, t => t.Item2);
            Assert.That(actual, Is.EquivalentTo(expected));
        }

        private static void AssertBackpack(HeroModel hero, params Artifact[] expected)
        {
            var actual = hero.Backpack.All.OfType<BackpackSlot>().OrderBy(s => s.Slot).Select(s => s.Art);
            Assert.That(actual, Is.EqualTo(expected));
        }

        private static void AssertSpellBook(HeroModel hero, Spell[] expected)
        {
            var learned = TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.All.Where(hero.SpellBook.IsLearned);
            Assert.That(learned, Is.EquivalentTo(expected));
        }
    }
}
