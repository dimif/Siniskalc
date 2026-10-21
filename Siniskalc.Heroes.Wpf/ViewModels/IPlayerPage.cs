namespace Siniskalc.Heroes.Wpf.ViewModels
{
    using System.Collections.Generic;
    using Siniskalc.Heroes.ViewModels;

    /// <summary>
    /// The page of a player in any game: its resources, and the heroes and towns it owns. A game's page adds what only that game has
    /// (the banishing of a player with no town, the obelisks of a player's map, the heroes of a tavern).
    /// </summary>
    public interface IPlayerPage : IResettablePage, IResourcesPage
    {
        /// <summary>
        /// Gets the name of the player, which is its colour.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the colour of the player as a hex string, the one the editor draws it with (see <c>PlayerPalette</c>).
        /// </summary>
        string ColorHex { get; }

        /// <summary>
        /// Gets or sets the tab shown: 0 for the resources, 1 for the heroes, 2 for the towns.
        /// </summary>
        int SelectedTab { get; set; }

        /// <summary>
        /// Gets the heroes the player owns, each as a link that opens its page.
        /// </summary>
        IReadOnlyList<ILink> Heroes { get; }

        /// <summary>
        /// Gets whether the player owns a hero, which is when the list of heroes is shown.
        /// </summary>
        bool HasHeroes { get; }

        /// <summary>
        /// Gets the towns the player owns, each as a link that opens its page.
        /// </summary>
        IReadOnlyList<ILink> Castles { get; }

        /// <summary>
        /// Gets whether the player owns a town, which is when the list of towns is shown.
        /// </summary>
        bool HasCastles { get; }
    }
}
