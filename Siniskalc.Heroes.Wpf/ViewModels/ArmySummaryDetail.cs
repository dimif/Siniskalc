namespace Siniskalc.Heroes.Wpf.ViewModels
{
    /// <summary>
    /// How much the summary of an army (the line in the heading of its card) tells.
    /// </summary>
    public enum ArmySummaryDetail
    {
        /// <summary>The hit points and the damage only.</summary>
        Combat,

        /// <summary>The hit points, the damage and what the army costs.</summary>
        CombatAndCost,

        /// <summary>The hit points, what the army costs, and the values the computer player and the game give it.</summary>
        CombatCostAndValues,
    }
}
