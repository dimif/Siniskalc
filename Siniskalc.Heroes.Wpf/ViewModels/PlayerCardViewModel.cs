namespace Siniskalc.Heroes.Wpf.ViewModels
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Input;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.ViewModels;

    /// <summary>
    /// A player's card in the overview: what the player owns and the heroes and castles that belong to the player.
    /// </summary>
    public sealed class PlayerCardViewModel
    {
        /// <param name="name">What the player is called.</param>
        /// <param name="colorHex">The colour of the marker beside the name.</param>
        /// <param name="resources">What the player owns, or <c>null</c> when the savefile has nothing for the player.</param>
        /// <param name="heroes">The heroes of the player.</param>
        /// <param name="open">Opens the page of the player; does nothing for a player without one.</param>
        /// <param name="controlText">"Human" or "Computer", or nothing when the savefile does not tell.</param>
        /// <param name="castles">The castles of the player.</param>
        /// <param name="banishingText">The pill of a player who owns no town, or nothing.</param>
        /// <param name="isBanished">Whether the player is out of the game.</param>
        public PlayerCardViewModel(string name, string colorHex, ResourceSet? resources, IReadOnlyList<ILink> heroes, ICommand open, string controlText = "", IReadOnlyList<CastleLinkViewModel>? castles = null, string banishingText = "", bool isBanished = false)
        {
            this.ControlText = controlText;
            this.Name = name;
            this.BanishingText = banishingText;
            this.IsBanished = isBanished;
            this.ColorHex = colorHex;
            this.Heroes = heroes;
            this.Castles = castles ?? [];
            this.OpenCommand = open;
            this.HasResources = resources != null;
            this.Gold = resources?.Gold.ToString("N0") ?? string.Empty;
            this.OtherResources = resources == null
                ? string.Empty
                : string.Join(" · ", ResourceSet.All.Where(r => r != Resource.Gold).Select(r => $"{resources[r]:N0} {r.ToString().ToLowerInvariant()}"));
        }

        /// <summary>
        /// Gets the name of the player, which is the name of the colour.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the colour of the player as a hex string.
        /// </summary>
        public string ColorHex { get; }

        /// <summary>
        /// Gets the pill of a player who owns no town ("No towns: 6 days left", "Banished"), or nothing.
        /// </summary>
        public string BanishingText { get; }

        /// <summary>
        /// Gets a value indicating whether the card has a note about the player being banished.
        /// </summary>
        public bool HasBanishing => this.BanishingText.Length > 0;

        /// <summary>
        /// Gets a value indicating whether the player was banished from the game.
        /// </summary>
        public bool IsBanished { get; }

        /// <summary>
        /// Gets "Human" or "Computer", or nothing when the savefile does not tell.
        /// </summary>
        public string ControlText { get; }

        /// <summary>
        /// Gets a value indicating whether the savefile has a record of the resources of the player.
        /// </summary>
        public bool HasResources { get; }

        /// <summary>
        /// Gets the gold of the player, as text.
        /// </summary>
        public string Gold { get; }

        /// <summary>
        /// Gets the amounts of everything but gold, on one line.
        /// </summary>
        public string OtherResources { get; }

        /// <summary>
        /// Gets the links to the heroes of the player.
        /// </summary>
        public IReadOnlyList<ILink> Heroes { get; }

        /// <summary>
        /// Gets a value indicating whether the player has any hero.
        /// </summary>
        public bool HasHeroes => this.Heroes.Count > 0;

        /// <summary>
        /// Gets the links to the castles of the player.
        /// </summary>
        public IReadOnlyList<CastleLinkViewModel> Castles { get; }

        /// <summary>
        /// Gets a value indicating whether the player has any castle.
        /// </summary>
        public bool HasCastles => this.Castles.Count > 0;

        /// <summary>
        /// Gets the command that opens the player's page; it does nothing for a player without one.
        /// </summary>
        public ICommand OpenCommand { get; }
    }
}
