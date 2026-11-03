namespace Siniskalc.Homm3.Models
{
    /// <summary>
    /// Why <see cref="CastleBuilder"/> refuses to build a building (<see cref="CastleBuilder.GetBuildFailureReason"/>), remove it
    /// (<see cref="CastleBuilder.GetUnbuildFailureReason"/>) or change whether the map allows it
    /// (<see cref="CastleBuilder.GetSetAllowedFailureReason"/>). The words for each reason belong to the view model.
    /// </summary>
    public enum CastleBuildFailureReason
    {
        /// <summary>Nothing stands in the way: the building can be built, removed or have its map permission changed, as asked.</summary>
        None,

        /// <summary>
        /// Build and set-allowed: the castle is <c>Neutral</c> (has no faction), or its faction has no such
        /// building (<see cref="Entities.Building.IsAvailableFor"/> is false).
        /// </summary>
        NotAvailableForTown,

        /// <summary>
        /// Build and set-allowed: the castle already has the building (<see cref="CastleModel.HasBuilt"/>). The map permission
        /// only matters for a building that is not built yet.
        /// </summary>
        AlreadyBuilt,

        /// <summary>Remove: the castle does not have the building, so there is nothing to remove.</summary>
        NotBuilt,

        /// <summary>
        /// The building is one of the upgraded horde buildings (<see cref="CastleBuilder.IsAutomatic"/>). The game adds it by itself
        /// once its prerequisites exist and drops it with them, so it can be neither built, removed nor allowed on its own.
        /// </summary>
        AutomaticBuilding,

        /// <summary>
        /// Build: the building is not in <see cref="CastleModel.EnabledBuildings"/>, the buildings the map allows. Set the
        /// permission first.
        /// </summary>
        NotAllowedByMap,

        /// <summary>
        /// Build: the building is an upgrade (<see cref="Entities.Building.Upgrades"/> is set) and the building it replaces has
        /// not been built. Other prerequisites (<see cref="Entities.Building.Requires"/>) are not checked.
        /// </summary>
        UpgradedBuildingMissing,

        /// <summary>Remove: the building is the village hall, which every town keeps.</summary>
        VillageHallRequired,

        /// <summary>
        /// Remove: another built building is an upgrade on top of this one
        /// (<see cref="CastleBuilder.UpgradesBuiltOn"/> is not empty), so it must be removed first.
        /// </summary>
        NeededByUpgrade,
    }
}
