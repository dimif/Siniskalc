namespace Siniskalc.Heroes.Wpf.ViewModels
{
    using System.Collections.Generic;

    /// <summary>
    /// What the overview page of a game shows (<c>OverviewView</c>). A text that is empty is not shown.
    /// </summary>
    public interface IOverviewPage
    {
        /// <summary>
        /// Gets the name of the savefile.
        /// </summary>
        string FileName { get; }

        string MapName { get; }

        /// <summary>
        /// Gets the size of the map and how many levels it has, e.g. "Small (36 x 36) · 2 levels".
        /// </summary>
        string MapSizeText { get; }

        /// <summary>
        /// Gets the difficulty the map was made with, e.g. "Rating: Rook (130%)".
        /// </summary>
        string MapDifficultyText { get; }

        /// <summary>
        /// Gets the version of the game the savefile is from.
        /// </summary>
        string VersionName { get; }

        /// <summary>
        /// Gets a line about the savefile, e.g. "7 players · 12 heroes · Difficulty: Queen (160%)".
        /// </summary>
        string Summary { get; }

        string DateText { get; }

        /// <summary>
        /// Gets whose turn it was, e.g. "Current turn: Tan".
        /// </summary>
        string TurnText { get; }

        bool HasMapDescription { get; }

        string MapDescription { get; }

        IReadOnlyList<PlayerCardViewModel> Players { get; }

        /// <summary>
        /// Gets the players in the order they play, the one whose turn it is lit. Empty when the savefile does not tell, and then the card is not shown.
        /// </summary>
        IReadOnlyList<OrderOfPlayItemViewModel> OrderOfPlay { get; }

        bool HasOrderOfPlay { get; }
    }
}
