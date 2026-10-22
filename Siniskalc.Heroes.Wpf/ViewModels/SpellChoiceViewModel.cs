namespace Siniskalc.Heroes.Wpf.ViewModels
{
    using System;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// One spell in the spells list of a hero: whether the hero has learned it, and whether something else (an artifact) makes it
    /// available. It knows no game; the game's row adds its spell.
    /// </summary>
    public class SpellChoiceViewModel : AbstractViewModelBase
    {
        private readonly Action<SpellChoiceViewModel, bool> onLearnedChanged;
        private bool isLearned;
        private bool isAvailable;
        private bool isSelected;

        /// <param name="name">The name of the spell.</param>
        /// <param name="group">The heading the spell is listed under, such as its school; none for one list.</param>
        /// <param name="level">The level of the spell, or 0 when the game has no levels.</param>
        /// <param name="kind">What the spell is for, such as "Combat", or nothing.</param>
        /// <param name="onLearnedChanged">Called with the row and the new value when the user ticks or unticks it.</param>
        public SpellChoiceViewModel(string name, string group, int level, string kind, Action<SpellChoiceViewModel, bool> onLearnedChanged)
        {
            this.Name = name;
            this.Group = group;
            this.Level = level;
            this.Kind = kind;
            this.onLearnedChanged = onLearnedChanged;
        }

        public string Name { get; }

        /// <summary>
        /// Gets the heading the spell is listed under.
        /// </summary>
        public string Group { get; }

        /// <summary>
        /// Gets the spell level, or 0 when the game has no levels.
        /// </summary>
        public int Level { get; }

        public string Kind { get; }

        /// <summary>
        /// Gets or sets whether the row is selected in the list. Changing what is learned for a selected row changes it for all selected rows.
        /// </summary>
        public bool IsSelected
        {
            get => this.isSelected;
            set
            {
                if (this.isSelected != value)
                {
                    this.isSelected = value;
                    this.RaisePropertyChanged(nameof(this.IsSelected));
                }
            }
        }

        /// <summary>
        /// Gets or sets whether the hero has the spell in their spell book.
        /// </summary>
        public bool IsLearned
        {
            get => this.isLearned;
            set
            {
                if (this.isLearned != value)
                {
                    this.isLearned = value;
                    this.RaisePropertyChanged(nameof(this.IsLearned));
                    this.onLearnedChanged(this, value);
                }
            }
        }

        /// <summary>
        /// Gets whether the hero can cast the spell through an artifact. The game works this out, so it cannot be edited.
        /// </summary>
        public bool IsAvailable => this.isAvailable;

        /// <summary>
        /// Gets what makes the spell available, such as the artifacts that grant it, or nothing for a spell that is not available.
        /// </summary>
        public string Source { get; private set; } = string.Empty;

        /// <summary>
        /// Shows the spell without reporting a change.
        /// </summary>
        public void Show(bool isLearned, bool isAvailable, string source = "")
        {
            this.isLearned = isLearned;
            this.isAvailable = isAvailable;
            this.Source = isAvailable ? source : string.Empty;
            this.RaisePropertyChanged(nameof(this.Source));
            this.RaisePropertyChanged(nameof(this.IsLearned));
            this.RaisePropertyChanged(nameof(this.IsAvailable));
        }
    }
}
