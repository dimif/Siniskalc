namespace Siniskalc.Homm3.ViewModels
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.ViewModels;
    using Siniskalc.Homm3.Entities;

    /// <summary>
    /// Words what artifacts do as lines such as "Attack +3". It is the one place the labels live, so that they can be translated
    /// later; it is passed to the view models that list effects instead of being called as a static method.
    /// </summary>
    public sealed class EffectLineProvider
    {
        /// <summary>
        /// Lists the numeric effects, the spells and spell book, and the resources. Only effects that are not zero get a line. The
        /// free text in <see cref="ArtifactEffectInfo.Other"/> is not included.
        /// </summary>
        /// <param name="info">The effects.</param>
        /// <returns>The lines.</returns>
        public IReadOnlyList<EffectLine> Describe(ArtifactEffectInfo info)
        {
            var lines = new List<EffectLine>();

            lines.AddRange(PrimarySkillBonusText.Facts(new PrimarySkillBonus(info.Atk, info.Def, info.Pow, info.Kno)).Select(fact => new EffectLine(fact.Label, fact.Value)));
            Add("Morale", info.Morale);
            Add("Luck", info.Luck);
            Add("Land movement", info.LandMovement);
            Add("Water movement", info.WaterMovement);
            if (info.SpellGrants is { Count: > 0 })
            {
                lines.Add(new EffectLine("Spells", string.Join("; ", info.SpellGrants.Select(g => g.Description))));
            }

            if (info.GrantsSpellBook)
            {
                lines.Add(new EffectLine("Spell book", "Given"));
            }

            Add("Gold", info.Gold);
            Add("Ore", info.Ore);
            Add("Wood", info.Wood);
            Add("Crystal", info.Crystal);
            Add("Gems", info.Gems);
            Add("Mercury", info.Mercury);
            Add("Sulfur", info.Sulfur);
            return lines;

            void Add(string label, short value)
            {
                if (value != 0)
                {
                    lines.Add(new EffectLine(label, value.ToString("+0;-0", CultureInfo.InvariantCulture)));
                }
            }
        }
    }
}
