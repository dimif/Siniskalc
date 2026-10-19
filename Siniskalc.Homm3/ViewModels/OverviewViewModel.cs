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
    public sealed class OverviewViewModel : AbstractViewModelBase, IOverviewPage
    {
        private readonly Game game;
        private readonly BanishingTextProvider banishingText;
        private readonly Action<int> openHero;
        private readonly Action<int> openPlayer;
        private readonly Action<int>? openCastle;

        public OverviewViewModel(Game game, BanishingTextProvider banishingText, Action<int> openHero, Action<int> openPlayer, Action<int>? openCastle = null)
        {
            this.game = game;
            this.banishingText = banishingText;
            this.openHero = openHero;
            this.openPlayer = openPlayer;
            this.openCastle = openCastle;
            this.Refresh();
        }

        public string FileName => string.IsNullOrEmpty(this.game.Name) ? "Savegame" : System.IO.Path.GetFileName(this.game.Name);

        public string VersionName => this.game.GameVersion switch
        {
            GameVersion.RestorationOfErathia => "Restoration of Erathia",
            GameVersion.ArmageddonsBlade => "Armageddon's Blade",
            GameVersion.ShadowOfDeath => "Shadow of Death",
            GameVersion.HeroesChronicles => "Heroes Chronicles",
            GameVersion.HornOfTheAbyss => "Horn of the Abyss",
            _ => "Unknown version",
        };

        /// <summary>
        /// Gets the date in the game, e.g. "Month 5, Week 4, Day 6", or nothing when the savefile's date is not known.
        /// </summary>
        public string DateText => this.game.Date is { } date ? $"Month {date.Month}, Week {date.Week}, Day {date.Day}" : string.Empty;

        /// <summary>
        /// Gets the difficulty, e.g. "Difficulty: Queen (160%)", or nothing when the savefile's difficulty is not known.
        /// </summary>
        public string DifficultyText => this.game.Difficulty is { } difficulty ? OverviewText.Difficulty(difficulty) : string.Empty;

        /// <summary>
        /// Gets the difficulty the map was made with, e.g. "Rating: King (200%)", or nothing when it is not known.
        /// </summary>
        public string MapDifficultyText => this.game.MapDifficulty is { } difficulty ? OverviewText.MapRating(difficulty) : string.Empty;

        /// <summary>
        /// Gets whether the savefile tells about the map. The other map properties are empty when it does not.
        /// </summary>
        public bool HasMap => this.game.Map != null;

        /// <summary>
        /// Gets the map's name, or nothing when it is not known.
        /// </summary>
        public string MapName => this.game.Map?.Name.Trim() ?? string.Empty;

        /// <summary>
        /// Gets the map's description, or nothing when it is not known or the map has none.
        /// </summary>
        public string MapDescription => this.game.Map?.Description.Trim() ?? string.Empty;

        /// <summary>
        /// Gets the map's size and number of levels, e.g. "Small (36 x 36) · 2 levels" for a map with an underground,
        /// or nothing when they are not known.
        /// </summary>
        public string MapSizeText => this.game.Map is { } map
            ? string.Join(" · ", new[] { map.SizeText, map.HasUnderground ? "2 levels" : "1 level" }.Where(t => t.Length > 0))
            : string.Empty;

        /// <summary>
        /// Gets whether the map has a description to show.
        /// </summary>
        public bool HasMapDescription => this.MapDescription.Length > 0;

        /// <summary>
        /// Gets whose turn it was, e.g. "Current turn: Tan", or nothing when the savefile does not tell.
        /// </summary>
        public string TurnText => Siniskalc.Heroes.ViewModels.TurnText.Overview(this.game.CurrentPlayer?.ToString());

        public ObservableCollection<PlayerCardViewModel> Players { get; } = new ObservableCollection<PlayerCardViewModel>();

        /// <summary>
        /// Gets the players in the order they play (the order of the colours), the one whose turn it is lit. The editor cannot change the turn here.
        /// </summary>
        public IReadOnlyList<OrderOfPlayItemViewModel> OrderOfPlay { get; private set; } = [];

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

        public bool HasOrderOfPlay => this.OrderOfPlay.Count > 0;

        System.Collections.Generic.IReadOnlyList<PlayerCardViewModel> IOverviewPage.Players => this.Players;

        /// <summary>
        /// Gets a line about the savefile, e.g. "7 players · 12 heroes · Difficulty: Queen (160%) · 5 obelisks".
        /// </summary>
        public string Summary { get; private set; } = string.Empty;

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
            this.Players.Clear();
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
                this.Players.Add(new PlayerCardViewModel(color.ToString(), PlayerPalette.Of(color), resources, heroes, open, PlayerTypeText.Describe(this.game.GetControl(color), this.game.CurrentPlayer == color), ownCastles, player == null ? string.Empty : this.banishingText.Pill(player), player?.IsBanished ?? false));
            }

            var unowned = summaries.Count(s => s.Owner.IsNeutral);
            this.Summary = $"{OverviewText.Count(this.Players.Count, "player", "players")} · {OverviewText.Count(summaries.Count - unowned, "hero", "heroes")}";
            if (this.DifficultyText.Length > 0)
            {
                this.Summary += " · " + this.DifficultyText;
            }

            if (this.game.History.Current.Obelisks().GetModel() is { Count: > 0 } obelisks)
            {
                this.Summary += " · " + OverviewText.Count(obelisks.Count, "obelisk", "obelisks");
            }

            this.UnownedNote = unowned == 0 ? string.Empty : $"{unowned} more {(unowned == 1 ? "hero belongs" : "heroes belong")} to no player; they are listed under \"Neutral\" on the left.";
            this.RaiseAllPropertiesChanged();
        }
    }
}
