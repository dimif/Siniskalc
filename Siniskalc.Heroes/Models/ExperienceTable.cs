namespace Siniskalc.Heroes.Models
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The experience a hero needs for each level. The thresholds are the game's own and differ between the games; what is shared is how a
    /// level is worked out from experience and the other way round. A table can list the thresholds of the first levels only and
    /// carry on with a rule for the next one.
    /// </summary>
    public sealed class ExperienceTable
    {
        private readonly long[] thresholds;
        private readonly Func<long, long, long>? extension;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExperienceTable"/> class.
        /// </summary>
        /// <param name="thresholds">The experience at which each level starts, the first being level 1 (so 0).</param>
        /// <param name="maxLevel">The highest level. Without an <paramref name="extension"/> it is the number of thresholds.</param>
        /// <param name="extension">
        /// For the levels after the last threshold: the experience of the next level from the experience of the last one and of the one
        /// before it.
        /// </param>
        public ExperienceTable(IReadOnlyList<long> thresholds, int maxLevel, Func<long, long, long>? extension = null)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(thresholds.Count, 2, nameof(thresholds));
            if (extension == null && maxLevel != thresholds.Count)
            {
                throw new ArgumentException("A table that has no rule for the next level ends with its last threshold.", nameof(maxLevel));
            }

            this.thresholds = [.. thresholds];
            this.extension = extension;
            this.MaxLevel = maxLevel;
        }

        /// <summary>
        /// Gets the highest level.
        /// </summary>
        public int MaxLevel { get; }

        /// <summary>
        /// Gets the experience with which a hero starts the given level. A level below 1 reads as 1 and one above the highest as the highest.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <returns>The experience points.</returns>
        public long ExperienceOf(int level)
        {
            var index = Math.Clamp(level, 1, this.MaxLevel) - 1;
            if (index < this.thresholds.Length)
            {
                return this.thresholds[index];
            }

            var previous = this.thresholds[^1];
            var beforeThat = this.thresholds[^2];
            for (var i = this.thresholds.Length; i <= index; i++)
            {
                var next = this.extension!(previous, beforeThat);
                beforeThat = previous;

                // The rule grows without bound; a level nobody can reach stays at the largest number there is.
                previous = next < previous ? long.MaxValue : next;
            }

            return previous;
        }

        /// <summary>
        /// Gets the level a hero with the given experience has.
        /// </summary>
        /// <param name="experience">The experience points.</param>
        /// <returns>The level.</returns>
        public int LevelOf(long experience)
        {
            var level = 1;
            while (level < this.MaxLevel && experience >= this.ExperienceOf(level + 1))
            {
                level++;
            }

            return level;
        }

        /// <summary>
        /// Gets the level above the one <paramref name="experience"/> reaches, and the experience it needs, or <c>null</c> at the highest level.
        /// </summary>
        public (int Level, long Experience)? NextLevel(long experience)
        {
            var level = this.LevelOf(experience);
            return level >= this.MaxLevel ? null : (level + 1, this.ExperienceOf(level + 1));
        }
    }
}
