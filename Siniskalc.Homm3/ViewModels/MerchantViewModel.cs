namespace Siniskalc.Homm3.ViewModels
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Input;
    using Siniskalc.Core.Data;
    using Siniskalc.Homm3.Data;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Models;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// What a Black Market or the shared town merchants have in store: seven slots, each picked like the artifact of a hero. A slot
    /// that is cleared is a sold one. Call <see cref="Refresh"/> after the stock changed.
    /// </summary>
    public sealed class MerchantViewModel : AbstractViewModelBase
    {
        private readonly IEditSession<MerchantStockModel> session;

        /// <summary>
        /// Initializes a new instance of the <see cref="MerchantViewModel"/> class.
        /// </summary>
        /// <param name="session">The session that edits the stock.</param>
        /// <param name="catalog">The catalog of the artifacts.</param>
        /// <param name="effectLines">Words the effects of artifacts.</param>
        /// <param name="location">Where the merchant is, as text.</param>
        public MerchantViewModel(IEditSession<MerchantStockModel> session, EntityCatalog catalog, EffectLineProvider effectLines, string location = "")
        {
            this.session = session;
            this.ResetCommand = new DelegateCommand(_ => this.session.Update(_ => this.session.Original), _ => this.HasPendingChanges);
            this.Location = location;
            this.Slots = Enumerable.Range(0, MerchantLayout.SlotCount)
                .Select(slot => new ArtifactSlotViewModel(
                    $"Slot {slot + 1}",
                    onChoose => new ArtifactFinderViewModel(catalog, effectLines, string.Empty, "Choose", onChoose, null, includeScrolls: false),
                    name => this.session.Update(stock => stock.WithSlot(slot, name == null ? null : catalog.FindArtifact(name)))))
                .ToList();
            this.Refresh();
        }

        /// <summary>
        /// Gets the name of the merchant.
        /// </summary>
        public string Name => this.session.Current.Name;

        /// <summary>
        /// Gets where the merchant is on the map, or nothing when a hero standing on it hides where it is.
        /// </summary>
        public string Location { get; }

        /// <summary>
        /// Gets whether the place of the merchant is known.
        /// </summary>
        public bool HasLocation => this.Location.Length > 0;

        /// <summary>
        /// Gets the seven slots of the shop, in order.
        /// </summary>
        public IReadOnlyList<ArtifactSlotViewModel> Slots { get; }

        /// <summary>Gets whether this stock differs from the loaded or last-saved stock.</summary>
        public bool HasPendingChanges => this.session.IsDirty;

        /// <summary>Gets the command that restores this stock only, shared by every town that uses it.</summary>
        public ICommand ResetCommand { get; }

        /// <summary>
        /// Reads the model again and tells the view that every value may have changed.
        /// </summary>
        public void Refresh()
        {
            var stock = this.session.Current;
            for (var slot = 0; slot < this.Slots.Count; slot++)
            {
                this.Slots[slot].UpdateFromModel(stock.Slots[slot]?.Name);
            }

            this.RaiseAllPropertiesChanged();
        }
    }
}
