namespace Siniskalc.Homm2.ViewModels
{
    using System;
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
        private readonly Action<PlayerColor> openPlayer;
        private readonly Action<int> openHero;
        private readonly Action<int> openCastle;

        public OverviewViewModel(Game game, BanishingTextProvider banishingText, Action<PlayerColor> openPlayer, Action<int> openHero, Action<int> openCastle)
            : base(game, game, banishingText)
        {
            this.game = game;
            this.openPlayer = openPlayer;
            this.openHero = openHero;
            this.openCastle = openCastle;
            this.Refresh();
        }

        /// <inheritdoc/>
        public override string VersionName => this.game.VersionName;

        /// <inheritdoc/>
        protected override string ComputerTurnToolTip => "Computer player: cannot be given the turn";

        /// <summary>
        /// Reads the players again from the current document.
        /// </summary>
        public override void Refresh()
        {
            var document = this.game.History.Current;
            var heroes = document.Heroes().GetModels();

            var inGame = PlayersInGame.Of(
                document.Players().GetColors(),
                heroes.Select(hero => hero.Owner),
                document.Castles().GetModels().Where(castle => castle.IsUsed).Select(castle => castle.Owner))
                .Where(color => !color.IsNeutral)
                .Select(color => document.Players().GetModel(color.Code))
                .ToList();
            this.Players = inGame.Select(player => this.CardOf(player)).ToList();
            this.OrderOfPlay = inGame.OrderBy(player => player.Color).Select(player => this.OrderItemOf(player.Color)).ToList();
            var owned = heroes.Count(h => !h.Owner.IsNeutral);
            this.Summary = SummaryOf(this.Players.Count, owned, this.DifficultyText);
            this.RaiseAllPropertiesChanged();
        }

        /// <inheritdoc/>
        protected override void GiveTurn(PlayerColor color) => this.game.History.CreateTurnSession().Update(turn => turn with { Record = this.game.RecordOf(color), Color = color });

        private PlayerCardViewModel CardOf(PlayerModel player)
        {
            var color = player.Color;
            var document = this.game.History.Current;
            var heroes = OwnedLinks.Heroes(document.Heroes(), color, this.openHero);
            var castles = CastleLinks.OwnedBy(document, color, this.openCastle, this.openHero);
            return this.CardOf(color, player, heroes, castles, new DelegateCommand(_ => this.openPlayer(color)));
        }
    }
}
