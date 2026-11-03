namespace Siniskalc.Heroes.Wpf.ViewModels
{
    using System.Collections.Generic;
    using System.Globalization;
    using Siniskalc.Heroes.Entities;

    /// <summary>
    /// What a whole army is worth, added up from its stacks: hit points, damage, cost, and the values the computer player and the game give it.
    /// It is the sum of what <see cref="StackTotals"/> says of each stack, and knows no game.
    /// </summary>
    public sealed class ArmyTotals
    {
        private long health;
        private long minDamage;
        private long maxDamage;
        private long gold;
        private long aiValue;
        private long fightValue;
        private long wood;
        private long ore;
        private long mercury;
        private long sulfur;
        private long crystal;
        private long gems;

        /// <summary>
        /// Gets a value indicating whether any stack was added.
        /// </summary>
        public bool Any { get; private set; }

        /// <summary>
        /// Adds a stack of <paramref name="count"/> creatures with the stats <paramref name="info"/>.
        /// </summary>
        /// <param name="info">The facts about the creature.</param>
        /// <param name="count">The number of creatures in the stack.</param>
        public void Add(CreatureInfo info, long count)
        {
            this.Any = true;
            this.health += info.Health * count;
            this.minDamage += info.MinDamage * count;
            this.maxDamage += info.MaxDamage * count;
            this.gold += info.Gold * count;
            this.aiValue += info.AiValue * count;
            this.fightValue += info.FightValue * count;
            this.wood += info.Wood * count;
            this.ore += info.Ore * count;
            this.mercury += info.Mercury * count;
            this.sulfur += info.Sulfur * count;
            this.crystal += info.Crystal * count;
            this.gems += info.Gems * count;
        }

        /// <summary>
        /// Tells what the army is worth, such as "HP Total 1,200  ·  Damage 100-150". Empty for an empty army.
        /// </summary>
        /// <param name="detail">What to tell besides the hit points: the damage, or the cost, or the cost and the values of the computer player and the game too.</param>
        /// <returns>The text, or nothing for an empty army.</returns>
        public string ToSummary(ArmySummaryDetail detail)
        {
            if (!this.Any)
            {
                return string.Empty;
            }

            if (detail == ArmySummaryDetail.Combat)
            {
                return $"HP Total {Format(this.health)}  ·  Damage {this.DamageRange()}";
            }

            var cost = $"Cost {this.LongCost()}";
            return detail == ArmySummaryDetail.CombatAndCost
                ? $"HP Total {Format(this.health)}  ·  Damage {this.DamageRange()}  ·  {cost}"
                : $"HP Total {Format(this.health)}  ·  {cost}  ·  AI {Format(this.aiValue)}  ·  Fight {Format(this.fightValue)}";
        }

        private static string Format(long number) => number.ToString("N0", CultureInfo.CurrentCulture);

        private string DamageRange() => this.minDamage == this.maxDamage ? Format(this.minDamage) : $"{Format(this.minDamage)}-{Format(this.maxDamage)}";

        // The gold and each other resource in full, such as "35,000 gold, 3 gems, 3 mercury".
        private string LongCost()
        {
            var parts = new List<string> { $"{Format(this.gold)} gold" };
            foreach (var (amount, name) in new[] { (this.wood, "wood"), (this.ore, "ore"), (this.mercury, "mercury"), (this.sulfur, "sulfur"), (this.crystal, "crystal"), (this.gems, "gems") })
            {
                if (amount != 0)
                {
                    parts.Add($"{Format(amount)} {name}");
                }
            }

            return string.Join(", ", parts);
        }
    }
}
