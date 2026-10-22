namespace Siniskalc.Homm3.Data
{
    using System.Linq;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// Reads the seven slots of a merchant. A number that is no artifact the editor knows reads as the first one it knows, and the serializer
    /// keeps the number while the model still holds it.
    /// </summary>
    public sealed class MerchantStockDeserializer : IMerchantStockDeserializer
    {
        public MerchantStockModel Deserialize(MerchantStockChunk chunk) => new(
            chunk.Index,
            chunk.Name,
            Enumerable.Range(0, MerchantLayout.SlotCount).Select(slot => ArtifactOf(chunk.GetUInt32At(MerchantLayout.SlotAt(slot)))).ToList());

        internal static Artifact? ArtifactOf(uint code) =>
            code == MerchantLayout.Empty ? null : code <= byte.MaxValue && Artifact.All.TryGetValue((byte)code, out var artifact) ? artifact : Artifact.All.Values.First();
    }
}
