namespace Siniskalc.Homm3.Data
{
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Data;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// What a castle page needs from the savefile around the castle it edits: the castle as a model, and the few facts of the savefile
    /// that the page shows beside it. <see cref="CastleEditSession"/> answers them from the document; a test answers them with a fake.
    /// </summary>
    public interface ICastleSession : IEditSession<CastleModel>
    {
        /// <summary>
        /// Gets the name of the castle as the savefile holds it.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the game version that wrote the savefile.
        /// </summary>
        GameVersion GameVersion { get; }

        /// <summary>
        /// Gets the heroes of the savefile, to name the ones in the castle.
        /// </summary>
        IHeroSummaries<PlayerColor> Heroes { get; }

        /// <summary>
        /// Returns the hero with the given id, or null when no hero has it.
        /// </summary>
        HeroModel? FindHero(int heroId);

        /// <summary>
        /// Gives the castle to <paramref name="owner"/> and moves the towns of the players with it. The rules of the game may refuse.
        /// </summary>
        void ChangeOwner(PlayerColor owner);

        /// <summary>
        /// Creates a target for the hex editor onto the raw bytes of the castle. The caller disposes it.
        /// </summary>
        IByteEditTarget CreateByteEditTarget(IByteEditTargetFactory factory);
    }
}
