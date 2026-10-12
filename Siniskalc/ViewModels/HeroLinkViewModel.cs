namespace Siniskalc.ViewModels
{
    using System.Windows.Input;
    using Siniskalc.Models;

    /// <summary>
    /// A hero in a player's card of the overview. Opening it shows the hero's pages.
    /// </summary>
    public sealed class HeroLinkViewModel
    {
        public HeroLinkViewModel(HeroSummary summary, ICommand open)
        {
            this.Name = summary.Name;
            this.Detail = summary.Description;
            this.OpenCommand = open;
            this.Index = summary.Index;
        }

        public int Index { get; }

        public string Name { get; }

        public string Detail { get; }

        public ICommand OpenCommand { get; }
    }
}
