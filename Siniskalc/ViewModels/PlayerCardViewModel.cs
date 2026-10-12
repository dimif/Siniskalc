namespace Siniskalc.ViewModels
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Input;
    using Siniskalc.Entities;
    using Siniskalc.Models;

    /// <summary>
    /// A player's card in the overview: what the player owns and the heroes that belong to the player.
    /// </summary>
    public sealed class PlayerCardViewModel
    {
        public PlayerCardViewModel(PlayerColor color, PlayerResources? resources, IReadOnlyList<HeroLinkViewModel> heroes, ICommand open, PlayerType? control = null, bool isCurrent = false, IReadOnlyList<CastleLinkViewModel>? castles = null)
        {
            this.ControlText = PlayerTypeText.Describe(control, isCurrent);
            this.Name = color.ToString();
            this.ColorHex = PlayerPalette.Of(color);
            this.Heroes = heroes;
            this.Castles = castles ?? [];
            this.OpenCommand = open;
            this.HasResources = resources != null;
            this.Gold = resources?.Gold.ToString("N0") ?? string.Empty;
            this.OtherResources = resources == null
                ? string.Empty
                : string.Join(" · ", PlayerResources.All.Where(r => r != Resource.Gold).Select(r => $"{resources[r]:N0} {r.ToString().ToLowerInvariant()}"));
        }

        public string Name { get; }

        public string ColorHex { get; }

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

        public IReadOnlyList<HeroLinkViewModel> Heroes { get; }

        public bool HasHeroes => this.Heroes.Count > 0;

        public IReadOnlyList<CastleLinkViewModel> Castles { get; }

        public bool HasCastles => this.Castles.Count > 0;

        /// <summary>
        /// Opens the player's page. Does nothing for a player without one.
        /// </summary>
        public ICommand OpenCommand { get; }
    }
}
