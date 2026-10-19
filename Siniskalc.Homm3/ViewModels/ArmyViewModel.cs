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
    public sealed class ArmyViewModel : ArmyViewModelBase<ArmySlotViewModel>
    {
        private readonly Func<IArmy<Creature>> getCreatures;
        private readonly Action<Func<IArmy<Creature>, IArmy<Creature>>> update;
        private readonly GameVersion gameVersion;

        public ArmyViewModel(
            GameVersion gameVersion,
            Func<IArmy<Creature>> getCreatures,
            Action<Func<IArmy<Creature>, IArmy<Creature>>> update,
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

            long size = RangeGuard.Clamp(count, Constants.Army.MinStackSize, Constants.Army.MaxStackSize);
            this.update(creatures =>
            {
                var free = FirstFreeSlot(creatures);
                return free < 0 ? creatures : creatures.WithSlot(free, creature, size);
            });
        }

        /// <summary>
        /// Puts a stack of <paramref name="count"/> of <paramref name="creature"/> in <paramref name="slot"/> (from 1), replacing what was there.
        /// </summary>
        public void SendToSlot(Creature creature, int slot, int count)
        {
            RangeGuard.ThrowIfOutOfRange(slot, 1, Constants.CreatureSlotCount, nameof(slot));
            long size = RangeGuard.Clamp(count, Constants.Army.MinStackSize, Constants.Army.MaxStackSize);
            this.update(creatures => creatures.WithSlot(slot - 1, creature, size));
        }

        protected override StackContent? ReadStack(int index)
        {
            if (this.getCreatures().Slots[index] is not { } stack)
            {
                return null;
            }

            var creature = stack.Creature;
            return new StackContent(creature.Name, (int)stack.Count, amount => new StackTotals(creature.GetInfo(this.gameVersion), amount).ToFacts(), this.UpgradesOf(creature));
        }

        private IReadOnlyList<string> UpgradesOf(Creature creature) => Creature.All
            .Where(c => c.BaseCreatureCode == creature.Code && c.IsAvailableIn(this.gameVersion))
            .Select(c => c.Name)
            .ToList();

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
                this.update(creatures => creatures.WithoutSlot(index));
                return;
            }

            // A newly picked creature starts with one in the stack, as the game does not allow an empty stack.
            long count = RangeGuard.Clamp(amount, Constants.Army.MinStackSize, Constants.Army.MaxStackSize);
            this.update(creatures => creatures.WithSlot(index, creature, count));
        }

        protected override void ClearStack(int index) => this.update(creatures => creatures.WithoutSlot(index));

        protected override void SwapStacks(int first, int second) => this.update(creatures => creatures.Swap(first, second));

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

        private string DescribeSlot(int slot)
        {
            return this.getCreatures().Slots[slot - 1] is { } stack ? $"{stack.Creature.Name} ×{stack.Count}" : "empty";
        }

        /// <summary>
        /// Gets the first empty slot, from 0, or -1 when the army is full.
        /// </summary>
        private static int FirstFreeSlot(IArmy<Creature> creatures)
        {
            for (var slot = 0; slot < creatures.Slots.Count; slot++)
            {
                if (creatures.Slots[slot] == null)
                {
                    return slot;
                }
            }

            return -1;
        }
    }
}
