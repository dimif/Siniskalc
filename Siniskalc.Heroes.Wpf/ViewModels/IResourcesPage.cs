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
        /// Gets a row for each resource, in the order the game lists them (gold last), each editable.
        /// </summary>
        IReadOnlyList<AmountRowViewModel> Resources { get; }

        /// <summary>
        /// Gets the command that adds to every resource but gold.
        /// </summary>
        ICommand SetAllResourcesCommand { get; }

        /// <summary>
        /// Gets the command that adds to gold.
        /// </summary>
        ICommand SetRichCommand { get; }

        /// <summary>
        /// Gets the text of the button that adds to every resource but gold, such as "All +100".
        /// </summary>
        string SetAllLabel { get; }

        /// <summary>
        /// Gets the text of the button that adds to gold, such as "Gold +100k".
        /// </summary>
        string SetRichLabel { get; }
    }
}
