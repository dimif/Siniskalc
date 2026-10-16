namespace Siniskalc.Homm2.ViewModels
{
    using System;
    using System.Linq;
    using Siniskalc.Core;
    using Siniskalc.Core.Entities;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// The army of a hero or the garrison of a castle in Heroes of Might and Magic II: five slots and a finder to add creatures. The slots,
    /// the commands and what is shown are in <see cref="ArmyViewModelBase{TSlot}"/>; this reads the army it is given and writes every edit
    /// back through <c>update</c>.
    /// </summary>
    public sealed class ArmyViewModel : ArmyViewModelBase<StackSlotViewModel>
    {
        private const int MinStackSize = 1;

        private readonly Func<Army> getArmy;
        private readonly Action<Func<Army, Army>> update;

        public ArmyViewModel(GameVersion gameVersion, Func<Army> getArmy, Action<Func<Army, Army>> update)
            : base(Creature.All.Where(c => c.IsAvailableIn(gameVersion)).Select(c => c.Name).ToList())
        {
            this.getArmy = getArmy;
            this.update = update;

            this.CreateSlots(Constants.ArmySlotCount, (i, changed, move, clear) => new StackSlotViewModel(i, changed, move, clear));
            this.Finder = new CreatureFinderViewModel(
                Creature.All.Where(c => c.IsAvailableIn(gameVersion)).Select(c => ((ICreature)c, c.Info)),
                Constants.ArmySlotCount,
                1,
                (int)Constants.MaxCreatureCount,
                () => this.HasFreeSlot,
                (creature, count) => this.AddCreature((Creature)creature, count),
                (creature, slot, count) => this.SendToSlot((Creature)creature, slot, count),
                this.DescribeSlot);

            this.Refresh();
        }

        public override CreatureFinderViewModel Finder { get; }

        /// <summary>
        /// Puts a creature in the first empty slot, as a stack of <paramref name="count"/>.
        /// </summary>
        public void AddCreature(Creature creature, int count)
        {
            var free = this.FirstFreeSlot();
            if (free >= 0)
            {
                this.SetSlot(free, creature, count);
            }
        }

        /// <summary>
        /// Puts a stack of <paramref name="count"/> of <paramref name="creature"/> in <paramref name="slot"/> (from 1), replacing what was there.
        /// </summary>
        public void SendToSlot(Creature creature, int slot, int count)
        {
            RangeGuard.ThrowIfOutOfRange(slot, 1, Constants.ArmySlotCount, nameof(slot));
            this.SetSlot(slot - 1, creature, count);
        }

        protected override StackContent? ReadStack(int index) => this.getArmy().Slots[index] is { } slot
            ? new StackContent(slot.Creature.Name, slot.Count, _ => Facts(slot.Creature))
            : null;

        protected override void PutStack(int index, string? creatureName, int amount)
        {
            var creature = string.IsNullOrEmpty(creatureName) ? null : AbstractEnumeration.FromName<Creature>(creatureName);
            if (creature == null)
            {
                // The empty entry, or a name that is not a creature, empties the slot.
                this.ClearStack(index);
                return;
            }

            this.SetSlot(index, creature, amount);
        }

        protected override void ClearStack(int index) => this.update(army => army.WithSlot(index, null));

        protected override void SwapStacks(int first, int second) => this.update(army => army.WithSlot(first, army.Slots[second]).WithSlot(second, army.Slots[first]));

        private static System.Collections.Generic.IReadOnlyList<StackFact> Facts(Creature creature) =>
        [
            new("Town", creature.Faction?.ToString() ?? "Neutral"),
            new("Level", creature.Tier.ToString()),
            new("Growth", $"{creature.Info.Growth} a week"),
        ];

        private void SetSlot(int index, Creature creature, int count)
        {
            // A newly picked creature starts with one in the stack, as the game does not allow an empty stack.
            var size = (int)RangeGuard.Clamp(count, MinStackSize, (int)Constants.MaxCreatureCount);
            this.update(army => army.WithSlot(index, new ArmySlot(creature, size)));
        }

        private string DescribeSlot(int slot) => this.getArmy().Slots[slot - 1] is { } stack ? $"{stack.Creature.Name} ×{stack.Count}" : "empty";
    }
}
