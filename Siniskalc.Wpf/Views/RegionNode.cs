namespace Siniskalc.Wpf.Views
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Media;
    using Siniskalc.Core.Data;

    /// <summary>
    /// One entry in the list of known fields beside a hex grid.
    /// </summary>
    internal sealed record RegionNode(string Label, string Range, Brush Swatch, ByteRegion? Region, IReadOnlyList<RegionNode> Children)
    {
        /// <summary>
        /// Returns the node of <paramref name="region"/>, with a child for each of its elements.
        /// </summary>
        /// <returns>The node.</returns>
        /// <param name="region">The region.</param>
        /// <param name="shade">0 or 1, to tell the region from its neighbour of the same kind; its elements alternate between the two.</param>
        public static RegionNode From(ByteRegion region, int shade)
        {
            var children = region.Elements.Select((e, i) => From(e, i % 2)).ToList();
            return new RegionNode(region.Name, $"({DescribeRange(region)})", HexPalette.GetBrush(region.Kind, shade), region, children);
        }

        /// <summary>
        /// Returns the offsets of the region in hex, and its length when it is more than one byte.
        /// </summary>
        /// <param name="region">The region.</param>
        /// <returns>The text of the range.</returns>
        public static string DescribeRange(ByteRegion region)
        {
            return region.Length == 1
                ? $"0x{region.Start:X}"
                : $"0x{region.Start:X}-0x{region.End - 1:X}, {region.Length} bytes";
        }
    }
}
