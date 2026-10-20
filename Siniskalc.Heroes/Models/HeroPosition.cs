namespace Siniskalc.Heroes.Models
{
    using Siniskalc.Heroes.Entities;

    /// <summary>
    /// Where a hero stands and which way it faces. A game with one level only keeps <see cref="MapPosition.Level"/> at 0.
    /// </summary>
    /// <param name="X">The column.</param>
    /// <param name="Y">The row.</param>
    /// <param name="Level">The level of the map.</param>
    /// <param name="FacingDirection">The direction the hero faces.</param>
    public record HeroPosition(int X, int Y, int Level, FacingDirection FacingDirection) : MapPosition(X, Y, Level)
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HeroPosition"/> record on the surface of the map.
        /// </summary>
        public HeroPosition(int x, int y, FacingDirection facingDirection)
            : this(x, y, Above, facingDirection)
        {
        }

        /// <inheritdoc/>
        public override string ToString() => new MapPosition(this.X, this.Y, this.Level).ToString();
    }
}
