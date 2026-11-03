namespace Siniskalc.Heroes.Wpf.ViewModels
{
    /// <summary>
    /// A hero (or nobody) to pick for a place of the tavern.
    /// </summary>
    public sealed class HeroChoiceViewModel
    {
        /// <summary>
        /// The <see cref="Key"/> of the choice of nobody; a hero id is a byte, so no hero has it.
        /// </summary>
        public const int NoneKey = -1;

        /// <summary>
        /// Initializes a new instance of the <see cref="HeroChoiceViewModel"/> class.
        /// </summary>
        /// <param name="heroId">The hero id.</param>
        /// <param name="name">The name.</param>
        public HeroChoiceViewModel(int? heroId, string name)
        {
            this.HeroId = heroId;
            this.Name = name;
        }

        /// <summary>
        /// Gets the id of the hero (see the hero number the savefile holds), or null for the choice of nobody.
        /// </summary>
        public int? HeroId { get; }

        /// <summary>
        /// Gets the id of the hero, or <see cref="NoneKey"/> for the choice of nobody, so that a combo box can select by value.
        /// </summary>
        public int Key => this.HeroId ?? NoneKey;

        /// <summary>
        /// Gets the name of the hero as the list of choices shows it.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Returns the name of the hero, which is what a list of choices shows.
        /// </summary>
        /// <returns><see cref="Name"/>.</returns>
        public override string ToString() => this.Name;
    }
}
