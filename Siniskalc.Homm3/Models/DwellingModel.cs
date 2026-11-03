namespace Siniskalc.Homm3.Models
{
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.ViewModels;
    using Siniskalc.Homm3.Entities;

    /// <summary>
    /// A dwelling of the map (a creature generator) as the savegame has it: the creature it gives, who owns it, where it is entered and how many
    /// creatures can be hired now.
    /// </summary>
    /// <param name="Index">The place of the dwelling among the dwellings of the savegame.</param>
    /// <param name="CreatureCode">The number of the creature the dwelling gives, as the savegame has it.</param>
    /// <param name="Creature">The creature, or <c>null</c> when the editor does not know the number.</param>
    /// <param name="Owner">The player that owns the dwelling, or <see cref="PlayerColor.Neutral"/>.</param>
    /// <param name="Position">The tile a hero stands on to flag the dwelling.</param>
    /// <param name="Stock">The creatures that can be hired now.</param>
    public sealed record DwellingModel(int Index, int CreatureCode, Creature? Creature, PlayerColor Owner, MapPosition Position, long Stock)
    {
        /// <summary>
        /// Gets the name of the dwelling with its place, such as "Pikeman dwelling at 31, 56, Surface", as the pending changes name it.
        /// </summary>
        public string Name => $"{this.Creature?.Name ?? $"Creature {this.CreatureCode}"} dwelling at {MapLevelText.Place(this.Position.X, this.Position.Y, this.Position.Level)}";
    }
}
