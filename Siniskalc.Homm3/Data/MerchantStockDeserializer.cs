namespace Siniskalc.Homm3.Data
{
    using System.Linq;
    using Microsoft.Extensions.Logging;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Runtime;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// Reads the seven slots of a merchant. An unknown artifact number reads as the catalog's fallback artifact, and the serializer
    /// keeps the number while the model still holds it.
    /// </summary>
    public sealed class MerchantStockDeserializer(EntityCatalog catalog) : IDeserializer<RecordChunk, MerchantStockModel>
    {
        private static readonly ILogger<MerchantStockDeserializer> Logger = LogFactory.For<MerchantStockDeserializer>();

        /// <inheritdoc/>
        public MerchantStockModel Deserialize(RecordChunk chunk)
        {
            using var subject = Logger.BeginSubject("Merchant", chunk.Name);
            return new(
                chunk.Index,
                chunk.Name,
                Enumerable.Range(0, MerchantLayout.SlotCount).Select(slot => this.ArtifactOf(chunk.GetUInt32At(MerchantLayout.SlotAt(slot)), slot)).ToList());
        }

        private Artifact? ArtifactOf(uint code, int slot)
        {
            if (code == MerchantLayout.Empty)
            {
                return null;
            }

            if (code <= byte.MaxValue && catalog.Artifacts.Find((byte)code) is { Wearable: true } artifact)
            {
                return artifact;
            }

            Logger.LogTrace("Artifact code {Code:X8} in slot {Slot} read as {StandIn} and kept until changed", code, slot, catalog.FallbackArtifact.Name);
            return catalog.FallbackArtifact;
        }
    }
}
