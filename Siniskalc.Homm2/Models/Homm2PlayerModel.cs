namespace Siniskalc.Homm2.Models
{
    using Siniskalc.Homm2.Entities;

    /// <summary>
    /// A player of Heroes of Might and Magic II as the savegame has it. There is a record for each of the six colours, whether the colour
    /// is in the game or not.
    /// </summary>
    /// <param name="Color">The colour of the player, which is its place in the table of players.</param>
    /// <param name="Resources">The stock of resources.</param>
    /// <param name="Income">What the player earns each day, worked out by the game at the start of the day (mines, castles and artifacts).</param>
    /// <param name="HeroIds">The numbers of the heroes the player has.</param>
    /// <param name="SelectedHero">The hero that is selected on the map, or <c>null</c> (a castle is selected or nothing).</param>
    /// <param name="CastleIds">The numbers of the castles and towns the player has.</param>
    /// <param name="SelectedCastle">The castle that is selected, or <c>null</c>.</param>
    /// <param name="DaysLeft">How many days the player has left to own a castle or a town before being out of the game, or <c>null</c> while he owns one.</param>
    /// <param name="Tavern">The heroes the taverns offer.</param>
    public sealed record Homm2PlayerModel(
        Homm2Color Color,
        Homm2Resources Resources,
        Homm2Resources Income,
        Homm2Numbers HeroIds,
        int? SelectedHero,
        Homm2Numbers CastleIds,
        int? SelectedCastle,
        int? DaysLeft,
        Homm2TavernHeroes Tavern)
    {
        /// <summary>
        /// Gets a value indicating whether the player was put out of the game: no castle, no hero, and the days ran out.
        /// </summary>
        public bool IsOut => this.DaysLeft == 0 && this.CastleIds.Count == 0 && this.HeroIds.Count == 0;
    }
}
