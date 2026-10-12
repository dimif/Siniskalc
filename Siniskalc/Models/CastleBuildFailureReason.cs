namespace Siniskalc.Models
{
    /// <summary>
    /// Why <see cref="CastleBuilder"/> will not build, remove or allow a building. The words for each reason belong to the view model.
    /// </summary>
    public enum CastleBuildFailureReason
    {
        /// <summary>The building can be changed.</summary>
        None,

        /// <summary>The town has no faction, or its faction does not have the building.</summary>
        NotAvailableForTown,

        AlreadyBuilt,

        NotBuilt,

        /// <summary>The game builds and removes the building by itself, together with its prerequisites.</summary>
        AutomaticBuilding,

        NotAllowedByMap,

        /// <summary>The building upgrades one that has not been built yet.</summary>
        UpgradedBuildingMissing,

        /// <summary>Every town has a village hall.</summary>
        VillageHallRequired,

        /// <summary>Another built building is an upgrade on top of this one.</summary>
        NeededByUpgrade,
    }
}
