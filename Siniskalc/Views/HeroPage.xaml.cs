namespace Siniskalc.Views
{
    using System;
    using System.Windows.Controls;
    using Siniskalc.ViewModels;
    using Siniskalc.Wpf.Views;

    /// <summary>
    /// Interaction logic for HeroPage.xaml
    /// Sidebar idea from https://github.com/TacticDevGit/WPF-Sidebar-Navigation
    /// Now follows MVVM: receives HeroViewModel as DataContext and uses settable Game/Hero properties for child pages
    /// </summary>
    public partial class HeroPage : Page
    {
        private readonly GeneralInfoPage generalPage;
        private readonly ArmyPage creaturesPage;
        private readonly ArtifactsPage artifactsPage;
        private readonly BackpackPage backpackPage;
        private readonly SecondarySkillsPage skillsPage;
        private readonly SpellsPage spellsPage;
        private readonly HexPage hexPage;
        private HeroViewModel heroViewModel;

        public HeroPage(HeroViewModel viewModel)
        {
            this.InitializeComponent();

            this.heroViewModel = viewModel;
            this.DataContext = this.heroViewModel;

            this.generalPage = new GeneralInfoPage(this.heroViewModel.GeneralInfoViewModel);
            this.creaturesPage = new ArmyPage(this.heroViewModel.ArmyViewModel);
            this.artifactsPage = new ArtifactsPage(this.heroViewModel.ArtifactsViewModel);
            this.backpackPage = new BackpackPage(this.heroViewModel.BackpackViewModel);
            this.skillsPage = new SecondarySkillsPage(this.heroViewModel.SecondarySkillsViewModel);
            this.spellsPage = new SpellsPage(this.heroViewModel.SpellsViewModel);
            this.hexPage = new HexPage();
            this.hexPage.BindHero(this.heroViewModel);

            this.OpenGeneralTab();
        }

        public HeroViewModel HeroViewModel
        {
            set
            {
                this.heroViewModel = value;
                this.DataContext = value;
                this.RefreshHeroContext();
            }
        }

        public void OpenGeneralTab()
        {
            if (this.sidebar.SelectedIndex != 0)
            {
                this.sidebar.SelectedIndex = 0;
                return;
            }

            this.NavigateToSelectedPage();
        }

        private void SidebarSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.NavigateToSelectedPage();
        }

        private void NavigateToSelectedPage()
        {
            if (this.sidebar.SelectedItem is not NavButton { Text: var selectedText })
            {
                return;
            }

            Page? page = selectedText switch
            {
                "General" => this.generalPage,
                "Skills" => this.skillsPage,
                "Creatures" => this.creaturesPage,
                "Spells" => this.spellsPage,
                "Artifacts" => this.artifactsPage,
                "Backpack" => this.backpackPage,
                "Hex" => this.hexPage,
                _ => null,
            };

            if (page != null && !ReferenceEquals(this.navframe.Content, page))
            {
                this.navframe.Navigate(page);
            }
        }

        private void RefreshHeroContext()
        {
            this.hexPage.BindHero(this.heroViewModel);
        }
    }
}
