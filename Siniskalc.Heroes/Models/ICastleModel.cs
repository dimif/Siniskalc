namespace Siniskalc.Heroes.Models
{
    using Siniskalc.Core.Entities;
    using Siniskalc.Core.Models;
    using Siniskalc.Heroes.Entities;

    /// <summary>
    /// What a castle of any game of the series has: a name, a faction, an owner, a place on the map, a garrison, a hero that visits it, a
    /// Mage Guild and creatures waiting in its dwellings. What a game builds its castle of (HoMM2 flags, HoMM3 a set of buildings) is its own.
    /// </summary>
    /// <typeparam name="TColor">The colour of the game, which is an enumeration of its own.</typeparam>
    /// <typeparam name="TCreature">The creature of the game.</typeparam>
    /// <typeparam name="TFaction">The faction of the game, which is an enumeration of its own.</typeparam>
    public interface ICastleModel<TColor, TCreature, TFaction>
        where TColor : class, IPlayerColor
        where TFaction : class, IFaction
        where TCreature : class, IGameEntity
    {
        /// <summary>
        /// Gets the name of the castle, empty for a record the map does not use.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the faction of the town; a savefile number that is no faction reads as the faction <c>Neutral</c>.
        /// </summary>
        TFaction Faction { get; }

        /// <summary>
        /// Gets the player who owns the castle, or the colour <c>Neutral</c> for nobody.
        /// </summary>
        TColor Owner { get; }

        /// <summary>
        /// Gets the place of the castle on the map.
        /// </summary>
        MapPosition Position { get; }

        /// <summary>
        /// Gets the place a ship built in the castle appears on, or <c>null</c> for a castle that is not on the coast.
        /// </summary>
        MapPosition? ShipyardPosition { get; }

        /// <summary>
        /// Gets the army that defends the castle when no hero is in it.
        /// </summary>
        IArmy<TCreature> Garrison { get; }

        /// <summary>
        /// Gets the number of the hero that visits the castle, or <c>null</c>.
        /// </summary>
        int? VisitingHeroId { get; }

        /// <summary>
        /// Gets the number of the hero that garrisons the castle, or <c>null</c> (always, in a game whose castles have no garrison hero).
        /// </summary>
        int? GarrisonedHeroId { get; }

        /// <summary>
        /// Gets how many dwellings are built in the castle.
        /// </summary>
        int DwellingCount { get; }

        /// <summary>
        /// Gets the level of the Mage Guild, 0 when there is none.
        /// </summary>
        int MageGuildLevel { get; }

        /// <summary>
        /// Gets the creatures that wait in each dwelling, by the level of the creature. A dwelling and its upgrade share the count.
        /// </summary>
        Numbers CreaturesAvailable { get; }
    }
}
