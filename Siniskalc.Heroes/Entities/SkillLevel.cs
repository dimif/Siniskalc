namespace Siniskalc.Heroes.Entities
{
    using Siniskalc.Core.Entities;

    /// <summary>
    /// How well a hero has a secondary skill. The savegames of every game hold it as a number from 1 (beginner) to 3 (expert),
    /// and 0 for a skill the hero does not have.
    /// </summary>
    public sealed class SkillLevel : AbstractEnumeration
    {
        /// <summary>
        /// The hero has no level in the skill.
        /// </summary>
        public static readonly SkillLevel None = new("None", 0);

        /// <summary>
        /// The first level of a skill.
        /// </summary>
        public static readonly SkillLevel Beginner = new("Beginner", 1);

        /// <summary>
        /// The second level of a skill.
        /// </summary>
        public static readonly SkillLevel Intermediate = new("Intermediate", 2);

        /// <summary>
        /// The third and highest level of a skill.
        /// </summary>
        public static readonly SkillLevel Expert = new("Expert", 3);

        private SkillLevel(string name, byte code)
            : base(name, code)
        {
        }

        /// <summary>
        /// Returns the level a byte of a savefile stands for: beginner, intermediate or expert for 1 to 3, and <see cref="None"/>
        /// for 0 and for any byte that is no level.
        /// </summary>
        /// <param name="code">The byte.</param>
        /// <returns>The level.</returns>
        public static SkillLevel Of(byte code) => code switch
        {
            1 => Beginner,
            2 => Intermediate,
            3 => Expert,
            _ => None,
        };
    }
}
