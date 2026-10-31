namespace Siniskalc.Homm3.Data
{
    using Siniskalc.Core.Data;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// What a hero page needs from the savefile around the hero it edits: the hero as a model, and the few facts of the savefile that the
    /// page shows beside it. <see cref="HeroEditSession"/> answers them from the document; a test answers them with a fake.
    /// </summary>
    public interface IHeroSession : IEditSession<HeroModel>
    {
        /// <summary>
        /// Gets the place of the hero among the heroes of the savefile.
        /// </summary>
        int HeroIndex { get; }

        /// <summary>
        /// Gets an object that is the same instance for as long as the record of the hero is not changed, so a page can tell whether an
        /// edit made to any hero was made to this one.
        /// </summary>
        object Revision { get; }

        /// <summary>
        /// Gets the game version that wrote the savefile.
        /// </summary>
        GameVersion GameVersion { get; }

        /// <summary>
        /// Gets whether the game version of the savefile stores that a hero was put to sleep.
        /// </summary>
        bool SupportsAsleep { get; }

        /// <summary>
        /// Gets the place of the castle that holds the hero inside it, or -1.
        /// </summary>
        int GarrisonCastleIndex { get; }

        /// <summary>
        /// Gets the name of the castle at <paramref name="castleIndex"/>.
        /// </summary>
        string CastleName(int castleIndex);

        /// <summary>
        /// Creates a target for the hex editor onto the raw bytes of the hero. The caller disposes it.
        /// </summary>
        IByteEditTarget CreateByteEditTarget(IByteEditTargetFactory factory);
    }
}
