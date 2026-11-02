namespace Siniskalc.Heroes.Wpf.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Input;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// The buttons that fill the dwellings of a castle in any game: a week of growth or a fixed number for all of the built dwellings, or
    /// emptying them, and the same for one dwelling (<see cref="AddWeek"/>). A dwelling that is not built is left alone.
    /// </summary>
    public sealed class DwellingStockActions
    {
        /// <summary>
        /// How many creatures the "+100" button adds to each dwelling.
        /// </summary>
        public const int Hundred = 100;

        /// <param name="dwellings">Gives the dwellings of the castle now (a game may rebuild its rows when the castle changes).</param>
        public DwellingStockActions(Func<IReadOnlyList<IStockedDwelling>> dwellings)
        {
            this.AddWeekToAllCommand = new DelegateCommand(_ => Each(dwellings, AddWeek));
            this.AddHundredToAllCommand = new DelegateCommand(_ => Each(dwellings, d => d.Stock += Hundred));
            this.ClearAllCommand = new DelegateCommand(_ => Each(dwellings, d => d.Stock = 0));
        }

        /// <summary>
        /// Gets the command that adds a week of growth to every dwelling that is built ("All +1 wk").
        /// </summary>
        public ICommand AddWeekToAllCommand { get; }

        /// <summary>
        /// Gets the command that adds 100 creatures to every dwelling that is built ("All +100").
        /// </summary>
        public ICommand AddHundredToAllCommand { get; }

        /// <summary>
        /// Gets the command that empties every dwelling that is built ("All 0").
        /// </summary>
        public ICommand ClearAllCommand { get; }

        /// <summary>
        /// Adds a week of growth to one dwelling, as the dwelling says it gains it.
        /// </summary>
        /// <param name="dwelling">The dwelling.</param>
        public static void AddWeek(IStockedDwelling dwelling) => dwelling.Stock += dwelling.WeeklyGrowthAmount;

        /// <summary>
        /// Returns the command that adds a week of growth to one dwelling, available while the dwelling is built.
        /// </summary>
        /// <param name="dwelling">The dwelling.</param>
        /// <returns>The command.</returns>
        public static ICommand AddWeekCommandOf(IStockedDwelling dwelling) => new DelegateCommand(_ => AddWeek(dwelling), _ => dwelling.CanStock);

        private static void Each(Func<IReadOnlyList<IStockedDwelling>> dwellings, Action<IStockedDwelling> act)
        {
            foreach (var dwelling in dwellings().Where(d => d.CanStock).ToList())
            {
                act(dwelling);
            }
        }
    }
}
