using System.Text;
using Siniskalc;
using Siniskalc.Data;
using Siniskalc.Entities;
using Siniskalc.Models;
using Siniskalc.Runtime;

namespace Siniskalc.Tests.Data
{
    /// <summary>
    /// Tests against a real HotA savegame. The expected values come from TestData/Hota/empire_of_the_world_01_m5w4d6_expected_info.md,
    /// which was written from the in-game hero screens.
    /// </summary>
    [TestFixture]
    internal sealed class HotaSaveTests
    {
        private static readonly string SavePath = Path.Combine("TestData", "Hota", "empire_of_the_world_01_m5w4d6.GM1");

        private byte[] bytes;
        private HeroBytesLoader loader;
        private IReadOnlyList<HeroChunk> chunks;
        private HeroOffsets offsets;
        private HeroDeserializer deserializer;
        private SafeHeroSerializer serializer;

        [OneTimeSetUp]
        public void LoadSave()
        {
            this.bytes = TestServices.SavefileCodec.Read(SavePath);
            this.loader = new HeroBytesLoader(Encoding.Latin1);
            this.chunks = this.loader.LoadHeroChunks(this.bytes, new SavegameVersionDetector().Detect(this.bytes));
            this.offsets = new HeroOffsets(new SavegameVersionDetector().Detect(this.bytes));
            var entities = TestServices.VersionEntities.For(this.offsets.GameVersion);
            this.deserializer = new HeroDeserializer(this.offsets, entities);
            this.serializer = new SafeHeroSerializer(this.offsets, entities);
        }

        [Test]
        public void LoaderDetectsHotaAndFindsThePurpleHeroes()
        {
            Assert.That(new SavegameVersionDetector().Detect(this.bytes), Is.EqualTo(GameVersion.HornOfTheAbyss));
            Assert.That(this.chunks, Has.Count.EqualTo(215));

            foreach (var name in new[] { "Sir Mullich", "Maximus", "Christian" })
            {
                Assert.That(this.chunks.Count(c => c.Name == name), Is.EqualTo(1), name);
            }
        }

        [Test]
        public void EveryHeroDeserializes()
        {
            foreach (var chunk in this.chunks)
            {
                Assert.DoesNotThrow(() => this.deserializer.Deserialize(chunk), chunk.Name);
            }
        }

        [Test]
        public void EveryHeroRoundTripsToIdenticalBytes()
        {
            foreach (var chunk in this.chunks)
            {
                var result = serializer.Serialize(chunk, this.deserializer.Deserialize(chunk));
                Assert.That(result.HasSameBytes(chunk), Is.True, chunk.Name);
            }
        }

        [Test]
        public void SirMullichIsDeserialized()
        {
            var hero = this.Hero("Sir Mullich");

            Assert.That(hero.Owner, Is.EqualTo(PlayerColor.Purple));
            Assert.That(hero.HeroClass, Is.EqualTo(HeroClass.Knight));
            Assert.That(hero.HeroLevel.Level, Is.EqualTo((short)19));
            Assert.That(hero.HeroLevel.Experience, Is.EqualTo(71483));
            Assert.That(hero.WarMachines, Is.Empty);
            Assert.That(hero.SpellBook.HasSpellBook, Is.True);
            Assert.That(hero.PrimarySkills, Is.EqualTo(new HeroPrimarySkills(21, 24, 11, 9)));

            AssertSecondarySkills(hero,
                (SecondarySkill.Pathfinding, SkillLevel.Expert),
                (SecondarySkill.Wisdom, SkillLevel.Expert),
                (SecondarySkill.EarthMagic, SkillLevel.Expert),
                (SecondarySkill.Logistics, SkillLevel.Expert),
                (SecondarySkill.Ballistics, SkillLevel.Expert),
                (SecondarySkill.Navigation, SkillLevel.Expert),
                (SecondarySkill.WaterMagic, SkillLevel.Expert),
                (SecondarySkill.Leadership, SkillLevel.Expert));

            AssertCreatures(hero,
                (Creature.Archangel, 1u),
                (Creature.Crusader, 348u),
                (Creature.Marksman, 750u),
                null,
                (Creature.Archangel, 1u),
                (Creature.Skeleton, 1802u),
                null);

            AssertEquipped(hero, new Dictionary<ArtifactSlots, Artifact>
            {
                [ArtifactSlots.Helmet] = Artifact.HelmOfChaos,
                [ArtifactSlots.Necklace] = Artifact.PendantOfDownfall,
                [ArtifactSlots.Torso] = Artifact.RibCage,
                [ArtifactSlots.Ring1] = Artifact.QuietEyeOfTheDragon,
                [ArtifactSlots.Ring2] = Artifact.QuietEyeOfTheDragon,
                [ArtifactSlots.Weapon] = Artifact.RedDragonFlameTongue,
                [ArtifactSlots.Shield] = Artifact.ShieldOfTheYawningDead,
                [ArtifactSlots.Misc2] = Artifact.StoicWatchman,
                [ArtifactSlots.Misc3] = Artifact.SpiritOfOppression,
                [ArtifactSlots.Misc5] = Artifact.EndlessSackOfGold,
            });

            AssertBackpack(hero,
                Artifact.RingOfConjuring,
                Artifact.BreastplateOfPetrifiedWood,
                Artifact.OgresClubOfHavoc,
                Artifact.PendantOfHoliness,
                Artifact.PendantOfLife,
                Artifact.PendantOfDispassion,
                Artifact.PendantOfDeath);

            AssertSpellBook(hero, [.. MullichSpells]);
        }

