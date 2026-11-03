namespace Siniskalc.Core.Data
{
    /// <summary>
    /// Reads a typed definition from the application's standard game data.
    /// </summary>
    public interface IGameDefinitionReader
    {
        /// <summary>
        /// Reads the definition in the file at <paramref name="relativePath"/>.
        /// </summary>
        /// <typeparam name="T">The type the definition is read as.</typeparam>
        /// <param name="relativePath">The path of the file, relative to the game data folder.</param>
        /// <returns>The definition.</returns>
        /// <exception cref="Siniskalc.Core.Runtime.GameDefinitionException">The path is not a relative one inside the game data folder, or the file cannot be read or understood.</exception>
        T Read<T>(string relativePath)
            where T : class;
    }
}
