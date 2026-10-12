namespace Siniskalc.ViewModels
{
    /// <summary>
    /// A hero (or nobody) to pick for a place of the tavern.
    /// </summary>
    public sealed class HeroChoiceViewModel
    {
        public HeroChoiceViewModel(int? heroId, string name)
        {
            this.HeroId = heroId;
            this.Name = name;
        }

        /// <summary>
        /// Gets the id of the hero (see <see cref="Siniskalc.Models.HeroModel.HeroId"/>), or null for the choice of nobody.
        /// </summary>
        public int? HeroId { get; }

        /// <summary>
        /// The <see cref="Key"/> of the choice of nobody; a hero id is a byte, so no hero has it.
        /// </summary>
        public const int NoneKey = -1;

        /// <summary>
        /// Gets the id of the hero, or <see cref="NoneKey"/> for the choice of nobody, so that a combo box can select by value.
        /// </summary>
        public int Key => this.HeroId ?? NoneKey;

        public string Name { get; }

        public override string ToString() => this.Name;
    }
}
