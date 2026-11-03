namespace Siniskalc.Homm3.Data
{
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm3.Entities;

    /// <summary>
    /// Finds the setup of the players (who is human, the difficulty, whose turn it is) in a savefile.
    /// </summary>
    public interface IPlayerControlLocator
    {
        /// <summary>Reads the setup of the game, or returns <c>null</c> when it cannot be found.</summary>
        GameSetup<PlayerColor>? FindSetup(byte[] bytes, GameVersion gameVersion);

        /// <summary>Returns where the setup is, or <c>null</c> when it cannot be found.</summary>
        PlayerSetupLocation? Locate(byte[] bytes, GameVersion gameVersion);
    }
}
