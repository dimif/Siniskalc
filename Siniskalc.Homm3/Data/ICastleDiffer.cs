namespace Siniskalc.Homm3.Data
{
    using Siniskalc.Core.Data;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// Finds the differences between two versions of the same castle.
    /// </summary>
    public interface ICastleDiffer : IDiffer<CastleModel>
    {
        CastleModel RestoreBuilding(CastleModel castle, Building building, CastleModel original);
    }
}
