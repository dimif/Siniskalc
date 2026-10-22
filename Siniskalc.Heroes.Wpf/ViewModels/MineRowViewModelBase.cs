namespace Siniskalc.Heroes.Wpf.ViewModels
{
    using System;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.ViewModels;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// A mine of the map in any game, as a row of the mines card (<c>MinesCard</c>): what it gives, where it is, and who owns it (picked from a
    /// list). A game derives from it to add what only it has, such as the guard of a mine, and says it in <see cref="Summary"/>.
    /// </summary>
    public class MineRowViewModelBase : AbstractViewModelBase
    {
        private readonly Func<MineKind> kind;
        private readonly Func<MapPosition> position;

        /// <param name="index">The place of the mine among the mines of the savegame.</param>
        /// <param name="kind">Gives the kind of the mine.</param>
        /// <param name="position">Gives the tile of the mine.</param>
        /// <param name="ownerField">The list the owner is picked from.</param>
        public MineRowViewModelBase(int index, Func<MineKind> kind, Func<MapPosition> position, ChoiceFieldViewModel ownerField)
        {
            this.Index = index;
            this.kind = kind;
            this.position = position;
            this.OwnerField = ownerField;
        }

        public int Index { get; }

        public string Kind => MineKinds.NameOf(this.kind());

        public string ResourceName => MineKinds.ResourceOf(this.kind()).ToString();

        public int X => this.position().X;

        public int Y => this.position().Y;

        /// <summary>
        /// Gets the place the list is sorted by: the column first, then the row.
        /// </summary>
        public (int X, int Y) Place => (this.X, this.Y);

        public string PositionText => MapLevelText.Coordinates(this.X, this.Y);

        public string OwnerSort => this.OwnerField.Selected;

        public ChoiceFieldViewModel OwnerField { get; }

        /// <summary>
        /// Gets a few words shown after the owner, such as what guards the mine; nothing for a game that has nothing to add.
        /// </summary>
        public virtual string Summary => string.Empty;

        public virtual void Refresh()
        {
            this.OwnerField.Refresh();
            this.RaiseAllPropertiesChanged();
        }
    }
}
