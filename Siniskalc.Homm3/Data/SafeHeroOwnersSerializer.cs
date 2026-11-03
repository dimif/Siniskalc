namespace Siniskalc.Homm3.Data
{
    using Siniskalc.Core;
    using Siniskalc.Core.Data;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// Writes the table of hero owners. The byte of a hero is left alone while it reads as what the model has, so a byte that was never seen,
    /// and the unexplained 41 of hero 3, stay as the game wrote them.
    /// </summary>
    public sealed class SafeHeroOwnersSerializer : ISerializer<RecordChunk, HeroOwnersModel>
    {
        /// <inheritdoc/>
        public RecordChunk Serialize(RecordChunk chunk, HeroOwnersModel model)
        {
            RangeGuard.ThrowIfOutOfRange(model.Heroes.Count, chunk.Length, chunk.Length, nameof(model));
            var bytes = chunk.ToArray();
            for (var heroId = 0; heroId < bytes.Length; heroId++)
            {
                if (HeroOwnersLayout.Read(bytes[heroId]) != model.Heroes[heroId])
                {
                    bytes[heroId] = HeroOwnersLayout.ByteOf(model.Heroes[heroId]);
                }
            }

            return chunk.WithBytes(bytes);
        }
    }
}
