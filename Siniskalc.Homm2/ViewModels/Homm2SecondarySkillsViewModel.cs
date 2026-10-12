namespace Siniskalc.Homm2.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// The secondary skills page of a hero of Heroes of Might and Magic II: up to eight skills in the order they were learned, which is
    /// what the hero screen shows, and the list of the fourteen skills with the hero's level in each. The slots and the list are in
    /// <see cref="SecondarySkillsViewModelBase"/>; this reads the hero's skills and writes every edit back through <c>update</c>.
    /// </summary>
    public sealed class Homm2SecondarySkillsViewModel : SecondarySkillsViewModelBase
    {
        private readonly Func<Homm2SecondarySkills> getSkills;
        private readonly Action<Func<Homm2SecondarySkills, Homm2SecondarySkills>> update;
        private readonly IReadOnlyList<Homm2SecondarySkill> knownSkills;

        public Homm2SecondarySkillsViewModel(
            Homm2GameVersion gameVersion,
            Func<Homm2SecondarySkills> getSkills,
            Action<Func<Homm2SecondarySkills, Homm2SecondarySkills>> update)
            : this(Homm2SecondarySkill.All.Where(s => s.IsAvailableIn(gameVersion)).ToList(), getSkills, update)
        {
        }

        private Homm2SecondarySkillsViewModel(
            IReadOnlyList<Homm2SecondarySkill> knownSkills,
            Func<Homm2SecondarySkills> getSkills,
            Action<Func<Homm2SecondarySkills, Homm2SecondarySkills>> update)
            : base(knownSkills.Select(s => s.Name).ToList(), Constants.Hero.MaxSecondarySkills)
        {
            this.knownSkills = knownSkills;
            this.getSkills = getSkills;
            this.update = update;

            this.Refresh();
        }

        public override string AllSkillsNote => "A hero can learn eight of the fourteen skills. They are shown in the order they were learned.";

        protected override int UsedSlots => this.getSkills().Count;

        protected override string? SkillInSlot(int slot) => this.getSkills().Entries[slot].Skill.Name;

        protected override string LevelInSlot(int slot) => SkillLevelText.FromNumber((int)this.getSkills().Entries[slot].Level);

        protected override string LevelOf(string skillName) => SkillLevelText.FromNumber((int)this.getSkills().LevelOf(this.Find(skillName)));

        protected override int SlotOf(string skillName)
        {
            var skill = this.Find(skillName);
            var entries = this.getSkills().Entries;
            for (var i = 0; i < entries.Count; i++)
            {
                if (entries[i].Skill == skill)
                {
                    return i;
                }
            }

            return -1;
        }

        protected override void ReplaceSkillInSlot(int slot, string skillName)
        {
            var skill = this.Find(skillName);
            this.update(skills => skills.WithSkillInSlot(slot, skill));
        }

        protected override void AddSkill(string skillName)
        {
            var skill = this.Find(skillName);
            this.update(skills => skills.IsFull ? skills : skills.With(skill, Homm2SkillLevel.Basic));
        }

        protected override void SetLevelInSlot(int slot, string levelName)
        {
            var level = (Homm2SkillLevel)SkillLevelText.ToNumber(levelName);
            this.update(skills => skills.With(skills.Entries[slot].Skill, level));
        }

        protected override void SetLevelOf(string skillName, string levelName)
        {
            var skill = this.Find(skillName);
            var level = (Homm2SkillLevel)SkillLevelText.ToNumber(levelName);

            // A hero that has eight skills cannot learn a ninth; the row goes back to what it was.
            this.update(skills => level != Homm2SkillLevel.None && skills.IsFull && skills.LevelOf(skill) == Homm2SkillLevel.None ? skills : skills.With(skill, level));
        }

        protected override void SwapSlots(int first, int second) => this.update(skills => skills.Swap(first, second));

        protected override void RemoveSlot(int slot) => this.update(skills => skills.WithoutSlot(slot));

        private Homm2SecondarySkill Find(string name) => this.knownSkills.First(s => s.Name == name);
    }
}
