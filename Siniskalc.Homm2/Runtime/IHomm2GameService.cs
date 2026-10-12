namespace Siniskalc.Homm2.Runtime
{
    /// <summary>
    /// Loads, reloads and saves <see cref="Homm2Game"/> instances.
    /// </summary>
    public interface IHomm2GameService
    {
        Homm2Game LoadGameFromFile(string filePath);

        Homm2Game LoadGameFromBytes(byte[] bytes, string? sourceName = null);

        Homm2Game ReloadGame(Homm2Game game);

        void SaveGame(Homm2Game game, string filePath);
    }
}
