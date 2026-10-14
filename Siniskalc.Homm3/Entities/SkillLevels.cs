namespace Siniskalc.Homm3.Entities
{
    using System.Collections.Generic;
    using Siniskalc.Heroes.Entities;

    /// <summary>
    /// The levels of a secondary skill of Heroes of Might and Magic III, as <see cref="ISkillLevel"/>. A hero that does not have the
    /// skill has no level.
    /// </summary>
    public static class SkillLevels
    {
        private static readonly SkillLevelInfo BasicLevel = new("Basic", (byte)SkillLevel.Beginner, 1);
        private static readonly SkillLevelInfo AdvancedLevel = new("Advanced", (byte)SkillLevel.Intermediate, 2);
        private static readonly SkillLevelInfo ExpertLevel = new("Expert", (byte)SkillLevel.Expert, 3);

        public static IReadOnlyList<ISkillLevel> All { get; } = [BasicLevel, AdvancedLevel, ExpertLevel];

        /// <summary>
        /// Returns <paramref name="level"/> as an <see cref="ISkillLevel"/>, or <c>null</c> when there is none.
        /// </summary>
        public static ISkillLevel? Of(SkillLevel? level) => level switch
        {
            SkillLevel.Beginner => BasicLevel,
            SkillLevel.Intermediate => AdvancedLevel,
            SkillLevel.Expert => ExpertLevel,
            _ => null,
        };
    }
}
