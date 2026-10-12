namespace Siniskalc.Data
{
    using System;
    using System.Collections.Generic;
    using Siniskalc.Core.Data;
    using Siniskalc.Models;

    /// <summary>
    /// Finds the differences between two versions of the same player.
    /// </summary>
    public interface IPlayerDiffer
    {
        /// <param name="heroName">Gives the name of the hero with an id, to word the change of a tavern hero.</param>
        IReadOnlyList<PendingChange> Diff(int playerIndex, PlayerModel original, PlayerModel current, Func<int, string> heroName);
    }
}
