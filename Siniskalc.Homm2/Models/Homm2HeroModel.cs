namespace Siniskalc.Homm2.Models
{
    using System;
    using Siniskalc.Homm2.Entities;

    /// <summary>
    /// A hero of Heroes of Might and Magic II as the savegame has it. There are 54 heroes, nine for each race and all in the savegame
    /// whether the map uses them or not; a hero that no player has is in the pool of those a tavern offers.
    /// </summary>
    /// <param name="HeroId">The number of the hero, which is its place in the table of heroes (0 is Lord Kilburn).</param>
    /// <param name="Name">The name of the hero.</param>
    /// <param name="Owner">The player the hero serves, or <c>null</c> when it serves nobody.</param>
    /// <param name="Position">The tile the hero stands on (meaningless while it serves nobody).</param>
    /// <param name="Destination">The tile the hero was going to when its movement ran out, or <c>null</c>.</param>
    /// <param name="SpellPoints">The spell points the hero has now.</param>
    /// <param name="MaxMovement">The movement points of a full day, which the game works out from the army, the skills and the artifacts.</param>
    /// <param name="MovementLeft">The movement points the hero has left today.</param>
    /// <param name="Experience">The experience of the hero.</param>
    /// <param name="Level">The level of the hero.</param>
    /// <param name="PrimarySkills">The attack, the defense, the spell power and the knowledge.</param>
    /// <param name="SecondarySkills">The secondary skills, in the order they were learned.</param>
    /// <param name="Army">The five slots of the army.</param>
    /// <param name="Artifacts">The fourteen artifact slots.</param>
    /// <param name="Spells">The spells of the spell book.</param>
    public sealed record Homm2HeroModel(
        int HeroId,
        string Name,
        Homm2Color? Owner,
        Homm2Position Position,
        Homm2Position? Destination,
        int SpellPoints,
        long MaxMovement,
        long MovementLeft,
        long Experience,
        int Level,
        Homm2PrimarySkills PrimarySkills,
        Homm2SecondarySkills SecondarySkills,
        Homm2Army Army,
        Homm2Artifacts Artifacts,
        Homm2SpellBook Spells)
    {
        private const int HeroesPerRace = 9;

        /// <summary>
        /// Gets the race of the hero: the heroes of each race are numbered together, nine at a time.
        /// </summary>
        public Homm2Race Race => (Homm2Race)Math.Min(this.HeroId / HeroesPerRace, (int)Homm2Race.Necromancer);

        /// <summary>
        /// Gets a value indicating whether the hero has a spell book: the Magic Book is one of its artifacts.
        /// </summary>
        public bool HasSpellBook => this.Artifacts.HasMagicBook;

        /// <summary>
        /// Returns the hero with the Magic Book in its first free slot (nothing changes when it has it already).
        /// </summary>
        /// <exception cref="InvalidOperationException">The hero has no free artifact slot.</exception>
        public Homm2HeroModel WithSpellBook() => this.HasSpellBook ? this : this with { Artifacts = this.Artifacts.Add(Homm2Artifact.MagicBook) };

        /// <summary>
        /// Returns the hero without the Magic Book. The spells it had stay marked, as they do in the game.
        /// </summary>
        public Homm2HeroModel WithoutSpellBook() => this with { Artifacts = this.Artifacts.Remove(Homm2Artifact.MagicBook) };
    }
}
