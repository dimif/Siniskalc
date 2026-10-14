namespace Siniskalc.Homm2.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// A node of the navigation tree: the game, a player (whose page shows the player's resources, and which holds the player's heroes
    /// and towns), a group of heroes or towns, a hero, or a castle.
    /// </summary>
    public sealed class NavNodeViewModel : AbstractViewModelBase
    {
        private readonly Action<NavNodeViewModel>? onSelected;
        private bool isSelected;
        private bool isExpanded;
        private bool isVisible = true;
        private bool hasChanges;
        private string visitingHeroName = string.Empty;

        public NavNodeViewModel(string title, string subtitle, string colorHex, NavNodeKind kind, int index = -1, int tab = 0, Action<NavNodeViewModel>? onSelected = null)
        {
            this.Title = title;
            this.Subtitle = subtitle;
            this.ColorHex = colorHex;
            this.Kind = kind;
            this.Index = index;
            this.Tab = tab;
            this.onSelected = onSelected;
        }

        public string Title { get; }

        /// <summary>
        /// Gets the smaller text beside the title, e.g. the race and level of a hero.
        /// </summary>
        public string Subtitle { get; }

        /// <summary>
        /// Gets the colour of the marker beside the title, e.g. "#C0392B".
        /// </summary>
        public string ColorHex { get; }

        public NavNodeKind Kind { get; }

        /// <summary>
        /// Gets the number of the hero, the castle or the player (its colour) the node stands for, or -1 for a node that is none of them.
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// Gets the tab of the player's page the node opens: 0 for the resources, 1 for the heroes, 2 for the towns.
        /// </summary>
        public int Tab { get; }

        public ObservableCollection<NavNodeViewModel> Children { get; } = [];

        /// <summary>
        /// Gets or sets whether a node that holds others is expanded when no filter is applied.
        /// </summary>
        public bool DefaultExpanded { get; set; }

        /// <summary>
        /// Gets or sets the name of the hero that visits the castle of the node, or nothing; shown after the race.
        /// </summary>
        public string VisitingHeroName
        {
            get => this.visitingHeroName;
            set
            {
                if (this.visitingHeroName != value)
                {
                    this.visitingHeroName = value;
                    this.RaisePropertyChanged(nameof(this.VisitingHeroName));
                    this.RaisePropertyChanged(nameof(this.VisitingHeroSuffix));
                }
            }
        }

        /// <summary>
        /// Gets the visiting hero as it follows the race, such as " · Ariel", or nothing.
        /// </summary>
        public string VisitingHeroSuffix => this.visitingHeroName.Length > 0 ? $" · {this.visitingHeroName}" : string.Empty;

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
        /// Gets or sets whether the node, or one below it, has unsaved edits.
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
