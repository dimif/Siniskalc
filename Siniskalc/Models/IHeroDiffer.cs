namespace Siniskalc.Models
{
    using System.Collections.Generic;

    /// <summary>
    /// Finds the differences between two versions of the same hero.
    /// </summary>
    public interface IHeroDiffer
    {
        IReadOnlyList<HeroChange> Diff(HeroModel original, HeroModel current);
    }
}
