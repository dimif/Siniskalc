namespace Siniskalc.Entities
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    /// <summary>
    /// A Spell Scroll is one artifact id; the spell it holds is stored beside it. The editor names each combination
    /// "Spell Scroll: Town Portal" so that anything that picks artifacts by name can pick the spell too.
    /// </summary>
    public static class SpellScroll
    {
        public const string Prefix = "Spell Scroll: ";

        public static string NameOf(Spell spell) => Prefix + spell.Name;

        /// <summary>
        /// Reads a name made by <see cref="NameOf"/>; false for anything else, including a bare "Spell Scroll".
        /// </summary>
        public static bool TryParse(string? name, [NotNullWhen(true)] out Spell? spell)
        {
            spell = null;
            if (name == null || !name.StartsWith(Prefix, System.StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            spell = Spell.FromName<Spell>(name[Prefix.Length..].Trim());
            return spell != null;
        }

        /// <summary>
        /// Gets the spells a scroll can hold in <paramref name="version"/>. The hero-only Titan's Lightning Bolt has no scroll.
        /// </summary>
        public static IReadOnlyList<Spell> For(GameVersion version)
        {
            return Spell.All.Where(s => s.IsAvailableIn(version) && s != Spell.TitansLightningBolt).ToList();
        }
    }
}
