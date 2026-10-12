namespace Siniskalc.Homm2.ViewModels
{
    using System.Collections.Generic;
    using Siniskalc.Homm2.Entities;

    /// <summary>
    /// The colours the interface uses for the players of Heroes of Might and Magic II.
    /// </summary>
    public static class Homm2Palette
    {
        public const string NoPlayer = "#8A93A6";

        private static readonly IReadOnlyDictionary<Homm2Color, string> PlayerColors = new Dictionary<Homm2Color, string>
        {
            [Homm2Color.Blue] = "#2E6FD0",
            [Homm2Color.Green] = "#3C8D40",
            [Homm2Color.Red] = "#C0392B",
            [Homm2Color.Yellow] = "#D4A91C",
            [Homm2Color.Orange] = "#E07B20",
            [Homm2Color.Purple] = "#7B4BA8",
        };

        public static string Of(Homm2Resource resource) => Siniskalc.Wpf.ViewModels.ResourcePalette.Of(resource.ToString());

        public static string Of(Homm2Color? player) => player is { } color && PlayerColors.TryGetValue(color, out var hex) ? hex : NoPlayer;
    }
}
