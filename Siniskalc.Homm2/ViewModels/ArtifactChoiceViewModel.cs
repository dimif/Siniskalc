namespace Siniskalc.Homm2.ViewModels
{
    using Siniskalc.Core;
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
        /// Gets the byte that stands for the artifact in a savefile.
        /// </summary>
        public byte CodeValue => this.Artifact.Code;

        /// <summary>
        /// Gets the byte code in hex, such as "0x2A".
        /// </summary>
        public string Code => ByteCodeText.Format(this.Artifact.Code);

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
