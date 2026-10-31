namespace Siniskalc.Heroes.Wpf.ViewModels
{
    using System;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.ViewModels;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// A mine of the map in any game, as a row of the mines card (<c>MinesCard</c>) and of <see cref="MinesListViewModel{TRow}"/>: what it gives,
    /// where it is, and who owns it (picked from a list). <see cref="MineRowViewModel{TMine, TColor}"/> reads it from the savegame; a game derives
    /// from that to add what only it has, such as the guard of a mine, and says it in <see cref="Summary"/>.
    /// </summary>
    public abstract class MineRowViewModelBase : AbstractViewModelBase
    {
        private readonly Func<MineKind> kind;
        private readonly Func<MapPosition> position;
        private readonly Func<bool> isOwned;

        /// <param name="index">The place of the mine among the mines of the savegame.</param>
        /// <param name="kind">Gives the kind of the mine.</param>
        /// <param name="position">Gives the tile of the mine.</param>
        /// <param name="isOwned">Gives whether a player owns the mine.</param>
        /// <param name="ownerField">The list the owner is picked from.</param>
        /// <param name="hasLevels">Whether the map of the game has two levels, so a place on it says which level it is on.</param>
        protected MineRowViewModelBase(int index, Func<MineKind> kind, Func<MapPosition> position, Func<bool> isOwned, ChoiceFieldViewModel ownerField, bool hasLevels)
        {
            this.Index = index;
            this.kind = kind;
            this.position = position;
            this.isOwned = isOwned;
            this.OwnerField = ownerField;
            this.HasLevel = hasLevels;
        }

        public int Index { get; }

        /// <summary>
        /// Gets a value indicating whether the map has two levels, so the level of the mine is shown with its place.
        /// </summary>
        public bool HasLevel { get; }

        public string Kind => MineKinds.NameOf(this.kind());

        public string ResourceName => MineKinds.ResourceOf(this.kind()).ToString();

        public int X => this.position().X;

        public int Y => this.position().Y;

        /// <summary>
        /// Gets the level of the map the mine is on (<see cref="MapPosition.Above"/> in a game whose map has one level).
        /// </summary>
        public int Level => this.position().Level;

        /// <summary>
        /// Gets the level of the map the mine is on, such as "Underground", or nothing in a game whose map has one level.
        /// </summary>
        public string LevelText => this.HasLevel ? MapLevelText.Of(this.Level) : string.Empty;

        /// <summary>
        /// Gets the place the list is sorted by: the column first, then the row.
        /// </summary>
        public (int X, int Y) Place => (this.X, this.Y);

        /// <summary>
        /// Gets where the mine is, such as "Pos: 12, 7", with the level in a game whose map has two ("Pos: 12, 7, Underground").
        /// </summary>
        public string PositionText => MapLevelText.Coordinates(this.position(), this.HasLevel);

        public string OwnerSort => this.OwnerField.Selected;

        /// <summary>
        /// Gets a value indicating whether a player owns the mine.
        /// </summary>
        public bool IsOwned => this.isOwned();

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
