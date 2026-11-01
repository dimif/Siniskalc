namespace Siniskalc.Homm3.Data
{
    using System.Linq;
    using Siniskalc.Core.Data;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// Reads the seven slots of a merchant. An unknown artifact number reads as the catalog's fallback artifact, and the serializer
    /// keeps the number while the model still holds it.
    /// </summary>
    public sealed class MerchantStockDeserializer(EntityCatalog catalog) : IDeserializer<RecordChunk, MerchantStockModel>
    {
        public MerchantStockModel Deserialize(RecordChunk chunk) => new(
            chunk.Index,
            chunk.Name,
            Enumerable.Range(0, MerchantLayout.SlotCount).Select(slot => this.ArtifactOf(chunk.GetUInt32At(MerchantLayout.SlotAt(slot)))).ToList());

        // TODO:LOG Trace: an unknown merchant artifact uses the catalog fallback and retains its bytes until edited.
        private Artifact? ArtifactOf(uint code) =>
            code == MerchantLayout.Empty ? null : code <= byte.MaxValue && catalog.Artifacts.Find((byte)code) is { Wearable: true } artifact ? artifact : catalog.FallbackArtifact;
    }
}
