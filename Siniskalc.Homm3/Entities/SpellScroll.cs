namespace Siniskalc.Homm3.Entities
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
        public static string NameOf(Spell spell) => spell.Catalog.Scroll.Name + ": " + spell.Name;

        /// <summary>
        /// Reads a name made by <see cref="NameOf"/>; false for anything else, including a bare "Spell Scroll".
        /// </summary>
        public static bool TryParse(EntityCatalog catalog, string? name, [NotNullWhen(true)] out Spell? spell)
        {
            spell = null;
            var prefix = catalog.Scroll.Name + ": ";
            if (name == null || !name.StartsWith(prefix, System.StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            spell = catalog.Spells.FindByName(name[prefix.Length..].Trim());
            return spell != null;
        }

        /// <summary>
        /// Gets the spells a scroll can hold in <paramref name="catalog"/>. Hero-only spells have no scroll.
        /// </summary>
        public static IReadOnlyList<Spell> For(EntityCatalog catalog)
        {
            return catalog.Spells.All.Where(s => !s.HeroOnly).ToList();
        }
    }
}
