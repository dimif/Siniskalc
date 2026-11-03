namespace Siniskalc.Homm3.Data
{
    using System.Linq;
    using Microsoft.Extensions.Logging;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Runtime;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// Reads the table of hero owners. A byte that was never seen reads as a hero nobody owns, and the serializer leaves it alone while the
    /// model still says so.
    /// </summary>
    public sealed class HeroOwnersDeserializer : IDeserializer<RecordChunk, HeroOwnersModel>
    {
        private static readonly ILogger<HeroOwnersDeserializer> Logger = LogFactory.For<HeroOwnersDeserializer>();

        /// <inheritdoc/>
        public HeroOwnersModel Deserialize(RecordChunk chunk) => new(Enumerable.Range(0, chunk.Length).Select(id => Read(chunk, id)).ToList());

        private static HeroOwnership Read(RecordChunk chunk, int heroId)
        {
            var value = chunk.GetByteAt(heroId);
            if (!HeroOwnersLayout.IsSeen(value))
            {
                using var subject = Logger.BeginSubject("Hero owners", chunk.Name);
                Logger.LogTrace("Owner byte {Value:X2} of hero {HeroId} was never seen, read as nobody", value, heroId);
            }

            return HeroOwnersLayout.Read(value);
        }
    }
}
