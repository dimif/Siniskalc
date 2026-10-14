namespace Siniskalc.Homm2.Models
{
    /// <summary>
    /// What the savegame says about the map it is played on.
    /// </summary>
    /// <param name="Name">The name of the map.</param>
    /// <param name="Description">The description of the map.</param>
    /// <param name="Width">The width of the map in tiles (a map is square).</param>
    /// <param name="Height">The height of the map in tiles.</param>
    public sealed record MapHeader(string Name, string Description, int Width, int Height);
}
