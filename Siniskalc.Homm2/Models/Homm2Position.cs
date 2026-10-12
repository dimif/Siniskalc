namespace Siniskalc.Homm2.Models
{
    /// <summary>
    /// A tile of the map.
    /// </summary>
    public readonly record struct Homm2Position(int X, int Y)
    {
        public override string ToString() => $"{this.X},{this.Y}";
    }
}
