namespace Siniskalc.Homm2.Models
{
    using System;
    using System.Linq;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Entities;

    /// <summary>
    /// A hero of Heroes of Might and Magic II as the savegame has it. There are 54 heroes, nine for each faction and all in the savegame
    /// whether the map uses them or not; a hero that no player has is in the pool of those a tavern offers.
    /// </summary>
    /// <param name="HeroId">The number of the hero, which is its place in the table of heroes (0 is Lord Kilburn).</param>
    /// <param name="Name">The name of the hero.</param>
    /// <param name="Owner">The player the hero serves, or <c>null</c> when it serves nobody.</param>
    /// <param name="Position">The tile the hero stands on (meaningless while it serves nobody).</param>
    /// <param name="Destination">The tile the hero was going to when its movement ran out, or <c>null</c>.</param>
    /// <param name="SpellPoints">The spell points the hero has now.</param>
    /// <param name="MovementPoints">The movement points the hero has left today, and the ones of a full day, which the game works out from the army, the skills and the artifacts.</param>
    /// <param name="HeroLevel">The level of the hero and its experience.</param>
    /// <param name="PrimarySkills">The attack, the defense, the spell power and the knowledge.</param>
    /// <param name="SecondarySkills">The secondary skills, in the order they were learned.</param>
    /// <param name="Army">The five slots of the army.</param>
    /// <param name="Artifacts">The fourteen artifact slots.</param>
    /// <param name="Spells">The spells of the spell book.</param>
    public sealed record HeroModel(
        int HeroId,
        string Name,
        Color? Owner,
        MapPosition Position,
        MapPosition? Destination,
        int SpellPoints,
        MovementPoints MovementPoints,
        HeroLevel HeroLevel,
        PrimarySkills PrimarySkills,
        SecondarySkills SecondarySkills,
        Army<Creature> Army,
        Artifacts Artifacts,
        SpellSet<Spell> Spells)
    {
        /// <summary>
        /// Gets or sets the level of the hero (a <c>with</c> expression sets it, and leaves the experience as it is).
        /// </summary>
        public int Level
        {
            get => this.HeroLevel.Level;
            init => this.HeroLevel = this.HeroLevel with { Level = value };
        }

        /// <summary>
        /// Gets or sets the experience of the hero (a <c>with</c> expression sets it, and leaves the level as it is).
        /// </summary>
        public long Experience
        {
            get => this.HeroLevel.Experience;
            init => this.HeroLevel = this.HeroLevel with { Experience = value };
        }

        /// <summary>
        /// Gets or sets the movement points of a full day.
        /// </summary>
        public long MaxMovement
        {
            get => this.MovementPoints.Max;
            init => this.MovementPoints = this.MovementPoints with { Max = value };
        }

        /// <summary>
        /// Gets or sets the movement points the hero has left today.
        /// </summary>
        public long MovementLeft
        {
            get => this.MovementPoints.Current;
            init => this.MovementPoints = this.MovementPoints with { Current = value };
        }

        /// <summary>
        /// Gets or sets the morale and luck the hero has for its next battle only.
        /// </summary>
        public NextBattleBonus NextBattle { get; init; }

        /// <summary>
        /// Gets or sets the places of the map the hero has visited and cannot get anything from again.
        /// </summary>
        public VisitedPlaces Visited { get; init; }

        /// <summary>
        /// Gets the faction of the hero: the heroes of each faction are numbered together, nine at a time.
        /// </summary>
        public Faction Faction => (Faction)Math.Min(this.HeroId / Constants.Hero.PerFaction, (int)Faction.Necromancer);

        /// <summary>
        /// Gets the few things about the hero that the lists of heroes show. The number of the hero is its place in the list as well.
        /// </summary>
        public HeroSummary<Color> Summary => new(this.HeroId, this.HeroId, this.Name, this.Owner, this.Faction.ToString(), this.Level);

        /// <summary>
        /// Gets a value indicating whether the hero has a spell book: the Magic Book is one of its artifacts.
        /// </summary>
        public bool HasSpellBook => this.Artifacts.HasMagicBook;

        /// <summary>
        /// Returns the hero with <paramref name="artifact"/> in <paramref name="slot"/> (or the slot emptied for <c>null</c>). The
        /// savegame keeps the primary skills with the bonuses of the artifacts in them, so what the artifact that leaves the slot gave
        /// is taken off and what the new one gives is added, as the game does when a hero is given an artifact. A skill stays between 0 and 255.
        /// </summary>
        public HeroModel WithArtifact(int slot, Artifact? artifact)
        {
            var old = this.Artifacts.Slots[slot];
            var delta = (artifact?.PrimarySkillBonus ?? PrimarySkillBonus.None) - (old?.PrimarySkillBonus ?? PrimarySkillBonus.None);
            var artifacts = this.Artifacts.WithSlot(slot, artifact);

            // The skills move by what the artifacts add or no longer add; the bonus is then worked out again from the artifacts the hero has.
            return this with { Artifacts = artifacts, PrimarySkills = this.PrimarySkills.Shifted(delta) with { Bonus = artifacts.TotalBonus } };
        }

        /// <summary>
        /// Returns the hero with <paramref name="artifact"/> in its first free slot, and its skills changed by what the artifact gives.
        /// </summary>
        /// <exception cref="GameRuleException">The hero has no free artifact slot.</exception>
        public HeroModel WithArtifact(Artifact artifact)
        {
            var free = Array.IndexOf(this.Artifacts.Slots.ToArray(), null);
            return free < 0 ? throw new GameRuleException("Every artifact slot is taken.") : this.WithArtifact(free, artifact);
        }

        /// <summary>
        /// Returns the hero without <paramref name="artifact"/> (nothing changes when it does not carry it), and its skills changed by what the artifact gave.
        /// </summary>
        public HeroModel WithoutArtifact(Artifact artifact)
        {
            var slot = Array.IndexOf(this.Artifacts.Slots.ToArray(), artifact);
            return slot < 0 ? this : this.WithArtifact(slot, null);
        }

        /// <summary>
        /// Returns the hero with the Magic Book in its first free slot (nothing changes when it has it already).
        /// </summary>
        /// <exception cref="GameRuleException">The hero has no free artifact slot.</exception>
        public HeroModel WithSpellBook() => this.HasSpellBook ? this : this with { Artifacts = this.Artifacts.Add(Artifact.MagicBook) };

        /// <summary>
        /// Returns the hero without the Magic Book. The spells it had stay marked, as they do in the game.
        /// </summary>
        public HeroModel WithoutSpellBook() => this with { Artifacts = this.Artifacts.Remove(Artifact.MagicBook) };
    }
}
