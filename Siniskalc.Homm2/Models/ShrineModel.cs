namespace Siniskalc.Homm2.Models
{
    using System.Linq;
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
    public sealed record ShrineModel(int Index, int Circle, MapPosition Position, int SpellCode, GameCatalog Catalog)
    {
        /// <summary>
        /// Gets the spell, or <c>null</c> when the tile holds a number that is no spell the editor knows.
        /// </summary>
        public Spell? Spell => this.SpellCode is >= 0 and <= byte.MaxValue ? this.Catalog.Spells.Find((byte)this.SpellCode) : null;

        /// <summary>
        /// Gets the name the editor shows for the shrine, which says its circle and its place.
        /// </summary>
        public string Name => this.Catalog.MapObjects.Single(o => o.Role == this.Circle switch { 1 => MapObjectRole.ShrineFirst, 2 => MapObjectRole.ShrineSecond, _ => MapObjectRole.ShrineThird }).FormatName(this.Position.X, this.Position.Y);

        /// <summary>
        /// Returns the shrine teaching another spell.
        /// </summary>
        /// <param name="spell">The spell.</param>
        /// <returns>The new shrine; this one is not changed.</returns>
        public ShrineModel WithSpell(Spell spell) => this with { SpellCode = spell.Code };
    }
}
