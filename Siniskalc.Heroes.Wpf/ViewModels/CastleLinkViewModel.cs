namespace Siniskalc.Heroes.Wpf.ViewModels
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Input;
    using Siniskalc.Core.Entities;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.ViewModels;

    /// <summary>
    /// A castle in a player's card of the overview or on its page. Opening it shows the castle's page.
    /// </summary>
    public sealed class CastleLinkViewModel : ILink
    {
        /// <param name="name">The name of the castle.</param>
        /// <param name="detail">What is told on the right of the name, such as the faction.</param>
        /// <param name="open">Opens the page of the castle.</param>
        /// <param name="status">What is under the name, such as "City · Citadel · 3 dwellings".</param>
        /// <param name="isCapital">Whether the castle is a capital, which marks it.</param>
        /// <param name="grailText">The name of the Grail structure that is built, or nothing.</param>
        /// <param name="heroes">The heroes in the castle.</param>
        public CastleLinkViewModel(string name, string detail, ICommand open, string status = "", bool isCapital = false, string grailText = "", IReadOnlyList<CastleHeroRowViewModel>? heroes = null)
        {
            this.Name = name;
            this.Detail = detail;
            this.OpenCommand = open;
            this.Status = status;
            this.IsCapital = isCapital;
            this.GrailText = grailText;
            this.Heroes = heroes ?? [];
        }

        /// <summary>
        /// Returns the link to a castle of any game: its name, its faction on the right, and under the name what stands in it (the words of the
        /// settlement, then the Mage Guild and how many dwellings are built).
        /// </summary>
        /// <typeparam name="TColor">The colour of the game.</typeparam>
        /// <typeparam name="TCreature">The creature of the game.</typeparam>
        /// <param name="castle">The castle.</param>
        /// <param name="open">Opens the page of the castle.</param>
        /// <param name="settlement">What the game calls the settlement, such as "City" and "Citadel", or "Castle".</param>
        /// <param name="dwellings">How many dwellings are built in the castle.</param>
        /// <param name="isCapital">Whether the castle is a capital, which marks it.</param>
        /// <param name="grailText">The name of the Grail structure that is built, or nothing.</param>
        /// <param name="heroes">The heroes in the castle.</param>
        public static CastleLinkViewModel For<TColor, TCreature>(
            ICastleModel<TColor, TCreature> castle,
            ICommand open,
            IEnumerable<string> settlement,
            int dwellings,
            bool isCapital = false,
            string grailText = "",
            IReadOnlyList<CastleHeroRowViewModel>? heroes = null)
            where TColor : class, IPlayerColor
            where TCreature : class, IGameEntity
        {
            var status = string.Join(
                " · ",
                settlement
                    .Append(castle.MageGuildLevel > 0 ? $"Mage Guild {castle.MageGuildLevel}" : string.Empty)
                    .Append(OverviewText.Count(dwellings, "dwelling", "dwellings"))
                    .Where(text => text.Length > 0));
            return new CastleLinkViewModel(castle.Name, castle.Faction?.Name ?? string.Empty, open, status, isCapital, grailText, heroes);
        }

        public string Name { get; }

        public string Detail { get; }

        /// <summary>
        /// Gets what is told under the name: the hall and the fortification of a settlement, the Mage Guild and how many dwellings are built.
        /// </summary>
        public string Status { get; }

        /// <summary>
        /// Gets whether the castle is a capital, which is to say it has a Capitol.
        /// </summary>
        public bool IsCapital { get; }

        /// <summary>
        /// Gets the name of the Grail structure that is built, e.g. "Colossus", or nothing.
        /// </summary>
        public string GrailText { get; }

        public bool HasGrail => this.GrailText.Length > 0;

        /// <summary>
        /// Gets the heroes in the castle, each as a link to the hero.
        /// </summary>
        public IReadOnlyList<CastleHeroRowViewModel> Heroes { get; }

        public ICommand OpenCommand { get; }
    }
}
