namespace Siniskalc.Homm2.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.ViewModels;
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;
    using Siniskalc.Homm2.Runtime;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// What the editor shows about the savegame that is open: the map, the date and a card for each player with its castles and heroes.
    /// It reads the models of the current document, so it is up to date after every edit, undo and redo.
    /// </summary>
    public sealed class OverviewViewModel : AbstractViewModelBase, IOverviewPage, IDisposable
    {
        private readonly Game game;
        private readonly BanishingTextProvider banishingText;
        private readonly Action<Color> openPlayer;
        private readonly Action<int> openHero;
        private readonly Action<int> openCastle;

        public OverviewViewModel(Game game, BanishingTextProvider banishingText, Action<Color> openPlayer, Action<int> openHero, Action<int> openCastle)
        {
            this.game = game;
            this.banishingText = banishingText;
            this.openPlayer = openPlayer;
            this.openHero = openHero;
            this.openCastle = openCastle;
            this.game.History.Changed += this.OnHistoryChanged;
            this.Refresh();
        }

        public string FileName => string.IsNullOrEmpty(this.game.Name) ? "Savegame" : System.IO.Path.GetFileName(this.game.Name);

        public string MapName => this.game.Map.Name.Trim();

        public string MapDescription => this.game.Map.Description.Trim();

        public bool HasMapDescription => this.MapDescription.Length > 0;

        public string MapSizeText => this.game.Map.SizeText;

        /// <summary>
        /// Gets the difficulty the map was made with, e.g. "Rating: Rook (130%)".
        /// </summary>
        public string MapDifficultyText => OverviewText.MapRating(this.game.Map.Difficulty);

        public string VersionName => this.game.GameVersion == GameVersion.PriceOfLoyalty ? "Price of Loyalty" : "Base game";

        public string DateText => this.game.Date.ToString();

        /// <summary>
        /// Gets the difficulty, e.g. "Difficulty: Queen (160%)".
        /// </summary>
        public string DifficultyText => OverviewText.Difficulty(this.game.Difficulty);

        /// <summary>
        /// Gets whose turn it is, such as "Current turn: Red", or nothing while the savegames do not tell (see <see cref="SaveMetadata"/>).
        /// </summary>
        public string TurnText => Siniskalc.Heroes.ViewModels.TurnText.Overview(this.game.CurrentPlayer?.ToString());

        /// <summary>
        /// Gets a line about the savegame, e.g. "2 players · 3 heroes · Difficulty: Queen (160%)".
        /// </summary>
        public string Summary { get; private set; } = string.Empty;

        public IReadOnlyList<PlayerCardViewModel> Players { get; private set; } = [];

        /// <summary>
        /// Reads the players again from the current document.
        /// </summary>
        public void Refresh()
        {
            var document = this.game.History.Current;
            var castles = document.Castles().GetModels().Where(c => c.IsUsed).ToList();
            var heroes = document.Heroes().GetModels();

            // A colour is in the game when it owns a castle or a hero, or the days of a player that was put out are counted.
            this.Players = document.Players().GetModels()
                .Where(player => player.CastleIds.Count > 0 || player.HeroIds.Count > 0 || player.DaysLeft != null)
                .Select(player => this.CardOf(player, heroes, castles))
                .ToList();
            var owned = heroes.Count(h => h.Owner != null);
            this.Summary = $"{OverviewText.Count(this.Players.Count, "player", "players")} · {OverviewText.Count(owned, "hero", "heroes")} · {this.DifficultyText}";
            this.RaiseAllPropertiesChanged();
        }

        public void Dispose() => this.game.History.Changed -= this.OnHistoryChanged;

        private PlayerCardViewModel CardOf(PlayerModel player, IReadOnlyList<HeroModel> allHeroes, IReadOnlyList<CastleModel> allCastles)
        {
            var color = player.Color;
            var heroes = allHeroes
                .Where(h => h.Owner == color)
                .OrderBy(h => h.Name)
                .Select(h => (ILink)new LinkViewModel(h.Name, h.Summary.Description, new DelegateCommand(_ => this.openHero(h.HeroId))))
                .ToList();
            var castles = allCastles
                .Where(c => c.Owner == color)
                .OrderBy(c => c.Name)
                .Select(c => CastleLinks.For(c, allHeroes, new DelegateCommand(_ => this.openCastle(c.Index)), this.openHero))
                .ToList();
            return new PlayerCardViewModel(
                color.ToString(),
                PlayerPalette.Of(PlayerColors<Color>.Of(color)),
                player.Resources,
                heroes,
                new DelegateCommand(_ => this.openPlayer(color)),
                Siniskalc.Heroes.ViewModels.TurnText.Beside(string.Empty, this.game.CurrentPlayer == color),
                castles: castles,
                banishingText: this.banishingText.Pill(player),
                isBanished: player.IsBanished);
        }

        private void OnHistoryChanged(object? sender, EventArgs e) => this.Refresh();
    }
}
