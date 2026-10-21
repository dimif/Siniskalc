namespace Siniskalc.Homm2.Models
{
    using System.Linq;
    using Siniskalc.Core;
    using Siniskalc.Core.Models;

    /// <summary>
    /// Which castles have built something today: the game lets a castle build one thing a day, and keeps one bit for each of the 72 castle
    /// slots in a set after the table of castles. The whole set is cleared when a new day starts.
    /// </summary>
    /// <param name="Index">The place in the list of these sets, which has only one.</param>
    /// <param name="Castles">The numbers of the castles that have built today, in order.</param>
    public sealed record BuiltTodayModel(int Index, Numbers Castles)
    {
        public string Name => "Built this turn";

        /// <summary>
        /// Gets a value indicating whether castle <paramref name="castle"/> (0 to 71) has built today.
        /// </summary>
        public bool Has(int castle)
        {
            RangeGuard.ThrowIfOutOfRange(castle, 0, Constants.Table.CastleSlotCount - 1, nameof(castle));
            return this.Castles.Contains(castle);
        }

        /// <summary>
        /// Returns the set with castle <paramref name="castle"/> (0 to 71) marked as having built today, or not.
        /// </summary>
        public BuiltTodayModel With(int castle, bool built)
        {
            RangeGuard.ThrowIfOutOfRange(castle, 0, Constants.Table.CastleSlotCount - 1, nameof(castle));
            return this.Has(castle) == built ? this : this with { Castles = new Numbers(built ? this.Castles.Append(castle).Order() : this.Castles.Where(c => c != castle)) };
        }
    }
}
