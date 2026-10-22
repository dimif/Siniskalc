namespace Siniskalc.Homm3.ViewModels
{
    using System;
    using System.Linq;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.ViewModels;
    using Siniskalc.Homm3.Data;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// A mine of the map: what it gives, where its record says it is, who owns it (picked from a list) and the creatures that guard it, which
    /// are edited with the same army editor as a hero's creatures.
    /// </summary>
    public sealed class MineRowViewModel : AbstractViewModelBase
    {
        private readonly MineEditSession session;
        private readonly GameVersion gameVersion;
        private ArmyViewModel? guardEditor;

        public MineRowViewModel(MineEditSession session, GameVersion gameVersion)
        {
            this.session = session;
            this.gameVersion = gameVersion;
            this.OwnerField = new ChoiceFieldViewModel(
                "Owner",
                OwnerNames<PlayerColor>.Choices,
                () => OwnerNames<PlayerColor>.Of(this.session.Current.Owner),
                name => this.session.Update(mine => mine with { Owner = OwnerNames<PlayerColor>.Parse(name) }),
                toolTip: "Who owns the mine. The game works the income out at the start of the next day and draws the flag from the owner. Not tried in the game.");
        }

        public int Index => this.session.Index;

        public string Kind => MineKinds.NameOf(this.session.Current.Kind);

        public string ResourceName => MineKinds.ResourceOf(this.session.Current.Kind).ToString();

        /// <summary>
        /// Gets the place the list is sorted by: the column first, then the row.
        /// </summary>
        public (int X, int Y) Place => (this.session.Current.Position.X, this.session.Current.Position.Y);

        /// <summary>
        /// Gets the tile the record of the mine gives. The mine is entered somewhere else (see <c>MineLayout</c>), so this is not the tile
        /// a hero stands on to take it.
        /// </summary>
        public string PositionText => MapLevelText.Coordinates(this.session.Current.Position.X, this.session.Current.Position.Y);

        /// <summary>
        /// Gets a few words about the guard: none, or how many slots it uses.
        /// </summary>
        public string GuardText
        {
            get
            {
                var count = this.session.Current.Guard.Filled.Count();
                return count == 0 ? "Unguarded" : $"{count} of {Constants.CreatureSlotCount} slots";
            }
        }

        public string OwnerSort => this.OwnerField.Selected;

        public ChoiceFieldViewModel OwnerField { get; }

        /// <summary>
        /// Gets the editor of the guard. It is made when first needed, as it builds the finder of every creature.
        /// </summary>
        public ArmyViewModel GuardEditor => this.guardEditor ??= new ArmyViewModel(
            this.gameVersion,
            () => this.session.Current.Guard,
            edit => this.session.Update(mine => mine with { Guard = edit(mine.Guard) }));

        public void Refresh()
        {
            this.OwnerField.Refresh();
            this.guardEditor?.Refresh();
            this.RaiseAllPropertiesChanged();
        }
    }
}
