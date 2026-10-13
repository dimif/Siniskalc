namespace Siniskalc.Homm2.ViewModels
{
    using System.Collections.Generic;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// One artifact in the artifact finder list, with what it adds to the skills of the hero that carries it.
    /// </summary>
    public sealed class Homm2ArtifactChoiceViewModel
    {
        public Homm2ArtifactChoiceViewModel(Homm2Artifact artifact)
        {
            this.Artifact = artifact;
            this.Bonuses = Describe(Homm2ArtifactBonuses.Of(artifact));
        }

        public Homm2Artifact Artifact { get; }

        public string Name => this.Artifact.Name;

        /// <summary>
        /// Gets what the artifact adds to the primary skills, such as "+6 attack, +6 defense", or nothing.
        /// </summary>
        public string Bonuses { get; }

        /// <summary>
        /// Gives what a set of bonuses adds, such as "+12 attack" or "+4 attack, +4 defense, +4 spell power, +4 knowledge".
        /// </summary>
        public static string Describe(Homm2PrimarySkills bonus)
        {
            var parts = new List<string>();
            Add(parts, bonus.Attack, "attack");
            Add(parts, bonus.Defense, "defense");
            Add(parts, bonus.SpellPower, "spell power");
            Add(parts, bonus.Knowledge, "knowledge");
            return string.Join(", ", parts);
        }

        private static void Add(List<string> parts, int amount, string skill)
        {
            if (amount != 0)
            {
                parts.Add($"{(amount > 0 ? "+" : string.Empty)}{amount} {skill}");
            }
        }
    }
}
