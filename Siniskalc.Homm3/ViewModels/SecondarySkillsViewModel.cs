namespace Siniskalc.Homm3.ViewModels
{
    using System;
    using System.Linq;
    using Siniskalc.Core.Entities;
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// ViewModel for the secondary skills page.
    /// The hero screen shows up to <see cref="Constants.SkillSlots.Max"/> skills, in slots. The game also applies
    /// skills a hero has a level in but that have no slot, so this page lists every skill and not only the slots.
    /// The slots and the list are in <see cref="SecondarySkillsViewModelBase"/>; this reads from the hero's skills and writes every edit
    /// back through <c>update</c>. Call <see cref="SecondarySkillsViewModelBase.Refresh"/> after they changed.
    /// </summary>
    public sealed class SecondarySkillsViewModel : HeroesSecondarySkillsViewModel<SecondarySkill, SecondarySkills>
    {
        public SecondarySkillsViewModel(
            GameVersion gameVersion,
            Func<SecondarySkills> getSkills,
            Action<Func<SecondarySkills, SecondarySkills>> update)
            : base(SecondarySkill.All.AvailableIn(gameVersion).ToList(), Constants.SkillSlots.Max, getSkills, update)
        {
        }

        /// <inheritdoc/>
        public override bool SupportsHiding => true;

        /// <inheritdoc/>
        public override int ExtraCount => this.Skills.ExtraSkills.Count();

        /// <inheritdoc/>
        public override string AllSkillsNote => "The game only allows a hero to learn eight skills, but it applies every skill that has a level.";

        /// <inheritdoc/>
        protected override void HideSlot(int slot) => this.Update(skills => skills.WithSlotAsExtra(slot));

        /// <inheritdoc/>
        protected override void ShowExtraSkill(string skillName)
        {
            var skill = this.Find(skillName);
            this.Update(skills => skills.WithExtraSkillInSlot(skill));
        }
    }
}