        [Test]
        public void MaximusIsDeserialized()
        {
            var hero = this.Hero("Maximus");

            Assert.That(hero.Owner, Is.EqualTo(PlayerColor.Purple));
            Assert.That(hero.HeroClass, Is.EqualTo(HeroClass.Knight));
            Assert.That(hero.HeroLevel.Level, Is.EqualTo((short)32));
            Assert.That(hero.HeroLevel.Experience, Is.EqualTo(779943));
            Assert.That(hero.WarMachines, Is.EquivalentTo(new[] { WarMachine.FirstAidTent }));
            Assert.That(hero.SpellBook.HasSpellBook, Is.True);
            Assert.That(hero.PrimarySkills, Is.EqualTo(new HeroPrimarySkills(32, 32, 35, 23)));

            AssertSecondarySkills(hero,
                (SecondarySkill.Wisdom, SkillLevel.Expert),
                (SecondarySkill.EarthMagic, SkillLevel.Expert),
                (SecondarySkill.Ballistics, SkillLevel.Expert),
                (SecondarySkill.AirMagic, SkillLevel.Expert),
                (SecondarySkill.Armorer, SkillLevel.Expert),
                (SecondarySkill.FireMagic, SkillLevel.Expert),
                (SecondarySkill.WaterMagic, SkillLevel.Expert),
                (SecondarySkill.Necromancy, SkillLevel.Expert));

            AssertCreatures(hero,
                (Creature.Skeleton, 15u),
                null,
                (Creature.RoyalGriffin, 832u),
                (Creature.Archangel, 145u),
                (Creature.Zealot, 411u),
                (Creature.Champion, 276u),
                (Creature.GoldDragon, 63u));

            AssertEquipped(hero, new Dictionary<ArtifactSlots, Artifact>
            {
                [ArtifactSlots.Helmet] = Artifact.HelmOfChaos,
                [ArtifactSlots.Necklace] = Artifact.NecklaceOfSwiftness,
                [ArtifactSlots.Torso] = Artifact.TitansCuirass,
                [ArtifactSlots.Ring1] = Artifact.RingOfTheWayfarer,
                [ArtifactSlots.Ring2] = Artifact.EquestriansGloves,
                [ArtifactSlots.Weapon] = Artifact.SwordOfJudgement,
                [ArtifactSlots.Shield] = Artifact.BucklerOfTheGnollKing,
                [ArtifactSlots.Feet] = Artifact.SandalsOfTheSaint,
                [ArtifactSlots.Cape] = Artifact.VampiresCowl,
                [ArtifactSlots.Misc1] = Artifact.HideousMask,
                [ArtifactSlots.Misc2] = Artifact.RunesOfImminency,
                [ArtifactSlots.Misc3] = Artifact.CardsOfProphecy,
                [ArtifactSlots.Misc4] = Artifact.MysticOrbOfSpellPoints,
                [ArtifactSlots.Misc5] = Artifact.CharmOfSpellPoints,
            });

            AssertBackpack(hero,
                Artifact.ShamansPuppet,
                Artifact.AmuletOfTheUndertaker,
                Artifact.BootsOfLevitation,
                Artifact.CapeOfConjuring,
                Artifact.GreaterKnollsFlail,
                Artifact.BadgeOfCourage,
                Artifact.BowOfElvenCherrywood,
                Artifact.HourglassOfTheEvilHour,
                Artifact.HelmOfTheAlabasterUnicorn,
                Artifact.OrbOfFirmament,
                Artifact.OrbOfInhibition,
                Artifact.BootsOfPolarity,
                Artifact.OrbOfTempestousFire,
                Artifact.AdmiralsHat);

            AssertSpellBook(hero, [.. MullichSpells, Spell.Teleport]);
        }

