namespace Siniskalc.Homm2.Entities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Heroes.Entities;

    /// <summary>
    /// The factions of Heroes of Might and Magic II, as <see cref="IFaction"/>.
    /// </summary>
    public static class Factions
    {
        private static readonly IReadOnlyList<FactionInfo> AllFactions = Enum.GetValues<Faction>()
            .Select(f => new FactionInfo(f.ToString(), (byte)f))
            .ToList();

        public static IReadOnlyList<IFaction> All => AllFactions;

        /// <summary>
        /// Returns <paramref name="faction"/> as an <see cref="IFaction"/>, or <c>null</c> when it is <c>null</c> (a neutral creature or an unknown town).
        /// </summary>
        public static IFaction? Of(Faction? faction) => faction is { } value ? AllFactions.First(f => f.Code == (byte)value) : null;
    }
}
