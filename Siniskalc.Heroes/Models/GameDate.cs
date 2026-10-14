namespace Siniskalc.Heroes.Models
{
    /// <summary>
    /// A day in the game: the month, the week of the month and the day of the week, each counted from 1.
    /// </summary>
    public readonly record struct GameDate(int Month, int Week, int Day)
    {
        public override string ToString() => $"Month {this.Month}, Week {this.Week}, Day {this.Day}";
    }
}
