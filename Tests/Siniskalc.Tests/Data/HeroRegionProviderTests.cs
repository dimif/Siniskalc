using Siniskalc;
using Siniskalc.Core.Data;
using Siniskalc.Data;
using Siniskalc.Entities;
using Siniskalc.Models;
using Siniskalc.Runtime;

namespace Siniskalc.Tests.Data
{
    [TestFixture]
    internal sealed class HeroRegionProviderTests
    {
        private static readonly HeroOffsets HotaOffsets = new(GameVersion.HornOfTheAbyss);

        private Game game;
        private HeroChunk chunk;

        [SetUp]
        public void SetUp()
        {
            var path = Path.Combine("TestData", "Hota", "empire_of_the_world_01_m5w4d6.GM1");
            this.game = TestServices.GameService.LoadGameFromFile(path);
            this.chunk = this.game.History.Current.Heroes()[this.game.History.Current.Heroes().FindByName("Christian")];
        }

        [Test]
        public void EveryOffsetNameHasARegionStartingAtItsOffset()
        {
            var regions = TestServices.HeroRegions.GetRegions(HotaOffsets, this.chunk.Length);

            foreach (var name in Enum.GetValues<HeroOffsetName>())
            {
                Assert.That(regions.Any(r => r.Start == HotaOffsets[name]), Is.True, name.ToString());
            }
        }

        [Test]
        public void RegionsFitInTheChunk()
        {
            var regions = TestServices.HeroRegions.GetRegions(HotaOffsets, this.chunk.Length);

            Assert.That(regions, Has.All.Matches<ByteRegion>(r => r.Start >= 0 && r.Length > 0 && r.End <= this.chunk.Length));
            Assert.That(regions.SelectMany(r => r.Elements), Has.All.Matches<ByteRegion>(
                e => regions.Any(r => r.Start <= e.Start && e.End <= r.End)));
        }

        [Test]
        public void OnlyTheBallistaAndCannonShareBytes()
        {
            var leaves = TestServices.HeroRegions.GetRegions(HotaOffsets).SelectMany(r => r.Leaves()).ToList();
            var overlaps = new List<string>();

            for (var i = 0; i < leaves.Count; i++)
            {
                for (var j = i + 1; j < leaves.Count; j++)
                {
                    if (leaves[i].Start < leaves[j].End && leaves[j].Start < leaves[i].End)
                    {
                        overlaps.Add($"{leaves[i].Name}/{leaves[j].Name}");
                    }
                }
            }

            Assert.That(overlaps, Is.EqualTo(new[] { "Ballista/Cannon" }));
        }

        [Test]
        public void CannonOnlyExistsInHota()
        {
            var classic = TestServices.HeroRegions.GetRegions(new HeroOffsets(GameVersion.ShadowOfDeath));

            Assert.That(classic.Select(r => r.Name), Does.Not.Contain("Cannon"));
            Assert.That(TestServices.HeroRegions.GetRegions(HotaOffsets).Select(r => r.Name), Does.Contain("Cannon"));
        }

        [Test]
        public void ArraysHaveTheirRealSizes()
        {
            var regions = TestServices.HeroRegions.GetRegions(HotaOffsets, this.chunk.Length);

            var creatures = regions.Single(r => r.Start == HotaOffsets[HeroOffsetName.Creatures]);
            Assert.That(creatures.Length, Is.EqualTo(Constants.CreatureSlotCount * 4));
            Assert.That(creatures.Elements, Has.Count.EqualTo(Constants.CreatureSlotCount));
            Assert.That(creatures.Elements.Select(e => e.Length), Has.All.EqualTo(4));

            var amounts = regions.Single(r => r.Start == HotaOffsets[HeroOffsetName.CreaturesAmounts]);
            Assert.That(amounts.Length, Is.EqualTo(Constants.CreatureSlotCount * 4));

            var backpack = regions.Single(r => r.Start == HotaOffsets[HeroOffsetName.Backpack]);
            Assert.That(backpack.Length, Is.EqualTo(Constants.BackpackSlotCount * Constants.BackpackSlotSize));
            Assert.That(backpack.Elements, Has.Count.EqualTo(Constants.BackpackSlotCount));

            var skills = regions.Single(r => r.Start == HotaOffsets[HeroOffsetName.Skills]);
            Assert.That(skills.Elements, Has.Count.EqualTo(TestServices.VersionEntities.For(GameVersion.HornOfTheAbyss).SecondarySkills.Count));

            var name = regions.Single(r => r.Start == HotaOffsets[HeroOffsetName.Name]);
            Assert.That(name.End, Is.EqualTo(HotaOffsets[HeroOffsetName.Skills]));
        }

        [Test]
        public void SerializerOnlyWritesInsideKnownRegions()
        {
            var entities = TestServices.VersionEntities.For(GameVersion.HornOfTheAbyss);
            var serializer = new SafeHeroSerializer(HotaOffsets, entities);
            var model = new HeroDeserializer(HotaOffsets, entities).Deserialize(this.chunk);
            var map = new ByteRegionMap(TestServices.HeroRegions.GetRegions(HotaOffsets, this.chunk.Length), this.chunk.Length);

            var edited = serializer.Serialize(this.chunk, model with
            {
                PrimarySkills = new HeroPrimarySkills(11, 12, 13, 14),
                HeroLevel = new HeroLevel { Level = 30, Experience = 123456 },
                Army = new Army([]).WithSlot(1, Creature.Pikeman, 10).WithSlot(7, Creature.Angel, 3),
                SecondarySkills = model.SecondarySkills.WithoutSlot(0),
                Backpack = model.Backpack.WithSlot(Constants.BackpackSlotCount - 1, Artifact.OrbOfInhibition),
                EquippedArtifacts = model.EquippedArtifacts.WithEquipped(ArtifactSlots.Feet, Artifact.BootsOfLevitation),
                SpellPoints = 77,
            });

            var changed = ByteEditTargetExtensions.FindDifferences(this.chunk.ToArray(), edited.ToArray());
            Assert.That(changed, Is.Not.Empty);
            Assert.That(changed.Where(i => map.FindLeaf(i) == null), Is.Empty);
        }

        [Test]
        public void MapFindsRegionsAndAlternatesShades()
        {
            var map = new ByteRegionMap(TestServices.HeroRegions.GetRegions(HotaOffsets, this.chunk.Length), this.chunk.Length);
            var start = HotaOffsets[HeroOffsetName.Creatures];

            Assert.That(map.FindLeaf(start)?.FullName, Is.EqualTo("Creature types: Slot 1"));
            Assert.That(map.FindLeaf(start + 4)?.FullName, Is.EqualTo("Creature types: Slot 2"));
            Assert.That(map.GetShade(start), Is.Not.EqualTo(map.GetShade(start + 4)));
            Assert.That(map.GetShade(start), Is.EqualTo(map.GetShade(start + 3)));
            Assert.That(map.FindLeaf(HotaOffsets[HeroOffsetName.Ballista] + 1)?.Kind, Is.EqualTo(ByteRegionKind.WarMachines));
            Assert.That(map.FindAll(HotaOffsets[HeroOffsetName.Ballista]), Has.Count.EqualTo(2));
            Assert.That(map.FindLeaf(HotaOffsets[HeroOffsetName.PositionX] + 2)?.Name, Is.EqualTo("Map Y"));
            Assert.That(map.FindLeaf(-1), Is.Null);
            Assert.That(map.FindLeaf(this.chunk.Length), Is.Null);
        }
    }
}
