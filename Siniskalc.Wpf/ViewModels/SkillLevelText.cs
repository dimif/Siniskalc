namespace Siniskalc.Wpf.ViewModels
{
    using System.Collections.Generic;

    /// <summary>
    /// The names of skill levels as the games show them: none, basic, advanced and expert, which are 0 to 3.
    /// </summary>
    public static class SkillLevelText
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

        /// <summary>
        /// Returns the name of a level number: "None" for 0, "Basic", "Advanced" and "Expert" for 1 to 3.
        /// </summary>
        public static string FromNumber(int level) => level switch
        {
            1 => Basic,
            2 => Advanced,
            3 => Expert,
            _ => None,
        };

        /// <summary>
        /// Returns 0 for none (or a name that is no level), and 1 to 3 for basic to expert.
        /// </summary>
        public static int ToNumber(string name) => name switch
        {
            Basic => 1,
            Advanced => 2,
            Expert => 3,
            _ => 0,
        };
    }
}
