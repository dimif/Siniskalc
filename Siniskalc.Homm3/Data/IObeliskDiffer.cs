namespace Siniskalc.Homm3.Data
{
    using System.Collections.Generic;
    using Siniskalc.Core.Data;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// Finds the differences between two versions of the visited obelisks.
    /// </summary>
    public interface IObeliskDiffer
    {
        IReadOnlyList<PendingChange> Diff(int index, ObeliskVisits original, ObeliskVisits current);
    }
}
