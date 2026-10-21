namespace Siniskalc.Heroes.Wpf.ViewModels
{
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core.Runtime;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.ViewModels;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// What the overview page of any game says about the open savefile: its name, the map, the date, the difficulty and whose turn it is, as
    /// the words <c>OverviewView</c> shows. A game says where the facts are (<see cref="Map"/>, <see cref="Date"/> and the others, null when
    /// its savefile does not tell), builds the cards of the players and sets <see cref="Players"/>, <see cref="OrderOfPlay"/> and
    /// <see cref="Summary"/>.
    /// </summary>
    public abstract class OverviewViewModelBase : AbstractViewModelBase, IOverviewPage
    {
        private const string UnnamedFile = "Savegame";

        private readonly AbstractSavegame savegame;

        protected OverviewViewModelBase(AbstractSavegame savegame)
        {
            this.savegame = savegame;
        }

        /// <inheritdoc/>
        public string FileName => string.IsNullOrEmpty(this.savegame.Name) ? UnnamedFile : System.IO.Path.GetFileName(this.savegame.Name);

        /// <summary>
        /// Gets the map's name, or nothing when it is not known.
        /// </summary>
        public string MapName => this.Map?.Name.Trim() ?? string.Empty;

        /// <summary>
        /// Gets the map's description, or nothing when it is not known or the map has none.
        /// </summary>
        public string MapDescription => this.Map?.Description.Trim() ?? string.Empty;

        /// <inheritdoc/>
        public bool HasMapDescription => this.MapDescription.Length > 0;

        /// <inheritdoc/>
        public string MapSizeText => this.Map is { } map ? this.SizeOf(map) : string.Empty;

        /// <inheritdoc/>
        public string MapDifficultyText => this.Map is { } map ? OverviewText.MapRating(map.Difficulty) : string.Empty;

        /// <inheritdoc/>
        public abstract string VersionName { get; }

        /// <summary>
        /// Gets the date in the game, e.g. "Month 5, Week 4, Day 6", or nothing when the savefile's date is not known.
        /// </summary>
        public string DateText => this.Date?.ToString() ?? string.Empty;

        /// <summary>
        /// Gets the difficulty, e.g. "Difficulty: Queen (160%)", or nothing when the savefile's difficulty is not known.
        /// </summary>
        public string DifficultyText => this.Difficulty is { } difficulty ? OverviewText.Difficulty(difficulty) : string.Empty;

        /// <inheritdoc/>
        public string TurnText => Siniskalc.Heroes.ViewModels.TurnText.Overview(this.CurrentPlayerName);

        /// <inheritdoc/>
        public string Summary { get; protected set; } = string.Empty;

        /// <inheritdoc/>
        public IReadOnlyList<PlayerCardViewModel> Players { get; protected set; } = [];

        /// <inheritdoc/>
        public IReadOnlyList<OrderOfPlayItemViewModel> OrderOfPlay { get; protected set; } = [];

        /// <inheritdoc/>
        public bool HasOrderOfPlay => this.OrderOfPlay.Count > 0;

        /// <summary>
        /// Gets the map's header, or null when the savefile does not tell.
        /// </summary>
        protected abstract MapHeader? Map { get; }

        /// <summary>
        /// Gets the date in the game, or null when the savefile does not tell.
        /// </summary>
        protected abstract GameDate? Date { get; }

        /// <summary>
        /// Gets the difficulty the game was started with, or null when the savefile does not tell.
        /// </summary>
        protected abstract GameDifficulty? Difficulty { get; }

        /// <summary>
        /// Gets the name of the colour whose turn it is, or null when the savefile does not tell.
        /// </summary>
        protected abstract string? CurrentPlayerName { get; }

        /// <summary>
        /// Returns the size of the map as the page shows it; the size alone unless the game says more (HoMM3 adds its levels).
        /// </summary>
        protected virtual string SizeOf(MapHeader map) => map.SizeText;

        /// <summary>
        /// Returns the line about the savefile: the players, the heroes owned, then each of <paramref name="more"/> that is not empty,
        /// separated by dots.
        /// </summary>
        protected static string SummaryOf(int players, int heroes, params string[] more)
        {
            return string.Join(" · ", new[] { OverviewText.Count(players, "player", "players"), OverviewText.Count(heroes, "hero", "heroes") }.Concat(more.Where(text => text.Length > 0)));
        }
    }
}
