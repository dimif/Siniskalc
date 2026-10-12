namespace Siniskalc.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// A node of the navigation tree: the game, a player (whose page shows the player's resources, and which holds the
    /// player's heroes and towns), a group of heroes or towns, a hero, or a castle.
    /// </summary>
    public sealed class NavNodeViewModel : AbstractViewModelBase
    {
        private readonly Action<NavNodeViewModel>? onSelected;
        private bool isSelected;
        private bool isExpanded;
        private bool isVisible = true;
        private bool hasChanges;
        private bool isCapital;
        private string grailText = string.Empty;
        private string garrisonedHeroName = string.Empty;

        public NavNodeViewModel(string title, string subtitle, string colorHex, int heroIndex = -1, Action<NavNodeViewModel>? onSelected = null, NavNodeKind? kind = null, int playerIndex = -1, int castleIndex = -1, int tab = 0)
        {
            this.Tab = tab;
            this.Kind = kind ?? (heroIndex >= 0 ? NavNodeKind.Hero : NavNodeKind.Group);
            this.PlayerIndex = playerIndex;
            this.CastleIndex = castleIndex;
            this.Title = title;
            this.Subtitle = subtitle;
            this.ColorHex = colorHex;
            this.HeroIndex = heroIndex;
            this.onSelected = onSelected;
        }

        public string Title { get; }

        /// <summary>
        /// Gets the smaller text beside the title, e.g. the class and level of a hero.
        /// </summary>
        public string Subtitle { get; }

        /// <summary>
        /// Gets the colour of the marker beside the title, e.g. "#C0392B".
        /// </summary>
        public string ColorHex { get; }

        /// <summary>
        /// Gets the hero's index in the savefile, or -1 for a group.
        /// </summary>
        public int HeroIndex { get; }

        public NavNodeKind Kind { get; }

        /// <summary>
        /// Gets the player's index in the savefile, or -1 if the node is not a player's page.
        /// </summary>
        public int PlayerIndex { get; }

        /// <summary>
        /// Gets the castle's index in the savefile, or -1 if the node is not a castle's page.
        /// </summary>
        public int CastleIndex { get; }

        /// <summary>
        /// Gets the tab of the player's page the node opens: 0 for the resources, 1 for the heroes, 2 for the towns.
        /// </summary>
        public int Tab { get; }

        public bool IsHero => this.Kind == NavNodeKind.Hero;

        /// <summary>
        /// Gets or sets whether the castle of the node has a Capitol; shown as a crown in front of the name.
        /// </summary>
        public bool IsCapital
        {
            get => this.isCapital;
            set
            {
                if (this.isCapital != value)
                {
                    this.isCapital = value;
                    this.RaisePropertyChanged(nameof(this.IsCapital));
                }
            }
        }

        /// <summary>
        /// Gets or sets the name of the Grail structure the castle of the node has built, or nothing; shown as a mark in front of the name.
        /// </summary>
        public string GrailText
        {
            get => this.grailText;
            set
            {
                if (this.grailText != value)
                {
                    this.grailText = value;
                    this.RaisePropertyChanged(nameof(this.GrailText));
                    this.RaisePropertyChanged(nameof(this.HasGrail));
                }
            }
        }

        public bool HasGrail => this.grailText.Length > 0;

        public bool IsCastle => this.Kind == NavNodeKind.Castle;

        /// <summary>
        /// Gets or sets the name of the hero garrisoned in the castle of the node, or nothing; shown as a mark in front of the name and
        /// after the faction.
        /// </summary>
        public string GarrisonedHeroName
        {
            get => this.garrisonedHeroName;
            set
            {
                if (this.garrisonedHeroName != value)
                {
                    this.garrisonedHeroName = value;
                    this.RaisePropertyChanged(nameof(this.GarrisonedHeroName));
                    this.RaisePropertyChanged(nameof(this.HasGarrisonedHero));
                    this.RaisePropertyChanged(nameof(this.GarrisonedHeroSuffix));
                }
            }
        }

        /// <summary>
        /// Gets the garrisoned hero as it follows the faction, such as " · Nymus", or nothing.
        /// </summary>
        public string GarrisonedHeroSuffix => this.HasGarrisonedHero ? $" · {this.garrisonedHeroName}" : string.Empty;

        public bool HasGarrisonedHero => this.garrisonedHeroName.Length > 0;

        public ObservableCollection<NavNodeViewModel> Children { get; } = new ObservableCollection<NavNodeViewModel>();

        /// <summary>
        /// Gets or sets whether a node that holds others is expanded when no filter is applied.
        /// </summary>
        public bool DefaultExpanded { get; set; }

        public bool IsSelected
        {
            get => this.isSelected;
            set
            {
                if (this.isSelected == value)
                {
                    return;
                }

                this.isSelected = value;
                this.RaisePropertyChanged(nameof(this.IsSelected));

                if (value)
                {
                    this.onSelected?.Invoke(this);
                }
            }
        }

        public bool IsExpanded
        {
            get => this.isExpanded;
            set
            {
                if (this.isExpanded != value)
                {
                    this.isExpanded = value;
                    this.RaisePropertyChanged(nameof(this.IsExpanded));
                }
            }
        }

        /// <summary>
        /// Gets or sets whether the node passes the current filter.
        /// </summary>
        public bool IsVisible
        {
            get => this.isVisible;
            set
            {
                if (this.isVisible != value)
                {
                    this.isVisible = value;
                    this.RaisePropertyChanged(nameof(this.IsVisible));
                }
            }
        }

        /// <summary>
        /// Gets or sets whether the node, or a hero below it, has unsaved edits.
        /// </summary>
        public bool HasChanges
        {
            get => this.hasChanges;
            set
            {
                if (this.hasChanges != value)
                {
                    this.hasChanges = value;
                    this.RaisePropertyChanged(nameof(this.HasChanges));
                    this.RaisePropertyChanged(nameof(this.ChangesMark));
                }
            }
        }

        /// <summary>
        /// Gets the dot that follows the text of a node with unsaved edits, or nothing. Part of the text, so it sits on its baseline.
        /// </summary>
        public string ChangesMark => this.hasChanges ? "  ●" : string.Empty;

        /// <summary>
        /// Shows the node if its title matches <paramref name="filter"/>, or if any node below it does.
        /// A node that matches by its own title shows everything below it.
        /// </summary>
        public void ApplyFilter(string filter, bool parentMatches = false)
        {
            var matches = parentMatches || string.IsNullOrWhiteSpace(filter) || this.Title.Contains(filter.Trim(), StringComparison.OrdinalIgnoreCase);

            foreach (var child in this.Children)
            {
                child.ApplyFilter(filter, matches && this.Children.Count > 0);
            }

            this.IsVisible = matches || this.Children.Any(c => c.IsVisible);

            // Expand nodes while searching, so matches are not hidden inside them.
            if (this.Children.Count > 0)
            {
                this.IsExpanded = string.IsNullOrWhiteSpace(filter) ? this.DefaultExpanded : this.IsVisible;
            }
        }
    }
}
