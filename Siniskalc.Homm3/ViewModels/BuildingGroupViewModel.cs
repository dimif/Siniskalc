namespace Siniskalc.Homm3.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// A group of buildings of a castle, such as its dwellings.
    /// </summary>
    public sealed class BuildingGroupViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BuildingGroupViewModel"/> class.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="rows">The rows.</param>
        public BuildingGroupViewModel(string title, IReadOnlyList<BuildingRowViewModel> rows)
        {
            this.Title = title;
            this.Rows = rows;
            this.Summary = $"{rows.Count(r => r.IsBuilt)} of {rows.Count} built";
        }

        /// <summary>
        /// Gets the title of the group.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Gets the line that says how much of the group is built.
        /// </summary>
        public string Summary { get; }

        /// <summary>
        /// Gets the buildings of the group.
        /// </summary>
        public IReadOnlyList<BuildingRowViewModel> Rows { get; }
    }
}
