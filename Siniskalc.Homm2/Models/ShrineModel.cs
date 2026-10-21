namespace Siniskalc.Homm2.Models
{
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Entities;

    /// <summary>
    /// A shrine of the map: it teaches a spell to the hero that visits it. The spell is in the tile of the shrine, the circle (1 to 3) is
    /// the level of the spells it teaches.
    /// </summary>
    /// <param name="Index">The place of the shrine in the list of the map's shrines.</param>
    /// <param name="Circle">The circle of the shrine, 1 to 3.</param>
    /// <param name="Position">The tile a hero enters the shrine by.</param>
    /// <param name="SpellCode">The number of the spell the tile holds.</param>
    public sealed record ShrineModel(int Index, int Circle, MapPosition Position, int SpellCode)
    {
        /// <summary>
        /// Gets the spell, or <c>null</c> when the tile holds a number that is no spell the editor knows.
        /// </summary>
        public Spell? Spell => this.SpellCode is >= 0 and <= byte.MaxValue ? Entities.Spell.FromByte((byte)this.SpellCode) : null;

        public string Name => $"Shrine of the {this.Circle switch { 1 => "first", 2 => "second", _ => "third" }} circle at X: {this.Position.X}, Y: {this.Position.Y}";

        /// <summary>
        /// Returns the shrine teaching another spell.
        /// </summary>
        public ShrineModel WithSpell(Spell spell) => this with { SpellCode = spell.Code };
    }
}
