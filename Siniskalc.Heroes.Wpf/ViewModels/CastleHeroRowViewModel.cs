namespace Siniskalc.Heroes.Wpf.ViewModels
{
    using System.Windows.Input;
    using Siniskalc.Heroes.ViewModels;

    /// <summary>
    /// A hero in a castle, as a line of a page: whether it is garrisoned or visiting, and a link to the page of the hero.
    /// </summary>
    public sealed class CastleHeroRowViewModel : ILink
    {
        /// <param name="role">What the hero does in the castle, as the page words it: "Garrisoned" or "Visiting".</param>
        /// <param name="name">The name of the hero.</param>
        /// <param name="open">Opens the page of the hero.</param>
        public CastleHeroRowViewModel(string role, string name, ICommand open)
        {
            this.Role = role;
            this.Name = name;
            this.OpenCommand = open;
        }

        /// <summary>
        /// Gets how the hero is tied to the castle, such as visiting or garrisoned.
        /// </summary>
        public string Role { get; }

        /// <summary>
        /// Gets the role as it reads after the name, such as "garrisoned".
        /// </summary>
        public string RoleText => this.Role.ToLowerInvariant();

        /// <summary>
        /// Gets the name of the hero.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the role of the hero in the castle, which is what is told beside its name.
        /// </summary>
        public string Detail => this.Role;

        /// <summary>
        /// Gets the command that opens the page of the hero.
        /// </summary>
        public ICommand OpenCommand { get; }
    }
}
