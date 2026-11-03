namespace Siniskalc.Homm3.Entities;

using Siniskalc.Homm3.Data;

public sealed class WarMachine : CatalogEntity
{
    /// <summary>
    /// Initializes a new instance of the <see cref="None"/> class.
    /// </summary>
    /// <param name="catalog">The catalog.</param>
    /// <param name="definition">The definition.</param>
    internal WarMachine(EntityCatalog catalog, WarMachineDefinition definition)
        : base(catalog, definition)
    {
        this.Slot = definition.Slot;
        this.Encoding = definition.Encoding;
    }

    /// <summary>
    /// Gets the slot of the hero the war machine is in.
    /// </summary>
    public WarMachineSlot Slot { get; }

    /// <summary>
    /// Gets how the savefile holds the war machine.
    /// </summary>
    public WarMachineEncoding Encoding { get; }
}
