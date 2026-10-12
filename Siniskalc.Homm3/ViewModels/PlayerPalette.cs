namespace Siniskalc.Homm3.ViewModels
{
    using System.Collections.Generic;
    using Siniskalc.Homm3.Entities;

    /// <summary>
    /// The colours the interface uses for the players and the resources.
    /// </summary>
    public static class PlayerPalette
    {
        public const string NoPlayer = "#8A93A6";

        private static readonly IReadOnlyDictionary<PlayerColor, string> PlayerColors = new Dictionary<PlayerColor, string>
        {
            [PlayerColor.Red] = "#C0392B",
            [PlayerColor.Blue] = "#2E6FD0",
            [PlayerColor.Tan] = "#B08D57",
            [PlayerColor.Green] = "#3C8D40",
            [PlayerColor.Orange] = "#E07B20",
            [PlayerColor.Purple] = "#7B4BA8",
            [PlayerColor.Teal] = "#1E9C90",
            [PlayerColor.Pink] = "#D96AA7",
        };

        public static string Of(PlayerColor? player) => player == null ? NoPlayer : PlayerColors[player.Value];

        public static string Of(Resource resource) => Siniskalc.Wpf.ViewModels.ResourcePalette.Of(resource.ToString());
    }
}
