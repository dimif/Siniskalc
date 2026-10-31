namespace Siniskalc.Heroes.Wpf.ViewModels
{
    using System;
    using System.Collections.Generic;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.ViewModels;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// One obelisk of the map, as a row of the obelisks list: its name, where it is and whether the player has visited it (picked from Yes and No).
    /// </summary>
    public sealed class ObeliskRowViewModel : AbstractViewModelBase
    {
        /// <summary>
        /// The words the visited column offers.
        /// </summary>
        public static readonly IReadOnlyList<string> VisitedChoices = ["Yes", "No"];

        private readonly Func<bool> getVisited;
        private readonly Action<bool> setVisited;
        private readonly bool hasLevels;

        /// <param name="number">The place of the obelisk among those of the map, counted from 1.</param>
        /// <param name="position">Where the obelisk is, or null when the savefile does not tell.</param>
        /// <param name="hasLevels">Whether the map of the game has two levels, so the place says which one.</param>
        /// <param name="getVisited">Gives whether the player has visited the obelisk.</param>
        /// <param name="setVisited">Marks the obelisk as visited or not.</param>
        public ObeliskRowViewModel(int number, MapPosition? position, bool hasLevels, Func<bool> getVisited, Action<bool> setVisited)
        {
            this.Number = number;
            this.Position = position;
            this.hasLevels = hasLevels;
            this.getVisited = getVisited;
            this.setVisited = setVisited;
        }

        public int Number { get; }

        public string Label => $"Obelisk {this.Number}";

        /// <summary>
        /// Gets where the obelisk is, or null when the savefile does not tell.
        /// </summary>
        public MapPosition? Position { get; }

        /// <summary>
        /// Gets the column of the obelisk on the map, or nothing when the savefile does not tell where it is.
        /// </summary>
        public int? X => this.Position?.X;

        /// <summary>
        /// Gets the row of the obelisk on the map, or nothing when the savefile does not tell where it is.
        /// </summary>
        public int? Y => this.Position?.Y;

        /// <summary>
        /// Gets the level of the map the obelisk is on, such as "Underground", or nothing in a game whose map has one.
        /// </summary>
        public string LevelText => this.hasLevels && this.Position is { } at ? MapLevelText.Of(at.Level) : string.Empty;

        /// <summary>
        /// Gets where the obelisk is on the map, such as "at 34, 58", or nothing when the savefile does not tell.
        /// </summary>
        public string Coordinates => this.Position is { } at ? MapLevelText.Coordinates(at, this.hasLevels) : string.Empty;

        public bool HasCoordinates => this.Coordinates.Length > 0;

        public bool IsVisited
        {
            get => this.getVisited();
            set
            {
                if (value != this.getVisited())
                {
                    this.setVisited(value);
                }
            }
        }

        /// <summary>
        /// Gets or sets whether the player has visited the obelisk, as "Yes" or "No".
        /// </summary>
        public string VisitedText
        {
            get => this.IsVisited ? "Yes" : "No";
            set => this.IsVisited = value == "Yes";
        }

        public void Refresh() => this.RaiseAllPropertiesChanged();
    }
}
