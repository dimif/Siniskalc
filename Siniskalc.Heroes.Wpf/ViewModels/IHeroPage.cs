namespace Siniskalc.Heroes.Wpf.ViewModels
{
    using Siniskalc.Heroes.ViewModels;

    /// <summary>
    /// The page of a hero in any game: its name and colour, a line under the name, the tab shown, its owner, and Reset. A game's page adds
    /// what only that game has (HoMM3: the backpack, the war machines and the class). Visited places and next-battle effects use shared cards.
    /// </summary>
    public interface IHeroPage : IResettablePage, IOwnerChoice
    {
        /// <summary>
        /// Gets the name of the hero.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the colour of the player that owns the hero, as a hex string.
        /// </summary>
        string ColorHex { get; }

        /// <summary>
        /// Gets the line under the name: what the hero is (its class or faction), its level and where it is.
        /// </summary>
        string SubtitleText { get; }

        /// <summary>
        /// Gets or sets the tab shown, counted from 0.
        /// </summary>
        int SelectedTab { get; set; }
    }
}
