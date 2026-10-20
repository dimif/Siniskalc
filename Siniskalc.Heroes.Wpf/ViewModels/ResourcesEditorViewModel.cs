namespace Siniskalc.Heroes.Wpf.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Input;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// The resources of a player as a page shows them: a row for each resource in the order the game lists them, and the two buttons that
    /// add to every resource but gold or to gold alone, each stopping at the limit of the resource.
    /// </summary>
    public sealed class ResourcesEditorViewModel
    {
        private readonly Action<Func<ResourceSet, ResourceSet>> update;

        /// <param name="order">The resources in the order the page lists them.</param>
        /// <param name="limits">What the player may hold, and what the buttons add.</param>
        /// <param name="current">Gets the resources of the player now.</param>
        /// <param name="original">Gets the resources of the player as the savefile had them.</param>
        /// <param name="update">Changes the resources of the player.</param>
        /// <param name="note">Tells what is shown beside the amount of a resource, such as its income; nothing when omitted.</param>
        public ResourcesEditorViewModel(
            IEnumerable<Resource> order,
            ResourceLimits limits,
            Func<ResourceSet> current,
            Func<ResourceSet> original,
            Action<Func<ResourceSet, ResourceSet>> update,
            Func<Resource, string>? note = null)
        {
            this.update = update;
            this.Rows = order.Select(resource => new ResourceRowViewModel(resource, limits, current, original, update, note)).ToList();
            this.SetAllCommand = new DelegateCommand(_ => this.Add(this.Rows.Where(r => !r.IsGold), limits.AllIncrease));
            this.SetRichCommand = new DelegateCommand(_ => this.Add(this.Rows.Where(r => r.IsGold), limits.GoldIncrease));
        }

        /// <summary>
        /// Gets a row for each resource.
        /// </summary>
        public IReadOnlyList<ResourceRowViewModel> Rows { get; }

        /// <summary>
        /// Gets the command that adds to every resource but gold.
        /// </summary>
        public ICommand SetAllCommand { get; }

        /// <summary>
        /// Gets the command that adds to gold.
        /// </summary>
        public ICommand SetRichCommand { get; }

        /// <summary>
        /// Shows the amounts again, after the resources changed.
        /// </summary>
        public void Refresh()
        {
            foreach (var row in this.Rows)
            {
                row.Refresh();
            }
        }

        private void Add(IEnumerable<ResourceRowViewModel> rows, int amount)
        {
            var kinds = rows.Select(r => (r.Resource, r.Maximum)).ToList();
            this.update(resources => kinds.Aggregate(resources, (set, kind) => set.With(kind.Resource, Math.Min(set[kind.Resource] + amount, kind.Maximum))));
        }
    }
}
