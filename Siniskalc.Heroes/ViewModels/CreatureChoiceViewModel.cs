namespace Siniskalc.Heroes.ViewModels
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Siniskalc.Heroes.Entities;

    /// <summary>
    /// One creature in the creature finder list of any game, with its stats for the game version of the savegame.
    /// </summary>
    public sealed class CreatureChoiceViewModel
    {
        private readonly CreatureInfo info;

        public CreatureChoiceViewModel(ICreature creature, CreatureInfo info)
        {
            this.Creature = creature;
            this.info = info;
        }

        public ICreature Creature { get; }

        public string Name => this.Creature.Name;

        /// <summary>
        /// Gets the town the creature belongs to, or "Neutral"; the list is grouped by it.
        /// </summary>
        public string Faction => this.Creature.Faction.Name;

        /// <summary>
        /// Gets the town the creature belongs to as a number to order the list by: neutral creatures come last.
        /// </summary>
        public int FactionOrder => this.Creature.Faction.Code;

        /// <summary>
        /// Gets the creature level among those of its town.
        /// </summary>
        public int Level => this.Creature.Tier;

        /// <summary>
        /// Gets "Basic", or "Upgraded" for the upgraded form of another creature. Searchable, not shown.
        /// </summary>
        public string Form => this.Creature.IsUpgrade ? "Upgraded" : "Basic";

        public int Attack => this.info.Atk;

        public int Defense => this.info.Def;

        /// <summary>
        /// Gets the damage a creature deals, such as "2-3", or one number when it does not vary.
        /// </summary>
        public string DamageRange => this.info.MinDamage == this.info.MaxDamage
            ? this.info.MinDamage.ToString(CultureInfo.InvariantCulture)
            : $"{this.info.MinDamage}-{this.info.MaxDamage}";

        public int Health => this.info.Health;

        public int Speed => this.info.Speed;

        /// <summary>
        /// Gets how many of the creature a town grows in a week.
        /// </summary>
        public int Growth => this.info.Growth;

        /// <summary>
        /// Gets the gold cost of one creature.
        /// </summary>
        public int Cost => this.info.Gold;

        /// <summary>
        /// Gets what one creature costs besides gold, such as "1 wood, 2 gems"; empty when it costs only gold.
        /// </summary>
        public string Resources => string.Join(", ", this.ResourceParts());

        /// <summary>
        /// Gets what the creature can do beyond fighting, such as "Undead, Regeneration"; empty when nothing.
        /// </summary>
        public string Special => string.Join(", ", (this.info.Special ?? []).Select(this.AbilityText));

        /// <summary>
        /// Gets what the creature costs besides gold, how many it grows a week, what it can do beyond fighting, and how many shots it has, one fact per line.
        /// </summary>
        public string Details
        {
            get
            {
                var lines = new List<string> { this.Name };
                var resources = this.ResourceParts();
                resources.Insert(0, $"{this.info.Gold:N0} gold");
                lines.Add("Cost: " + string.Join(", ", resources));
                lines.Add($"Growth: {this.info.Growth} a week");

                if (this.info.Shots > 0)
                {
                    lines.Add($"Shots: {this.info.Shots}");
                }

                if (this.Special.Length > 0)
                {
                    lines.Add(this.Special);
                }

                return string.Join("\n", lines);
            }
        }

        private static void AddResource(List<string> resources, string name, int amount)
        {
            if (amount != 0)
            {
                resources.Add($"{amount} {name}");
            }
        }

        private string AbilityText(ICreatureAbility ability) =>
            ability.IsRanged && this.info.Shots > 0 ? $"{ability.Name} ({this.info.Shots} shots)" : ability.Name;

        private List<string> ResourceParts()
        {
            var resources = new List<string>();
            AddResource(resources, "wood", this.info.Wood);
            AddResource(resources, "ore", this.info.Ore);
            AddResource(resources, "mercury", this.info.Mercury);
            AddResource(resources, "sulfur", this.info.Sulfur);
            AddResource(resources, "crystal", this.info.Crystal);
            AddResource(resources, "gems", this.info.Gems);
            return resources;
        }
    }
}
