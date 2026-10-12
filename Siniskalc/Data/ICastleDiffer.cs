namespace Siniskalc.Data
{
    using System.Collections.Generic;
    using Siniskalc.Core.Data;
    using Siniskalc.Entities;
    using Siniskalc.Models;

    /// <summary>
    /// Finds the differences between two versions of the same castle.
    /// </summary>
    public interface ICastleDiffer
    {
        IReadOnlyList<PendingChange> Diff(int castleIndex, CastleModel original, CastleModel current);

        CastleModel RestoreBuilding(CastleModel castle, Building building, CastleModel original);
    }
}
