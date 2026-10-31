namespace Siniskalc.Homm3.Models
{
    using System.Collections.Generic;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm3.Entities;

    /// <summary>
    /// What a player earns each day in HoMM3, as far as the savegame lets the editor work it out: the mines it owns and the town halls of its
    /// towns. The savegame does not store the income. It leaves out what other things add (artifacts, the Estates skill, the Grail, the resource
    /// silo and the dwellings of the map that give a resource), so the amounts are what the player earns at least.
    /// </summary>
    public static class DailyIncome
    {
        /// <summary>
        /// What a gold mine gives each day.
        /// </summary>
        public const int GoldMine = 1000;

        /// <summary>
        /// What a sawmill and an ore mine give each day.
        /// </summary>
        public const int Timber = 2;

        /// <summary>
        /// What the other mines give each day.
        /// </summary>
        public const int Mineral = 1;

        /// <summary>
        /// Returns the gold the best town hall of a town gives each day: 500 for a village hall, 1,000 for a town hall, 2,000 for a city hall and
        /// 4,000 for a capitol; nothing for a town without a hall.
        /// </summary>
        public static long HallGold(CastleModel castle)
        {
            if (castle.HasBuilt(Building.Capitol))
            {
                return 4000;
            }

            if (castle.HasBuilt(Building.CityHall))
            {
                return 2000;
            }

            if (castle.HasBuilt(Building.TownHall))
            {
                return 1000;
            }

            return castle.HasBuilt(Building.VillageHall) ? 500 : 0;
        }

        /// <summary>
        /// Returns what the mines and the town halls of a player give each day.
        /// </summary>
        /// <param name="owner">The player.</param>
        /// <param name="mines">The mines of the map.</param>
        /// <param name="castles">The towns of the map.</param>
        public static ResourceSet Of(PlayerColor owner, IEnumerable<MineModel> mines, IEnumerable<CastleModel> castles)
        {
            var income = ResourceSet.None;
            foreach (var mine in mines)
            {
                if (mine.Owner == owner)
                {
                    var resource = MineKinds.ResourceOf(mine.Kind);
                    income = income.With(resource, income[resource] + YieldOf(mine.Kind));
                }
            }

            foreach (var castle in castles)
            {
                if (castle.IsUsed && castle.Owner == owner)
                {
                    income = income.With(Resource.Gold, income.Gold + HallGold(castle));
                }
            }

            return income;
        }

        private static long YieldOf(MineKind kind) => kind switch
        {
            MineKind.GoldMine => GoldMine,
            MineKind.Sawmill or MineKind.OreMine => Timber,
            _ => Mineral,
        };
    }
}
