namespace Siniskalc.Homm2.Models
{
    /// <summary>
    /// The two heroes the taverns offer a player, as hero numbers (<c>null</c> for none).
    /// </summary>
    public readonly record struct TavernHeroes(int? First, int? Second);
}
