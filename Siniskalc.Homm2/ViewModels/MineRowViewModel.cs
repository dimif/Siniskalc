namespace Siniskalc.Homm2.ViewModels
{
    using Siniskalc.Core.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    /// <summary>A mine row whose presentation and resource come from the document's catalog.</summary>
    public sealed class MineRowViewModel : Siniskalc.Heroes.Wpf.ViewModels.MineRowViewModel<MineModel, PlayerColor>
    {
        private readonly GameCatalog catalog;

        /// <summary>
        /// Initializes a new instance of the <see cref="MineRowViewModel"/> class.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="catalog">The catalog.</param>
        /// <param name="ownerToolTip">The owner tool tip.</param>
        public MineRowViewModel(IEditSession<MineModel> session, GameCatalog catalog, string ownerToolTip)
            : base(session, (mine, color) => mine with { Owner = color }, ownerToolTip, hasLevels: false)
        {
            this.catalog = catalog;
        }

        /// <inheritdoc/>
        public override string Kind => this.catalog.Mines.Find((byte)this.Session.Current.Kind)!.Name;

        /// <inheritdoc/>
        public override string ResourceName => this.catalog.Mines.Find((byte)this.Session.Current.Kind)!.Resource.ToString();
    }
}
