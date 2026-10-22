using Siniskalc.Homm3.ViewModels;
using Siniskalc.Wpf.ViewModels;

namespace Siniskalc.Homm3.Tests.ViewModels
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
        /// <summary>
        /// Returns the notes of the node as they are told after its subtitle, without the separators.
        /// </summary>
        public static string NoteText(this NavNodeViewModel node) => string.Join(" · ", node.Notes);

        public static bool HasBadge(this NavNodeViewModel node, string key) => node.Badges.Any(b => b.Key == key);

        public static string BadgeTooltip(this NavNodeViewModel node, string key) => node.Badges.Single(b => b.Key == key).Tooltip;

        public static IList<NavNodeViewModel> PlayerNodes(this Homm3EditorViewModel main) => main.NavigationNodes.Single().Children.Where(n => n.Kind != NavNodeKind.Map).ToList();

        public static IEnumerable<NavNodeViewModel> Descendants(this IEnumerable<NavNodeViewModel> nodes) => nodes.SelectMany(n => n.Descendants());
    }
}
