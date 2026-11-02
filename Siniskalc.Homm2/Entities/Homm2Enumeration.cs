namespace Siniskalc.Homm2.Entities
{
    using Siniskalc.Core.Entities;

    /// <summary>
    /// The base of the entities of Heroes of Might and Magic II: a name, the code the savegame holds, and the version of the game it first appears in.
    /// </summary>
    public abstract class Homm2Enumeration : VersionedEnumeration<GameVersion>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Homm2Enumeration"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="code">The code.</param>
        /// <param name="gameVersion">The game version.</param>
        protected Homm2Enumeration(string name, byte code, GameVersion gameVersion)
            : base(name, code, gameVersion)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Homm2Enumeration"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="code">The code.</param>
        protected Homm2Enumeration(string name, byte code)
            : this(name, code, GameVersion.BaseGame)
        {
        }
    }
}
