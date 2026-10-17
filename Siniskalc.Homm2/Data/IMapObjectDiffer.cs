namespace Siniskalc.Homm2.Data
{
    using System.Collections.Generic;
    using Siniskalc.Core.Data;

    /// <summary>
    /// Lists what is different between an object of the map as loaded and as edited, as pending changes that can be put back one by one.
    /// </summary>
    public interface IMapObjectDiffer<TModel>
    {
        IReadOnlyList<PendingChange> Diff(int index, TModel original, TModel current);
    }
}
