namespace Siniskalc.Homm3.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
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
    public sealed class OverviewViewModel : OverviewViewModelBase
    {
        private readonly Game game;
        private readonly BanishingTextProvider banishingText;
        private readonly Action<int> openHero;
        private readonly Action<int> openPlayer;
        private readonly Action<int>? openCastle;

        public OverviewViewModel(Game game, BanishingTextProvider banishingText, Action<int> openHero, Action<int> openPlayer, Action<int>? openCastle = null)
            : base(game)
        {
            this.game = game;
            this.banishingText = banishingText;
            this.openHero = openHero;
            this.openPlayer = openPlayer;
            this.openCastle = openCastle;
            this.Refresh();
        }

        /// <inheritdoc/>
        public override string VersionName => this.game.GameVersion switch
        {
            GameVersion.RestorationOfErathia => "Restoration of Erathia",
            GameVersion.ArmageddonsBlade => "Armageddon's Blade",
            GameVersion.ShadowOfDeath => "Shadow of Death",
            GameVersion.HeroesChronicles => "Heroes Chronicles",
            GameVersion.HornOfTheAbyss => "Horn of the Abyss",
            _ => "Unknown version",
        };

        /// <summary>
        /// Gets whether the savefile tells about the map. The other map properties are empty when it does not.
        /// </summary>
        public bool HasMap => this.game.Map != null;

        /// <inheritdoc/>
        protected override MapHeader? Map => this.game.Map;

        /// <inheritdoc/>
        protected override GameDate? Date => this.game.Date;

        /// <inheritdoc/>
        protected override GameDifficulty? Difficulty => this.game.Difficulty;

        /// <inheritdoc/>
        protected override string? CurrentPlayerName => this.game.CurrentPlayer?.ToString();

        /// <summary>
        /// Returns the size of the map and its number of levels, e.g. "Small (36 x 36) · 2 levels" for a map with an underground.
        /// </summary>
        protected override string SizeOf(MapHeader map) => string.Join(" · ", new[] { map.SizeText, map.HasUnderground ? "2 levels" : "1 level" }.Where(t => t.Length > 0));

        // The button of a human who has not the turn can be pressed, but changes nothing yet: where the savefile keeps the turn is known
        // (see PlayerControlLocator) and writing it is not built.
        // TODO:LOG info: the turn was asked to be given to a player, and the editor does not write it for this game yet.
        private OrderOfPlayItemViewModel OrderItemOf(PlayerColor color)
        {
            var isCurrent = this.game.CurrentPlayer == color;
            var isHuman = this.game.GetControl(color) == PlayerType.Human;
            return new OrderOfPlayItemViewModel(
                color.ToString(),
                PlayerPalette.Of(color),
                isCurrent,
                isHuman && !isCurrent ? new DelegateCommand(_ => { }) : null,
                isCurrent ? "It is this player's turn" : isHuman ? "Give the turn to this player (not saved yet: the editor does not write it for this game)" : "A computer player cannot be given the turn");
        }

        /// <summary>
        /// Gets a note about the heroes that no player owns, or nothing when every hero has an owner.
        /// </summary>
        public string UnownedNote { get; private set; } = string.Empty;

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
                var heroes = summaries
                    .Where(s => s.Owner == color)
                    .OrderBy(s => s.Name)
                    .Select(s => new HeroLinkViewModel(s, new DelegateCommand(_ => this.openHero(s.Index))))
                    .ToList();
                var open = new DelegateCommand(_ => this.openPlayer(playerIndex), _ => playerIndex >= 0);
                var ownCastles = castles
                    .Where(c => c.Model.Owner == color)
                    .OrderBy(c => c.Model.Name)
                    .Select(c => CastleLinks.For(c.Model, new DelegateCommand(_ => this.openCastle?.Invoke(c.Index)), CastleLinks.HeroesOf(c.Model, this.game.History.Current.Heroes(), this.openHero)))
                    .ToList();
                cards.Add(new PlayerCardViewModel(color.ToString(), PlayerPalette.Of(color), resources, heroes, open, PlayerTypeText.Describe(this.game.GetControl(color), this.game.CurrentPlayer == color), ownCastles, player == null ? string.Empty : this.banishingText.Pill(player), player?.IsBanished ?? false));
            }

            this.Players = cards;
            var unowned = summaries.Count(s => s.Owner.IsNeutral);
            var obelisks = this.game.History.Current.Obelisks().GetModel() is { Count: > 0 } found ? OverviewText.Count(found.Count, "obelisk", "obelisks") : string.Empty;
            this.Summary = SummaryOf(this.Players.Count, summaries.Count - unowned, this.DifficultyText, obelisks);

            this.UnownedNote = unowned == 0 ? string.Empty : $"{unowned} more {(unowned == 1 ? "hero belongs" : "heroes belong")} to no player; they are listed under \"Neutral\" on the left.";
            this.RaiseAllPropertiesChanged();
        }
    }
}
