namespace Siniskalc.Homm2.Data
{
    using System;
    using System.Linq;
    using System.Text;
    using Siniskalc.Core;
    using Siniskalc.Core.Data;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// Writes a sphinx into its block. The block has a fixed size, so a riddle longer than the room it has, or an answer longer than 12
    /// characters, or more than 8 answers, is refused. What is the same as in the block is not written.
    /// </summary>
    public sealed class SafeSphinxSerializer : ISerializer<MapObjectChunk, SphinxModel>
    {
        /// <inheritdoc/>
        public MapObjectChunk Serialize(MapObjectChunk chunk, SphinxModel model)
        {
            var bytes = chunk.ToArray();
            RangeGuard.ThrowIfOutOfRange<long>(model.Answers.Count, 1, Constants.Sphinx.MaxAnswers, nameof(model.Answers));
            RangeGuard.ThrowIfOutOfRange<long>(model.Riddle.Length, 1, bytes.Length - MapObjectLayout.SphinxRiddle - 1, nameof(model.Riddle));

            if ((bytes[MapObjectLayout.SphinxActive] != 0) != model.IsActive)
            {
                bytes[MapObjectLayout.SphinxActive] = model.IsActive ? (byte)1 : (byte)0;
            }

            for (var i = 0; i < 7; i++)
            {
                if ((int)BitConverter.ToUInt32(bytes, MapObjectLayout.SphinxReward + (i * 4)) != model.Reward[i])
                {
                    RangeGuard.ThrowIfOutOfRange<long>(model.Reward[i], 0, int.MaxValue, nameof(model.Reward));
                    ByteBufferHelper.WriteUInt32(bytes, MapObjectLayout.SphinxReward + (i * 4), (uint)model.Reward[i]);
                }
            }

            if (BitConverter.ToUInt16(bytes, MapObjectLayout.SphinxArtifact) != model.ArtifactCode)
            {
                RangeGuard.ThrowIfOutOfRange<long>(model.ArtifactCode, 0, ushort.MaxValue, nameof(model.ArtifactCode));
                ByteBufferHelper.WriteUInt16(bytes, MapObjectLayout.SphinxArtifact, (ushort)model.ArtifactCode);
            }

            var oldAnswers = Enumerable.Range(0, bytes[MapObjectLayout.SphinxAnswerCount])
                .Select(i => Bytes.ReadText(bytes, MapObjectLayout.SphinxAnswers + (i * MapObjectLayout.SphinxAnswerSize), MapObjectLayout.SphinxAnswerSize));
            if (!oldAnswers.SequenceEqual(model.Answers))
            {
                bytes[MapObjectLayout.SphinxAnswerCount] = (byte)model.Answers.Count;
                bytes.AsSpan(MapObjectLayout.SphinxAnswers, Constants.Sphinx.MaxAnswers * MapObjectLayout.SphinxAnswerSize).Clear();
                for (var i = 0; i < model.Answers.Count; i++)
                {
                    RangeGuard.ThrowIfOutOfRange<long>(model.Answers[i].Length, 1, Constants.Sphinx.MaxAnswerLength, nameof(model.Answers));
                    Encoding.Latin1.GetBytes(model.Answers[i], bytes.AsSpan(MapObjectLayout.SphinxAnswers + (i * MapObjectLayout.SphinxAnswerSize)));
                }
            }

            if (Bytes.ReadText(bytes, MapObjectLayout.SphinxRiddle, bytes.Length - MapObjectLayout.SphinxRiddle) != model.Riddle)
            {
                bytes.AsSpan(MapObjectLayout.SphinxRiddle).Clear();
                Encoding.Latin1.GetBytes(model.Riddle, bytes.AsSpan(MapObjectLayout.SphinxRiddle));
            }

            return chunk.WithBytes(bytes);
        }
    }
}
