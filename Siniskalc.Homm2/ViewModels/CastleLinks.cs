namespace Siniskalc.Homm2.ViewModels
{
    using System;
    using System.Collections.Generic;
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// Makes the links to the castles of a savegame and to the hero in them, with the words HoMM2 uses for a settlement.
    /// </summary>
    public static class CastleLinks
    {
        /// <summary>
        /// Returns the link of each castle <paramref name="owner"/> owns that the map uses, ordered by name: its faction, whether it is a
        /// castle or a town, the level of its Mage Guild and how many dwellings stand in it, and the hero that visits it.
        /// </summary>
        /// <param name="document">The savefile.</param>
        /// <param name="owner">The colour whose castles are wanted.</param>
        /// <param name="openCastle">Opens the page of the castle with a number.</param>
        /// <param name="openHero">Opens the page of the hero at an index among the heroes.</param>
        /// <returns>The links.</returns>
        public static IReadOnlyList<CastleLinkViewModel> OwnedBy(SaveDocument document, PlayerColor owner, Action<int> openCastle, Action<int> openHero) =>
            OwnedLinks.Castles<CastleModel, PlayerColor, Creature, Faction>(
                document.Castles().GetModels(),
                document.Heroes(),
                owner,
                openCastle,
                openHero,
                castle => [castle.HasCastle ? "Castle" : "Town"]);
    }
}
