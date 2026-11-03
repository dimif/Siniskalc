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

        /// <summary>
        /// Initializes a new instance of the <see cref="StackTotals"/> class.
        /// </summary>
        /// <param name="info">The info.</param>
        /// <param name="amount">The amount.</param>
        public StackTotals(CreatureInfo info, int amount)
        {
            this.info = info;
            this.amount = amount;
        }

        /// <summary>
        /// Gets the attack skill of one creature of the stack.
        /// </summary>
        public string CreatureAttack => Format(this.info.Atk);

        /// <summary>
        /// Gets the defense skill of one creature of the stack.
        /// </summary>
        public string CreatureDefense => Format(this.info.Def);

        /// <summary>
        /// Gets the speed of one creature of the stack.
        /// </summary>
        public string CreatureSpeed => Format(this.info.Speed);

        /// <summary>
        /// Gets the hit points of one creature of the stack.
        /// </summary>
        public string CreatureHealth => Format(this.info.Health);

        /// <summary>
        /// Gets the damage one creature of the stack deals, as a range such as "2-5", or one number when it does not vary.
        /// </summary>
        public string CreatureDamage => this.info.MinDamage == this.info.MaxDamage ? Format(this.info.MinDamage) : $"{Format(this.info.MinDamage)}-{Format(this.info.MaxDamage)}";

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
        /// Gets what the stack costs besides gold, short, such as "12w, 3g" (w wood, o ore, m mercury, s sulfur, c crystal, g gems);
        /// empty when it costs only gold.
        /// </summary>
        public string OtherCost
        {
            get
            {
                var parts = new List<string>();
                this.AddResource(parts, "w", this.info.Wood);
                this.AddResource(parts, "o", this.info.Ore);
                this.AddResource(parts, "m", this.info.Mercury);
                this.AddResource(parts, "s", this.info.Sulfur);
                this.AddResource(parts, "c", this.info.Crystal);
                this.AddResource(parts, "g", this.info.Gems);
                return string.Join(", ", parts);
            }
        }

        /// <summary>
        /// Gets the cost of the stack as one fact: its gold, then the other resources it costs, such as "35,000, 3g".
        /// </summary>
        public StackFact CostFact => new("Cost", this.OtherCost.Length == 0 ? this.Cost : $"{this.Cost}, {this.OtherCost}", "What the whole stack costs");

        /// <summary>
        /// Gives the totals as the lines the army editor shows.
        /// </summary>
        /// <returns>The facts.</returns>
        public IReadOnlyList<StackFact> ToFacts() =>
        [
            new("Atk", this.CreatureAttack, "Attack of one creature"),
            new("Def", this.CreatureDefense, "Defense of one creature"),
            new("Speed", this.CreatureSpeed, "Speed of one creature in combat"),
            new("HP", this.CreatureHealth, "Hit points of one creature"),
            new("Damage", this.CreatureDamage, "Damage of one creature"),
            new("Stack HP", this.Health, "Hit points of the whole stack"),
            new("Stack Damage", this.Damage, "Damage of the whole stack"),
            this.CostFact,
            new("AI value", this.AiValue, "What the computer player counts the stack as worth"),
            new("Fight value", this.FightValue, "How much the stack counts for in the strength of an army"),
        ];

        /// <summary>
        /// Gives the combat stats, hit points and damage only, for a game that adds its own facts (no values of the computer player).
        /// </summary>
        /// <returns>The facts.</returns>
        public IReadOnlyList<StackFact> ToHealthAndDamageFacts() =>
        [
            new("Atk", this.CreatureAttack, "Attack of one creature"),
            new("Def", this.CreatureDefense, "Defense of one creature"),
            new("Speed", this.CreatureSpeed, "Speed of one creature in combat"),
            new("HP", this.CreatureHealth, "Hit points of one creature"),
            new("Damage", this.CreatureDamage, "Damage of one creature"),
            new("Stack HP", this.Health, "Hit points of the whole stack"),
            new("Stack Damage", this.Damage, "Damage of the whole stack"),
        ];

        private static string Format(long number) => number.ToString("N0", CultureInfo.CurrentCulture);

        private void AddResource(List<string> parts, string name, short perCreature)
        {
            if (perCreature != 0)
            {
                parts.Add($"{Format((long)perCreature * this.amount)}{name}");
            }
        }
    }
}
