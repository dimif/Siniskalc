namespace Siniskalc.Heroes.Wpf.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Input;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// The "send to" command of the position card of a hero, in any game: a list, with a box to search it, of the castles, mines and dwellings the player of
    /// the hero owns. Picking one moves the hero to its entrance; a place whose tile another hero stands on is in the list but cannot
    /// be picked.
    /// </summary>
    public sealed class SendToViewModel : AbstractViewModelBase
    {
        private readonly Func<IReadOnlyList<MapDestination>> destinations;
        private readonly Action<MapPosition> send;
        private string searchText = string.Empty;
        private bool isOpen;

        /// <param name="destinations">Gives the places the hero can be sent to, as they are now.</param>
        /// <param name="send">Moves the hero to a tile (its whole position, so the hero is moved by one edit).</param>
        public SendToViewModel(Func<IReadOnlyList<MapDestination>> destinations, Action<MapPosition> send)
        {
            this.destinations = destinations;
            this.send = send;
            this.OpenCommand = new DelegateCommand(_ => this.IsOpen = true, _ => this.HasDestinations);
            this.SendCommand = new DelegateCommand(this.Send, parameter => parameter is MapDestination { IsOccupied: false });
        }

        /// <summary>
        /// Gets a value indicating whether the hero has a place to be sent to.
        /// </summary>
        public bool HasDestinations => this.destinations().Count > 0;

        /// <summary>
        /// Gets or sets the text the list is searched with, which keeps the places whose name, kind, place or faction contains it.
        /// </summary>
        public string SearchText
        {
            get => this.searchText;
            set
            {
                if (value != this.searchText)
                {
                    this.searchText = value;
                    this.RaisePropertyChanged(nameof(this.SearchText));
                    this.RaisePropertyChanged(nameof(this.Destinations));
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the list is open.
        /// </summary>
        public bool IsOpen
        {
            get => this.isOpen;
            set
            {
                if (value != this.isOpen)
                {
                    this.isOpen = value;
                    this.RaisePropertyChanged(nameof(this.IsOpen));
                    this.RaisePropertyChanged(nameof(this.Destinations));
                }
            }
        }

        /// <summary>
        /// Gets the places whose name, kind, place or faction contains the search text, by name.
        /// </summary>
        public IReadOnlyList<MapDestination> Destinations => this.destinations()
            .Where(c => c.Matches(this.searchText.Trim()))
            .OrderBy(c => c.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

        /// <summary>
        /// Gets the command that opens the list.
        /// </summary>
        public ICommand OpenCommand { get; }

        /// <summary>
        /// Gets the command that sends the hero to the place given as its parameter.
        /// </summary>
        public ICommand SendCommand { get; }

        /// <summary>
        /// Shows the places again. Call after the hero or a place changed.
        /// </summary>
        public void Refresh() => this.RaiseAllPropertiesChanged();

        private void Send(object? parameter)
        {
            if (parameter is MapDestination { IsOccupied: false } place)
            {
                this.send(place.Entrance);
                this.IsOpen = false;
                this.SearchText = string.Empty;
            }
        }
    }
}
