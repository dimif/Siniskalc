namespace Siniskalc.Homm3.Models
{
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core;

    /// <summary>The observed guard and retained-resource presets, weakest first.</summary>
    public static class CreatureBankPresets
    {
        public static int[] Types(CreatureBankKind kind, bool upgradedCenter)
        {
            if (kind == CreatureBankKind.DragonUtopia)
            {
                return [26, 82, 27, 83, -1, -1, -1];
            }

            var creature = kind switch
            {
                CreatureBankKind.CyclopsStockpile => 94,
                CreatureBankKind.DwarvenTreasury => 16,
                CreatureBankKind.ImpCache => 42,
                CreatureBankKind.MedusaStores => 76,
                _ => 38,
            };
            return [creature, creature, creature + (upgradedCenter ? 1 : 0), creature, creature, -1, -1];
        }

        public static int[] Counts(CreatureBankKind kind, int strength)
        {
            CheckStrength(strength);
            if (kind == CreatureBankKind.DragonUtopia)
            {
                return DragonUtopiaGuards.Counts(strength);
            }

            var counts = kind switch
            {
                CreatureBankKind.CyclopsStockpile => new[] { 4, 6, 8, 10 },
                CreatureBankKind.DwarvenTreasury => [10, 15, 20, 30],
                CreatureBankKind.ImpCache => [20, 30, 40, 60],
                CreatureBankKind.MedusaStores => [4, 6, 8, 10],
                _ => [2, 3, 4, 6],
            };
            var count = counts[strength - Constants.CreatureBank.MinStrength];
            return [count, count, count, count, count, 0, 0];
        }

        public static int[] Resources(CreatureBankKind kind, int strength)
        {
            CheckStrength(strength);
            var index = strength - Constants.CreatureBank.MinStrength;
            if (kind == CreatureBankKind.DragonUtopia)
            {
                return [0, 0, 0, 0, 0, 0, DragonUtopiaGuards.Gold(strength)];
            }

            if (kind == CreatureBankKind.CyclopsStockpile)
            {
                return Enumerable.Repeat(Counts(kind, strength)[0], Constants.CreatureBank.ResourceCount - 1).Append(0).ToArray();
            }

            var payouts = kind switch
            {
                CreatureBankKind.DwarvenTreasury => new[] { (2, 2500), (3, 4000), (5, 5000), (10, 7500) },
                CreatureBankKind.ImpCache => new[] { (2, 1000), (3, 1500), (4, 2000), (6, 3000) },
                CreatureBankKind.MedusaStores => new[] { (5, 2000), (6, 3000), (8, 4000), (10, 5000) },
                _ => new[] { (8, 4000), (12, 6000), (16, 8000), (24, 12000) },
            };
            var (amount, gold) = payouts[index];
            return kind switch
            {
                CreatureBankKind.DwarvenTreasury => [0, 0, 0, 0, amount, 0, gold],
                CreatureBankKind.ImpCache => [0, amount, 0, 0, 0, 0, gold],
                CreatureBankKind.MedusaStores => [0, 0, 0, amount, 0, 0, gold],
                _ => [0, 0, 0, 0, 0, amount, gold],
            };
        }

        public static int? FindStrength(CreatureBankKind kind, IReadOnlyList<int> resources)
        {
            for (var strength = Constants.CreatureBank.MinStrength; strength <= Constants.CreatureBank.MaxStrength; strength++)
            {
                if (resources.SequenceEqual(Resources(kind, strength)))
                {
                    return strength;
                }
            }

            return null;
        }

        private static void CheckStrength(int strength) =>
            RangeGuard.ThrowIfOutOfRange(strength, Constants.CreatureBank.MinStrength, Constants.CreatureBank.MaxStrength, nameof(strength));
    }
}
