namespace Siniskalc.Homm2.Models
{
    using System;

    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Entities;

    /// <summary>
    /// A hero of Heroes of Might and Magic II can have ten spell points for each point of knowledge, and nothing adds to it.
    /// </summary>
    public sealed class MaxSpellPointsCalculator : IMaxSpellPointsCalculator<SecondarySkill, SecondarySkills>
    {
        /// <inheritdoc/>
        public int GetMaximum(PrimarySkills primarySkills, SecondarySkills secondarySkills) => Math.Max(0, primarySkills.Knowledge) * Constants.Hero.SpellPointsPerKnowledge;
    }
}
