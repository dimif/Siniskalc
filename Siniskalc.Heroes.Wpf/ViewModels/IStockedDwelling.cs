namespace Siniskalc.Heroes.Wpf.ViewModels
{
    /// <summary>
    /// A dwelling of a castle in any game, as far as the buttons that fill it are concerned: whether it holds creatures, how many wait in it
    /// and how many it gains each week.
    /// </summary>
    public interface IStockedDwelling
    {
        /// <summary>
        /// Gets a value indicating whether the dwelling is built, so creatures can wait in it.
        /// </summary>
        bool CanStock { get; }

        /// <summary>
        /// Gets or sets how many creatures wait in the dwelling. Setting more than the dwelling can hold, or less than none, stops at the limit.
        /// </summary>
        long Stock { get; set; }

        /// <summary>
        /// Gets how many creatures the dwelling gains each week, 0 when it is not built.
        /// </summary>
        int WeeklyGrowthAmount { get; }
    }
}
