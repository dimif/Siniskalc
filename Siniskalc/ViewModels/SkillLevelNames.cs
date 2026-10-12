namespace Siniskalc.ViewModels
{
    using System.Collections.Generic;
    using Siniskalc.Entities;

    /// <summary>
    /// The names of skill levels as the game shows them. The entity calls them beginner, intermediate and expert.
    /// </summary>
    public static class SkillLevelNames
    {
        public const string None = "None";

        public const string Basic = "Basic";

        public const string Advanced = "Advanced";

        public const string Expert = "Expert";

        /// <summary>
        /// Gets the names to pick a level from, for a skill the hero may not have.
        /// </summary>
        public static IReadOnlyList<string> WithNone { get; } = [None, Basic, Advanced, Expert];

        /// <summary>
        /// Gets the names to pick a level from, for a skill the hero has.
        /// </summary>
        public static IReadOnlyList<string> WithoutNone { get; } = [Basic, Advanced, Expert];

        public static string FromLevel(SkillLevel? level) => level switch
        {
            SkillLevel.Beginner => Basic,
            SkillLevel.Intermediate => Advanced,
            SkillLevel.Expert => Expert,
            _ => None,
        };

        public static SkillLevel? ToLevel(string name) => name switch
        {
            Basic => SkillLevel.Beginner,
            Advanced => SkillLevel.Intermediate,
            Expert => SkillLevel.Expert,
            _ => null,
        };

        /// <summary>
        /// Returns 0 for none, and 1 to 3 for basic to expert.
        /// </summary>
        public static int ToNumber(string name) => (int?)ToLevel(name) ?? 0;
    }
}
