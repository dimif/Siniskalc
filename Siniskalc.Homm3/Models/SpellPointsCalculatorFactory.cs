namespace Siniskalc.Homm3.Models
{
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm3.Entities;

    /// <inheritdoc/>
    public sealed class SpellPointsCalculatorFactory : ISpellPointsCalculatorFactory
    {
        /// <inheritdoc/>
        public IMaxSpellPointsCalculator<SecondarySkill, SecondarySkills> Create(GameVersion version) => new MaxSpellPointsCalculator(version);
    }
}
