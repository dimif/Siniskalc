namespace Siniskalc.Homm3.ViewModels
{
    using System.Globalization;
    using Siniskalc.Core;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.ViewModels;
    using Siniskalc.Homm3.Data;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// A dwelling of the map: the creature it gives, where it is entered, who owns it (picked from a list) and how many creatures can be hired
    /// now (typed).
    /// </summary>
    public sealed class MapDwellingRowViewModel : AbstractViewModelBase
    {
        private readonly DwellingEditSession session;

        public MapDwellingRowViewModel(DwellingEditSession session)
        {
            this.session = session;
            this.OwnerField = new ChoiceFieldViewModel(
                "Owner",
                OwnerNames<Siniskalc.Homm3.Entities.PlayerColor>.Choices,
                () => OwnerNames<Siniskalc.Homm3.Entities.PlayerColor>.Of(this.session.Current.Owner),
                name => this.session.Update(dwelling => dwelling with { Owner = OwnerNames<Siniskalc.Homm3.Entities.PlayerColor>.Parse(name) }),
                toolTip: "Who owns the dwelling. Not tried in the game.");
        }

        public int Index => this.session.Index;

        public string CreatureName => this.session.Current.Creature?.Name ?? $"Creature {this.session.Current.CreatureCode}";

        public int X => this.session.Current.Position.X;

        public int Y => this.session.Current.Position.Y;

        /// <summary>
        /// Gets the tile the savegame gives for the dwelling. A hero stood on it to flag the dwellings of the Castle and the Rampart of the test
        /// map, and 3 tiles lower to visit the neutral ones in the row above, so it is only where the dwelling is, give or take its entrance.
        /// </summary>
        /// <summary>
        /// Gets the tile as the other lists show it, such as "X: 31, Y: 56".
        /// </summary>
        public string PositionText => MapLevelText.Coordinates(this.session.Current.Position.X, this.session.Current.Position.Y);

        public long Stock => this.session.Current.Stock;

        /// <summary>
        /// Gets or sets the creatures that can be hired now, as the player types them: a number that is not one, or is out of range, is
        /// taken as the nearest the savegame can hold.
        /// </summary>
        public string StockText
        {
            get => this.session.Current.Stock.ToString(CultureInfo.CurrentCulture);
            set
            {
                if (long.TryParse(value, NumberStyles.Integer, CultureInfo.CurrentCulture, out var stock))
                {
                    var clamped = (long)RangeGuard.Clamp(stock, 0, Constants.Army.MaxStackSize);
                    if (clamped != this.session.Current.Stock)
                    {
                        this.session.Update(dwelling => dwelling with { Stock = clamped });
                    }
                }

                this.RaisePropertyChanged(nameof(this.StockText));
            }
        }

        public string OwnerSort => this.OwnerField.Selected;

        public ChoiceFieldViewModel OwnerField { get; }

        public void Refresh()
        {
            this.OwnerField.Refresh();
            this.RaiseAllPropertiesChanged();
        }
    }
}
