namespace Siniskalc.Homm3.Entities
{
    using Siniskalc.Core.Entities;

    /// <summary>
    /// The base of the entities of Heroes of Might and Magic III: a name, the code the savegame holds, and the version of the game it first appears in.
    /// </summary>
    public abstract class Homm3Enumeration : VersionedEnumeration<GameVersion>
    {
        protected Homm3Enumeration(string name, byte code, GameVersion gameVersion)
            : base(name, code, gameVersion)
        {
        }

        protected Homm3Enumeration(string name, byte code)
            : this(name, code, GameVersion.RestorationOfErathia)
        {
        }
    }
}
