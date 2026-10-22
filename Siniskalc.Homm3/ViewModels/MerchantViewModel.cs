namespace Siniskalc.Homm3.ViewModels
{
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Homm3.Data;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Models;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// What a merchant (the artifact merchants, the black market) has in store: seven slots, each picked like the artifact of a hero. A slot
    /// that is cleared is a sold one. Call <see cref="Refresh"/> after the stock changed.
    /// </summary>
    public sealed class MerchantViewModel : AbstractViewModelBase
    {
        private readonly IEditSession<MerchantStockModel> session;

        public MerchantViewModel(IEditSession<MerchantStockModel> session, GameVersion gameVersion, EffectLineProvider effectLines)
        {
            this.session = session;
            this.Slots = Enumerable.Range(0, MerchantLayout.SlotCount)
                .Select(slot => new ArtifactSlotViewModel(
                    $"Slot {slot + 1}",
                    onChoose => new ArtifactFinderViewModel(gameVersion, effectLines, string.Empty, "Choose", onChoose, null, includeScrolls: false),
                    name => this.session.Update(stock => stock.WithSlot(slot, name == null ? null : Artifact.FindByName(name)))))
                .ToList();
            this.Refresh();
        }

        public string Name => this.session.Current.Name;

        /// <summary>
        /// Gets the seven slots of the shop, in order.
        /// </summary>
        public IReadOnlyList<ArtifactSlotViewModel> Slots { get; }

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
