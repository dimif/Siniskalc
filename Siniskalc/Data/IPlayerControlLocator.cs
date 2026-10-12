namespace Siniskalc.Data
{
    using Siniskalc.Entities;
    using Siniskalc.Models;

    /// <summary>
    /// Finds the setup of the players (who is human, the difficulty, whose turn it is) in a savefile.
    /// </summary>
    public interface IPlayerControlLocator
    {
        /// <summary>Reads the setup of the game, or returns <c>null</c> when it cannot be found.</summary>
        GameSetup? FindSetup(byte[] bytes, GameVersion gameVersion);

        /// <summary>Returns where the setup is, or <c>null</c> when it cannot be found.</summary>
        PlayerSetupLocation? Locate(byte[] bytes, GameVersion gameVersion);
    }
}
