namespace Siniskalc.Heroes.Wpf.ViewModels
{
    using System.Collections.Generic;
    using Siniskalc.Heroes.Entities;

    /// <summary>
    /// The names of skill levels as the games show them: none, basic, advanced and expert, which are 0 to 3. The entity calls them
    /// beginner, intermediate and expert.
    /// </summary>
    public static class SkillLevelNames
    {
        /// <summary>
        /// The name of no level: a skill the hero has not learned.
        /// </summary>
        public const string None = "None";

        /// <summary>
        /// The name of level 1.
        /// </summary>
        public const string Basic = "Basic";

        /// <summary>
        /// The name of level 2.
        /// </summary>
        public const string Advanced = "Advanced";

        /// <summary>
        /// The name of level 3.
        /// </summary>
        public const string Expert = "Expert";

        /// <summary>
        /// Gets the names to pick a level from, for a skill the hero may not have.
        /// </summary>
        public static IReadOnlyList<string> WithNone { get; } = [None, Basic, Advanced, Expert];

        /// <summary>
        /// Gets the names to pick a level from, for a skill the hero has.
        /// </summary>
        public static IReadOnlyList<string> WithoutNone { get; } = [Basic, Advanced, Expert];

        /// <summary>
        /// Returns the name of a level: "None" for none, "Basic", "Advanced" and "Expert" for beginner to expert.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <returns>The name.</returns>
        public static string FromLevel(SkillLevel level) => level.Code switch
        {
            1 => Basic,
            2 => Advanced,
            3 => Expert,
            _ => None,
        };

        /// <summary>
        /// Returns the level a name stands for; none for "None" or a name that is no level.
        /// </summary>
        /// <param name="name">The name of the level.</param>
        /// <returns>The level.</returns>
        public static SkillLevel ToLevel(string name) => name switch
        {
            Basic => SkillLevel.Beginner,
            Advanced => SkillLevel.Intermediate,
            Expert => SkillLevel.Expert,
            _ => SkillLevel.None,
        };

        /// <summary>
        /// Returns 0 for none (or a name that is no level), and 1 to 3 for basic to expert.
        /// </summary>
        /// <param name="name">The name of the level.</param>
        /// <returns>The number of the level.</returns>
        public static int ToNumber(string name) => ToLevel(name).Code;
    }
}
