namespace Siniskalc.Homm2.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;

    using Siniskalc.Homm2.Entities;

    /// <summary>
    /// A hero of Heroes of Might and Magic II as the savegame has it. There are 54 heroes, nine for each faction and all in the savegame
    /// whether the map uses them or not; a hero that no player has is in the pool of those a tavern offers.
    /// </summary>
    /// <param name="Id">The stored hero/portrait byte (0 is Lord Kilburn); scenario portraits can differ from the reference id, which is the table index.</param>
    /// <param name="Name">The name of the hero.</param>
    /// <param name="Owner">The player the hero serves, or the colour <c>Neutral</c> when it serves nobody.</param>
    /// <param name="Position">The tile the hero stands on (meaningless while it serves nobody).</param>
    /// <param name="Destination">The tile the hero is going to (the game marks it on the map), or <see cref="MapPosition.None"/>.</param>
    /// <param name="MovementPoints">The movement points the hero has left today, and the ones of a full day, which the game works out from the army, the skills and the artifacts.</param>
    /// <param name="HeroLevel">The level of the hero and its experience.</param>
    /// <param name="SpellPoints">The spell points the hero has now.</param>
    /// <param name="PrimarySkills">The attack, the defense, the spell power and the knowledge.</param>
    /// <param name="SecondarySkills">The secondary skills, in the order they were learned.</param>
    /// <param name="Army">The five slots of the army.</param>
    /// <param name="Faction">The faction of the hero: the heroes of each faction are numbered together, nine at a time. The savegame does not hold it.</param>
    /// <param name="Artifacts">The fourteen artifact slots.</param>
    /// <param name="Spells">The spells of the spell book.</param>
    public sealed record HeroModel(
        int Id,
        string Name,
        PlayerColor Owner,
        HeroPosition Position,
        MapPosition Destination,
        MovementPoints MovementPoints,
        HeroLevel HeroLevel,
        int SpellPoints,
        PrimarySkills PrimarySkills,
        SecondarySkills SecondarySkills,
        IArmy<Creature> Army,
        Faction Faction,
        Artifacts Artifacts,
        SpellSet<Spell> Spells) : IHeroModel<SecondarySkill, SecondarySkills, Creature, PlayerColor>
    {
        /// <summary>
        /// Gets or sets the morale and luck the hero has for its next battle only.
        /// </summary>
        public NextBattleBonus NextBattle { get; init; }

        /// <summary>
        /// Gets or sets what the hero remembers of the fountain, oasis, faerie ring, graveyard, temple, arena and stables it visited.
        /// </summary>
        public HeroEffects Effects { get; init; }

        /// <summary>
        /// Gets or sets the places of the map the hero has visited and cannot get anything from again.
        /// </summary>
        public VisitedPlaces Visited { get; init; }

        /// <summary>
        /// Gets the few things about the hero that lists show. The collection supplies the reference id and actual index when returning summaries.
        /// </summary>
        public HeroSummary<PlayerColor> Summary => new(this.Id, this.Id, this.Name, this.Owner, this.Faction.ToString(), this.HeroLevel.Level);

        /// <inheritdoc/>
        public IReadOnlyCollection<ISpell> LearnedSpells => [.. this.Spells];

        /// <summary>
        /// Gets a value indicating whether the hero has a spell book: the Magic Book is one of its artifacts.
        /// </summary>
        public bool HasSpellBook => this.Artifacts.HasMagicBook;

        /// <summary>
        /// Returns the hero at <paramref name="level"/>, with the experience that level starts at.
        /// </summary>
        public HeroModel WithLevel(int level)
        {
            RangeGuard.ThrowIfOutOfRange<long>(level, Constants.Hero.MinLevel, Constants.Hero.MaxLevel, nameof(level));
            return this with { HeroLevel = HeroLevel.AtLevel(HeroLevels.Table, level) };
        }

        /// <summary>
        /// Returns the hero with <paramref name="experience"/> points, at the level the game gives that much experience.
        /// </summary>
        public HeroModel WithExperience(long experience)
        {
            RangeGuard.ThrowIfOutOfRange(experience, Constants.Hero.MinExperience, Constants.Hero.MaxExperience, nameof(experience));
            return this with { HeroLevel = HeroLevel.AtExperience(HeroLevels.Table, experience) };
        }

        /// <summary>
        /// Returns the hero with <paramref name="movement"/> movement points left today; the movement of a full day stays.
        /// </summary>
        public HeroModel WithMovementLeft(long movement) => this with { MovementPoints = this.MovementPoints with { Current = movement } };

        /// <summary>
        /// Returns the hero with <paramref name="spellPoints"/> spell points.
        /// </summary>
        public HeroModel WithSpellPoints(int spellPoints) => this with { SpellPoints = spellPoints };

        /// <summary>
        /// Returns the hero in the service of <paramref name="owner"/>. A hero that serves nobody has no move to finish, so its destination
        /// is cleared; the tile it stands on is left as the game leaves it (a defeated hero keeps the tile it fell on).
        /// </summary>
        public HeroModel WithOwner(PlayerColor owner) => owner.IsNeutral ? this with { Owner = owner, Destination = MapPosition.None } : this with { Owner = owner };

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
