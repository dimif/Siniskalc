namespace Siniskalc.Entities
{
    using System;

    /// <summary>
    /// Flags used to indicate which artifact slots are/can be occupied by a hero's artifacts.
    /// </summary>
    [Flags]
    public enum ArtifactSlots
    {
        None = 0,
        Helmet = 1,
        Necklace = 2,
        Torso = 4,
        Feet = 8,
        Weapon = 16,
        Shield = 32,
        Cape = 64,
        Ring1 = 128,
        Ring2 = 256,
        Rings = Ring1 | Ring2,
        Misc1 = 512,
        Misc2 = 1024,
        Misc3 = 2048,
        Misc4 = 4096,
        Misc5 = 8192,
        Misc = Misc1 | Misc2 | Misc3 | Misc4 | Misc5,
    }
}
