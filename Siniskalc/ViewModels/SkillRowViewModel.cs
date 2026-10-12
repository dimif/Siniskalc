namespace Siniskalc.ViewModels
{
    using System;
    using System.Windows.Input;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// One skill in the list of every secondary skill: its level, and whether the hero screen shows it.
    /// </summary>
    public sealed class SkillRowViewModel : AbstractViewModelBase
    {
        private readonly Action<string, string> onLevelChosen;
        private string levelName = SkillLevelNames.None;
        private int slot = -1;
        private bool canShow;

        /// <param name="name">The skill's name.</param>
        /// <param name="onLevelChosen">Called with the skill name and level name when the user picks a level.</param>
        /// <param name="onShow">Called with the skill name to put an extra skill in a free slot.</param>
        /// <param name="onHide">Called with the skill name to take a skill off the hero screen but keep its level.</param>
        public SkillRowViewModel(string name, Action<string, string> onLevelChosen, Action<string> onShow, Action<string> onHide)
        {
            this.Name = name;
            this.onLevelChosen = onLevelChosen;
            this.ShowCommand = new DelegateCommand(_ => onShow(name), _ => this.IsExtra && this.canShow);
            this.HideCommand = new DelegateCommand(_ => onHide(name), _ => this.IsInSlot);
        }

        public string Name { get; }

        public ICommand ShowCommand { get; }

        public ICommand HideCommand { get; }

        /// <summary>
        /// Gets or sets the skill's level, as shown to the player; "None" removes the skill.
        /// </summary>
        public string LevelName
        {
            get => this.levelName;
            set
            {
                if (!string.IsNullOrEmpty(value) && value != this.levelName)
                {
                    this.onLevelChosen(this.Name, value);
                }
            }
        }

        public bool HasSkill => this.levelName != SkillLevelNames.None;

        public bool IsInSlot => this.slot >= 0;

        /// <summary>
        /// Gets whether the hero has a level in the skill, but the hero screen does not show it.
        /// </summary>
        public bool IsExtra => this.HasSkill && !this.IsInSlot;

        /// <summary>
        /// Gets where the hero screen shows the skill, for the list.
        /// </summary>
        public string Placement => !this.HasSkill ? string.Empty : this.IsInSlot ? $"Slot {this.slot + 1}" : "Not shown on the hero screen";

        /// <summary>
        /// Shows the skill without reporting a change.
        /// </summary>
        public void Show(string levelName, int slot, bool canShow)
        {
            this.levelName = levelName;
            this.slot = slot;
            this.canShow = canShow;

            this.RaiseAllPropertiesChanged();
        }
    }
}
