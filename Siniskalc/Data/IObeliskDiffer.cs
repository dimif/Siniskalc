namespace Siniskalc.Data
{
    using System.Collections.Generic;
    using Siniskalc.Core.Data;
    using Siniskalc.Models;

    /// <summary>
    /// Finds the differences between two versions of the visited obelisks.
    /// </summary>
    public interface IObeliskDiffer
    {
        IReadOnlyList<PendingChange> Diff(int index, ObeliskVisits original, ObeliskVisits current);
    }
}
