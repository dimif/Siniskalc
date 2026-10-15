namespace Siniskalc.Heroes.Wpf.ViewModels
{
    /// <summary>
    /// A hero or a castle as a game gives it to <see cref="OwnerNavGroup"/>.
    /// </summary>
    /// <param name="Index">The number of the hero or castle in the savefile.</param>
    /// <param name="Title">What the node is called.</param>
    /// <param name="Subtitle">The smaller text beside the title.</param>
    public sealed record NavEntry(int Index, string Title, string Subtitle);
}
