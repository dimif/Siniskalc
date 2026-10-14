namespace Siniskalc.Wpf.ViewModels
{
    using System.Collections.Generic;
    using Siniskalc.Core.Data;

    /// <summary>
    /// What a game says about its navigation tree: which nodes there are, how they look, and when the tree has to be built again.
    /// </summary>
    /// <typeparam name="TGame">What the game calls an open savefile.</typeparam>
    public interface INavigationBuilder<in TGame>
    {
        /// <summary>
        /// Gets the kind of chunk the nodes of each kind stand for, to mark the nodes with unsaved edits.
        /// </summary>
        IReadOnlyDictionary<NavNodeKind, ChunkKind> ChunkKinds { get; }

        /// <summary>
        /// Gets a text that changes when the tree has to be built again, such as who owns each castle and hero: the tree groups them by owner.
        /// </summary>
        string StructureKey(TGame game);

        /// <summary>
        /// Fills <paramref name="tree"/> (which is empty) with the nodes of <paramref name="game"/>.
        /// </summary>
        void Build(NavigationTree tree, TGame game);

        /// <summary>
        /// Updates the badges and notes of the nodes to <paramref name="game"/> as it is now, after an edit or an undo.
        /// </summary>
        void RefreshMarks(NavigationTree tree, TGame game);
    }
}
