namespace Siniskalc.Homm3.Models
{
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm3.Entities;

    /// <summary>
    /// Makes the calculator of the spell points of a savefile, which depends on the version of the game it is from.
    /// </summary>
    public interface ISpellPointsCalculatorFactory
    {
        /// <summary>
        /// Returns the calculator for a savefile of <paramref name="version"/>.
        /// </summary>
        IMaxSpellPointsCalculator<SecondarySkill, SecondarySkills> Create(GameVersion version);
    }
}
