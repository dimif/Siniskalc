namespace Siniskalc.Entities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// A rule that says which spells an artifact lets its wearer cast: some named spells, every spell of a school, or every
    /// spell of a level. It is resolved for a game version, since spells from later versions do not exist in earlier ones.
    /// </summary>
    public sealed class SpellGrantProvider
    {
        private readonly Func<Spell, bool> matches;
        private readonly bool includeHeroOnly;

        private SpellGrantProvider(string description, Func<Spell, bool> matches, bool includeHeroOnly)
        {
            this.Description = description;
            this.matches = matches;
            this.includeHeroOnly = includeHeroOnly;
        }

        /// <summary>
        /// Gets what the grant gives in words, such as "Summon Boat, Scuttle Boat" or "all Fire spells".
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Grants the named spells.
        /// </summary>
        public static SpellGrantProvider Of(params Spell[] spells)
        {
            return new SpellGrantProvider(string.Join(", ", spells.Select(s => s.Name)), spells.Contains, includeHeroOnly: true);
        }

        /// <summary>
        /// Grants every spell of the school, and the ones that belong to all schools. Not Titan's Lightning Bolt, which only a
        /// hero with Titan's Thunder can cast.
        /// </summary>
        public static SpellGrantProvider OfSchool(SpellSchool school)
        {
            return new SpellGrantProvider($"all {school} spells", s => s.School == school || s.School == SpellSchool.All, includeHeroOnly: false);
        }

        /// <summary>
        /// Grants every spell of the level. Not Titan's Lightning Bolt.
        /// </summary>
        public static SpellGrantProvider OfLevel(int level)
        {
            return new SpellGrantProvider($"all level {level} spells", s => s.Level == level, includeHeroOnly: false);
        }

        /// <summary>
        /// Gets the spells the grant gives in <paramref name="version"/>.
        /// </summary>
        public IReadOnlyCollection<Spell> Resolve(GameVersion version)
        {
            return Spell.All
                .Where(s => s.IsAvailableIn(version) && this.matches(s) && (this.includeHeroOnly || s != Spell.TitansLightningBolt))
                .ToList();
        }
    }
}
