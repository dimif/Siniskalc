namespace Siniskalc.Heroes.Models
{
    /// <summary>
    /// What a player is in every game of the series: a colour, a stock of resources, the heroes its taverns offer, and the countdown
    /// that puts it out of the game once it owns no town (<see cref="BanishablePlayerBase"/>). A game's player derives from it and adds the rest.
    /// </summary>
    /// <typeparam name="TColor">The colour of the game, which is an enumeration of its own.</typeparam>
    /// <param name="Color">The colour of the player.</param>
    /// <param name="Resources">What the player owns.</param>
    /// <param name="Tavern">The heroes the taverns of the player offer.</param>
    /// <param name="DaysLeft">
    /// The days before the player is out of the game for owning no town, <see cref="Constants.Player.MaxDaysLeft"/> at most, or 0 once it
    /// is out; <c>null</c> while the player owns a town.
    /// </param>
    public abstract record PlayerModelBase<TColor>(TColor Color, ResourceSet Resources, TavernHeroes Tavern, int? DaysLeft)
        : BanishablePlayerBase(DaysLeft);
}
