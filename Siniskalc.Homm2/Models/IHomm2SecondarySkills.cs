namespace Siniskalc.Homm2.Models
{
    using System.Collections.Generic;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Entities;

    /// <summary>
    /// HoMM2-specific secondary skills collection.
    /// </summary>
    public interface IHomm2SecondarySkills : ISecondarySkills<SecondarySkill, IHomm2SecondarySkills>
    {
        /// <summary>
        /// Gets an ordered list of the skills of the hero.
        /// </summary>
        /// <remarks>
        /// The order represents the order in which the hero has learned them.
        /// </remarks>
        IReadOnlyList<SkillEntry> Entries { get; }
    }
}
