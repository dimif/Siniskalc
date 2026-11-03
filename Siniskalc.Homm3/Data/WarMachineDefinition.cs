namespace Siniskalc.Homm3.Data;

using Siniskalc.Homm3.Entities;

public sealed record WarMachineDefinition : EntityDefinition
{
    /// <summary>
    /// Gets the slot of the hero the war machine is in.
    /// </summary>
    public required WarMachineSlot Slot { get; init; }

    /// <summary>
    /// Gets how the savefile holds the war machine.
    /// </summary>
    public required WarMachineEncoding Encoding { get; init; }
}
