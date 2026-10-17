namespace Siniskalc.Heroes.Entities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// The factions of a game whose factions are an enum, as <see cref="IFaction"/>.
    /// </summary>
    /// <typeparam name="TFaction">The enum of the factions; its byte values are the codes of the savegame.</typeparam>
    public static class Factions<TFaction>
        where TFaction : struct, Enum
    {
        private static readonly IReadOnlyList<FactionInfo> AllFactions = Enum.GetValues<TFaction>()
            .Select(f => new FactionInfo(f.ToString(), Convert.ToByte(f)))
            .ToList();

        public static IReadOnlyList<IFaction> All => AllFactions;

        /// <summary>
        /// Returns <paramref name="faction"/> as an <see cref="IFaction"/>, or <c>null</c> when it is <c>null</c> (a neutral creature or an unknown town).
        /// </summary>
        public static IFaction? Of(TFaction? faction) => faction is { } value ? AllFactions.First(f => f.Code == Convert.ToByte(value)) : null;
    }
}
