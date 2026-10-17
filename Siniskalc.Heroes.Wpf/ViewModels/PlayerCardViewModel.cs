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

        public string Name { get; }

        public string ColorHex { get; }

        /// <summary>
        /// Gets the pill of a player who owns no town ("No towns: 6 days left", "Banished"), or nothing.
        /// </summary>
        public string BanishingText { get; }

        public bool HasBanishing => this.BanishingText.Length > 0;

        public bool IsBanished { get; }

        /// <summary>
        /// Gets "Human" or "Computer", or nothing when the savefile does not tell.
        /// </summary>
        public string ControlText { get; }

        public bool HasResources { get; }

        public string Gold { get; }

        /// <summary>
        /// Gets the amounts of everything but gold, on one line.
        /// </summary>
        public string OtherResources { get; }

        public IReadOnlyList<ILink> Heroes { get; }

        public bool HasHeroes => this.Heroes.Count > 0;

        public IReadOnlyList<CastleLinkViewModel> Castles { get; }

        public bool HasCastles => this.Castles.Count > 0;

        /// <summary>
        /// Opens the player's page. Does nothing for a player without one.
        /// </summary>
        public ICommand OpenCommand { get; }
    }
}
