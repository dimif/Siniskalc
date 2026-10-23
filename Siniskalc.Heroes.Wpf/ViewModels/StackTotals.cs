namespace Siniskalc.Heroes.Wpf.ViewModels
{
    using System.Collections.Generic;
    using System.Globalization;
    using Siniskalc.Heroes.Entities;

    /// <summary>
    /// What a whole stack of a creature is worth, worked out from the stats of one creature: hit points, damage, cost, and the values the
    /// computer player and the game give it.
    /// </summary>
    public sealed class StackTotals
    {
        private readonly CreatureInfo info;
        private readonly int amount;

        public StackTotals(CreatureInfo info, int amount)
        {
            this.info = info;
            this.amount = amount;
        }

        /// <summary>
        /// Gets the hit points of the whole stack.
        /// </summary>
        public string Health => Format((long)this.info.Health * this.amount);

        /// <summary>
        /// Gets the damage the whole stack deals, as a range such as "100-150", or one number when it does not vary.
        /// </summary>
        public string Damage
        {
            get
            {
                var min = (long)this.info.MinDamage * this.amount;
                var max = (long)this.info.MaxDamage * this.amount;
                return min == max ? Format(min) : $"{Format(min)}-{Format(max)}";
            }
        }

        /// <summary>
        /// Gets the gold the whole stack costs.
        /// </summary>
        public string Cost => Format((long)this.info.Gold * this.amount);

        /// <summary>
        /// Gets the value the computer player gives the whole stack when it decides what to fight.
        /// </summary>
        public string AiValue => Format((long)this.info.AiValue * this.amount);

        /// <summary>
        /// Gets the fight value of the whole stack: how much it counts for in the strength of an army.
        /// </summary>
        public string FightValue => Format((long)this.info.FightValue * this.amount);

        /// <summary>
        /// Gets what the stack costs besides gold, such as "and 12 wood, 3 gems"; empty when it costs only gold.
        /// </summary>
        public string OtherCost
        {
            get
            {
                var parts = new List<string>();
                this.AddResource(parts, "wood", this.info.Wood);
                this.AddResource(parts, "ore", this.info.Ore);
                this.AddResource(parts, "mercury", this.info.Mercury);
                this.AddResource(parts, "sulfur", this.info.Sulfur);
                this.AddResource(parts, "crystal", this.info.Crystal);
                this.AddResource(parts, "gems", this.info.Gems);
                return parts.Count == 0 ? string.Empty : "and " + string.Join(", ", parts);
            }
        }

        /// <summary>
        /// Gives the totals as the lines the army editor shows.
        /// </summary>
        public IReadOnlyList<StackFact> ToFacts() =>
        [
            new("HP Total", this.Health),
            new("Damage", this.Damage),
            new("Cost", this.Cost),
            new(string.Empty, this.OtherCost, IsNote: true),
            new("AI value", this.AiValue, "What the computer player counts the stack as worth"),
            new("Fight value", this.FightValue, "How much the stack counts for in the strength of an army"),
        ];

        /// <summary>
        /// Gives the hit points and damage only, for a game that knows nothing else of a stack (no cost or values of the computer player).
        /// </summary>
        public IReadOnlyList<StackFact> ToHealthAndDamageFacts() =>
        [
            new("HP Total", this.Health),
            new("Damage", this.Damage),
        ];

        private static string Format(long number) => number.ToString("N0", CultureInfo.CurrentCulture);

        private void AddResource(List<string> parts, string name, short perCreature)
        {
            if (perCreature != 0)
            {
                parts.Add($"{Format((long)perCreature * this.amount)} {name}");
            }
        }
    }
}
