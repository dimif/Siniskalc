namespace Siniskalc.Homm3.Data
{
    using System.Collections.Generic;
    using Siniskalc.Core.Data;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// Finds the differences between two versions of the same hero.
    /// </summary>
    public interface IHeroDiffer
    {
        IReadOnlyList<PendingChange> Diff(int heroIndex, HeroModel original, HeroModel current);
    }
}
