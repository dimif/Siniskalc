namespace Siniskalc.Heroes.Wpf.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;

    /// <summary>
    /// The secondary skills editor of every game: the slots of a hero and the level in each skill the game has, over the
    /// <see cref="ISecondarySkills{TSecondarySkill, TSelf}"/> of the game. A game adds what is its own, such as skills a hero has a level
    /// in that its screen does not show.
    /// </summary>
    /// <typeparam name="TSkill">The secondary skill of the game.</typeparam>
    /// <typeparam name="TSkills">The secondary skills of a hero in the game.</typeparam>
    public class HeroesSecondarySkillsViewModel<TSkill, TSkills> : SecondarySkillsViewModelBase
        where TSkill : class, ISecondarySkill
        where TSkills : class, ISecondarySkills<TSkill, TSkills>
    {
        private readonly IReadOnlyList<TSkill> knownSkills;
        private readonly Func<TSkills> getSkills;
        private readonly Action<Func<TSkills, TSkills>> update;

        /// <summary>
        /// Initializes a new instance of the <see cref="HeroesSecondarySkillsViewModel{TSkill, TSkills}"/> class.
        /// </summary>
        /// <param name="knownSkills">The skills the savefile can hold.</param>
        /// <param name="maxSlots">The most skills the hero screen shows.</param>
        /// <param name="getSkills">Gets the skills as they are now.</param>
        /// <param name="update">Replaces the skills with what an edit makes of them.</param>
        public HeroesSecondarySkillsViewModel(IReadOnlyList<TSkill> knownSkills, int maxSlots, Func<TSkills> getSkills, Action<Func<TSkills, TSkills>> update)
            : base(knownSkills.Select(s => s.Name).ToList(), maxSlots)
        {
            this.knownSkills = knownSkills;
            this.getSkills = getSkills;
            this.update = update;
            this.Refresh();
        }

        /// <summary>
        /// Gets the skills as they are now.
        /// </summary>
        protected TSkills Skills => this.getSkills();

        /// <inheritdoc/>
        protected override int UsedSlots => this.Skills.SlotCount;

        /// <summary>
        /// Replaces the skills with what <paramref name="edit"/> makes of them.
        /// </summary>
        /// <param name="edit">Returns the edited skills.</param>
        protected void Update(Func<TSkills, TSkills> edit) => this.update(edit);

        /// <summary>
        /// Finds the skill with a name.
        /// </summary>
        /// <param name="name">The name of the skill.</param>
        protected TSkill Find(string name) => this.knownSkills.First(s => s.Name == name);

        /// <inheritdoc/>
        protected override string? SkillInSlot(int slot) => slot < this.Skills.SlotCount ? this.Skills.GetSkillInSlot(slot).Name : null;

        /// <inheritdoc/>
        protected override string LevelInSlot(int slot) =>
            slot < this.Skills.SlotCount ? SkillLevelNames.FromLevel(this.Skills.GetSkillLevel(this.Skills.GetSkillInSlot(slot))) : SkillLevelNames.Basic;

        /// <inheritdoc/>
        protected override string LevelOf(string skillName) => SkillLevelNames.FromLevel(this.Skills.GetSkillLevel(this.Find(skillName)));

        /// <inheritdoc/>
        protected override int SlotOf(string skillName) => this.Skills.GetSlotOf(this.Find(skillName));

        /// <inheritdoc/>
        protected override void ReplaceSkillInSlot(int slot, string skillName)
        {
            var skill = this.Find(skillName);
            this.update(skills => skills.WithSkillInSlot(slot, skill));
        }

        /// <inheritdoc/>
        protected override void AddSkill(string skillName)
        {
            var skill = this.Find(skillName);
            this.update(skills => skills.WithSkillLevel(skill, SkillLevel.Beginner));
        }

        /// <inheritdoc/>
        protected override void SetLevelInSlot(int slot, string levelName)
        {
            var level = SkillLevelNames.ToLevel(levelName);
            if (level != SkillLevel.None)
            {
                this.update(skills => slot < skills.SlotCount ? skills.WithSkillLevel(skills.GetSkillInSlot(slot), level) : skills);
            }
        }

        /// <inheritdoc/>
        protected override void SetLevelOf(string skillName, string levelName)
        {
            var skill = this.Find(skillName);
            var level = SkillLevelNames.ToLevel(levelName);
            this.update(skills => skills.WithSkillLevel(skill, level));
        }

        /// <inheritdoc/>
        protected override void SwapSlots(int first, int second) => this.update(skills => skills.Swap(first, second));

        /// <inheritdoc/>
        protected override void RemoveSlot(int slot) => this.update(skills => skills.WithoutSlot(slot));
    }
}
