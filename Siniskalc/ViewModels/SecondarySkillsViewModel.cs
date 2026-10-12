namespace Siniskalc.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows.Input;
    using Siniskalc.Entities;
    using Siniskalc.Models;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// ViewModel for the secondary skills page.
    /// The hero screen shows up to <see cref="Constants.SkillSlots.Max"/> skills, in slots. The game also applies
    /// skills a hero has a level in but that have no slot, so this page lists every skill and not only the slots.
    /// Reads from the hero's skills and writes every edit back through <c>update</c>; call <see cref="Refresh"/> after they changed.
    /// </summary>
    public sealed class SecondarySkillsViewModel : PageViewModelBase
    {
        private readonly Func<SecondarySkills> getSkills;
        private readonly Action<Func<SecondarySkills, SecondarySkills>> update;
        private readonly IReadOnlyList<SecondarySkill> knownSkills;
        private bool isRefreshing;

        public SecondarySkillsViewModel(
            GameVersion gameVersion,
            Func<SecondarySkills> getSkills,
            Action<Func<SecondarySkills, SecondarySkills>> update)
            : base("Secondary Skills")
        {
            this.getSkills = getSkills;
            this.update = update;
            this.knownSkills = SecondarySkill.All.Where(s => s.IsAvailableIn(gameVersion)).ToList();
            this.SkillNames = this.knownSkills.Select(s => s.Name).ToList();

            this.Slots = new ObservableCollection<SkillSlotViewModel>(
                Enumerable.Range(0, Constants.SkillSlots.Max).Select(i =>
                    new SkillSlotViewModel(i, this.ChooseSkill, this.ChooseSlotLevel, this.MoveSkill, this.ClearSkill, this.HideSkill)));

            this.AllSkills = new ObservableCollection<SkillRowViewModel>(
                this.knownSkills.Select(s => new SkillRowViewModel(s.Name, this.ChooseLevel, this.ShowSkill, this.HideSkillByName)));

            this.Refresh();
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
        /// Gets every skill the game version has, with the hero's level in it.
        /// </summary>
        public ObservableCollection<SkillRowViewModel> AllSkills { get; }

        /// <summary>
        /// Gets the names to pick a skill from.
        /// </summary>
        public IReadOnlyList<string> SkillNames { get; }

        public IReadOnlyList<string> SlotLevelNames => SkillLevelNames.WithoutNone;

        public IReadOnlyList<string> RowLevelNames => SkillLevelNames.WithNone;

        /// <summary>
        /// Gets how many slots are taken, as text, e.g. "3 of 8".
        /// </summary>
        public string SlotSummary => $"{this.getSkills().SlotCount} of {Constants.SkillSlots.Max} slots used";

        /// <summary>
        /// Gets the number of skills the hero has a level in that the hero screen does not show.
        /// </summary>
        public int ExtraCount => this.getSkills().ExtraSkills.Count();

        public bool HasExtraSkills => this.ExtraCount > 0;

        /// <summary>
        /// Gets a sentence about the skills the hero screen does not show.
        /// </summary>
        public string ExtraSummary => this.ExtraCount == 1
            ? "1 skill is applied by the game but not shown on the hero screen."
            : $"{this.ExtraCount} skills are applied by the game but not shown on the hero screen.";

        /// <summary>
        /// Shows the hero's skills again. Call after they changed.
        /// </summary>
        public void Refresh()
        {
            var skills = this.getSkills();

            this.isRefreshing = true;
            try
            {
                for (var i = 0; i < this.Slots.Count; i++)
                {
                    var skill = i < skills.SlotCount ? skills.GetSkillInSlot(i) : null;
                    this.Slots[i].Show(
                        skill?.Name,
                        skill == null ? SkillLevelNames.Basic : SkillLevelNames.FromLevel(skills.GetSkillLevel(skill)),
                        canMoveUp: i > 0 && i < skills.SlotCount,
                        canMoveDown: i < skills.SlotCount - 1);
                }

                var hasFreeSlot = skills.SlotCount < Constants.SkillSlots.Max;
                for (var i = 0; i < this.AllSkills.Count; i++)
                {
                    var skill = this.knownSkills[i];
                    this.AllSkills[i].Show(SkillLevelNames.FromLevel(skills.GetSkillLevel(skill)), skills.GetSlotOf(skill), hasFreeSlot);
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
        public void MoveSkill(int slot, int direction)
        {
            var target = slot + direction;
            var count = this.getSkills().SlotCount;
            if (this.isRefreshing || slot < 0 || slot >= count || target < 0 || target >= count)
            {
                return;
            }

            this.update(skills => skills.Swap(slot, target));
        }

        /// <summary>
        /// Removes the skill in a slot. Later skills move up a slot, as the game needs the slots to be contiguous.
        /// </summary>
        public void ClearSkill(int slot)
        {
            if (this.isRefreshing || slot < 0 || slot >= this.getSkills().SlotCount)
            {
                return;
            }

            this.update(skills => skills.WithoutSlot(slot));
        }

        private void HideSkill(int slot)
        {
            if (this.isRefreshing || slot < 0 || slot >= this.getSkills().SlotCount)
            {
                return;
            }

            this.update(skills => skills.WithSlotAsExtra(slot));
        }

        private void HideSkillByName(string name)
        {
            var skill = this.Find(name);
            var slot = skill == null ? -1 : this.getSkills().GetSlotOf(skill);
            this.HideSkill(slot);
        }

        private void ShowSkill(string name)
        {
            var skill = this.Find(name);
            if (!this.isRefreshing && skill != null)
            {
                this.update(skills => skills.WithExtraSkillInSlot(skill));
            }
        }

        private void ChooseSkill(int slot, string name)
        {
            var skill = this.Find(name);
            if (this.isRefreshing || skill == null)
            {
                return;
            }

            var count = this.getSkills().SlotCount;
            if (slot < count)
            {
                this.update(skills => skills.WithSkillInSlot(slot, skill));
                this.Refresh();
            }
            else if (slot == count)
            {
                // Only the first free slot can be filled, as the slots stay contiguous.
                this.update(skills => skills.WithSkill(skill, SkillLevel.Beginner));
            }
        }

        private void ChooseSlotLevel(int slot, string levelName)
        {
            var level = SkillLevelNames.ToLevel(levelName);
            if (this.isRefreshing || level == null || slot < 0 || slot >= this.getSkills().SlotCount)
            {
                return;
            }

            this.update(skills => skills.GetSkillInSlot(slot) is { } skill ? skills.WithSkill(skill, level.Value) : skills);
            this.Refresh();
        }

        private void ChooseLevel(string name, string levelName)
        {
            var skill = this.Find(name);
            if (this.isRefreshing || skill == null)
            {
                return;
            }

            this.update(skills => skills.WithSkillLevel(skill, SkillLevelNames.ToLevel(levelName)));
            this.Refresh();
        }

        private SecondarySkill? Find(string name) => this.knownSkills.FirstOrDefault(s => s.Name == name);
    }
}
