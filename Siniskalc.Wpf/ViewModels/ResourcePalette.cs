namespace Siniskalc.Wpf.ViewModels
{
    using System.Collections.Generic;

    /// <summary>
    /// The colours the interface uses for the resources, which are the same in every game: gold, wood, mercury, ore, sulfur, crystal and gems.
    /// </summary>
    public static class ResourcePalette
    {
        public const string Unknown = "#8A93A6";

        private static readonly IReadOnlyDictionary<string, string> Colors = new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase)
        {
            ["Gold"] = "#B8902A",
            ["Wood"] = "#6B4A2B",
            ["Mercury"] = "#6D7A8C",
            ["Ore"] = "#4E5560",
            ["Sulfur"] = "#A59A22",
            ["Crystal"] = "#A0356A",
            ["Gems"] = "#2B8A78",
        };

        /// <summary>
        /// Returns the colour of the resource called <paramref name="name"/>, e.g. "#B8902A" for "Gold".
        /// </summary>
        public static string Of(string name) => Colors.TryGetValue(name, out var hex) ? hex : Unknown;
    }
}
