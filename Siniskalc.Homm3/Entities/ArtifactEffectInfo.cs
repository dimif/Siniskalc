namespace Siniskalc.Homm3.Entities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// What an artifact (or all of a hero's artifacts together) does.
    /// </summary>
    public readonly record struct ArtifactEffectInfo(short Atk, short Def, short Pow, short Kno, short Luck, short Morale, short LandMovement, short WaterMovement, IReadOnlyList<SpellGrantProvider>? SpellGrants, bool GrantsSpellBook, short Gold, short Ore, short Wood, short Crystal, short Gems, short Mercury, short Sulfur, string Other)
    {
        /// <summary>
        /// Gets the spells the artifact lets the wearer cast in <paramref name="version"/>.
        /// </summary>
        /// <param name="version">The version of the game.</param>
        /// <returns>The spells.</returns>
        public IReadOnlyCollection<Spell> GrantedSpells(GameVersion version)
        {
            return (this.SpellGrants ?? []).SelectMany(g => g.Resolve(version)).Distinct().ToList();
        }

        /// <summary>
        /// Determines whether another effect is the same as this one.
        /// </summary>
        /// <param name="other">The effect to compare with.</param>
        /// <returns><see langword="true"/> if every value and every spell grant is the same.</returns>
        public bool Equals(ArtifactEffectInfo other)
        {
            return this.Atk == other.Atk && this.Def == other.Def && this.Pow == other.Pow && this.Kno == other.Kno
                && this.Luck == other.Luck && this.Morale == other.Morale && this.LandMovement == other.LandMovement
                && this.WaterMovement == other.WaterMovement && this.GrantsSpellBook == other.GrantsSpellBook
                && this.Gold == other.Gold && this.Ore == other.Ore && this.Wood == other.Wood && this.Crystal == other.Crystal
                && this.Gems == other.Gems && this.Mercury == other.Mercury && this.Sulfur == other.Sulfur
                && this.Other == other.Other
                && (this.SpellGrants ?? []).SequenceEqual(other.SpellGrants ?? []);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            var hash = default(HashCode);
            hash.Add(this.Atk);
            hash.Add(this.Def);
            hash.Add(this.Pow);
            hash.Add(this.Kno);
            hash.Add(this.GrantsSpellBook);
            hash.Add(this.Other);
            hash.Add((this.SpellGrants ?? []).Count);
            return hash.ToHashCode();
        }
    }
}
