namespace Siniskalc.Homm3.Data
{
    using Siniskalc.Core.Data;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// Finds the differences between two versions of the same hero.
    /// </summary>
    public interface IHeroDiffer : IDiffer<HeroModel>
    {
    }
}
