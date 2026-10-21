namespace Siniskalc.Heroes.Wpf.ViewModels
{
    using System.Collections.Generic;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// The names of skill levels as the game shows them. The entity calls them beginner, intermediate and expert.
    /// </summary>
    public static class SkillLevelNames
    {
        public const string None = SkillLevelText.None;

        public const string Basic = SkillLevelText.Basic;

        public const string Advanced = SkillLevelText.Advanced;

        public const string Expert = SkillLevelText.Expert;

        /// <summary>
        /// Gets the names to pick a level from, for a skill the hero may not have.
        /// </summary>
        public static IReadOnlyList<string> WithNone => SkillLevelText.WithNone;

        /// <summary>
        /// Gets the names to pick a level from, for a skill the hero has.
        /// </summary>
        public static IReadOnlyList<string> WithoutNone => SkillLevelText.WithoutNone;

        public static string FromLevel(SkillLevel level) => level.Code switch
        {
            1 => Basic,
            2 => Advanced,
            3 => Expert,
            _ => None,
        };

        public static SkillLevel ToLevel(string name) => name switch
        {
            Basic => SkillLevel.Beginner,
            Advanced => SkillLevel.Intermediate,
            Expert => SkillLevel.Expert,
            _ => SkillLevel.None,
        };

        /// <summary>
        /// Returns 0 for none, and 1 to 3 for basic to expert.
        /// </summary>
        public static int ToNumber(string name) => ToLevel(name).Code;
    }
}
