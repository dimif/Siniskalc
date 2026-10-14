namespace Siniskalc.Homm2.Data
{
    using System.Collections.Generic;
    using Siniskalc.Core.Data;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// Lists what is different between a castle as loaded and as edited, as pending changes that can be put back one by one.
    /// </summary>
    public interface ICastleDiffer
    {
        IReadOnlyList<PendingChange> Diff(int castleIndex, CastleModel original, CastleModel current);
    }
}
