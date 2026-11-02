namespace Siniskalc.Homm3.Data;

using Siniskalc.Heroes.Data;
using Siniskalc.Homm3.Entities;

public sealed class CreatureCodes(EntityCatalog catalog) : ICreatureCodes<Creature>
{
    /// <summary>
    /// Gets the catalog the creatures are taken from.
    /// </summary>
    public EntityCatalog Catalog => catalog;

    /// <summary>
    /// Gets the creature a code that is no creature reads as.
    /// </summary>
    public Creature Fallback => catalog.FallbackCreature;

    /// <summary>
    /// Finds a creature by its code.
    /// </summary>
    /// <param name="code">The code, a 32-bit number in the savefile.</param>
    /// <returns>The creature, or <see langword="null"/> when no creature has the code.</returns>
    public Creature? Find(uint code) => code > byte.MaxValue ? null : catalog.Creatures.Find((byte)code);
}
