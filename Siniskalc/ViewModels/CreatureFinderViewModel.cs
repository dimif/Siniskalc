namespace Siniskalc.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using Siniskalc.Core;
    using Siniskalc.Entities;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// A finder for creatures. The search text is matched against the name, the town, the level ("level 3") and whether it is
    /// an upgraded form: "castle", "dragon", "level 7" and "rampart upgraded" all work.
    /// Picking one adds a new stack of <see cref="AddAmount"/> to the army.
    /// </summary>
    public sealed class CreatureFinderViewModel : FinderViewModel<CreatureChoiceViewModel>
    {
        private readonly Func<bool> hasFreeSlot;
        private readonly Action<Creature, int, int>? sendToSlot;
        private readonly Func<int, string>? describeSlot;
        private int addAmount = 1;

        /// <param name="gameVersion">The game version; creatures from later versions are left out.</param>
        /// <param name="hasFreeSlot">Tells whether the army has an empty slot for a creature.</param>
        /// <param name="add">Called with the creature and the size of the stack when the user picks one.</param>
        /// <param name="sendToSlot">Called with the creature, a slot (from 1) and a count when the user sends some of it there; the right-click menu is left out when null.</param>
        /// <param name="describeSlot">Says what is in a slot (from 1), for its menu entry.</param>
        public CreatureFinderViewModel(
            GameVersion gameVersion,
            Func<bool> hasFreeSlot,
            Action<Creature, int> add,
            Action<Creature, int, int>? sendToSlot = null,
            Func<int, string>? describeSlot = null)
            : base(CreateRows(gameVersion), nameof(CreatureChoiceViewModel.Faction), string.Empty, "creatures", FinderColumns)
        {
            this.hasFreeSlot = hasFreeSlot;
            this.sendToSlot = sendToSlot;
            this.describeSlot = describeSlot;

            this.SetAction("Add", choice => add(choice.Creature, this.addAmount), _ => this.hasFreeSlot());
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
                value = RangeGuard.Clamp(value, Constants.Army.MinStackSize, Constants.Army.MaxStackSize);
                if (this.addAmount != value)
                {
                    this.addAmount = value;
                    this.RaisePropertyChanged(nameof(this.AddAmount));
                }
            }
        }

        public override string? RowToolTipPath => nameof(CreatureChoiceViewModel.Details);

        public override string SearchPlaceholder => "Search by name, level, castle...";

        public override string SearchHint => "Type any of: a name, a town (castle, inferno...), a level (level 3) or basic / upgraded";

        public override string Summary =>
            this.hasFreeSlot() ? base.Summary : $"{base.Summary}. All {Constants.CreatureSlotCount} slots are in use; empty one to add more";

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
        /// Offers "Send 1 to slot" with one entry per slot; a slot that is taken gets its stack replaced by one creature.
        /// </summary>
        public override IReadOnlyList<FinderMenuItem> GetRowMenu(object? row)
        {
            if (row is not CreatureChoiceViewModel choice || this.sendToSlot == null)
            {
                return [];
            }

            return [this.SendMenu(choice, 1), this.SendMenu(choice, 100)];
        }

        protected override bool Matches(CreatureChoiceViewModel choice)
        {
            return this.TextMatches(choice.Name, choice.Faction, $"Level {choice.Level}", choice.Form);
        }

        private static int FactionOrder(Faction faction) => faction == Faction.Neutral ? int.MaxValue : faction.Code;

        /// <summary>
        /// Lists creatures town by town, neutral ones last, and inside a town by level.
        /// </summary>
        private static List<CreatureChoiceViewModel> CreateRows(GameVersion gameVersion)
        {
            return Creature.All
                .Where(c => c.IsAvailableIn(gameVersion))
                .OrderBy(c => FactionOrder(c.Faction)).ThenBy(c => c.Tier).ThenBy(c => c.BaseCreatureCode != null).ThenBy(c => c.Name)
                .Select(c => new CreatureChoiceViewModel(c, gameVersion))
                .ToList();
        }

        private FinderMenuItem SendMenu(CreatureChoiceViewModel choice, int count)
        {
            var send = this.sendToSlot!;
            var slots = Enumerable.Range(1, Constants.CreatureSlotCount)
                .Select(slot => new FinderMenuItem(
                    this.describeSlot == null ? $"Slot {slot}" : $"Slot {slot}: {this.describeSlot(slot)}",
                    () => send(choice.Creature, slot, count)))
                .ToList();
            return new FinderMenuItem($"Send {count} to slot", Children: slots);
        }
    }
}
