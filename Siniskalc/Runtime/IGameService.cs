namespace Siniskalc.Runtime
{
    /// <summary>
    /// Loads, reloads and saves <see cref="Game"/> instances.
    /// </summary>
    public interface IGameService
    {
        Game LoadGameFromFile(string filePath);

        Game LoadGameFromBytes(byte[] bytes, string? sourceName = null);

        Game ReloadGame(Game game);

        void SaveGame(Game game, string filePath);
    }
}
