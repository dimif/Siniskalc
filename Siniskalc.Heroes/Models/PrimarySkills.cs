namespace Siniskalc.Heroes.Models
{
    using System;
    using Siniskalc.Core;

    /// <summary>
    /// The four primary skills of a hero. <see cref="Attack"/>, <see cref="Defense"/>, <see cref="SpellPower"/> and
    /// <see cref="Knowledge"/> are the values the savefile holds, which are what counts in the game: the hero's own skill with what the
    /// worn artifacts add already in it (a hero with Armageddon's Blade is saved with 3 more attack, in both games).
    /// <see cref="Bonus"/> is that part, kept up to date by the hero, and the <c>Base</c> properties are what is left without it, which
    /// is what the hero learned and what a level-up raises.
    /// </summary>
    public sealed record PrimarySkills(int Attack, int Defense, int SpellPower, int Knowledge)
    {
        /// <summary>
        /// Gets what the worn artifacts add to the saved values.
        /// </summary>
        public PrimarySkillBonus Bonus { get; init; }

        /// <summary>
        /// Gets the attack the hero learned, without what the artifacts add.
        /// </summary>
        public int BaseAttack => Base(this.Attack, this.Bonus.Attack);

        /// <summary>
        /// Gets the defense the hero learned, without what the artifacts add.
        /// </summary>
        public int BaseDefense => Base(this.Defense, this.Bonus.Defense);

        /// <summary>
        /// Gets the spell power the hero learned, without what the artifacts add.
        /// </summary>
        public int BaseSpellPower => Base(this.SpellPower, this.Bonus.SpellPower);

        /// <summary>
        /// Gets the knowledge the hero learned, without what the artifacts add.
        /// </summary>
        public int BaseKnowledge => Base(this.Knowledge, this.Bonus.Knowledge);

        /// <summary>
        /// Returns the skills as the savefile has them, from the base values the hero learned and the bonus the artifacts add.
        /// </summary>
        /// <param name="attack">The base attack.</param>
        /// <param name="defense">The base defense.</param>
        /// <param name="spellPower">The base spell power.</param>
        /// <param name="knowledge">The base knowledge.</param>
        /// <param name="bonus">What the artifacts add.</param>
        /// <returns>The skills.</returns>
        public static PrimarySkills FromBase(int attack, int defense, int spellPower, int knowledge, PrimarySkillBonus bonus) => new(
            Saved(attack, bonus.Attack),
            Saved(defense, bonus.Defense),
            Saved(spellPower, bonus.SpellPower),
            Saved(knowledge, bonus.Knowledge))
        {
            Bonus = bonus,
        };

        /// <summary>
        /// Two skill sets are equal when the values the savefile holds are: the bonus comes from the artifacts and is worked out again
        /// whenever they change, so it is not part of what a hero's skills are.
        /// </summary>
        /// <param name="other">The skills to compare with.</param>
        /// <returns><see langword="true"/> if the four saved values are equal.</returns>
        public bool Equals(PrimarySkills? other) =>
            other is not null && this.Attack == other.Attack && this.Defense == other.Defense && this.SpellPower == other.SpellPower && this.Knowledge == other.Knowledge;

        /// <summary>
        /// Returns a hash code made from the four saved values, which is what <see cref="Equals(PrimarySkills)"/> compares.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode() => HashCode.Combine(this.Attack, this.Defense, this.SpellPower, this.Knowledge);

        /// <summary>
        /// Returns the skills with the base attack set to <paramref name="value"/>: what the artifacts add stays on top of it.
        /// </summary>
        /// <param name="value">The base attack.</param>
        /// <returns>The new skills; these are not changed.</returns>
        public PrimarySkills WithBaseAttack(int value) => this with { Attack = Saved(value, this.Bonus.Attack) };

        /// <summary>
        /// Returns the skills with the base defense set to <paramref name="value"/>: what the artifacts add stays on top of it.
        /// </summary>
        /// <param name="value">The base defense.</param>
        /// <returns>The new skills; these are not changed.</returns>
        public PrimarySkills WithBaseDefense(int value) => this with { Defense = Saved(value, this.Bonus.Defense) };

        /// <summary>
        /// Returns the skills with the base spell power set to <paramref name="value"/>: what the artifacts add stays on top of it.
        /// </summary>
        /// <param name="value">The base spell power.</param>
        /// <returns>The new skills; these are not changed.</returns>
        public PrimarySkills WithBaseSpellPower(int value) => this with { SpellPower = Saved(value, this.Bonus.SpellPower) };

        /// <summary>
        /// Returns the skills with the base knowledge set to <paramref name="value"/>: what the artifacts add stays on top of it.
        /// </summary>
        /// <param name="value">The base knowledge.</param>
        /// <returns>The new skills; these are not changed.</returns>
        public PrimarySkills WithBaseKnowledge(int value) => this with { Knowledge = Saved(value, this.Bonus.Knowledge) };

        /// <summary>
        /// Returns the skills as they are when the artifacts add <paramref name="to"/> where they added <see cref="Bonus"/>: what the game
        /// does when a hero puts an artifact on or takes it off. A skill stays within what the savefile can hold.
        /// </summary>
        /// <param name="to">What the artifacts add now.</param>
        /// <returns>The new skills; these are not changed.</returns>
        public PrimarySkills WithBonus(PrimarySkillBonus to) => new(
            Saved(this.Attack, to.Attack - this.Bonus.Attack),
            Saved(this.Defense, to.Defense - this.Bonus.Defense),
            Saved(this.SpellPower, to.SpellPower - this.Bonus.SpellPower),
            Saved(this.Knowledge, to.Knowledge - this.Bonus.Knowledge))
        {
            Bonus = to,
        };

        /// <summary>
        /// Returns the skills with <paramref name="delta"/> added to the saved values and to <see cref="Bonus"/>: the artifacts that were
        /// put on add their bonus, the ones taken off add less than nothing. This is for a hero whose bonus is worked out artifact by
        /// artifact; a skill stays within what the savefile can hold.
        /// </summary>
        /// <param name="delta">What to add to the saved values and to the bonus.</param>
        /// <returns>The new skills; these are not changed.</returns>
        public PrimarySkills Shifted(PrimarySkillBonus delta) => new(
            Saved(this.Attack, delta.Attack),
            Saved(this.Defense, delta.Defense),
            Saved(this.SpellPower, delta.SpellPower),
            Saved(this.Knowledge, delta.Knowledge))
        {
            Bonus = this.Bonus + delta,
        };

        private static int Base(int saved, int bonus) => Math.Max(0, saved - bonus);

        private static int Saved(int value, int add) => (int)RangeGuard.Clamp<long>(value + add, Constants.Hero.MinSavedPrimarySkill, Constants.Hero.MaxSavedPrimarySkill);
    }
}
