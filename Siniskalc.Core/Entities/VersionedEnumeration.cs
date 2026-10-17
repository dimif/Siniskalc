namespace Siniskalc.Core.Entities
{
    using System;

    /// <summary>
    /// An enumeration whose entries are available from a version of the game: the game says which versions there are (an enum of
    /// its own), and <see cref="AbstractEnumeration.AvailableFrom"/> holds the one an entry first appears in.
    /// </summary>
    /// <typeparam name="TVersion">The game's enum of its versions, in the order they came out.</typeparam>
    public abstract class VersionedEnumeration<TVersion> : AbstractEnumeration
        where TVersion : struct, Enum
    {
        protected VersionedEnumeration(string name, byte code, TVersion gameVersion)
            : base(name, code, Convert.ToInt32(gameVersion))
        {
        }

        /// <summary>
        /// Gets the first version of the game the entry is in.
        /// </summary>
        public TVersion GameVersion => (TVersion)Enum.ToObject(typeof(TVersion), this.AvailableFrom);

        /// <summary>
        /// Returns whether the entry is in <paramref name="version"/> of the game.
        /// </summary>
        public bool IsAvailableIn(TVersion version) => this.IsAvailableIn(Convert.ToInt32(version));
    }
}
