namespace Siniskalc.Homm2.Entities
{
    using Siniskalc.Core.Entities;

    /// <summary>
    /// The base of the entities of Heroes of Might and Magic II: a name, the code the savegame holds, and the version of the game it first appears in.
    /// </summary>
    public abstract class Homm2Enumeration : VersionedEnumeration<GameVersion>
    {
        protected Homm2Enumeration(string name, byte code, GameVersion gameVersion)
            : base(name, code, gameVersion)
        {
        }

        protected Homm2Enumeration(string name, byte code)
            : this(name, code, GameVersion.BaseGame)
        {
        }
    }
}
