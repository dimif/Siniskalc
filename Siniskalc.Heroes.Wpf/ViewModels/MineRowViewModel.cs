namespace Siniskalc.Heroes.Wpf.ViewModels
{
    using System;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Entities;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// A mine of the map read from the savegame through its edit session: the row every game shows, with the owner picked from the colours of the
    /// game. A game whose mine has more (the guard of a HoMM3 mine) derives from it.
    /// </summary>
    /// <typeparam name="TMine">The mine of the game.</typeparam>
    /// <typeparam name="TColor">The colour of the game, which is an enumeration of its own.</typeparam>
    public class MineRowViewModel<TMine, TColor> : MineRowViewModelBase
        where TMine : class, IMineModel<TColor>
        where TColor : AbstractEnumeration, IPlayerColor
    {
        /// <param name="session">Edits the mine.</param>
        /// <param name="withOwner">Gives the mine with another owner.</param>
        /// <param name="ownerToolTip">What the list of the owners says about a change of the owner in the game.</param>
        /// <param name="hasLevels">Whether the map of the game has two levels, so a place on it says which level it is on.</param>
        public MineRowViewModel(IEditSession<TMine> session, Func<TMine, TColor, TMine> withOwner, string ownerToolTip, bool hasLevels)
            : base(session.Index, () => session.Current.Kind, () => session.Current.Position, () => !session.Current.Owner.IsNeutral, OwnerOf(session, withOwner, ownerToolTip), hasLevels)
        {
            this.Session = session;
        }

        /// <summary>
        /// Gets the session that edits the mine.
        /// </summary>
        protected IEditSession<TMine> Session { get; }

        private static ChoiceFieldViewModel OwnerOf(IEditSession<TMine> session, Func<TMine, TColor, TMine> withOwner, string toolTip) => new(
            "Owner",
            OwnerNames<TColor>.Choices,
            () => OwnerNames<TColor>.Of(session.Current.Owner),
            name => session.Update(mine => withOwner(mine, OwnerNames<TColor>.Parse(name))),
            toolTip: toolTip);
    }
}
