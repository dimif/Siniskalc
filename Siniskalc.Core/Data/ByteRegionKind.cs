namespace Siniskalc.Core.Data
{
    /// <summary>
    /// What a <see cref="ByteRegion"/> holds. The hex editor colours regions by kind.
    /// </summary>
    public enum ByteRegionKind
    {
        Identity,
        Position,
        Movement,
        Stats,
        Creatures,
        Skills,
        Spells,
        Artifacts,
        WarMachines,
        Resources,

        /// <summary>The savefile's signature, version and the map's header.</summary>
        Header,

        /// <summary>How the game was set up and where it stands: who plays, the difficulty, the date.</summary>
        Game,
        Other,

        /// <summary>A kind that only one game has, which that game colours and labels itself.</summary>
        Custom,
    }
}
