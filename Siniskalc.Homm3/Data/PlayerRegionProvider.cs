namespace Siniskalc.Homm3.Data
{
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Models;

    /// <summary>
    /// Describes where each known field lives in a player chunk, using the offsets of <see cref="PlayerLayout"/>.
    /// </summary>
    public sealed class PlayerRegionProvider : IPlayerRegionProvider
    {
        /// <summary>
        /// Returns the known regions of a player chunk laid out as <paramref name="layout"/>, ordered by offset. Regions that
        /// would run past <paramref name="chunkLength"/> are left out: the last player of the table is cut short.
        /// </summary>
        /// <param name="layout">The layout of the player records.</param>
        /// <param name="chunkLength">The length of the chunk; regions past it are left out.</param>
        /// <returns>The regions.</returns>
        public IReadOnlyList<ByteRegion> GetRegions(PlayerLayout layout, int chunkLength = int.MaxValue)
        {
            const string resourcesName = "Resources";
            var resourceParts = ResourceSet.All
                .Select(r => new ByteRegion(r.ToString(), PlayerLayout.OffsetOf(r), PlayerLayout.ResourceSize, ByteRegionKind.Resources, resourcesName))
                .ToList();

            const string heroIdsName = "Hero ids";
            const string tavernName = "Tavern heroes";
            var heroIdParts = Enumerable.Range(0, PlayerLayout.HeroIdPlaces)
                .Select(i => new ByteRegion($"Place {i + 1}", PlayerLayout.HeroIds + i, 1, ByteRegionKind.Identity, heroIdsName))
                .ToList();
            var tavernParts = Enumerable.Range(0, PlayerLayout.TavernHeroPlaces)
                .Select(i => new ByteRegion($"Hero {i + 1}", PlayerLayout.TavernHeroes + i, 1, ByteRegionKind.Identity, tavernName))
                .ToList();

            var regions = new List<ByteRegion>
            {
                new("Hero count", PlayerLayout.HeroCount, 1, ByteRegionKind.Identity, null, "How many heroes the player has. A hero garrisoned in a castle is not counted"),
                new("Selected hero", PlayerLayout.SelectedHero, 1, ByteRegionKind.Identity, null, "The id of the hero selected on the adventure map; FF when none is"),
                new(heroIdsName, PlayerLayout.HeroIds, PlayerLayout.HeroIdPlaces, ByteRegionKind.Identity, null, "The ids of the heroes of the player, one byte each; FF for a place that is empty", heroIdParts),
                new(tavernName, PlayerLayout.TavernHeroes, PlayerLayout.TavernHeroPlaces, ByteRegionKind.Identity, null, "The ids of the two heroes the tavern offers; FF when it has none for a place", tavernParts),
                new("Town count", PlayerLayout.TownCount, 1, ByteRegionKind.Identity, null, "How many towns the player has"),
                new("Days left", PlayerLayout.DaysLeft, 1, ByteRegionKind.Identity, null, "The days before the player is banished for owning no town: FF while the player owns one, 7 the day the last is lost, one less each day, 0 once banished"),
                new("Selected town", PlayerLayout.SelectedTown, 1, ByteRegionKind.Identity, null, "The index of the town selected on the adventure map, in the savefile's table of towns; FF when none is"),
                new("Town ids", PlayerLayout.TownIds, Constants.Player.TownListSize, ByteRegionKind.Identity, null, "The indices of the player's towns in acquisition order; FF in unused places"),
                new(resourcesName, PlayerLayout.ResourcesOffset, resourceParts.Count * PlayerLayout.ResourceSize, ByteRegionKind.Resources, null, "One 32-bit integer per resource", resourceParts),
                new("Marker", layout.MarkerOffset, layout.Marker.Length, ByteRegionKind.Other, null, "The same in every player; it is what the loader looks for to find the table of players"),
                new("Colour", layout.ColorOffset, 1, ByteRegionKind.Identity, null, "The colour of the player plus one: 1 is red. Missing in a slot that is not in play"),
            };

            return regions.Where(r => r.End <= chunkLength).OrderBy(r => r.Start).ThenBy(r => r.Length).ToList();
        }
    }
}
