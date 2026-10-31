namespace Siniskalc.Homm3.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.ViewModels;
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Homm3.Data;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Runtime;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// What the savefile contains, with a way into each player and hero.
    /// </summary>
    public sealed class OverviewViewModel : OverviewViewModelBase<PlayerColor>
    {
        /// <summary>
        /// The tooltip of the button that gives the turn to a human player.
        /// </summary>
        public const string GiveTheTurnToolTip = "Give the turn to this player: the savegame then opens on this player's turn. Not tried in the game.";

        private readonly Game game;
        private readonly BanishingTextProvider banishingText;
        private readonly Action<int> openHero;
        private readonly Action<int> openPlayer;
        private readonly Action<int>? openCastle;

        public OverviewViewModel(Game game, BanishingTextProvider banishingText, Action<int> openHero, Action<int> openPlayer, Action<int>? openCastle = null)
            : base(game, game)
        {
            this.game = game;
            this.banishingText = banishingText;
            this.openHero = openHero;
            this.openPlayer = openPlayer;
            this.openCastle = openCastle;
            this.Refresh();
        }

        /// <inheritdoc/>
        public override string VersionName => this.game.VersionName;

        /// <summary>
        /// Gets whether the savefile tells about the map. The other map properties are empty when it does not.
        /// </summary>
        public bool HasMap => this.game.Map != null;

        /// <summary>
        /// Gets a note about the heroes that no player owns, or nothing when every hero has an owner.
        /// </summary>
        public string UnownedNote { get; private set; } = string.Empty;

        /// <inheritdoc/>
        protected override string GiveTurnToolTip => GiveTheTurnToolTip;

        /// <inheritdoc/>
        protected override string ComputerTurnToolTip => "A computer player cannot be given the turn";

        /// <summary>
        /// Builds the cards again from the savefile as it is edited now.
        /// </summary>
        public void Refresh()
        {
            var summaries = this.game.History.Current.Heroes().GetSummaries();
            var colors = this.game.History.Current.Players().GetColors()
                .Concat(summaries.Select(s => s.Owner).Where(o => !o.IsNeutral))
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            var castles = this.game.History.Current.Castles().GetModels()
                .Select((model, index) => (Index: index, Model: model))
                .ToList();
            colors = colors.Concat(castles.Select(c => c.Model.Owner).Where(o => !o.IsNeutral)).Distinct().OrderBy(c => c).ToList();

            this.OrderOfPlay = colors.Select(this.OrderItemOf).ToList();
            var cards = new List<PlayerCardViewModel>();
            foreach (var color in colors)
            {
                var playerIndex = this.game.History.Current.Players().FindByColor(color);
                var player = playerIndex < 0 ? null : this.game.History.CreatePlayerSession(playerIndex).Current;
                var resources = player?.Resources;
                var heroes = OwnedLinks.Heroes(this.game.History.Current.Heroes(), color, this.openHero);
                var open = new DelegateCommand(_ => this.openPlayer(playerIndex), _ => playerIndex >= 0);
                var ownCastles = CastleLinks.OwnedBy(this.game.History.Current, color, this.openCastle, this.openHero);
                cards.Add(new PlayerCardViewModel(color.ToString(), PlayerPalette.Of(color), resources, heroes, open, this.ControlTextOf(color), ownCastles, player == null ? string.Empty : this.banishingText.Pill(player), player?.IsBanished ?? false));
            }

            this.Players = cards;
            var unowned = summaries.Count(s => s.Owner.IsNeutral);
            var obelisks = this.game.History.Current.Obelisks().GetModel() is { Count: > 0 } found ? OverviewText.Count(found.Count, "obelisk", "obelisks") : string.Empty;
            this.Summary = SummaryOf(this.Players.Count, summaries.Count - unowned, this.DifficultyText, obelisks);

            this.UnownedNote = unowned == 0 ? string.Empty : $"{unowned} more {(unowned == 1 ? "hero belongs" : "heroes belong")} to no player; they are listed under \"Neutral\" on the left.";
            this.RaiseAllPropertiesChanged();
        }

        /// <summary>
        /// Returns the size of the map and its number of levels, e.g. "Small (36 x 36) · 2 levels" for a map with an underground.
        /// </summary>
        protected override string SizeOf(MapHeader map) => string.Join(" · ", new[] { map.SizeText, map.HasUnderground ? "2 levels" : "1 level" }.Where(t => t.Length > 0));

        /// <inheritdoc/>
        protected override void GiveTurn(PlayerColor color) => this.game.History.CreateTurnSession().Update(turn => turn with { Color = color });
    }
}
