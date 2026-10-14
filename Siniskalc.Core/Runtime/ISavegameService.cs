namespace Siniskalc.Core.Runtime
{
    /// <summary>
    /// Loads, reloads and saves the open games of one game.
    /// </summary>
    /// <typeparam name="TGame">What the game calls an open savefile.</typeparam>
    public interface ISavegameService<TGame>
        where TGame : IOpenGame
    {
        TGame LoadGameFromFile(string filePath);

        TGame LoadGameFromBytes(byte[] bytes, string? sourceName = null);

        TGame ReloadGame(TGame game);

        void SaveGame(TGame game, string filePath);
    }
}
