namespace Siniskalc.Heroes.Wpf.ViewModels
{
    using System.Collections.Generic;
    using Siniskalc.Heroes.ViewModels;

    /// <summary>
    /// The page of what nobody owns, in any game: the heroes and towns that belong to no player, with the mines and dwellings a game adds.
    /// </summary>
    public interface INeutralPage : IResettablePage
    {
        /// <summary>
        /// Gets the name of the page, "Neutral".
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the colour of the page as a hex string, the one the editor draws neutral with.
        /// </summary>
        string ColorHex { get; }

        /// <summary>
        /// Gets how many heroes, towns, mines and dwellings there are, such as "40 heroes · 6 towns · 31 mines".
        /// </summary>
        string Summary { get; }

        /// <summary>
        /// Gets or sets the tab shown, as <see cref="NeutralTabs"/> numbers it.
        /// </summary>
        int SelectedTab { get; set; }

        /// <summary>
        /// Gets the heroes nobody owns, each as a link that opens its page.
        /// </summary>
        IReadOnlyList<ILink> Heroes { get; }

        /// <summary>
        /// Gets whether there is a hero nobody owns.
        /// </summary>
        bool HasHeroes { get; }

        /// <summary>
        /// Gets the towns nobody owns, each as a link that opens its page.
        /// </summary>
        IReadOnlyList<ILink> Castles { get; }

        /// <summary>
        /// Gets whether there is a town nobody owns.
        /// </summary>
        bool HasCastles { get; }
    }
}
