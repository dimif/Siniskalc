namespace Siniskalc.Homm2.ViewModels
{
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// Supplies the read-only wording of a mine's saved guardian.
    /// </summary>
    public sealed class MineGuardianTextProvider
    {
        /// <summary>
        /// Describes the guardian without hiding encodings the editor does not recognize.
        /// </summary>
        /// <param name="guardian">The saved guardian, or <see langword="null"/> for none.</param>
        /// <returns>The words shown after the owner in the mines card.</returns>
        public string Describe(MineGuardian? guardian) => guardian switch
        {
            null => "Unguarded",
            { Creature: { } creature } => $"{creature.Name} × {guardian.Count:N0}",
            _ => $"Unknown guardian (spell {guardian.SpellCode}, count {guardian.Count})",
        };
    }
}
