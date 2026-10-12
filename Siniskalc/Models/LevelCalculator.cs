namespace Siniskalc.Models
{
    using System.Collections.Generic;
    using Siniskalc.Core;

    /// <summary>
    /// Allows to calculate the level of a hero based on their experience points and vice versa.
    /// Source for the experience thresholds is https://heroes.thelazy.net/index.php/Experience#Experience_required_for_each_level.
    /// </summary>
    public static class LevelCalculator
    {
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

        public static short GetLevelFromXp(int experience)
        {
            short level = 1;
            foreach (var kvp in LevelToExperienceThresholds)
            {
                if (experience >= kvp.Value)
                {
                    level = kvp.Key;
                }
                else
                {
                    break;
                }
            }

            return level;
        }

        /// <summary>
        /// Gets the level above the one <paramref name="experience"/> reaches, and the experience it needs.
        /// Null when the hero is already at the highest level.
        /// </summary>
        public static (short Level, int Experience)? GetNextLevel(int experience)
        {
            foreach (var kvp in LevelToExperienceThresholds)
            {
                if (kvp.Value > experience)
                {
                    return (kvp.Key, kvp.Value);
                }
            }

            return null;
        }

        public static int GetXpForLevel(short level)
        {
            RangeGuard.ThrowIfOutOfRange<long>(level, Constants.Hero.MinLevel, Constants.Hero.MaxLevel, nameof(level), $"Invalid level: {level}. Level must be between {Constants.Hero.MinLevel} and {Constants.Hero.MaxLevel}.");
            return LevelToExperienceThresholds[level];
        }
    }
}
