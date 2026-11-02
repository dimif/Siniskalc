namespace Siniskalc.Heroes.Wpf.ViewModels
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows.Input;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// The secondary skills of a hero: the slots the hero screen shows, filled or free, and a list of every skill with the hero's level in
    /// it. It knows no game: skills and levels are names, and the game's view model says what the hero has and how to change it. The view
    /// (<c>SecondarySkillsView</c>) only binds to this. Call <see cref="Refresh"/> after the skills changed.
    /// </summary>
    public abstract class SecondarySkillsViewModelBase : PageViewModelBase
    {
        private readonly int maxSlots;
        private bool isRefreshing;

        /// <summary>
        /// Initializes a new instance of the <see cref="SecondarySkillsViewModelBase"/> class.
        /// </summary>
        /// <param name="skillNames">The names of every skill the game has.</param>
        /// <param name="maxSlots">The number of skills a hero can show on the hero screen.</param>
        protected SecondarySkillsViewModelBase(IReadOnlyList<string> skillNames, int maxSlots)
            : base("Secondary Skills")
        {
            this.maxSlots = maxSlots;
            this.SkillNames = skillNames;

            this.Slots = new ObservableCollection<SkillSlotViewModel>(
                Enumerable.Range(0, maxSlots).Select(i =>
                    new SkillSlotViewModel(i, this.ChooseSkill, this.ChooseSlotLevel, this.MoveSkill, this.ClearSkill, this.HideSkill)));

            this.AllSkills = new ObservableCollection<SkillRowViewModel>(
                skillNames.Select(name => new SkillRowViewModel(name, this.ChooseLevel, this.ShowSkill, this.HideSkillByName)));
        }

        /// <summary>
        /// Gets the slots of the hero screen, filled or free.
        /// </summary>
        public ObservableCollection<SkillSlotViewModel> Slots { get; }

        /// <summary>
        /// Gets the first half of the slots, which the page shows in its left column.
        /// </summary>
        public IEnumerable<SkillSlotViewModel> LeftSlots => this.Slots.Take(this.Slots.Count / 2);

        /// <summary>
        /// Gets the second half of the slots, which the page shows in its right column.
        /// </summary>
        public IEnumerable<SkillSlotViewModel> RightSlots => this.Slots.Skip(this.Slots.Count / 2);

        /// <summary>
        /// Gets the first half of the skills, which the page shows in its left column.
        /// </summary>
        public IEnumerable<SkillRowViewModel> LeftSkills => this.AllSkills.Take((this.AllSkills.Count + 1) / 2);

        /// <summary>
        /// Gets the second half of the skills, which the page shows in its right column.
        /// </summary>
        public IEnumerable<SkillRowViewModel> RightSkills => this.AllSkills.Skip((this.AllSkills.Count + 1) / 2);

        /// <summary>
        /// Gets every skill the game has, with the hero's level in it.
        /// </summary>
        public ObservableCollection<SkillRowViewModel> AllSkills { get; }

        /// <summary>
        /// Gets the names to pick a skill from.
        /// </summary>
        public IReadOnlyList<string> SkillNames { get; }

        /// <summary>
        /// Gets the levels a slot can have, which are all but None.
        /// </summary>
        public IReadOnlyList<string> SlotLevelNames => SkillLevelNames.WithoutNone;

        /// <summary>
        /// Gets the levels a row of the list of every skill can have, None included.
        /// </summary>
        public IReadOnlyList<string> RowLevelNames => SkillLevelNames.WithNone;

        /// <summary>
        /// Gets how many slots are taken, as text, e.g. "3 of 8 slots used".
        /// </summary>
        public string SlotSummary => $"{this.UsedSlots} of {this.maxSlots} slots used";

        /// <summary>
        /// Gets whether the game can keep a skill's level without showing the skill on the hero screen (Show and Hide).
        /// </summary>
        public virtual bool SupportsHiding => false;

        /// <summary>
        /// Gets a sentence under the heading of the list of every skill, or nothing.
        /// </summary>
        public virtual string AllSkillsNote => string.Empty;

        /// <summary>
        /// Gets a value indicating whether the hero has a level in skills that the hero screen does not show.
        /// </summary>
        public bool HasExtraSkills => this.ExtraCount > 0;

        /// <summary>
        /// Gets the number of skills the hero has a level in that the hero screen does not show.
        /// </summary>
        public virtual int ExtraCount => 0;

        /// <summary>
        /// Gets a sentence about the skills the hero screen does not show.
        /// </summary>
        public string ExtraSummary => this.ExtraCount == 1
            ? "1 skill is applied by the game but not shown on the hero screen."
            : $"{this.ExtraCount} skills are applied by the game but not shown on the hero screen.";

        /// <summary>
        /// Gets how many slots hold a skill; they are the first ones.
        /// </summary>
        protected abstract int UsedSlots { get; }

        /// <summary>
        /// Shows the hero's skills again. Call after they changed.
        /// </summary>
        public void Refresh()
        {
            var used = this.UsedSlots;

            this.isRefreshing = true;
            try
            {
                for (var i = 0; i < this.Slots.Count; i++)
                {
                    var name = i < used ? this.SkillInSlot(i) : null;
                    this.Slots[i].Show(name, name == null ? SkillLevelNames.Basic : this.LevelInSlot(i), canMoveUp: i > 0 && i < used, canMoveDown: i < used - 1);
                }

                var hasFreeSlot = used < this.maxSlots;
                foreach (var row in this.AllSkills)
                {
                    row.Show(this.LevelOf(row.Name), this.SlotOf(row.Name), hasFreeSlot);
                }

                this.RaiseAllPropertiesChanged();
            }
            finally
            {
                this.isRefreshing = false;
            }

            // The move, clear and show buttons depend on how many skills there are.
            CommandManager.InvalidateRequerySuggested();
        }

        /// <summary>
        /// Moves the skill in a slot up (<paramref name="direction"/> -1) or down (1) the hero screen.
        /// </summary>
        /// <param name="slot">The slot, from 0.</param>
        /// <param name="direction">-1 to move it up, 1 to move it down.</param>
        public void MoveSkill(int slot, int direction)
        {
            var target = slot + direction;
            var count = this.UsedSlots;
            if (this.isRefreshing || slot < 0 || slot >= count || target < 0 || target >= count)
            {
                return;
            }

            this.SwapSlots(slot, target);
        }

        /// <summary>
        /// Removes the skill in a slot. Later skills move up a slot, as the game needs the slots to be contiguous.
        /// </summary>
        /// <param name="slot">The slot, from 0.</param>
        public void ClearSkill(int slot)
        {
            if (this.isRefreshing || slot < 0 || slot >= this.UsedSlots)
            {
                return;
            }

            this.RemoveSlot(slot);
        }

        /// <summary>
        /// Gets the name of the skill in a slot (from 0) that is in use.
        /// </summary>
        /// <param name="slot">The slot, from 0.</param>
        /// <returns>The name of the skill.</returns>
        protected abstract string? SkillInSlot(int slot);

        /// <summary>
        /// Gets the level of the skill in a slot (from 0) that is in use, as <see cref="SkillLevelNames"/> names it.
        /// </summary>
        /// <param name="slot">The slot, from 0.</param>
        /// <returns>The name of the level.</returns>
        protected abstract string LevelInSlot(int slot);

        /// <summary>
        /// Gets the hero's level in a skill, as <see cref="SkillLevelNames"/> names it ("None" for a skill the hero has not learned).
        /// </summary>
        /// <param name="skillName">The name of the skill.</param>
        /// <returns>The name of the level.</returns>
        protected abstract string LevelOf(string skillName);

        /// <summary>
        /// Gets the slot (from 0) the hero screen shows a skill in, or -1 when it does not.
        /// </summary>
        /// <param name="skillName">The name of the skill.</param>
        /// <returns>The slot, or -1.</returns>
        protected abstract int SlotOf(string skillName);

        /// <summary>
        /// Puts another skill in a slot that is in use, keeping what the game keeps of its level.
        /// </summary>
        /// <param name="slot">The slot, from 0.</param>
        /// <param name="skillName">The name of the skill to put there.</param>
        protected abstract void ReplaceSkillInSlot(int slot, string skillName);

        /// <summary>
        /// Adds a skill in the first free slot, at basic level.
        /// </summary>
        /// <param name="skillName">The name of the skill.</param>
        protected abstract void AddSkill(string skillName);

        /// <summary>
        /// Gives the skill in a slot a level.
        /// </summary>
        /// <param name="slot">The slot, from 0.</param>
        /// <param name="levelName">The level, as <see cref="SkillLevelNames"/> names it.</param>
        protected abstract void SetLevelInSlot(int slot, string levelName);

        /// <summary>
        /// Gives the hero a level in a skill; "None" takes the skill away.
        /// </summary>
        /// <param name="skillName">The name of the skill.</param>
        /// <param name="levelName">The name of the level.</param>
        protected abstract void SetLevelOf(string skillName, string levelName);

        /// <summary>
        /// Swaps the skills of two slots, with their levels.
        /// </summary>
        /// <param name="first">The first slot, from 0.</param>
        /// <param name="second">The second slot, from 0.</param>
        protected abstract void SwapSlots(int first, int second);

        /// <summary>
        /// Removes the skill of a slot; the later skills move up a slot.
        /// </summary>
        /// <param name="slot">The slot, from 0.</param>
        protected abstract void RemoveSlot(int slot);

        /// <summary>
        /// Takes a skill off the hero screen but keeps its level. Only called when <see cref="SupportsHiding"/>.
        /// </summary>
        /// <param name="slot">The slot, from 0.</param>
        protected virtual void HideSlot(int slot)
        {
        }

        /// <summary>
        /// Puts a skill that has a level but no slot in a free slot. Only called when <see cref="SupportsHiding"/>.
        /// </summary>
        /// <param name="skillName">The name of the skill.</param>
        protected virtual void ShowExtraSkill(string skillName)
        {
        }

        private void HideSkill(int slot)
        {
            if (this.SupportsHiding && !this.isRefreshing && slot >= 0 && slot < this.UsedSlots)
            {
                this.HideSlot(slot);
            }
        }

        private void HideSkillByName(string name) => this.HideSkill(this.SlotOf(name));

        private void ShowSkill(string name)
        {
            if (this.SupportsHiding && !this.isRefreshing && this.SkillNames.Contains(name))
            {
                this.ShowExtraSkill(name);
            }
        }

        private void ChooseSkill(int slot, string name)
        {
            if (this.isRefreshing || !this.SkillNames.Contains(name))
            {
                return;
            }

            var count = this.UsedSlots;
            if (slot < count)
            {
                this.ReplaceSkillInSlot(slot, name);
                this.Refresh();
            }
            else if (slot == count)
            {
                // Only the first free slot can be filled, as the slots stay contiguous.
                this.AddSkill(name);
            }
        }

        private void ChooseSlotLevel(int slot, string levelName)
        {
            if (this.isRefreshing || SkillLevelNames.ToNumber(levelName) == 0 || slot < 0 || slot >= this.UsedSlots)
            {
                return;
            }

            this.SetLevelInSlot(slot, levelName);
            this.Refresh();
        }

        private void ChooseLevel(string name, string levelName)
        {
            if (this.isRefreshing || !this.SkillNames.Contains(name))
            {
                return;
            }

            this.SetLevelOf(name, levelName);
            this.Refresh();
        }
    }
}
