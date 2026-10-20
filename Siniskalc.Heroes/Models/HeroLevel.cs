namespace Siniskalc.Heroes.Models
{
    using System;

    /// <summary>
    /// The level of a hero and the experience it has: the two numbers the savefile holds and keeps in step.
    /// </summary>
    /// <param name="Experience">The experience points.</param>
    /// <param name="Level">The level, from 1.</param>
    public readonly record struct HeroLevel(long Experience, int Level)
    {
        /// <summary>
        /// Returns the level a hero has when it has just reached <paramref name="level"/>: that level and the experience the game's
        /// <paramref name="table"/> starts it at. A level the table does not have reads as the nearest one it has.
        /// </summary>
        /// <param name="table">The experience each level of the game needs.</param>
        /// <param name="level">The level.</param>
        public static HeroLevel AtLevel(ExperienceTable table, int level)
        {
            var reached = Math.Clamp(level, 1, table.MaxLevel);
            return new HeroLevel(table.ExperienceOf(reached), reached);
        }

        /// <summary>
        /// Returns the level of a hero that has <paramref name="experience"/> points: the experience and the level the game's
        /// <paramref name="table"/> gives it.
        /// </summary>
        /// <param name="table">The experience each level of the game needs.</param>
        /// <param name="experience">The experience points.</param>
        public static HeroLevel AtExperience(ExperienceTable table, long experience) => new(experience, table.LevelOf(experience));
    }
}
