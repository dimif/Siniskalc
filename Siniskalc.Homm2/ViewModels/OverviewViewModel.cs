namespace Siniskalc.Homm2.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core.Data;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;
    using Siniskalc.Homm2.Runtime;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// What the editor shows about the savegame that is open: the map, the date and each player. It reads the models of the current
    /// document, so it is up to date after every edit, undo and redo.
    /// </summary>
    public sealed class OverviewViewModel : AbstractViewModelBase, IDisposable
    {
        private readonly Game game;

        public OverviewViewModel(Game game, Action<Color> openPlayer)
        {
            this.game = game;
            this.OpenPlayerCommand = new DelegateCommand(color => openPlayer((Color)color!));
            this.game.History.Changed += this.OnHistoryChanged;
            this.Refresh();
        }

        public string MapName => this.game.Map.Name;

        public string Description => this.game.Map.Description;

        public string MapSize => $"{this.game.Map.Width} × {this.game.Map.Height}";

        public string Version => this.game.GameVersion == GameVersion.PriceOfLoyalty ? "Price of Loyalty" : "Base game";

        public string Date => this.game.Date.ToString();

        /// <summary>
        /// Gets the command that shows the page of a player; its parameter is the colour of the player.
        /// </summary>
        public System.Windows.Input.ICommand OpenPlayerCommand { get; }

        public IReadOnlyList<PlayerSummary> Players { get; private set; } = [];

        /// <summary>
        /// Reads the players again from the current document.
        /// </summary>
        public void Refresh()
        {
            var document = this.game.History.Current;
            var castles = document.Castles().GetModels();
            var heroes = document.Heroes().GetModels();

            // A colour is in the game when it owns a castle or a hero, or the days of a player that was put out are counted.
            this.Players = document.Players().GetModels()
                .Where(player => player.CastleIds.Count > 0 || player.HeroIds.Count > 0 || player.DaysLeft != null)
                .Select(player => new PlayerSummary(
                    player.Color,
                    player.Resources.Gold,
                    heroes.Count(h => h.Owner == player.Color),
                    castles.Count(c => c.IsUsed && c.Owner == player.Color),
                    StatusOf(player)))
                .ToList();
            this.RaisePropertyChanged(nameof(this.Players));
        }

        public void Dispose() => this.game.History.Changed -= this.OnHistoryChanged;

        private static string StatusOf(PlayerModel player) => player.IsOut
            ? "out of the game"
            : player.DaysLeft is { } days ? $"{days} days left to own a castle" : string.Empty;

        private void OnHistoryChanged(object? sender, EventArgs e) => this.Refresh();
    }
}
