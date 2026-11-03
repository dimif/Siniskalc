namespace Siniskalc.Heroes.Wpf.ViewModels
{
    using System;
    using System.Collections.Generic;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// The card of the four primary skills of a hero, in any game: for each, the base the hero learned, the total with its artifacts and what
    /// the artifacts add (<see cref="PrimarySkills"/>). An edit sets the base and keeps what the artifacts add on top of it, so the savegame
    /// gets the total, as the game keeps it. Call <see cref="Refresh"/> after the hero changed.
    /// </summary>
    public sealed class PrimarySkillsCardViewModel : AbstractViewModelBase
    {
        /// <param name="skills">Reads the primary skills of the hero.</param>
        /// <param name="update">Writes an edit of the primary skills of the hero.</param>
        /// <param name="range">The limits of a base value.</param>
        public PrimarySkillsCardViewModel(Func<PrimarySkills> skills, Action<Func<PrimarySkills, PrimarySkills>> update, NumberRange range)
        {
            this.Attack = new("Attack", range, () => skills().BaseAttack, value => update(s => s.WithBaseAttack(value)), () => skills().Attack, () => skills().Bonus.Attack);
            this.Defense = new("Defense", range, () => skills().BaseDefense, value => update(s => s.WithBaseDefense(value)), () => skills().Defense, () => skills().Bonus.Defense);
            this.SpellPower = new("Spell power", range, () => skills().BaseSpellPower, value => update(s => s.WithBaseSpellPower(value)), () => skills().SpellPower, () => skills().Bonus.SpellPower);
            this.Knowledge = new("Knowledge", range, () => skills().BaseKnowledge, value => update(s => s.WithBaseKnowledge(value)), () => skills().Knowledge, () => skills().Bonus.Knowledge);
            this.Rows = [this.Attack, this.Defense, this.SpellPower, this.Knowledge];
        }

        /// <summary>
        /// Gets the attack skill.
        /// </summary>
        public PrimarySkillRowViewModel Attack { get; }

        /// <summary>
        /// Gets the defense skill.
        /// </summary>
        public PrimarySkillRowViewModel Defense { get; }

        /// <summary>
        /// Gets the spell power.
        /// </summary>
        public PrimarySkillRowViewModel SpellPower { get; }

        /// <summary>
        /// Gets the knowledge.
        /// </summary>
        public PrimarySkillRowViewModel Knowledge { get; }

        /// <summary>
        /// Gets the four skills in the order the game shows them.
        /// </summary>
        public IReadOnlyList<PrimarySkillRowViewModel> Rows { get; }

        /// <summary>
        /// Shows the hero again. Call after the hero changed.
        /// </summary>
        public void Refresh()
        {
            foreach (var row in this.Rows)
            {
                row.Refresh();
            }
        }
    }
}
