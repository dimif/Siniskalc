namespace Siniskalc.Entities
{
    using System.Collections.Generic;

    public sealed class WarMachine : Homm3Enumeration
    {
        public static readonly WarMachine Ballista = new("Ballista", 0x04, WarMachineSlot.Artillery);
        public static readonly WarMachine AmmoCart = new("Ammo Cart", 0x05, WarMachineSlot.AmmoCart);
        public static readonly WarMachine FirstAidTent = new("First Aid Tent", 0x06, WarMachineSlot.FirstAidTent);
        public static readonly WarMachine Cannon = new("Cannon", 0x92, GameVersion.HornOfTheAbyss, WarMachineSlot.Artillery);

        private WarMachine(string name, byte code, GameVersion gameVersion, WarMachineSlot slot)
            : base(name, code, gameVersion)
        {
            this.Slot = slot;
        }

        private WarMachine(string name, byte code, WarMachineSlot slot)
            : this(name, code, GameVersion.RestorationOfErathia, slot)
        {
        }

        public static IReadOnlyList<WarMachine> All => GetAll<WarMachine>();

        /// <summary>
        /// Gets the place the hero keeps the machine. Machines of one place exclude each other.
        /// </summary>
        public WarMachineSlot Slot { get; }
    }
}
