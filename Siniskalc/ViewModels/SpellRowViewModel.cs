namespace Siniskalc.ViewModels
{
    using System;
    using Siniskalc.Entities;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// One spell in the spells list: whether the hero has learned it, and whether an artifact makes it available.
    /// </summary>
    public sealed class SpellRowViewModel : AbstractViewModelBase
    {
        private readonly Action<Spell, bool> onLearnedChanged;
        private bool isLearned;
        private bool isAvailable;
        private bool isSelected;

        public SpellRowViewModel(Spell spell, Action<Spell, bool> onLearnedChanged)
        {
            this.Spell = spell;
            this.onLearnedChanged = onLearnedChanged;
        }

        public Spell Spell { get; }

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

        public string Name => this.Spell.Name;

        /// <summary>
        /// Gets the heading the spell is listed under.
        /// </summary>
        public string School => this.Spell.School switch
        {
            SpellSchool.Wind => "Air magic",
            SpellSchool.Water => "Water magic",
            SpellSchool.Fire => "Fire magic",
            SpellSchool.Earth => "Earth magic",
            _ => "All schools",
        };

        /// <summary>
        /// Gets the spell level, 1 to 5.
        /// </summary>
        public int Level => this.Spell.Level;

        public string Kind => this.Spell.Type == SpellType.Adventure ? "Adventure" : "Combat";

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
                    this.onLearnedChanged(this.Spell, value);
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
