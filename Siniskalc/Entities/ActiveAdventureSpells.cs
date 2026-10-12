namespace Siniskalc.Entities
{
    using System;

    /// <summary>
    /// Active adventure map spells on a hero.
    /// </summary>
    [Flags]
    public enum ActiveAdventureSpells
    {
        None = 0,
        Disguise = 1,
        Fly = 2,
        WaterWalk = 4,
        Visions = 8,
    }
}
