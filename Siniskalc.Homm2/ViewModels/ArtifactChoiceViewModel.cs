namespace Siniskalc.Homm2.ViewModels
{
    using Siniskalc.Heroes.ViewModels;
    using Siniskalc.Homm2.Entities;

    /// <summary>
    /// One artifact in the artifact finder list, with what it adds to the skills of the hero that carries it.
    /// </summary>
    public sealed class ArtifactChoiceViewModel
    {
        public ArtifactChoiceViewModel(Artifact artifact)
        {
            this.Artifact = artifact;
            this.Bonuses = PrimarySkillBonusText.Describe(artifact.PrimarySkillBonus);
        }

        public Artifact Artifact { get; }

        public string Name => this.Artifact.Name;

        /// <summary>
        /// Gets what the artifact adds to the primary skills, such as "Attack +6, Defense +6", or nothing.
        /// </summary>
        public string Bonuses { get; }
    }
}
