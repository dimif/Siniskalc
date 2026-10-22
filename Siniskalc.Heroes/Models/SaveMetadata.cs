namespace Siniskalc.Heroes.Models
{
    using Siniskalc.Heroes.Entities;

    /// <summary>
    /// What a savefile of any game of the series says about the game besides the records that can be edited. Each part is <c>null</c>
    /// when the savefile does not hold it (or the editor cannot find it in that version).
    /// </summary>
    /// <typeparam name="TColor">The colour of the game.</typeparam>
    /// <param name="Date">The date in the game when the savefile was made.</param>
    /// <param name="Map">The name, description, difficulty and size of the map.</param>
    /// <param name="Setup">Who plays each colour, the difficulty and whose turn it is.</param>
    public sealed record SaveMetadata<TColor>(GameDate? Date, MapHeader? Map, GameSetup<TColor>? Setup)
        where TColor : class, IPlayerColor
    {
        /// <summary>
        /// Gets the metadata of a savefile that holds none of it.
        /// </summary>
        public static SaveMetadata<TColor> None { get; } = new(null, null, null);
    }
}
