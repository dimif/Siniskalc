namespace Siniskalc.Homm2.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Homm2.Entities;

    /// <summary>
    /// Cuts the records out of a savegame: every table has a fixed number of records of a fixed size, so nothing has to be searched for.
    /// </summary>
    public sealed class Homm2BytesLoader : IHomm2BytesLoader
    {
        /// <inheritdoc/>
        public IReadOnlyList<Homm2HeroChunk> LoadHeroChunks(byte[] bytes, Homm2SaveLayout layout)
        {
            return Enumerable.Range(0, Constants.Table.HeroCount)
                .Select(i => new Homm2HeroChunk(
                    Homm2Bytes.ReadText(bytes, layout.HeroStart(i) + Homm2HeroLayout.Name, Homm2HeroLayout.NameLength),
                    bytes.AsSpan(layout.HeroStart(i), layout.HeroStride).ToArray(),
                    layout.HeroStart(i)))
                .ToList()
                .AsReadOnly();
        }

        /// <inheritdoc/>
        public IReadOnlyList<Homm2PlayerChunk> LoadPlayerChunks(byte[] bytes, Homm2SaveLayout layout)
        {
            return Enumerable.Range(0, Constants.Table.PlayerCount)
                .Select(i => new Homm2PlayerChunk(((Homm2Color)i).ToString(), bytes.AsSpan(layout.PlayerStart(i), Homm2PlayerLayout.RecordSize).ToArray(), layout.PlayerStart(i)))
                .ToList()
                .AsReadOnly();
        }

        /// <inheritdoc/>
        public IReadOnlyList<Homm2CastleChunk> LoadCastleChunks(byte[] bytes, Homm2SaveLayout layout)
        {
            return Enumerable.Range(0, Constants.Table.CastleSlotCount)
                .Select(i => new Homm2CastleChunk(
                    Homm2Bytes.ReadText(bytes, layout.CastleStart(i) + Homm2CastleLayout.Name, Constants.Castle.NameLength),
                    bytes.AsSpan(layout.CastleStart(i), Homm2CastleLayout.RecordSize).ToArray(),
                    layout.CastleStart(i)))
                .ToList()
                .AsReadOnly();
        }
    }
}
