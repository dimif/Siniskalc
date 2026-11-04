namespace Siniskalc.Homm2.ViewModels
{
    using System.Linq;

    /// <summary>
    /// Supplies the wording of the map: how many shrines and sphinxes it has.
    /// </summary>
    public sealed class MapTextProvider
    {
        /// <summary>
        /// Says how many shrines and sphinxes a map has; a kind the map does not have is left out.
        /// </summary>
        /// <param name="shrines">The number of shrines.</param>
        /// <param name="sphinxes">The number of sphinxes.</param>
        /// <returns>The summary, such as "13 shrines, 1 sphinx".</returns>
        public string Summarize(int shrines, int sphinxes) => string.Join(
            ", ",
            new[]
            {
                shrines > 0 ? $"{shrines} {(shrines == 1 ? "shrine" : "shrines")}" : null,
                sphinxes > 0 ? $"{sphinxes} {(sphinxes == 1 ? "sphinx" : "sphinxes")}" : null,
            }.Where(part => part != null));
    }
}
