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
        /// Returns the node of <paramref name="region"/>, with a child for each of its elements. <paramref name="shade"/> is 0 or 1;
        /// neighbouring elements alternate.
        /// </summary>
        public static RegionNode From(ByteRegion region, int shade)
        {
            var children = region.Elements.Select((e, i) => From(e, i % 2)).ToList();
            return new RegionNode(region.Name, $"({DescribeRange(region)})", HexPalette.GetBrush(region.Kind, shade), region, children);
        }

        /// <summary>
        /// Returns the offsets of the region in hex, and its length when it is more than one byte.
        /// </summary>
        public static string DescribeRange(ByteRegion region)
        {
            return region.Length == 1
                ? $"0x{region.Start:X}"
                : $"0x{region.Start:X}-0x{region.End - 1:X}, {region.Length} bytes";
        }
    }
}
