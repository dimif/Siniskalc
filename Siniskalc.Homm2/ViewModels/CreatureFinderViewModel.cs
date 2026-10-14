namespace Siniskalc.Homm2.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// A finder for the creatures of Heroes of Might and Magic II. The search text is matched against the name, the town and the level
    /// ("level 3"): "knight", "dragon" and "level 6" all work.
    /// </summary>
    public sealed class CreatureFinderViewModel : CreatureFinderViewModelBase<CreatureChoiceViewModel>
    {
        public CreatureFinderViewModel(
            GameVersion gameVersion,
            Func<bool> hasFreeSlot,
            Action<Creature, int> add,
            Action<Creature, int, int>? sendToSlot = null,
            Func<int, string>? describeSlot = null)
            : base(
                CreateRows(gameVersion),
                nameof(CreatureChoiceViewModel.Race),
                FinderColumns,
                Constants.ArmySlotCount,
                1,
                (int)Constants.MaxCreatureCount,
                hasFreeSlot,
                (choice, count) => add(choice.Creature, count),
                sendToSlot == null ? null : (choice, slot, count) => sendToSlot(choice.Creature, slot, count),
                describeSlot)
        {
        }

        public override string SearchPlaceholder => "Search by name, level, town...";

        public override string SearchHint => "Type any of: a name, a town (knight, wizard...) or a level (level 3)";

        private static IReadOnlyList<FinderColumn> FinderColumns { get; } =
        [
            new("Creature", nameof(CreatureChoiceViewModel.Name), new GridLength(150)),
            new("Town", nameof(CreatureChoiceViewModel.Race), new GridLength(100)),
            new("Level", nameof(CreatureChoiceViewModel.Level), new GridLength(60), FinderColumnKind.MutedText, "The level of the creature among those of its town"),
            new("Growth", nameof(CreatureChoiceViewModel.Growth), new GridLength(1, GridUnitType.Star), FinderColumnKind.MutedText, "How many a dwelling grows each week"),
        ];

        protected override bool Matches(CreatureChoiceViewModel choice) => this.TextMatches(choice.Name, choice.Race, $"Level {choice.Level}");

        /// <summary>
        /// Lists creatures town by town, neutral ones last, and inside a town by level.
        /// </summary>
        private static List<CreatureChoiceViewModel> CreateRows(GameVersion gameVersion) => Creature.All
            .Where(c => c.IsAvailableIn(gameVersion))
            .OrderBy(c => c.Race == null ? int.MaxValue : (int)c.Race).ThenBy(c => c.Level).ThenBy(c => c.Code)
            .Select(c => new CreatureChoiceViewModel(c))
            .ToList();
    }
}
