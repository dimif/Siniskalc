namespace Siniskalc.Homm2.ViewModels
{
    using System;
    using System.Linq;
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
        protected override System.Collections.Generic.IReadOnlyList<StackFact> FactsOf(Creature creature, int amount) =>
        [
            .. new StackTotals(creature.Info, amount).ToHealthAndDamageFacts(),
            new("Town", creature.Faction?.ToString() ?? "Neutral"),
            new("Level", creature.Tier.ToString()),
            new("Growth", $"{creature.Info.Growth} a week"),
        ];
    }
}
