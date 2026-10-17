namespace Siniskalc.Homm2.Models
{
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Entities;

    /// <summary>
    /// A mine of the map: a record of the map part of the savegame that says who owns it, where it is and what it is. The flag on the
    /// map follows the owner (see <c>MapObjectsReader</c> for where it is drawn).
    /// </summary>
    /// <param name="Index">The place of the mine in the list of the map's mines.</param>
    /// <param name="Kind">What the mine gives.</param>
    /// <param name="Position">The tile of the mine.</param>
    /// <param name="Owner">The player that owns it, or <c>null</c> for nobody.</param>
    public sealed record MineModel(int Index, MineKind Kind, MapPosition Position, Color? Owner)
    {
        public string Name => $"{MineKinds.NameOf(this.Kind)} at X: {this.Position.X}, Y: {this.Position.Y}";
    }
}
