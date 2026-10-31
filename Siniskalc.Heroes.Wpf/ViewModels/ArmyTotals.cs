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
        /// <param name="withValues">Whether to tell the cost and the values of the computer player and the game too, for a game that knows them.</param>
        public string ToSummary(bool withValues)
        {
            if (!this.Any)
            {
                return string.Empty;
            }

            if (!withValues)
            {
                var damage = this.minDamage == this.maxDamage ? Format(this.minDamage) : $"{Format(this.minDamage)}-{Format(this.maxDamage)}";
                return $"HP Total {Format(this.health)}  ·  Damage {damage}";
            }

            var others = new List<string>();
            foreach (var (amount, name) in new[] { (this.wood, "wood"), (this.ore, "ore"), (this.mercury, "mercury"), (this.sulfur, "sulfur"), (this.crystal, "crystal"), (this.gems, "gems") })
            {
                if (amount != 0)
                {
                    others.Add($"{Format(amount)} {name}");
                }
            }

            var cost = others.Count == 0 ? Format(this.gold) : $"{Format(this.gold)} and {string.Join(", ", others)}";
            return $"HP Total {Format(this.health)}  ·  Cost {cost}  ·  AI {Format(this.aiValue)}  ·  Fight {Format(this.fightValue)}";
        }

        private static string Format(long number) => number.ToString("N0", CultureInfo.CurrentCulture);
    }
}
