namespace Siniskalc.Homm2.ViewModels
{
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.ViewModels;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// A mine of the map: what it is, where it is, and who owns it (picked from a list, which changes the record and the flag).
    /// </summary>
    public sealed class MineRowViewModel : AbstractViewModelBase
    {
        private readonly MapObjectEditSession<MineModel> session;

        public MineRowViewModel(MapObjectEditSession<MineModel> session)
        {
            this.session = session;
            this.OwnerField = new ChoiceFieldViewModel(
                "Owner",
                OwnerNames<Color>.Choices,
                () => OwnerNames<Color>.Of(this.session.Current.Owner),
                name => this.session.Update(mine => mine with { Owner = OwnerNames<Color>.Parse(name) }),
                toolTip: "Who owns the mine. The flag on the map and the daily income change with it: the game works the income out at the start of the next day. Not tried in the game.");
        }

        public int Index => this.session.Index;

        public string Kind => MineKinds.NameOf(this.session.Current.Kind);

        public string ResourceName => MineKinds.ResourceOf(this.session.Current.Kind).ToString();

        public string PositionText => MapLevelText.Coordinates(this.session.Current.Position.X, this.session.Current.Position.Y);

        public ChoiceFieldViewModel OwnerField { get; }

        public void Refresh()
        {
            this.OwnerField.Refresh();
            this.RaiseAllPropertiesChanged();
        }
    }
}
