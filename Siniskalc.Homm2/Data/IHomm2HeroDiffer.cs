namespace Siniskalc.Homm2.Data
{
    using System.Collections.Generic;
    using Siniskalc.Core.Data;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// Lists what is different between a hero as loaded and as edited, as pending changes that can be put back one by one.
    /// </summary>
    public interface IHomm2HeroDiffer
    {
        IReadOnlyList<PendingChange> Diff(int heroIndex, Homm2HeroModel original, Homm2HeroModel current);
    }
}
