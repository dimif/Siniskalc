using Siniskalc.ViewModels;

namespace Siniskalc.Tests.ViewModels
{
    internal static class NavTreeExtensions
    {
        /// <summary>
        /// Returns every node below <paramref name="node"/>, at any depth.
        /// </summary>
        public static IEnumerable<NavNodeViewModel> Descendants(this NavNodeViewModel node)
        {
            foreach (var child in node.Children)
            {
                yield return child;
                foreach (var below in child.Descendants())
                {
                    yield return below;
                }
            }
        }

        /// <summary>
        /// Returns the players (and neutral) nodes: the children of the game node, which is the root of the tree.
        /// </summary>
        public static IList<NavNodeViewModel> PlayerNodes(this MainViewModel main) => main.NavigationNodes.Single().Children;

        public static IEnumerable<NavNodeViewModel> Descendants(this IEnumerable<NavNodeViewModel> nodes) => nodes.SelectMany(n => n.Descendants());
    }
}
