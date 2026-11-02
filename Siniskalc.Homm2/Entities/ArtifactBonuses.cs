namespace Siniskalc.Homm2.Entities
{
    using Siniskalc.Heroes.Models;

    public static class ArtifactBonuses
    {
        /// <summary>
        /// Gets what an artifact adds to the four primary skills.
        /// </summary>
        /// <param name="artifact">The artifact.</param>
        /// <returns>The bonus.</returns>
        public static PrimarySkillBonus Of(Artifact artifact) => artifact.PrimarySkillBonus;
    }
}
