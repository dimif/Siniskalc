namespace Siniskalc.Heroes.Wpf.ViewModels
{
    using System;
    using System.Linq;
    using System.Windows.Input;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// One of the slots the hero screen shows a secondary skill in. A slot is either filled or free.
    /// </summary>
    public sealed class SkillSlotViewModel : AbstractViewModelBase
    {
        private readonly Action<int, string> onSkillChosen;
        private readonly Action<int, string> onLevelChosen;
        private string? skillName;
        private string levelName = SkillLevelNames.Basic;
        private bool canMoveUp;
        private bool canMoveDown;

        /// <param name="index">The slot, from 0.</param>
        /// <param name="onSkillChosen">Called with the slot and skill name when the user picks a skill.</param>
        /// <param name="onLevelChosen">Called with the slot and level name when the user picks a level.</param>
        /// <param name="onMove">Called with the slot and direction (-1 up, 1 down).</param>
        /// <param name="onClear">Called with the slot when the user removes the skill.</param>
        /// <param name="onHide">Called with the slot when the user keeps the skill but takes it off the hero screen.</param>
        public SkillSlotViewModel(
            int index,
            Action<int, string> onSkillChosen,
            Action<int, string> onLevelChosen,
            Action<int, int> onMove,
            Action<int> onClear,
            Action<int> onHide)
        {
            this.Index = index;
            this.onSkillChosen = onSkillChosen;
            this.onLevelChosen = onLevelChosen;
            this.MoveUpCommand = new DelegateCommand(_ => onMove(index, -1), _ => this.canMoveUp);
            this.MoveDownCommand = new DelegateCommand(_ => onMove(index, 1), _ => this.canMoveDown);
            this.ClearCommand = new DelegateCommand(_ => onClear(index), _ => this.IsFilled);
            this.HideCommand = new DelegateCommand(_ => onHide(index), _ => this.IsFilled);
        }

        /// <summary>
        /// Gets the place of the slot, from 0.
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// Gets the slot as the player sees it, from 1.
        /// </summary>
        public int DisplayNumber => this.Index + 1;

        /// <summary>
        /// Gets the command that moves the skill of the slot up the hero screen.
        /// </summary>
        public ICommand MoveUpCommand { get; }

        /// <summary>
        /// Gets the command that moves the skill of the slot down the hero screen.
        /// </summary>
        public ICommand MoveDownCommand { get; }

        /// <summary>
        /// Gets the command that removes the skill of the slot.
        /// </summary>
        public ICommand ClearCommand { get; }

        /// <summary>
        /// Gets the command that takes the skill off the hero screen but keeps its level.
        /// </summary>
        public ICommand HideCommand { get; }

        /// <summary>
        /// Gets a value indicating whether the slot holds a skill.
        /// </summary>
        public bool IsFilled => !string.IsNullOrEmpty(this.skillName);

        /// <summary>
        /// Gets or sets the skill in the slot. Picking one in a free slot adds it at basic level.
        /// </summary>
        public string? SkillName
        {
            get => this.skillName;
            set
            {
                if (!string.IsNullOrEmpty(value) && value != this.skillName)
                {
                    this.onSkillChosen(this.Index, value);
                }
            }
        }

        /// <summary>
        /// Gets or sets the level of the skill in the slot, as shown to the player (Basic, Advanced or Expert).
        /// </summary>
        public string LevelName
        {
            get => this.levelName;
            set
            {
                if (!string.IsNullOrEmpty(value) && value != this.levelName && this.IsFilled)
                {
                    this.onLevelChosen(this.Index, value);
                }
            }
        }

        /// <summary>
        /// Gets three flags, one per level step, for the row of pips.
        /// </summary>
        public bool[] Pips => [.. Enumerable.Range(1, 3).Select(step => this.IsFilled && SkillLevelNames.ToNumber(this.levelName) >= step)];

        /// <summary>
        /// Shows the slot without reporting a change.
        /// </summary>
        public void Show(string? skillName, string levelName, bool canMoveUp, bool canMoveDown)
        {
            this.skillName = skillName;
            this.levelName = levelName ?? SkillLevelNames.Basic;
            this.canMoveUp = canMoveUp;
            this.canMoveDown = canMoveDown;

            this.RaiseAllPropertiesChanged();
        }
    }
}
