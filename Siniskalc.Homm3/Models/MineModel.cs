namespace Siniskalc.Homm3.Models
{
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm3.Entities;

    /// <summary>
    /// A mine of the map as the savegame has it: what it is, who owns it, and the creatures that guard it.
    /// </summary>
    /// <param name="Index">The place of the mine among the mines of the savegame.</param>
    /// <param name="Kind">The kind of mine; an abandoned mine has the kind of the resource it gives once it is taken.</param>
    /// <param name="Owner">The player that owns the mine, or <see cref="PlayerColor.Neutral"/>.</param>
    /// <param name="Position">The tile the record gives (see <c>MineLayout</c>): not the tile where the mine is entered.</param>
    /// <param name="Guard">The creatures that guard the mine. Shaped like a hero's army: slots 1 to 7, empty slots absent.</param>
    public sealed record MineModel(int Index, MineKind Kind, PlayerColor Owner, MapPosition Position, IArmy<Creature> Guard)
    {
        public string Name => $"{MineKinds.NameOf(this.Kind)} {this.Position.X}, {this.Position.Y}";
    }
}
