namespace Siniskalc.Data
{
    using System;
    using Siniskalc.Entities;

    /// <summary>
    /// Where in a hero chunk a <see cref="WarMachine"/> is written. A layout, so it is kept here and not on the entity.
    /// </summary>
    public static class WarMachineOffsets
    {
        public static HeroOffsetName For(WarMachine warMachine)
        {
            if (warMachine == WarMachine.Ballista)
            {
                return HeroOffsetName.Ballista;
            }

            if (warMachine == WarMachine.Cannon)
            {
                return HeroOffsetName.Cannon;
            }

            if (warMachine == WarMachine.AmmoCart)
            {
                return HeroOffsetName.AmmoCart;
            }

            if (warMachine == WarMachine.FirstAidTent)
            {
                return HeroOffsetName.FirstAidTent;
            }

            throw new ArgumentOutOfRangeException(nameof(warMachine), warMachine, "The war machine has no known place in a hero.");
        }
    }
}
