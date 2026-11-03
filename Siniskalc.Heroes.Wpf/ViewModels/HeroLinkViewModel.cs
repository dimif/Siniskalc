namespace Siniskalc.Heroes.Wpf.ViewModels
{
    using System.Windows.Input;
    using Siniskalc.Heroes.ViewModels;

    /// <summary>
    /// A hero in a list of a player's heroes. Opening it shows the hero's pages.
    /// </summary>
    public sealed class HeroLinkViewModel : ILink
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HeroLinkViewModel"/> class.
        /// </summary>
        /// <param name="index">The place of the hero among the heroes.</param>
        /// <param name="name">The name of the hero.</param>
        /// <param name="detail">What to say about the hero beside its name.</param>
        /// <param name="open">Opens the page of the hero.</param>
        public HeroLinkViewModel(int index, string name, string detail, ICommand open)
        {
            this.Index = index;
            this.Name = name;
            this.Detail = detail;
            this.OpenCommand = open;
        }

        /// <summary>
        /// Gets the place of the hero among the heroes, which is what opens its page.
        /// </summary>
        public int Index { get; }

        /// <inheritdoc/>
        public string Name { get; }

        /// <inheritdoc/>
        public string Detail { get; }

        /// <inheritdoc/>
        public ICommand OpenCommand { get; }
    }
}
