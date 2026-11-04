namespace Siniskalc.Homm3.Data
{
    using System.Collections.Generic;
    using Siniskalc.Homm3.Entities;

    public interface ICreatureBankLoader
    {
        IReadOnlyList<CreatureBankChunk> Load(byte[] bytes, GameVersion version, IReadOnlyList<HeroChunk> heroes, int searchFrom);
    }
}
