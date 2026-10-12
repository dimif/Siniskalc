namespace Siniskalc.Homm3.Entities
{
    public record HeroPosition(short X, short Y, short Z, FacingDirection FacingDirection) : Position(X, Y, Z);
}
