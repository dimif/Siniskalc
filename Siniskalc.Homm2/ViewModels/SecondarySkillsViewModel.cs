namespace Siniskalc.Homm2.ViewModels
{
    using System;
    using System.Linq;
    using Siniskalc.Core.Entities;
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// The secondary skills page of a hero of Heroes of Might and Magic II: up to eight skills in the order they were learned, which is
    /// what the hero screen shows, and the list of the fourteen skills with the hero's level in each. The slots and the list are in
    /// <see cref="SecondarySkillsViewModelBase"/>; this reads the hero's skills and writes every edit back through <c>update</c>.
    /// </summary>
    public sealed class SecondarySkillsViewModel : HeroesSecondarySkillsViewModel<SecondarySkill, SecondarySkills>
    {
        public SecondarySkillsViewModel(
            GameVersion gameVersion,
            Func<SecondarySkills> getSkills,
            Action<Func<SecondarySkills, SecondarySkills>> update)
            : base(SecondarySkill.All.AvailableIn(gameVersion).ToList(), Constants.Hero.MaxSecondarySkills, getSkills, update)
        {
        }

        /// <inheritdoc/>
        public override string AllSkillsNote => "A hero can learn eight of the fourteen skills. They are shown in the order they were learned.";
    }
}
