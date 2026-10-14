namespace Siniskalc.Homm2.ViewModels
{
    using System.Windows.Input;

    /// <summary>
    /// A hero or a town in the card of a player: the name opens its pages, the detail sits beside it.
    /// </summary>
    public sealed class LinkViewModel
    {
        public LinkViewModel(string name, string detail, ICommand open)
        {
            this.Name = name;
            this.Detail = detail;
            this.OpenCommand = open;
        }

        public string Name { get; }

        public string Detail { get; }

        public ICommand OpenCommand { get; }
    }
}
