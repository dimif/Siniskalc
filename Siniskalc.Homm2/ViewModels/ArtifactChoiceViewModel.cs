namespace Siniskalc.Homm2.ViewModels
{
    using Siniskalc.Heroes.ViewModels;
    using Siniskalc.Homm2.Entities;

    /// <summary>
    /// One artifact in the artifact finder list, with what it adds to the skills of the hero that carries it.
    /// </summary>
    public sealed class ArtifactChoiceViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ArtifactChoiceViewModel"/> class.
        /// </summary>
        /// <param name="artifact">The artifact.</param>
        public ArtifactChoiceViewModel(Artifact artifact)
        {
            this.Artifact = artifact;
            this.Bonuses = PrimarySkillBonusText.Describe(artifact.PrimarySkillBonus);
        }

        /// <summary>
        /// Gets the artifact the choice stands for.
        /// </summary>
        public Artifact Artifact { get; }

        /// <summary>
        /// Gets the name of the artifact.
        /// </summary>
        public string Name => this.Artifact.Name;

        /// <summary>
        /// Gets what the artifact adds to the primary skills, such as "Attack +6, Defense +6", or nothing.
        /// </summary>
        public string Bonuses { get; }
    }
}
