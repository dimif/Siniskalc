namespace Siniskalc.Homm3.Data
{
    using System.Collections.Generic;
    using Siniskalc.Core.Data;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// Lists nothing: the table of hero owners only changes with the owner of a hero, which <see cref="HeroDiffer"/> lists and whose revert
    /// puts the table back too (<see cref="HeroOwnerEdits.WithOwnerAsIn"/>).
    /// </summary>
    public sealed class HeroOwnersDiffer : IDiffer<HeroOwnersModel>
    {
        /// <inheritdoc/>
        public IReadOnlyList<PendingChange> Diff(int index, HeroOwnersModel original, HeroOwnersModel current) => [];
    }
}
