namespace Siniskalc.Homm2.Data
{
    using System.Collections.Generic;
    using Siniskalc.Core.Data;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// Lists what is different between a player as loaded and as edited, as pending changes that can be put back one by one.
    /// </summary>
    public interface IHomm2PlayerDiffer
    {
        IReadOnlyList<PendingChange> Diff(int playerIndex, Homm2PlayerModel original, Homm2PlayerModel current);
    }
}
