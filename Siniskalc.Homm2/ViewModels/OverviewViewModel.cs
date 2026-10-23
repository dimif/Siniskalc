namespace Siniskalc.Homm2.ViewModels
{
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
    public sealed class OverviewViewModel : OverviewViewModelBase<PlayerColor>
    {
        private readonly Game game;
        private readonly BanishingTextProvider banishingText;
        private readonly Action<PlayerColor> openPlayer;
        private readonly Action<int> openHero;
        private readonly Action<int> openCastle;

        public OverviewViewModel(Game game, BanishingTextProvider banishingText, Action<PlayerColor> openPlayer, Action<int> openHero, Action<int> openCastle)
            : base(game, game)
        {
            this.game = game;
            this.banishingText = banishingText;
            this.openPlayer = openPlayer;
            this.openHero = openHero;
            this.openCastle = openCastle;
            this.Refresh();
        }

        /// <inheritdoc/>
        public override string VersionName => this.game.GameVersion == GameVersion.PriceOfLoyalty ? "Price of Loyalty" : "Base game";

        /// <summary>
        /// Reads the players again from the current document.
        /// </summary>
        public override void Refresh()
        {
            var document = this.game.History.Current;
            var heroes = document.Heroes().GetModels();

            // A colour is in the game when it owns a castle or a hero, or the days of a player that was put out are counted.
            var inGame = document.Players().GetModels()
                .Where(player => player.CastleIds.Count > 0 || player.HeroIds.Count > 0 || player.DaysLeft != null)
                .ToList();
            this.Players = inGame.Select(player => this.CardOf(player)).ToList();
            this.OrderOfPlay = inGame.OrderBy(player => player.Color).Select(this.OrderItemOf).ToList();
            var owned = heroes.Count(h => !h.Owner.IsNeutral);
            this.Summary = SummaryOf(this.Players.Count, owned, this.DifficultyText);
            this.RaiseAllPropertiesChanged();
        }

        private OrderOfPlayItemViewModel OrderItemOf(PlayerModel player)
        {
            var color = player.Color;
            var isHuman = this.game.GetControl(color) == PlayerType.Human;
            var isCurrent = this.game.CurrentPlayer == color;
            var command = isHuman && !isCurrent
                ? new DelegateCommand(_ => this.game.History.CreateTurnSession().Update(turn => turn with { Record = this.game.RecordOf(color), Color = color }))
                : null;
            return new OrderOfPlayItemViewModel(
                color.ToString(),
                PlayerPalette.Of(color),
                isCurrent,
                command,
                isCurrent ? "It is this player's turn" : isHuman ? "Give the turn to this player" : "A computer player: it plays after the humans, and cannot be given the turn");
        }

        private PlayerCardViewModel CardOf(PlayerModel player)
        {
            var color = player.Color;
            var document = this.game.History.Current;
            var heroes = OwnedLinks.Heroes(document.Heroes(), color, this.openHero);
            var castles = CastleLinks.OwnedBy(document, color, this.openCastle, this.openHero);
            return new PlayerCardViewModel(
                color.ToString(),
                PlayerPalette.Of(color),
                player.Resources,
                heroes,
                new DelegateCommand(_ => this.openPlayer(color)),
                this.ControlTextOf(color),
                castles: castles,
                banishingText: this.banishingText.Pill(player),
                isBanished: player.IsBanished);
        }
    }
}
