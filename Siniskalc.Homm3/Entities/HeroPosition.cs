namespace Siniskalc.Homm3.Entities
{
    using Siniskalc.Heroes.Models;

    public record HeroPosition(int X, int Y, int Level, FacingDirection FacingDirection) : MapPosition(X, Y, Level);
}
