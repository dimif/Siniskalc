namespace Siniskalc.Heroes.ViewModels
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Siniskalc.Heroes.Models;

    /// <summary>
    /// The words for what an artifact adds to the primary skills: "Attack +6", one line for each skill it changes.
    /// </summary>
    public static class PrimarySkillBonusText
    {
        /// <summary>
        /// Returns a line for each skill the bonus changes, such as "Attack" and "+6".
        /// </summary>
        public static IReadOnlyList<FactRow> Facts(PrimarySkillBonus bonus) =>
            new[] { ("Attack", bonus.Attack), ("Defense", bonus.Defense), ("Spell power", bonus.SpellPower), ("Knowledge", bonus.Knowledge) }
                .Where(skill => skill.Item2 != 0)
                .Select(skill => new FactRow(skill.Item1, skill.Item2.ToString("+0;-0", CultureInfo.InvariantCulture)))
                .ToList();

        /// <summary>
        /// Returns the bonus in one line, such as "Attack +6, Defense +6", or nothing when it changes no skill.
        /// </summary>
        public static string Describe(PrimarySkillBonus bonus) => string.Join(", ", Facts(bonus).Select(fact => $"{fact.Label} {fact.Value}"));
    }
}
