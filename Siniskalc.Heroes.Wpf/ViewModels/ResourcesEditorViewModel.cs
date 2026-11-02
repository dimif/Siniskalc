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
    /// The resources of a player as a page shows them: a row for each resource, the buttons that add to or clear the resources, and a button
    /// on each row, each stopping at the limit of the resource.
    /// </summary>
    public sealed class ResourcesEditorViewModel
    {
        private readonly Action<Func<ResourceSet, ResourceSet>> update;

        /// <param name="limits">What the player may hold, and what the buttons add.</param>
        /// <param name="current">Gets the resources of the player now.</param>
        /// <param name="original">Gets the resources of the player as the savefile had them.</param>
        /// <param name="update">Changes the resources of the player.</param>
        /// <param name="note">Tells what is shown beside the amount of a resource, such as its income; nothing when omitted.</param>
        public ResourcesEditorViewModel(
            ResourceLimits limits,
            Func<ResourceSet> current,
            Func<ResourceSet> original,
            Action<Func<ResourceSet, ResourceSet>> update,
            Func<Resource, string>? note = null)
            : this(Enum.GetValues<Resource>(), limits, current, original, update, note)
        {
        }

        private ResourcesEditorViewModel(
            IEnumerable<Resource> order,
            ResourceLimits limits,
            Func<ResourceSet> current,
            Func<ResourceSet> original,
            Action<Func<ResourceSet, ResourceSet>> update,
            Func<Resource, string>? note)
        {
            this.update = update;
            this.Rows = order.Select(resource => new ResourceRowViewModel(resource, limits, current, original, update, note)).ToList();
            this.SetAllLabel = $"Res {AmountText.Plus(limits.AllIncrease)}";
            this.SetRichLabel = $"Gold {AmountText.Plus(limits.GoldIncrease)}";
            this.SetAllCommand = new DelegateCommand(_ => this.Add(this.Rows.Where(r => !r.IsGold), limits.AllIncrease));
            this.SetRichCommand = new DelegateCommand(_ => this.Add(this.Rows.Where(r => r.IsGold), limits.GoldIncrease));
            this.ClearAllCommand = new DelegateCommand(_ => this.Clear(this.Rows.Where(r => !r.IsGold)));
            this.ClearRichCommand = new DelegateCommand(_ => this.Clear(this.Rows.Where(r => r.IsGold)));
        }

        /// <summary>
        /// Gets a row for each resource.
        /// </summary>
        public IReadOnlyList<ResourceRowViewModel> Rows { get; }

        /// <summary>
        /// Gets the text of the button that adds to every resource but gold, such as "Res +100".
        /// </summary>
        public string SetAllLabel { get; }

        /// <summary>
        /// Gets the text of the button that adds to gold, such as "Gold +100k".
        /// </summary>
        public string SetRichLabel { get; }

        /// <summary>
        /// Gets the command that adds to every resource but gold.
        /// </summary>
        public ICommand SetAllCommand { get; }

        /// <summary>
        /// Gets the command that adds to gold.
        /// </summary>
        public ICommand SetRichCommand { get; }

        /// <summary>
        /// Gets the command that sets every resource but gold to 0.
        /// </summary>
        public ICommand ClearAllCommand { get; }

        /// <summary>
        /// Gets the command that sets gold to 0.
        /// </summary>
        public ICommand ClearRichCommand { get; }

        /// <summary>
        /// Builds the resources of a player in the order both games show them: gold first, then the savefile-table order.
        /// </summary>
        /// <param name="limits">What the player may hold, and what the buttons add.</param>
        /// <param name="current">Gets the resources of the player now.</param>
        /// <param name="original">Gets the resources of the player as the savefile had them.</param>
        /// <param name="update">Changes the resources of the player.</param>
        /// <param name="note">Tells what is shown beside the amount of a resource, such as its income; nothing when omitted.</param>
        public static ResourcesEditorViewModel ForPlayer(
            ResourceLimits limits,
            Func<ResourceSet> current,
            Func<ResourceSet> original,
            Action<Func<ResourceSet, ResourceSet>> update,
            Func<Resource, string>? note = null)
        {
            return new ResourcesEditorViewModel(ResourceSet.All.OrderBy(resource => resource != Resource.Gold), limits, current, original, update, note);
        }

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

        private void Clear(IEnumerable<ResourceRowViewModel> rows)
        {
            var kinds = rows.Select(r => r.Resource).ToList();
            this.update(resources => kinds.Aggregate(resources, (set, kind) => set.With(kind, 0)));
        }

        private void Add(IEnumerable<ResourceRowViewModel> rows, int amount)
        {
            var kinds = rows.Select(r => (r.Resource, r.Maximum)).ToList();
            this.update(resources => kinds.Aggregate(resources, (set, kind) => set.With(kind.Resource, Math.Min(set[kind.Resource] + amount, kind.Maximum))));
        }
    }
}
