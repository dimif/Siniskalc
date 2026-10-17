namespace Siniskalc.Heroes.Wpf.ViewModels
{
    using System.Collections.Generic;
    using System.Windows.Input;
    using Siniskalc.Heroes.ViewModels;

    /// <summary>
    /// A castle in a player's card of the overview or on its page. Opening it shows the castle's page.
    /// </summary>
    public sealed class CastleLinkViewModel : ILink
    {
        /// <param name="name">The name of the castle.</param>
        /// <param name="detail">What is told on the right of the name, such as the faction.</param>
        /// <param name="open">Opens the page of the castle.</param>
        /// <param name="status">What is under the name, such as "City · Citadel · 3 dwellings".</param>
        /// <param name="isCapital">Whether the castle is a capital, which marks it.</param>
        /// <param name="grailText">The name of the Grail structure that is built, or nothing.</param>
        /// <param name="heroes">The heroes in the castle.</param>
        public CastleLinkViewModel(string name, string detail, ICommand open, string status = "", bool isCapital = false, string grailText = "", IReadOnlyList<CastleHeroRowViewModel>? heroes = null)
        {
            this.Name = name;
            this.Detail = detail;
            this.OpenCommand = open;
            this.Status = status;
            this.IsCapital = isCapital;
            this.GrailText = grailText;
            this.Heroes = heroes ?? [];
        }

        public string Name { get; }

        public string Detail { get; }

        /// <summary>
        /// Gets what is told under the name: the hall and the fortification of a settlement, the Mage Guild and how many dwellings are built.
        /// </summary>
        public string Status { get; }

        /// <summary>
        /// Gets whether the castle is a capital, which is to say it has a Capitol.
        /// </summary>
        public bool IsCapital { get; }

        /// <summary>
        /// Gets the name of the Grail structure that is built, e.g. "Colossus", or nothing.
        /// </summary>
        public string GrailText { get; }

        public bool HasGrail => this.GrailText.Length > 0;

        /// <summary>
        /// Gets the heroes in the castle, each as a link to the hero.
        /// </summary>
        public IReadOnlyList<CastleHeroRowViewModel> Heroes { get; }

        public ICommand OpenCommand { get; }
    }
}
