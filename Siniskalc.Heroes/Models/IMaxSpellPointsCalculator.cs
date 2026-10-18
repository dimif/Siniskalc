namespace Siniskalc.Heroes.Models
{
    using Siniskalc.Heroes.Entities;

    /// <summary>
    /// Calculates the max number of spell points for a hero.
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
        /// Gets the maximum spell points for the given hero.
        /// </summary>
        /// <param name="hero">
        /// The hero.
        /// </param>
        /// <returns>
        /// The max spellpoints.
        /// </returns>
        int GetMaximum(IHeroModel<TSecondarySkill, TSecondarySkills> hero);
    }
}
