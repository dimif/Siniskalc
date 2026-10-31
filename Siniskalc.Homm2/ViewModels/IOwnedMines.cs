namespace Siniskalc.Homm2.ViewModels
{
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// A page that lists the mines one owner has on the map, and can show the bytes of the mine that is picked.
    /// </summary>
    public interface IOwnedMines
    {
        /// <summary>
        /// Gets the mines of the owner.
        /// </summary>
        MinesListViewModel<MineRowViewModel<MineModel, PlayerColor>> Mines { get; }

        /// <summary>
        /// Creates the raw bytes of the mine that is picked for the hex editor, or <c>null</c> when there is none or the page has no way to show
        /// bytes. The caller owns the target and disposes it.
        /// </summary>
        IByteEditTarget? CreateMineByteEditTarget();
    }
}
