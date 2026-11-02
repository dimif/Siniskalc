namespace Siniskalc.Heroes.Entities
{
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.ViewModels;

    /// <summary>
    /// What each kind of mine is called and which resource it gives.
    /// </summary>
    public static class MineKinds
    {
        /// <summary>
        /// Returns the name of a kind of mine.
        /// </summary>
        /// <param name="kind">The kind of mine.</param>
        /// <returns>The name.</returns>
        public static string NameOf(MineKind kind) => kind switch
        {
            MineKind.Sawmill => "Sawmill",
            MineKind.AlchemistLab => "Alchemist's lab",
            MineKind.OreMine => "Ore mine",
            MineKind.SulfurMine => "Sulfur mine",
            MineKind.CrystalMine => "Crystal mine",
            MineKind.GemMine => "Gem mine",
            _ => "Gold mine",
        };

        /// <summary>
        /// Returns the name of a mine with its place, as the pending changes and the records name it: "Gold mine at 12, 7", with the level in a
        /// game whose map has two ("Gold mine at 12, 7, Underground").
        /// </summary>
        /// <param name="kind">The kind of the mine.</param>
        /// <param name="position">The tile of the mine.</param>
        /// <param name="hasLevels">Whether the map of the game has two levels.</param>
        /// <returns>The name.</returns>
        public static string NameAt(MineKind kind, MapPosition position, bool hasLevels) => $"{NameOf(kind)} at {MapLevelText.Place(position, hasLevels)}";

        /// <summary>
        /// Returns the resource a kind of mine gives.
        /// </summary>
        /// <param name="kind">The kind of mine.</param>
        /// <returns>The resource.</returns>
        public static Resource ResourceOf(MineKind kind) => kind switch
        {
            MineKind.Sawmill => Resource.Wood,
            MineKind.AlchemistLab => Resource.Mercury,
            MineKind.OreMine => Resource.Ore,
            MineKind.SulfurMine => Resource.Sulfur,
            MineKind.CrystalMine => Resource.Crystal,
            MineKind.GemMine => Resource.Gems,
            _ => Resource.Gold,
        };
    }
}
