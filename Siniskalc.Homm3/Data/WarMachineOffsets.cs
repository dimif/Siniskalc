namespace Siniskalc.Homm3.Data;

using System;
using Siniskalc.Homm3.Entities;

public static class WarMachineOffsets
{
    /// <summary>
    /// Gets the field of the chunk of a hero that holds a war machine.
    /// </summary>
    /// <param name="machine">The war machine.</param>
    /// <returns>The name of the offset.</returns>
    public static HeroOffsetName For(WarMachine machine) => machine.Encoding switch
    {
        WarMachineEncoding.Ballista => HeroOffsetName.Ballista,
        WarMachineEncoding.Cannon => HeroOffsetName.Cannon,
        WarMachineEncoding.AmmoCart => HeroOffsetName.AmmoCart,
        WarMachineEncoding.FirstAidTent => HeroOffsetName.FirstAidTent,
        _ => throw new ArgumentOutOfRangeException(nameof(machine)),
    };
}
