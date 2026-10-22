namespace Siniskalc.Heroes.Entities
{
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.ViewModels;

    /// <summary>
    /// What each kind of mine gives and how the savegame draws its flag.
    /// </summary>
    public static class MineKinds
    {
        /// <summary>
        /// Returns the name of a kind of mine.
        /// </summary>
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
        /// Returns the name of a mine with its place, as the pending changes and the records name it: "Gold mine at X: 12, Y: 7", with the level in a
        /// game whose map has two ("Gold mine at X: 12, Y: 7, Underground (1)").
        /// </summary>
        /// <param name="kind">The kind of the mine.</param>
        /// <param name="position">The tile of the mine.</param>
        /// <param name="hasLevels">Whether the map of the game has two levels.</param>
        public static string NameAt(MineKind kind, MapPosition position, bool hasLevels) => $"{NameOf(kind)} at {MapLevelText.Coordinates(position, hasLevels)}";

        /// <summary>
        /// Returns the resource a kind of mine gives.
        /// </summary>
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

        /// <summary>
        /// Returns the picture of the flag of the first colour (Blue): the picture of the flag of a player is this plus its colour. The
        /// flags of the mines, the alchemist's lab and the sawmill are three series of seven pictures.
        /// </summary>
    }
}
