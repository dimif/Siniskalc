namespace Siniskalc.Wpf.ViewModels
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// An entry of the right-click menu of a finder row. It either runs <see cref="Invoke"/> or opens a submenu of <see cref="Children"/>.
    /// </summary>
    /// <param name="Header">The text of the entry.</param>
    /// <param name="Invoke">Runs when the entry is clicked; null for an entry that only opens a submenu.</param>
    /// <param name="Children">The entries of the submenu; empty for none.</param>
    public sealed record FinderMenuItem(string Header, Action? Invoke = null, IReadOnlyList<FinderMenuItem>? Children = null);
}
