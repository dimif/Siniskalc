namespace Siniskalc.ViewModels
{
    using System.Globalization;
    using System.Linq;
    using Siniskalc.Entities;

    /// <summary>
    /// One artifact in an <see cref="ArtifactFinderViewModel"/> list.
    /// </summary>
    public sealed class ArtifactChoiceViewModel
    {
        /// <param name="effectLines">Words what the artifact does.</param>
        /// <param name="spell">For a Spell Scroll row, the spell the scroll holds.</param>
        public ArtifactChoiceViewModel(Artifact artifact, EffectLineProvider effectLines, Spell? spell = null)
        {
            this.Artifact = artifact;
            this.Spell = spell;
            this.SlotName = SlotNameOf(artifact.Slot);
            this.Effects = spell == null ? DescribeEffects(effectLines, artifact.EffectInfo) : $"Casts {spell.Name}, level {spell.Level} {spell.School} spell";
        }

        public Artifact Artifact { get; }

        /// <summary>
        /// Gets the spell of a Spell Scroll row; null for every other artifact.
        /// </summary>
        public Spell? Spell { get; }

        public string Name => this.Spell == null ? this.Artifact.Name : SpellScroll.NameOf(this.Spell);

        /// <summary>
        /// Gets where the artifact is worn, such as "Helm", "Ring" or "Item"; the list is grouped by it.
        /// </summary>
        public string SlotName { get; }

        /// <summary>
        /// Gets the rarity class, such as "Relic".
        /// </summary>
        public string TypeName => this.Artifact.Class.ToString();

        public bool IsCombined => this.Artifact.IsCombination;

        /// <summary>
        /// Gets the gold cost, or a dash when it is not known.
        /// </summary>
        public string Cost => this.Artifact.Cost > 0 ? this.Artifact.Cost.ToString("N0", CultureInfo.CurrentCulture) : "—";

        /// <summary>
        /// Gets what the artifact does in a line, such as "Attack +3, Defense +3".
        /// </summary>
        public string Effects { get; }

        /// <summary>
        /// Gets the heading a slot is listed under, such as "Helm" for the helmet slot and "Item" for the five spare slots.
        /// </summary>
        private static string SlotNameOf(ArtifactSlots slot)
        {
            if (slot == ArtifactSlots.None)
            {
                return "Other";
            }

            if ((slot & ArtifactSlots.Misc) != 0)
            {
                return "Item";
            }

            if ((slot & ArtifactSlots.Rings) != 0)
            {
                return "Ring";
            }

            return slot switch
            {
                ArtifactSlots.Helmet => "Helm",
                ArtifactSlots.Necklace => "Neck",
                ArtifactSlots.Torso => "Armor",
                ArtifactSlots.Cape => "Cloak",
                ArtifactSlots.Feet => "Boots",
                ArtifactSlots.Weapon => "Weapon",
                ArtifactSlots.Shield => "Shield",
                _ => "Other",
            };
        }

        private static string DescribeEffects(EffectLineProvider effectLines, ArtifactEffectInfo info)
        {
            var parts = effectLines.Describe(info).Select(l => $"{l.Label} {l.Value}").ToList();
            if (!string.IsNullOrEmpty(info.Other))
            {
                parts.Add(info.Other);
            }

            return string.Join(", ", parts);
        }
    }
}
