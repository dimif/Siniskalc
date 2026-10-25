namespace Siniskalc.Heroes.Wpf.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Input;
    using Siniskalc.Core;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.ViewModels;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// The card of a point on the map that a hero has, in any game: where the hero stands (with the direction it faces) or where its marker is.
    /// The level of the map, the facing and the button that takes the point away are shown only when the game gives the card what they need.
    /// Use <see cref="ForPosition"/> and <see cref="ForDestination"/> to make the cards of a hero.
    /// </summary>
    public sealed class MapPointCardViewModel : AbstractViewModelBase
    {
        private readonly Func<int> getX;
        private readonly Action<int> setX;
        private readonly Func<int> getY;
        private readonly Action<int> setY;
        private readonly Func<int>? getLevel;
        private readonly Action<int>? setLevel;
        private readonly Func<IReadOnlyList<MapLevelChoice>>? levelChoices;
        private readonly Func<FacingDirection>? getFacing;
        private readonly Action<FacingDirection>? setFacing;

        /// <summary>
        /// Initializes a new instance of the <see cref="MapPointCardViewModel"/> class.
        /// </summary>
        /// <param name="heading">The heading of the card, such as "Position" or "Marker".</param>
        /// <param name="getX">Reads the column.</param>
        /// <param name="setX">Writes it.</param>
        /// <param name="getY">Reads the row.</param>
        /// <param name="setY">Writes it.</param>
        /// <param name="coordinates">The limits of a column and a row.</param>
        /// <param name="getLevel">Reads the level of the map, or <c>null</c> in a game whose map has one level.</param>
        /// <param name="setLevel">Writes it.</param>
        /// <param name="levelChoices">The levels to pick from.</param>
        /// <param name="getFacing">Reads the direction the hero faces, or <c>null</c> for a point that has none.</param>
        /// <param name="setFacing">Writes it.</param>
        /// <param name="remove">Takes the point away, or <c>null</c> for a point that cannot be taken away.</param>
        /// <param name="isSet">Tells whether there is a point to take away.</param>
        /// <param name="removeToolTip">What the remove button says it does.</param>
        /// <param name="sendToCastle">The command that moves the hero to a castle of its player, or <c>null</c> for a card that has none.</param>
        public MapPointCardViewModel(
            string heading,
            Func<int> getX,
            Action<int> setX,
            Func<int> getY,
            Action<int> setY,
            NumberRange coordinates,
            Func<int>? getLevel = null,
            Action<int>? setLevel = null,
            Func<IReadOnlyList<MapLevelChoice>>? levelChoices = null,
            Func<FacingDirection>? getFacing = null,
            Action<FacingDirection>? setFacing = null,
            Action? remove = null,
            Func<bool>? isSet = null,
            string removeToolTip = "",
            SendToCastleViewModel? sendToCastle = null)
        {
            this.SendToCastle = sendToCastle;
            this.Heading = heading;
            this.getX = getX;
            this.setX = setX;
            this.getY = getY;
            this.setY = setY;
            this.Coordinates = coordinates;
            this.getLevel = getLevel;
            this.setLevel = setLevel;
            this.levelChoices = levelChoices;
            this.getFacing = getFacing;
            this.setFacing = setFacing;
            this.RemoveToolTip = removeToolTip;
            this.RotateLeftCommand = new DelegateCommand(_ => this.Rotate(-1));
            this.RotateRightCommand = new DelegateCommand(_ => this.Rotate(1));
            this.RemoveCommand = new DelegateCommand(_ => remove?.Invoke(), _ => remove != null && (isSet?.Invoke() ?? true));
            this.CanRemove = remove != null;
        }

        public string Heading { get; }

        /// <summary>
        /// Gets the command that moves the hero to a castle of its player, or <c>null</c> when the card has none.
        /// </summary>
        public SendToCastleViewModel? SendToCastle { get; }

        /// <summary>
        /// Gets a value indicating whether the card has the send to castle command.
        /// </summary>
        public bool HasSendToCastle => this.SendToCastle != null;

        public NumberRange Coordinates { get; }

        public int X
        {
            get => this.getX();
            set => SetIfChanged(this.X, (int)RangeGuard.Clamp<long>(value, this.Coordinates.Minimum, this.Coordinates.Maximum), this.setX);
        }

        public int Y
        {
            get => this.getY();
            set => SetIfChanged(this.Y, (int)RangeGuard.Clamp<long>(value, this.Coordinates.Minimum, this.Coordinates.Maximum), this.setY);
        }

        /// <summary>
        /// Gets a value indicating whether the map of the game has levels (a surface and an underground).
        /// </summary>
        public bool HasLevel => this.getLevel != null;

        public int Level
        {
            get => this.getLevel?.Invoke() ?? MapPosition.Above;
            set
            {
                if (this.setLevel != null)
                {
                    SetIfChanged(this.Level, value, this.setLevel);
                }
            }
        }

        public IReadOnlyList<MapLevelChoice> LevelChoices => this.levelChoices?.Invoke() ?? [];

        /// <summary>
        /// Gets a value indicating whether the point has a direction.
        /// </summary>
        public bool HasFacing => this.getFacing != null;

        public FacingDirection Facing
        {
            get => this.getFacing?.Invoke() ?? FacingDirection.North;
            set
            {
                if (this.setFacing != null)
                {
                    SetIfChanged(this.Facing, value, this.setFacing);
                }
            }
        }

        public Array FacingChoices => Enum.GetValues(typeof(FacingDirection));

        public ICommand RotateLeftCommand { get; }

        public ICommand RotateRightCommand { get; }

        /// <summary>
        /// Gets a value indicating whether the card has a button that takes the point away.
        /// </summary>
        public bool CanRemove { get; }

        public ICommand RemoveCommand { get; }

        public string RemoveToolTip { get; }

        /// <summary>
        /// Makes the card of where a hero stands, with the direction it faces when the game gives one.
        /// </summary>
        /// <param name="get">Reads the position of the hero.</param>
        /// <param name="set">Writes it.</param>
        /// <param name="coordinates">The limits of a column and a row.</param>
        /// <param name="levelChoices">Gives the levels of the map to pick from for the level the point is on now, or <c>null</c> in a game whose map has one level.</param>
        /// <param name="getFacing">Reads the direction the hero faces, or <c>null</c> when the game has none.</param>
        /// <param name="setFacing">Writes it.</param>
        /// <param name="castles">Gives the castles of the player of the hero it can be sent to, or <c>null</c> for a hero that cannot be.</param>
        public static MapPointCardViewModel ForPosition(
            Func<MapPosition> get,
            Action<MapPosition> set,
            NumberRange coordinates,
            Func<int, IReadOnlyList<MapLevelChoice>>? levelChoices = null,
            Func<FacingDirection>? getFacing = null,
            Action<FacingDirection>? setFacing = null,
            Func<IReadOnlyList<CastleDestination>>? castles = null)
        {
            return Build("Position", get, set, coordinates, levelChoices, getFacing, setFacing, takeAway: false, castles is null ? null : new SendToCastleViewModel(castles, entrance => set(get() with { X = entrance.X, Y = entrance.Y, Level = entrance.Level })));
        }

        /// <summary>
        /// Makes the card of the destination of a hero: the tile it is going to, which the game marks on the map. It can be taken away, which
        /// sets every part of it to -1 (the game's "none").
        /// </summary>
        /// <param name="get">Reads the destination, <see cref="MapPosition.None"/> when there is none.</param>
        /// <param name="set">Writes it.</param>
        /// <param name="coordinates">The limits of a column and a row; the lowest is -1 for none.</param>
        /// <param name="levelChoices">Gives the levels of the map to pick from for the level the point is on now, or <c>null</c> in a game whose map has one level.</param>
        public static MapPointCardViewModel ForDestination(
            Func<MapPosition> get,
            Action<MapPosition> set,
            NumberRange coordinates,
            Func<int, IReadOnlyList<MapLevelChoice>>? levelChoices = null)
        {
            return Build("Destination", get, set, coordinates, levelChoices, null, null, takeAway: true);
        }

        /// <summary>
        /// Shows the hero again. Call after the hero changed.
        /// </summary>
        public void Refresh()
        {
            this.RaiseAllPropertiesChanged();
            this.SendToCastle?.Refresh();
        }

        private static MapPointCardViewModel Build(
            string heading,
            Func<MapPosition> get,
            Action<MapPosition> set,
            NumberRange coordinates,
            Func<int, IReadOnlyList<MapLevelChoice>>? levelChoices,
            Func<FacingDirection>? getFacing,
            Action<FacingDirection>? setFacing,
            bool takeAway,
            SendToCastleViewModel? sendToCastle = null)
        {
            return new MapPointCardViewModel(
                heading,
                () => get().X,
                x => set(get() with { X = x }),
                () => get().Y,
                y => set(get() with { Y = y }),
                coordinates,
                levelChoices is null ? null : () => get().Level,
                levelChoices is null ? null : level => set(get() with { Level = level }),
                levelChoices is null ? null : () => levelChoices(get().Level),
                getFacing,
                setFacing,
                takeAway ? () => set(MapPosition.None) : null,
                takeAway ? () => get() != MapPosition.None : null,
                takeAway ? $"Take the {heading.ToLowerInvariant()} away: sets its X, Y{(levelChoices is null ? string.Empty : " and level")} to -1" : string.Empty,
                sendToCastle);
        }

        private static void SetIfChanged<T>(T current, T value, Action<T> set)
        {
            if (!EqualityComparer<T>.Default.Equals(current, value))
            {
                set(value);
            }
        }

        private void Rotate(int turns)
        {
            var count = Enum.GetValues<FacingDirection>().Length;
            this.Facing = (FacingDirection)(((int)this.Facing + turns + count) % count);
        }
    }
}
