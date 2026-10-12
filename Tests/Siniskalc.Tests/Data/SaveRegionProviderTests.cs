using System.Text;
using Siniskalc.Core.Data;
using Siniskalc.Data;
using Siniskalc.Entities;
using Siniskalc.Runtime;

namespace Siniskalc.Tests.Data
{
    [TestFixture]
    internal sealed class SaveRegionProviderTests
    {
        private static readonly string[] SavePaths =
        [
            Path.Combine("TestData", "Hota", "empire_of_the_world_01_m5w4d6.GM1"),
            Path.Combine("TestData", "Hota", "bios_custom.GM1"),
            Path.Combine("TestData", "Sod", "sod_game_00_baseline.GM1"),
        ];

        private static Game Load(string path) => TestServices.GameService.LoadGameFromFile(path);

        private static IReadOnlyList<ByteRegion> RegionsOf(Game game) => TestServices.SaveRegions.GetRegions(game.History.Current, game.GetBytesToSave());

        private static ByteRegion Leaf(IReadOnlyList<ByteRegion> regions, string fullName) =>
            regions.SelectMany(r => r.Leaves()).First(l => l.FullName == fullName);

        [TestCaseSource(nameof(SavePaths))]
        public void RegionsFitInTheSavefileAndOnlyTheBallistaAndCannonShareBytes(string path)
        {
            var game = Load(path);
            var length = game.GetBytesToSave().Length;
            var leaves = RegionsOf(game).SelectMany(r => r.Leaves()).OrderBy(l => l.Start).ToList();

            Assert.That(leaves, Has.All.Matches<ByteRegion>(l => l.Start >= 0 && l.Length > 0 && l.End <= length));

            var overlaps = new List<string>();
            var reach = leaves[0];
            foreach (var leaf in leaves.Skip(1))
            {
                if (leaf.Start < reach.End)
                {
                    overlaps.Add($"{reach.Name}/{leaf.Name}");
                }

                if (leaf.End > reach.End)
                {
                    reach = leaf;
                }
            }

            Assert.That(overlaps.Distinct(), Is.SubsetOf(new[] { "Ballista/Cannon" }));
        }

        [TestCaseSource(nameof(SavePaths))]
        public void EveryHeroPlayerAndCastleIsPlacedAtItsChunk(string path)
        {
            var game = Load(path);
            var document = game.History.Current;
            var regions = RegionsOf(game);

            // Names are not unique, so the regions are compared with the chunks in the order of the savefile.
            Assert.That(
                regions.Where(r => r.Name.StartsWith("Hero: ")).Select(r => r.Start),
                Is.EqualTo(document.Heroes().Select(h => h.HeaderIndex).OrderBy(i => i)));

            Assert.That(
                regions.Where(r => r.Name.StartsWith("Castle: ")).Select(r => r.Start),
                Is.EqualTo(document.Castles().Select(c => c.StartIndex).OrderBy(i => i)));

            Assert.That(
                regions.Where(r => r.Name.StartsWith("Player: ")).Select(r => r.Start),
                Is.EqualTo(document.Players().Select(p => p.StartIndex).OrderBy(i => i)));
        }

        [Test]
        public void TheFieldsOfAChunkAreMovedToItsPositionInTheSavefile()
        {
            var game = Load(SavePaths[0]);
            var document = game.History.Current;
            var regions = RegionsOf(game);
            var index = document.Heroes().FindByName("Christian");
            var hero = document.Heroes()[index];
            var id = hero.GetByteAt(HeroOffsets.HeroIdOffset);

            var experience = Leaf(regions, $"Christian (id {id}): Experience");
            Assert.That(experience.Start, Is.EqualTo(hero.StartIndex + document.Heroes().Offsets[HeroOffsetName.ExperiencePoints]));

            var slot = Leaf(regions, $"Christian (id {id}) / Creature types: Slot 2");
            Assert.That(slot.Start, Is.EqualTo(hero.StartIndex + document.Heroes().Offsets[HeroOffsetName.Creatures] + 4));
        }

        [Test]
        public void ThePlayersResourcesAreWhereThePlayerModelReadsThem()
        {
            var game = Load(SavePaths[0]);
            var bytes = game.GetBytesToSave();
            var regions = RegionsOf(game);

            foreach (var player in game.History.Current.Players())
            {
                var gold = Leaf(regions, $"{player.Name} / Resources: Gold");
                Assert.That(BitConverter.ToInt32(bytes, gold.Start), Is.EqualTo(player.GetInt32At(PlayerLayout.OffsetOf(Resource.Gold))), player.Name);
                Assert.That(bytes[Leaf(regions, $"{player.Name}: Colour").Start], Is.EqualTo(player.GetByteAt(player.Layout.ColorOffset)), player.Name);
            }
        }

