namespace Siniskalc.Heroes.Entities
{
    using Siniskalc.Core.Entities;

    /// <summary>
    /// A creature of a game, as much of it as the creature finder needs. Its stats are a <see cref="CreatureInfo"/>, which can depend on
    /// the version of the game, so the creature does not hold them in this interface.
    /// </summary>
    public interface ICreature : IGameEntity
    {
        /// <summary>
        /// Gets the tier of the creature among those of its faction (1 is the weakest).
        /// </summary>
        int Tier { get; }

        /// <summary>
        /// Gets the town the creature is recruited in, or the neutral faction for a neutral creature; never <c>null</c>.
        /// </summary>
        IFaction Faction { get; }

        /// <summary>
        /// Gets a value indicating whether the creature is the upgraded form of another one.
        /// </summary>
        bool IsUpgrade { get; }

        /// <summary>
        /// Gets the code of the creature this one upgrades from, or <c>null</c> when it is not an upgrade.
        /// </summary>
        byte? BaseCreatureCode { get; }
    }
}
