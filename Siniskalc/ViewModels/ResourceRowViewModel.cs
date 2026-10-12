namespace Siniskalc.ViewModels
{
    using System;
    using System.Globalization;
    using Siniskalc.Core;
    using Siniskalc.Entities;
    using Siniskalc.Models;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// One resource of a player: its amount, editable, and how far it is from what the savegame had.
    /// </summary>
    public sealed class ResourceRowViewModel : AbstractViewModelBase
    {
        private readonly Func<PlayerResources> current;
        private readonly Func<PlayerResources> original;
        private readonly Action<Func<PlayerResources, PlayerResources>> update;

        public ResourceRowViewModel(
            Resource resource,
            Func<PlayerResources> current,
            Func<PlayerResources> original,
            Action<Func<PlayerResources, PlayerResources>> update)
        {
            this.Resource = resource;
            this.current = current;
            this.original = original;
            this.update = update;
        }

        public Resource Resource { get; }

        public string Name => this.Resource.ToString();

        /// <summary>
        /// Gets the first two letters, shown on the resource's badge.
        /// </summary>
        public string Badge => this.Name[..2];

        public string ColorHex => PlayerPalette.Of(this.Resource);

        public bool IsGold => this.Resource == Resource.Gold;

        public long Maximum => this.IsGold ? Constants.Resources.MaxGold : Constants.Resources.MaxOther;

        /// <summary>
        /// Gets how much one press of the buttons adds or removes.
        /// </summary>
        public long Step => this.IsGold ? Constants.Resources.GoldStep : Constants.Resources.OtherStep;

        public long Value
        {
            get => this.current()[this.Resource];
            set
            {
                var amount = (int)RangeGuard.Clamp(value, Constants.Resources.Min, this.Maximum);
                this.update(resources => resources.With(this.Resource, amount));
            }
        }

        public bool IsChanged => this.Value != this.original()[this.Resource];

        /// <summary>
        /// Gets how far the amount is from the savegame's, such as "+17 from save", or nothing when it is the same.
        /// </summary>
        public string DeltaText
        {
            get
            {
                var delta = this.Value - this.original()[this.Resource];
                return delta == 0 ? string.Empty : $"{(delta > 0 ? "+" : string.Empty)}{delta.ToString("N0", CultureInfo.CurrentCulture)} from save";
            }
        }

        public void Refresh()
        {
            this.RaiseAllPropertiesChanged();
        }
    }
}
