namespace Siniskalc.Heroes.Wpf.ViewModels
{
    /// <summary>
    /// The numbers of the tabs of the Neutral page, as <see cref="Siniskalc.Wpf.ViewModels.NavNodeViewModel.Tab"/> says them. Nobody has
    /// resources there, so the first tab is the heroes.
    /// </summary>
    public static class NeutralTabs
    {
        /// <summary>
        /// The index of the heroes tab.
        /// </summary>
        public const int Heroes = 0;
        /// <summary>
        /// The index of the towns tab.
        /// </summary>
        public const int Towns = 1;
        /// <summary>
        /// The index of the mines tab.
        /// </summary>
        public const int Mines = 2;
        /// <summary>
        /// The index of the dwellings tab.
        /// </summary>
        public const int Dwellings = 3;

        /// <summary>
        /// Returns the number of the tab of the Neutral page that holds what the tab <paramref name="ownerTab"/> of a player's page holds.
        /// </summary>
        public static int Of(int ownerTab) => ownerTab - OwnerTabs.Heroes;
    }
}
