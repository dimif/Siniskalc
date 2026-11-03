namespace Siniskalc.Heroes.ViewModels
{
    using System.Windows.Input;

    /// <summary>
    /// A hero or a town in the card of a player: the name opens its pages, the detail sits beside it.
    /// </summary>
    public sealed class LinkViewModel : ILink
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LinkViewModel"/> class.
        /// </summary>
        /// <param name="name">The text of the link.</param>
        /// <param name="detail">The line under the name.</param>
        /// <param name="open">What the link does when pressed.</param>
        public LinkViewModel(string name, string detail, ICommand open)
        {
            this.Name = name;
            this.Detail = detail;
            this.OpenCommand = open;
        }

        /// <summary>
        /// Gets the text of the link.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the line under the name.
        /// </summary>
        public string Detail { get; }

        /// <summary>
        /// Gets the command that opens what the link points to.
        /// </summary>
        public ICommand OpenCommand { get; }
    }
}
