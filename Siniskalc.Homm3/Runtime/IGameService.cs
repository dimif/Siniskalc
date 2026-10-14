namespace Siniskalc.Homm3.Runtime
{
    /// <summary>
    /// Loads, reloads and saves <see cref="Game"/> instances.
    /// </summary>
    public interface IGameService : Siniskalc.Core.Runtime.ISavegameService<Game>
    {
        Game LoadGameFromFile(string filePath);

        Game LoadGameFromBytes(byte[] bytes, string? sourceName = null);

        Game ReloadGame(Game game);

        void SaveGame(Game game, string filePath);
    }
}
