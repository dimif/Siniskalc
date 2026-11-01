namespace Siniskalc.Core.Data
{
    /// <summary>
    /// What a <see cref="ByteRegion"/> holds. The hex editor colours regions by kind.
    /// </summary>
    public enum ByteRegionKind
    {
        /// <summary>Who or what the entity is: its id, name, class or owner.</summary>
        Identity,

        /// <summary>Where the entity is on the map.</summary>
        Position,

        /// <summary>The movement points of the entity.</summary>
        Movement,

        /// <summary>The primary statistics, experience and level.</summary>
        Stats,

        /// <summary>The creature types and counts of an army.</summary>
        Creatures,

        /// <summary>The secondary skills and their levels.</summary>
        Skills,

        /// <summary>The spells known and the spell book.</summary>
        Spells,

        /// <summary>The artifacts worn and carried.</summary>
        Artifacts,

        /// <summary>The war machines of a hero.</summary>
        WarMachines,

        /// <summary>The amounts of the game's resources.</summary>
        Resources,

        /// <summary>The savefile's signature, version and the map's header.</summary>
        Header,

        /// <summary>How the game was set up and where it stands: who plays, the difficulty, the date.</summary>
        Game,

        /// <summary>Bytes whose purpose is known but that fit none of the other kinds.</summary>
        Other,

        /// <summary>A kind that only one game has, which that game colours and labels itself.</summary>
        Custom,
    }
}
