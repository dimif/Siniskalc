namespace Siniskalc.Models
{
    /// <summary>
    /// The date in the game, as the adventure map shows it. All three start at 1; a week has seven days and a month four weeks.
    /// </summary>
    public readonly record struct GameDate(int Month, int Week, int Day);
}
