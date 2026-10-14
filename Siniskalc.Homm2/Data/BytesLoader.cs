namespace Siniskalc.Homm2.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Homm2.Entities;

    /// <summary>
    /// Cuts the records out of a savegame: every table has a fixed number of records of a fixed size, so nothing has to be searched for.
    /// </summary>
    public sealed class BytesLoader : IBytesLoader
    {
        /// <inheritdoc/>
        public IReadOnlyList<HeroChunk> LoadHeroChunks(byte[] bytes, SaveLayout layout)
        {
            return Enumerable.Range(0, Constants.Table.HeroCount)
                .Select(i => new HeroChunk(
                    Bytes.ReadText(bytes, layout.HeroStart(i) + HeroLayout.Name, HeroLayout.NameLength),
                    bytes.AsSpan(layout.HeroStart(i), layout.HeroStride).ToArray(),
                    layout.HeroStart(i)))
                .ToList()
                .AsReadOnly();
        }

        /// <inheritdoc/>
        public IReadOnlyList<PlayerChunk> LoadPlayerChunks(byte[] bytes, SaveLayout layout)
        {
            return Enumerable.Range(0, Constants.Table.PlayerCount)
                .Select(i => new PlayerChunk(((Color)i).ToString(), bytes.AsSpan(layout.PlayerStart(i), PlayerLayout.RecordSize).ToArray(), layout.PlayerStart(i)))
                .ToList()
                .AsReadOnly();
        }

        /// <inheritdoc/>
        public IReadOnlyList<CastleChunk> LoadCastleChunks(byte[] bytes, SaveLayout layout)
        {
            return Enumerable.Range(0, Constants.Table.CastleSlotCount)
                .Select(i => new CastleChunk(
                    Bytes.ReadText(bytes, layout.CastleStart(i) + CastleLayout.Name, Constants.Castle.NameLength),
                    bytes.AsSpan(layout.CastleStart(i), CastleLayout.RecordSize).ToArray(),
                    layout.CastleStart(i)))
                .ToList()
                .AsReadOnly();
        }
    }
}
