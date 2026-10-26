namespace Siniskalc.Heroes.Wpf.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Input;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// The "send to castle" command of the position card of a hero, in any game: a list, with a box to search it, of the castles the player of
    /// the hero owns. Picking a castle moves the hero to its entrance; a castle whose entrance another hero stands on is in the list but cannot
    /// be picked.
    /// </summary>
    public sealed class SendToCastleViewModel : AbstractViewModelBase
    {
        private readonly Func<IReadOnlyList<CastleDestination>> castles;
        private readonly Action<MapPosition> send;
        private string searchText = string.Empty;
        private bool isOpen;

        /// <param name="castles">Gives the castles the hero can be sent to, as they are now.</param>
        /// <param name="send">Moves the hero to a tile (its whole position, so the hero is moved by one edit).</param>
        public SendToCastleViewModel(Func<IReadOnlyList<CastleDestination>> castles, Action<MapPosition> send)
        {
            this.castles = castles;
            this.send = send;
            this.OpenCommand = new DelegateCommand(_ => this.IsOpen = true, _ => this.HasCastles);
            this.SendCommand = new DelegateCommand(this.Send, parameter => parameter is CastleDestination { IsOccupied: false });
        }

        /// <summary>
        /// Gets a value indicating whether the hero has a castle to be sent to.
        /// </summary>
        public bool HasCastles => this.castles().Count > 0;

        /// <summary>
        /// Gets or sets the text the list is searched with, which keeps the castles whose name, place or faction contains it.
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
                    this.RaisePropertyChanged(nameof(this.Castles));
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
                    this.RaisePropertyChanged(nameof(this.Castles));
                }
            }
        }

        /// <summary>
        /// Gets the castles whose name, place or faction contains the search text, by name.
        /// </summary>
        public IReadOnlyList<CastleDestination> Castles => this.castles()
            .Where(c => c.Matches(this.searchText.Trim()))
            .OrderBy(c => c.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

        /// <summary>
        /// Gets the command that opens the list.
        /// </summary>
        public ICommand OpenCommand { get; }

        /// <summary>
        /// Gets the command that sends the hero to the castle given as its parameter.
        /// </summary>
        public ICommand SendCommand { get; }

        /// <summary>
        /// Shows the castles again. Call after the hero or a castle changed.
        /// </summary>
        public void Refresh() => this.RaiseAllPropertiesChanged();

        private void Send(object? parameter)
        {
            if (parameter is CastleDestination { IsOccupied: false } castle)
            {
                this.send(castle.Entrance);
                this.IsOpen = false;
                this.SearchText = string.Empty;
            }
        }
    }
}
