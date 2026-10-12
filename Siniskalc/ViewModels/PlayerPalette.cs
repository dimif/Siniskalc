namespace Siniskalc.ViewModels
{
    using System.Collections.Generic;
    using Siniskalc.Entities;

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

        private static readonly IReadOnlyDictionary<Resource, string> ResourceColors = new Dictionary<Resource, string>
        {
            [Resource.Gold] = "#B8902A",
            [Resource.Wood] = "#6B4A2B",
            [Resource.Mercury] = "#6D7A8C",
            [Resource.Ore] = "#4E5560",
            [Resource.Sulfur] = "#A59A22",
            [Resource.Crystal] = "#A0356A",
            [Resource.Gems] = "#2B8A78",
        };

        public static string Of(PlayerColor? player) => player == null ? NoPlayer : PlayerColors[player.Value];

        public static string Of(Resource resource) => ResourceColors[resource];
    }
}
