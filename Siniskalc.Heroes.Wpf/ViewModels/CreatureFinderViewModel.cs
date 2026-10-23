namespace Siniskalc.Heroes.Wpf.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using Siniskalc.Core;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.ViewModels;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// A finder for the creatures of any game. The search text is matched against the name, the town, the level ("level 3") and whether
    /// it is an upgraded form: "castle", "dragon", "level 6" and "knight upgraded" all work.
    /// Picking one adds a new stack of <see cref="AddAmount"/> to the army, and the right-click menu sends it to a slot.
    /// </summary>
    public sealed class CreatureFinderViewModel : FinderViewModel<CreatureChoiceViewModel>
    {
        private readonly Func<bool> hasFreeSlot;
        private readonly Action<ICreature, int, int>? sendToSlot;
        private readonly Func<int, string>? describeSlot;
        private readonly int slotCount;
        private readonly int minAmount;
        private readonly int maxAmount;
        private int addAmount = 1;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreatureFinderViewModel"/> class.
        /// </summary>
        /// <param name="creatures">The creatures to list, each with its stats for the game version of the savegame.</param>
        /// <param name="slotCount">How many slots the army has.</param>
        /// <param name="minAmount">The least a stack can have.</param>
        /// <param name="maxAmount">The most a stack can have.</param>
        /// <param name="hasFreeSlot">Tells whether the army has an empty slot for a creature.</param>
        /// <param name="add">Called with the creature and the size of the stack when the user picks one.</param>
        /// <param name="sendToSlot">Called with the creature, a slot (from 1) and a count when the user sends some of it there; the right-click menu is left out when null.</param>
        /// <param name="describeSlot">Says what is in a slot (from 1), for its menu entry.</param>
        /// <param name="actionLabel">The text of the button of each row.</param>
        public CreatureFinderViewModel(
            IEnumerable<(ICreature Creature, CreatureInfo Info)> creatures,
            int slotCount,
            int minAmount,
            int maxAmount,
            Func<bool> hasFreeSlot,
            Action<ICreature, int> add,
            Action<ICreature, int, int>? sendToSlot = null,
            Func<int, string>? describeSlot = null,
            string actionLabel = "Add")
            : base(CreateRows(creatures), nameof(CreatureChoiceViewModel.Faction), string.Empty, "creatures", FinderColumns)
        {
            this.slotCount = slotCount;
            this.minAmount = minAmount;
            this.maxAmount = maxAmount;
            this.hasFreeSlot = hasFreeSlot;
            this.sendToSlot = sendToSlot;
            this.describeSlot = describeSlot;

            this.SetAction(actionLabel, choice => add(choice.Creature, this.addAmount), _ => this.hasFreeSlot());
            this.StartFiltering();
        }

        /// <summary>
        /// Gets or sets how many creatures a new stack from the finder starts with.
        /// </summary>
        public int AddAmount
        {
            get => this.addAmount;
            set
            {
                value = RangeGuard.Clamp(value, this.minAmount, this.maxAmount);
                if (this.addAmount != value)
                {
                    this.addAmount = value;
                    this.RaisePropertyChanged(nameof(this.AddAmount));
                }
            }
        }

        /// <inheritdoc/>
        public override string Summary =>
            this.hasFreeSlot() ? base.Summary : $"{base.Summary}. All {this.slotCount} slots are in use; empty one to add more";

        /// <inheritdoc/>
        public override string? RowToolTipPath => nameof(CreatureChoiceViewModel.Details);

        /// <inheritdoc/>
        public override string SearchPlaceholder => "Search by name, level, town...";

        /// <inheritdoc/>
        public override string SearchHint => "Type any of: a name, a town (castle, knight...), a level (level 3) or basic / upgraded";

        private static IReadOnlyList<FinderColumn> FinderColumns { get; } =
        [
            new("Creature", nameof(CreatureChoiceViewModel.Name), new GridLength(130)),
            new("Level", nameof(CreatureChoiceViewModel.Level), new GridLength(50)),
            new("Att", nameof(CreatureChoiceViewModel.Attack), new GridLength(42), FinderColumnKind.MutedText, "Attack skill"),
            new("Def", nameof(CreatureChoiceViewModel.Defense), new GridLength(42), FinderColumnKind.MutedText, "Defense skill"),
            new("Damage", nameof(CreatureChoiceViewModel.DamageRange), new GridLength(64), FinderColumnKind.MutedText, "Damage per creature"),
            new("HP", nameof(CreatureChoiceViewModel.Health), new GridLength(48), FinderColumnKind.MutedText, "Hit points per creature"),
            new("Speed", nameof(CreatureChoiceViewModel.Speed), new GridLength(52), FinderColumnKind.MutedText, "Speed in combat"),
            new("Cost", nameof(CreatureChoiceViewModel.Cost), new GridLength(52), FinderColumnKind.MutedText, "Gold cost of one creature"),
            new("Resources", nameof(CreatureChoiceViewModel.Resources), new GridLength(110), FinderColumnKind.MutedText, "What one creature costs besides gold"),
            new("Special", nameof(CreatureChoiceViewModel.Special), new GridLength(1, GridUnitType.Star), FinderColumnKind.MutedText, "What the creature can do beyond fighting; point at a row to read it in full"),
        ];

        /// <summary>
        /// Offers "Send 1 to slot" and "Send 100 to slot" with one entry per slot; a slot that is taken gets its stack replaced.
        /// </summary>
        /// <param name="row">The row that was right-clicked.</param>
        /// <returns>The entries of the menu, none when the finder sends nowhere.</returns>
        public override IReadOnlyList<FinderMenuItem> GetRowMenu(object? row)
        {
            if (row is not CreatureChoiceViewModel choice || this.sendToSlot == null)
            {
                return [];
            }

            return [this.SendMenu(choice, 1), this.SendMenu(choice, 100)];
        }

        /// <inheritdoc/>
        protected override bool Matches(CreatureChoiceViewModel choice) => this.TextMatches(choice.Name, choice.Faction, $"Level {choice.Level}", choice.Form);

        /// <summary>
        /// Lists creatures town by town, neutral ones last, and inside a town by level, the basic form before the upgraded one.
        /// </summary>
        private static List<CreatureChoiceViewModel> CreateRows(IEnumerable<(ICreature Creature, CreatureInfo Info)> creatures) => creatures
            .Select(c => new CreatureChoiceViewModel(c.Creature, c.Info))
            .OrderBy(c => c.FactionOrder).ThenBy(c => c.Level).ThenBy(c => c.Creature.IsUpgrade).ThenBy(c => c.Creature.Code)
            .ToList();

        private FinderMenuItem SendMenu(CreatureChoiceViewModel choice, int count)
        {
            var send = this.sendToSlot!;
            var slots = Enumerable.Range(1, this.slotCount)
                .Select(slot => new FinderMenuItem(
                    this.describeSlot == null ? $"Slot {slot}" : $"Slot {slot}: {this.describeSlot(slot)}",
                    () => send(choice.Creature, slot, count)))
                .ToList();
            return new FinderMenuItem($"Send {count} to slot", Children: slots);
        }
    }
}
