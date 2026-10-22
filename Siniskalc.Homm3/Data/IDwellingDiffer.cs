namespace Siniskalc.Homm3.Data
{
    using System.Collections.Generic;
    using Siniskalc.Core.Data;
    using Siniskalc.Homm3.Models;

    public interface IDwellingDiffer
    {
        IReadOnlyList<PendingChange> Diff(int index, DwellingModel original, DwellingModel current);
    }
}
