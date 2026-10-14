namespace Siniskalc.Homm2.Entities
{
    using System.Collections.Generic;
    using Siniskalc.Heroes.Entities;

    /// <summary>
    /// The levels of a secondary skill of Heroes of Might and Magic II, as <see cref="ISkillLevel"/>. A skill with no level has none.
    /// </summary>
    public static class SkillLevels
    {
        private static readonly SkillLevelInfo BasicLevel = new("Basic", (byte)SkillLevel.Basic, 1);
        private static readonly SkillLevelInfo AdvancedLevel = new("Advanced", (byte)SkillLevel.Advanced, 2);
        private static readonly SkillLevelInfo ExpertLevel = new("Expert", (byte)SkillLevel.Expert, 3);

        public static IReadOnlyList<ISkillLevel> All { get; } = [BasicLevel, AdvancedLevel, ExpertLevel];

        /// <summary>
        /// Returns <paramref name="level"/> as an <see cref="ISkillLevel"/>, or <c>null</c> for <see cref="SkillLevel.None"/>.
        /// </summary>
        public static ISkillLevel? Of(SkillLevel level) => level switch
        {
            SkillLevel.Basic => BasicLevel,
            SkillLevel.Advanced => AdvancedLevel,
            SkillLevel.Expert => ExpertLevel,
            _ => null,
        };
    }
}
