namespace Siniskalc.Wpf.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    /// <summary>
    /// A node of the navigation tree: the game, a player (whose page shows the player's resources, and which holds the player's heroes
    /// and towns), a group of heroes or towns, a hero, or a castle. What a game adds to the look of a node are its
    /// <see cref="Badges"/> and its <see cref="Notes"/>.
    /// </summary>
    public sealed class NavNodeViewModel : AbstractViewModelBase
    {
        private static readonly IReadOnlyList<NavBadge> NoBadges = [];
        private static readonly IReadOnlyList<string> NoNotes = [];

        private readonly Action<NavNodeViewModel>? onSelected;
        private bool isSelected;
        private bool isExpanded;
        private bool isVisible = true;
        private bool hasChanges;
        private IReadOnlyList<NavBadge> badges = NoBadges;
        private IReadOnlyList<string> notes = NoNotes;

        /// <summary>
        /// Initializes a new instance of the <see cref="NavNodeViewModel"/> class.
        /// </summary>
        /// <param name="title">The text of the node.</param>
        /// <param name="subtitle">The smaller text under it.</param>
        /// <param name="colorHex">The colour of the node as a hex string.</param>
        /// <param name="kind">What the node stands for.</param>
        /// <param name="index">The number of the hero, castle, player or other thing the node is about; -1 when it is none.</param>
        /// <param name="tab">The tab of the page to show when the node is picked.</param>
        /// <param name="onSelected">Called with the node when the user picks it.</param>
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

        /// <summary>
        /// Gets the text of the node.
        /// </summary>
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
        /// Gets what the node stands for, which tells the tree how to draw it and what page it opens.
        /// </summary>
        public NavNodeKind Kind { get; }

        /// <summary>
        /// Gets the number of the hero, the castle or the player the node stands for, or -1 for a node that is none of them.
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// Gets the tab of the player's page the node opens: 0 for the resources, 1 for the heroes, 2 for the towns.
        /// </summary>
        public int Tab { get; }

        /// <summary>
        /// Gets a value indicating whether the node is a hero.
        /// </summary>
        public bool IsHero => this.Kind == NavNodeKind.Hero;

        /// <summary>
        /// Gets a value indicating whether the node is a castle.
        /// </summary>
        public bool IsCastle => this.Kind == NavNodeKind.Castle;

        /// <summary>
        /// Gets the nodes under this one.
        /// </summary>
        public ObservableCollection<NavNodeViewModel> Children { get; } = [];

        /// <summary>
        /// Gets or sets whether a node that holds others is expanded when no filter is applied.
        /// </summary>
        public bool DefaultExpanded { get; set; }

        /// <summary>
        /// Gets or sets the marks shown in front of the name.
        /// </summary>
        public IReadOnlyList<NavBadge> Badges
        {
            get => this.badges;
            set
            {
                value ??= NoBadges;
                if (!this.badges.SequenceEqual(value))
                {
                    this.badges = value;
                    this.RaisePropertyChanged(nameof(this.Badges));
                }
            }
        }

        /// <summary>
        /// Gets or sets what is told after the subtitle, one text each, such as the hero that visits a castle or the days a player has left.
        /// </summary>
        public IReadOnlyList<string> Notes
        {
            get => this.notes;
            set
            {
                value ??= NoNotes;
                if (!this.notes.SequenceEqual(value))
                {
                    this.notes = value;
                    this.RaisePropertyChanged(nameof(this.Notes));
                    this.RaisePropertyChanged(nameof(this.NotesText));
                }
            }
        }

        /// <summary>
        /// Gets the notes as they follow the subtitle, such as " · Ariel · 6 days left", or nothing.
        /// </summary>
        public string NotesText => string.Concat(this.notes.Select(n => $" · {n}"));

        /// <summary>
        /// Gets or sets a value indicating whether the node is the selected one in the tree.
        /// </summary>
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

        /// <summary>
        /// Gets or sets a value indicating whether the node shows its children.
        /// </summary>
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
