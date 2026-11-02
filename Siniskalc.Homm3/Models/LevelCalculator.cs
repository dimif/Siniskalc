namespace Siniskalc.Homm3.Models
{
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Heroes.Models;

    /// <summary>
    /// Allows to calculate the level of a hero based on their experience points and vice versa.
    /// Source for the experience thresholds is https://heroes.thelazy.net/index.php/Experience#Experience_required_for_each_level.
    /// </summary>
    public static class LevelCalculator
    {
        /// <summary>
        /// The experience a hero needs to reach each level, by level.
        /// </summary>
        public static readonly IReadOnlyDictionary<short, int> LevelToExperienceThresholds = new Dictionary<short, int>
        {
            { 1, 0 },
            { 2, 1000 },
            { 3, 2000 },
            { 4, 3200 },
            { 5, 4600 },
            { 6, 6200 },
            { 7, 8000 },
            { 8, 10000 },
            { 9, 12200 },
            { 10, 14700 },
            { 11, 17500 },
            { 12, 20600 },
            { 13, 24320 },
            { 14, 28784 },
            { 15, 34140 },
            { 16, 40567 },
            { 17, 48279 },
            { 18, 57533 },
            { 19, 68637 },
            { 20, 81961 },
            { 21, 97949 },
            { 22, 117134 },
            { 23, 140156 },
            { 24, 167782 },
            { 25, 200933 },
            { 26, 240714 },
            { 27, 288451 },
            { 28, 345735 },
            { 29, 414475 },
            { 30, 496963 },
            { 31, 595948 },
            { 32, 714730 },
            { 33, 857268 },
            { 34, 1028313 },
            { 35, 1233567 },
            { 36, 1479871 },
            { 37, 1775435 },
            { 38, 2130111 },
            { 39, 2555722 },
            { 40, 3066455 },
            { 41, 3679334 },
            { 42, 4414788 },
            { 43, 5297332 },
            { 44, 6356384 },
            { 45, 7627246 },
            { 46, 9152280 },
            { 47, 10982320 },
            { 48, 13178368 },
            { 49, 15813625 },
            { 50, 18975933 },
            { 51, 22770702 },
            { 52, 27324424 },
            { 53, 32788890 },
            { 54, 39346249 },
            { 55, 47215079 },
            { 56, 56657675 },
            { 57, 67988790 },
            { 58, 81586128 },
            { 59, 97902933 },
            { 60, 117483099 },
            { 61, 140979298 },
            { 62, 169174736 },
            { 63, 203009261 },
            { 64, 243610691 },
            { 65, 292332407 },
            { 66, 350798466 },
            { 67, 420957736 },
            { 68, 505148860 },
            { 69, 606178208 },
            { 70, 727413425 },
            { 71, 872895685 },
            { 72, 1047474397 },
            { 73, 1256968851 },
            { 74, 1508362195 },
            { 75, 1810034207 },
        };

        private static readonly ExperienceTable Levels = new([.. LevelToExperienceThresholds.OrderBy(t => t.Key).Select(t => (long)t.Value)], LevelToExperienceThresholds.Count);

        /// <summary>
        /// Gets the experience each level needs.
        /// </summary>
        public static ExperienceTable Table => Levels;

        /// <summary>
        /// Gets the level above the one <paramref name="experience"/> reaches, and the experience it needs.
        /// Null when the hero is already at the highest level.
        /// </summary>
        public static (short Level, int Experience)? GetNextLevel(int experience) =>
            Levels.NextLevel(experience) is { } next ? ((short)next.Level, (int)next.Experience) : null;
    }
}
