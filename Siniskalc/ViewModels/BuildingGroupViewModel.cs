namespace Siniskalc.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// A group of buildings of a castle, such as its dwellings.
    /// </summary>
    public sealed class BuildingGroupViewModel
    {
        public BuildingGroupViewModel(string title, IReadOnlyList<BuildingRowViewModel> rows)
        {
            this.Title = title;
            this.Rows = rows;
            this.Summary = $"{rows.Count(r => r.IsBuilt)} of {rows.Count} built";
        }

        public string Title { get; }

        public string Summary { get; }

        public IReadOnlyList<BuildingRowViewModel> Rows { get; }
    }
}
