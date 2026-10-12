namespace Siniskalc.Models
{
    using System;
    using Siniskalc.Entities;

    /// <summary>
    /// Intelligence adds 25%, 50% or 100% to the spell points of a hero in the original games and 20%, 35% or 50% in Horn of the Abyss.
    /// </summary>
    public sealed class SpellPointsCalculator : ISpellPointsCalculator
    {
        private const int SpellPointsPerKnowledge = 10;

        /// <inheritdoc/>
        public int GetMaximum(GameVersion version, int knowledge, SkillLevel? intelligence)
        {
            var hota = version == GameVersion.HornOfTheAbyss;
            var percent = intelligence switch
            {
                SkillLevel.Beginner => hota ? 20 : 25,
                SkillLevel.Intermediate => hota ? 35 : 50,
                SkillLevel.Expert => hota ? 50 : 100,
                _ => 0,
            };

            return Math.Max(0, knowledge) * SpellPointsPerKnowledge * (100 + percent) / 100;
        }

        /// <inheritdoc/>
        public int GetMaximum(GameVersion version, HeroModel hero)
        {
            return this.GetMaximum(version, hero.PrimarySkills.TotalKno, hero.SecondarySkills.GetSkillLevel(SecondarySkill.Intelligence));
        }
    }
}