        [TestCaseSource(nameof(SavePaths))]
        public void TheMapHeaderAndTheSetupAreWhereTheLocatorsFoundThem(string path)
        {
            var game = Load(path);
            var bytes = game.GetBytesToSave();
            var regions = RegionsOf(game);

            Assert.That(Encoding.ASCII.GetString(bytes, 0, 5), Is.EqualTo("H3SVG"));
            Assert.That(Leaf(regions, "Signature").Start, Is.EqualTo(0));

            var name = Leaf(regions, "Map header: Name");
            Assert.That(Encoding.ASCII.GetString(bytes, name.Start, name.Length), Is.EqualTo(game.Map?.Name));

            var difficulty = Leaf(regions, "Game setup: Game difficulty");
            Assert.That(difficulty.Start, Is.EqualTo(new PlayerControlLocator().Locate(bytes, game.GameVersion)?.DifficultyAt));

            var fileName = Leaf(regions, "Game setup: Map file name");
            Assert.That(Encoding.ASCII.GetString(bytes, fileName.Start, fileName.Length), Does.EndWith(".h3m").Or.EndsWith(".h3c"));
        }

        [Test]
        public void TheDateIsMarkedWhereItIsFound()
        {
            var game = Load(SavePaths[0]);
            var bytes = game.GetBytesToSave();
            var regions = RegionsOf(game);

            var day = Leaf(regions, "Game date: Day");
            var week = Leaf(regions, "Game date: Week");
            var month = Leaf(regions, "Game date: Month");

            Assert.That(BitConverter.ToUInt16(bytes, day.Start), Is.EqualTo(game.Date!.Value.Day));
            Assert.That(BitConverter.ToUInt16(bytes, week.Start), Is.EqualTo(game.Date.Value.Week));
            Assert.That(BitConverter.ToUInt16(bytes, month.Start), Is.EqualTo(game.Date.Value.Month));
        }

        [Test]
        public void TheHeaderAndBiographyOfAHeroWithOneAreMarkedInFrontOfItsChunk()
        {
            var game = Load(Path.Combine("TestData", "Hota", "bios_custom.GM1"));
            var document = game.History.Current;
            var bytes = game.GetBytesToSave();
            var regions = RegionsOf(game);
            var hero = document.Heroes().First(h => h.Biography != null);
            var group = $"{hero.Name} (id {hero.GetByteAt(HeroOffsets.HeroIdOffset)})";

            var biography = Leaf(regions, $"{group}: Biography");
            Assert.That(Encoding.Latin1.GetString(bytes, biography.Start, biography.Length), Is.EqualTo(hero.Biography));

            var length = Leaf(regions, $"{group}: Biography length");
            Assert.That(BitConverter.ToInt32(bytes, length.Start), Is.EqualTo(hero.Biography!.Length));

            Assert.That(Leaf(regions, $"{group}: Gender").Start, Is.EqualTo(hero.HeaderIndex + document.Heroes().Offsets[HeroOffsetName.Gender]));
            Assert.That(regions.SelectMany(r => r.Leaves()).Where(l => l.FullName == $"{group}: Map X").ToList(), Has.Count.EqualTo(1));
            Assert.That(Leaf(regions, $"{group}: Map X").Start, Is.EqualTo(hero.HeaderIndex));
        }

        [Test]
        public void TheDumpHoldsTheEditedAndTheOriginalSavefile()
        {
            var game = Load(SavePaths[0]);
            var original = game.GetBytesToSave();
            var hero = game.History.CreateHeroSession(game.History.Current.Heroes().FindByName("Christian"));
            hero.Update(h => h with { PrimarySkills = new Siniskalc.Models.HeroPrimarySkills(11, 12, 13, 14) });

            var dump = SaveHexDump.Create("test", game.History, TestServices.SaveRegions);

            Assert.That(dump.Bytes, Is.EqualTo(game.GetBytesToSave()));
            Assert.That(dump.OriginalBytes, Is.EqualTo(original));
            Assert.That(ByteEditTargetExtensions.FindDifferences(dump.OriginalBytes, dump.Bytes), Is.Not.Empty);
            Assert.That(dump.Regions, Is.Not.Empty);
        }

        [Test]
        public void ShiftedMovesTheRegionAndItsElementsAndPrefixesTheGroups()
        {
            var region = new ByteRegion("Types", 10, 8, ByteRegionKind.Creatures, null, null, [new ByteRegion("Slot 1", 10, 4, ByteRegionKind.Creatures, "Types"), new ByteRegion("Slot 2", 14, 4, ByteRegionKind.Creatures, "Types")]);

            var shifted = region.Shifted(100, "Hero");

            Assert.That(shifted.Start, Is.EqualTo(110));
            Assert.That(shifted.Leaves().Select(l => (l.Start, l.FullName)), Is.EqualTo(new[] { (110, "Hero / Types: Slot 1"), (114, "Hero / Types: Slot 2") }));
            Assert.That(region.Start, Is.EqualTo(10));
        }
    }
}
