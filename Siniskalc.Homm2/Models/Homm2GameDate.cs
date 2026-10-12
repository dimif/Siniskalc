namespace Siniskalc.Homm2.Models
{
    /// <summary>
    /// The date in the game: the day of the week (1 to 7), the week of the month (1 to 4) and the month (from 1).
    /// </summary>
    public sealed record Homm2GameDate(int Day, int Week, int Month)
    {
        public override string ToString() => $"Month {this.Month}, Week {this.Week}, Day {this.Day}";
    }
}
