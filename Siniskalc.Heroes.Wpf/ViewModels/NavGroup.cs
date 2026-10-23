namespace Siniskalc.Heroes.Wpf.ViewModels
{
    using System.Collections.Generic;

    /// <summary>
    /// A group of the mines or dwellings of an owner as a game gives it to <see cref="OwnerNavGroup"/>: the mines of one resource, the dwellings of one faction.
    /// </summary>
    /// <param name="Title">What the group is called.</param>
    /// <param name="Entries">The mines or dwellings in the group.</param>
    public sealed record NavGroup(string Title, IReadOnlyList<NavEntry> Entries);
}
