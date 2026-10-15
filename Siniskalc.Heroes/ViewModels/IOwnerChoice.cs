namespace Siniskalc.Heroes.ViewModels
{
    using System.Collections.Generic;

    /// <summary>
    /// A page about something a player can own (a hero, a castle) that lets the user change the owner: the choices are "Neutral" and the
    /// names of the player colours of the game, and the owner is picked by name. The owner picker of the pages binds to it.
    /// </summary>
    public interface IOwnerChoice
    {
        /// <summary>
        /// Gets the owners to pick from: "Neutral" and the colours of the players.
        /// </summary>
        IReadOnlyList<string> OwnerChoices { get; }

        /// <summary>
        /// Gets or sets the owner, as "Neutral" or the name of a colour.
        /// </summary>
        string SelectedOwner { get; set; }
    }
}
