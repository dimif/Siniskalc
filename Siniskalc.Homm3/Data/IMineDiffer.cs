namespace Siniskalc.Homm3.Data
{
    using System.Collections.Generic;
    using Siniskalc.Core.Data;
    using Siniskalc.Homm3.Models;

    public interface IMineDiffer
    {
        IReadOnlyList<PendingChange> Diff(int index, MineModel original, MineModel current);
    }
}
