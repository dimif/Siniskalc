namespace Siniskalc.Entities
{
    using Siniskalc.Core.Entities;

    /// <summary>
    /// A value of a Heroes of Might and Magic III enumeration, which exists from the game version that introduced it.
    /// </summary>
    public abstract class Homm3Enumeration : AbstractEnumeration
    {
        protected Homm3Enumeration(string name, byte code, GameVersion gameVersion)
            : base(name, code, (int)gameVersion)
        {
        }

        protected Homm3Enumeration(string name, byte code)
            : this(name, code, GameVersion.RestorationOfErathia)
        {
        }

        public GameVersion GameVersion => (GameVersion)this.AvailableFrom;

        /// <summary>
        /// Gets whether this value exists in savefiles written by the given game version.
        /// Expansions only add values, so anything introduced in or before <paramref name="version"/> is available.
        /// <see cref="Entities.GameVersion.Unknown"/> comes before every version, so nothing is available in it.
        /// </summary>
        public bool IsAvailableIn(GameVersion version) => this.IsAvailableIn((int)version);
    }
}
