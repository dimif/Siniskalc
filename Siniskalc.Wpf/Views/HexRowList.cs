namespace Siniskalc.Wpf.Views
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;
    using Siniskalc.Core;
    using Siniskalc.Core.Data;

    /// <summary>
    /// The lines of a hex grid, made when the grid asks for them.
    /// As an entire Homm 3 savefile is ~ a million bytes, the grid only asks for the lines that are on screen for performance.
    /// </summary>
    internal sealed class HexRowList : IList
    {
        /// <summary>
        /// Bytes per row.
        /// </summary>
        public const int BytesPerRow = 16;

        private const int MaxCachedRows = 1024;

        private readonly byte[] bytes;
        private readonly byte[] original;
        private readonly IByteRegionLookup map;
        private readonly Dictionary<int, HexRow> cache = [];

        /// <summary>
        /// Initializes a new instance of the <see cref="HexRowList"/> class.
        /// </summary>
        /// <param name="bytes">The bytes of the savefile as they are now.</param>
        /// <param name="original">The bytes of the savefile as loaded, to mark the bytes that changed.</param>
        /// <param name="map">Tells which regions a byte is in, for its colour and tooltip.</param>
        public HexRowList(byte[] bytes, byte[] original, IByteRegionLookup map)
        {
            this.bytes = bytes;
            this.original = original;
            this.map = map;
        }

        /// <inheritdoc/>
        public int Count => (this.bytes.Length + BytesPerRow - 1) / BytesPerRow;

        /// <inheritdoc/>
        public bool IsReadOnly => true;

        /// <inheritdoc/>
        public bool IsFixedSize => true;

        /// <inheritdoc/>
        public bool IsSynchronized => false;

        /// <inheritdoc/>
        public object SyncRoot => this;

        /// <inheritdoc/>
        public object? this[int index]
        {
            get => this.GetRow(index);
            set => throw new NotSupportedException();
        }

        /// <summary>
        /// Returns the row at <paramref name="index"/>.
        /// </summary>
        /// <param name="index">The place of the row, from 0.</param>
        /// <returns>The row.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is not a row of the list.</exception>
        public HexRow GetRow(int index)
        {
            RangeGuard.ThrowIfNotIndex(index, this.Count, nameof(index));

            if (!this.cache.TryGetValue(index, out var row))
            {
                if (this.cache.Count >= MaxCachedRows)
                {
                    this.cache.Clear();
                }

                this.cache[index] = row = this.CreateRow(index);
            }

            return row;
        }

        /// <inheritdoc/>
        public int IndexOf(object? value) => value is HexRow row && row.Index < this.Count ? row.Index : -1;

        /// <inheritdoc/>
        public bool Contains(object? value) => this.IndexOf(value) >= 0;

        /// <inheritdoc/>
        public IEnumerator GetEnumerator()
        {
            for (var i = 0; i < this.Count; i++)
            {
                yield return this.GetRow(i);
            }
        }

        /// <inheritdoc/>
        public void CopyTo(Array array, int index)
        {
            foreach (var row in this)
            {
                array.SetValue(row, index++);
            }
        }

        /// <inheritdoc/>
        public int Add(object? value) => throw new NotSupportedException();

        /// <inheritdoc/>
        public void Clear() => throw new NotSupportedException();

        /// <inheritdoc/>
        public void Insert(int index, object? value) => throw new NotSupportedException();

        /// <inheritdoc/>
        public void Remove(object? value) => throw new NotSupportedException();

        /// <inheritdoc/>
        public void RemoveAt(int index) => throw new NotSupportedException();

        private HexRow CreateRow(int index)
        {
            var rowStart = index * BytesPerRow;
            var cells = new List<HexCell>(BytesPerRow);
            var text = new StringBuilder(BytesPerRow);

            for (var i = 0; i < BytesPerRow; i++)
            {
                var offset = rowStart + i;
                if (offset >= this.bytes.Length)
                {
                    cells.Add(new HexCell(string.Empty, null, null, false));
                    text.Append(' ');
                    continue;
                }

                var value = this.bytes[offset];
                var leaf = this.map.FindLeaf(offset);
                var isChanged = offset < this.original.Length && this.original[offset] != value;
                var brush = leaf == null ? null : HexPalette.GetBrush(leaf.Kind, this.map.GetShade(offset));

                cells.Add(new HexCell(value.ToString("X2"), brush, this.DescribeByte(offset), isChanged));
                text.Append(HexFormat.ToAsciiPreview(value));
            }

            return new HexRow(index, $"0x{rowStart:X8} ({rowStart})", cells, text.ToString());
        }

        private string? DescribeByte(int offset)
        {
            var lines = new List<string>();
            foreach (var region in this.map.FindAll(offset))
            {
                lines.Add($"{region.FullName} ({RegionNode.DescribeRange(region)})");
                if (!string.IsNullOrEmpty(region.Description))
                {
                    lines.Add(region.Description);
                }
            }

            if (offset < this.original.Length && this.original[offset] != this.bytes[offset])
            {
                lines.Add($"Changed: was {this.original[offset]:X2}, now {this.bytes[offset]:X2}");
            }

            return lines.Count == 0 ? null : string.Join(Environment.NewLine, lines);
        }
    }
}
