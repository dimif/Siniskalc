namespace Siniskalc.Homm2.Entities
{
    /// <summary>
    /// The kinds of spell that the rules of the game single out.
    /// </summary>
    public enum SpellGroup
    {
        /// <summary>
        /// A spell that is neither of the others.
        /// </summary>
        None,

        /// <summary>
        /// Blind, Paralyze, Berserker and Hypnotize: the giants and the titans are immune to them.
        /// </summary>
        Mind,

        /// <summary>
        /// Lightning Bolt, Chain Lightning, Cold Ray, Cold Ring, Fireball, Fireblast and Elemental Storm.
        /// </summary>
        Elemental,
    }
}
