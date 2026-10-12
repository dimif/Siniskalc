namespace Siniskalc.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Siniskalc.Data;
    using Siniskalc.Entities;
    using Siniskalc.Runtime;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// What the savefile contains, with a way into each player and hero.
    /// </summary>
    public sealed class OverviewViewModel : AbstractViewModelBase
    {
        private readonly Game game;
        private readonly Action<int> openHero;
        private readonly Action<int> openPlayer;
        private readonly Action<int>? openCastle;

        public OverviewViewModel(Game game, Action<int> openHero, Action<int> openPlayer, Action<int>? openCastle = null)
        {
            this.game = game;
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
        public string DifficultyText => this.game.Difficulty is { } difficulty ? $"Difficulty: {difficulty} ({(int)difficulty}%)" : string.Empty;

        /// <summary>
        /// Gets the difficulty the map was made with, e.g. "Rating: King (200%)", or nothing when it is not known.
        /// </summary>
        public string MapDifficultyText => this.game.MapDifficulty is { } difficulty ? $"Rating: {difficulty} ({(int)difficulty}%)" : string.Empty;

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
        public string TurnText => this.game.CurrentPlayer is { } player ? $"Current turn: {player}" : string.Empty;

        public ObservableCollection<PlayerCardViewModel> Players { get; } = new ObservableCollection<PlayerCardViewModel>();

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
                .Concat(summaries.Select(s => s.Owner).OfType<PlayerColor>())
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            var castles = this.game.History.Current.Castles().GetModels()
                .Select((model, index) => (Index: index, Model: model))
                .ToList();
            colors = colors.Concat(castles.Select(c => c.Model.Owner).OfType<PlayerColor>()).Distinct().OrderBy(c => c).ToList();

            this.Players.Clear();
            foreach (var color in colors)
            {
                var playerIndex = this.game.History.Current.Players().FindByColor(color);
                var resources = playerIndex < 0 ? null : this.game.History.CreatePlayerSession(playerIndex).Current.Resources;
                var heroes = summaries
                    .Where(s => s.Owner == color)
                    .OrderBy(s => s.Name)
                    .Select(s => new HeroLinkViewModel(s, new DelegateCommand(_ => this.openHero(s.Index))))
                    .ToList();
                var open = new DelegateCommand(_ => this.openPlayer(playerIndex), _ => playerIndex >= 0);
                var ownCastles = castles
                    .Where(c => c.Model.Owner == color)
                    .OrderBy(c => c.Model.Name)
                    .Select(c => new CastleLinkViewModel(c.Model, new DelegateCommand(_ => this.openCastle?.Invoke(c.Index)), CastleHeroRowViewModel.For(c.Model, this.game.History.Current.Heroes(), this.openHero)))
                    .ToList();
                this.Players.Add(new PlayerCardViewModel(color, resources, heroes, open, this.game.GetControl(color), this.game.CurrentPlayer == color, ownCastles));
            }

            var unowned = summaries.Count(s => s.Owner == null);
            this.Summary = $"{this.Players.Count} {(this.Players.Count == 1 ? "player" : "players")} · {summaries.Count - unowned} {(summaries.Count - unowned == 1 ? "hero" : "heroes")}";
            if (this.DifficultyText.Length > 0)
            {
                this.Summary += " · " + this.DifficultyText;
            }

            if (this.game.History.Current.Obelisks().GetModel() is { Count: > 0 } obelisks)
            {
                this.Summary += $" · {obelisks.Count} {(obelisks.Count == 1 ? "obelisk" : "obelisks")}";
            }

            this.UnownedNote = unowned == 0 ? string.Empty : $"{unowned} more {(unowned == 1 ? "hero belongs" : "heroes belong")} to no player; they are listed under \"Neutral\" on the left.";
            this.RaiseAllPropertiesChanged();
        }
    }
}
