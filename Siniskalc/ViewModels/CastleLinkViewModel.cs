namespace Siniskalc.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Input;
    using Siniskalc.Entities;
    using Siniskalc.Models;

    /// <summary>
    /// A castle in a player's card of the overview. Opening it shows the castle's page.
    /// </summary>
    public sealed class CastleLinkViewModel
    {
        public CastleLinkViewModel(string name, string detail, ICommand open, string hallText = "", string fortText = "", bool isCapital = false, int mageGuildLevel = 0, int dwellings = 0, string grailText = "", IReadOnlyList<CastleHeroRowViewModel>? heroes = null)
        {
            this.Heroes = heroes ?? [];
            this.Name = name;
            this.Detail = detail;
            this.OpenCommand = open;
            this.HallText = hallText;
            this.FortText = fortText;
            this.IsCapital = isCapital;
            this.MageGuildLevel = mageGuildLevel;
            this.Dwellings = dwellings;
            this.GrailText = grailText;
        }

        /// <param name="heroes">The hero garrisoned in the castle and the one at its gate, as links; see <see cref="CastleHeroRowViewModel.For"/>.</param>
        public CastleLinkViewModel(CastleModel castle, ICommand open, IReadOnlyList<CastleHeroRowViewModel>? heroes = null)
            : this(
                castle.Name,
                castle.Faction?.Name ?? string.Empty,
                open,
                HallOf(castle),
                FortOf(castle),
                castle.HasBuilt(Building.Capitol),
                castle.MageGuildLevel,
                castle.Buildings.Count(b => b.IsDwelling),
                castle.Buildings.FirstOrDefault(b => b.IsGrail)?.Name ?? string.Empty,
                heroes)
        {
        }

        public string Name { get; }

        public string Detail { get; }

        /// <summary>
        /// Gets the hall that stands in the castle: Village, Town, City or Capitol.
        /// </summary>
        public string HallText { get; }

        /// <summary>
        /// Gets the settlement's fortification: Fort, Citadel, Castle, or empty when there is no fort.
        /// </summary>
        public string FortText { get; }

        /// <summary>
        /// Gets whether the castle is a capital, which is to say it has a Capitol.
        /// </summary>
        public bool IsCapital { get; }

        /// <summary>
        /// Gets the hall and the fortification on one line, e.g. "City · Citadel".
        /// </summary>
        public string Status => string.Join(" · ", new[]
        {
            this.HallText,
            this.FortText,
            this.MageGuildLevel > 0 ? $"Mage Guild {this.MageGuildLevel}" : string.Empty,
            $"{this.Dwellings} {(this.Dwellings == 1 ? "dwelling" : "dwellings")}",
        }.Where(t => t.Length > 0));

        /// <summary>
        /// Gets the level of the Mage Guild, 0 when there is none.
        /// </summary>
        public int MageGuildLevel { get; }

        /// <summary>
        /// Gets how many of the seven creature dwellings are built, 0 to 7.
        /// </summary>
        public int Dwellings { get; }

        /// <summary>
        /// Gets the name of the Grail structure that is built, e.g. "Colossus", or nothing.
        /// </summary>
        public string GrailText { get; }

        public bool HasGrail => this.GrailText.Length > 0;

        /// <summary>
        /// Gets the hero garrisoned in the castle and the one at its gate, each as a link to the hero.
        /// </summary>
        public IReadOnlyList<CastleHeroRowViewModel> Heroes { get; }

        public ICommand OpenCommand { get; }

        private static string HallOf(CastleModel castle)
        {
            return castle.HasBuilt(Building.Capitol) ? "Capitol"
                : castle.HasBuilt(Building.CityHall) ? "City"
                : castle.HasBuilt(Building.TownHall) ? "Town"
                : "Village";
        }

        private static string FortOf(CastleModel castle)
        {
            return castle.HasBuilt(Building.Castle) ? "Castle"
                : castle.HasBuilt(Building.Citadel) ? "Citadel"
                : castle.HasBuilt(Building.Fort) ? "Fort"
                : string.Empty;
        }
    }
}
