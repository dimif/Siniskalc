namespace Siniskalc.Heroes.Wpf.ViewModels
{
    using System.Collections.Generic;
    using System.Windows.Input;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// What the resources of a player (<c>ResourcesPanel</c>) need of the page: a row for each resource, and the quick add commands.
    /// </summary>
    public interface IResourcesPage
    {
        /// <summary>
        /// Gets a row for each resource, gold first, each editable.
        /// </summary>
        IReadOnlyList<AmountRowViewModel> Resources { get; }

        /// <summary>
        /// Gets the command that adds 99 of every resource but gold.
        /// </summary>
        ICommand SetAllResourcesCommand { get; }

        /// <summary>
        /// Gets the command that adds 100,000 gold.
        /// </summary>
        ICommand SetRichCommand { get; }
    }
}
