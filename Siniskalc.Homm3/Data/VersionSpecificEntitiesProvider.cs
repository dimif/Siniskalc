namespace Siniskalc.Homm3.Data
{
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core.Entities;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// Provides entities that exist in savefiles of a specific game version, e.g. HotA adds skills and the cannon.
    /// </summary>
    public sealed class VersionSpecificEntitiesProvider(GameVersion gameVersion, IGrantedSpellsResolver grantedSpellsResolver)
    {
        public GameVersion GameVersion { get; } = gameVersion;

        public IGrantedSpellsResolver GrantedSpellsResolver { get; } = grantedSpellsResolver;

        public IReadOnlyList<SecondarySkill> SecondarySkills { get; } = SecondarySkill.All.AvailableIn(gameVersion).ToList();

        public IReadOnlyList<WarMachine> WarMachines { get; } = WarMachine.All.AvailableIn(gameVersion).ToList();

        public IReadOnlyList<Spell> Spells { get; } = Spell.All.AvailableIn(gameVersion).ToList();
    }
}