        [Test]
        public void ChristianIsDeserialized()
        {
            var hero = this.Hero("Christian");

            Assert.That(hero.Owner, Is.EqualTo(PlayerColor.Purple));
            Assert.That(hero.HeroClass, Is.EqualTo(HeroClass.Knight));
            Assert.That(hero.HeroLevel.Level, Is.EqualTo((short)14));
            Assert.That(hero.HeroLevel.Experience, Is.EqualTo(32273));
            Assert.That(hero.WarMachines, Is.EquivalentTo(new[] { WarMachine.Ballista }));
            Assert.That(hero.SpellBook.HasSpellBook, Is.True);
            Assert.That(hero.PrimarySkills, Is.EqualTo(new HeroPrimarySkills(24, 21, 19, 17)));

            AssertSecondarySkills(hero,
                (SecondarySkill.Wisdom, SkillLevel.Expert),
                (SecondarySkill.EarthMagic, SkillLevel.Expert),
                (SecondarySkill.Offense, SkillLevel.Expert),
                (SecondarySkill.Ballistics, SkillLevel.Expert),
                (SecondarySkill.Navigation, SkillLevel.Expert),
                (SecondarySkill.Leadership, SkillLevel.Beginner),
                (SecondarySkill.Tactics, SkillLevel.Intermediate),
                (SecondarySkill.WaterMagic, SkillLevel.Beginner));

            AssertCreatures(hero, (Creature.GoldDragon, 1u), null, null, null, null, null, null);

            AssertEquipped(hero, new Dictionary<ArtifactSlots, Artifact>
            {
                [ArtifactSlots.Necklace] = Artifact.NecklaceOfDragonteeth,
            });

            AssertBackpack(hero);
        }

        [Test]
        public void UnchangedGameSavesIdenticalBytes()
        {
            var game = TestServices.GameService.LoadGameFromBytes(this.bytes, SavePath);

            Assert.That(game.GetBytesToSave(), Is.EqualTo(this.bytes));
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
                PrimarySkills = new HeroPrimarySkills(50, 40, 30, 20),
                EquippedArtifacts = h.EquippedArtifacts.WithEquipped(ArtifactSlots.Feet, Artifact.BootsOfLevitation),
                Backpack = h.Backpack.WithSlot(0, Artifact.OrbOfInhibition),
                WarMachines = [WarMachine.Ballista, WarMachine.AmmoCart],
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

                Assert.That(hero.PrimarySkills, Is.EqualTo(new HeroPrimarySkills(50, 40, 30, 20)));
                Assert.That(hero.EquippedArtifacts.GetArtifact(ArtifactSlots.Feet), Is.EqualTo(Artifact.BootsOfLevitation));
                Assert.That(hero.EquippedArtifacts.GetArtifact(ArtifactSlots.Necklace), Is.EqualTo(Artifact.NecklaceOfDragonteeth));
                Assert.That(hero.Backpack.All[0]?.Art, Is.EqualTo(Artifact.OrbOfInhibition));
                Assert.That(hero.WarMachines, Is.EquivalentTo(new[] { WarMachine.Ballista, WarMachine.AmmoCart }));
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
            Spell.SummonBoat, Spell.ViewEarth, Spell.ScuttleBoat,

            // Combat
            Spell.Haste, Spell.ProtectionFromFire, Spell.Bless, Spell.Cure, Spell.Dispel, Spell.ProtectionFromWater,
            Spell.MagicArrow, Spell.Shield, Spell.Slow, Spell.DisruptingRay, Spell.Fortune, Spell.Precision, Spell.Blind,
            Spell.FireWall, Spell.IceBolt, Spell.RemoveObstacle, Spell.QuickSand, Spell.DestroyUndead, Spell.Hypnotize,
            Spell.Fireball, Spell.LandMine, Spell.Misfortune, Spell.Forgetfulness, Spell.FrostRing, Spell.ForceField,
            Spell.ChainLightning, Spell.Counterstrike, Spell.Berserk, Spell.Frenzy, Spell.Clone, Spell.Prayer,
            Spell.MeteorShower, Spell.Resurrection, Spell.MagicMirror, Spell.SummonWaterElemental, Spell.Implosion,
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
            var unslottedWithLevel = SecondarySkill.All.Where(s => !slotted.Contains(s) && skills.GetSkillLevel(s) != null);
            Assert.That(unslottedWithLevel, Is.Empty);
        }

        private static void AssertCreatures(HeroModel hero, params (Creature Creature, uint Count)?[] expectedBySlot)
        {
            var expected = expectedBySlot
                .Select((e, i) => e is { } slot ? new ArmySlot(i + 1, slot.Creature.Code, slot.Count) : null)
                .Where(s => s != null);

            Assert.That(hero.Army.Slots, Is.EquivalentTo(expected));
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
            var learned = Spell.All.Where(hero.SpellBook.IsLearned);
            Assert.That(learned, Is.EquivalentTo(expected));
        }
    }
}
