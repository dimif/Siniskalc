namespace Siniskalc.Homm3.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Homm3.Entities;

    /// <summary>
    /// ViewModel for an army of Heroes of Might and Magic III: the seven creature slots of a hero, and a <see cref="Finder"/> to look up
    /// creatures to add. The slots, the commands and what is shown are in <see cref="ArmyViewModelBase{TSlot}"/>; this reads the army
    /// from the hero it is given and writes every edit back through <c>update</c>.
    /// Call <see cref="ArmyViewModelBase{TSlot}.Refresh"/> after the army changed.
    /// </summary>
    public sealed class ArmyViewModel : HeroesArmyViewModelBase<Creature, ArmySlotViewModel>
    {
        private readonly EntityCatalog catalog;

        public ArmyViewModel(
            EntityCatalog catalog,
            Func<IArmy<Creature>> getCreatures,
            Action<Func<IArmy<Creature>, IArmy<Creature>>> update,
            Func<bool>? isReadOnly = null)
            : base(
                catalog.Creatures.All,
                c => c.Info,
                Constants.CreatureSlotCount,
                Constants.Army.MinStackSize,
                Constants.Army.MaxStackSize,
                getCreatures,
                update,
                (i, changed, move, clear) => new ArmySlotViewModel(i, changed, move, clear),
                isReadOnly)
        {
            this.catalog = catalog;
            this.Refresh();
        }

        /// <inheritdoc/>
        protected override bool SummaryHasValues => true;

        /// <inheritdoc/>
        protected override IReadOnlyList<StackFact> FactsOf(Creature creature, int amount) => new StackTotals(creature.Info, amount).ToFacts();

        /// <inheritdoc/>
        protected override void AfterSlotRead(ArmySlotViewModel slot, StackContent? stack)
        {
            var creature = stack == null || stack.CreatureName.Length == 0 ? null : this.catalog.Creatures.FindByName(stack.CreatureName);
            slot.SetInfo(creature?.Info);
        }
    }
}
