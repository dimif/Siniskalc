namespace Siniskalc.Homm3.Data;

using Siniskalc.Heroes.Data;
using Siniskalc.Homm3.Entities;

public sealed class CreatureCodes(EntityCatalog catalog) : ICreatureCodes<Creature>
{
    public EntityCatalog Catalog => catalog;

    public Creature Fallback => catalog.FallbackCreature;

    public Creature? Find(uint code) => code > byte.MaxValue ? null : catalog.Creatures.Find((byte)code);
}
