namespace Siniskalc.Heroes.Entities
{

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
