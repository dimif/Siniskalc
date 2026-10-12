namespace Siniskalc.ViewModels
{
    using System;
    using System.Linq;
    using Siniskalc.Entities;
    using Siniskalc.Models;

    /// <summary>
    /// The words for why a building cannot be built, removed or allowed, built from the <see cref="CastleBuildFailureReason"/> that
    /// <see cref="CastleBuilder"/> gives. Every method returns null when the building can be changed.
    /// </summary>
    public sealed class CastleBuildMessageProvider
    {
        public string? DescribeBuild(CastleModel castle, Building building)
        {
            var reason = CastleBuilder.GetBuildFailureReason(castle, building);
            return reason switch
            {
                CastleBuildFailureReason.None => null,
                CastleBuildFailureReason.AutomaticBuilding => "The game builds it by itself with " + string.Join(" and ", building.Prerequisites.Select(p => p.Name)),
                CastleBuildFailureReason.UpgradedBuildingMissing => "Requires " + building.Upgrades?.Name,
                _ => Describe(reason),
            };
        }

        public string? DescribeUnbuild(CastleModel castle, Building building)
        {
            var reason = CastleBuilder.GetUnbuildFailureReason(castle, building);
            return reason switch
            {
                CastleBuildFailureReason.None => null,
                CastleBuildFailureReason.AutomaticBuilding => "Goes away with " + string.Join(" or ", building.Prerequisites.Select(p => p.Name)),
                CastleBuildFailureReason.NeededByUpgrade => "Needed by " + string.Join(", ", CastleBuilder.UpgradesBuiltOn(castle, building).Select(b => b.Name)),
                _ => Describe(reason),
            };
        }

        public string? DescribeSetAllowed(CastleModel castle, Building building)
        {
            var reason = CastleBuilder.GetSetAllowedFailureReason(castle, building);
            return reason switch
            {
                CastleBuildFailureReason.None => null,
                CastleBuildFailureReason.AutomaticBuilding => "The game builds it by itself",
                _ => Describe(reason),
            };
        }

        private static string Describe(CastleBuildFailureReason reason)
        {
            return reason switch
            {
                CastleBuildFailureReason.NotAvailableForTown => "This town cannot have it",
                CastleBuildFailureReason.AlreadyBuilt => "Already built",
                CastleBuildFailureReason.NotBuilt => "Not built",
                CastleBuildFailureReason.NotAllowedByMap => "Not allowed by the map",
                CastleBuildFailureReason.VillageHallRequired => "Every town has a village hall",
                _ => throw new ArgumentOutOfRangeException(nameof(reason), reason, "No words for this reason."),
            };
        }
    }
}
