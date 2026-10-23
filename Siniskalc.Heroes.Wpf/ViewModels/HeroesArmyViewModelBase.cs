namespace Siniskalc.Heroes.Wpf.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core;
    using Siniskalc.Core.Entities;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// The army editor of every game: the stacks of an <see cref="IArmy{TCreature}"/>, the creature finder that adds to it, and the edits
    /// that put a creature in a slot, empty a slot or swap two. A game adds what is its own: the facts of a stack and the summary.
    /// </summary>
    /// <typeparam name="TCreature">The creature of the game.</typeparam>
    /// <typeparam name="TSlot">The view model of a slot.</typeparam>
    public abstract class HeroesArmyViewModelBase<TCreature, TSlot> : ArmyViewModelBase<TSlot>
        where TCreature : AbstractEnumeration, ICreature
        where TSlot : StackSlotViewModel
    {
        private readonly IReadOnlyList<TCreature> creatures;
        private readonly Func<TCreature, CreatureInfo> infoOf;
        private readonly Func<IArmy<TCreature>> getArmy;
        private readonly Action<Func<IArmy<TCreature>, IArmy<TCreature>>> update;
        private readonly int minStack;
        private readonly int maxStack;

        /// <summary>
        /// Initializes a new instance of the <see cref="HeroesArmyViewModelBase{TCreature, TSlot}"/> class.
        /// </summary>
        /// <param name="creatures">The creatures the savefile can hold.</param>
        /// <param name="infoOf">Gives the stats of a creature, which can depend on the version of the game.</param>
        /// <param name="slotCount">The number of slots of an army.</param>
        /// <param name="minStack">The fewest creatures a stack holds.</param>
        /// <param name="maxStack">The most creatures a stack holds.</param>
        /// <param name="getArmy">Gets the army as it is now.</param>
        /// <param name="update">Replaces the army with what an edit makes of it.</param>
        /// <param name="createSlot">Makes the view model of a slot.</param>
        /// <param name="isReadOnly">Tells whether the army cannot be changed now.</param>
        protected HeroesArmyViewModelBase(
            IReadOnlyList<TCreature> creatures,
            Func<TCreature, CreatureInfo> infoOf,
            int slotCount,
            int minStack,
            int maxStack,
            Func<IArmy<TCreature>> getArmy,
            Action<Func<IArmy<TCreature>, IArmy<TCreature>>> update,
            Func<int, Action<int, string?, int>, Action<int, int>, Action<int>, TSlot> createSlot,
            Func<bool>? isReadOnly = null)
            : base(creatures.Select(c => c.Name).ToList(), isReadOnly)
        {
            this.creatures = creatures;
            this.infoOf = infoOf;
            this.getArmy = getArmy;
            this.update = update;
            this.minStack = minStack;
            this.maxStack = maxStack;
            this.CreateSlots(slotCount, createSlot);
            this.Finder = new CreatureFinderViewModel(
                creatures.Select(c => ((ICreature)c, infoOf(c))),
                slotCount,
                minStack,
                maxStack,
                () => this.HasFreeSlot,
                (creature, count) => this.AddCreature((TCreature)creature, count),
                (creature, slot, count) => this.SendToSlot((TCreature)creature, slot, count),
                this.DescribeSlot);
        }

        /// <inheritdoc/>
        public override CreatureFinderViewModel Finder { get; }

        /// <inheritdoc/>
        protected override AbstractFinderViewModel CreatePickerFinder(Action<string> onPicked) => new CreatureFinderViewModel(
            this.creatures.Select(c => ((ICreature)c, this.infoOf(c))),
            this.CreatureSlots.Count,
            this.minStack,
            this.maxStack,
            () => true,
            (creature, _) => onPicked(creature.Name),
            actionLabel: "Choose");

        /// <summary>
        /// Puts <paramref name="count"/> creatures in the first free slot, or does nothing when every slot is taken.
        /// </summary>
        /// <param name="creature">The creature.</param>
        /// <param name="count">How many; kept between the fewest and the most a stack holds.</param>
        public void AddCreature(TCreature creature, int count)
        {
            if (!this.HasFreeSlot)
            {
                return;
            }

            var size = this.ClampStack(count);
            this.update(army =>
            {
                var free = FirstFreeSlot(army);
                return free < 0 ? army : army.WithSlot(free, creature, size);
            });
        }

        /// <summary>
        /// Puts <paramref name="count"/> creatures in a slot, replacing what was there.
        /// </summary>
        /// <param name="creature">The creature.</param>
        /// <param name="slot">The number of the slot, from 1.</param>
        /// <param name="count">How many; kept between the fewest and the most a stack holds.</param>
        public void SendToSlot(TCreature creature, int slot, int count)
        {
            RangeGuard.ThrowIfOutOfRange(slot, 1, this.CreatureSlots.Count, nameof(slot));
            var size = this.ClampStack(count);
            this.update(army => army.WithSlot(slot - 1, creature, size));
        }

        /// <summary>
        /// Gives the facts to show for a stack.
        /// </summary>
        /// <param name="creature">The creature of the stack.</param>
        /// <param name="amount">How many creatures it holds.</param>
        protected abstract IReadOnlyList<StackFact> FactsOf(TCreature creature, int amount);

        /// <summary>
        /// Gets a value indicating whether the summary of the army tells the cost and the values of the computer player too. False for a game
        /// that does not know them.
        /// </summary>
        protected virtual bool SummaryHasValues => false;

        /// <inheritdoc/>
        protected override string Summarize()
        {
            var totals = new ArmyTotals();
            foreach (var stack in this.getArmy().Filled)
            {
                totals.Add(this.infoOf(stack.Creature), stack.Count);
            }

            return totals.ToSummary(this.SummaryHasValues);
        }

        /// <inheritdoc/>
        protected override StackContent? ReadStack(int index)
        {
            if (this.getArmy().Slots[index] is not { } stack)
            {
                return null;
            }

            var creature = stack.Creature;
            return new StackContent(creature.Name, (int)stack.Count, amount => this.FactsOf(creature, amount), this.UpgradesOf(creature));
        }

        /// <inheritdoc/>
        protected override void PutStack(int index, string? creatureName, int amount)
        {
            var creature = string.IsNullOrEmpty(creatureName) ? null : this.creatures.FirstOrDefault(c => c.Name == creatureName);
            if (creature == null)
            {
                // The empty entry, or a name that is not a creature, empties the slot.
                this.ClearStack(index);
                return;
            }

            // A newly picked creature starts with one in the stack, as the game does not allow an empty stack.
            var size = this.ClampStack(amount);
            this.update(army => army.WithSlot(index, creature, size));
        }

        /// <inheritdoc/>
        protected override void ClearStack(int index) => this.update(army => army.WithoutSlot(index));

        /// <inheritdoc/>
        protected override void SwapStacks(int first, int second) => this.update(army => army.Swap(first, second));

        private static int FirstFreeSlot(IArmy<TCreature> army)
        {
            for (var slot = 0; slot < army.Slots.Count; slot++)
            {
                if (army.Slots[slot] == null)
                {
                    return slot;
                }
            }

            return -1;
        }

        private long ClampStack(long count) => RangeGuard.Clamp(count, this.minStack, this.maxStack);

        private IReadOnlyList<string> UpgradesOf(TCreature creature) => this.creatures
            .Where(c => c.BaseCreatureCode == creature.Code)
            .Select(c => c.Name)
            .ToList();

        private string DescribeSlot(int slot) => this.getArmy().Slots[slot - 1] is { } stack ? $"{stack.Creature.Name} ×{stack.Count}" : "empty";
    }
}
