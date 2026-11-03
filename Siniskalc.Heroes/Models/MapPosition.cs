namespace Siniskalc.Heroes.Models
{
    /// <summary>
    /// A tile of the map, and the level of the map it is on.
    /// </summary>
    /// <param name="X">The column.</param>
    /// <param name="Y">The row.</param>
    /// <param name="Level">The level of the map: <see cref="Above"/> or <see cref="Underground"/>. A game that has one level only keeps it 0.</param>
    public record MapPosition(int X, int Y, int Level = MapPosition.Above)
    {
        /// <summary>
        /// The level of the surface of the map.
        /// </summary>
        public const int Above = 0;

        /// <summary>
        /// The level underground, if the map has one.
        /// </summary>
        public const int Underground = 1;

        /// <summary>
        /// The position of something that is not on the map, such as a hero that has no marker.
        /// </summary>
        public static readonly MapPosition None = new(Constants.Map.NoCoordinate, Constants.Map.NoCoordinate, Constants.Map.NoCoordinate);

        /// <inheritdoc/>
        public override string ToString() => this.Level == Above ? $"{this.X},{this.Y}" : $"{this.X},{this.Y},{this.Level}";
    }
}
