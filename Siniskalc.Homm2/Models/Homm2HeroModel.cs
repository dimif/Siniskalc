namespace Siniskalc.Homm2.Models
{
    using System;
    using System.Linq;
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
        /// Returns the hero with <paramref name="artifact"/> in <paramref name="slot"/> (or the slot emptied for <c>null</c>). The
        /// savegame keeps the primary skills with the bonuses of the artifacts in them, so what the artifact that leaves the slot gave
        /// is taken off and what the new one gives is added, as the game does when a hero is given an artifact. A skill stays between 0 and 255.
        /// </summary>
        public Homm2HeroModel WithArtifact(int slot, Homm2Artifact? artifact)
        {
            var old = this.Artifacts.Slots[slot];
            var skills = this.PrimarySkills;
            if (old != null)
            {
                skills = Shift(skills, Homm2ArtifactBonuses.Of(old), -1);
            }

            if (artifact != null)
            {
                skills = Shift(skills, Homm2ArtifactBonuses.Of(artifact), 1);
            }

            return this with { Artifacts = this.Artifacts.WithSlot(slot, artifact), PrimarySkills = skills };
        }

        /// <summary>
        /// Returns the hero with <paramref name="artifact"/> in its first free slot, and its skills changed by what the artifact gives.
        /// </summary>
        /// <exception cref="InvalidOperationException">The hero has no free artifact slot.</exception>
        public Homm2HeroModel WithArtifact(Homm2Artifact artifact)
        {
            var free = Array.IndexOf(this.Artifacts.Slots.ToArray(), null);
            return free < 0 ? throw new InvalidOperationException("Every artifact slot is taken.") : this.WithArtifact(free, artifact);
        }

        /// <summary>
        /// Returns the hero without <paramref name="artifact"/> (nothing changes when it does not carry it), and its skills changed by what the artifact gave.
        /// </summary>
        public Homm2HeroModel WithoutArtifact(Homm2Artifact artifact)
        {
            var slot = Array.IndexOf(this.Artifacts.Slots.ToArray(), artifact);
            return slot < 0 ? this : this.WithArtifact(slot, null);
        }

        /// <summary>
        /// Returns the hero with the Magic Book in its first free slot (nothing changes when it has it already).
        /// </summary>
        /// <exception cref="InvalidOperationException">The hero has no free artifact slot.</exception>
        public Homm2HeroModel WithSpellBook() => this.HasSpellBook ? this : this with { Artifacts = this.Artifacts.Add(Homm2Artifact.MagicBook) };

        /// <summary>
        /// Returns the hero without the Magic Book. The spells it had stay marked, as they do in the game.
        /// </summary>
        public Homm2HeroModel WithoutSpellBook() => this with { Artifacts = this.Artifacts.Remove(Homm2Artifact.MagicBook) };

        private static Homm2PrimarySkills Shift(Homm2PrimarySkills skills, Homm2PrimarySkills bonus, int sign) => new(
            Limit(skills.Attack + (sign * bonus.Attack)),
            Limit(skills.Defense + (sign * bonus.Defense)),
            Limit(skills.SpellPower + (sign * bonus.SpellPower)),
            Limit(skills.Knowledge + (sign * bonus.Knowledge)));

        private static int Limit(int skill) => (int)Math.Clamp(skill, Constants.Hero.MinPrimarySkill, Constants.Hero.MaxPrimarySkill);
    }
}
