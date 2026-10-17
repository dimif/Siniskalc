namespace Siniskalc.Homm2.Data
{
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// Writes the owner of a mine into its record, and the flag that shows it on the map: the colour of the flag is its first picture plus the colour of the owner, and a mine of nobody has no flag.
    /// </summary>
    public sealed class SafeMineSerializer : IMapObjectSerializer<MineModel>
    {
        /// <inheritdoc/>
        public MapObjectChunk Serialize(MapObjectChunk chunk, MineModel model)
        {
            // An owner that is not changed leaves the flag as it is: a mine whose flag the game never drew stays so.
            if (Bytes.ReadColor(chunk.GetByteAt(0)) == model.Owner)
            {
                return chunk;
            }

            var bytes = chunk.ToArray();
            bytes[0] = Bytes.ColorByte(model.Owner, bytes[0]);
            var written = chunk.WithBytes(bytes);

            for (var i = 0; i < chunk.Parts.Count; i++)
            {
                // A flag that already shows the new owner is left as it is.
                var wanted = model.Owner is { } owner
                    ? new[] { MapObjectLayout.FlagClass, (byte)(MineKinds.FlagPicture(model.Kind) + (int)owner) }
                    : [MapObjectLayout.NoClass, MapObjectLayout.NoPicture];
                var current = chunk.Parts[i].Bytes;
                var showsOwner = model.Owner is { } shown
                    ? current[0] == MapObjectLayout.FlagClass && current[1] == MineKinds.FlagPicture(model.Kind) + (int)shown
                    : current[0] == MapObjectLayout.NoClass && current[1] == MapObjectLayout.NoPicture;
                if (!showsOwner)
                {
                    written = written.WithPart(i, wanted);
                }
            }

            return written;
        }
    }
}
