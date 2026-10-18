namespace Siniskalc.Homm2.Data
{
    using Siniskalc.Core.Data;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// Lists what is different between a hero as loaded and as edited, as pending changes that can be put back one by one.
    /// </summary>
    public interface IHeroDiffer : IDiffer<HeroModel>
    {
    }
}
