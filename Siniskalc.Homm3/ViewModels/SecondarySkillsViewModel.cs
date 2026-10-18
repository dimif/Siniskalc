namespace Siniskalc.Homm3.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Models;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// ViewModel for the secondary skills page.
    /// The hero screen shows up to <see cref="Constants.SkillSlots.Max"/> skills, in slots. The game also applies
    /// skills a hero has a level in but that have no slot, so this page lists every skill and not only the slots.
    /// The slots and the list are in <see cref="SecondarySkillsViewModelBase"/>; this reads from the hero's skills and writes every edit
    /// back through <c>update</c>. Call <see cref="SecondarySkillsViewModelBase.Refresh"/> after they changed.
    /// </summary>
    public sealed class SecondarySkillsViewModel : SecondarySkillsViewModelBase
    {
        private readonly Func<SecondarySkills> getSkills;
        private readonly Action<Func<SecondarySkills, SecondarySkills>> update;
        private readonly IReadOnlyList<SecondarySkill> knownSkills;

        public SecondarySkillsViewModel(
            GameVersion gameVersion,
            Func<SecondarySkills> getSkills,
            Action<Func<SecondarySkills, SecondarySkills>> update)
            : this(SecondarySkill.All.Where(s => s.IsAvailableIn(gameVersion)).ToList(), getSkills, update)
        {
        }

        private SecondarySkillsViewModel(
            IReadOnlyList<SecondarySkill> knownSkills,
            Func<SecondarySkills> getSkills,
            Action<Func<SecondarySkills, SecondarySkills>> update)
            : base(knownSkills.Select(s => s.Name).ToList(), Constants.SkillSlots.Max)
        {
            this.knownSkills = knownSkills;
            this.getSkills = getSkills;
            this.update = update;

            this.Refresh();
        }

        public override bool SupportsHiding => true;

        public override int ExtraCount => this.getSkills().ExtraSkills.Count();

        public override string AllSkillsNote => "The game only allows a hero to learn eight skills, but it applies every skill that has a level.";

        protected override int UsedSlots => this.getSkills().SlotCount;

        protected override string? SkillInSlot(int slot) => this.getSkills().GetSkillInSlot(slot)?.Name;

        protected override string LevelInSlot(int slot) =>
            this.getSkills().GetSkillInSlot(slot) is { } skill ? SkillLevelNames.FromLevel(this.getSkills().GetSkillLevel(skill)) : SkillLevelNames.Basic;

        protected override string LevelOf(string skillName) => SkillLevelNames.FromLevel(this.getSkills().GetSkillLevel(this.Find(skillName)!));

        protected override int SlotOf(string skillName) => this.getSkills().GetSlotOf(this.Find(skillName)!);

        protected override void ReplaceSkillInSlot(int slot, string skillName)
        {
            var skill = this.Find(skillName)!;
            this.update(skills => skills.WithSkillInSlot(slot, skill));
        }

        protected override void AddSkill(string skillName)
        {
            var skill = this.Find(skillName)!;
            this.update(skills => skills.WithSkill(skill, SkillLevel.Beginner));
        }

        protected override void SetLevelInSlot(int slot, string levelName)
        {
            var level = SkillLevelNames.ToLevel(levelName);
            if (level != SkillLevel.None)
            {
                this.update(skills => skills.GetSkillInSlot(slot) is { } skill ? skills.WithSkill(skill, level) : skills);
            }
        }

        protected override void SetLevelOf(string skillName, string levelName)
        {
            var skill = this.Find(skillName)!;
            this.update(skills => skills.WithSkillLevel(skill, SkillLevelNames.ToLevel(levelName)));
        }

        protected override void SwapSlots(int first, int second) => this.update(skills => skills.Swap(first, second));

        protected override void RemoveSlot(int slot) => this.update(skills => skills.WithoutSlot(slot));

        protected override void HideSlot(int slot) => this.update(skills => skills.WithSlotAsExtra(slot));

        protected override void ShowExtraSkill(string skillName)
        {
            var skill = this.Find(skillName)!;
            this.update(skills => skills.WithExtraSkillInSlot(skill));
        }

        private SecondarySkill? Find(string name) => this.knownSkills.FirstOrDefault(s => s.Name == name);
    }
}
