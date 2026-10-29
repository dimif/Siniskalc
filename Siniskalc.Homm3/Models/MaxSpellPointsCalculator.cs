namespace Siniskalc.Homm3.Models
{
    using System;

    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm3.Entities;

    /// <summary>
    /// In Homm 3, intelligence adds 25%, 50% or 100% to the spell points of a hero in the original games and 20%, 35% or 50% in Horn of the Abyss.
    /// </summary>
    public sealed class MaxSpellPointsCalculator : IMaxSpellPointsCalculator<SecondarySkill, SecondarySkills>
    {
        private readonly bool hota;

        public MaxSpellPointsCalculator(GameVersion version)
        {
            this.hota = version == GameVersion.HornOfTheAbyss;
        }

        /// <inheritdoc/>
        public int GetMaximum(PrimarySkills primarySkills, SecondarySkills secondarySkills)
        {
            var percent = secondarySkills.GetSkillLevel(SecondarySkill.Intelligence).Code switch
            {
                1 => this.hota ? 20 : 25,
                2 => this.hota ? 35 : 50,
                3 => this.hota ? 50 : 100,
                _ => 0,
            };

            return Math.Max(0, primarySkills.Knowledge) * Constants.Hero.SpellPointsPerKnowledge * (100 + percent) / 100;
        }
    }
}
