namespace Siniskalc.Heroes.Models
{
    using Siniskalc.Heroes.Entities;

    /// <summary>
    /// Calculates the max number of spell points for a hero from the skills that decide it.
    /// </summary>
    /// <typeparam name="TSecondarySkill">
    /// The type of secondary skill - game specific.
    /// </typeparam>
    /// <typeparam name="TSecondarySkills">
    /// The type of secondary skills collection - game specific.
    /// </typeparam>
    public interface IMaxSpellPointsCalculator<TSecondarySkill, TSecondarySkills>
        where TSecondarySkill : ISecondarySkill
        where TSecondarySkills : ISecondarySkills<TSecondarySkill, TSecondarySkills>
    {
        /// <summary>
        /// Gets the maximum spell points of a hero with the given skills.
        /// </summary>
        /// <param name="primarySkills">The primary skills of the hero.</param>
        /// <param name="secondarySkills">The secondary skills of the hero.</param>
        /// <returns>
        /// The max spellpoints.
        /// </returns>
        int GetMaximum(PrimarySkills primarySkills, TSecondarySkills secondarySkills);
    }
}
