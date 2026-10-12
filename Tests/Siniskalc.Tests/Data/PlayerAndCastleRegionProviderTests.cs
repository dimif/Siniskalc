using Siniskalc;
using Siniskalc.Core.Data;
using Siniskalc.Data;
using Siniskalc.Entities;

namespace Siniskalc.Tests.Data
{
    [TestFixture]
    internal sealed class PlayerAndCastleRegionProviderTests
    {
        [Test]
        public void ThePlayerHasItsResourcesTheMarkerAndTheColour()
        {
            var regions = TestServices.PlayerRegions.GetRegions(PlayerLayout.HornOfTheAbyss);

            var resources = regions.Single(r => r.Name == "Resources");
            Assert.That(resources.Start, Is.EqualTo(PlayerLayout.ResourcesOffset));
            Assert.That(resources.Elements.Select(e => e.Name), Is.EqualTo(new[] { "Wood", "Mercury", "Ore", "Sulfur", "Crystal", "Gems", "Gold" }));
            Assert.That(resources.Elements.Select(e => e.Start), Is.EqualTo(Enum.GetValues<Resource>().Select(PlayerLayout.OffsetOf)));
            Assert.That(regions.Single(r => r.Name == "Marker"), Has.Property("Start").EqualTo(PlayerLayout.ResourcesOffset + 44).And.Property("Length").EqualTo(5));
            Assert.That(regions.Single(r => r.Name == "Colour").Start, Is.EqualTo(PlayerLayout.HornOfTheAbyss.ColorOffset));
        }

        [Test]
        public void ARegionPastTheEndOfAShortChunkIsLeftOut()
        {
            var regions = TestServices.PlayerRegions.GetRegions(PlayerLayout.HornOfTheAbyss, PlayerLayout.HornOfTheAbyss.ColorOffset);

            Assert.That(regions.Select(r => r.Name), Is.EqualTo(new[] { "Hero count", "Selected hero", "Hero ids", "Tavern heroes", "Town count", "Selected town", "Resources", "Marker" }));
        }

        [Test]
        public void ThePlayerHexTargetEditsThroughTheHistory()
        {
            var game = TestServices.GameService.LoadGameFromFile(Path.Combine("TestData", "Hota", "empire_of_the_world_01_m5w4d6.GM1"));
            using var target = new PlayerByteEditTarget(game.History.CreatePlayerSession(0), TestServices.PlayerRegions);

            Assert.That(target.Regions.Select(r => r.Name), Does.Contain("Resources"));
            Assert.That(target.WriteBytes(PlayerLayout.ResourcesOffset, [1, 0, 0, 0]), Is.True);
            Assert.That(game.History.CanUndo, Is.True);
            Assert.That(game.History.Current.Players().GetModel(0).Resources.Wood, Is.EqualTo(1));
        }

        [Test]
        public void TheHeroHasItsReservedSlotCounters()
        {
            var offsets = new HeroOffsets(GameVersion.HornOfTheAbyss);
            var regions = TestServices.HeroRegions.GetRegions(offsets);

            var reserved = regions.Single(r => r.Name == "Reserved slots");
            Assert.That(reserved.Elements.Select(e => e.Name), Is.EqualTo(new[] { "Helmet", "Cape", "Necklace", "Weapon", "Shield", "Torso", "Rings", "Feet", "Misc" }));
            Assert.That(reserved.Elements.Select(e => e.Start), Is.EqualTo(offsets.ReservedSlotCounters.Select(c => c.Offset)));
            Assert.That(TestServices.HeroRegions.GetRegions(new HeroOffsets(GameVersion.ShadowOfDeath)).Select(r => r.Name), Does.Contain("Reserved slots"));
            Assert.That(TestServices.HeroRegions.GetRegions(new HeroOffsets(GameVersion.RestorationOfErathia)).Select(r => r.Name), Does.Not.Contain("Reserved slots"));
        }

        [Test]
        public void TheHeroHeaderHasItsPositionGenderAndBiographyLength()
        {
            var offsets = new HeroOffsets(GameVersion.HornOfTheAbyss);
            var header = TestServices.HeroRegions.GetHeaderRegions(offsets);

            Assert.That(header.Select(r => (r.Name, r.Start)), Is.EqualTo(new[]
            {
                ("Map X", 0), ("Map Y", 2), ("Map level", 4), ("Gender", 20), ("Biography length", 22),
            }));
            Assert.That(header.Max(r => r.End), Is.EqualTo(HeroChunk.HeaderLength));
        }

        [Test]
        public void EveryCastleByteBeforeTheNameIsAccountedFor()
        {
            var game = TestServices.GameService.LoadGameFromFile(Path.Combine("TestData", "Hota", "castles_00_baseline.GM1"));
            var castle = game.History.Current.Castles()[0];
            var regions = TestServices.CastleRegions.GetRegions(castle);

            var prefix = regions.Where(r => r.Start >= CastleLayout.NamePrefix && r.End <= CastleLayout.NameLength).Select(r => (r.Name, r.Start)).ToList();
            Assert.That(prefix, Is.EqualTo(new[]
            {
                ("Garrisoned hero ID", 65), ("Visiting hero ID", 66), ("Mage guild level", 67), ("Unused, always 0", 68),
            }));
            Assert.That(regions.Single(r => r.Name == "Creatures available").Elements[0].Name, Is.EqualTo("Level 1"));
        }
    }
}
