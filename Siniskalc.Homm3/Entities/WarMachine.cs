namespace Siniskalc.Homm3.Entities;

using Siniskalc.Homm3.Data;

public sealed class WarMachine : CatalogEntity
{
    internal WarMachine(EntityCatalog catalog, WarMachineDefinition definition)
        : base(catalog, definition)
    {
        this.Slot = definition.Slot;
        this.Encoding = definition.Encoding;
    }

    public WarMachineSlot Slot { get; }

    public WarMachineEncoding Encoding { get; }
}
