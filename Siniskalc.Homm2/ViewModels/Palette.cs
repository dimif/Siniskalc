namespace Siniskalc.Homm2.ViewModels
{
    using System.Collections.Generic;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Homm2.Entities;

    /// <summary>
    /// The colours the interface uses for the players of Heroes of Might and Magic II.
    /// </summary>
    public static class Palette
    {
        public const string NoPlayer = "#8A93A6";

        private static readonly IReadOnlyDictionary<Color, string> PlayerColors = new Dictionary<Color, string>
        {
            [Color.Blue] = "#2E6FD0",
            [Color.Green] = "#3C8D40",
            [Color.Red] = "#C0392B",
            [Color.Yellow] = "#D4A91C",
            [Color.Orange] = "#E07B20",
            [Color.Purple] = "#7B4BA8",
        };

        public static string Of(Resource resource) => Siniskalc.Wpf.ViewModels.ResourcePalette.Of(resource.ToString());

        public static string Of(Color? player) => player is { } color && PlayerColors.TryGetValue(color, out var hex) ? hex : NoPlayer;
    }
}
