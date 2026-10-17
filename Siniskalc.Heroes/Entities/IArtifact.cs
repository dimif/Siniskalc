namespace Siniskalc.Heroes.Entities
{
    using Siniskalc.Core.Entities;
    using Siniskalc.Heroes.Models;

    /// <summary>
    /// An artifact of a game, as far as the pages and the hero's skills need it: a name, the code the savefile holds, and what it
    /// adds to the primary skills of the hero that wears it. Where it can be worn, what it costs and what else it does are the game's.
    /// </summary>
    public interface IArtifact : IGameEntity
    {
        /// <summary>
        /// Gets what the artifact adds to the primary skills, or <see cref="PrimarySkillBonus.None"/>.
        /// </summary>
        PrimarySkillBonus PrimarySkillBonus { get; }
    }
}
