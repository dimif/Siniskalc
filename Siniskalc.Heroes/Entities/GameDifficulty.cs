namespace Siniskalc.Heroes.Entities
{
    /// <summary>
    /// The difficulty a game was started with, by the names Horn of the Abyss gives the levels. The value is the percentage the game shows.
    /// </summary>
    public enum GameDifficulty : byte
    {
        Pawn = 80,
        Knight = 100,
        Rook = 130,
        Queen = 160,
        King = 200,
    }
}
