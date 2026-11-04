namespace Siniskalc.Homm3.Models
{
    using Siniskalc.Core;

    /// <summary>The four guard strengths and associated gold payouts offered by the map editor.</summary>
    public static class DragonUtopiaGuards
    {
        public static int Gold(int strength)
        {
            RangeGuard.ThrowIfOutOfRange(strength, Constants.CreatureBank.MinStrength, Constants.CreatureBank.MaxStrength, nameof(strength));
            return (strength + 1) * Constants.CreatureBank.GoldStep;
        }

        public static int[] Counts(int strength)
        {
            RangeGuard.ThrowIfOutOfRange(strength, Constants.CreatureBank.MinStrength, Constants.CreatureBank.MaxStrength, nameof(strength));
            return strength switch
            {
                1 => [8, 5, 2, 1, 0, 0, 0],
                2 => [8, 6, 3, 2, 0, 0, 0],
                3 => [8, 6, 4, 3, 0, 0, 0],
                _ => [8, 7, 6, 5, 0, 0, 0],
            };
        }
    }
}
