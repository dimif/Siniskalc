namespace Siniskalc.Homm3.Data;

using Siniskalc.Homm3.Entities;

public sealed record WarMachineDefinition : EntityDefinition
{
    public required WarMachineSlot Slot { get; init; }

    public required WarMachineEncoding Encoding { get; init; }
}
