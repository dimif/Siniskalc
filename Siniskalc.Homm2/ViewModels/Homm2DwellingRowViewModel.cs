namespace Siniskalc.Homm2.ViewModels
{
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// One of the six dwellings of a castle: whether it is built, and how many creatures wait in it.
    /// </summary>
    public sealed class Homm2DwellingRowViewModel
    {
        public Homm2DwellingRowViewModel(ToggleRowViewModel built, AmountRowViewModel waiting)
        {
            this.Built = built;
            this.Waiting = waiting;
        }

        public ToggleRowViewModel Built { get; }

        public AmountRowViewModel Waiting { get; }

        public void Refresh()
        {
            this.Built.Refresh();
            this.Waiting.Refresh();
        }
    }
}
