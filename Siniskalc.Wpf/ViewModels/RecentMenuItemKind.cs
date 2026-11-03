namespace Siniskalc.Wpf.ViewModels
{
    /// <summary>
    /// What a <see cref="RecentMenuItemViewModel"/> is.
    /// </summary>
    public enum RecentMenuItemKind
    {
        /// <summary>A recent file; picking it opens the file.</summary>
        File,

        /// <summary>The note that there are no recent files; it cannot be picked.</summary>
        Empty,

        /// <summary>A line between groups of items.</summary>
        Separator,

        /// <summary>The command that empties the list.</summary>
        Clear,
    }
}
