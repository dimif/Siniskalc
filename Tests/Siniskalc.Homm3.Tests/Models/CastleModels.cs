using Siniskalc.Core.Models;
using Siniskalc.Heroes.Models;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Models;

namespace Siniskalc.Homm3.Tests.Models
{
    /// <summary>Castles built in memory, so a rule can be tested without a savefile.</summary>
    internal static class CastleModels
    {
        /// <summary>A town of <paramref name="faction"/> that has only its village hall and may build everything its faction can.</summary>
        public static CastleModel Village(Faction faction)
        {
            var village = new HashSet<Building> { Building.VillageHall };
            return new CastleModel(
                "Test Town",
                faction,
                PlayerColor.Red,
                new MapPosition(1, 1),
                null,
                new Army<Creature>(Constants.CreatureSlotCount),
                null,
                Numbers.Of(0, 0, 0, 0, 0, 0, 0),
                null,
                Building.ForFaction(faction).ToHashSet(),
                village,
                new HashSet<Building>(village),
                false,
                Numbers.Of(0, 0, 0, 0, 0),
                Enumerable.Range(0, 5).Select(_ => (IReadOnlyList<Spell>)Array.Empty<Spell>()).ToList());
        }

        /// <summary>The town with <paramref name="buildings"/> built on top of the village hall, shown as they stand.</summary>
        public static CastleModel With(CastleModel castle, params Building[] buildings)
        {
            var all = castle.Buildings.Concat(buildings).ToHashSet();
            return castle with { Buildings = all, VisibleBuildings = new HashSet<Building>(all) };
        }
    }
}
