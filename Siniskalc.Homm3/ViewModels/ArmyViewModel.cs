namespace Siniskalc.Homm3.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core;
    using Siniskalc.Core.Entities;
    using Siniskalc.Heroes.Entities;
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
    public sealed class ArmyViewModel : ArmyViewModelBase<ArmySlotViewModel>
    {
        private readonly Func<Army> getCreatures;
        private readonly Action<Func<Army, Army>> update;
        private readonly GameVersion gameVersion;

        public ArmyViewModel(
            GameVersion gameVersion,
            Func<Army> getCreatures,
            Action<Func<Army, Army>> update,
            Func<bool>? isReadOnly = null)
            : base(Creature.All.Where(c => c.IsAvailableIn(gameVersion)).Select(c => c.Name).ToList(), isReadOnly)
        {
            this.gameVersion = gameVersion;
            this.getCreatures = getCreatures;
            this.update = update;

            this.CreateSlots(Constants.CreatureSlotCount, (i, changed, move, clear) => new ArmySlotViewModel(i, changed, move, clear));
            this.Finder = new CreatureFinderViewModel(
                Creature.All.Where(c => c.IsAvailableIn(gameVersion)).Select(c => ((ICreature)c, c.GetInfo(gameVersion))),
                Constants.CreatureSlotCount,
                Constants.Army.MinStackSize,
                Constants.Army.MaxStackSize,
                () => this.HasFreeSlot,
                (creature, count) => this.AddCreature((Creature)creature, count),
                (creature, slot, count) => this.SendToSlot((Creature)creature, slot, count),
                this.DescribeSlot);

            this.Refresh();
        }

        /// <summary>
        /// Gets the finder that looks up creatures by name, town and level and adds them to the first free slot.
        /// </summary>
        public override CreatureFinderViewModel Finder { get; }

        /// <summary>
        /// Puts a creature in the first empty slot, as a stack of <paramref name="count"/>.
        /// </summary>
        public void AddCreature(Creature creature, int count)
        {
            if (!this.HasFreeSlot)
            {
                return;
            }

            var size = (uint)RangeGuard.Clamp(count, Constants.Army.MinStackSize, Constants.Army.MaxStackSize);
            this.update(creatures =>
            {
                var free = this.FirstFreeSlot(creatures);
                return free == 0 ? creatures : creatures.WithSlot(free, creature, size);
            });
        }

        /// <summary>
        /// Puts a stack of <paramref name="count"/> of <paramref name="creature"/> in <paramref name="slot"/> (from 1), replacing what was there.
        /// </summary>
        public void SendToSlot(Creature creature, int slot, int count)
        {
            RangeGuard.ThrowIfOutOfRange(slot, 1, Constants.CreatureSlotCount, nameof(slot));
            var size = (uint)RangeGuard.Clamp(count, Constants.Army.MinStackSize, Constants.Army.MaxStackSize);
            this.update(creatures => creatures.WithSlot(slot, creature, size));
        }

        protected override StackContent? ReadStack(int index)
        {
            var stack = this.getCreatures().FindSlot(index + 1);
            var creature = stack == null ? null : Creature.FromByte(stack.CreatureCode);
            if (stack == null)
            {
                return null;
            }

            // A creature the editor does not know shows as an empty slot, as it always did.
            return creature == null ? new StackContent(string.Empty, (int)stack.CreatureCount) : new StackContent(creature.Name, (int)stack.CreatureCount, amount => new StackTotals(creature.GetInfo(this.gameVersion), amount).ToFacts());
        }

        protected override void AfterSlotRead(ArmySlotViewModel slot, StackContent? stack)
        {
            var creature = stack == null || stack.CreatureName.Length == 0 ? null : AbstractEnumeration.FromName<Creature>(stack.CreatureName);
            slot.SetInfo(creature?.GetInfo(this.gameVersion));
        }

        protected override void PutStack(int index, string? creatureName, int amount)
        {
            var creature = string.IsNullOrEmpty(creatureName) ? null : AbstractEnumeration.FromName<Creature>(creatureName);
            if (creature == null)
            {
                // The empty entry, or a name that is not a creature, empties the slot.
                this.update(creatures => creatures.WithoutSlot(index + 1));
                return;
            }

            // A newly picked creature starts with one in the stack, as the game does not allow an empty stack.
            var count = (uint)RangeGuard.Clamp(amount, Constants.Army.MinStackSize, Constants.Army.MaxStackSize);
            this.update(creatures => creatures.WithSlot(index + 1, creature, count));
        }

        protected override void ClearStack(int index) => this.update(creatures => creatures.WithoutSlot(index + 1));

        protected override void SwapStacks(int first, int second) => this.update(creatures => creatures.Swap(first + 1, second + 1));

        protected override string Summarize()
        {
            var creatures = this.getCreatures();
            long health = 0, gold = 0, ai = 0, fight = 0, wood = 0, ore = 0, mercury = 0, sulfur = 0, crystal = 0, gems = 0;
            var any = false;
            for (var slot = 1; slot <= Constants.CreatureSlotCount; slot++)
            {
                var stack = creatures.FindSlot(slot);
                var creature = stack == null ? null : Creature.FromByte(stack.CreatureCode);
                if (stack == null || creature == null)
                {
                    continue;
                }

                any = true;
                var info = creature.GetInfo(this.gameVersion);
                long count = stack.CreatureCount;
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

        private string DescribeSlot(int slot)
        {
            var stack = this.getCreatures().FindSlot(slot);
            var creature = stack == null ? null : Creature.FromByte(stack.CreatureCode);
            return creature == null || stack == null ? "empty" : $"{creature.Name} ×{stack.CreatureCount}";
        }

        /// <summary>
        /// Gets the first empty slot, from 1, or 0 when the army is full.
        /// </summary>
        private int FirstFreeSlot(Army creatures)
        {
            for (var slot = 1; slot <= Constants.CreatureSlotCount; slot++)
            {
                if (creatures.FindSlot(slot) == null)
                {
                    return slot;
                }
            }

            return 0;
        }
    }
}
