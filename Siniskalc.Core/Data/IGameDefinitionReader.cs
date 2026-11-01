namespace Siniskalc.Core.Data
{
    /// <summary>
    /// Reads a typed definition from the application's standard game data.
    /// </summary>
    public interface IGameDefinitionReader
    {
        T Read<T>(string relativePath)
            where T : class;
    }
}
