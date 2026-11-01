namespace Siniskalc.Homm3.ViewModels
{
    using System;
    using System.Linq;
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// A mine of the map: the row every game shows (what it gives, where its record says it is, who owns it) and the creatures that guard it,
    /// which are edited with the same army editor as a hero's creatures.
    /// </summary>
    public sealed class MineRowViewModel : MineRowViewModel<MineModel, PlayerColor>
    {
        public MineRowViewModel(IEditSession<MineModel> session)
            : base(
                session,
                (mine, owner) => mine with { Owner = owner },
                "Who owns the mine. The game works the income out at the start of the next day and draws the flag from the owner. Not tried in the game.",
                hasLevels: true)
        {
        }

        public override string Kind => this.Session.Current.Catalog.MapMetadata.Mine(this.Session.Current.Kind).Name;

        public override string ResourceName => this.Session.Current.Catalog.MapMetadata.Mine(this.Session.Current.Kind).Resource.ToString();

        /// <summary>
        /// Gets a few words about the guard: none, or how many slots it uses.
        /// </summary>
        public override string Summary
        {
            get
            {
                var count = this.Session.Current.Guard.Filled.Count();
                return count == 0 ? "Unguarded" : $"{count} of {Constants.CreatureSlotCount} slots";
            }
        }

        /// <summary>
        /// Gets the creatures that guard the mine.
        /// </summary>
        public IArmy<Creature> Guard => this.Session.Current.Guard;

        /// <summary>
        /// Changes the creatures that guard the mine.
        /// </summary>
        public void UpdateGuard(Func<IArmy<Creature>, IArmy<Creature>> edit) => this.Session.Update(mine => mine with { Guard = edit(mine.Guard) });
    }
}
