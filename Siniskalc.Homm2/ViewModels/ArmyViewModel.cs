namespace Siniskalc.Homm2.ViewModels
{
    using System;
    using System.Linq;
    using Siniskalc.Core.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Homm2.Entities;

    /// <summary>
    /// The army of a hero or the garrison of a castle in Heroes of Might and Magic II: five slots and a finder to add creatures. The slots,
    /// the commands and what is shown are in <see cref="ArmyViewModelBase{TSlot}"/>; this reads the army it is given and writes every edit
    /// back through <c>update</c>.
    /// </summary>
    public sealed class ArmyViewModel : HeroesArmyViewModelBase<Creature, StackSlotViewModel>
    {
        private const int MinStackSize = 1;

        /// <summary>
        /// Initializes a new instance of the <see cref="ArmyViewModel"/> class.
        /// </summary>
        /// <param name="catalog">The catalog.</param>
        /// <param name="gameVersion">The game version.</param>
        /// <param name="getArmy">The get army.</param>
        /// <param name="update">The update.</param>
        public ArmyViewModel(GameCatalog catalog, GameVersion gameVersion, Func<IArmy<Creature>> getArmy, Action<Func<IArmy<Creature>, IArmy<Creature>>> update)
            : base(
                catalog.Creatures.Where(c => c.IsAvailableIn(gameVersion)).ToList(),
                c => c.Info,
                Constants.ArmySlotCount,
                MinStackSize,
                (int)Constants.MaxCreatureCount,
                getArmy,
                update,
                (i, changed, move, clear) => new StackSlotViewModel(i, changed, move, clear))
        {
            this.Refresh();
        }

        /// <inheritdoc/>
        protected override ArmySummaryDetail SummaryDetail => ArmySummaryDetail.CombatAndCost;

        /// <inheritdoc/>
        protected override System.Collections.Generic.IReadOnlyList<StackFact> FactsOf(Creature creature, int amount)
        {
            var totals = new StackTotals(creature.Info, amount);
            return
            [
                .. totals.ToHealthAndDamageFacts(),
                totals.CostFact,
                new("Town", creature.Faction?.ToString() ?? "Neutral"),
                new("Level", creature.Tier.ToString()),
            ];
        }
    }
}
