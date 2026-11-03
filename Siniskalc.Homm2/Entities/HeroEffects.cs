namespace Siniskalc.Homm2.Entities
{
    using System;

    /// <summary>
    /// The things a hero remembers of the places it visited that are not one of the sets of <see cref="PlaceKind"/> (the 32 bits from
    /// the byte after the spell book flags, +0xD9 of the record from the name). Seen in the saves of Threedom: a fountain, an oasis, a faerie
    /// ring, a graveyard that was visited before and a temple set a bit with the luck or morale they gave for the next battle, and the game
    /// clears it with that bonus; the arena sets a bit for good, and the stables one that the game clears at the start of a week.
    /// </summary>
    [Flags]
    public enum HeroEffects : uint
    {
        None = 0,

        /// <summary>
        /// +1 luck for the next battle.
        /// </summary>
        Fountain = 0x04,

        /// <summary>
        /// +1 morale for the next battle (and movement points: the oasis also raises the movement of the day).
        /// </summary>
        Oasis = 0x08,

        /// <summary>
        /// +1 luck for the next battle.
        /// </summary>
        FaerieRing = 0x10,

        /// <summary>
        /// -1 morale for the next battle: the graveyard was visited before.
        /// </summary>
        Graveyard = 0x20,

        /// <summary>
        /// +2 morale for the next battle.
        /// </summary>
        Temple = 0x100,

        /// <summary>
        /// The hero used the arena, which gives a point to a skill once per hero.
        /// </summary>
        Arena = 0x400000,

        /// <summary>
        /// The hero used the stables this week (more movement points); the game clears it at the start of the week.
        /// </summary>
        Stables = 0x800000,
    }
}
