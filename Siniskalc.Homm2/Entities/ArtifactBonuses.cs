namespace Siniskalc.Homm2.Entities
{
    using Siniskalc.Heroes.Models;

    public static class ArtifactBonuses
    {
        public static PrimarySkillBonus Of(Artifact artifact) => artifact.PrimarySkillBonus;
    }
}
