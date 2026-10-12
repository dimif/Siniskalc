namespace Siniskalc.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Input;
    using Siniskalc.Core.Data;
    using Siniskalc.Data;
    using Siniskalc.Models;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// A hero in a castle, as a line of a page: whether it is garrisoned or visiting, and a link to the page of the hero.
    /// </summary>
    public sealed class CastleHeroRowViewModel
    {
        public CastleHeroRowViewModel(CastleHeroRole kind, string name, ICommand open)
        {
            this.Kind = kind;
            this.Name = name;
            this.OpenCommand = open;
        }

        public CastleHeroRole Kind { get; }

        /// <summary>
        /// Gets the role as the page words it: "Garrisoned" or "Visiting".
        /// </summary>
        public string Role => this.Kind.ToString();

        /// <summary>
        /// Gets the role as it reads after the name, such as "garrisoned".
        /// </summary>
        public string RoleText => this.Role.ToLowerInvariant();

        public string Name { get; }

        public ICommand OpenCommand { get; }

        /// <summary>
        /// Returns the heroes in <paramref name="castle"/>: the garrisoned one, then the one at the gate, if there are any.
        /// </summary>
        /// <param name="heroes">The heroes of the savefile, to find the name and the place of a hero by the id the castle holds.</param>
        /// <param name="openHero">Opens the page of the hero at an index among the heroes; the links do nothing without it.</param>
        public static IReadOnlyList<CastleHeroRowViewModel> For(CastleModel castle, HeroChunkCollection heroes, Action<int>? openHero)
        {
            var rows = new List<CastleHeroRowViewModel>();
            Add(rows, CastleHeroRole.Garrisoned, castle.GarrisonedHeroId);
            Add(rows, CastleHeroRole.Visiting, castle.VisitingHeroId);
            return rows;

            void Add(List<CastleHeroRowViewModel> list, CastleHeroRole kind, int? heroId)
            {
                if (heroId == null)
                {
                    return;
                }

                // The castle holds the id of the hero, which is not its place among the heroes.
                var hero = heroes.FindById(heroId.Value);
                var index = hero?.Index ?? -1;
                list.Add(new CastleHeroRowViewModel(kind, hero?.Name ?? $"Unknown hero {heroId}", new DelegateCommand(_ => openHero?.Invoke(index), _ => index >= 0 && openHero != null)));
            }
        }
    }
}
