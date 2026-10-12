namespace Siniskalc.Runtime
{
    using Siniskalc.Models;

    /// <summary>
    /// What a savefile says about the game besides the structs that can be edited. Each part is <c>null</c> when it cannot be
    /// found in the version of the savefile.
    /// </summary>
    /// <param name="Date">The date in the game when the savefile was made.</param>
    /// <param name="Setup">Who plays each colour, the difficulty and whose turn it is.</param>
    /// <param name="Map">The map's name, description and difficulty.</param>
    public sealed record SaveMetadata(GameDate? Date, GameSetup? Setup, MapHeader? Map)
    {
        public static SaveMetadata None { get; } = new(null, null, null);
    }
}
