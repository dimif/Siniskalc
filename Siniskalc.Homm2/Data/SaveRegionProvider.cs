namespace Siniskalc.Homm2.Data
{
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core.Data;

    /// <summary>
    /// Describes where each known field lives in a Heroes of Might and Magic II savegame, for the hex view: the header, then each hero,
    /// player and castle as one region whose elements are its fields, in the order of the savefile.
    /// </summary>
    public sealed class SaveRegionProvider : ISaveRegionProvider
    {
        private readonly ISaveLayoutLocator locator;

        public SaveRegionProvider(ISaveLayoutLocator locator)
        {
            this.locator = locator;
        }

        /// <inheritdoc/>
        public IReadOnlyList<ByteRegion> GetRegions(SaveDocument document, byte[] bytes)
        {
            var regions = new List<ByteRegion>();
            var layout = this.locator.Locate(bytes);
            if (layout is null)
            {
                return regions;
            }

            AddHeader(regions, layout);

            foreach (var player in document.Players())
            {
                regions.Add(Record(player, $"{player.Name} player", RecordRegions.Player, ByteRegionKind.Resources));
            }

            foreach (var hero in document.Heroes())
            {
                regions.Add(Record(hero, hero.Name, RecordRegions.Hero, ByteRegionKind.Identity));
            }

            foreach (var castle in document.Castles().Where(c => c.Name.Length > 0))
            {
                regions.Add(Record(castle, castle.Name, RecordRegions.Castle, ByteRegionKind.Other));
            }

            return regions.OrderBy(r => r.Start).ThenBy(r => r.Length).ToList();
        }

        private static void AddHeader(List<ByteRegion> regions, SaveLayout layout)
        {
            regions.Add(new ByteRegion("Map name", layout.MapNameStart, HeaderLayout.MapNameLength, ByteRegionKind.Header, "Header", "Text, zero terminated"));
            regions.Add(new ByteRegion("Map description", layout.MapDescriptionStart, HeaderLayout.MapDescriptionMaxLength, ByteRegionKind.Header, "Header", "Text, zero terminated"));
            regions.Add(new ByteRegion(
                "Date",
                layout.DateStart,
                (2 * HeaderLayout.DateStep) + 1,
                ByteRegionKind.Game,
                "Header",
                "The day of the week, the week and the month, a byte each with a byte between",
                Enumerable.Range(0, 3).Select(i => new ByteRegion(new[] { "Day", "Week", "Month" }[i], layout.DateStart + (i * HeaderLayout.DateStep), 1, ByteRegionKind.Game, "Date")).ToList()));
        }

        private static ByteRegion Record(BytesChunk chunk, string name, IReadOnlyList<ByteRegion> fields, ByteRegionKind kind)
        {
            return new ByteRegion(name, chunk.StartIndex, chunk.Length, kind, null, null, fields.Where(f => f.End <= chunk.Length).Select(f => f.Shifted(chunk.StartIndex)).ToList());
        }
    }
}
