namespace Siniskalc.Heroes.Wpf.ViewModels
{
    using System.Collections.Generic;
    using Siniskalc.Heroes.ViewModels;

    /// <summary>
    /// The page of a castle in any game: its name and colour, the heroes in it, its owner, and why the owner could not be changed.
    /// </summary>
    public interface ICastlePage : IResettablePage, IOwnerChoice
    {
        string Name { get; }

        string ColorHex { get; }

        int SelectedTab { get; set; }

        /// <summary>
        /// Gets the heroes in the castle, each with its role (garrisoned, visiting) as the detail.
        /// </summary>
        IReadOnlyList<CastleHeroRowViewModel> Heroes { get; }

        /// <summary>
        /// Gets why an edit of the owner was refused, or nothing.
        /// </summary>
        string Notice { get; }

        bool HasNotice { get; }
    }
}
