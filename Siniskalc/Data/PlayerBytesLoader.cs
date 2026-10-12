namespace Siniskalc.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core.Data;
    using Siniskalc.Entities;
    using Siniskalc.Models;

    /// <summary>
    /// Finds the table of players in a savefile and loads each as a <see cref="PlayerChunk"/> named after its colour.
    /// </summary>
    /// <remarks>
    /// The table is not at a fixed position (it follows the map, whose size varies), so it is found by what it looks like:
    /// a run of structs <see cref="PlayerLayout.Stride"/> bytes apart, each with non-negative resources, the
    /// <see cref="PlayerLayout.Marker"/> and a colour that is higher than the previous one's.
    /// </remarks>
    public sealed class PlayerBytesLoader : IPlayerBytesLoader
    {
        private const int SmallestRun = 2;

        /// <param name="bytes">The savefile bytes.</param>
        /// <param name="gameVersion">The version of the savefile.</param>
        /// <param name="occupied">Chunks of other things (heroes, castles) found in the savefile. The last player of the table is
        /// followed by other data, so its chunk is cut short where one of them begins.</param>
        public IReadOnlyList<PlayerChunk> LoadPlayerChunks(byte[] bytes, GameVersion gameVersion, IEnumerable<BytesChunk>? occupied = null)
        {
            // Only the layouts of the versions that have been compared are known.
            var layout = PlayerLayout.For(gameVersion);
            if (layout == null)
            {
                return Array.Empty<PlayerChunk>();
            }

            var start = -1;
            var colors = new List<int>();

            for (var position = 0; position < bytes.Length - layout.ColorOffset; position++)
            {
                // Cheap test first: most positions are not the start of a player.
                if (bytes[position + layout.MarkerOffset] != layout.Marker[0] || ColorAt(bytes, position, layout) == 0)
                {
                    continue;
                }

                var run = CountRunAt(bytes, position, layout);
                if (run.Count > colors.Count)
                {
                    colors = run;
                    start = position;
                }
            }

            if (colors.Count < SmallestRun)
            {
                return Array.Empty<PlayerChunk>();
            }

            var chunks = colors
                .Select((color, i) => CreateChunk(bytes, start + (i * layout.Stride), layout, color))
                .ToList();

            // The data after the table (a castle, say) can start inside the last player. Its chunk then ends there, so that no two chunks
            // cover the same bytes. If that leaves out the colour, what looks like a player is something else, and is left out.
            var others = occupied?.ToList() ?? [];
            while (chunks.Count > 0)
            {
                var last = chunks[^1];
                var cutAt = others
                    .Where(o => last.StartIndex < o.End && o.StartIndex < last.End)
                    .Select(o => Math.Max(o.StartIndex, last.StartIndex))
                    .DefaultIfEmpty(last.End)
                    .Min();
                if (cutAt - last.StartIndex > layout.ColorOffset)
                {
                    chunks[^1] = cutAt == last.End ? last : CreateChunk(bytes, last.StartIndex, layout, colors[chunks.Count - 1], cutAt - last.StartIndex);
                    break;
                }

                chunks.RemoveAt(chunks.Count - 1);
            }

            if (chunks.Count < SmallestRun)
            {
                return Array.Empty<PlayerChunk>();
            }

            // The savefile holds a struct for every colour, in play or not. The ones not in play have all resources at zero, no heroes and no
            // towns. A player that has only heroes or towns is in play too (Red of the Horn of the Abyss saves of TestData/Hota/SleepGarrisonVisiting,
            // Blue of the hotseat saves of TestData/Sod/Hotseat): the resources alone would hide it.
            var inPlay = chunks.Where(c => IsInPlay(c.ToArray())).ToList();
            return (inPlay.Count > 0 ? inPlay : chunks).AsReadOnly();
        }

        private static bool IsInPlay(byte[] chunk) => HasResources(chunk) || chunk[PlayerLayout.HeroCount] > 0 || chunk[PlayerLayout.TownCount] > 0;

        private static bool HasResources(byte[] chunk)
        {
            for (var resource = 0; resource < PlayerResources.All.Count; resource++)
            {
                if (BitConverter.ToInt32(chunk, PlayerLayout.ResourcesOffset + (resource * PlayerLayout.ResourceSize)) != 0)
                {
                    return true;
                }
            }

            return false;
        }

        private static PlayerChunk CreateChunk(byte[] bytes, int start, PlayerLayout layout, int color, int length = 0)
        {
            var chunk = bytes.AsSpan(start, length == 0 ? layout.Stride : length).ToArray();
            return new PlayerChunk(PlayerLayout.ColorFromByte((byte)color), chunk, start, layout);
        }

        /// <summary>
        /// Returns the colour plus one of each player of the run starting at <paramref name="position"/>.
        /// </summary>
        /// <remarks>
        /// A player slot that is not in play (typically the last colour) has the usual marker but no valid colour byte.
        /// It still follows the others, so it is taken to be the colour after the previous one.
        /// </remarks>
        private static List<int> CountRunAt(byte[] bytes, int position, PlayerLayout layout)
        {
            var colors = new List<int>();
            var previous = 0;

            while (position + (colors.Count * layout.Stride) + layout.Stride <= bytes.Length)
            {
                var start = position + (colors.Count * layout.Stride);
                var color = ColorAt(bytes, start, layout);
                if (color == 0 && HasPlayerShape(bytes, start, layout))
                {
                    color = previous + 1;
                }

                if (color <= previous || color > Constants.Player.ColorCount)
                {
                    break;
                }

                previous = color;
                colors.Add(color);
            }

            return colors;
        }

        /// <summary>
        /// Returns the colour plus one of the player struct at <paramref name="start"/>, or 0 if there is none.
        /// </summary>
        private static int ColorAt(byte[] bytes, int start, PlayerLayout layout)
        {
            if (!HasPlayerShape(bytes, start, layout))
            {
                return 0;
            }

            var color = bytes[start + layout.ColorOffset];
            return color >= 1 && color <= Constants.Player.ColorCount ? color : 0;
        }

        /// <summary>
        /// Returns whether the struct at <paramref name="start"/> has the marker and non-negative resources, whatever its colour byte.
        /// </summary>
        private static bool HasPlayerShape(byte[] bytes, int start, PlayerLayout layout)
        {
            for (var i = 0; i < layout.Marker.Length; i++)
            {
                if (bytes[start + layout.MarkerOffset + i] != layout.Marker[i])
                {
                    return false;
                }
            }

            for (var resource = 0; resource < PlayerResources.All.Count; resource++)
            {
                if (BitConverter.ToInt32(bytes, start + PlayerLayout.ResourcesOffset + (resource * PlayerLayout.ResourceSize)) < 0)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
