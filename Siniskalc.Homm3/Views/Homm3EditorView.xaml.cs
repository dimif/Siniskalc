namespace Siniskalc.Homm3.Views
{
    using System;
    using System.Windows.Controls;
    using Siniskalc.Homm3.ViewModels;

    /// <summary>
    /// What the shell shows for the HoMM3 editor: the navigation tree of the players, heroes and towns, and the page of what is picked in it.
    /// </summary>
    public partial class Homm3EditorView : UserControl
    {
        public Homm3EditorView()
        {
            this.InitializeComponent();
            this.navigator.PageFactory = CreatePage;
        }

        private static Page CreatePage(object viewModel) => viewModel switch
        {
            HeroViewModel hero => new HeroPage(hero),
            PlayerViewModel player => new PlayerPage(player),
            CastleViewModel castle => new CastlePage(castle),
            OverviewViewModel overview => new OverviewPage(overview),
            _ => throw new InvalidOperationException($"No page for {viewModel.GetType().Name}."),
        };
    }
}
