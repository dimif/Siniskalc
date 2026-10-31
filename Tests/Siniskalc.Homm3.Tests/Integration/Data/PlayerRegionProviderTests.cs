using Siniskalc;
using Siniskalc.Core.Data;
using Siniskalc.Heroes.Entities;
using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;

using Siniskalc.Homm3.Tests.Data;

namespace Siniskalc.Homm3.Tests.Integration.Data
{
    /// <summary>
    /// Tests the regions the hex tab shows for the record of a player.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class PlayerRegionProviderTests
    {
        [Test]
        public void ThePlayerHasItsResourcesTheMarkerAndTheColour()
        {
            var regions = TestServices.PlayerRegions.GetRegions(PlayerLayout.HornOfTheAbyss);

            var resources = regions.Single(r => r.Name == "Resources");
            Assert.That(resources.Start, Is.EqualTo(PlayerLayout.ResourcesOffset));
            Assert.That(resources.Elements.Select(e => e.Name), Is.EqualTo(new[] { "Wood", "Mercury", "Ore", "Sulfur", "Crystal", "Gems", "Gold" }));
            Assert.That(resources.Elements.Select(e => e.Start), Is.EqualTo(Enum.GetValues<Resource>().Select(PlayerLayout.OffsetOf)));
            Assert.That(regions.Single(r => r.Name == "Marker"), Has.Property("Start").EqualTo(PlayerLayout.FlagOffset + 1).And.Property("Length").EqualTo(4));
            Assert.That(regions.Single(r => r.Name == "Colour").Start, Is.EqualTo(PlayerLayout.HornOfTheAbyss.ColorOffset));
        }

        [Test]
        public void ARegionPastTheEndOfAShortChunkIsLeftOut()
        {
            var regions = TestServices.PlayerRegions.GetRegions(PlayerLayout.HornOfTheAbyss, PlayerLayout.HornOfTheAbyss.ColorOffset);

            Assert.That(regions.Select(r => r.Name), Is.EqualTo(new[] { "Hero count", "Selected hero", "Hero ids", "Tavern heroes", "Days left", "Town count", "Selected town", "Resources", "Marker" }));
        }

        [Test]
        public void ThePlayerHexTargetEditsThroughTheHistory()
        {
            var game = SavedGames.Get(Path.Combine("TestData", "Hota", "empire_of_the_world_01_m5w4d6.GM1"));
            using var target = new PlayerByteEditTarget(game.History.CreatePlayerSession(0), TestServices.PlayerRegions);

            Assert.That(target.Regions.Select(r => r.Name), Does.Contain("Resources"));
            Assert.That(target.WriteBytes(PlayerLayout.ResourcesOffset, [1, 0, 0, 0]), Is.True);
            Assert.That(game.History.CanUndo, Is.True);
            Assert.That(game.History.Current.Players().GetModel(0).Resources.Wood, Is.EqualTo(1));
        }
    }
}
