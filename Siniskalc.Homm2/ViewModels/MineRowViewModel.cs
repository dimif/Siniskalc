namespace Siniskalc.Homm2.ViewModels
{
    using Siniskalc.Core.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    /// <summary>A mine row whose presentation and resource come from the document's catalog.</summary>
    public sealed class MineRowViewModel : Siniskalc.Heroes.Wpf.ViewModels.MineRowViewModel<MineModel, PlayerColor>
    {
        private readonly GameCatalog catalog;

        public MineRowViewModel(IEditSession<MineModel> session, GameCatalog catalog, string ownerToolTip)
            : base(session, (mine, color) => mine with { Owner = color }, ownerToolTip, hasLevels: false)
        {
            this.catalog = catalog;
        }

        public override string Kind => this.catalog.Mines.Find((byte)this.Session.Current.Kind)!.Name;

        public override string ResourceName => this.catalog.Mines.Find((byte)this.Session.Current.Kind)!.Resource.ToString();
    }
}
