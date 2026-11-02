namespace Siniskalc.Homm2.Data
{
    using System;
    using System.Collections.Generic;
    using Siniskalc.Core.Data;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// Lists what is different between a player as loaded and as edited, as pending changes that can be put back one by one.
    /// </summary>
    public interface IPlayerDiffer
    {
        /// <summary>
        /// Lists what is different between the two versions of the player at <paramref name="playerIndex"/>.
        /// </summary>
        /// <param name="playerIndex">The place of the player among the players.</param>
        /// <param name="original">The player as loaded.</param>
        /// <param name="current">The player now.</param>
        /// <param name="heroName">Gives the name of the hero with an id, to word the change of a tavern hero.</param>
        IReadOnlyList<PendingChange> Diff(int playerIndex, PlayerModel original, PlayerModel current, Func<int, string> heroName);
    }
}
