namespace Siniskalc.Models
{
    using Siniskalc.Entities;

    /// <summary>
    /// Works out how many spell points a hero can have at most.
    /// </summary>
    public interface ISpellPointsCalculator
    {
        /// <summary>
        /// Returns the most spell points a hero has: ten per point of <paramref name="knowledge"/> (with what artifacts add), plus the
        /// bonus of Intelligence, whose level is <paramref name="intelligence"/> (null for a hero without it), in <paramref name="version"/>.
        /// </summary>
        int GetMaximum(GameVersion version, int knowledge, SkillLevel? intelligence);

        /// <summary>
        /// Returns the most spell points <paramref name="hero"/> can have in <paramref name="version"/>.
        /// </summary>
        int GetMaximum(GameVersion version, HeroModel hero);
    }
}
