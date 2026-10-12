namespace Siniskalc.ViewModels
{
    using System;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// One obelisk of the map: its name, where it is and whether the player has visited it.
    /// </summary>
    public sealed class ObeliskRowViewModel : AbstractViewModelBase
    {
        private readonly Func<bool> getVisited;
        private readonly Action<bool> setVisited;

        public ObeliskRowViewModel(int number, string coordinates, Func<bool> getVisited, Action<bool> setVisited)
        {
            this.Number = number;
            this.Coordinates = coordinates;
            this.getVisited = getVisited;
            this.setVisited = setVisited;
        }

        public int Number { get; }

        public string Label => $"Obelisk {this.Number}";

        /// <summary>
        /// Gets where the obelisk is on the map, such as "34, 58", or nothing when the savefile does not tell.
        /// </summary>
        public string Coordinates { get; }

        public bool HasCoordinates => this.Coordinates.Length > 0;

        public bool IsVisited
        {
            get => this.getVisited();
            set
            {
                if (value != this.getVisited())
                {
                    this.setVisited(value);
                }
            }
        }

        public void Refresh() => this.RaiseAllPropertiesChanged();
    }
}
