namespace Siniskalc.Homm3.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core;
    using Siniskalc.Core.Entities;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Models;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// ViewModel for an army of Heroes of Might and Magic III: the seven creature slots of a hero, and a <see cref="Finder"/> to look up
    /// creatures to add. The slots, the commands and what is shown are in <see cref="ArmyViewModelBase{TSlot}"/>; this reads the army
    /// from the hero it is given and writes every edit back through <c>update</c>.
    /// Call <see cref="ArmyViewModelBase{TSlot}.Refresh"/> after the army changed.
    /// </summary>
    public sealed class ArmyViewModel : HeroesArmyViewModelBase<Creature, ArmySlotViewModel>
    {
        private readonly Func<IArmy<Creature>> getCreatures;
        private readonly GameVersion gameVersion;

        public ArmyViewModel(
            GameVersion gameVersion,
            Func<IArmy<Creature>> getCreatures,
            Action<Func<IArmy<Creature>, IArmy<Creature>>> update,
            Func<bool>? isReadOnly = null)
            : base(
                Creature.All.Where(c => c.IsAvailableIn(gameVersion)).ToList(),
                c => c.GetInfo(gameVersion),
                Constants.CreatureSlotCount,
                Constants.Army.MinStackSize,
                Constants.Army.MaxStackSize,
                getCreatures,
                update,
                (i, changed, move, clear) => new ArmySlotViewModel(i, changed, move, clear),
                isReadOnly)
        {
            this.gameVersion = gameVersion;
            this.getCreatures = getCreatures;
            this.Refresh();
        }

        /// <inheritdoc/>
        protected override IReadOnlyList<StackFact> FactsOf(Creature creature, int amount) => new StackTotals(creature.GetInfo(this.gameVersion), amount).ToFacts();

        /// <inheritdoc/>
        protected override void AfterSlotRead(ArmySlotViewModel slot, StackContent? stack)
        {
            var creature = stack == null || stack.CreatureName.Length == 0 ? null : AbstractEnumeration.FromName<Creature>(stack.CreatureName);
            slot.SetInfo(creature?.GetInfo(this.gameVersion));
        }

        /// <inheritdoc/>
        protected override string Summarize()
        {
            var creatures = this.getCreatures();
            long health = 0, gold = 0, ai = 0, fight = 0, wood = 0, ore = 0, mercury = 0, sulfur = 0, crystal = 0, gems = 0;
            var any = false;
            foreach (var stack in creatures.Filled)
            {
                any = true;
                var info = stack.Creature.GetInfo(this.gameVersion);
                long count = stack.Count;
                health += info.Health * count;
                gold += info.Gold * count;
                ai += info.AiValue * count;
                fight += info.FightValue * count;
                wood += info.Wood * count;
                ore += info.Ore * count;
                mercury += info.Mercury * count;
                sulfur += info.Sulfur * count;
                crystal += info.Crystal * count;
                gems += info.Gems * count;
            }

            if (!any)
            {
                return string.Empty;
            }

            var others = new List<string>();
            foreach (var (amount, name) in new[] { (wood, "wood"), (ore, "ore"), (mercury, "mercury"), (sulfur, "sulfur"), (crystal, "crystal"), (gems, "gems") })
            {
                if (amount != 0)
                {
                    others.Add($"{amount:N0} {name}");
                }
            }

            var cost = others.Count == 0 ? $"{gold:N0}" : $"{gold:N0} and {string.Join(", ", others)}";
            return $"HP Total {health:N0}  ·  Cost {cost}  ·  AI {ai:N0}  ·  Fight {fight:N0}";
        }
    }
}
