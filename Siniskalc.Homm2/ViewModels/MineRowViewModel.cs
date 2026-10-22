namespace Siniskalc.Homm2.ViewModels
{
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.ViewModels;
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// A mine of the map: what it is, where it is, and who owns it (picked from a list, which changes the record and the flag).
    /// </summary>
    public sealed class MineRowViewModel : MineRowViewModelBase
    {
        public MineRowViewModel(IEditSession<MineModel> session)
            : base(session.Index, () => session.Current.Kind, () => session.Current.Position, OwnerOf(session), hasLevels: false)
        {
        }

        private static ChoiceFieldViewModel OwnerOf(IEditSession<MineModel> session) => new(
            "Owner",
            OwnerNames<PlayerColor>.Choices,
            () => OwnerNames<PlayerColor>.Of(session.Current.Owner),
            name => session.Update(mine => mine with { Owner = OwnerNames<PlayerColor>.Parse(name) }),
            toolTip: "Who owns the mine. The flag on the map and the daily income change with it: the game works the income out at the start of the next day. Not tried in the game.");
    }
}
