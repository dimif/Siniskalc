namespace Siniskalc.Homm3.Data
{
    using System.Collections.Generic;
    using Siniskalc.Core.Data;
    using Siniskalc.Homm3.Models;

    public interface IMerchantStockDiffer
    {
        IReadOnlyList<PendingChange> Diff(int index, MerchantStockModel original, MerchantStockModel current);
    }
}
