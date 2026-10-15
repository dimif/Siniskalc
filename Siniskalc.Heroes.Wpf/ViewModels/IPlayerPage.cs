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

        string ColorHex { get; }

        /// <summary>
        /// Gets or sets the tab shown: 0 for the resources, 1 for the heroes, 2 for the towns.
        /// </summary>
        int SelectedTab { get; set; }

        IReadOnlyList<ILink> Heroes { get; }

        bool HasHeroes { get; }

        IReadOnlyList<ILink> Castles { get; }

        bool HasCastles { get; }
    }
}
