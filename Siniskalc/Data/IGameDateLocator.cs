namespace Siniskalc.Data
{
    using Siniskalc.Entities;
    using Siniskalc.Models;

    /// <summary>
    /// Finds the date of the game in a savefile.
    /// </summary>
    public interface IGameDateLocator
    {
        /// <summary>Returns the date in the savefile, or <c>null</c> when it cannot be found.</summary>
        GameDate? Find(byte[] bytes, GameVersion gameVersion, int searchFrom = 0);

        /// <summary>Returns where the date is (the 16-bit day, week and month, in that order), or -1 when it cannot be found.</summary>
        int FindOffset(byte[] bytes, GameVersion gameVersion, int searchFrom = 0);
    }
}
