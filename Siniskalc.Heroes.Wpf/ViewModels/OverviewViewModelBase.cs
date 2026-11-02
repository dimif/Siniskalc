namespace Siniskalc.Heroes.Wpf.ViewModels
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Input;
    using Siniskalc.Core.Runtime;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.Runtime;
    using Siniskalc.Heroes.ViewModels;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// What the overview page of any game says about the open savefile: its name, the map, the date, the difficulty and whose turn it is, as
    /// the words <c>OverviewView</c> shows. A game says where the facts are (<see cref="IGameInfo{TColor}.Map"/>, <see cref="IGameInfo{TColor}.Date"/> and the others, null when
    /// its savefile does not tell), builds the cards of the players and sets <see cref="Players"/>, <see cref="OrderOfPlay"/> (of the items
    /// <see cref="OrderItemOf"/> makes) and <see cref="Summary"/>, and says how the turn is given to a human (<see cref="GiveTurn"/>).
    /// </summary>
    public abstract class OverviewViewModelBase<TColor> : AbstractViewModelBase, IOverviewPage, IRefreshableViewModel
        where TColor : class, IPlayerColor
    {
        private const string UnnamedFile = "Savegame";

        private readonly AbstractSavegame savegame;
        private readonly IGameInfo<TColor> game;
        private readonly BanishingTextProvider banishingText;

        /// <summary>
        /// Initializes a new instance of the <see cref="OverviewViewModelBase"/> class.
        /// </summary>
        /// <param name="savegame">The savegame.</param>
        /// <param name="game">The game.</param>
        /// <param name="banishingText">The banishing text.</param>
        protected OverviewViewModelBase(AbstractSavegame savegame, IGameInfo<TColor> game, BanishingTextProvider banishingText)
        {
            this.savegame = savegame;
            this.game = game;
            this.banishingText = banishingText;
        }

        /// <inheritdoc/>
        public string FileName => string.IsNullOrEmpty(this.savegame.Name) ? UnnamedFile : System.IO.Path.GetFileName(this.savegame.Name);

        /// <inheritdoc/>
        public string MapName => this.game.Map?.Name.Trim() ?? string.Empty;

        /// <inheritdoc/>
        public string MapDescription => this.game.Map?.Description.Trim() ?? string.Empty;

        /// <inheritdoc/>
        public bool HasMapDescription => this.MapDescription.Length > 0;

        /// <inheritdoc/>
        public string MapSizeText => this.game.Map is { } map ? this.SizeOf(map) : string.Empty;

        /// <inheritdoc/>
        public string MapDifficultyText => this.game.Map is { } map ? OverviewText.MapRating(map.Difficulty) : string.Empty;

        /// <inheritdoc/>
        public abstract string VersionName { get; }

        /// <inheritdoc/>
        public string DateText => this.game.Date?.ToString() ?? string.Empty;

        /// <summary>
        /// Gets the difficulty, e.g. "Difficulty: Queen (160%)", or nothing when the savefile's difficulty is not known.
        /// </summary>
        public string DifficultyText => this.game.Difficulty is { } difficulty ? OverviewText.Difficulty(difficulty) : string.Empty;

        /// <inheritdoc/>
        public string TurnText => Siniskalc.Heroes.ViewModels.TurnText.Overview(this.game.CurrentPlayer?.ToString());

        /// <inheritdoc/>
        public string Summary { get; protected set; } = string.Empty;

        /// <inheritdoc/>
        public IReadOnlyList<PlayerCardViewModel> Players { get; protected set; } = [];

        /// <inheritdoc/>
        public IReadOnlyList<OrderOfPlayItemViewModel> OrderOfPlay { get; protected set; } = [];

        /// <inheritdoc/>
        public bool HasOrderOfPlay => this.OrderOfPlay.Count > 0;

        /// <summary>
        /// Gets the game the page is about.
        /// </summary>
        protected IGameInfo<TColor> Game => this.game;

        /// <summary>
        /// Gets the tooltip of the item of a human who has not the turn, which gives it the turn when pressed.
        /// </summary>
        protected virtual string GiveTurnToolTip => "Give the turn to this player";

        /// <summary>
        /// Gets a value indicating whether the button of a human who has not the turn can be pressed; a game switches it off while giving the
        /// turn is not trusted yet.
        /// </summary>
        protected virtual bool CanGiveTurn => true;

        /// <summary>
        /// Gets the tooltip of the item of a computer player, which cannot be given the turn.
        /// </summary>
        protected abstract string ComputerTurnToolTip { get; }

        /// <inheritdoc/>
        public abstract void Refresh();

        /// <summary>
        /// Returns the line about the savefile: the players, the heroes owned, then each of <paramref name="more"/> that is not empty,
        /// separated by dots.
        /// </summary>
        /// <param name="players">The number of players.</param>
        /// <param name="heroes">The number of heroes owned.</param>
        /// <returns>The line.</returns>
        /// <param name="more">More parts of the line; empty ones are left out.</param>
        protected static string SummaryOf(int players, int heroes, params string[] more)
        {
            return string.Join(" · ", new[] { OverviewText.Count(players, "player", "players"), OverviewText.Count(heroes, "hero", "heroes") }.Concat(more.Where(text => text.Length > 0)));
        }

        /// <summary>
        /// Returns who plays <paramref name="color"/>, followed by "current turn" when it is that colour's turn.
        /// </summary>
        /// <param name="color">The colour.</param>
        /// <returns>The text.</returns>
        protected string ControlTextOf(TColor color) => PlayerTypeText.Describe(this.game, color);

        /// <summary>
        /// Builds a player's overview card, including a colour whose player record is absent.
        /// </summary>
        /// <param name="color">The colour of the player.</param>
        /// <param name="player">The player record, or nothing when the savefile has none.</param>
        /// <param name="heroes">The links to the player's heroes.</param>
        /// <param name="castles">The links to the player's castles.</param>
        /// <param name="open">The game's command to open the player page.</param>
        /// <returns>The card.</returns>
        protected PlayerCardViewModel CardOf(TColor color, PlayerModelBase<TColor>? player, IReadOnlyList<ILink> heroes, IReadOnlyList<CastleLinkViewModel> castles, ICommand open) =>
            new(
                color.ToString() ?? string.Empty,
                PlayerPalette.Of(color),
                player?.Resources,
                heroes,
                open,
                this.ControlTextOf(color),
                castles,
                player == null ? string.Empty : this.banishingText.Pill(player),
                player?.IsBanished ?? false);

        /// <summary>
        /// Returns the item of <paramref name="color"/> in the order of play: lit when it is that colour's turn, and with a command that gives it
        /// the turn (<see cref="GiveTurn"/>) when it is a human who has not the turn.
        /// </summary>
        /// <param name="color">The colour.</param>
        /// <returns>The item.</returns>
        protected OrderOfPlayItemViewModel OrderItemOf(TColor color)
        {
            var isCurrent = Equals(this.game.CurrentPlayer, color);
            var isHuman = this.game.GetControl(color) == PlayerType.Human;
            return new OrderOfPlayItemViewModel(
                color.ToString() ?? string.Empty,
                PlayerPalette.Of(color),
                isCurrent,
                isHuman && !isCurrent ? new DelegateCommand(_ => this.GiveTurn(color), _ => this.CanGiveTurn) : null,
                isCurrent ? "It is this player's turn" : isHuman ? this.GiveTurnToolTip : this.ComputerTurnToolTip,
                isHuman);
        }

        /// <summary>
        /// Gives the turn to <paramref name="color"/>, a human, through the history of the savefile.
        /// </summary>
        /// <param name="color">The colour of a human who has not the turn.</param>
        protected abstract void GiveTurn(TColor color);

        /// <summary>
        /// Returns the size of the map as the page shows it; the size alone unless the game says more (HoMM3 adds its levels).
        /// </summary>
        /// <param name="map">The header of the map.</param>
        /// <returns>The text.</returns>
        protected virtual string SizeOf(MapHeader map) => map.SizeText;
    }
}
