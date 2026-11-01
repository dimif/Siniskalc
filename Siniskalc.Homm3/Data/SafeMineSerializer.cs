namespace Siniskalc.Homm3.Data
{
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Data;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// Writes the owner and the guard of a mine into its record. The kind and the tile are not edited. An owner byte that is no colour is left
    /// alone while the model says Neutral, as it reads.
    /// </summary>
    public sealed class SafeMineSerializer(CreatureCodes creatureCodes) : ISerializer<RecordChunk, MineModel>
    {
        /// <inheritdoc/>
        public RecordChunk Serialize(RecordChunk chunk, MineModel model)
        {
            var bytes = chunk.ToArray();
            if (PlayerColor.FromByte(bytes[MineLayout.Owner]) != model.Owner)
            {
                bytes[MineLayout.Owner] = (byte)model.Owner.Code;
            }

            ArmyWriter.Write(bytes, MineLayout.Guard, model.Guard, creatureCodes, nameof(model.Guard));
            return chunk.WithBytes(bytes);
        }
    }
}
