namespace Siniskalc.Homm2.Entities
{
    using Siniskalc.Core.Entities;

    /// <summary>
    /// A kind of entity of Heroes of Might and Magic II that the savegame keeps as one byte, with the version of the game it comes from.
    /// </summary>
    public abstract class Homm2Enumeration : AbstractEnumeration
    {
        protected Homm2Enumeration(string name, byte code, GameVersion gameVersion)
            : base(name, code, (int)gameVersion)
        {
        }

        protected Homm2Enumeration(string name, byte code)
            : this(name, code, GameVersion.BaseGame)
        {
        }

        public GameVersion GameVersion => (GameVersion)this.AvailableFrom;

        public bool IsAvailableIn(GameVersion version) => this.IsAvailableIn((int)version);
    }
}
