namespace Siniskalc.Homm3.Data;

using System;
using Siniskalc.Homm3.Entities;

public static class WarMachineOffsets
{
    public static HeroOffsetName For(WarMachine machine) => machine.Encoding switch
    {
        WarMachineEncoding.Ballista => HeroOffsetName.Ballista,
        WarMachineEncoding.Cannon => HeroOffsetName.Cannon,
        WarMachineEncoding.AmmoCart => HeroOffsetName.AmmoCart,
        WarMachineEncoding.FirstAidTent => HeroOffsetName.FirstAidTent,
        _ => throw new ArgumentOutOfRangeException(nameof(machine)),
    };
}
