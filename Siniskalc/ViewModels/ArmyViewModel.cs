namespace Siniskalc.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Siniskalc.Core;
    using Siniskalc.Core.Entities;
    using Siniskalc.Entities;
    using Siniskalc.Models;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// ViewModel for an army: the seven creature slots of a hero, and a <see cref="Finder"/> to look up creatures to add.
    /// Reads the army from the hero it is given and writes every edit back through <c>update</c>.
    /// Call <see cref="Refresh"/> after the army changed.
    /// </summary>
    public sealed class ArmyViewModel : PageViewModelBase
    {
        private readonly Func<Army> getCreatures;
        private readonly Action<Func<Army, Army>> update;
        private readonly GameVersion gameVersion;
        private readonly Func<bool>? isReadOnly;
        private bool isRefreshing;

        public ArmyViewModel(
            GameVersion gameVersion,
            Func<Army> getCreatures,
            Action<Func<Army, Army>> update,
            Func<bool>? isReadOnly = null)
            : base("Creatures")
        {
            this.isReadOnly = isReadOnly;
            this.gameVersion = gameVersion;
            this.getCreatures = getCreatures;
            this.update = update;

            // An empty entry first, so a slot can be left empty.
            this.AvailableCreatures = [string.Empty, .. Creature.All.Where(c => c.IsAvailableIn(gameVersion)).Select(c => c.Name)];

            this.CreatureSlots = new ObservableCollection<ArmySlotViewModel>(
                Enumerable.Range(0, Constants.CreatureSlotCount).Select(i =>
                    new ArmySlotViewModel(i, this.OnCreatureSlotChanged, this.MoveSlot, this.ClearSlot)));

            this.Finder = new CreatureFinderViewModel(gameVersion, () => this.HasFreeSlot, this.AddCreature, this.SendToSlot, this.DescribeSlot);

            this.Refresh();
        }

        /// <summary>
        /// Gets the finder that looks up creatures by name, town and level and adds them to the first free slot.
        /// </summary>
        public CreatureFinderViewModel Finder { get; }

        /// <summary>
        /// Gets whether the army can be edited. False for the garrison of a castle while a hero is garrisoned in it.
        /// </summary>
        public bool IsEditable => !(this.isReadOnly?.Invoke() ?? false);

        /// <summary>
        /// Gets what the whole army is worth, such as "HP Total 1,200 · Cost 5,000 · AI 800 · Fight 700". Empty for an empty army.
        /// </summary>
        public string ArmySummary { get; private set; } = string.Empty;

        /// <summary>
        /// Gets whether the army has an empty slot for a creature from the finder.
        /// </summary>
        public bool HasFreeSlot => this.FirstFreeSlot(this.getCreatures()) > 0;

        /// <summary>
        /// Gets the creature slots of the army, in order.
        /// </summary>
        public ObservableCollection<ArmySlotViewModel> CreatureSlots { get; }

        /// <summary>
        /// Gets the creature names to pick from, with an empty entry first.
        /// </summary>
        public IReadOnlyList<string> AvailableCreatures { get; }

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

        /// <summary>
        /// Shows the current army again. Call after the army changed.
        /// </summary>
        public void Refresh()
        {
            var creatures = this.getCreatures();

            this.isRefreshing = true;
            try
            {
                for (var i = 0; i < this.CreatureSlots.Count; i++)
                {
                    var slot = creatures.FindSlot(i + 1);
                    var creature = slot == null ? null : Creature.FromByte(slot.CreatureCode);

                    this.CreatureSlots[i].InitializeSlot(
                        creature?.Name,
                        (int)(slot?.CreatureCount ?? 0),
                        canMoveLeft: i > 0,
                        canMoveRight: i < this.CreatureSlots.Count - 1,
                        info: creature?.GetInfo(this.gameVersion));
                }
            }
            finally
            {
                this.isRefreshing = false;
            }

            this.ArmySummary = this.SummarizeArmy(creatures);
            this.RaisePropertyChanged(nameof(this.ArmySummary));

            // The finder's add buttons and summary depend on whether a slot is free.
            this.RaisePropertyChanged(nameof(this.HasFreeSlot));
            this.RaisePropertyChanged(nameof(this.IsEditable));
            this.Finder.Refresh();
        }

        /// <summary>
        /// Swaps the stack in a slot with the one next to it. <paramref name="direction"/> is -1 for left, 1 for right.
        /// An empty neighbour just receives the stack.
        /// </summary>
        public void MoveSlot(int index, int direction)
        {
            var target = index + direction;
            if (this.isRefreshing || index < 0 || index >= this.CreatureSlots.Count || target < 0 || target >= this.CreatureSlots.Count)
            {
                return;
            }

            this.update(creatures => creatures.Swap(index + 1, target + 1));
        }

        /// <summary>
        /// Empties a slot.
        /// </summary>
        public void ClearSlot(int index)
        {
            if (this.isRefreshing || index < 0 || index >= this.CreatureSlots.Count)
            {
                return;
            }

            this.update(creatures => creatures.WithoutSlot(index + 1));
        }

        private string DescribeSlot(int slot)
        {
            var stack = this.getCreatures().FindSlot(slot);
            var creature = stack == null ? null : Creature.FromByte(stack.CreatureCode);
            return creature == null || stack == null ? "empty" : $"{creature.Name} ×{stack.CreatureCount}";
        }

        private string SummarizeArmy(Army creatures)
        {
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

        private void OnCreatureSlotChanged(int index, string? creatureName, int amount)
        {
            if (this.isRefreshing)
            {
                return;
            }

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
    }
}
