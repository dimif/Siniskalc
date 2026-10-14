namespace Siniskalc.Homm2.Models
{
    using System;

    /// <summary>
    /// The experience a hero needs for a level. The game works the level out from the experience (write test: level byte 1 with 50,000
    /// experience showed level 17, level byte 20 with none showed level 1). The table is the one of fheroes2; the original game's
    /// thresholds differ by under half a percent (it showed 54,244 and 231,549 where this gives 54,200 and 232,700).
    /// </summary>
    public static class HeroLevels
    {
        private static readonly long[] Table =
        [
            0, 1000, 2000, 3200, 4500, 6000, 7700, 9000, 11000, 13200, 15500, 18500, 22100, 26400, 31600, 37800, 45300, 54200, 65000, 78000,
            93600, 112300, 134700, 161600, 193900, 232700, 279300, 335200, 402300, 482800, 579400, 695300, 834400, 1001300, 1201600,
            1442000, 1730500, 2076700, 2492100, 2990600,
        ];

        /// <summary>
        /// Gets the experience with which a hero starts the given level.
        /// </summary>
        /// <param name="level">The level, 1 or more.</param>
        /// <returns>The experience.</returns>
        public static long ExperienceOf(int level)
        {
            var index = Math.Max(level, 1) - 1;
            if (index < Table.Length)
            {
                return Table[index];
            }

            long previous = Table[^1];
            long beforeThat = Table[^2];
            for (var i = Table.Length; i <= index; i++)
            {
                var next = previous + ((long)Math.Round((previous - beforeThat) * 1.2 / 100) * 100);
                beforeThat = previous;
                previous = next;
            }

            return previous;
        }

        /// <summary>
        /// Gets the level a hero with the given experience has.
        /// </summary>
        /// <param name="experience">The experience.</param>
        /// <returns>The level.</returns>
        public static int LevelOf(long experience)
        {
            var level = 1;
            while (level < Constants.Hero.MaxLevel && experience >= ExperienceOf(level + 1))
            {
                level++;
            }

            return level;
        }
    }
}
