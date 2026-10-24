namespace Siniskalc.Homm3.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core;
    using Siniskalc.Homm3.Entities;

    /// <summary>
    /// The table of the savefile that says, for each hero id, who owns the hero (see <see cref="Data.HeroOwnersLayout"/>). The game keeps it
    /// in step with the owner of each hero and the lists of heroes of the players; the editor changes it with the owner of a hero
    /// (<see cref="Data.HeroEditSession.ChangeOwner"/>).
    /// </summary>
    /// <param name="Heroes">An entry per hero id, in the order of the ids.</param>
    public sealed record HeroOwnersModel(IReadOnlyList<HeroOwnership> Heroes)
    {
        /// <summary>
        /// Returns whether the table has an entry for the hero.
        /// </summary>
        public bool Has(int heroId) => heroId >= 0 && heroId < this.Heroes.Count;

        /// <summary>
        /// Returns the table with the hero owned by <paramref name="owner"/>, or by nobody. A hero given to a player is no longer reserved, as a
        /// hired hero is not; a hero that leaves its player may be drawn for a tavern again, as a defeated hero may.
        /// </summary>
        public HeroOwnersModel WithOwner(int heroId, PlayerColor owner) =>
            this.With(heroId, owner.IsNeutral ? HeroOwnership.Free : new HeroOwnership(owner, false));

        /// <summary>
        /// Returns the table with <paramref name="ownership"/> for the hero.
        /// </summary>
        public HeroOwnersModel With(int heroId, HeroOwnership ownership)
        {
            RangeGuard.ThrowIfNotIndex(heroId, this.Heroes.Count, nameof(heroId));
            var heroes = this.Heroes.ToArray();
            heroes[heroId] = ownership;
            return new HeroOwnersModel(heroes);
        }

        /// <inheritdoc/>
        public bool Equals(HeroOwnersModel? other) => other is not null && this.Heroes.SequenceEqual(other.Heroes);

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(this.Heroes.Count);
    }
}
