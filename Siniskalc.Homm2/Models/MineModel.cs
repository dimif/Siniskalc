namespace Siniskalc.Homm2.Models
{
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Entities;

    /// <summary>
    /// A mine of the map: a record of the map part of the savegame that says who owns it, where it is and what it is. The flag on the
    /// map follows the owner (see <c>MapObjectsReader</c> for where it is drawn).
    /// </summary>
    /// <param name="Index">The place of the mine in the list of the map's mines.</param>
    /// <param name="Kind">What the mine gives.</param>
    /// <param name="Position">The tile of the mine.</param>
    /// <param name="Owner">The player that owns it, or <see cref="PlayerColor.Neutral"/> for nobody.</param>
    public sealed record MineModel(int Index, MineKind Kind, MapPosition Position, PlayerColor Owner) : IMineModel<PlayerColor>
    {
        /// <summary>
        /// Gets the name of the mine with its place, such as "Gold mine at X: 12, Y: 7" (the map of HoMM2 has one level).
        /// </summary>
        public string Name => MineKinds.NameAt(this.Kind, this.Position, hasLevels: false);
    }
}
